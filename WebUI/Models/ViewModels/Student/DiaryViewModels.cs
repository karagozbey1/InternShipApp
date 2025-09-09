using System.ComponentModel.DataAnnotations;
using Core.Entity;

namespace WebUI.Models.ViewModels.Student
{
    public class DiaryEntryViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int DiaryId { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal WorkingHours { get; set; }
        public DiaryCategory Category { get; set; }
        public string Notes { get; set; } = "";
        public bool IsApproved { get; set; }
        public string? AdvisorFeedback { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Missing properties used by controller
        public string LearnedTopics { get; set; } = "";
        public string Challenges { get; set; } = "";
        public string Solutions { get; set; } = "";

        public string CategoryDisplayName => Category switch
        {
            DiaryCategory.Daily => "Günlük",
            DiaryCategory.Weekly => "Haftalýk",
            DiaryCategory.Monthly => "Aylýk",
            DiaryCategory.Project => "Proje",
            DiaryCategory.Meeting => "Toplantý",
            DiaryCategory.Training => "Eðitim",
            DiaryCategory.Other => "Diðer",
            _ => "Bilinmiyor"
        };

        public string CategoryIcon => Category switch
        {
            DiaryCategory.Daily => "fas fa-calendar-day",
            DiaryCategory.Weekly => "fas fa-calendar-week",
            DiaryCategory.Monthly => "fas fa-calendar",
            DiaryCategory.Project => "fas fa-project-diagram",
            DiaryCategory.Meeting => "fas fa-users",
            DiaryCategory.Training => "fas fa-graduation-cap",
            DiaryCategory.Other => "fas fa-question",
            _ => "fas fa-question"
        };

        public string CategoryColor => Category switch
        {
            DiaryCategory.Daily => "blue",
            DiaryCategory.Weekly => "green",
            DiaryCategory.Monthly => "purple",
            DiaryCategory.Project => "orange",
            DiaryCategory.Meeting => "red",
            DiaryCategory.Training => "teal",
            DiaryCategory.Other => "gray",
            _ => "gray"
        };
    }

    public class DiaryCreateViewModel
    {
        [Required(ErrorMessage = "Baþlýk zorunludur")]
        [StringLength(200, ErrorMessage = "Baþlýk 200 karakterden uzun olamaz")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Ýçerik zorunludur")]
        [StringLength(2000, ErrorMessage = "Ýçerik 2000 karakterden uzun olamaz")]
        public string Content { get; set; } = "";

        [Required(ErrorMessage = "Tarih zorunludur")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Çalýþma saati zorunludur")]
        [Range(0.5, 24, ErrorMessage = "Çalýþma saati 0.5-24 saat arasýnda olmalýdýr")]
        public decimal WorkingHours { get; set; }

        [Required(ErrorMessage = "Kategori seçimi zorunludur")]
        public DiaryCategory Category { get; set; }

        [StringLength(500, ErrorMessage = "Notlar 500 karakterden uzun olamaz")]
        public string? Notes { get; set; }

        [StringLength(1000, ErrorMessage = "Öðrenilenler 1000 karakterden uzun olamaz")]
        public string? LearnedTopics { get; set; }

        [StringLength(1000, ErrorMessage = "Zorluklar 1000 karakterden uzun olamaz")]
        public string? Challenges { get; set; }

        [StringLength(1000, ErrorMessage = "Çözümler 1000 karakterden uzun olamaz")]
        public string? Solutions { get; set; }
    }

    public class DiaryEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Baþlýk zorunludur")]
        [StringLength(200, ErrorMessage = "Baþlýk 200 karakterden uzun olamaz")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Ýçerik zorunludur")]
        [StringLength(2000, ErrorMessage = "Ýçerik 2000 karakterden uzun olamaz")]
        public string Content { get; set; } = "";

        [Required(ErrorMessage = "Tarih zorunludur")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Çalýþma saati zorunludur")]
        [Range(0.5, 24, ErrorMessage = "Çalýþma saati 0.5-24 saat arasýnda olmalýdýr")]
        public decimal WorkingHours { get; set; }

        [Required(ErrorMessage = "Kategori seçimi zorunludur")]
        public DiaryCategory Category { get; set; }

        [StringLength(500, ErrorMessage = "Notlar 500 karakterden uzun olamaz")]
        public string? Notes { get; set; }

        public bool CanEdit { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class DiaryListViewModel
    {
        public List<DiaryEntryViewModel> DiaryEntries { get; set; } = new();
        public DiaryCreateViewModel CreateModel { get; set; } = new();
        public DiaryStatsViewModel Stats { get; set; } = new();
        public List<DiaryEntryViewModel> WeeklyEntries { get; set; } = new();
        public int CurrentWeek { get; set; }
        public int TotalWeeks { get; set; }
        public DateTime InternshipStartDate { get; set; }
        public DateTime InternshipEndDate { get; set; }
        public string CompanyName { get; set; } = "";
        public string Department { get; set; } = "";
        // Whether the student can create diary entries (requires an approved internship application)
        public bool CanCreateDiary { get; set; } = false;
        // ID of the InternshipDiary associated with the approved application (if any)
        public int? ActiveDiaryId { get; set; }
        // Approved applications that do NOT yet have a diary (available to create)
        public List<WebUI.Models.ViewModels.Student.ApplicationItemVM> EligibleApplicationsForDiary { get; set; } = new();
    }

    public class DiaryStatsViewModel
    {
        public int TotalEntries { get; set; }
        public int ApprovedEntries { get; set; }
        public int PendingEntries { get; set; }
        public decimal TotalHours { get; set; }
        public decimal WeeklyAverageHours { get; set; }
        public Dictionary<DiaryCategory, int> CategoryDistribution { get; set; } = new();
        public DateTime? LastEntryDate { get; set; }
        public decimal CompletionRate { get; set; }

        // Missing properties from views
        public decimal TotalWorkingHours { get; set; }
        public int TotalDays { get; set; }
        public DateTime? FirstEntryDate { get; set; }
        public decimal AverageDailyHours { get; set; }
        public int CompletionPercentage { get; set; }
    }

    public class DiaryWeeklyReportViewModel
    {
        public int WeekNumber { get; set; }
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public List<DiaryEntryViewModel> Entries { get; set; } = new();
        public decimal TotalHours { get; set; }
        public int EntryCount { get; set; }
        public string Summary { get; set; } = "";

        // Missing properties from views
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string WeeklySummary { get; set; } = "";
        public string Achievements { get; set; } = "";
        public string NextWeekGoals { get; set; } = "";
    }

    public class DiaryFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DiaryCategory? Category { get; set; }
        public bool? IsApproved { get; set; }
        public string? SearchTerm { get; set; }
    }
}