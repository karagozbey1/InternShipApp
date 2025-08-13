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
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
    }
}