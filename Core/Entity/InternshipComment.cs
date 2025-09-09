using System;
using System.Collections.Generic;

namespace Core.Entity
{
    public class InternshipComment
    {
        public enum Status
        {
            Unread = 0,
            Read = 1
        }

        public int Id { get; set; }
        public int SenderUserId { get; set; }
        public User? Sender { get; set; }
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public int ApplicationId { get; set; }
        public InternshipApplication? InternshipApplication { get; set; }
        public Status CommentStatus { get; set; } = Status.Unread;
    }
}