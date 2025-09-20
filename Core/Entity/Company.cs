using Core.Entity;

namespace Core.Entity;

/// <summary>
/// Şirket entity'si
/// Staj pozisyonları sunan kurumların bilgilerini temsil eder
/// İş ilanı veren şirketlerin profil ve iletişim bilgilerini içerir
/// </summary>
public class Company
{
    /// <summary>
    /// Şirketin benzersiz tanımlayıcısı
    /// Primary key olarak kullanılır
    /// </summary>
    public int CompanyId { get; set; }

    /// <summary>
    /// Şirketin resmi adı
    /// İş ilanlarında ve profilde görünecek şirket ismi
    /// Zorunlu alan
    /// </summary>
    public string CompanyName { get; set; } = null!;
    
    /// <summary>
    /// Şirketin faaliyet gösterdiği sektör
    /// Teknoloji, Sağlık, Eğitim, Finans vb.
    /// Öğrencilerin sektör bazlı arama yapabilmesi için
    /// </summary>
    public string? Sector { get; set; }
   
    /// <summary>
    /// Şirketin iletişim telefon numarası
    /// Acil durumlarda ve koordinasyon için kullanılır
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Şirketin sistem tarafından onaylanıp onaylanmadığı
    /// True ise şirket iş ilanı verebilir
    /// False ise beklemede veya reddedilmiş
    /// </summary>
    public bool Approved { get; set; }

    /// <summary>
    /// Şirketin verdiği staj teklifleri koleksiyonu
    /// Navigation property - tüm iş ilanları listesi
    /// </summary>
    public ICollection<InternshipOffer>? InternshipOffers { get; set; }
    
    /// <summary>
    /// Şirket ile ilişkili kullanıcılar koleksiyonu
    /// İK temsilcileri, staj koordinatörleri vb.
    /// Navigation property
    /// </summary>
    public ICollection<CompanyUser>? CompanyUsers { get; set; }
    
    /// <summary>
    /// Şirketin vergi numarası
    /// Resmi kayıt kontrolü ve faturalandırma için
    /// Opsiyonel bilgi
    /// </summary>
    public string? TaxNumber { get; set; }
    
    /// <summary>
    /// Şirketin fiziksel adresi
    /// Stajyerlerin nereye gideceğini bilmesi için
    /// Detaylı adres bilgisi
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// Şirket hakkında genel açıklama
    /// Şirketin faaliyet alanı, misyonu, vizyonu
    /// Öğrencilerin şirket kültürünü anlaması için
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Şirketin resmi e-posta adresi
    /// Genel iletişim için kullanılan kurumsal e-posta
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Şirketin web sitesi URL'i
    /// Öğrencilerin şirket hakkında daha fazla bilgi alması için
    /// Opsiyonel bilgi
    /// </summary>
    public string? Website { get; set; }
    
    /// <summary>
    /// Şirketin çalışan sayısı
    /// String olarak saklanır (örn: "50-100", "500+")
    /// Şirket büyüklüğünü belirtir
    /// </summary>
    public string? EmployeeCount { get; set; }
    
    /// <summary>
    /// Şirketin kuruluş tarihi
    /// Şirketin deneyimini ve geçmişini gösterir
    /// Opsiyonel bilgi
    /// </summary>
    public DateTime? EstablishedDate { get; set; }
    
    /// <summary>
    /// Şirket logosunun dosya yolu
    /// Profil sayfasında ve ilanlarda görüntülenir
    /// Web server'daki logo dosyasının relatif yolu
    /// </summary>
    public string? LogoPath { get; set; }
}
