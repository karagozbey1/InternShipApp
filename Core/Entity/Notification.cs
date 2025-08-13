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
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public bool IsRead { get; set; } = false;
        }
    }
