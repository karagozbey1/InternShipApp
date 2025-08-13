using Core.Entity;

namespace Business.Interfaces.Chat
{
    public interface IIChatService
    {
        Task<ChatRoom?> GetChatRoomByIdAsync(int chatRoomId);
        Task<List<ChatRoom>> GetUserChatRoomsAsync(int userId);
        Task<ChatRoom> CreateOrGetChatRoomAsync(int user1Id, int user2Id);
        Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderId, string content);
        Task<List<ChatMessage>> GetChatHistoryAsync(int chatRoomId);
    }
}