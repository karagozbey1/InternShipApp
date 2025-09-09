using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Interfaces.UserRepose;
using WebUI.Models.ViewModels.Advisor;
using WebUI.Models.ViewModels.Account;

namespace WebUI.Controllers.Advisor
{
    [Authorize(Roles = "Advisor")]
    public class AdvisorProfileController : Controller
    {
        private readonly IIUserService _userService;

        public AdvisorProfileController(IIUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
                
                if (advisor == null)
                {
                    TempData["ErrorMessage"] = "Danýþman bilgileri bulunamadý.";
                    return RedirectToAction("Index", "AdvisorDashboard");
                }

                var viewModel = new AdvisorProfileViewModel
                {
                    Id = advisor.Id,
                    FullName = advisor.FullName ?? "",
                    Email = advisor.Email ?? "",
                    Phone = advisor.Phone ?? "",
                    UniversityName = advisor.University?.Name ?? "",
                    Department = advisor.Department ?? "",
                    Title = advisor.Title ?? ""
                };

                return View("~/Views/Advisor/Profile/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Profil bilgileri yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction("Index", "AdvisorDashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AdvisorProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("~/Views/Advisor/Profile/Index.cshtml", model);
                }

                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserByIdAsync(advisorId);
                
                if (advisor == null)
                {
                    TempData["ErrorMessage"] = "Danýþman bulunamadý.";
                    return View("~/Views/Advisor/Profile/Index.cshtml", model);
                }

                // Update all advisor information
                advisor.FullName = model.FullName;
                advisor.Phone = model.Phone;
                advisor.Department = model.Department;
                advisor.Title = model.Title;

                await _userService.UpdateUserAsync(advisor);
                
                TempData["SuccessMessage"] = "Profil baþarýyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Profil güncellenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Advisor/Profile/Index.cshtml", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // ModelState hatalarýný TempData'ya ekle
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    TempData["ErrorMessage"] = string.Join(", ", errors);
                    return RedirectToAction(nameof(Index));
                }

                var userId = GetCurrentUserId();
                var result = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
                
                if (result)
                {
                    TempData["SuccessMessage"] = "Þifre baþarýyla deðiþtirildi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Þifre deðiþtirilemedi. Mevcut þifrenizi kontrol edin.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Þifre deðiþtirme sýrasýnda hata oluþtu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}