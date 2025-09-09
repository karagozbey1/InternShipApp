using System.ComponentModel.DataAnnotations;
using Core.Entity;

namespace WebUI.Models.ViewModels.Student
{
    // Approval Form List ViewModels
    public class ApprovalFormListViewModel
    {
        public List<ApprovalFormViewModel> ApprovalForms { get; set; } = new();
        public Core.Entity.InternshipApprovalStatus? StatusFilter { get; set; }
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public string SearchTerm { get; set; } = "";
    }

    public class DirectApprovalFormViewModel
    {
        [Required(ErrorMessage = "Þirket adý zorunludur")]
        public string CompanyName { get; set; } = "";
        
        [Required(ErrorMessage = "Baþlangýç tarihi zorunludur")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "Bitiþ tarihi zorunludur")]
        public DateTime EndDate { get; set; }
        
        [Required(ErrorMessage = "Haftalýk çalýþma saati zorunludur")]
        [Range(1, 40, ErrorMessage = "Haftalýk çalýþma saati 1-40 saat arasýnda olmalýdýr")]
        public int WeeklyWorkHours { get; set; }
        
        public string? WorkplaceAddress { get; set; }
        public string? InternshipCoordinator { get; set; }
        public string? CoordinatorPhone { get; set; }
        public string? CoordinatorEmail { get; set; }
        public string? InternshipDescription { get; set; }
        public string? StudentNote { get; set; }
        
        public List<StudentDocumentVM> StudentDocuments { get; set; } = new();
        public List<int> SelectedDocumentIds { get; set; } = new();
    }

    public class ModernApprovalViewModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeeklyWorkHours { get; set; }
        public string? WorkplaceAddress { get; set; }
        public string? InternshipCoordinator { get; set; }
        public string? CoordinatorPhone { get; set; }
        public string? CoordinatorEmail { get; set; }
        public string? InternshipDescription { get; set; }
        public string? StudentNote { get; set; }
        public string? AdvisorNote { get; set; }
        public string? RejectionReason { get; set; }
        public Core.Entity.InternshipApprovalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? AdvisorName { get; set; }
        public string StatusText { get; set; } = "";
        public string StatusColor { get; set; } = "";
        
        // For document selection
        public List<StudentDocumentVM> StudentDocuments { get; set; } = new();
        public List<int> SelectedDocumentIds { get; set; } = new();
        
        // For Step 2 - Company Selection
        public List<ApprovedApplicationVM> ApprovedApplications { get; set; } = new();

        // New: populated for Step4 review
        public List<DocumentVM> SelectedDocuments { get; set; } = new();
        public ApprovedApplicationVM? SelectedCompany { get; set; }
        public AdvisorVM? SelectedAdvisor { get; set; }
        public int? SelectedApplicationId { get; set; }
        public int? SelectedAdvisorId { get; set; }
    }

    public class ApprovedApplicationVM
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string CompanyName { get; set; } = "";
        public string Position { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class AdvisorVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Department { get; set; } = "";
        public string University { get; set; } = "";
    }

    public class ApprovalFormViewModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public string Position { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AdvisorName { get; set; } = "";
        public string? AdvisorDepartment { get; set; } // Added: show "Danýþman - Bölüm" in lists
        public Core.Entity.InternshipApprovalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StudentNote { get; set; } = "";
        public string AdvisorComment { get; set; } = "";
        public List<DocumentVM> Documents { get; set; } = new();
        
        // Additional properties needed by views
        public string? CoordinatorPhone { get; set; }
        public string? CoordinatorEmail { get; set; }
        public string StatusColor { get; set; } = "";
        public string StatusText { get; set; } = "";
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public Core.Entity.InternshipApprovalStatus StatusEnum { get; set; }
        public string? OfferTitle { get; set; }
        public int? WeeklyWorkHours { get; set; }
        public string? InternshipCoordinator { get; set; }
        public string? InternshipDescription { get; set; }
        public string? AdvisorNote { get; set; }
        public string? RejectionReason { get; set; }
        public string? WorkplaceAddress { get; set; }

        // NEW: Student display fields used by the modern details view
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public string? StudentDepartment { get; set; }
        public string? StudentUniversity { get; set; }
        public int? StudentGrade { get; set; }

        // NEW: Advisor contact fields (from Step 3 selection)
        public string? AdvisorEmail { get; set; }
        public string? AdvisorPhone { get; set; }
    }

    public class DocumentVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string FileName { get; set; } = "";
        public string DocumentType { get; set; } = "";
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string FilePath { get; set; } = "";
    }

    public class CreateApprovalFormViewModel
    {
        [Required(ErrorMessage = "Baþvuru seçimi zorunludur")]
        public int ApplicationId { get; set; }
        
        [Required(ErrorMessage = "Danýþman seçimi zorunludur")]
        public int AdvisorId { get; set; }
        
        [Required(ErrorMessage = "Belge seçimi zorunludur")]
        public List<int> SelectedDocumentIds { get; set; } = new();
        
        [StringLength(1000, ErrorMessage = "Not 1000 karakterden uzun olamaz")]
        public string StudentNote { get; set; } = "";
        
        // Display data
        public List<InternshipApplication> AvailableApplications { get; set; } = new();
        public List<InternshipDocument> AvailableDocuments { get; set; } = new();
        public List<User> AvailableAdvisors { get; set; } = new();
        
        // Additional properties needed by views
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeeklyWorkHours { get; set; }
        public string? InternshipDescription { get; set; }
        public string CompanyName { get; set; } = "";
        public string? WorkplaceAddress { get; set; }
        public string? InternshipCoordinator { get; set; }
        public string? CoordinatorPhone { get; set; }
        public string? CoordinatorEmail { get; set; }
        
        // For Edit.cshtml view
        public string ApplicationTitle { get; set; } = "";
    }

    public class UniversityApprovalFormViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Þirket adý zorunludur")]
        [StringLength(200, ErrorMessage = "Þirket adý 200 karakterden uzun olamaz")]
        public string CompanyName { get; set; } = "";
        
        [Required(ErrorMessage = "Staj baþlangýç tarihi zorunludur")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "Staj bitiþ tarihi zorunludur")]
        public DateTime EndDate { get; set; }
        
        [Range(1, 60, ErrorMessage = "Haftalýk çalýþma saati 1-60 arasýnda olmalýdýr")]
        public int WeeklyWorkHours { get; set; }
        
        [StringLength(500, ErrorMessage = "Ýþ yeri adresi 500 karakterden uzun olamaz")]
        public string? WorkplaceAddress { get; set; }
        
        [StringLength(100, ErrorMessage = "Koordinatör adý 100 karakterden uzun olamaz")]
        public string? InternshipCoordinator { get; set; }
        
        [StringLength(20, ErrorMessage = "Telefon numarasý 20 karakterden uzun olamaz")]
        public string? CoordinatorPhone { get; set; }
        
        [StringLength(100, ErrorMessage = "E-posta adresi 100 karakterden uzun olamaz")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string? CoordinatorEmail { get; set; }
        
        [StringLength(1000, ErrorMessage = "Staj açýklamasý 1000 karakterden uzun olamaz")]
        public string? InternshipDescription { get; set; }
        
        [StringLength(1000, ErrorMessage = "Öðrenci notu 1000 karakterden uzun olamaz")]
        public string? StudentNote { get; set; }
        
        public string? AdvisorNote { get; set; }
        public string? RejectionReason { get; set; }
        public Core.Entity.InternshipApprovalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? AdvisorName { get; set; }
        public string StatusText { get; set; } = "";
        public string StatusColor { get; set; } = "";
        
        // Additional properties needed by views
        public string? AdditionalNotes { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public int UniversityId { get; set; }
        public string UniversityName { get; set; } = "";
        
        // For UniversityApproval.cshtml view
        public string Subject { get; set; } = "";
        public string RequestType { get; set; } = "";
        public string Priority { get; set; } = "";
        public string ApplicationDetails { get; set; } = "";
    }
}