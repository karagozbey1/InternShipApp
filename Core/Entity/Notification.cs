using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = "Info"; // Info, Warning, Error, Success, Message, Application, Document
        public string? ActionUrl { get; set; }
        public string? IconClass { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public bool IsRead { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime? ExpiresAt { get; set; }
        
        // Related entities
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; } // Application, Document, ChatMessage, etc.
        
        // Grouping notifications
        public string? GroupKey { get; set; }
        public bool IsGrouped { get; set; } = false;
        
        // Priority
        public int Priority { get; set; } = 1; // 1=Low, 2=Normal, 3=High, 4=Critical
    }
}
