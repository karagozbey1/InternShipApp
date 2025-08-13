using Core.Entity;
using System;


namespace Core.Entity
{
    public class InternshipComment
    {
        public int Id { get; set; }
    
        public int SenderUserId { get; set; }
        public User Sender { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public int ApplicationId { get; set; }
        public InternshipApplication InternshipApplication { get; set; } = null!;

    }
}
