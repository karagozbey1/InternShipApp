using System.ComponentModel.DataAnnotations;
using Core.Entity;

namespace WebUI.Models.ViewModels.Student
{
    // Onay formu listesi ViewModels - staj onay formlarýnýn topluca gösterilmesi için
    // Dashboard ve listeleme sayfalarýnda kullanýlýr
    public class ApprovalFormListViewModel
    {
        // Ana form listesi - filtrelenmiþ ve sayfalanmýþ onay formlarý
        public List<ApprovalFormViewModel> ApprovalForms { get; set; } = new();
        
        // Aktif durum filtresi - hangi duruma göre filtreleme yapýldýðý
        // null ise tüm durumlar gösterilir
        public Core.Entity.InternshipApprovalStatus? StatusFilter { get; set; }
        
        // Sayfalama ve istatistik bilgileri
        public int TotalCount { get; set; }        // Toplam form sayýsý
        public int PendingCount { get; set; }      // Bekleyen form sayýsý  
        public int ApprovedCount { get; set; }     // Onaylanmýþ form sayýsý
        public int RejectedCount { get; set; }     // Reddedilmiþ form sayýsý
        
        // Arama terimi - form içeriklerinde arama yapmak için
        public string SearchTerm { get; set; } = "";
    }

    // Staj onayý düzenleme sayfasý için ViewModel
    // Danýþman tarafýndan onay formlarýný deðerlendirmek ve düzenlemek için kullanýlýr
    // Form durumu deðiþikliði, yorum ekleme ve belge yönetimi fonksiyonlarý içerir
    public class ApprovalEditViewModel
    {
        // Onay formu ID'si - düzenlenecek formun benzersiz tanýmlayýcýsý
        // Primary key, veritabaný referansý için kullanýlýr
        public int Id { get; set; }

        // Danýþman deðerlendirme notu - öðrenci performansý ve staj süreci hakkýnda
        // Required field - danýþman mutlaka deðerlendirme yapmalý
        // Max 2000 karakter ile detaylý deðerlendirme imkaný saðlanýr
        [Required(ErrorMessage = "Danýþman notu zorunludur")]
        [StringLength(2000, ErrorMessage = "Danýþman notu en fazla 2000 karakter olabilir")]
        public string StudentNote { get; set; } = "";

        // Danýþman onay kararý - formun nihai durumunu belirler
        // Enum deðeri string olarak saklanýr (Approved, Rejected, Pending)
        // UI'da dropdown veya radio button olarak gösterilebilir
        [Required(ErrorMessage = "Onay durumu seçimi zorunludur")]
        public string AdvisorApprovalStatus { get; set; } = "";

        // Ýliþkili öðrenci bilgileri - read-only display için
        // Navigation property yerine embedded ViewModel kullanýlýr
        public StudentInfoViewModel Student { get; set; } = new();

        // Forma eklenen belgeler listesi - CV, transkript, staj defteri vb.
        // Danýþmanýn belgeleri incelemesi için gerekli
        public List<ApprovalDocumentViewModel> Documents { get; set; } = new();

        // Onay formuna dahil edilecek belge ID'leri
        // Checkbox list'ten seçilenler buraya eklenir
        public List<int> SelectedDocumentIds { get; set; } = new();

        // Staj þirketi ve pozisyon bilgileri - onay için kritik context
        public string CompanyName { get; set; } = "";    // Staj yapýlan þirket adý
        public string Position { get; set; } = "";       // Staj pozisyon baþlýðý

        // Staj dönem bilgileri - akademik takvim uygunluðu kontrolü için
        public DateTime StartDate { get; set; }          // Staj baþlangýç tarihi
        public DateTime EndDate { get; set; }            // Staj bitiþ tarihi
        public int WeeklyWorkHours { get; set; }         // Haftalýk çalýþma saat miktarý

        // Staj detay açýklamasý - yapýlacak iþlerin tanýmý
        // Danýþmanýn staj kalitesini deðerlendirmesi için önemli
        public string? InternshipDescription { get; set; }

        // Form mevcut durumu - workflow tracking için
        // Display-only, kullanýcý bu alaný deðiþtiremez
        public string Status { get; set; } = "";

        // Edit permission kontrolü - formun düzenlenebilir olup olmadýðý
        // Business rules'a göre belirlenir (pending formlar düzenlenebilir)
        public bool CanEdit { get; set; }

        // Revizyon gerekmesi durumunda açýklama notu
        // Öðrencinin neleri düzeltmesi gerektiðini belirtir
        public string? RevisionNote { get; set; }
    }

    // Öðrenci bilgileri display ViewModel'i
    // Onay formlarýnda öðrenci context'i saðlamak için
    // User entity'sinden sadece gerekli alanlarý içerir
    public class StudentInfoViewModel
    {
        public int Id { get; set; }                    // Öðrenci benzersiz ID'si
        public string FullName { get; set; } = "";    // Tam ad (Ad Soyad)
        public string StudentNumber { get; set; } = "";// Üniversite öðrenci numarasý
        public string Email { get; set; } = "";       // Ýletiþim email adresi
        public string? Department { get; set; }       // Akademik bölüm/department
        public string? University { get; set; }       // Üniversite adý
    }

    // Onay belgesi görüntüleme ViewModel'i
    // Onay formlarýnda kullanýlan belgelerin detaylarýný göstermek için
    // InternshipDocument entity'sinden UI için gerekli alanlarý içerir
    public class ApprovalDocumentViewModel
    {
        public int Id { get; set; }                      // Belge benzersiz ID'si
        public string Title { get; set; } = "";         // Belge baþlýðý/adý
        public string FileName { get; set; } = "";      // Dosya sistem adý
        public string FilePath { get; set; } = "";      // Storage'daki dosya yolu
        public long FileSize { get; set; }              // Byte cinsinden dosya boyutu
        public string DocumentType { get; set; } = "";  // Belge kategorisi (CV, Transcript vb.)
        public DateTime UploadedAt { get; set; }        // Yüklenme zamaný
    }

    // Direkt onay formu ViewModel'i - baþvuru olmadan onay formu oluþturma
    // Öðrencinin henüz baþvuru yapmadýðý durumlarda kullanýlýr
    // University approval süreçleri için tasarlanmýþ
    public class DirectApprovalFormViewModel
    {
        // Zorunlu þirket bilgileri - staj yeri identificiation için
        [Required(ErrorMessage = "Þirket adý zorunludur")]
        public string CompanyName { get; set; } = "";
        
        // Staj tarihleri - akademik takvim uygunluðu kontrolü için
        [Required(ErrorMessage = "Baþlangýç tarihi zorunludur")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "Bitiþ tarihi zorunludur")]
        public DateTime EndDate { get; set; }
        
        // Çalýþma saati validasyonu - iþ kanunu ve akademik kurallar uygunluðu
        [Required(ErrorMessage = "Haftalýk çalýþma saati zorunludur")]
        [Range(1, 40, ErrorMessage = "Haftalýk çalýþma saati 1-40 saat arasýnda olmalýdýr")]
        public int WeeklyWorkHours { get; set; }
        
        // Opsiyonel þirket detay bilgileri - iletiþim ve koordinasyon için
        public string? WorkplaceAddress { get; set; }     // Ýþ yeri adresi
        public string? InternshipCoordinator { get; set; }// Staj koordinatörü adý
        public string? CoordinatorPhone { get; set; }     // Koordinatör telefon
        public string? CoordinatorEmail { get; set; }     // Koordinatör email
        public string? InternshipDescription { get; set; }// Staj iþ tanýmý
        public string? StudentNote { get; set; }          // Öðrenci ek notlarý
        
        // Belge seçimi için listeleme ve seçim alanlarý
        public List<StudentDocumentVM> StudentDocuments { get; set; } = new();
        public List<int> SelectedDocumentIds { get; set; } = new();
    }

    // Modern UI onay ViewModel'i - kullanýcý dostu arayüz için
    // Step-by-step wizard interface desteði ile kapsamlý onay süreci
    public class ModernApprovalViewModel
    {
        // Temel form bilgileri
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeeklyWorkHours { get; set; }
        
        // Þirket iletiþim bilgileri
        public string? WorkplaceAddress { get; set; }
        public string? InternshipCoordinator { get; set; }
        public string? CoordinatorPhone { get; set; }
        public string? CoordinatorEmail { get; set; }
        
        // Açýklama ve not alanlarý
        public string? InternshipDescription { get; set; }
        public string? StudentNote { get; set; }
        public string? AdvisorNote { get; set; }
        public string? RejectionReason { get; set; }
        
        // Durum ve zaman bilgileri - workflow tracking
        public Core.Entity.InternshipApprovalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }       // Onaylanma zamaný
        public DateTime? RejectedAt { get; set; }       // Reddedilme zamaný
        public string? AdvisorName { get; set; }        // Onaylayan danýþman
        
        // UI display helpers - status görüntüleme için
        public string StatusText { get; set; } = "";   // "Onaylandý", "Bekliyor" vb.
        public string StatusColor { get; set; } = "";  // CSS class/color
        
        // Multi-step wizard için veri koleksiyonlarý
        public List<StudentDocumentVM> StudentDocuments { get; set; } = new();        // Step 1: Belge seçimi
        public List<int> SelectedDocumentIds { get; set; } = new();
        public List<ApprovedApplicationVM> ApprovedApplications { get; set; } = new(); // Step 2: Baþvuru seçimi

        // Review step için populated data - wizard'ýn son adýmýnda gösterilecek özet
        public List<DocumentVM> SelectedDocuments { get; set; } = new();              // Seçilen belgeler
        public ApprovedApplicationVM? SelectedCompany { get; set; }                   // Seçilen þirket/baþvuru
        public AdvisorVM? SelectedAdvisor { get; set; }                               // Seçilen danýþman
        
        // Wizard navigation için ID'ler
        public int? SelectedApplicationId { get; set; }
        public int? SelectedAdvisorId { get; set; }
    }

    // Onaylanmýþ baþvuru bilgileri ViewModel'i
    // Step 2'de þirket seçimi için kullanýlýr
    public class ApprovedApplicationVM
    {
        public int Id { get; set; }                        // OnayForm ID'si
        public int ApplicationId { get; set; }             // Ýliþkili baþvuru ID'si
        public string CompanyName { get; set; } = "";     // Þirket adý
        public string Position { get; set; } = "";        // Staj pozisyonu
        public DateTime StartDate { get; set; }           // Staj baþlangýcý
        public DateTime EndDate { get; set; }             // Staj bitiþi
        public string? Description { get; set; }          // Staj açýklamasý
        public DateTime ApprovedDate { get; set; }        // Onaylanma tarihi
        public string Status { get; set; } = "";          // Mevcut durum
        public DateTime CreatedAt { get; set; }           // Oluþturulma zamaný
    }

    // Danýþman bilgileri ViewModel'i
    // Step 3'te danýþman seçimi için kullanýlýr
    public class AdvisorVM
    {
        public int Id { get; set; }                      // Danýþman ID'si
        public string FullName { get; set; } = "";      // Tam adý
        public string Email { get; set; } = "";         // Email adresi
        public string Department { get; set; } = "";    // Akademik bölüm
        public string University { get; set; } = "";    // Üniversite
    }

    // Ana onay formu display ViewModel'i
    // Listeleme ve detay görüntüleme sayfalarýnda kullanýlýr
    // Tüm gerekli bilgileri tek bir yerde toplayan comprehensive model
    public class ApprovalFormViewModel
    {
        // Temel form identifiers
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public string Position { get; set; } = "";
        
        // Tarih bilgileri
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Danýþman bilgileri
        public string AdvisorName { get; set; } = "";
        public string? AdvisorDepartment { get; set; }    // "Danýþman - Bölüm" formatýnda gösterim için
        
        // Durum ve zaman tracking
        public Core.Entity.InternshipApprovalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Ýçerik alanlarý
        public string StudentNote { get; set; } = "";
        public string AdvisorComment { get; set; } = "";
        public List<DocumentVM> Documents { get; set; } = new();
        
        // View'lar tarafýndan kullanýlan ek özellikler
        public string? CoordinatorPhone { get; set; }
        public string? CoordinatorEmail { get; set; }
        public string StatusColor { get; set; } = "";        // CSS styling için
        public string StatusText { get; set; } = "";         // Human-readable status
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public Core.Entity.InternshipApprovalStatus StatusEnum { get; set; }
        public string? OfferTitle { get; set; }
        public int? WeeklyWorkHours { get; set; }
        public string? InternshipCoordinator { get; set; }
        public string? InternshipDescription { get; set; }
        public string? AdvisorNote { get; set; }
        public string? RejectionReason { get; set; }
        public string? WorkplaceAddress { get; set; }

        // Modern detay view'da kullanýlan öðrenci display alanlarý
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public string? StudentDepartment { get; set; }
        public string? StudentUniversity { get; set; }
        public int? StudentGrade { get; set; }

        // Step 3'ten gelen danýþman iletiþim bilgileri
        public string? AdvisorEmail { get; set; }
        public string? AdvisorPhone { get; set; }
        
        // Revizyon özellikleri - form düzeltme süreçleri için
        public string? RevisionNote { get; set; }         // Düzeltme talebi açýklamasý
        public DateTime? RevisedAt { get; set; }          // Son düzeltme zamaný
    }

    // Belge display ViewModel'i - document listing için
    // Hem onay formlarýnda hem de genel belge yönetiminde kullanýlýr
    public class DocumentVM
    {
        public int Id { get; set; }                      // Belge ID'si
        public string Title { get; set; } = "";         // Belge baþlýðý
        public string FileName { get; set; } = "";      // Dosya adý
        public string DocumentType { get; set; } = "";  // Belge tipi
        public long FileSize { get; set; }              // Dosya boyutu (bytes)
        public DateTime UploadedAt { get; set; }        // Upload zamaný
        public string FilePath { get; set; } = "";      // Storage path
    }

    // Onay formu oluþturma ViewModel'i
    // Create sayfasýnda form submission için kullanýlýr
    public class CreateApprovalFormViewModel
    {
        // Zorunlu seçimler - kullanýcý mutlaka bu alanlarý doldurmalý
        [Required(ErrorMessage = "Baþvuru seçimi zorunludur")]
        public int ApplicationId { get; set; }
        
        [Required(ErrorMessage = "Danýþman seçimi zorunludur")]
        public int AdvisorId { get; set; }
        
        [Required(ErrorMessage = "Belge seçimi zorunludur")]
        public List<int> SelectedDocumentIds { get; set; } = new();
        
        // Opsiyonel öðrenci notu
        [StringLength(1000, ErrorMessage = "Not 1000 karakterden uzun olamaz")]
        public string StudentNote { get; set; } = "";
        
        // Dropdown/selection için data koleksiyonlarý
        public List<InternshipApplication> AvailableApplications { get; set; } = new();
        public List<InternshipDocument> AvailableDocuments { get; set; } = new();
        public List<User> AvailableAdvisors { get; set; } = new();
        
        // View'lar tarafýndan gereken ek özellikler
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeeklyWorkHours { get; set; }
        public string? InternshipDescription { get; set; }
        public string CompanyName { get; set; } = "";
        public string? WorkplaceAddress { get; set; }
        public string? InternshipCoordinator { get; set; }
        public string? CoordinatorPhone { get; set; }
        public string? CoordinatorEmail { get; set; }
        
        // Edit.cshtml view için
        public string ApplicationTitle { get; set; } = "";
    }

    // Üniversite onay formu ViewModel'i
    // University-specific approval process için
    // Akademik kurumlarýn kendi onay süreçleri için özelleþtirilmiþ
    public class UniversityApprovalFormViewModel
    {
        public int Id { get; set; }
        
        // Þirket bilgileri - validation ile zorunlu alanlar
        [Required(ErrorMessage = "Þirket adý zorunludur")]
        [StringLength(200, ErrorMessage = "Þirket adý 200 karakterden uzun olamaz")]
        public string CompanyName { get; set; } = "";
        
        // Staj tarihleri - academic calendar compliance için
        [Required(ErrorMessage = "Staj baþlangýç tarihi zorunludur")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "Staj bitiþ tarihi zorunludur")]
        public DateTime EndDate { get; set; }
        
        // Çalýþma saati kontrolü - labor law compliance
        [Range(1, 60, ErrorMessage = "Haftalýk çalýþma saati 1-60 arasýnda olmalýdýr")]
        public int WeeklyWorkHours { get; set; }
        
        // Þirket detay bilgileri - all optional with length constraints
        [StringLength(500, ErrorMessage = "Ýþ yeri adresi 500 karakterden uzun olamaz")]
        public string? WorkplaceAddress { get; set; }
        
        [StringLength(100, ErrorMessage = "Koordinatör adý 100 karakterden uzun olamaz")]
        public string? InternshipCoordinator { get; set; }
        
        [StringLength(20, ErrorMessage = "Telefon numarasý 20 karakterden uzun olamaz")]
        public string? CoordinatorPhone { get; set; }
        
        [StringLength(100, ErrorMessage = "E-posta adresi 100 karakterden uzun olamaz")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string? CoordinatorEmail { get; set; }
        
        // Ýçerik alanlarý
        [StringLength(1000, ErrorMessage = "Staj açýklamasý 1000 karakterden uzun olamaz")]
        public string? InternshipDescription { get; set; }
        
        [StringLength(1000, ErrorMessage = "Öðrenci notu 1000 karakterden uzun olamaz")]
        public string? StudentNote { get; set; }
        
        // Approval workflow fields
        public string? AdvisorNote { get; set; }
        public string? RejectionReason { get; set; }
        public Core.Entity.InternshipApprovalStatus Status { get; set; }
        
        // Timestamp tracking
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? AdvisorName { get; set; }
        
        // UI helpers
        public string StatusText { get; set; } = "";
        public string StatusColor { get; set; } = "";
        
        // University-specific fields
        public string? AdditionalNotes { get; set; }      // Ek akademik notlar
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public int UniversityId { get; set; }
        public string UniversityName { get; set; } = "";
        
        // UniversityApproval.cshtml view için özel alanlar
        public string Subject { get; set; } = "";         // Onay konusu
        public string RequestType { get; set; } = "";     // Talep türü
        public string Priority { get; set; } = "";        // Öncelik seviyesi
        public string ApplicationDetails { get; set; } = "";// Baþvuru detaylarý
    }
}