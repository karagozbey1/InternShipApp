using System;

namespace Core.Entity
{
    // Staj defteri
    public class InternshipDiary
    {
        // Kimlik numarasý
        public int Id { get; set; }
        
        // Öðrenci kimliði
        public int StudentId { get; set; }
        
        // Öðrenci
        public User? Student { get; set; }

        // Baþvuru kimliði
        public int? ApplicationId { get; set; }
        
        // Baþvuru
        public InternshipApplication? Application { get; set; }

        // Defter baþlýðý
        public string Title { get; set; } = null!;
        
        // Açýklama
        public string? Description { get; set; }
        
        // Oluþturulma tarihi
        public DateTime CreatedAt { get; set; }
        
        // Güncellenme tarihi
        public DateTime UpdatedAt { get; set; }
        
        // Þirket adý
        public string? CompanyName { get; set; }
        
        // Bölüm
        public string? Department { get; set; }
        
        // Staj baþlangýç tarihi
        public DateTime? InternshipStartDate { get; set; }
        
        // Staj bitiþ tarihi
        public DateTime? InternshipEndDate { get; set; }
        
        // Toplam gün sayýsý
        public int? TotalDays { get; set; }
        
        // Zorunlu mu
        public bool IsRequired { get; set; } = true;
        
        // Tamamlandý mý
        public bool IsCompleted { get; set; } = false;
        
        // Günlük giriþler
        public ICollection<DiaryEntry>? Entries { get; set; }
    }
}