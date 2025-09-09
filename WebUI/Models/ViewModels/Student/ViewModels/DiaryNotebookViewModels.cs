using System.ComponentModel.DataAnnotations;
using Core.Entity;
using WebUI.Models.ViewModels.Student;

namespace WebUI.Models.ViewModels.Student.ViewModels
{
    public class HelpViewModel
    {
        public List<FaqItem> FaqItems { get; set; } = new();
        public string CoordinatorName { get; set; } = "Dr. Ahmet Yýlmaz";
        public string CoordinatorEmail { get; set; } = "staj@universite.edu.tr";
        public string OfficeHours { get; set; } = "Pazartesi-Cuma 09:00-17:00";
        public string SupportEmail { get; set; } = "destek@universite.edu.tr";
        public string SupportPhone { get; set; } = "+90 212 555 01 23";
    }

    public class FaqItem
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }

    public class DiaryNotebookListViewModel
    {
        public List<DiaryNotebookViewModel> Diaries { get; set; } = new();
        public bool HasAnyDiaries => Diaries.Any();
    }

    public class DiaryNotebookViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public string? CompanyName { get; set; }
        public string? Department { get; set; }
        public DateTime? InternshipStartDate { get; set; }
        public DateTime? InternshipEndDate { get; set; }
        public int? TotalDays { get; set; }
        
        public bool IsRequired { get; set; }
        public bool IsCompleted { get; set; }
        
        // Ýstatistikler
        public int TotalEntries { get; set; }
        public int CompletedDays { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime? LastEntryDate { get; set; }
        
        // Helper properties
        public string StatusText => IsCompleted ? "Tamamlandý" : "Devam Ediyor";
        public string StatusColor => IsCompleted ? "green" : "blue";
        public string RequiredText => IsRequired ? "Zorunlu" : "Ýsteðe Baðlý";
        public string DurationText => InternshipStartDate.HasValue && InternshipEndDate.HasValue ? 
            $"{InternshipStartDate:dd.MM.yyyy} - {InternshipEndDate:dd.MM.yyyy}" : "Tarih Belirtilmemiþ";
    }

    public class CreateDiaryNotebookViewModel
    {
        [Required(ErrorMessage = "Defter baþlýðý zorunludur.")]
        [StringLength(200, ErrorMessage = "Baþlýk en fazla 200 karakter olabilir.")]
        [Display(Name = "Defter Baþlýðý")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açýklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açýklama")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "Þirket adý en fazla 200 karakter olabilir.")]
        [Display(Name = "Þirket Adý")]
        public string? CompanyName { get; set; }

        [StringLength(200, ErrorMessage = "Departman adý en fazla 200 karakter olabilir.")]
        [Display(Name = "Departman")]
        public string? Department { get; set; }

        [Display(Name = "Staj Baþlangýç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? InternshipStartDate { get; set; }

        [Display(Name = "Staj Bitiþ Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? InternshipEndDate { get; set; }

        [Display(Name = "Toplam Staj Günü")]
        [Range(1, 365, ErrorMessage = "Staj günü 1-365 arasýnda olmalýdýr.")]
        public int? TotalDays { get; set; }

        [Display(Name = "Bu defter zorunlu mu?")]
        public bool IsRequired { get; set; } = true;

        // Optional: link to the InternshipApplication that this diary is created for
        public int? ApplicationId { get; set; }
    }

    public class DiaryNotebookDetailViewModel
    {
        public DiaryNotebookViewModel Diary { get; set; } = new();
        public List<DiaryEntryViewModel> Entries { get; set; } = new();
        public WebUI.Models.ViewModels.Student.DiaryCreateViewModel CreateEntryModel { get; set; } = new();
        
        // Ýstatistikler
        public WebUI.Models.ViewModels.Student.DiaryStatsViewModel Stats { get; set; } = new();
        
        // Navigation
        public bool CanAddEntry => !Diary.IsCompleted;
        public bool HasEntries => Entries.Any();
    }
}