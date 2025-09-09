using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity;

public class ChatMessage
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public bool IsRead { get; set; } = false;
    public string? MessageType { get; set; } = "Text"; // Text, File, Image, Document
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    
    // Navigation Properties
    public int ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; } = null!;
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;
    
    // Reply functionality
    public int? ReplyToMessageId { get; set; }
    public ChatMessage? ReplyToMessage { get; set; }
    public ICollection<ChatMessage>? Replies { get; set; }
    
    // Attachments
    public ICollection<MessageAttachment>? Attachments { get; set; }
    
    // Helper properties
    public bool HasAttachments => Attachments?.Any() == true;
    public bool IsEdited => EditedAt.HasValue;
}




