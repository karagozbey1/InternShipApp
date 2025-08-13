using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.Account
{
    public class CompanyRegisterViewModel
    {
        [Required(ErrorMessage = "Firma adı zorunludur.")]
        [Display(Name = "Firma Adı")]
        public string CompanyFullName { get; set; }=null!;


        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; } = null!;
     
        [Required(ErrorMessage = "Vergi Numarası girilmesi zorunludur.")]
        public string TaxNumber { get; set; } = null!;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}