using System.ComponentModel.DataAnnotations;
using WebUI.Models.ViewModels.Education;

namespace WebUI.Models.ViewModels.Account
{
    /// <summary>
    /// Kullanýcý giriþ iþlemi için ViewModel
    /// Login sayfasýndan gelen form verilerini taþýr
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Kullanýcýnýn e-posta adresi - giriþ için zorunlu alan
        /// </summary>
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = "";
        
        /// <summary>
        /// Kullanýcýnýn þifresi - giriþ için zorunlu alan
        /// </summary>
        [Required(ErrorMessage = "Þifre zorunludur")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
        
        /// <summary>
        /// Kullanýcýnýn "Beni Hatýrla" seçeneði
        /// True ise kullanýcý oturumu uzun süre açýk kalýr
        /// </summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Þifre unutma iþlemi için ViewModel
    /// Kullanýcýnýn e-posta adresini alýr, þifre sýfýrlama linki gönderir
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// Þifre sýfýrlama linki gönderilecek e-posta adresi
        /// </summary>
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = "";
    }

    /// <summary>
    /// Þifre sýfýrlama iþlemi için ViewModel
    /// E-posta ile gelen link üzerinden yeni þifre belirleme
    /// </summary>
    public class ResetPasswordViewModel
    {
        /// <summary>
        /// E-posta ile gelen þifre sýfýrlama token'ý
        /// Güvenlik için gerekli, deðiþtirilmemeli
        /// </summary>
        public string Token { get; set; } = "";
        
        /// <summary>
        /// Þifresi sýfýrlanacak kullanýcýnýn e-posta adresi
        /// Token ile birlikte doðrulama için kullanýlýr
        /// </summary>
        public string Email { get; set; } = "";
        
        /// <summary>
        /// Kullanýcýnýn belirleyeceði yeni þifre
        /// Minimum 6 karakter olmalý
        /// </summary>
        [Required(ErrorMessage = "Yeni þifre zorunludur")]
        [StringLength(100, ErrorMessage = "Þifre en az {2} karakter olmalýdýr.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";
        
        /// <summary>
        /// Yeni þifrenin tekrarý - doðrulama amaçlý
        /// NewPassword ile birebir ayný olmalý
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Þifre ve doðrulama þifresi uyuþmuyor.")]
        public string ConfirmPassword { get; set; } = "";
    }

    /// <summary>
    /// Üniversite kayýt iþlemleri için temel ViewModel (abstract sýnýf)
    /// Öðrenci ve danýþman kayýt formlarý bu sýnýftan türetilir
    /// Ortak alanlarý içerir (Ad Soyad, E-posta, Üniversite, Þifre)
    /// </summary>
    public abstract class UniversityRegisterBaseViewModel
    {
        /// <summary>
        /// Kullanýcýnýn ad ve soyadý
        /// Kayýt sýrasýnda profilde görünecek isim
        /// </summary>
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [StringLength(100, ErrorMessage = "Ad Soyad 100 karakterden uzun olamaz")]
        public string FullName { get; set; } = "";
        
        /// <summary>
        /// Kullanýcýnýn e-posta adresi
        /// Giriþ yaparken kullanýlacak, benzersiz olmalý
        /// </summary>
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = "";
        
        /// <summary>
        /// Kullanýcýnýn baðlý olduðu üniversite adý
        /// Dropdown'dan seçilecek, validation için gerekli
        /// </summary>
        [Required(ErrorMessage = "Üniversite seçimi zorunludur")]
        public string UniversityName { get; set; } = "";
        
        /// <summary>
        /// Kullanýcýnýn hesap þifresi
        /// Minimum 6 karakter, güvenlik için þifrelenir
        /// </summary>
        [Required(ErrorMessage = "Þifre zorunludur")]
        [StringLength(100, ErrorMessage = "Þifre en az {2} karakter olmalýdýr.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
        
        /// <summary>
        /// Þifre doðrulama alaný
        /// Password ile birebir ayný olmalý, yanlýþ yazým kontrolü
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Þifre ve doðrulama þifresi uyuþmuyor.")]
        public string ConfirmPassword { get; set; } = "";

        /// <summary>
        /// Üniversite dropdown listesi için
        /// Controller'da doldurulacak üniversite listesi
        /// Education namespace'indeki UniversityDropdownViewModel kullanýlýr
        /// </summary>
        public List<UniversityDropdownViewModel> Universities { get; set; } = new();
    }

    /// <summary>
    /// Öðrenci kayýt iþlemi için ViewModel
    /// UniversityRegisterBaseViewModel'den türetilir
    /// Öðrenciye özel ek alanlar içerir
    /// </summary>
    public class StudentRegisterViewModel : UniversityRegisterBaseViewModel
    {
        /// <summary>
        /// Öðrenci numarasý - üniversite tarafýndan verilen benzersiz numara
        /// Kayýt sýrasýnda manuel girilir veya otomatik oluþturulur
        /// </summary>
        [Required(ErrorMessage = "Öðrenci numarasý zorunludur")]
        [StringLength(20, ErrorMessage = "Öðrenci numarasý 20 karakterden uzun olamaz")]
        public string StudentNumber { get; set; } = "";
        
        /// <summary>
        /// Öðrencinin kayýtlý olduðu bölüm/program adý
        /// Staj alaný belirlemek için önemli bilgi
        /// </summary>
        [Required(ErrorMessage = "Bölüm seçimi zorunludur")]
        public string Department { get; set; } = "";
        
        /// <summary>
        /// Öðrencinin hangi sýnýfta/yarýyýlda olduðu bilgisi
        /// Staj zorunluluðu kontrolü için kullanýlýr
        /// </summary>
        public int? CurrentYear { get; set; }
        
        /// <summary>
        /// Öðrencinin telefon numarasý
        /// Ýletiþim için opsiyonel alan
        /// </summary>
        [Phone(ErrorMessage = "Geçerli bir telefon numarasý giriniz")]
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Danýþman kayýt iþlemi için ViewModel
    /// UniversityRegisterBaseViewModel'den türetilir
    /// Akademik personele özel alanlar içerir
    /// </summary>
    public class AdvisorRegisterViewModel : UniversityRegisterBaseViewModel
    {
        /// <summary>
        /// Danýþmanýn akademik unvaný (Dr., Prof. vb.)
        /// Profil sayfasýnda görünecek unvan bilgisi
        /// </summary>
        [Required(ErrorMessage = "Unvan seçimi zorunludur")]
        public string Title { get; set; } = "";
        
        /// <summary>
        /// Danýþmanýn çalýþtýðý bölüm/fakülte
        /// Hangi alanda uzman olduðunu belirtir
        /// </summary>
        [Required(ErrorMessage = "Bölüm seçimi zorunludur")]
        public string Department { get; set; } = "";
        
        /// <summary>
        /// Danýþmanýn uzmanlýk alanlarý
        /// Virgül ile ayrýlmýþ liste halinde (örn: "Yazýlým, Veri Bilimi")
        /// </summary>
        public string? SpecializationAreas { get; set; }
        
        /// <summary>
        /// Danýþmanýn ofis numarasý veya konumu
        /// Öðrencilerle yüz yüze görüþme için gerekli
        /// </summary>
        public string? OfficeLocation { get; set; }
        
        /// <summary>
        /// Danýþmanýn telefon numarasý
        /// Acil durumlarda iletiþim için
        /// </summary>
        [Phone(ErrorMessage = "Geçerli bir telefon numarasý giriniz")]
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Þirket kayýt iþlemi için ViewModel
    /// Þirket temsilcilerinin sisteme kayýt olmasý için kullanýlýr
    /// </summary>
    public class CompanyRegisterViewModel
    {
        /// <summary>
        /// Þirket temsilcisinin ad soyadý
        /// ÝK veya staj koordinatörü bilgisi
        /// </summary>
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [StringLength(100, ErrorMessage = "Ad Soyad 100 karakterden uzun olamaz")]
        public string FullName { get; set; } = "";
        
        /// <summary>
        /// Þirket temsilcisinin ad soyadý (alternatif alan adý)
        /// View'larda CompanyUserFullName olarak kullanýlýr
        /// </summary>
        [Required(ErrorMessage = "Yetkili ad soyad zorunludur")]
        public string CompanyUserFullName { get; set; } = "";
        
        /// <summary>
        /// Þirket temsilcisinin e-posta adresi
        /// Giriþ yaparken kullanýlacak, þirket e-postasý olmalý
        /// </summary>
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = "";

        /// <summary>
        /// Yetkili e-posta adresi (alternatif alan adý)
        /// View'larda EmailUser olarak kullanýlýr
        /// </summary>
        [Required(ErrorMessage = "Yetkili e-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string EmailUser { get; set; } = "";
        
        /// <summary>
        /// Þirketin resmi adý
        /// Staj ilanlarýnda görünecek þirket ismi
        /// </summary>
        [Required(ErrorMessage = "Þirket adý zorunludur")]
        [StringLength(200, ErrorMessage = "Þirket adý 200 karakterden uzun olamaz")]
        public string CompanyName { get; set; } = "";
        
        /// <summary>
        /// Þirketin faaliyet alaný/sektörü
        /// Öðrencilerin uygun staj arayabilmesi için
        /// </summary>
        [Required(ErrorMessage = "Sektör seçimi zorunludur")]
        public string Industry { get; set; } = "";

        /// <summary>
        /// Þirket sektörü (alternatif alan adý)
        /// View'larda Sector olarak kullanýlýr
        /// </summary>
        [Required(ErrorMessage = "Sektör seçimi zorunludur")]
        public string Sector { get; set; } = "";
        
        /// <summary>
        /// Þirketin bulunduðu þehir
        /// Staj konumu belirlemek için kritik bilgi
        /// </summary>
        [Required(ErrorMessage = "Þehir seçimi zorunludur")]
        public string City { get; set; } = "";
        
        /// <summary>
        /// Þirketin detaylý adresi
        /// Stajyer yönlendirmesi ve iletiþim için
        /// </summary>
        public string? Address { get; set; }
        
        /// <summary>
        /// Þirketin telefon numarasý
        /// Ýletiþim ve doðrulama için
        /// </summary>
        [Required(ErrorMessage = "Telefon numarasý zorunludur")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarasý giriniz")]
        public string PhoneNumber { get; set; } = "";

        /// <summary>
        /// Þirket telefonu (alternatif alan adý)
        /// View'larda Phone olarak kullanýlýr
        /// </summary>
        [Phone(ErrorMessage = "Geçerli bir telefon numarasý giriniz")]
        public string? Phone { get; set; }

        /// <summary>
        /// Þirket vergi numarasý
        /// Resmi kayýt kontrolü için
        /// </summary>
        public string? TaxNumber { get; set; }

        /// <summary>
        /// Yetkili kiþinin þirketteki pozisyonu
        /// ÝK Müdürü, Staj Koordinatörü vb.
        /// </summary>
        public string? RoleInCompany { get; set; }
        
        /// <summary>
        /// Þirket web sitesi URL'i
        /// Opsiyonel, þirket hakkýnda ek bilgi için
        /// </summary>
        [Url(ErrorMessage = "Geçerli bir web sitesi adresi giriniz")]
        public string? Website { get; set; }
        
        /// <summary>
        /// Hesap þifresi
        /// Minimum 6 karakter, güvenlik için þifrelenir
        /// </summary>
        [Required(ErrorMessage = "Þifre zorunludur")]
        [StringLength(100, ErrorMessage = "Þifre en az {2} karakter olmalýdýr.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
        
        /// <summary>
        /// Þifre doðrulama alaný
        /// Password ile birebir ayný olmalý
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Þifre ve doðrulama þifresi uyuþmuyor.")]
        public string ConfirmPassword { get; set; } = "";
    }

    /// <summary>
    /// Þifre deðiþtirme iþlemi için ViewModel
    /// Giriþ yapmýþ kullanýcýlarýn mevcut þifrelerini deðiþtirmesi için
    /// </summary>
    public class ChangePasswordViewModel
    {
        /// <summary>
        /// Kullanýcýnýn mevcut þifresi
        /// Deðiþiklik yapabilmek için doðrulama amaçlý
        /// </summary>
        [Required(ErrorMessage = "Mevcut þifre zorunludur")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = "";
        
        /// <summary>
        /// Kullanýcýnýn belirlediði yeni þifre
        /// Mevcut þifreden farklý olmalý
        /// </summary>
        [Required(ErrorMessage = "Yeni þifre zorunludur")]
        [StringLength(100, ErrorMessage = "Þifre en az {2} karakter olmalýdýr.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";
        
        /// <summary>
        /// Yeni þifre doðrulama alaný
        /// NewPassword ile birebir ayný olmalý
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Yeni þifre ve doðrulama þifresi uyuþmuyor.")]
        public string ConfirmNewPassword { get; set; } = "";
    }
}