using System;

namespace Core.Entity
{
    // Staj defteri entity'si - öðrenci staj günlükleri
    public class InternshipDiary
    {
        // Primary key - staj defteri benzersiz tanýmlayýcýsý
        public int Id { get; set; }
        
        // Foreign key - defteri tutan öðrencinin ID'si
        public int StudentId { get; set; }
        
        // Navigation property - defteri tutan öðrenci
        public User? Student { get; set; }

        // Foreign key - iliþkili baþvurunun ID'si (opsiyonel)
        public int? ApplicationId { get; set; }
        
        // Navigation property - iliþkili baþvuru
        public InternshipApplication? Application { get; set; }

        // Defter baþlýðý - zorunlu alan
        public string Title { get; set; } = null!;
        
        // Defter açýklamasý - opsiyonel
        public string? Description { get; set; }
        
        // Defter oluþturulma zamaný - UTC format
        public DateTime CreatedAt { get; set; }
        
        // Defter güncellenme zamaný - UTC format
        public DateTime UpdatedAt { get; set; }
        
        // Þirket adý - opsiyonel
        public string? CompanyName { get; set; }
        
        // Departman adý - opsiyonel
        public string? Department { get; set; }
        
        // Staj baþlangýç tarihi - opsiyonel
        public DateTime? InternshipStartDate { get; set; }
        
        // Staj bitiþ tarihi - opsiyonel
        public DateTime? InternshipEndDate { get; set; }
        
        // Toplam staj günü - opsiyonel
        public int? TotalDays { get; set; }
        
        // Bu defter zorunlu mu - varsayýlan true
        public bool IsRequired { get; set; } = true;
        
        // Defter tamamlandý mý - varsayýlan false
        public bool IsCompleted { get; set; } = false;
        
        // Navigation property - defterdeki günlük giriþler
        public ICollection<DiaryEntry>? Entries { get; set; }
    }
}