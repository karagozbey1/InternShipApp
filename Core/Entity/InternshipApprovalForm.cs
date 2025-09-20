using System.ComponentModel.DataAnnotations;

namespace Core.Entity
{
    // Staj onay formu entity'si - danýþman onay sürecini yönetir
    public class InternshipApprovalForm
    {
        // Primary key - onay formu benzersiz tanýmlayýcýsý
        public int Id { get; set; }
        
        // Foreign key - onaylanan baþvurunun ID'si
        [Required]
        public int ApplicationId { get; set; }
        
        // Navigation property - onaylanan baþvuru
        public InternshipApplication? Application { get; set; }
        
        // Foreign key - stajyer öðrencinin ID'si
        [Required]
        public int StudentId { get; set; }
        
        // Navigation property - stajyer öðrenci
        public User? Student { get; set; }
        
        // Foreign key - onaylayan danýþmanýn ID'si (opsiyonel)
        public int? AdvisorId { get; set; }
        
        // Navigation property - onaylayan danýþman
        public User? Advisor { get; set; }
        
        // Þirket adý - zorunlu alan
        [Required]
        [Display(Name = "Þirket Adý")]
        public string CompanyName { get; set; } = null!;
        
        // Staj baþlangýç tarihi - zorunlu alan
        [Required]
        [Display(Name = "Staj Baþlangýç Tarihi")]
        public DateTime StartDate { get; set; }
        
        // Staj bitiþ tarihi - zorunlu alan
        [Required]
        [Display(Name = "Staj Bitiþ Tarihi")]
        public DateTime EndDate { get; set; }
        
        // Haftalýk çalýþma saati - zorunlu alan
        [Required]
        [Display(Name = "Haftalýk Çalýþma Saati")]
        public int WeeklyWorkHours { get; set; }
        
        // Staj yeri adresi - opsiyonel
        [Display(Name = "Staj Yeri Adresi")]
        public string? WorkplaceAddress { get; set; }
        
        // Staj koordinatörü adý - opsiyonel
        [Display(Name = "Staj Koordinatörü")]
        public string? InternshipCoordinator { get; set; }
        
        // Koordinatör telefonu - opsiyonel
        [Display(Name = "Koordinatör Telefonu")]
        public string? CoordinatorPhone { get; set; }
        
        // Koordinatör e-postasý - opsiyonel
        [Display(Name = "Koordinatör E-posta")]
        public string? CoordinatorEmail { get; set; }
        
        // Staj açýklamasý - opsiyonel
        [Display(Name = "Staj Konusu/Açýklamasý")]
        public string? InternshipDescription { get; set; }
        
        // Öðrenci notu - opsiyonel
        [Display(Name = "Öðrenci Notu")]
        public string? StudentNote { get; set; }
        
        // Danýþman notu - opsiyonel
        [Display(Name = "Danýþman Notu")]
        public string? AdvisorNote { get; set; }
        
        // Revizyon notu - opsiyonel
        [Display(Name = "Revizyon Notu")]
        public string? RevisionNote { get; set; }
        
        // Onay durumu - enum deðeri
        public InternshipApprovalStatus Status { get; set; } = InternshipApprovalStatus.Pending;
        
        // Form oluþturulma zamaný - otomatik atanýr
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Onaylanma zamaný - opsiyonel
        public DateTime? ApprovedAt { get; set; }
        
        // Reddedilme zamaný - opsiyonel
        public DateTime? RejectedAt { get; set; }
        
        // Revizyon zamaný - opsiyonel
        public DateTime? RevisedAt { get; set; }
        
        // Red nedeni - opsiyonel
        [Display(Name = "Red Nedeni")]
        public string? RejectionReason { get; set; }
    }
    
    // Staj onay durumu enum'u
    public enum InternshipApprovalStatus
    {
        // Onay bekleniyor - varsayýlan durum
        [Display(Name = "Onay Bekleniyor")]
        Pending = 0,
        
        // Onaylandý - danýþman onayladý
        [Display(Name = "Onaylandý")]
        Approved = 1,
        
        // Reddedildi - danýþman reddetti
        [Display(Name = "Reddedildi")]
        Rejected = 2,
        
        // Revizyonda - düzeltme istendi
        [Display(Name = "Revizyonda")]
        Revision = 3
    }
}