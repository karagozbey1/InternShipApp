using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity;

// Sohbet mesajı
public class ChatMessage
{
    // Kimlik numarası
    public int Id { get; set; }
    
    // Mesaj içeriği
    public string Content { get; set; } = null!;
    
    // Gönderim zamanı
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    // Okunma zamanı
    public DateTime? ReadAt { get; set; }
    
    // Okundu mu
    public bool IsRead { get; set; } = false;
    
    // Mesaj türü
    public string? MessageType { get; set; } = "Text";
    
    // Silinmiş mi
    public bool IsDeleted { get; set; } = false;
    
    // Silinme zamanı
    public DateTime? DeletedAt { get; set; }
    
    // Düzenlenme zamanı
    public DateTime? EditedAt { get; set; }
    
    // Sohbet odası kimliği
    public int ChatRoomId { get; set; }
    
    // Sohbet odası
    public ChatRoom ChatRoom { get; set; } = null!;
    
    // Gönderen kimliği
    public int SenderId { get; set; }
    
    // Gönderen kullanıcı
    public User Sender { get; set; } = null!;
    
    // Yanıtlanan mesaj kimliği
    public int? ReplyToMessageId { get; set; }
    
    // Yanıtlanan mesaj
    public ChatMessage? ReplyToMessage { get; set; }
    
    // Yanıtlar
    public ICollection<ChatMessage>? Replies { get; set; }
    
    // Ekler
    public ICollection<MessageAttachment>? Attachments { get; set; }
    
    // Ek var mı
    public bool HasAttachments => Attachments?.Any() == true;
    
    // Düzenlenmiş mi
    public bool IsEdited => EditedAt.HasValue;
}




