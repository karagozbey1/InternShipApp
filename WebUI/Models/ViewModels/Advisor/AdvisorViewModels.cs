using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.ViewModels.Advisor
{
    public class AdvisorProfileViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string? Department { get; set; }
        public string? Title { get; set; }
        public string? UniversityName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdvisorActivityItem
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Date { get; set; }
        public string Type { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Color { get; set; } = "";

        // Additional properties that controller expects
        public DateTime CreatedAt { get; set; }
        public string StudentName { get; set; } = "";
        public int RelatedId { get; set; }
    }

    // Missing statistic classes
    public class ApplicationStatistic
    {
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int PendingApplications { get; set; }
        public int RejectedApplications { get; set; }
        public decimal SuccessRate { get; set; }
        public DateTime? LastApplicationDate { get; set; }
    }

    public class DiaryStatistic
    {
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public int TotalEntries { get; set; }
        public int ApprovedEntries { get; set; }
        public int PendingEntries { get; set; }
        public double TotalHours { get; set; }
        public decimal CompletionRate { get; set; }
        public double AverageHoursPerEntry { get; set; }
        public DateTime? LastEntryDate { get; set; }
        public decimal ApprovalRate => TotalEntries > 0 ? (decimal)ApprovedEntries / TotalEntries * 100 : 0;
    }

    public class ActivityData
    {
        public DateTime Date { get; set; }
        public int Applications { get; set; }
        public int DiaryEntries { get; set; }
        public string Description { get; set; } = "";
    }

    public class ReportSummary
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int TotalApplications { get; set; }
        public int TotalDiaryEntries { get; set; }
        public decimal TotalWorkingHours { get; set; }
        public decimal AverageCompletionRate { get; set; }
        public string Period { get; set; } = "";
    }

    public class StudentReportData
    {
        public string StudentName { get; set; } = "";
        public string StudentNumber { get; set; } = "";
        public string Department { get; set; } = "";
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int TotalDiaryEntries { get; set; }
        public decimal TotalWorkingHours { get; set; }
        public DateTime? LastActivity { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = "";
        public decimal CompletionRate { get; set; }
        public string StudentEmail { get; set; } = "";
    }

    public class AdvisorReportViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalApplications { get; set; }
        public int PendingApprovals { get; set; }
        public int CompletedInternships { get; set; }
        public List<AdvisorActivityItem> RecentActivities { get; set; } = new();
        
        // Additional properties needed by Report/Index.cshtml
        public string CurrentReportType { get; set; } = "";
        public List<StudentReportItem> StudentReports { get; set; } = new();
        public List<ActivityDataItem> ActivityData { get; set; } = new();
        public string CurrentPeriod { get; set; } = "";
        
        // Chart and statistics data
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, decimal> MonthlyProgress { get; set; } = new();
        public List<DepartmentStatistic> DepartmentStats { get; set; } = new();

        // Missing properties from views
        public AdvisorInfo? AdvisorInfo { get; set; }
        public ReportSummary? Summary { get; set; }
        public List<ApplicationStatistic> ApplicationStatistics { get; set; } = new();
        public List<DiaryStatistic> DiaryStatistics { get; set; } = new();
    }
    
    public class StudentReportItem
    {
        public string StudentName { get; set; } = "";
        public string StudentNumber { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "";
        public int DiaryEntriesCount { get; set; }
        public decimal CompletionRate { get; set; }
        
        // Additional properties needed by Report/Index.cshtml
        public string ActivityStatus { get; set; } = "";
        public int TotalApplications { get; set; }
        public int TotalDiaryEntries { get; set; }
        public decimal TotalWorkingHours { get; set; }
        public DateTime? LastActivity { get; set; }
        public string StudentEmail { get; set; } = "";
        public string Department { get; set; } = "";
        public bool IsActive { get; set; }
    }
    
    public class ActivityDataItem
    {
        public DateTime Date { get; set; }
        public string ActivityType { get; set; } = "";
        public int Count { get; set; }
        public string Description { get; set; } = "";
        
        // Additional properties needed by Report/Index.cshtml
        public int Applications { get; set; }
        public int DiaryEntries { get; set; }
    }
    
    public class DepartmentStatistic
    {
        public string DepartmentName { get; set; } = "";
        public int StudentCount { get; set; }
        public int CompletedInternships { get; set; }
        public decimal SuccessRate { get; set; }
    }

    // Application related ViewModels
    public class AdvisorApplicationListViewModel
    {
        public List<AdvisorApplicationCardViewModel> Applications { get; set; } = new();
        public string? SearchTerm { get; set; }
        public Core.Entity.InternshipApprovalStatus? StatusFilter { get; set; }
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public class AdvisorApplicationCardViewModel
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public string StudentNumber { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string Position { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeeklyWorkHours { get; set; }
        public Core.Entity.InternshipApprovalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? StudentNote { get; set; }
        public string? AdvisorNote { get; set; }
        public string? RejectionReason { get; set; }
        public string? WorkplaceAddress { get; set; }
        public string? InternshipCoordinator { get; set; }
        public string? CoordinatorPhone { get; set; }
        public string? CoordinatorEmail { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        // Yeni eklenen öðrenci detaylarý
        public string? StudentDepartment { get; set; }
        public string? StudentUniversity { get; set; }
        public int? StudentGrade { get; set; }

        // Missing properties from views
        public string StatusText => Status switch
        {
            Core.Entity.InternshipApprovalStatus.Pending => "Beklemede",
            Core.Entity.InternshipApprovalStatus.Approved => "Onaylandý",
            Core.Entity.InternshipApprovalStatus.Rejected => "Reddedildi",
            _ => "Bilinmiyor"
        };

        public string StatusColor => Status switch
        {
            Core.Entity.InternshipApprovalStatus.Pending => "yellow",
            Core.Entity.InternshipApprovalStatus.Approved => "green",
            Core.Entity.InternshipApprovalStatus.Rejected => "red",
            _ => "gray"
        };

        public string StatusIcon => Status switch
        {
            Core.Entity.InternshipApprovalStatus.Pending => "fas fa-clock",
            Core.Entity.InternshipApprovalStatus.Approved => "fas fa-check-circle",
            Core.Entity.InternshipApprovalStatus.Rejected => "fas fa-times-circle",
            _ => "fas fa-question-circle"
        };

        public int DurationDays => (EndDate - StartDate).Days;
        public int DaysFromSubmission => (DateTime.Now - CreatedAt).Days;
    }

    // Diary related ViewModels
    public class AdvisorDiaryViewModel
    {
        public List<StudentDiaryGroup> StudentDiaries { get; set; } = new();
        public DiaryFilters Filters { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public AdvisorInfo? AdvisorInfo { get; set; }
        
        // Pagination properties
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class StudentDiaryGroup
    {
        public Core.Entity.User Student { get; set; } = null!;
        public List<DiaryEntryWithStatus> DiaryEntries { get; set; } = new();
        public int TotalEntries { get; set; }
        public int PendingEntries { get; set; }
        public int ApprovedEntries { get; set; }
        public DateTime? LastEntryDate { get; set; }
        public string InternshipCompany { get; set; } = "";
        public DateTime? InternshipStartDate { get; set; }
        public DateTime? InternshipEndDate { get; set; }
        // Yeni alanlar: her staj defterini ayýrmak için
        public int? DiaryId { get; set; }
        public string? DiaryTitle { get; set; }
        public bool IsRequired { get; set; }
    }

    public class DiaryEntryWithStatus
    {
        public Core.Entity.DiaryEntry Entry { get; set; } = null!;
        public string StatusText { get; set; } = "";
        public string StatusColor { get; set; } = "";
    }

    public class DiaryFilters
    {
        public string? StudentName { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }

    public class AdvisorInfo
    {
        public string UniversityName { get; set; } = "";
        public string Department { get; set; } = "";
        public int TotalStudentsInDepartment { get; set; }
        public int StudentsWithDiaries { get; set; }
    }

    public class AdvisorDiaryDetailViewModel
    {
        public Core.Entity.User Student { get; set; } = null!;
        public List<DiaryEntryWithStatus> DiaryEntries { get; set; } = new();
        public Core.Entity.Company? InternshipCompany { get; set; }
        public Core.Entity.InternshipApplication? Application { get; set; }
        public DiaryStatistics Statistics { get; set; } = new();

        // Missing properties from views
        public string StudentName => Student?.FullName ?? "";
        public string CompanyName => InternshipCompany?.CompanyName ?? "";
        public int TotalEntries => DiaryEntries?.Count ?? 0;
        public int PendingEntries => DiaryEntries?.Count(d => !d.Entry.IsApproved) ?? 0;
        public int ApprovedEntries => DiaryEntries?.Count(d => d.Entry.IsApproved) ?? 0;
        public DateTime? InternshipStartDate => Application?.StartDate;
        public DateTime? InternshipEndDate => Application?.EndDate;
    }

    public class DiaryStatistics
    {
        public int TotalEntries { get; set; }
        public int ApprovedEntries { get; set; }
        public int PendingEntries { get; set; }
        public decimal CompletionRate { get; set; }
        public DateTime? FirstEntryDate { get; set; }
        public DateTime? LastEntryDate { get; set; }
    }

    public class DiaryEntryDetailViewModel
    {
        public Core.Entity.DiaryEntry Entry { get; set; } = null!;
        public Core.Entity.User Student { get; set; } = null!;
        public bool CanTakeAction { get; set; }
        public DateTime? PreviousEntryDate { get; set; }
        public DateTime? NextEntryDate { get; set; }
    }

    public class DiaryActionViewModel
    {
        public int DiaryEntryId { get; set; }
        public string Feedback { get; set; } = "";
    }

    // Notification related ViewModels
    public class AdvisorNotificationViewModel
    {
        public List<Core.Entity.Notification> Notifications { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public string CurrentFilter { get; set; } = "";
        public NotificationStatistics Statistics { get; set; } = new();
        
        // Pagination properties
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class NotificationStatistics
    {
        public int UnreadCount { get; set; }
        public int TodayCount { get; set; }
        public int ThisWeekCount { get; set; }
        public int TotalCount { get; set; }
    }

    public class AdvisorDashboardViewModel
    {
        public string AdvisorName { get; set; } = "";
        public string UniversityName { get; set; } = "";
        public string Department { get; set; } = "";
        public int TotalStudents { get; set; }
        public int PendingApprovals { get; set; }
        public int TotalDiaryEntries { get; set; }
        public int PendingDiaryEntries { get; set; }
        public int CompletedInternships { get; set; }
        public List<AdvisorActivityItem> RecentActivities { get; set; } = new();
        public List<Core.Entity.InternshipApprovalForm> PendingApprovalForms { get; set; } = new();
        public List<Core.Entity.DiaryEntry> RecentDiaryEntries { get; set; } = new();
        public AdvisorStatistics Statistics { get; set; } = new();

        // Properties that AdvisorDashboardController expects
        public int PendingApplicationsCount { get; set; }
        public int MyStudentsCount { get; set; }
        public int PendingDiaryEntriesCount { get; set; }
        public int CompletedInternshipsCount { get; set; }
        public int UnreadNotificationsCount { get; set; }
        public Core.Entity.User? AdvisorInfo { get; set; }
        public List<Core.Entity.News> RecentNews { get; set; } = new();
        public List<Core.Entity.Notification> RecentNotifications { get; set; } = new();
        public List<Core.Entity.InternshipApplication> RecentApplications { get; set; } = new();
        public WebUI.Models.ViewModels.Education.AcademicYearInfo? AcademicYear { get; set; }
    }

    public class AdvisorStatistics
    {
        public int ThisMonthApplications { get; set; }
        public int ThisMonthApprovals { get; set; }
        public int ThisWeekDiaryEntries { get; set; }
        public decimal ApprovalRate { get; set; }
        public int ActiveStudents { get; set; }
    }

    public class AdvisorStudentFilters
    {
        public string? Search { get; set; }
        public string? Department { get; set; }
        public int? Grade { get; set; }
        public string? Status { get; set; } // active / passive / null
        public string? Sort { get; set; } // name, lastActivity, applications, diaries
        public string? Order { get; set; } = "desc"; // asc/desc
    }

    public class AdvisorStudentItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Department { get; set; }
        public int? Grade { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int PendingApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int TotalDiaryEntries { get; set; }
        public int PendingDiaryEntries { get; set; }
        public int ApprovedDiaryEntries { get; set; }
        public DateTime? LastActivity { get; set; }
        public string InternshipStatus { get; set; } = "Yok";
        // Yeni alanlar
        public string? StudentNumber { get; set; }
        public string? UniversityName { get; set; }
    }

    public class AdvisorStudentListViewModel
    {
        public AdvisorStudentFilters Filters { get; set; } = new();
        public List<AdvisorStudentItemViewModel> Students { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalItems { get; set; }
        public int TotalPages => PageSize == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        // Quick stats
        public int ActiveCount { get; set; }
        public int PassiveCount { get; set; }
        public int WithApprovedInternship { get; set; }
        public int WithPendingApplications { get; set; }
    }
}