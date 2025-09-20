using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    // Bildirim entity'si - kullanıcılara gönderilen sistem mesajları
    public class Notification
    {
        // Primary key - bildirim benzersiz tanımlayıcısı
        public int Id { get; set; }
        
        // Foreign key - bildirimi alan kullanıcının ID'si
        public int UserId { get; set; }
        
        // Navigation property - bildirimi alan kullanıcı
        public User User { get; set; } = null!;
        
        // Bildirim başlığı - kısa özet
        public string Title { get; set; } = null!;
        
        // Bildirim içeriği - detaylı mesaj
        public string Message { get; set; } = null!;
        
        // Bildirim türü - Info, Warning, Error, Success, Message, Application, Document
        public string Type { get; set; } = "Info";
        
        // Bildirime tıklandığında gidilecek URL - opsiyonel
        public string? ActionUrl { get; set; }
        
        // Bildirim ikonu CSS sınıfı - FontAwesome class
        public string? IconClass { get; set; }
        
        // Bildirim oluşturulma zamanı - otomatik atanır
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Bildirim okunma zamanı - null ise okunmamış
        public DateTime? ReadAt { get; set; }
        
        // Bildirim okundu mu - boolean flag
        public bool IsRead { get; set; } = false;
        
        // Bildirim silinmiş mi - soft delete
        public bool IsDeleted { get; set; } = false;
        
        // Bildirim son geçerlilik tarihi - opsiyonel
        public DateTime? ExpiresAt { get; set; }
        
        // İlişkili entity'nin ID'si - opsiyonel
        public int? RelatedEntityId { get; set; }
        
        // İlişkili entity'nin türü - Application, Document, ChatMessage vb.
        public string? RelatedEntityType { get; set; }
        
        // Grup anahtarı - ilişkili bildirimleri gruplamak için
        public string? GroupKey { get; set; }
        
        // Bu bildirim gruplanmış mı - batch notifications için
        public bool IsGrouped { get; set; } = false;
        
        // Bildirim önceliği - 1=Low, 2=Normal, 3=High, 4=Critical
        public int Priority { get; set; } = 1;
    }
}
