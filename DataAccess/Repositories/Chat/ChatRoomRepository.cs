using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Chat;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Chat
{
    public class ChatRoomRepository : Repository<ChatRoom>, IChatRoomRepository
    {
        public ChatRoomRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<ChatRoom>> GetChatRoomsByUserIdAsync(int userId)
        {
            return await _context.UserChatRooms
                .Include(ucr => ucr.ChatRoom)
                    .ThenInclude(cr => cr.Messages!)
                        .ThenInclude(m => m.Sender)
                .Include(ucr => ucr.ChatRoom)
                    .ThenInclude(cr => cr.Participants!)
                        .ThenInclude(p => p.User)
                .Where(ucr => ucr.UserId == userId)
                .Select(ucr => ucr.ChatRoom)
                .Where(cr => cr.IsActive)
                .ToListAsync();
        }

        public async Task<ChatRoom?> FindChatRoomBetweenUsersAsync(int user1Id, int user2Id)
        {
            return await _context.ChatRooms
                .Include(cr => cr.Participants)
                .Where(cr => cr.RoomType == "Private" &&
                             cr.Participants!.Any(p => p.UserId == user1Id) &&
                             cr.Participants!.Any(p => p.UserId == user2Id) &&
                             cr.Participants!.Count == 2) 
                .FirstOrDefaultAsync();
        }

        public async Task<ChatRoom?> GetChatRoomWithMessagesAndParticipantsAsync(int chatRoomId)
        {
            return await _context.ChatRooms
                .Include(cr => cr.Messages!)
                    .ThenInclude(m => m.Sender)
                .Include(cr => cr.Messages!)
                    .ThenInclude(m => m.Attachments)
                .Include(cr => cr.Participants!)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);
        }

        public async Task UpdateAsync(ChatRoom chatRoom)
        {
            _context.ChatRooms.Update(chatRoom);
        }

        public async Task AddUserToChatRoomAsync(UserChatRoom userChatRoom)
        {
            await _context.UserChatRooms.AddAsync(userChatRoom);
        }

        public async Task RemoveUserFromChatRoomAsync(int chatRoomId, int userId)
        {
            var userChatRoom = await _context.UserChatRooms
                .FirstOrDefaultAsync(ucr => ucr.ChatRoomId == chatRoomId && ucr.UserId == userId);
            
            if (userChatRoom != null)
            {
                _context.UserChatRooms.Remove(userChatRoom);
            }
        }

        public async Task<bool> IsUserInChatRoomAsync(int chatRoomId, int userId)
        {
            return await _context.UserChatRooms
                .AnyAsync(ucr => ucr.ChatRoomId == chatRoomId && ucr.UserId == userId);
        }

        public async Task<List<ChatRoom>> GetPublicChatRoomsAsync()
        {
            return await _context.ChatRooms
                .Include(cr => cr.Participants)
                .Where(cr => cr.RoomType == "Group" && cr.IsActive)
                .OrderByDescending(cr => cr.LastActivityAt ?? cr.CreatedAt)
                .ToListAsync();
        }
    }
}