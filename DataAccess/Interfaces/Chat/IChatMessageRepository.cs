using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Chat
{
    public interface IChatMessageRepository : IRepository<ChatMessage>
    {
        Task<List<ChatMessage>> GetMessagesByChatRoomIdAsync(int chatRoomId);
        Task<int> GetUnreadCountByUserIdAsync(int userId);
        Task<List<ChatMessage>> GetUnreadMessagesByChatRoomIdAsync(int chatRoomId, int userId);
        Task<List<ChatMessage>> SearchMessagesAsync(string query, int? chatRoomId = null, int? userId = null);
        Task UpdateAsync(ChatMessage message);
        Task<ChatMessage?> GetMessageWithDetailsAsync(int messageId);
    }
}