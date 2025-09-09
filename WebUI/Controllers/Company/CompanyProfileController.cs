using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models.Company;
using Business.Interfaces.Com;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Core.Entity;
using System.Security.Claims;
using WebUI.Models.ViewModels.Account;

namespace WebUI.Controllers.Company
{
    [Authorize(Roles = "Company")]
    public class CompanyProfileController : Controller
    {
        private readonly IICompanyService _companyService;
        private readonly IInternshipOfferService _offerService;
        private readonly IInternshipApplicationService _applicationService;
        private readonly IIUserService _userService;

        public CompanyProfileController(
            IICompanyService companyService,
            IInternshipOfferService offerService,
            IInternshipApplicationService applicationService,
            IIUserService userService)
        {
            _companyService = companyService;
            _offerService = offerService;
            _applicationService = applicationService;
            _userService = userService;
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
            // Tüm iliþkili verilerle birlikte þirketi çek
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
            return View("~/Views/Company/Profile.cshtml", model);
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
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "company-logos");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var fileName = $"company_{company.CompanyId}_{DateTime.Now.Ticks}{Path.GetExtension(LogoFile.FileName)}";
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
        public async Task<IActionResult> Profile(CompanyProfileViewModel model)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            // Ýliþkili verilerle birlikte þirketi çek
            var company = await _companyService.GetCompanyWithDetailsAsync(companyId);
            var companyUser = company?.CompanyUsers?.FirstOrDefault();
            var user = companyUser?.User;

            if (company == null || user == null)
            {
                TempData["ErrorMessage"] = "Þirket veya kullanýcý bulunamadý.";
                return RedirectToAction("Index");
            }

            // Þirket alanlarý
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

            // Kullanýcý alanlarý
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
