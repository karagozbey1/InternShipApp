namespace Core.Entity;

// Staj başvurusu durumunu belirten enum - başvurunun hangi aşamada olduğunu gösterir
public enum InternshipApplicationStatus
{
    // Beklemede - başvuru henüz değerlendirilmiyor
    Pending = 0,
    
    // Onaylandı - başvuru şirket tarafından kabul edildi
    Approved = 1,
    
    // Reddedildi - başvuru şirket tarafından reddedildi
    Rejected = 2
}

// Staj başvurusu entity'si - öğrencilerin şirket pozisyonlarına yaptığı başvuruları temsil eder
public class InternshipApplication
{
    // Primary key - başvuru benzersiz tanımlayıcısı
    public int Id { get; set; }
    
    // Başvurunun mevcut durumu - Pending, Approved veya Rejected
    public InternshipApplicationStatus Status { get; set; }
    
    // Öğrencinin başvuru ile birlikte gönderdiği not - opsiyonel
    public string? StudentNote { get; set; }
    
    // Başvurunun oluşturulma tarihi - otomatik atanır
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key - başvuru yapılan staj teklifinin ID'si
    public int InternshipOfferId { get; set; }
    
    // Navigation property - başvuru yapılan staj teklifi
    public InternshipOffer? InternshipOffer { get; set; }
    
    // Foreign key - başvuru yapan öğrencinin ID'si
    public int StudentId { get; set; }
    
    // Navigation property - başvuruyu yapan öğrenci
    public User? Student { get; set; }
    
    // Foreign key - başvuruyu değerlendiren danışmanın ID'si (opsiyonel)
    public int? AdvisorId { get; set; }
    
    // Navigation property - başvuruyu değerlendiren danışman
    public User? Advisor { get; set; }
    
    // Stajın başlangıç tarihi - öğrencinin çalışmaya başlayacağı tarih
    public DateTime StartDate { get; set; }
    
    // Stajın bitiş tarihi - öğrencinin stajını tamamlayacağı tarih
    public DateTime EndDate { get; set; }
    
    // Navigation property - başvuruya eklenen belgeler koleksiyonu
    public ICollection<InternshipDocument>? InternshipDocuments { get; set; }
    
    // Navigation property - başvuru hakkında yapılan yorumlar
    public ICollection<InternshipComment>? InternshipComments { get; set; }
    
    // Başvurunun son güncellenme tarihi - durum değişikliklerinde güncellenir
    public DateTime UpdatedAt { get; set; }
    
    // Bu stajın öğrenci için zorunlu olup olmadığını belirtir
    public bool IsCompulsory { get; set; } = false;
}
