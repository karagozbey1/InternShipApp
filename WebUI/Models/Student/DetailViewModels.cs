using System.ComponentModel.DataAnnotations;
using WebUI.Models.ViewModels.Student;

namespace WebUI.Models.Student
{
    public class DiaryNotebookDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? CompanyName { get; set; }
        public string? Department { get; set; }
        public DateTime? InternshipStartDate { get; set; }
        public DateTime? InternshipEndDate { get; set; }
        public int? TotalDays { get; set; }
        public bool IsRequired { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Entries
        public List<DiaryEntryViewModel> Entries { get; set; } = new();
        public DiaryEntryCreateViewModel NewEntry { get; set; } = new();
        
        // Stats
        public int TotalEntries { get; set; }
        public int CompletedDays { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime? LastEntryDate { get; set; }
    }

    public class DiaryEntryCreateViewModel
    {
        [Required(ErrorMessage = "Tarih zorunludur")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Aktiviteler zorunludur")]
        [StringLength(1000, ErrorMessage = "Aktiviteler en fazla 1000 karakter olabilir")]
        public string Activities { get; set; } = "";

        [StringLength(2000, ErrorMessage = "Açýklama en fazla 2000 karakter olabilir")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Çalýþma süresi zorunludur")]
        [Range(1, 24, ErrorMessage = "Çalýþma süresi 1-24 saat arasýnda olmalýdýr")]
        public int Duration { get; set; } = 8;

        [StringLength(1000, ErrorMessage = "Öðrenilenler en fazla 1000 karakter olabilir")]
        public string? Learnings { get; set; }

        [StringLength(1000, ErrorMessage = "Zorluklar en fazla 1000 karakter olabilir")]
        public string? Challenges { get; set; }
    }
}