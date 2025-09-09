using System.ComponentModel.DataAnnotations;

namespace Core.Entity
{
    public class InternshipApprovalForm
    {
        public int Id { get; set; }
        
        [Required]
        public int ApplicationId { get; set; }
        public InternshipApplication? Application { get; set; }
        
        [Required]
        public int StudentId { get; set; }
        public User? Student { get; set; }
        
        public int? AdvisorId { get; set; }
        public User? Advisor { get; set; }
        
        [Required]
        [Display(Name = "Þirket Adý")]
        public string CompanyName { get; set; } = null!;
        
        [Required]
        [Display(Name = "Staj Baþlangýç Tarihi")]
        public DateTime StartDate { get; set; }
        
        [Required]
        [Display(Name = "Staj Bitiþ Tarihi")]
        public DateTime EndDate { get; set; }
        
        [Required]
        [Display(Name = "Haftalýk Çalýþma Saati")]
        public int WeeklyWorkHours { get; set; }
        
        [Display(Name = "Staj Yeri Adresi")]
        public string? WorkplaceAddress { get; set; }
        
        [Display(Name = "Staj Koordinatörü")]
        public string? InternshipCoordinator { get; set; }
        
        [Display(Name = "Koordinatör Telefonu")]
        public string? CoordinatorPhone { get; set; }
        
        [Display(Name = "Koordinatör E-posta")]
        public string? CoordinatorEmail { get; set; }
        
        [Display(Name = "Staj Konusu/Açýklamasý")]
        public string? InternshipDescription { get; set; }
        
        [Display(Name = "Öðrenci Notu")]
        public string? StudentNote { get; set; }
        
        [Display(Name = "Danýþman Notu")]
        public string? AdvisorNote { get; set; }
        
        public InternshipApprovalStatus Status { get; set; } = InternshipApprovalStatus.Pending;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        
        [Display(Name = "Red Nedeni")]
        public string? RejectionReason { get; set; }
    }
    
    public enum InternshipApprovalStatus
    {
        [Display(Name = "Onay Bekleniyor")]
        Pending = 0,
        
        [Display(Name = "Onaylandý")]
        Approved = 1,
        
        [Display(Name = "Reddedildi")]
        Rejected = 2
    }
}