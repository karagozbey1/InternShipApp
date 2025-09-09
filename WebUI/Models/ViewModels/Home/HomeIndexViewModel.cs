using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        public List<Core.Entity.News> News { get; set; } = new();
        public List<Core.Entity.InternshipOffer> RecentOffers { get; set; } = new();
        public int TotalStudents { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalInternships { get; set; }
        public int ActiveOffers { get; set; }
        public List<Core.Entity.SliderItem> SliderItems { get; set; } = new();
        public bool IsAuthenticated { get; set; }
        public string? UserRole { get; set; }
        
        // Additional properties needed by Index.cshtml
        public List<AnnouncementItem> Announcements { get; set; } = new();
        public ContactInfo ContactInfo { get; set; } = new();
        public List<ImportantDate> ImportantDates { get; set; } = new();
    }
    
    public class AnnouncementItem
    {
        public string Title { get; set; } = "";
        public string? Content { get; set; }
        public DateTime PublishDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
    
    public class ContactInfo
    {
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string WorkingHours { get; set; } = "";
        public string Website { get; set; } = "";
    }
    
    public class ImportantDate
    {
        public string Title { get; set; } = "";
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public string Type { get; set; } = ""; // deadline, event, etc.
    }
}