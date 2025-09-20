using Core.Entity;

namespace Core.Entity;

// Şirket entity'si - staj pozisyonları sunan kurumların bilgilerini temsil eder
public class Company
{
    // Primary key - şirketin benzersiz tanımlayıcısı
    public int CompanyId { get; set; }

    // Şirketin resmi adı - zorunlu alan
    public string CompanyName { get; set; } = null!;
    
    // Şirketin faaliyet gösterdiği sektör - opsiyonel
    public string? Sector { get; set; }
   
    // Şirketin iletişim telefon numarası - opsiyonel
    public string? Phone { get; set; }
    
    // Şirketin sistem onay durumu - true ise ilan verebilir
    public bool Approved { get; set; }

    // Navigation property - şirketin verdiği staj teklifleri
    public ICollection<InternshipOffer>? InternshipOffers { get; set; }
    
    // Navigation property - şirket ile ilişkili kullanıcılar
    public ICollection<CompanyUser>? CompanyUsers { get; set; }
    
    // Şirketin vergi numarası - opsiyonel
    public string? TaxNumber { get; set; }
    
    // Şirketin fiziksel adresi - opsiyonel
    public string? Address { get; set; }
    
    // Şirket hakkında genel açıklama - opsiyonel
    public string? Description { get; set; }
    
    // Şirketin resmi e-posta adresi - opsiyonel
    public string? Email { get; set; }
    
    // Şirketin web sitesi URL'i - opsiyonel
    public string? Website { get; set; }
    
    // Şirketin çalışan sayısı - string format (örn: "50-100")
    public string? EmployeeCount { get; set; }
    
    // Şirketin kuruluş tarihi - opsiyonel
    public DateTime? EstablishedDate { get; set; }
    
    // Şirket logosunun dosya yolu - opsiyonel
    public string? LogoPath { get; set; }
}
