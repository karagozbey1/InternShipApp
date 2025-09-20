using System.ComponentModel.DataAnnotations;

namespace Core.Entity
{
    // Staj onay formu
    public class InternshipApprovalForm
    {
        // Kimlik numarasý
        public int Id { get; set; }
        
        // Baþvuru kimliði
        [Required]
        public int ApplicationId { get; set; }
        
        // Baþvuru
        public InternshipApplication? Application { get; set; }
        
        // Öðrenci kimliði
        [Required]
        public int StudentId { get; set; }
        
        // Öðrenci
        public User? Student { get; set; }
        
        // Danýþman kimliði
        public int? AdvisorId { get; set; }
        
        // Danýþman
        public User? Advisor { get; set; }
        
        // Þirket adý
        [Required]
        [Display(Name = "Þirket Adý")]
        public string CompanyName { get; set; } = null!;
        
        // Baþlangýç tarihi
        [Required]
        [Display(Name = "Staj Baþlangýç Tarihi")]
        public DateTime StartDate { get; set; }
        
        // Bitiþ tarihi
        [Required]
        [Display(Name = "Staj Bitiþ Tarihi")]
        public DateTime EndDate { get; set; }
        
        // Haftalýk çalýþma saati
        [Required]
        [Display(Name = "Haftalýk Çalýþma Saati")]
        public int WeeklyWorkHours { get; set; }
        
        // Ýþ yeri adresi
        [Display(Name = "Staj Yeri Adresi")]
        public string? WorkplaceAddress { get; set; }
        
        // Koordinatör
        [Display(Name = "Staj Koordinatörü")]
        public string? InternshipCoordinator { get; set; }
        
        // Koordinatör telefonu
        [Display(Name = "Koordinatör Telefonu")]
        public string? CoordinatorPhone { get; set; }
        
        // Koordinatör e-posta
        [Display(Name = "Koordinatör E-posta")]
        public string? CoordinatorEmail { get; set; }
        
        // Staj açýklamasý
        [Display(Name = "Staj Konusu/Açýklamasý")]
        public string? InternshipDescription { get; set; }
        
        // Öðrenci notu
        [Display(Name = "Öðrenci Notu")]
        public string? StudentNote { get; set; }
        
        // Danýþman notu
        [Display(Name = "Danýþman Notu")]
        public string? AdvisorNote { get; set; }
        
        // Düzeltme notu
        [Display(Name = "Revizyon Notu")]
        public string? RevisionNote { get; set; }
        
        // Durum
        public InternshipApprovalStatus Status { get; set; } = InternshipApprovalStatus.Pending;
        
        // Oluþturulma tarihi
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Onaylanma tarihi
        public DateTime? ApprovedAt { get; set; }
        
        // Reddedilme tarihi
        public DateTime? RejectedAt { get; set; }
        
        // Düzeltme tarihi
        public DateTime? RevisedAt { get; set; }
        
        // Red nedeni
        [Display(Name = "Red Nedeni")]
        public string? RejectionReason { get; set; }
    }
    
    // Onay durumu
    public enum InternshipApprovalStatus
    {
        // Beklemede
        [Display(Name = "Onay Bekleniyor")]
        Pending = 0,
        
        // Onaylandý
        [Display(Name = "Onaylandý")]
        Approved = 1,
        
        // Reddedildi
        [Display(Name = "Reddedildi")]
        Rejected = 2,
        
        // Düzeltme istendi
        [Display(Name = "Revizyonda")]
        Revision = 3
    }
}