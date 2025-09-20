using Core.Entity;

namespace Core.Entity;

// Şirket bilgileri
public class Company
{
    // Kimlik numarası
    public int CompanyId { get; set; }

    // Şirket adı
    public string CompanyName { get; set; } = null!;
    
    // Sektör
    public string? Sector { get; set; }
   
    // Telefon
    public string? Phone { get; set; }
    
    // Onaylandı mı
    public bool Approved { get; set; }

    // Staj ilanları
    public ICollection<InternshipOffer>? InternshipOffers { get; set; }
    
    // Şirket kullanıcıları
    public ICollection<CompanyUser>? CompanyUsers { get; set; }
    
    // Vergi numarası
    public string? TaxNumber { get; set; }
    
    // Adres
    public string? Address { get; set; }
    
    // Açıklama
    public string? Description { get; set; }
    
    // E-posta
    public string? Email { get; set; }
    
    // Web sitesi
    public string? Website { get; set; }
    
    // Çalışan sayısı
    public string? EmployeeCount { get; set; }
    
    // Kuruluş tarihi
    public DateTime? EstablishedDate { get; set; }
    
    // Logo yolu
    public string? LogoPath { get; set; }
}
