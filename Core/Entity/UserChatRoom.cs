using Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    // Kullanıcı sohbet odası
    public class UserChatRoom
    {
        // Kullanıcı kimliği
        public int UserId { get; set; }
        
        // Kullanıcı
        public User User { get; set; } = null!;
        
        // Sohbet odası kimliği
        public int ChatRoomId { get; set; }
        
        // Sohbet odası
        public ChatRoom ChatRoom { get; set; } = null!;
        
        // Katılma zamanı
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        // Yönetici mi
        public bool IsAdmin { get; set; } = false;
        
        // Sessize alınmış mı
        public bool IsMuted { get; set; } = false;
        
        // Ayrılma zamanı
        public DateTime? LeftAt { get; set; }
    }
}
