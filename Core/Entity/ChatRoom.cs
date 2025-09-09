using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string RoomType { get; set; } = "Private"; // Private, Group, Support
        public string? RoomIdentifier { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActivityAt { get; set; }
        
        // Group chat settings
        public int? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }
        public bool AllowFileSharing { get; set; } = true;
        public int MaxParticipants { get; set; } = 50;
        
        // Navigation Properties
        public ICollection<UserChatRoom>? Participants { get; set; }
        public ICollection<ChatMessage>? Messages { get; set; }
        
        // Helper method to get unread message count for a user
        public int GetUnreadCount(int userId)
        {
            return Messages?.Count(m => !m.IsRead && m.SenderId != userId) ?? 0;
        }
        
        // Helper method to get last message
        public ChatMessage? GetLastMessage()
        {
            return Messages?.Where(m => !m.IsDeleted)
                           .OrderByDescending(m => m.SentAt)
                           .FirstOrDefault();
        }
    }
}
