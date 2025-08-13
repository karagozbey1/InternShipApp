using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.Student
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string CurrentPassword { get; set; }=null!;

        [Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; }=null!;

        [Required(ErrorMessage = "Yeni şifre tekrar zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmNewPassword { get; set; }=null!;
    }
}
