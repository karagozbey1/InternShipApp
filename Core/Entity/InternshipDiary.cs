using System;

namespace Core.Entity
{
    public class InternshipDiary
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public User? Student { get; set; }

        // Optional link to the original InternshipApplication
        public int? ApplicationId { get; set; }
        public InternshipApplication? Application { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        
        // PostgreSQL için UTC DateTime - default deðerleri kaldýrýldý
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Staj bilgileri
        public string? CompanyName { get; set; }
        public string? Department { get; set; }
        public DateTime? InternshipStartDate { get; set; }
        public DateTime? InternshipEndDate { get; set; }
        public int? TotalDays { get; set; }
        
        // Defter ayarlarý
        public bool IsRequired { get; set; } = true;
        public bool IsCompleted { get; set; } = false;
        
        // Ýliþkiler
        public ICollection<DiaryEntry>? Entries { get; set; }
    }
}