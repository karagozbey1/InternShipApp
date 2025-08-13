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

    public int ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; } = null!;

    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;
}



