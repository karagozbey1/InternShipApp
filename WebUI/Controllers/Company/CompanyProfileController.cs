using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models.Company;
using Business.Interfaces.Com;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Core.Entity;
using System.Security.Claims;
using WebUI.Models.ViewModels.Account;
using System.Text;

namespace WebUI.Controllers.Company
{
    [Authorize(Roles = "Company")]
    public class CompanyProfileController : Controller
    {
        private readonly IICompanyService _companyService;
        private readonly IInternshipOfferService _offerService;
        private readonly IInternshipApplicationService _applicationService;
        private readonly IIUserService _userService;
        private readonly IWebHostEnvironment _env;

        public CompanyProfileController(
            IICompanyService companyService,
            IInternshipOfferService offerService,
            IInternshipApplicationService applicationService,
            IIUserService userService,
            IWebHostEnvironment env)
        {
            _companyService = companyService;
            _offerService = offerService;
            _applicationService = applicationService;
            _userService = userService;
            _env = env;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private async Task<int> GetCurrentCompanyIdAsync()
        {
            var userId = GetCurrentUserId();
            var company = await _companyService.GetCompanyByUserIdAsync(userId);
            return company?.CompanyId ?? 0;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var companyId = await GetCurrentCompanyIdAsync();
            var company = await _companyService.GetCompanyWithDetailsAsync(companyId);
            var companyUser = company?.CompanyUsers?.FirstOrDefault();
            var user = companyUser?.User;

            var model = new CompanyProfileViewModel
            {
                CompanyId = company?.CompanyId ?? 0,
                CompanyName = company?.CompanyName ?? "",
                Sector = company?.Sector ?? "",
                Phone = company?.Phone ?? "",
                TaxNumber = company?.TaxNumber ?? "",
                Address = company?.Address ?? "",
                Description = company?.Description ?? "",
                Website = company?.Website ?? "",
                EmployeeCount = company?.EmployeeCount ?? "",
                EstablishedDate = company?.EstablishedDate,
                LogoPath = company?.LogoPath ?? "",
                UserFullName = user?.FullName ?? "",
                Email = user?.Email ?? "",
                TotalInterns = 0,
                ActiveJobPostings = 0,
                SuccessRate = 0,
                MembershipYear = company?.EstablishedDate?.Year ?? DateTime.Now.Year
            };
            return View("~/Views/Company/Profile/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLogo(int CompanyId, IFormFile LogoFile)
        {
            var company = await _companyService.GetCompanyByIdAsync(CompanyId);
            if (company == null)
            {
                TempData["ErrorMessage"] = "Þirket bulunamadý.";
                return RedirectToAction("Index");
            }
            if (LogoFile != null && LogoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "company-logos");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // eski logolarý temizle
                foreach (var old in Directory.GetFiles(uploadsFolder, $"company_{company.CompanyId}_*"))
                    System.IO.File.Delete(old);

                var fileName = $"company_{company.CompanyId}_{DateTime.UtcNow.Ticks}{Path.GetExtension(LogoFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await LogoFile.CopyToAsync(stream);
                }
                company.LogoPath = $"/uploads/company-logos/{fileName}";
                await _companyService.UpdateCompanyAsync(company);
                TempData["SuccessMessage"] = "Logo baþarýyla güncellendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogo(int CompanyId)
        {
            var company = await _companyService.GetCompanyByIdAsync(CompanyId);
            if (company == null)
            {
                TempData["ErrorMessage"] = "Þirket bulunamadý.";
                return RedirectToAction("Index");
            }
            try
            {
                if (!string.IsNullOrWhiteSpace(company.LogoPath))
                {
                    var physical = Path.Combine(_env.WebRootPath, company.LogoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(physical))
                    {
                        System.IO.File.Delete(physical);
                    }
                    company.LogoPath = null;
                    await _companyService.UpdateCompanyAsync(company);
                }
                TempData["SuccessMessage"] = "Logo kaldýrýldý.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Logo kaldýrýlýrken hata oluþtu: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Logo(int? id = null)
        {
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            try
            {
                int companyId = id ?? await GetCurrentCompanyIdAsync();
                var company = await _companyService.GetCompanyByIdAsync(companyId);
                if (company != null && !string.IsNullOrWhiteSpace(company.LogoPath))
                {
                    var path = Path.Combine(_env.WebRootPath, company.LogoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    var contentType = GetContentTypeByExtension(Path.GetExtension(path));
                    if (System.IO.File.Exists(path))
                        return PhysicalFile(path, contentType);
                }
                var initials = GetInitials(company?.CompanyName ?? "?");
                var svg = GenerateAvatarSvg(initials);
                return File(Encoding.UTF8.GetBytes(svg), "image/svg+xml");
            }
            catch
            {
                var svg = GenerateAvatarSvg("?");
                return File(Encoding.UTF8.GetBytes(svg), "image/svg+xml");
            }
        }

        private static string GetInitials(string name)
        {
            var parts = (name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
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
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(CompanyProfileViewModel model)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            var company = await _companyService.GetCompanyWithDetailsAsync(companyId);
            var companyUser = company?.CompanyUsers?.FirstOrDefault();
            var user = companyUser?.User;

            if (company == null || user == null)
            {
                TempData["ErrorMessage"] = "Þirket veya kullanýcý bulunamadý.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrWhiteSpace(model.CompanyName)) company.CompanyName = model.CompanyName;
            if (!string.IsNullOrWhiteSpace(model.Sector)) company.Sector = model.Sector;
            if (!string.IsNullOrWhiteSpace(model.Phone)) company.Phone = model.Phone;
            if (!string.IsNullOrWhiteSpace(model.TaxNumber)) company.TaxNumber = model.TaxNumber;
            if (!string.IsNullOrWhiteSpace(model.Address)) company.Address = model.Address;
            if (!string.IsNullOrWhiteSpace(model.Description)) company.Description = model.Description;
            if (!string.IsNullOrWhiteSpace(model.Website)) company.Website = model.Website;
            if (!string.IsNullOrWhiteSpace(model.EmployeeCount)) company.EmployeeCount = model.EmployeeCount;
            if (model.EstablishedDate.HasValue)
                company.EstablishedDate = DateTime.SpecifyKind(model.EstablishedDate.Value, DateTimeKind.Utc);

            if (!string.IsNullOrWhiteSpace(model.UserFullName)) user.FullName = model.UserFullName;
            if (!string.IsNullOrWhiteSpace(model.Email)) user.Email = model.Email;

            await _companyService.UpdateCompanyAsync(company);
            await _userService.UpdateUserAsync(user);

            TempData["SuccessMessage"] = "Profil baþarýyla güncellendi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    TempData["ErrorMessage"] = string.Join(", ", errors);
                    return RedirectToAction("Index");
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

                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Þifre deðiþtirme sýrasýnda hata oluþtu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
