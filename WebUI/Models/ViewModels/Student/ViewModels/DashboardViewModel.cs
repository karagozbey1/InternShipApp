using Core.Entity;
using WebUI.Models.ViewModels.Student;

namespace WebUI.Models.ViewModels.Student.ViewModels;

public class DashboardViewModel
{
    // User Information
    public Core.Entity.User? User { get; set; }
    public string FullName => User?.FullName ?? "Kullanýcý";
    
    public int TotalInternshipDays { get; set; }
    public int CompletedInternshipDays { get; set; }
    public int RemainingInternshipDays => TotalInternshipDays - CompletedInternshipDays;
    public string CompletionRate => TotalInternshipDays > 0 ? $"{(CompletedInternshipDays * 100 / TotalInternshipDays):F0}%" : "0%";
    
    // Process Information
    public string CurrentProcessStage { get; set; } = "Baþvuru Süresinde";
    public int ProcessProgress { get; set; } = 0;
    public string ProcessDescription { get; set; } = "";
    
    // Recent Activities (Dynamic)
    public List<ActivityItem> RecentActivities { get; set; } = new();
    
    // Recent Applications (Last 3)
    public List<Core.Entity.InternshipApplication> RecentApplications { get; set; } = new();
    
    // Legacy properties for backward compatibility
    public int CompletedInternships { get; set; }
    public int ActiveApplications { get; set; }
    public int PendingDocuments { get; set; }
    public int UnreadNotifications { get; set; }
    public int ProgressPercentage { get; set; }
    public List<Core.Entity.Notification> RecentNotifications { get; set; } = new();
    public List<string> TodoList { get; set; } = new();
    public List<InternshipFieldStat> InternshipsByField { get; set; } = new();
    public Core.Entity.InternshipApplication? ActiveInternship { get; set; }
    
    // Legacy for StudentDashboard  
    public WebUI.Models.Student.StudentModel Student { get; set; } = null!;
    public List<ApplicationDetailsViewModel> Applications { get; set; } = new();
    public List<NotificationViewModel> Notifications { get; set; } = new();
    public List<MessageViewModel> RecentMessages { get; set; } = new();
    public DashboardStatsViewModel Stats { get; set; } = new();
    public List<DiaryEntryViewModel> RecentDiaryEntries { get; set; } = new();
}

public class InternshipFieldStat
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class ActivityItem
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Color { get; set; } = "";
    public DateTime Time { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ActionUrl { get; set; }
    public string Type { get; set; } = "";
    public string TimeText { get; set; } = "";
}
