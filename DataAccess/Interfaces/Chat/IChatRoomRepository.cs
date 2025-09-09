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
        Task<ChatRoom?> GetChatRoomWithMessagesAndParticipantsAsync(int chatRoomId);
        Task UpdateAsync(ChatRoom chatRoom);
        Task AddUserToChatRoomAsync(UserChatRoom userChatRoom);
        Task RemoveUserFromChatRoomAsync(int chatRoomId, int userId);
        Task<bool> IsUserInChatRoomAsync(int chatRoomId, int userId);
        Task<List<ChatRoom>> GetPublicChatRoomsAsync();
    }
}