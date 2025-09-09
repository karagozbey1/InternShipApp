using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Chat;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Chat
{
    public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<ChatMessage>> GetMessagesByChatRoomIdAsync(int chatRoomId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Attachments)
                .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountByUserIdAsync(int userId)
        {
            // Get all chat rooms where user is participant
            var userChatRoomIds = await _context.UserChatRooms
                .Where(ucr => ucr.UserId == userId)
                .Select(ucr => ucr.ChatRoomId)
                .ToListAsync();

            // Count unread messages from other users in those chat rooms
            return await _context.ChatMessages
                .Where(m => userChatRoomIds.Contains(m.ChatRoomId) 
                           && m.SenderId != userId 
                           && !m.IsRead 
                           && !m.IsDeleted)
                .CountAsync();
        }

        public async Task<List<ChatMessage>> GetUnreadMessagesByChatRoomIdAsync(int chatRoomId, int userId)
        {
            return await _context.ChatMessages
                .Where(m => m.ChatRoomId == chatRoomId 
                           && m.SenderId != userId 
                           && !m.IsRead 
                           && !m.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> SearchMessagesAsync(string query, int? chatRoomId = null, int? userId = null)
        {
            var queryable = _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => !m.IsDeleted && m.Content.Contains(query));

            if (chatRoomId.HasValue)
            {
                queryable = queryable.Where(m => m.ChatRoomId == chatRoomId.Value);
            }

            if (userId.HasValue)
            {
                // Get user's chat rooms
                var userChatRoomIds = await _context.UserChatRooms
                    .Where(ucr => ucr.UserId == userId.Value)
                    .Select(ucr => ucr.ChatRoomId)
                    .ToListAsync();

                queryable = queryable.Where(m => userChatRoomIds.Contains(m.ChatRoomId));
            }

            return await queryable
                .OrderByDescending(m => m.SentAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task UpdateAsync(ChatMessage message)
        {
            _context.ChatMessages.Update(message);
        }

        public async Task<ChatMessage?> GetMessageWithDetailsAsync(int messageId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Attachments)
                .Include(m => m.ReplyToMessage)
                .FirstOrDefaultAsync(m => m.Id == messageId);
        }
    }
}