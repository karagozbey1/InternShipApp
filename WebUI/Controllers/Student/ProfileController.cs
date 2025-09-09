using Business.Interfaces;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Core.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models.ViewModels.Student;

namespace WebUI.Controllers.Student
{
    [Authorize(Roles = "Student")]
    public class ProfileController : Controller
    {
        private readonly IIUserService _userService;

        public ProfileController(IIUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetUserWithUniversityAsync(userId);
                
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bilgileri bulunamadı.";
                    return RedirectToAction("Index", "Dashboard");
                }

                var model = new ProfileUpdateViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName ?? "",
                    Phone = user.Phone ?? "",
                    Email = user.Email ?? "",
                    StudentNumber = user.StudentNumber ?? "",
                    Department = user.Department ?? "",
                    Grade = user.Grade ?? 1
                };

                return View("~/Views/Profile/Index.cshtml", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Profil yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileUpdateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("~/Views/Profile/Index.cshtml", model);
                }

                var userId = GetCurrentUserId();
                var user = await _userService.GetUserByIdAsync(userId);
                
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bilgileri bulunamadı.";
                    return View("~/Views/Profile/Index.cshtml", model);
                }

                // Update user information
                user.FullName = model.FullName;
                user.Phone = model.Phone;
                user.Department = model.Department;
                user.StudentNumber = string.IsNullOrWhiteSpace(model.StudentNumber) ? null : model.StudentNumber;
                user.Grade = model.Grade;

                await _userService.UpdateUserAsync(user);
                
                TempData["SuccessMessage"] = "Profil başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Profil güncellenirken hata oluştu: " + ex.Message;
                return View("~/Views/Profile/Index.cshtml", model);
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
                    // ModelState hatalarını TempData'ya ekle
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    TempData["ErrorMessage"] = string.Join(", ", errors);
                    return RedirectToAction(nameof(Index));
                }

                var userId = GetCurrentUserId();
                var result = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
                
                if (result)
                {
                    TempData["SuccessMessage"] = "Şifre başarıyla değiştirildi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Şifre değiştirilemedi. Mevcut şifrenizi kontrol edin.";
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
                TempData["ErrorMessage"] = "Şifre değiştirme sırasında hata oluştu: " + ex.Message;
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
