using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    public class UserChatRoom
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsAdmin { get; set; } = false;
        public bool IsMuted { get; set; } = false;
        public DateTime? LeftAt { get; set; }
    }
}
