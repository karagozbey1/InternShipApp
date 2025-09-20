namespace Core.Entity;

// Başvuru durumu
public enum InternshipApplicationStatus
{
    // Beklemede
    Pending = 0,
    
    // Onaylandı
    Approved = 1,
    
    // Reddedildi
    Rejected = 2
}

// Staj başvurusu
public class InternshipApplication
{
    // Kimlik numarası
    public int Id { get; set; }
    
    // Başvuru durumu
    public InternshipApplicationStatus Status { get; set; }
    
    // Öğrenci notu
    public string? StudentNote { get; set; }
    
    // Başvuru tarihi
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Staj ilanı kimliği
    public int InternshipOfferId { get; set; }
    
    // Staj ilanı
    public InternshipOffer? InternshipOffer { get; set; }
    
    // Öğrenci kimliği
    public int StudentId { get; set; }
    
    // Öğrenci
    public User? Student { get; set; }
    
    // Danışman kimliği
    public int? AdvisorId { get; set; }
    
    // Danışman
    public User? Advisor { get; set; }
    
    // Staj başlangıç tarihi
    public DateTime StartDate { get; set; }
    
    // Staj bitiş tarihi
    public DateTime EndDate { get; set; }
    
    // Başvuru belgeleri
    public ICollection<InternshipDocument>? InternshipDocuments { get; set; }
    
    // Başvuru yorumları
    public ICollection<InternshipComment>? InternshipComments { get; set; }
    
    // Son güncelleme tarihi
    public DateTime UpdatedAt { get; set; }
    
    // Zorunlu staj mı
    public bool IsCompulsory { get; set; } = false;
}
