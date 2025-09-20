namespace Core.Entity;

/// <summary>
/// Staj başvurusu durumunu belirten enum
/// Başvurunun hangi aşamada olduğunu gösterir
/// </summary>
public enum InternshipApplicationStatus
{
    /// <summary>
    /// Beklemede - Başvuru henüz değerlendirilmedi
    /// Yeni gönderilen başvuruların varsayılan durumu
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Onaylandı - Başvuru şirket tarafından kabul edildi
    /// Öğrenci staja başlayabilir
    /// </summary>
    Approved = 1,
    
    /// <summary>
    /// Reddedildi - Başvuru şirket tarafından reddedildi
    /// Öğrenci başka pozisyonlara başvurabilir
    /// </summary>
    Rejected = 2
}

/// <summary>
/// Staj başvurusu entity'si
/// Öğrencilerin şirket pozisyonlarına yaptığı başvuruları temsil eder
/// Başvuru süreci, değerlendirme ve sonuçlarını içerir
/// </summary>
public class InternshipApplication
{
    /// <summary>
    /// Başvuru benzersiz tanımlayıcısı
    /// Primary key olarak kullanılır
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Başvurunun mevcut durumu
    /// Pending, Approved veya Rejected olabilir
    /// </summary>
    public InternshipApplicationStatus Status { get; set; }
    
    /// <summary>
    /// Öğrencinin başvuru ile birlikte gönderdiği not
    /// Neden bu pozisyonu istediği, yetenekleri hakkında bilgi
    /// Opsiyonel alan - null olabilir
    /// </summary>
    public string? StudentNote { get; set; }
    
    /// <summary>
    /// Başvurunun oluşturulma tarihi ve saati
    /// Otomatik olarak şu anki zaman atanır
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Başvuru yapılan staj teklifinin ID'si
    /// InternshipOffer entity'sine foreign key
    /// </summary>
    public int InternshipOfferId { get; set; }
    
    /// <summary>
    /// Başvuru yapılan staj teklifi
    /// Navigation property - EF Core tarafından doldurulur
    /// </summary>
    public InternshipOffer? InternshipOffer { get; set; }
    
    /// <summary>
    /// Başvuru yapan öğrencinin ID'si
    /// User entity'sindeki öğrenciye foreign key
    /// </summary>
    public int StudentId { get; set; }
    
    /// <summary>
    /// Başvuruyu yapan öğrenci
    /// Navigation property - öğrenci bilgilerine erişim için
    /// </summary>
    public User? Student { get; set; }
    
    /// <summary>
    /// Başvuruyu değerlendiren danışmanın ID'si
    /// Opsiyonel - her başvurunun danışmanı olmayabilir
    /// </summary>
    public int? AdvisorId { get; set; }
    
    /// <summary>
    /// Başvuruyu değerlendiren danışman
    /// Navigation property - danışman bilgilerine erişim için
    /// </summary>
    public User? Advisor { get; set; }
    
    /// <summary>
    /// Stajın başlangıç tarihi
    /// Öğrencinin şirkette çalışmaya başlayacağı tarih
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Stajın bitiş tarihi
    /// Öğrencinin stajını tamamlayacağı tarih
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Başvuruya eklenen belgeler koleksiyonu
    /// CV, transkript, motivasyon mektubu gibi dokümanlar
    /// </summary>
    public ICollection<InternshipDocument>? InternshipDocuments { get; set; }
    
    /// <summary>
    /// Başvuru hakkında yapılan yorumlar koleksiyonu
    /// Şirket, öğrenci ve danışman yorumları
    /// </summary>
    public ICollection<InternshipComment>? InternshipComments { get; set; }
    
    /// <summary>
    /// Başvurunun son güncellenme tarihi
    /// Durum değişiklikleri veya notlar eklendiğinde güncellenir
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Bu stajın öğrenci için zorunlu olup olmadığını belirtir
    /// True ise zorunlu staj, false ise gönüllü staj
    /// Mezuniyet gereksinimleri için önemli
    /// </summary>
    public bool IsCompulsory { get; set; } = false;
}
