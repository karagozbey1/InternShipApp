using System.ComponentModel.DataAnnotations;
using Core.Entity;

namespace WebUI.Models.ViewModels.Student
{
    public class StudentDocumentVM
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string? Title { get; set; }
        public string DocumentType { get; set; } = "";
        public DateTime UploadedAt { get; set; }
        public long FileSize { get; set; }
        public string FilePath { get; set; } = "";
        public bool IsRequired { get; set; }
        public string? Description { get; set; }

        // Missing property from views
        public string DocumentTypeDisplayName => DocumentType switch
        {
            "CV" => "CV/Özgeçmiþ",
            "Transcript" => "Transkript",
            "ApplicationForm" => "Baþvuru Formu", 
            "Other" => "Diðer",
            _ => "Bilinmiyor"
        };
    }

    public class StudentDiaryGroup
    {
        public User Student { get; set; } = null!;
        public List<DiaryEntryViewModel> DiaryEntries { get; set; } = new();
        public int TotalEntries { get; set; }
        public int PendingEntries { get; set; }
        public int ApprovedEntries { get; set; }
        public DateTime? LastEntryDate { get; set; }
        public string InternshipCompany { get; set; } = "";
        public DateTime? InternshipStartDate { get; set; }
        public DateTime? InternshipEndDate { get; set; }
    }

    public class ProfileUpdateViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [StringLength(100, ErrorMessage = "Ad Soyad 100 karakterden uzun olamaz")]
        public string FullName { get; set; } = "";
        
        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = "";
        
        [Phone(ErrorMessage = "Geçerli bir telefon numarasý giriniz")]
        public string? Phone { get; set; }
        
        [StringLength(100, ErrorMessage = "Bölüm adý 100 karakterden uzun olamaz")]
        public string? Department { get; set; }
        
        [StringLength(20, ErrorMessage = "Öðrenci numarasý 20 karakterden uzun olamaz")]
        public string? StudentNumber { get; set; }
        
        [Range(1, 8, ErrorMessage = "Sýnýf 1-8 arasýnda olmalýdýr")]
        public int? Grade { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut þifre zorunludur")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "Yeni þifre zorunludur")]
        [StringLength(100, ErrorMessage = "Þifre en az {2} karakter olmalýdýr.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Þifre ve doðrulama þifresi uyuþmuyor.")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class DashboardStatsViewModel
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int CompletedInternships { get; set; }
        public int TotalDiaryEntries { get; set; }
        public int UnreadNotifications { get; set; }
    }

    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Type { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ActionUrl { get; set; }
        public string? IconClass { get; set; }
    }

    public class MessageViewModel
    {
        public int Id { get; set; }
        public string SenderName { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public string MessageType { get; set; } = "";
    }

    public class OfferBrowseViewModel
    {
        public List<InternshipOffer> Offers { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? CityFilter { get; set; }
        public string? FieldFilter { get; set; }
        public string? CompanyFilter { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public List<string> AvailableCities { get; set; } = new();
        public List<string> AvailableFields { get; set; } = new();
        public List<string> AvailableCompanies { get; set; } = new();

        // Missing property from views
        public string? LocationFilter { get; set; }
    }
}