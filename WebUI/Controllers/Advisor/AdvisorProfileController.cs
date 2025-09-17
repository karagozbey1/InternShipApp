using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Interfaces.UserRepose;
using WebUI.Models.ViewModels.Advisor;
using WebUI.Models.ViewModels.Account;
using System.Text;

namespace WebUI.Controllers.Advisor
{
    [Authorize(Roles = "Advisor")]
    public class AdvisorProfileController : Controller
    {
        private readonly IIUserService _userService;
        private readonly IWebHostEnvironment _env;

        public AdvisorProfileController(IIUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _env = env;
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

                // Danýþman bilgilerini güncelle
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
                TempData["ErrorMessage"] = "Dosya boyutu 5MB'ý geçemez.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var userId = GetCurrentUserId();
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "user-avatars");
                Directory.CreateDirectory(uploadsRoot);

                // önceki dosyayý temizle
                foreach (var old in Directory.GetFiles(uploadsRoot, $"user_{userId}.*"))
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
                TempData["ErrorMessage"] = "Profil resmi yüklenirken hata oluþtu: " + ex.Message;
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
                TempData["SuccessMessage"] = "Profil resmi kaldýrýldý.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Profil resmi kaldýrýlýrken hata oluþtu: " + ex.Message;
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
                var user = await _userService.GetUserByIdAsync(userId);
                var initials = GetInitials(user?.FullName ?? "?");
                var svg = GenerateAvatarSvg(initials);
                return File(System.Text.Encoding.UTF8.GetBytes(svg), "image/svg+xml");
            }
            catch
            {
                var svg = GenerateAvatarSvg("blackpink");
                return File(System.Text.Encoding.UTF8.GetBytes(svg), "image/svg+xml");
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