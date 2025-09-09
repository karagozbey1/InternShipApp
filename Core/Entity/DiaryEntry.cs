using System;

namespace Core.Entity
{
    public class DiaryEntry
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public User? Student { get; set; }

        // Hangi deftere ait olduğunu belirten alan
        public int DiaryId { get; set; }
        public InternshipDiary? Diary { get; set; }

        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        
        // PostgreSQL için UTC DateTime - default değerleri kaldırıldı
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }

        // Extended fields for richer diary functionality
        public decimal WorkingHours { get; set; } = 0;
        public int Category { get; set; } = 1;
        public string? Notes { get; set; }
        public string? LearnedTopics { get; set; }
        public string? Challenges { get; set; }
        public string? Solutions { get; set; }
        public bool IsApproved { get; set; }
        public string? AdvisorFeedback { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
