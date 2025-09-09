using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Controllers;
using WebUI.Models.ViewModels.Student.ViewModels;

namespace WebUI.Controllers.Student
{
    [Authorize]
    public class HelpController : BaseAppController
    {
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new HelpViewModel
            {
                FaqItems = new List<FaqItem>
                {
                    new() { Question = "Staj baþvurusu nasýl yapýlýr?", Answer = "Baþvurularým sayfasýndaki 'Yeni Baþvuru' butonunu kullanýn." },
                    new() { Question = "Belgeler nasýl yüklenir?", Answer = "Belgelerim sekmesinden PDF veya resim formatýnda yükleyebilirsiniz." },
                    new() { Question = "Staj defteri nedir?", Answer = "Günlük çalýþma ve öðrenimlerinizi kaydettiðiniz bölümdür." },
                    new() { Question = "Þifremi unuttum ne yapmalýyým?", Answer = "Giriþ sayfasýndan 'Þifremi Unuttum' baðlantýsýný kullanýn." }
                }
            };
            return View("~/Views/Student/Help.cshtml", vm);
        }
    }
}
