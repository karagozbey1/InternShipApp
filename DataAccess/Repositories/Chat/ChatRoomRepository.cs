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
                .Where(ucr => ucr.UserId == userId)
                .Select(ucr => ucr.ChatRoom)
                .ToListAsync();
        }

        public async Task<ChatRoom?> FindChatRoomBetweenUsersAsync(int user1Id, int user2Id)
        {
            return await _context.ChatRooms
                .Where(cr => cr.Participants!.Any(p => p.UserId == user1Id) &&
                             cr.Participants!.Any(p => p.UserId == user2Id) &&
                             cr.Participants!.Count == 2) 
                .FirstOrDefaultAsync();
        }

        public async Task<ChatRoom?> GetChatRoomWithMessagesAndParticipantsAsync(int chatRoomId)
        {
            return await _context.ChatRooms
                .Include(cr => cr.Messages!)
                .ThenInclude(m => m.Sender)
                .Include(cr => cr.Participants!)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);
        }
    }
}