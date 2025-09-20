using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebUI.Controllers
{
    // Temel uygulama kontrolcü sınıfı
    public abstract class BaseAppController : Controller
    {
        // Mevcut kullanıcı ID'sini getirir
        protected int GetCurrentUserId()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                throw new UnauthorizedAccessException("Lütfen önce giriş yapın.");
            }

            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User?.FindFirst("UserId")?.Value;

            if (int.TryParse(idClaim, out var userId))
                return userId;

            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            var claimsString = string.Join(", ", claims);
            
            throw new InvalidOperationException($"Kullanıcı oturumu bulunamadı veya UserId claim'i yok. Mevcut claim'ler: [{claimsString}]");
        }

        // Kullanıcı giriş yapmış mı kontrol eder
        protected bool IsUserAuthenticated()
        {
            return User.Identity?.IsAuthenticated ?? false;
        }

        // Mevcut kullanıcı adını getirir
        protected string GetCurrentUserName()
        {
            return User?.FindFirst(ClaimTypes.Name)?.Value ?? "Bilinmeyen Kullanıcı";
        }

        // Mevcut kullanıcı e-postasını getirir
        protected string GetCurrentUserEmail()
        {
            return User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        // Kullanıcının rolünü getirir
        protected string GetCurrentUserRole()
        {
            return User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        // Kullanıcının belirtilen rolde olup olmadığını kontrol eder
        protected bool IsUserInRole(string role)
        {
            return User.IsInRole(role);
        }

        // Kullanıcının öğrenci olup olmadığını kontrol eder
        protected bool IsStudent()
        {
            return IsUserInRole("Student");
        }

        // Kullanıcının danışman olup olmadığını kontrol eder
        protected bool IsAdvisor()
        {
            return IsUserInRole("Advisor");
        }

        // Kullanıcının şirket olup olmadığını kontrol eder
        protected bool IsCompany()
        {
            return IsUserInRole("Company");
        }

        // Kullanıcının admin olup olmadığını kontrol eder
        protected bool IsAdmin()
        {
            return IsUserInRole("Admin");
        }

        // Başarı mesajı ayarlar
        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        // Hata mesajı ayarlar
        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        // Bilgi mesajı ayarlar
        protected void SetInfoMessage(string message)
        {
            TempData["InfoMessage"] = message;
        }

        // Uyarı mesajı ayarlar
        protected void SetWarningMessage(string message)
        {
            TempData["WarningMessage"] = message;
        }
    }
}
