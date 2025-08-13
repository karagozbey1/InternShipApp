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
        public string RoomIdentifier { get; set; } = null!;
        public ICollection<UserChatRoom>? Participants { get; set; }
        public ICollection<ChatMessage>? Messages { get; set; }
    }
}
