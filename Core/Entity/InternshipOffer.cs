using Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    // Staj süre türü
    public enum OfferDurationType
    {
        // Kısa süreli
        [Display(Name = "Kısa")]
        Kisa = 1,
        
        // Uzun süreli
        [Display(Name = "Uzun")]
        Uzun = 2,
        
        // Yarı zamanlı
        [Display(Name = "Yarı zamanlı")]
        YariZamanli = 3,
        
        // Tam zamanlı
        [Display(Name = "Tam zamanlı")]
        TamZamanli = 4
    }

    // İş türü
    public enum OfferJobType
    {
        // Sadece staj
        Internship = 1,
        
        // Sadece iş
        Job = 2,
        
        // Staj ve iş
        Both = 3
    }

    // Para birimi
    public enum CurrencyType
    {
        // Türk Lirası
        [Display(Name = "₺ - TL")]
        TRY = 1,
        
        // Amerikan Doları
        [Display(Name = "$ - USD")]
        USD = 2,
        
        // Euro
        [Display(Name = "€ - EUR")]
        EUR = 3
    }

    // Tarih seçim türü
    public enum DateSelectionMode
    {
        // Belirli dönem
        Period = 1,
        
        // Sabit tarih aralığı
        FixedRange = 2,
        
        // Esnek tarih
        Open = 3
    }

    // Staj ilanı
    public class InternshipOffer
    {
        // Kimlik numarası
        public int Id { get; set; }
        
        // İlan başlığı
        public string Title { get; set; } = null!;

        // İş açıklaması
        public string? Description { get; set; }
        
        // Gerekli şartlar
        public string? Requirements { get; set; }
        
        // Şehir
        public string City { get; set; } = null!;
        
        // Alan/sektör
        public string Field { get; set; } = null!;
     
        // Mevcut pozisyon sayısı
        public int AvailablePositions { get; set; } = 0;
        
        // Aktif durumu
        public bool IsActive { get; set; } = true;
        
        // Oluşturulma tarihi
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Başvuru son tarihi
        public DateTime? ApplicationDeadline { get; set; }
        
        // Şirket kimliği
        public int CompanyId { get; set; }
        
        // Şirket bilgileri
        public Company? Company { get; set; }

        // Dönem kimliği
        public int PeriodId { get; set; }
        
        // Dönem bilgileri
        public InternshipPeriod? Period { get; set; }

        // Süre türü
        public OfferDurationType Duration { get; set; } = OfferDurationType.Kisa;

        // İş türü
        public OfferJobType JobType { get; set; } = OfferJobType.Internship;

        // Aylık maaş
        public decimal? MonthlySalary { get; set; }

        // Maaş para birimi
        public CurrencyType MonthlySalaryCurrency { get; set; } = CurrencyType.TRY;

        // Başvurular
        public ICollection<InternshipApplication>? InternshipApplications { get; set; }

        // Tarih seçim türü
        public DateSelectionMode DateMode { get; set; } = DateSelectionMode.Period;
    }
}
