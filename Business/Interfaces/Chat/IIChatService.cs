using Core.Entity;

namespace Business.Interfaces.Chat
{
    // Sohbet servisi arabirimi
    public interface IIChatService
    {
        // Sohbet odasını kimlik ile getirir
        Task<ChatRoom?> GetChatRoomByIdAsync(int chatRoomId);
        
        // Kullanıcının sohbet odalarını getirir
        Task<List<ChatRoom>> GetUserChatRoomsAsync(int userId);
        
        // İki kullanıcı için sohbet odası oluşturur veya getirir
        Task<ChatRoom> CreateOrGetChatRoomAsync(int user1Id, int user2Id);
        
        // Mesaj gönderir
        Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderId, string content);
        
        // Sohbet geçmişini getirir
        Task<List<ChatMessage>> GetChatHistoryAsync(int chatRoomId);
        
        // Okunmamış mesaj sayısını getirir
        Task<int> GetUnreadCountAsync(int userId);
        
        // Son mesajları getirir
        Task<List<ChatMessage>> GetRecentMessagesAsync(int userId);

        // Kullanıcının odaya erişimi var mı kontrol eder
        Task<bool> HasUserAccessToChatRoomAsync(int userId, int chatRoomId);
        
        // Mesaj gönderir
        Task<ChatMessage> SendMessageAsync(ChatMessage message);
        
        // Mesajı kimlik ile getirir
        Task<ChatMessage?> GetMessageByIdAsync(int messageId);
        
        // Mesajı günceller
        Task<ChatMessage> UpdateMessageAsync(ChatMessage message);
        
        // Mesaj siler
        Task DeleteMessageAsync(int messageId);
        
        // Mesajları okundu olarak işaretler
        Task MarkMessagesAsReadAsync(int chatRoomId, int userId);
        
        // Sohbet odası oluşturur
        Task<ChatRoom> CreateChatRoomAsync(ChatRoom chatRoom);
        
        // Kullanıcıyı odaya ekler
        Task AddUserToChatRoomAsync(int chatRoomId, int userId);
        
        // Kullanıcıyı odadan çıkarır
        Task RemoveUserFromChatRoomAsync(int chatRoomId, int userId);
        
        // Mesajlarda arama yapar
        Task<List<ChatMessage>> SearchMessagesAsync(string query, int? chatRoomId = null, int? userId = null);
        
        // Eki kaydeder
        Task<MessageAttachment> SaveAttachmentAsync(MessageAttachment attachment);
        
        // Mesaj eklerini getirir
        Task<List<MessageAttachment>> GetMessageAttachmentsAsync(int messageId);
        
        // Eki siler
        Task DeleteAttachmentAsync(int attachmentId);
        
        // Kullanıcı çevrimiçi mı kontrol eder
        Task<bool> IsUserOnlineAsync(int userId);
        
        // Kullanıcı çevrimiçi durumunu ayarlar
        Task SetUserOnlineStatusAsync(int userId, bool isOnline);
        
        // Kullanıcının son görülme zamanını getirir
        Task<DateTime?> GetUserLastSeenAsync(int userId);
        
        // Kullanıcının son görülme zamanını günceller
        Task UpdateUserLastSeenAsync(int userId);
        
        // Yazıyor olan kullanıcıları getirir
        Task<List<int>> GetTypingUsersAsync(int chatRoomId);
        
        // Kullanıcının yazıyor durumunu ayarlar
        Task SetUserTypingAsync(int chatRoomId, int userId, bool isTyping);
    }
}