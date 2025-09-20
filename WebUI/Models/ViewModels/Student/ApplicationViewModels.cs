using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Entity;

namespace WebUI.Models.ViewModels.Student
{
    // Baþvuru görünüm modelleri
    
    // Baþvuru listesi modeli
    public class ApplicationListViewModel
    {
        public List<ApplicationItemVM> Applications { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public InternshipApplicationStatus? StatusFilter { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public ApplicationStatsVM Stats { get; set; } = new();
    }

    // Baþvuru liste öðesi modeli
    public class ApplicationItemVM
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public string Position { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public InternshipApplicationStatus Status { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Description { get; set; } = "";
        public bool CanCreateApprovalForm { get; set; }
        public int DocumentCount { get; set; }
        public string AdvisorName { get; set; } = "";
        public string AdvisorComment { get; set; } = "";
        public bool IsCompulsory { get; set; }
    }

    // Baþvuru istatistikleri modeli
    public class ApplicationStatsVM
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int WithdrawnApplications { get; set; }
        public double SuccessRate { get; set; }
    }

    // Baþvuru detay modeli
    public class ApplicationDetailsViewModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public string Position { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime AppliedDate { get; set; }
        public DateTime UpdatedAt { get; set; }
        public InternshipApplicationStatus Status { get; set; }
        public string StatusDisplayText { get; set; } = "";
        public string StatusColor { get; set; } = "";
        public List<DocumentViewModel> Documents { get; set; } = new();
        public bool CanWithdraw { get; set; }
        public bool CanCreateApprovalForm { get; set; }
        public string? StudentNote { get; set; }
        public string? AdvisorFeedback { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public OfferDetailsViewModel? OfferDetails { get; set; }
        public bool HasDiary { get; set; } = false;
        public int? DiaryId { get; set; }
        public bool IsCompulsory { get; set; }
    }

    // Baþvuru oluþturma modeli
    public class ApplicationCreateViewModel
    {
        [Required(ErrorMessage = "Staj teklifi seçimi zorunludur")]
        public int SelectedOfferId { get; set; }

        [Required(ErrorMessage = "Baþlangýç tarihi zorunludur")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiþ tarihi zorunludur")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [StringLength(1000, ErrorMessage = "Açýklama en fazla 1000 karakter olabilir")]
        public string? Description { get; set; }

        public bool IsCompulsory { get; set; }

        public List<int> SelectedDocumentIds { get; set; } = new();
        public List<OfferSelectViewModel> AvailableOffers { get; set; } = new();
        public List<StudentDocumentVM> AvailableDocuments { get; set; } = new();
    }

    // Ýlan seçim modeli
    public class OfferSelectViewModel
    {
        public int OfferId { get; set; }
        public string Title { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string City { get; set; } = "";
        public string Field { get; set; } = "";
        public DateTime? ApplicationDeadline { get; set; }
    }

    // Belge görünüm modeli
    public class DocumentViewModel
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
    }

    // Ýlan detay modeli
    public class OfferDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Requirements { get; set; } = "";
        public string Benefits { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public string WorkingHours { get; set; } = "";
        public string Location { get; set; } = "";
        public string Position { get; set; } = "";
        public int Quota { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public string CompanyPhone { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanyWebsite { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CompanyDescription { get; set; } = string.Empty;
        public decimal? Salary { get; set; }
        public Core.Entity.CurrencyType SalaryCurrency { get; set; } = Core.Entity.CurrencyType.TRY;
        public List<OfferSelectViewModel> RelatedOffers { get; set; } = new List<OfferSelectViewModel>();
        public List<OfferSelectViewModel> OtherCompanyOffers { get; set; } = new List<OfferSelectViewModel>();
        public bool ShowEcommerceMode { get; set; } = false;
        public bool HasApplied { get; set; } = false;
        public bool IsFavorite { get; set; } = false;
    }

    // Favori ilanlar modeli
    public class FavoritesListViewModel
    {
        public List<OfferSelectViewModel> Favorites { get; set; } = new();
        public int TotalCount => Favorites?.Count ?? 0;
    }

    // Baþvuru durumu numaralandýrmasý
    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected,
        InProgress,
        Completed
    }
}
