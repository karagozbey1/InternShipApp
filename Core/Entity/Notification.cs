using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    // Bildirim
    public class Notification
    {
        // Kimlik numarası
        public int Id { get; set; }
        
        // Kullanıcı kimliği
        public int UserId { get; set; }
        
        // Kullanıcı
        public User User { get; set; } = null!;
        
        // Başlık
        public string Title { get; set; } = null!;
        
        // Mesaj
        public string Message { get; set; } = null!;
        
        // Bildirim türü
        public string Type { get; set; } = "Info";
        
        // Tıklama bağlantısı
        public string? ActionUrl { get; set; }
        
        // İkon sınıfı
        public string? IconClass { get; set; }
        
        // Oluşturulma tarihi
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Okunma tarihi
        public DateTime? ReadAt { get; set; }
        
        // Okundu mu
        public bool IsRead { get; set; } = false;
        
        // Silindi mi
        public bool IsDeleted { get; set; } = false;
        
        // Son geçerlilik tarihi
        public DateTime? ExpiresAt { get; set; }
        
        // İlişkili öğe kimliği
        public int? RelatedEntityId { get; set; }
        
        // İlişkili öğe türü
        public string? RelatedEntityType { get; set; }
        
        // Grup anahtarı
        public string? GroupKey { get; set; }
        
        // Gruplu mu
        public bool IsGrouped { get; set; } = false;
        
        // Öncelik seviyesi
        public int Priority { get; set; } = 1;
    }
}
