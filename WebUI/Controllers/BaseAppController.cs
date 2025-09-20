using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebUI.Controllers
{
    /// <summary>
    /// Temel uygulama controller sınıfı
    /// Tüm controller'ların türetileceği abstract base sınıf
    /// Ortak kullanıcı işlemleri ve yetkilendirme metodlarını sağlar
    /// </summary>
    public abstract class BaseAppController : Controller
    {
        /// <summary>
        /// Şu anda giriş yapmış kullanıcının ID'sini alır
        /// Claims'lerden NameIdentifier veya UserId değerini okur
        /// </summary>
        /// <returns>Mevcut kullanıcının benzersiz ID'si</returns>
        /// <exception cref="UnauthorizedAccessException">Kullanıcı giriş yapmamışsa</exception>
        /// <exception cref="InvalidOperationException">UserId claim'i bulunamazsa</exception>
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

        /// <summary>
        /// Kullanıcının oturum açıp açmadığını kontrol eder
        /// True ise kullanıcı giriş yapmış, false ise misafir
        /// </summary>
        /// <returns>Kimlik doğrulama durumu</returns>
        protected bool IsUserAuthenticated()
        {
            return User.Identity?.IsAuthenticated ?? false;
        }

        /// <summary>
        /// Mevcut kullanıcının adını alır
        /// Claims'den Name değerini okur
        /// </summary>
        /// <returns>Kullanıcının tam adı veya "Bilinmeyen Kullanıcı"</returns>
        protected string GetCurrentUserName()
        {
            return User?.FindFirst(ClaimTypes.Name)?.Value ?? "Bilinmeyen Kullanıcı";
        }

        /// <summary>
        /// Mevcut kullanıcının e-posta adresini alır
        /// Claims'den Email değerini okur
        /// </summary>
        /// <returns>Kullanıcının e-posta adresi veya boş string</returns>
        protected string GetCurrentUserEmail()
        {
            return User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Mevcut kullanıcının rolünü alır
        /// Claims'den Role değerini okur (Student, Company, Advisor, Admin)
        /// </summary>
        /// <returns>Kullanıcının sistem rolü veya boş string</returns>
        protected string GetCurrentUserRole()
        {
            return User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Güvenli şekilde kullanıcı ID'sini almaya çalışır
        /// Hata fırlatmaz, null döndürür
        /// </summary>
        /// <returns>Kullanıcı ID'si veya null (giriş yapılmamışsa)</returns>
        protected int? TryGetCurrentUserId()
        {
            if (!IsUserAuthenticated())
                return null;

            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User?.FindFirst("UserId")?.Value;

            return int.TryParse(idClaim, out var userId) ? userId : null;
        }

        /// <summary>
        /// Kullanıcının belirli bir role sahip olup olmadığını kontrol eder
        /// ASP.NET Core'un built-in role kontrolünü kullanır
        /// </summary>
        /// <param name="role">Kontrol edilecek rol adı (Student, Company, Advisor, Admin)</param>
        /// <returns>Kullanıcı belirtilen role sahipse true</returns>
        protected bool IsInRole(string role)
        {
            return User.IsInRole(role);
        }

        /// <summary>
        /// Mevcut kullanıcının öğrenci olup olmadığını kontrol eder
        /// Öğrenci özel sayfalarında erişim kontrolü için kullanılır
        /// </summary>
        /// <returns>Kullanıcı öğrenci rolündeyse true</returns>
        protected bool IsStudent()
        {
            return GetCurrentUserRole().Equals("Student", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Mevcut kullanıcının şirket temsilcisi olup olmadığını kontrol eder
        /// Şirket paneli sayfalarında erişim kontrolü için kullanılır
        /// </summary>
        /// <returns>Kullanıcı şirket rolündeyse true</returns>
        protected bool IsCompany()
        {
            return GetCurrentUserRole().Equals("Company", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Mevcut kullanıcının danışman olup olmadığını kontrol eder
        /// Danışman paneli sayfalarında erişim kontrolü için kullanılır
        /// </summary>
        /// <returns>Kullanıcı danışman rolündeyse true</returns>
        protected bool IsAdvisor()
        {
            return GetCurrentUserRole().Equals("Advisor", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Mevcut kullanıcının admin olup olmadığını kontrol eder
        /// Admin paneli sayfalarında erişim kontrolü için kullanılır
        /// </summary>
        /// <returns>Kullanıcı admin rolündeyse true</returns>
        protected bool IsAdmin()
        {
            return GetCurrentUserRole().Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
