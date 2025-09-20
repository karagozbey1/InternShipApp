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
    // Stajın ne kadar süreceğini ve çalışma düzenini tanımlar
    // UI'da dropdown menülerde gösterilir
    public enum OfferDurationType
    {
        // Kısa süreli staj - Genellikle 1-2 ay arası
        // Yazlık stajlar veya deneme amaçlı stajlar için
        [Display(Name = "Kısa")]
        Kisa = 1,
        
        // Uzun süreli staj - Genellikle 3-6 ay arası
        // Zorunlu stajlar ve kapsamlı projeler için
        [Display(Name = "Uzun")]
        Uzun = 2,
        
        // Yarı zamanlı çalışma düzeni - Günde 4-5 saat
        // Eğitimle birlikte yapılabilen esnek çalışma
        [Display(Name = "Yarı zamanlı")]
        YariZamanli = 3,
        
        // Tam zamanlı çalışma düzeni - Günde 8 saat
        // Normal çalışma saatleri, tam mesai
        [Display(Name = "Tam zamanlı")]
        TamZamanli = 4
    }

    // İş teklifi türünü belirten enum
    // Pozisyonun staj mı iş mi yoksa her ikisi mi olduğunu gösterir
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
    // Maaş ödemelerinde hangi para biriminin kullanıldığını belirtir
    // Uluslararası şirketler için çoklu para birimi desteği
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
    // Şirketin staj tarihlerini nasıl belirlediğini gösterir
    // Öğrencinin tarih seçme esnekliğini kontrol eder
    public enum DateSelectionMode
    {
        // Şirket önceden tanımlanmış bir dönem belirlemiş
        // Öğrenci bu tarih aralığını değiştiremez
        // Academic calendar ile uyumlu dönemler için
        Period = 1,
        
        // Şirket kesin başlangıç ve bitiş tarihlerini vermiş
        // Sabit tarih aralığı, değiştirilemez
        // Belirli proje dönemleri için
        FixedRange = 2,
        
        // Şirket tarih kısıtlaması getirmemiş
        // Öğrenci kendi uygun tarihlerini seçebilir
        // Esnek staj programları için
        Open = 3
    }

    // Staj teklifi entity - şirketlerin staj ilanlarını temsil eder
    // Bu sınıf veritabanında InternshipOffers tablosuna karşılık gelir
    // Öğrencilerin başvurabileceği pozisyonları tanımlar
    public class InternshipOffer
    {
        // Primary key - benzersiz teklif tanımlayıcısı
        // Auto-increment, database tarafından otomatik atanır
        public int Id { get; set; }
        
        // İlan başlığı - öğrencilerin ilk göreceği bilgi
        // Pozisyon adını net bir şekilde belirtmeli
        // Not null - zorunlu alan, boş olamaz
        public string Title { get; set; } = null!;

        // Detaylı iş açıklaması - pozisyonun kapsamlı tanımı
        // Sorumluluklar, beklentiler ve yapılacak işler
        // Nullable - opsiyonel, kısa ilanlar için boş olabilir
        public string? Description { get; set; }
        
        // Başvuru için gerekli şartlar ve nitelikler
        // Teknik beceriler, deneyim seviyesi, eğitim gereksinimleri
        // Öğrencilerin self-assessment yapabilmesi için önemli
        public string? Requirements { get; set; }
        
        // Stajın yapılacağı şehir bilgisi
        // Konum bazlı filtreleme için kritik
        // Uzaktan çalışma durumunda "Remote" yazılabilir
        public string City { get; set; } = null!;
        
        // Staj alanı/sektörü - kategorik sınıflandırma
        // "Yazılım", "Pazarlama", "İnsan Kaynakları" gibi
        // Öğrencilerin alan bazlı arama yapabilmesi için
        public string Field { get; set; } = null!;
     
        // Kaç öğrenci alınacağını belirten kontenjan
        // 0 değeri kontenjan dolduğunu veya unlimited'i belirtebilir
        // Başvuru kontrolünde kullanılır
        public int AvailablePositions { get; set; } = 0;
        
        // İlanın aktif durumu - yayında mı yoksa pasif mi
        // false olduğunda öğrenciler başvuru yapamaz
        // Soft delete pattern için de kullanılabilir
        public bool IsActive { get; set; } = true;
        
        // İlan oluşturulma zamanı - audit trail için
        // Otomatik olarak şu anki UTC zamanı atanır
        // Sıralama ve filtreleme işlemlerinde kullanılır
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Başvuru kabul edilen son tarih - deadline kontrolü
        // null olduğunda süresiz başvuru kabul edilir
        // Business logic'te tarih kontrolü için kullanılır
        public DateTime? ApplicationDeadline { get; set; }
        
        // Foreign key - bu ilanı veren şirketin ID'si
        // Company tablosu ile one-to-many ilişki
        // Referential integrity için önemli
        public int CompanyId { get; set; }
        
        // Navigation property - ilişkili şirket entity'si
        // EF Core tarafından lazy loading ile doldurulur
        // Include() ile eager loading yapılabilir
        public Company? Company { get; set; }

        // Foreign key - hangi staj dönemine ait olduğu
        // Academic calendar ile entegrasyon için
        // Tarih yönetimini merkezi hale getirir
        public int PeriodId { get; set; }
        
        // Navigation property - staj dönemi bilgileri
        // Dönem başlangıç/bitiş tarihleri buradan alınır
        // Çoklu ilanların aynı dönemde gruplandırılmasını sağlar
        public InternshipPeriod? Period { get; set; }

        // Staj süre türü - enum değeri
        // Kısa/Uzun/Yarı/Tam zamanlı seçenekleri
        // Default olarak "Kısa" atanır
        public OfferDurationType Duration { get; set; } = OfferDurationType.Kisa;

        // İş türü kategorisi - enum değeri  
        // Staj/İş/Her ikisi seçenekleri
        // Öğrencilerin hedef türüne göre filtreleme yapar
        public OfferJobType JobType { get; set; } = OfferJobType.Internship;

        // Aylık maaş miktarı - opsiyonel finansal bilgi
        // null olduğunda "maaş belirtilmemiş" anlamına gelir
        // decimal türü para hesaplamaları için precision sağlar
        public decimal? MonthlySalary { get; set; }

        // Maaş para birimi - enum değeri
        // TRY/USD/EUR seçenekleri mevcut
        // Default Türk Lirası olarak ayarlanmış
        public CurrencyType MonthlySalaryCurrency { get; set; } = CurrencyType.TRY;

        // Navigation property - bu ilana yapılan başvurular
        // One-to-many ilişki, bir ilan birden fazla başvuru alabilir
        // Collection lazy loading ile doldurulur
        public ICollection<InternshipApplication>? InternshipApplications { get; set; }

        // Tarih seçim modu - öğrencinin tarih esnekliği
        // Period/FixedRange/Open seçenekleri
        // Başvuru formunda tarih alanlarının davranışını kontrol eder
        public DateSelectionMode DateMode { get; set; } = DateSelectionMode.Period;
    }
}
