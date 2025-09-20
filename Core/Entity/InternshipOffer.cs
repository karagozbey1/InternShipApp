using Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    // Staj süre türünü belirten enum
    public enum OfferDurationType
    {
        // Kısa süreli staj - genellikle 1-2 ay arası
        [Display(Name = "Kısa")]
        Kisa = 1,
        
        // Uzun süreli staj - genellikle 3-6 ay arası
        [Display(Name = "Uzun")]
        Uzun = 2,
        
        // Yarı zamanlı çalışma düzeni - günde 4-5 saat
        [Display(Name = "Yarı zamanlı")]
        YariZamanli = 3,
        
        // Tam zamanlı çalışma düzeni - günde 8 saat
        [Display(Name = "Tam zamanlı")]
        TamZamanli = 4
    }

    // İş teklifi türünü belirten enum
    public enum OfferJobType
    {
        // Sadece staj pozisyonu - geçici, eğitim amaçlı
        Internship = 1,
        
        // Sadece iş pozisyonu - kalıcı istihdam
        Job = 2,
        
        // Hem staj hem iş imkanı - staj sonrası kalma fırsatı
        Both = 3
    }

    // Para birimi türlerini tanımlayan enum
    public enum CurrencyType
    {
        // Türk Lirası - yerel para birimi
        [Display(Name = "₺ - TL")]
        TRY = 1,
        
        // Amerikan Doları - uluslararası standart
        [Display(Name = "$ - USD")]
        USD = 2,
        
        // Euro - Avrupa para birimi
        [Display(Name = "€ - EUR")]
        EUR = 3
    }

    // Tarih seçim modunu belirten enum
    public enum DateSelectionMode
    {
        // Şirket önceden tanımlanmış bir dönem belirlemiş
        Period = 1,
        
        // Şirket kesin başlangıç ve bitiş tarihlerini vermiş
        FixedRange = 2,
        
        // Şirket tarih kısıtlaması getirmemiş
        Open = 3
    }

    // Staj teklifi entity - şirketlerin staj ilanlarını temsil eder
    public class InternshipOffer
    {
        // Primary key - benzersiz teklif tanımlayıcısı
        public int Id { get; set; }
        
        // İlan başlığı - pozisyon adını belirtir
        public string Title { get; set; } = null!;

        // Detaylı iş açıklaması - opsiyonel
        public string? Description { get; set; }
        
        // Başvuru için gerekli şartlar - opsiyonel
        public string? Requirements { get; set; }
        
        // Stajın yapılacağı şehir - zorunlu alan
        public string City { get; set; } = null!;
        
        // Stajın alanı/sektörü - zorunlu alan
        public string Field { get; set; } = null!;
     
        // Mevcut pozisyon sayısı - kaç öğrenci alınacağını belirtir
        public int AvailablePositions { get; set; } = 0;
        
        // Teklifin aktif durumu - false ise başvuru alınmaz
        public bool IsActive { get; set; } = true;
        
        // Teklifin oluşturulma tarihi - otomatik atanır
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Başvuru son tarihi - opsiyonel, null ise süresiz
        public DateTime? ApplicationDeadline { get; set; }
        
        // Foreign key - teklifi veren şirketin ID'si
        public int CompanyId { get; set; }
        
        // Navigation property - teklifi veren şirket
        public Company? Company { get; set; }

        // Foreign key - staj döneminin ID'si
        public int PeriodId { get; set; }
        
        // Navigation property - staj dönemi
        public InternshipPeriod? Period { get; set; }

        // Stajın süre türü - enum değeri
        public OfferDurationType Duration { get; set; } = OfferDurationType.Kisa;

        // İş teklifi türü - enum değeri
        public OfferJobType JobType { get; set; } = OfferJobType.Internship;

        // Aylık maaş miktarı - opsiyonel
        public decimal? MonthlySalary { get; set; }

        // Maaş para birimi - enum değeri
        public CurrencyType MonthlySalaryCurrency { get; set; } = CurrencyType.TRY;

        // Navigation property - bu ilana yapılan başvurular
        public ICollection<InternshipApplication>? InternshipApplications { get; set; }

        // Tarih seçim modu - öğrencinin tarih esnekliği
        public DateSelectionMode DateMode { get; set; } = DateSelectionMode.Period;
    }
}
