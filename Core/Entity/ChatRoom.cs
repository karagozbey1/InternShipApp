using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    // Sohbet odası
    public class ChatRoom
    {
        // Kimlik numarası
        public int Id { get; set; }
        
        // Oda adı
        public string Name { get; set; } = null!;
        
        // Açıklama
        public string? Description { get; set; }
        
        // Oda türü
        public string RoomType { get; set; } = "Private"; // Private, Group, Support
        
        // Oda tanımlayıcısı
        public string? RoomIdentifier { get; set; }
        
        // Aktif mi
        public bool IsActive { get; set; } = true;
        
        // Oluşturulma zamanı
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Son aktivite zamanı
        public DateTime? LastActivityAt { get; set; }
        
        // Oluşturan kullanıcı kimliği
        public int? CreatedByUserId { get; set; }
        
        // Oluşturan kullanıcı
        public User? CreatedByUser { get; set; }
        
        // Dosya paylaşımına izin ver
        public bool AllowFileSharing { get; set; } = true;
        
        // Maksimum katılımcı sayısı
        public int MaxParticipants { get; set; } = 50;
        
        // Katılımcılar
        public ICollection<UserChatRoom>? Participants { get; set; }
        
        // Mesajlar
        public ICollection<ChatMessage>? Messages { get; set; }
        
        // Okunmamış mesaj sayısını getirir
        public int GetUnreadCount(int userId)
        {
            return Messages?.Count(m => !m.IsRead && m.SenderId != userId) ?? 0;
        }
        
        // Son mesajı getirir
        public ChatMessage? GetLastMessage()
        {
            return Messages?.Where(m => !m.IsDeleted)
                           .OrderByDescending(m => m.SentAt)
                           .FirstOrDefault();
        }
    }
}
