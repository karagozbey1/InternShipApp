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
        
        // Extended (optional) fields for detailed public announcement / offer style view
        public string? CompanyName { get; set; }
        public string? City { get; set; }
        public string? Field { get; set; }
        public string? Position { get; set; }
        public int? Quota { get; set; }
        public string? WorkType { get; set; } // Tam/Zaman, Remote vb.
        public string? Salary { get; set; }
        public string? Requirements { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
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

    // New DTOs for public offers (Announcements page repurposed)
    public class PublicOfferListItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public DateTime PublishDate { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public int Quota { get; set; }
        public string WorkType { get; set; } = string.Empty; // Internship / Job / Both
        public string Salary { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public bool IsFavorite { get; set; } // New property for UI star state

        // Newly added properties
        public bool IsActive { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public string? CompanyDescription { get; set; } // newly added
        public string? EmployeeCount { get; set; } // new
        public DateTime? EstablishedDate { get; set; } // new
    }

    public class AnnouncementOffersViewModel
    {
        // Filter state
        public string Query { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Sort { get; set; } = "new";
        // Pagination
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        // Data
        public List<PublicOfferListItem> Offers { get; set; } = new();
        // Distinct filter options
        public List<string> DistinctCities { get; set; } = new();
        public List<string> DistinctFields { get; set; } = new();
        public List<string> DistinctCompanies { get; set; } = new();
    }

    // Dynamic About page models
    public class AboutViewModel
    {
        public string Title { get; set; } = "Hakkýmýzda";
        public string Mission { get; set; } = "";
        public string Vision { get; set; } = "";
        public List<TeamMember> Team { get; set; } = new();
        public List<CoreValueItem> CoreValues { get; set; } = new();
        public int ActiveStudentCount { get; set; }
        public int CompanyCount { get; set; }
        public int InternshipCompleted { get; set; }
    }
    public class TeamMember
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
    public class CoreValueItem
    {
        public string Icon { get; set; } = "fas fa-check-circle";
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // Dynamic Contact page models
    public class ContactViewModel
    {
        public List<ContactPerson> People { get; set; } = new();
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string WorkingHours { get; set; } = string.Empty;
        public List<FaqItem> Faq { get; set; } = new();
    }
    public class ContactPerson
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
    public class FaqItem
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }
}