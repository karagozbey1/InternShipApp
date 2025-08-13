
using Core.Entity;

namespace WebUI.Models.ViewModels.Student
{
    public class StudentProfileViewModel
    {
        public int Id { get; set; }
        public string Fullname { get; set; } = null!;
   
        public string Email { get; set; }=null!;
        // Gerekli diğer profil bilgileri buraya eklenebilir.
    }
}