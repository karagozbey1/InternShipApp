using System.ComponentModel.DataAnnotations;
using Core.Entity;

namespace WebUI.Models.Student
{
    public class ApprovalFormListViewModel
    {
        public List<ApprovalFormItemVM> ApprovalForms { get; set; } = new();
        public ApprovalFormStatsVM Stats { get; set; } = new();
        public string StatusFilter { get; set; } = "";
        public string SearchTerm { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class ApprovalFormItemVM
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public string Position { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AdvisorName { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string StudentNote { get; set; } = "";
        public string AdvisorComment { get; set; } = "";
        public int DocumentCount { get; set; }
    }

    public class ApprovalFormStatsVM
    {
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int DraftCount { get; set; }
    }

    public class ApprovalFormDetailsViewModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public string Position { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeeklyWorkHours { get; set; }
        public string InternshipDescription { get; set; } = "";
        public string StudentNote { get; set; } = "";
        public string AdvisorComment { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? AdvisorResponseDate { get; set; }
        
        // Student info
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public string StudentPhone { get; set; } = "";
        public string University { get; set; } = "";
        public string Department { get; set; } = "";
        
        // Advisor info
        public string AdvisorName { get; set; } = "";
        public string AdvisorEmail { get; set; } = "";
        public string AdvisorDepartment { get; set; } = "";
        
        // Documents
        public List<ApprovalDocumentVM> Documents { get; set; } = new();
        
        // Application info
        public int ApplicationId { get; set; }
        public string ApplicationStatus { get; set; } = "";
    }

    public class ApprovalDocumentVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string FileName { get; set; } = "";
        public string DocumentType { get; set; } = "";
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string FilePath { get; set; } = "";
        public bool IsRequired { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovalComment { get; set; } = "";
    }

    public class EditApprovalFormViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Þirket adý zorunludur")]
        [StringLength(200, ErrorMessage = "Þirket adý 200 karakterden uzun olamaz")]
        public string CompanyName { get; set; } = "";
        
        [Required(ErrorMessage = "Pozisyon zorunludur")]
        [StringLength(200, ErrorMessage = "Pozisyon 200 karakterden uzun olamaz")]
        public string Position { get; set; } = "";
        
        [Required(ErrorMessage = "Baþlangýç tarihi zorunludur")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "Bitiþ tarihi zorunludur")]
        public DateTime EndDate { get; set; }
        
        [Range(1, 60, ErrorMessage = "Haftalýk çalýþma saati 1-60 arasýnda olmalýdýr")]
        public int WeeklyWorkHours { get; set; } = 40;
        
        [Required(ErrorMessage = "Staj açýklamasý zorunludur")]
        [StringLength(1000, ErrorMessage = "Staj açýklamasý 1000 karakterden uzun olamaz")]
        public string InternshipDescription { get; set; } = "";
        
        [StringLength(500, ErrorMessage = "Öðrenci notu 500 karakterden uzun olamaz")]
        public string StudentNote { get; set; } = "";
        
        public List<int> SelectedDocumentIds { get; set; } = new();
        public List<ApprovalDocumentVM> AvailableDocuments { get; set; } = new();
        public List<ApprovalDocumentVM> CurrentDocuments { get; set; } = new();
        
        public string Status { get; set; } = "";
        public bool CanEdit { get; set; } = true;
    }
}