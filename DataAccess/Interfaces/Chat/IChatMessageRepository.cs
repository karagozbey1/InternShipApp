using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Chat
{
    public interface IChatMessageRepository : IRepository<ChatMessage>
    {
        Task<List<ChatMessage>> GetMessagesByChatRoomIdAsync(int chatRoomId);
    }
}