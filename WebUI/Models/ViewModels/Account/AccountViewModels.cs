using System.ComponentModel.DataAnnotations;
using WebUI.Models.ViewModels.Education;

namespace WebUI.Models.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = "";
        
        [Required(ErrorMessage = "Þifre zorunludur")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
        
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = "";
    }

    public class ResetPasswordViewModel
    {
        public string Token { get; set; } = "";
        public string Email { get; set; } = "";
        
        [Required(ErrorMessage = "Yeni þifre zorunludur")]
        [StringLength(100, ErrorMessage = "Þifre en az {2} karakter olmalýdýr.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";
        
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Þifre ve doðrulama þifresi uyuþmuyor.")]
        public string ConfirmPassword { get; set; } = "";
    }

    public abstract class UniversityRegisterBaseViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [StringLength(100, ErrorMessage = "Ad Soyad 100 karakterden uzun olamaz")]
        public string FullName { get; set; } = "";
        
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = "";
        
        [Required(ErrorMessage = "Üniversite seçimi zorunludur")]
        public string UniversityName { get; set; } = "";
        
        [Required(ErrorMessage = "Þifre zorunludur")]
        [StringLength(100, ErrorMessage = "Þifre en az {2} karakter olmalýdýr.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
        
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Þifre ve doðrulama þifresi uyuþmuyor.")]
        public string ConfirmPassword { get; set; } = "";
        
        public List<UniversityDropdownViewModel> Universities { get; set; } = new();
    }

    public class StudentRegisterViewModel : UniversityRegisterBaseViewModel
    {
        [StringLength(20, ErrorMessage = "Öðrenci numarasý 20 karakterden uzun olamaz")]
        public string? StudentNumber { get; set; }
        
        [StringLength(100, ErrorMessage = "Bölüm adý 100 karakterden uzun olamaz")]
        public string? Department { get; set; }
        
        [Range(1, 8, ErrorMessage = "Sýnýf 1-8 arasýnda olmalýdýr")]
        public int? Grade { get; set; }
    }

    public class AdvisorRegisterViewModel : UniversityRegisterBaseViewModel
    {
        [StringLength(100, ErrorMessage = "Bölüm adý 100 karakterden uzun olamaz")]
        public string? Department { get; set; }
        
        [StringLength(50, ErrorMessage = "Unvan 50 karakterden uzun olamaz")]
        public string? Title { get; set; }
        
        [Phone(ErrorMessage = "Geçerli bir telefon numarasý giriniz")]
        public string? Phone { get; set; }
        
        [StringLength(200, ErrorMessage = "Ofis adresi 200 karakterden uzun olamaz")]
        public string? OfficeAddress { get; set; }
    }

    public class CompanyRegisterViewModel
    {
        [Required(ErrorMessage = "Þirket adý zorunludur")]
        [StringLength(200, ErrorMessage = "Þirket adý 200 karakterden uzun olamaz")]
        public string CompanyName { get; set; } = "";
        
        [Required(ErrorMessage = "Sektör seçimi zorunludur")]
        [StringLength(100, ErrorMessage = "Sektör adý 100 karakterden uzun olamaz")]
        public string Sector { get; set; } = "";
        
        [Phone(ErrorMessage = "Geçerli bir telefon numarasý giriniz")]
        public string? Phone { get; set; }
        
        [Required(ErrorMessage = "Vergi numarasý zorunludur")]
        [StringLength(20, ErrorMessage = "Vergi numarasý 20 karakterden uzun olamaz")]
        public string TaxNumber { get; set; } = "";
        
        [StringLength(300, ErrorMessage = "Adres 300 karakterden uzun olamaz")]
        public string? Address { get; set; }
        
        [Url(ErrorMessage = "Geçerli bir web sitesi adresi giriniz")]
        public string? Website { get; set; }
        
        [StringLength(1000, ErrorMessage = "Açýklama 1000 karakterden uzun olamaz")]
        public string? Description { get; set; }
        
        // Company User Information
        [Required(ErrorMessage = "Yetkili kiþi adý zorunludur")]
        [StringLength(100, ErrorMessage = "Ad Soyad 100 karakterden uzun olamaz")]
        public string CompanyUserFullName { get; set; } = "";
        
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string EmailUser { get; set; } = "";
        
        [Required(ErrorMessage = "Þirket içi rol zorunludur")]
        [StringLength(100, ErrorMessage = "Rol adý 100 karakterden uzun olamaz")]
        public string RoleInCompany { get; set; } = "";
        
        [Required(ErrorMessage = "Þifre zorunludur")]
        [StringLength(100, ErrorMessage = "Þifre en az {2} karakter olmalýdýr.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
        
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Þifre ve doðrulama þifresi uyuþmuyor.")]
        public string ConfirmPassword { get; set; } = "";

        // Missing properties from views
        public List<string> SectorList { get; set; } = new List<string>
        {
            "Teknoloji",
            "Finans",
            "Saðlýk",
            "Eðitim",
            "Ýmalat",
            "Hizmet",
            "Perakende",
            "Ýnþaat",
            "Turizm",
            "Diðer"
        };

        public List<string> RoleList { get; set; } = new List<string>
        {
            "Genel Müdür",
            "Ýnsan Kaynaklarý Müdürü",
            "Departman Müdürü",
            "Proje Müdürü",
            "Uzman",
            "Koordinatör",
            "Diðer"
        };
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut þifre zorunludur")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "Yeni þifre zorunludur")]
        [StringLength(100, ErrorMessage = "Þifre en az {2} karakter olmalýdýr.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Þifre ve doðrulama þifresi uyuþmuyor.")]
        public string ConfirmNewPassword { get; set; } = "";
    }
}