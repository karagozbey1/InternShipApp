using Core.Entity;

namespace Business.Interfaces.Chat
{
    public interface IIChatService
    {
        // Existing methods
        Task<ChatRoom?> GetChatRoomByIdAsync(int chatRoomId);
        Task<List<ChatRoom>> GetUserChatRoomsAsync(int userId);
        Task<ChatRoom> CreateOrGetChatRoomAsync(int user1Id, int user2Id);
        Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderId, string content);
        Task<List<ChatMessage>> GetChatHistoryAsync(int chatRoomId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<List<ChatMessage>> GetRecentMessagesAsync(int userId);

        // New methods for enhanced chat functionality
        Task<bool> HasUserAccessToChatRoomAsync(int userId, int chatRoomId);
        Task<ChatMessage> SendMessageAsync(ChatMessage message);
        Task<ChatMessage?> GetMessageByIdAsync(int messageId);
        Task<ChatMessage> UpdateMessageAsync(ChatMessage message);
        Task DeleteMessageAsync(int messageId);
        Task MarkMessagesAsReadAsync(int chatRoomId, int userId);
        Task<ChatRoom> CreateChatRoomAsync(ChatRoom chatRoom);
        Task AddUserToChatRoomAsync(int chatRoomId, int userId);
        Task RemoveUserFromChatRoomAsync(int chatRoomId, int userId);
        Task<List<ChatMessage>> SearchMessagesAsync(string query, int? chatRoomId = null, int? userId = null);
        Task<MessageAttachment> SaveAttachmentAsync(MessageAttachment attachment);
        Task<List<MessageAttachment>> GetMessageAttachmentsAsync(int messageId);
        Task DeleteAttachmentAsync(int attachmentId);
        Task<bool> IsUserOnlineAsync(int userId);
        Task SetUserOnlineStatusAsync(int userId, bool isOnline);
        Task<DateTime?> GetUserLastSeenAsync(int userId);
        Task UpdateUserLastSeenAsync(int userId);
        Task<List<int>> GetTypingUsersAsync(int chatRoomId);
        Task SetUserTypingAsync(int chatRoomId, int userId, bool isTyping);
    }
}