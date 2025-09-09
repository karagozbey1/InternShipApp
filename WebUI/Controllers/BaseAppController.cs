using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebUI.Controllers
{
    public abstract class BaseAppController : Controller
    {
        protected int GetCurrentUserId()
        {
            // Kullanıcının kimlik doğrulaması yapılmış mı kontrol et
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                // AuthExceptionFilter tarafından yakalanacak ve login'e yönlendirilecek
                throw new UnauthorizedAccessException("Lütfen önce giriş yapın.");
            }

            // NameIdentifier veya custom "UserId" claim'inden okur
            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User?.FindFirst("UserId")?.Value;

            if (int.TryParse(idClaim, out var userId))
                return userId;

            // Debug için claim'leri logla
            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            var claimsString = string.Join(", ", claims);
            
            throw new InvalidOperationException($"Kullanıcı oturumu bulunamadı veya UserId claim'i yok. Mevcut claim'ler: [{claimsString}]");
        }

        protected bool IsUserAuthenticated()
        {
            return User.Identity?.IsAuthenticated ?? false;
        }

        protected string GetCurrentUserName()
        {
            return User?.FindFirst(ClaimTypes.Name)?.Value ?? "Bilinmeyen Kullanıcı";
        }

        protected string GetCurrentUserEmail()
        {
            return User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        protected string GetCurrentUserRole()
        {
            return User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        // Güvenli UserId alma 
        protected int? TryGetCurrentUserId()
        {
            if (!IsUserAuthenticated())
                return null;

            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User?.FindFirst("UserId")?.Value;

            return int.TryParse(idClaim, out var userId) ? userId : null;
        }

        // Kullanıcının belirli bir role sahip olup olmadığını kontrol eder
        protected bool IsInRole(string role)
        {
            return User.IsInRole(role);
        }

        // Student kullanıcı mı kontrol eder
        protected bool IsStudent()
        {
            return GetCurrentUserRole().Equals("Student", StringComparison.OrdinalIgnoreCase);
        }
    }
}
