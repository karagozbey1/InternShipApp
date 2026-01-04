using System;
using System.ComponentModel.DataAnnotations;
using Core.Entity;

namespace WebUI.Models.Company
{
    public class CompanyDashboardViewModel
    {
        public string CompanyName { get; set; } = "";
        public string Sector { get; set; } = "";
        public int TotalJobPostings { get; set; }
        public int TotalApplications { get; set; }
        public int ActiveInterns { get; set; }
        public int TotalInterns { get; set; }
        public List<RecentApplicationViewModel> RecentApplications { get; set; } = new();
        public List<ActiveInternViewModel> ActiveInternsList { get; set; } = new();
        
        // New: Dynamic recent activities
        public List<CompanyActivityViewModel> RecentActivities { get; set; } = new();
    }

    // New: Company activity view model
    public class CompanyActivityViewModel
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Color { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string ActivityType { get; set; } = "";
        public int? RelatedId { get; set; }
        
        // Helper properties
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;
                if (timeSpan.TotalMinutes < 1)
                    return "Az önce";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} dakika önce";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} saat önce";
                if (timeSpan.TotalDays < 7)
                    return $"{(int)timeSpan.TotalDays} gün önce";
                return CreatedAt.ToString("dd MMM yyyy");
            }
        }
    }

    public class CompanyProfileViewModel
    {
        public int CompanyId { get; set; }
        
        [Display(Name = "Þirket Adý")]
        public string CompanyName { get; set; } = "";
        
        [Display(Name = "Sektör")]
        public string Sector { get; set; } = "";
        
        [Display(Name = "Telefon")]
        public string Phone { get; set; } = "";
        
        [Display(Name = "Vergi Numarasý")]
        public string TaxNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string Description { get; set; } = "";
        
        [Display(Name = "E-posta")]
        public string Email { get; set; } = "";
        
        [Display(Name = "Web Sitesi")]
        public string Website { get; set; } = "";
        
        [Display(Name = "Çalýþan Sayýsý")]
        public string EmployeeCount { get; set; } = "";
        
        [Display(Name = "Kuruluþ Tarihi")]
        public DateTime? EstablishedDate { get; set; }
        public string LogoPath { get; set; } = "";
        
        [Display(Name = "Kullanýcý Adý Soyadý")]
        public string UserFullName { get; set; } = "";
        
        public int TotalInterns { get; set; }
        public int ActiveJobPostings { get; set; }
        public int SuccessRate { get; set; }
        public int MembershipYear { get; set; }
    }

    public class JobPostingCreateViewModel
    {
        // If Id > 0 then this is edit mode
        public int Id { get; set; }

        [Required(ErrorMessage = "Ýlan baþlýðý zorunludur")]
        [Display(Name = "Ýlan Baþlýðý")]
        public string Title { get; set; } = "";
        
        [Required(ErrorMessage = "Ýlan açýklamasý zorunludur")]
        [Display(Name = "Ýlan Açýklamasý")]
        public string Description { get; set; } = "";
        
        [Required(ErrorMessage = "Pozisyon zorunludur")]
        [Display(Name = "Pozisyon")]
        public string Position { get; set; } = "";
        
        [Required(ErrorMessage = "Ýlan türü zorunludur")]
        [Display(Name = "Ýlan Türü")]
        public JobPostingType JobType { get; set; } = JobPostingType.Internship;
        
        [Display(Name = "Gereksinimler")]
        public string? Requirements { get; set; }
        
        [Required(ErrorMessage = "Þehir zorunludur")]
        [Display(Name = "Þehir")]
        public int LocationId { get; set; }
        public List<Core.Entity.Province> AvailableLocations { get; set; } = new();
        
        [Required(ErrorMessage = "Son baþvuru tarihi zorunludur")]
        [Display(Name = "Son Baþvuru Tarihi")]
        [DataType(DataType.Date)]
        public DateTime ApplicationDeadline { get; set; }
        
        [Range(1, 100, ErrorMessage = "Kota 1-100 arasýnda olmalýdýr")]
        [Display(Name = "Kota")]
        public int Quota { get; set; } = 1;
        
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        // New: how company wants date selection to work for students
        [Display(Name = "Tarih Seçim Modu")]
        public Core.Entity.DateSelectionMode DateMode { get; set; } = Core.Entity.DateSelectionMode.Period;

        // Mode: true => use Period selection; false => manual dates (legacy)
        public bool UsePeriod { get; set; } = true;

        // Start and End dates used when no period selected or for manual override
        [Display(Name = "Baþlangýç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow.AddDays(30);

        [Display(Name = "Bitiþ Tarihi")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(90);

        // Salary (monthly)
        [Display(Name = "Ücret (Aylýk)")]
        [Range(0, double.MaxValue, ErrorMessage = "Ücret negatif olamaz")]
        public decimal? MonthlySalary { get; set; }

        [Display(Name = "Para Birimi")]
        public Core.Entity.CurrencyType MonthlySalaryCurrency { get; set; } = Core.Entity.CurrencyType.TRY;

        [Display(Name = "Dönem")]
        public int PeriodId { get; set; }
        public List<InternshipPeriod> AvailablePeriods { get; set; } = new();

        [Required(ErrorMessage = "Süre tipi seçimi zorunludur")]
        [Display(Name = "Çalýþma Tipi")]
        public OfferDurationType? Duration { get; set; }
        public List<OfferDurationType> AvailableDurations { get; set; } = new();
    }

    public class JobPostingListViewModel
    {
        public List<JobPostingItemViewModel> JobPostings { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public JobPostingType? TypeFilter { get; set; }
        public bool? IsActiveFilter { get; set; }
    }

    public class JobPostingItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Position { get; set; } = "";
        public JobPostingType JobType { get; set; }
        public string Location { get; set; } = "";
        public decimal? Salary { get; set; }
        public CurrencyType SalaryCurrency { get; set; } = CurrencyType.TRY;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public int Quota { get; set; }
        public int ApplicationCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ApplicationListViewModel
    {
        public List<ApplicationItemViewModel> Applications { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public ApplicationStatus? StatusFilter { get; set; }
        public int? JobPostingIdFilter { get; set; }
        public List<JobPostingSelectViewModel> AvailableJobPostings { get; set; } = new();
    }

    public class ApplicationItemViewModel
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public string University { get; set; } = "";
        public string Department { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public ApplicationStatus Status { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string? CoverLetter { get; set; }
        public string? CvPath { get; set; }
    }

    public class ApplicationDetailViewModel
    {
        public int StudentId { get; set; }
        public int Id { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public string StudentPhone { get; set; } = "";
        public string University { get; set; } = "";
        public string Department { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public string JobDescription { get; set; } = "";
        public ApplicationStatus Status { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string? CoverLetter { get; set; }
        public string? CvPath { get; set; }
        public decimal? Gpa { get; set; }
        public List<string> Skills { get; set; } = new();
        public List<ExperienceViewModel> Experiences { get; set; } = new();

        // Salary info from the related InternshipOffer
        public decimal? Salary { get; set; }
        public Core.Entity.CurrencyType SalaryCurrency { get; set; } = Core.Entity.CurrencyType.TRY;

        // New: attached documents (CV, transkript, vb.)
        public List<DocumentItemViewModel> Documents { get; set; } = new();

        // New: formatted student class (e.g. "4. sýnýf")
        public string StudentClass { get; set; } = "Bilinmeyen";
    }

    // Simple view model to represent uploaded documents for company view
    public class DocumentItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public long FileSize { get; set; }
        public string DocumentType { get; set; } = "Other";
        public DateTime UploadedAt { get; set; }
    }

    public class RecentApplicationViewModel
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public DateTime ApplicationDate { get; set; }
        public ApplicationStatus Status { get; set; }
    }

    public class ActiveInternViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string Position { get; set; } = "";
        public string University { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CompletedDays { get; set; }
        public int TotalDays { get; set; }
        public decimal ProgressPercentage => TotalDays > 0 ? (decimal)CompletedDays / TotalDays * 100 : 0;
    }

    // New: Detailed intern view model for detail page
    public class InternDetailViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public string StudentPhone { get; set; } = "";
        public string StudentNumber { get; set; } = "";
        public string University { get; set; } = "";
        public string Department { get; set; } = "";
        public string Position { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string RemainingTime { get; set; } = "";
        public decimal Progress { get; set; }
        public string Status { get; set; } = "";
        public string StudentClass { get; set; } = "";
        public string? CoverLetter { get; set; }
        public List<DocumentItemViewModel> Documents { get; set; } = new();
        
        // Deðerlendirme bilgileri
        public bool HasEvaluation { get; set; }
        public EvaluationViewModel? Evaluation { get; set; }
    }

    // Deðerlendirme görüntüleme modeli
    public class EvaluationViewModel
    {
        public int Id { get; set; }
        public int TechnicalSkillsRating { get; set; }
        public int CommunicationRating { get; set; }
        public int TeamworkRating { get; set; }
        public int PunctualityRating { get; set; }
        public int InitiativeRating { get; set; }
        public int ProblemSolvingRating { get; set; }
        public int OverallRating { get; set; }
        public string? StrengthsComment { get; set; }
        public string? ImprovementAreasComment { get; set; }
        public string? GeneralComment { get; set; }
        public string? RecommendationComment { get; set; }
        public bool WouldRecommendForHire { get; set; }
        public bool WouldAcceptAgain { get; set; }
        public decimal AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CompanyReportsViewModel
    {
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int ActiveInterns { get; set; }
        public int CompletedInterns { get; set; }
        public int ThisMonthApplications { get; set; }
        public decimal ApprovalRate { get; set; }
        public List<MonthlyDataItem> MonthlyData { get; set; } = new();
        public List<DepartmentDataItem> DepartmentData { get; set; } = new();
        public List<StatusDataItem> StatusData { get; set; } = new();
    }

    public class MonthlyDataItem
    {
        public string Month { get; set; } = "";
        public int Applications { get; set; }
        public int Approvals { get; set; }
    }

    public class DepartmentDataItem
    {
        public string Department { get; set; } = "";
        public int Count { get; set; }
    }

    public class StatusDataItem
    {
        public string Status { get; set; } = "";
        public int Count { get; set; }
    }

    public class JobPostingSelectViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Position { get; set; } = "";
    }

    public class ExperienceViewModel
    {
        public string Company { get; set; } = "";
        public string Position { get; set; } = "";
        public string Duration { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class JobPostingDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Position { get; set; } = "";
        public JobPostingType JobType { get; set; }
        public string Location { get; set; } = "";
        public decimal? Salary { get; set; }
        public CurrencyType SalaryCurrency { get; set; } = CurrencyType.TRY;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public int Quota { get; set; }
        public int ApplicationCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Description { get; set; } = "";
        public string Requirements { get; set; } = "";
        public string CompanyName { get; set; } = "";
    }

    public enum JobPostingType
    {
        [Display(Name = "Staj")]
        Internship = 1,
        [Display(Name = "Ýþ")]
        Job = 2,
        [Display(Name = "Staj ve Ýþ")]
        Both = 3
    }

    public enum ApplicationStatus
    {
        [Display(Name = "Beklemede")]
        Pending = 1,
        [Display(Name = "Ýnceleniyor")]
        Reviewing = 2,
        [Display(Name = "Onaylandý")]
        Approved = 3,
        [Display(Name = "Reddedildi")]
        Rejected = 4,
        [Display(Name = "Ýptal Edildi")]
        Cancelled = 5
    }
}