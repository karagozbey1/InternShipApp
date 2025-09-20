using Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entity
{
    /// <summary>
    /// Staj süre türünü belirten enum
    /// Stajın ne kadar süreceğini ve çalışma düzenini gösterir
    /// </summary>
    public enum OfferDurationType
    {
        /// <summary>
        /// Kısa süreli staj - Genellikle 1-2 ay
        /// </summary>
        [Display(Name = "Kısa")]
        Kisa = 1,
        
        /// <summary>
        /// Uzun süreli staj - Genellikle 3-6 ay
        /// </summary>
        [Display(Name = "Uzun")]
        Uzun = 2,
        
        /// <summary>
        /// Yarı zamanlı çalışma - Günde 4-5 saat
        /// </summary>
        [Display(Name = "Yarı zamanlı")]
        YariZamanli = 3,
        
        /// <summary>
        /// Tam zamanlı çalışma - Günde 8 saat
        /// </summary>
        [Display(Name = "Tam zamanlı")]
        TamZamanli = 4
    }

    /// <summary>
    /// İş teklifi türünü belirten enum
    /// Staj mı yoksa tam zamanlı iş mi olduğunu gösterir
    /// </summary>
    public enum OfferJobType
    {
        /// <summary>
        /// Sadece staj pozisyonu
        /// </summary>
        Internship = 1,
        
        /// <summary>
        /// Sadece iş pozisyonu
        /// </summary>
        Job = 2,
        
        /// <summary>
        /// Hem staj hem iş imkanı
        /// </summary>
        Both = 3
    }

    /// <summary>
    /// Para birimi türünü belirten enum
    /// Maaş ödemelerinde kullanılan para birimi
    /// </summary>
    public enum CurrencyType
    {
        /// <summary>
        /// Türk Lirası
        /// </summary>
        [Display(Name = "₺ - TL")]
        TRY = 1,
        
        /// <summary>
        /// Amerikan Doları
        /// </summary>
        [Display(Name = "$ - USD")]
        USD = 2,
        
        /// <summary>
        /// Euro
        /// </summary>
        [Display(Name = "€ - EUR")]
        EUR = 3
    }

    /// <summary>
    /// Tarih seçim modunu belirten enum
    /// Şirketin tarihleri nasıl belirlediğini gösterir
    /// </summary>
    public enum DateSelectionMode
    {
        /// <summary>
        /// Şirket önceden tanımlanmış bir dönem seçmiş
        /// Öğrenci tarihleri değiştiremez
        /// </summary>
        Period = 1,
        
        /// <summary>
        /// Şirket sabit başlangıç/bitiş tarihleri vermiş
        /// Öğrenci tarihleri değiştiremez
        /// </summary>
        FixedRange = 2,
        
        /// <summary>
        /// Şirket tarih kısıtlaması getirmemiş
        /// Öğrenci istediği tarihleri girebilir
        /// </summary>
        Open = 3
    }

    /// <summary>
    /// Staj teklifi entity'si
    /// Şirketlerin öğrencilere sunduğu staj pozisyonlarını temsil eder
    /// İş tanımı, şartlar, süreler ve başvuru kriterlerini içerir
    /// </summary>
    public class InternshipOffer
    {
        /// <summary>
        /// Staj teklifinin benzersiz tanımlayıcısı
        /// Primary key olarak kullanılır
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Staj teklifinin başlığı
        /// Öğrencilerin göreceği ana başlık (örn: "Yazılım Geliştirici Stajyeri")
        /// Zorunlu alan
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Staj pozisyonunun detaylı açıklaması
        /// İş tanımı, sorumluluklar, beklentiler
        /// Opsiyonel alan
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Başvuru için gerekli şartlar
        /// Teknik beceriler, deneyim, eğitim seviyesi
        /// Opsiyonel alan
        /// </summary>
        public string? Requirements { get; set; }
        
        /// <summary>
        /// Stajın yapılacağı şehir
        /// Öğrencilerin konum bazlı filtreleme yapabilmesi için
        /// Zorunlu alan
        /// </summary>
        public string City { get; set; } = null!;
        
        /// <summary>
        /// Stajın alanı/sektörü
        /// Yazılım, Pazarlama, İnsan Kaynakları vb.
        /// Zorunlu alan
        /// </summary>
        public string Field { get; set; } = null!;
     
        /// <summary>
        /// Mevcut pozisyon sayısı
        /// Kaç öğrenci alınacağını belirtir
        /// 0 ise kontenjan dolmuş demektir
        /// </summary>
        public int AvailablePositions { get; set; } = 0;
        
        /// <summary>
        /// Teklifin aktif olup olmadığını belirtir
        /// False ise başvuru alınmaz
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Teklifin oluşturulma tarihi
        /// Otomatik olarak şu anki zaman atanır
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Başvuru son tarihi
        /// Bu tarihten sonra başvuru kabul edilmez
        /// Opsiyonel - null ise süresiz
        /// </summary>
        public DateTime? ApplicationDeadline { get; set; }
        
        /// <summary>
        /// Teklifi veren şirketin ID'si
        /// Company entity'sine foreign key
        /// </summary>
        public int CompanyId { get; set; }
        
        /// <summary>
        /// Teklifi veren şirket
        /// Navigation property - şirket bilgilerine erişim için
        /// </summary>
        public Company? Company { get; set; }

        /// <summary>
        /// Staj döneminin ID'si
        /// InternshipPeriod entity'sine bağlantı
        /// Başlangıç/bitiş tarihleri bu dönemden alınır
        /// </summary>
        public int PeriodId { get; set; }
        
        /// <summary>
        /// Staj dönemi
        /// Navigation property - dönem bilgilerine erişim için
        /// </summary>
        public InternshipPeriod? Period { get; set; }

        /// <summary>
        /// Stajın süre türü
        /// Kısa, Uzun, Yarı zamanlı veya Tam zamanlı
        /// </summary>
        public OfferDurationType Duration { get; set; } = OfferDurationType.Kisa;

        /// <summary>
        /// İş teklifi türü
        /// Staj, İş veya Her ikisi
        /// </summary>
        public OfferJobType JobType { get; set; } = OfferJobType.Internship;

        /// <summary>
        /// Aylık maaş miktarı
        /// Opsiyonel - null ise maaş belirtilmemiş
        /// </summary>
        public decimal? MonthlySalary { get; set; }

        /// <summary>
        /// Maaş para birimi
        /// TRY, USD veya EUR olabilir
        /// </summary>
        public CurrencyType MonthlySalaryCurrency { get; set; } = CurrencyType.TRY;

        /// <summary>
        /// Bu teklife yapılan başvurular koleksiyonu
        /// Navigation property - başvuru listesine erişim için
        /// </summary>
        public ICollection<InternshipApplication>? InternshipApplications { get; set; }

        /// <summary>
        /// Tarihlerin nasıl seçildiğini belirtir
        /// Period: Önceden tanımlı dönem
        /// FixedRange: Sabit tarih aralığı
        /// Open: Öğrenci tarihleri seçebilir
        /// </summary>
        public DateSelectionMode DateMode { get; set; } = DateSelectionMode.Period;
    }
}
