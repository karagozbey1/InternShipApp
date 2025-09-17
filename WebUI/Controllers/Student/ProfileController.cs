using Business.Interfaces;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Core.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models.ViewModels.Student;
using System.Text;

namespace WebUI.Controllers.Student
{
    [Authorize(Roles = "Student")]
    public class ProfileController : Controller
    {
        private readonly IIUserService _userService;
        private readonly IWebHostEnvironment _env;

        public ProfileController(IIUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _env = env;
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

                return View("~/Views/Student/Profile/Index.cshtml", model);
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
                    return View("~/Views/Student/Profile/Index.cshtml", model);
                }

                var userId = GetCurrentUserId();
                var user = await _userService.GetUserByIdAsync(userId);
                
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bilgileri bulunamadı.";
                    return View("~/Views/Student/Profile/Index.cshtml", model);
                }

                // Update user information (resim haricinde)
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
                return View("~/Views/Student/Profile/Index.cshtml", model);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile AvatarFile)
        {
            if (AvatarFile == null || AvatarFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir resim seçin.";
                return RedirectToAction(nameof(Index));
            }
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(AvatarFile.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                TempData["ErrorMessage"] = "Sadece JPG, PNG veya WEBP yükleyebilirsiniz.";
                return RedirectToAction(nameof(Index));
            }
            if (AvatarFile.Length > 5 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "Dosya boyutu 5MB'ı geçemez.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var userId = GetCurrentUserId();
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "user-avatars");
                Directory.CreateDirectory(uploadsRoot);

                // eski dosyaları sil
                var prefix = $"user_{userId}.*";
                foreach (var old in Directory.GetFiles(uploadsRoot, prefix))
                    System.IO.File.Delete(old);

                var fileName = $"user_{userId}{ext}";
                var physicalPath = Path.Combine(uploadsRoot, fileName);
                await using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }
                TempData["SuccessMessage"] = "Profil resmi güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Profil resmi yüklenirken hata oluştu: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveAvatar()
        {
            try
            {
                var userId = GetCurrentUserId();
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "user-avatars");
                if (Directory.Exists(uploadsRoot))
                {
                    foreach (var old in Directory.GetFiles(uploadsRoot, $"user_{userId}.*"))
                        System.IO.File.Delete(old);
                }
                TempData["SuccessMessage"] = "Profil resmi kaldırıldı.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Profil resmi kaldırılırken hata oluştu: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Avatar(int? id = null)
        {
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            try
            {
                var userId = id ?? GetCurrentUserId();
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "user-avatars");
                var files = Directory.Exists(uploadsRoot) ? Directory.GetFiles(uploadsRoot, $"user_{userId}.*") : Array.Empty<string>();
                if (files.Length > 0)
                {
                    var path = files[0];
                    var contentType = GetContentTypeByExtension(Path.GetExtension(path));
                    return PhysicalFile(path, contentType);
                }
                // Dosya yoksa isim baş harfleri ile SVG üret
                var user = await _userService.GetUserByIdAsync(userId);
                var initials = GetInitials(user?.FullName ?? "?");
                var svg = GenerateAvatarSvg(initials);
                return File(Encoding.UTF8.GetBytes(svg), "image/svg+xml");
            }
            catch
            {
                var svg = GenerateAvatarSvg("?");
                return File(Encoding.UTF8.GetBytes(svg), "image/svg+xml");
            }
        }

        private static string GetInitials(string fullName)
        {
            var parts = (fullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpperInvariant();
        }

        private static string GenerateAvatarSvg(string initials)
        {
            var bg = "url(#g)";
            return $"<svg xmlns='http://www.w3.org/2000/svg' width='96' height='96' viewBox='0 0 96 96'><defs><linearGradient id='g' x1='0' y1='0' x2='1' y2='1'><stop offset='0%' stop-color='#3b82f6'/><stop offset='100%' stop-color='#9333ea'/></linearGradient></defs><circle cx='48' cy='48' r='48' fill='{bg}'/><text x='50%' y='54%' dominant-baseline='middle' text-anchor='middle' font-family='Inter,Arial' font-size='36' fill='#fff' font-weight='700'>{System.Net.WebUtility.HtmlEncode(initials)}</text></svg>";
        }

        private static string GetContentTypeByExtension(string ext)
        {
            return ext.ToLowerInvariant() switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}
