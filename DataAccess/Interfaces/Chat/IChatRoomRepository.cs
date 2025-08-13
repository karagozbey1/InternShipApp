using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Chat
{
    public interface IChatRoomRepository : IRepository<ChatRoom>
    {
       
        Task<List<ChatRoom>> GetChatRoomsByUserIdAsync(int userId);
      
        Task<ChatRoom?> FindChatRoomBetweenUsersAsync(int user1Id, int user2Id);
        // Sohbet odasını tüm mesajları ve katılımcılarıyla birlikte getiren metot
        Task<ChatRoom?> GetChatRoomWithMessagesAndParticipantsAsync(int chatRoomId);
    }
}