using Business.Interfaces.Com;
using Business.Interfaces.UserRepose;
using Business.OrderMethod;
using Business.Services.Interfaces;
using Core.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebUI.Models.ViewModels;
using WebUI.Models.ViewModels.Account;
using WebUI.Models.ViewModels.Education;

namespace WebUI.Controllers;

/// <summary>
/// Hesap Yönetimi Controller'ı
/// Kullanıcı kimlik doğrulama, kayıt olma, şifre sıfırlama ve aktivasyon işlemlerini yönetir
/// Öğrenci, danışman, şirket ve admin için farklı kayıt süreçlerini destekler
/// </summary>
public class AccountController : Controller
{
    /// <summary>
    /// Kullanıcı işlemleri servisi - kullanıcı CRUD, aktivasyon, şifre işlemleri
    /// </summary>
    private readonly IIUserService _userService;
    
    /// <summary>
    /// Şirket işlemleri servisi - şirket kayıt ve yönetimi
    /// </summary>
    private readonly IICompanyService _companyService;
    
    /// <summary>
    /// Üniversite işlemleri servisi - üniversite listesi ve validasyon
    /// </summary>
    private readonly IIUniversityService _universityService;

    /// <summary>
    /// AccountController constructor
    /// Dependency injection ile gerekli servisleri alır
    /// </summary>
    public AccountController(IIUserService userService, IICompanyService ıCompanyService, IIUniversityService _UniversityService)
    {
        _userService = userService;
        _companyService = ıCompanyService;
        this._universityService = _UniversityService;
    }

    /// <summary>
    /// Giriş yapma sayfasını gösterir
    /// Rol bazlı yönlendirme ve return URL desteği
    /// </summary>
    /// <param name="role">Hedef kullanıcı rolü (Student, Company, Advisor)</param>
    /// <param name="returnUrl">Giriş sonrası dönülecek sayfa</param>
    /// <returns>Login formu</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string role, string? returnUrl = null)
    {
        ViewBag.Role = role;
        ViewData["ReturnUrl"] = returnUrl;
        return View("~/Views/Account/Authentication/Login.cshtml");
    }

    /// <summary>
    /// Giriş yapma POST işlemi
    /// E-posta/şifre doğrulaması, claims oluşturma ve oturum başlatma
    /// </summary>
    /// <param name="model">Giriş form verileri (email, password, rememberMe)</param>
    /// <param name="returnUrl">Giriş sonrası dönülecek sayfa</param>
    /// <returns>Başarılıysa dashboard'a, hatalıysa tekrar login formuna</returns>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View("~/Views/Account/Authentication/Login.cshtml", model);

        // E-posta ile kullanıcıyı bul
        var user = await _userService.GetUserByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
            return View("~/Views/Account/Authentication/Login.cshtml", model);
        }

        // Hesap aktivasyon kontrolü
        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Hesabınız aktif değil. Lütfen onay/aktivasyon işlemini tamamlayın.");
            return View("~/Views/Account/Authentication/Login.cshtml", model);
        }

        // Şifre doğrulaması - hash karşılaştırması
        if (!PasswordSecurity.VerifyPassword(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
            return View("~/Views/Account/Authentication/Login.cshtml", model);
        }

        // Mevcut oturumu temizle (güvenlik için)
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Claims (kimlik bilgileri) oluştur
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // RememberMe durumuna göre farklı oturum süreleri
        var authProps = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true,
            ExpiresUtc = model.RememberMe 
                ? DateTimeOffset.UtcNow.AddDays(30)  // 30 gün hatırla
                : DateTimeOffset.UtcNow.AddHours(8)   // 8 saat normal oturum
        };

        // Cookie authentication ile oturum başlat
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProps
        );

        // Session'a ek bilgiler kaydet (hızlı erişim için)
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserEmail", user.Email ?? string.Empty);
        HttpContext.Session.SetString("UserFullName", user.FullName ?? string.Empty);
        HttpContext.Session.SetString("UserRole", user.Role.ToString());

        // Return URL varsa oraya yönlendir
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        // Role göre farklı dashboard'lara yönlendir
        if (user.Role == UserRole.Advisor)
            return RedirectToAction("Index", "AdvisorDashboard");
        else if (user.Role == UserRole.Company)
            return RedirectToAction("Index", "Company");
        else if (user.Role == UserRole.Student)
            return RedirectToAction("Index", "Dashboard");

        TempData["LoginSuccess"] = "Başarıyla giriş yaptınız.";
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Kayıt olma seçenekleri sayfası
    /// Öğrenci, Danışman, Şirket rollerinden birini seçme
    /// </summary>
    /// <returns>Rol seçim sayfası</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterOptions()
    {
        return View("~/Views/Account/Registration/Options.cshtml");
    }

    /// <summary>
    /// Öğrenci kayıt formu sayfası
    /// Üniversite listesi ile birlikte form gösterir
    /// </summary>
    /// <returns>Öğrenci kayıt formu</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> StudentRegister()
    {
        var universities = await _universityService.GetAllUniversities();
        var model = new StudentRegisterViewModel
        {
            Universities = universities.Select(u => new UniversityDropdownViewModel
            {
                Id = u.Id,
                Name = u.Name
            }).ToList()
        };
        return View(model);
    }

    /// <summary>
    /// Öğrenci kayıt POST işlemi
    /// Validasyon, edu.tr email kontrolü ve kullanıcı oluşturma
    /// </summary>
    /// <param name="model">Öğrenci kayıt form verileri</param>
    /// <returns>Başarılıysa onay bekleme sayfası</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> StudentRegister(StudentRegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return await FillUniversitiesAsync(model);
        }

        // Kurumsal email kontrolü - öğrenciler için edu.tr zorunlu
        if (!model.Email.EndsWith("edu.tr"))
        {
            ModelState.AddModelError("Email", "Lütfen üniversitenizin kurumsal e-posta adresini giriniz.");
            return await FillUniversitiesAsync(model);
        }

        // Üniversite validasyonu
        var university = await _universityService.GetUniversityByNameAsync(model.UniversityName);

        if (university == null)
        {
            ModelState.AddModelError("UniversityName", "Seçilen üniversite geçersiz.");
            return await FillUniversitiesAsync(model);
        }

        // Yeni öğrenci kullanıcısı oluştur
        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            PasswordHash = PasswordSecurity.HashPassword(model.Password), // Şifre hash'le
            Role = UserRole.Student,
            IsActive = false, // Aktivasyon bekliyor
            UniversityId = university.Id
        };

        try
        {
            await _userService.AddUserAsync(user);
        }
        catch (InvalidOperationException ex)
        {
            // Duplicate email gibi hatalar
            ModelState.AddModelError("", ex.Message);
            return await FillUniversitiesAsync(model);
        }

        return RedirectToAction("WaitingForApproval", new { email = model.Email });
    }

    /// <summary>
    /// Danışman kayıt formu sayfası
    /// Üniversite listesi ile birlikte form gösterir
    /// </summary>
    /// <returns>Danışman kayıt formu</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> AdvisorRegister()
    {
        var universities = await _universityService.GetAllUniversities();
        var model = new AdvisorRegisterViewModel
        {
            Universities = universities.Select(u => new UniversityDropdownViewModel
            {
                Id = u.Id,
                Name = u.Name
            }).ToList()
        };
        return View("~/Views/Account/Registration/Advisor.cshtml", model);
    }

    /// <summary>
    /// Danışman kayıt POST işlemi
    /// Validasyon, kurumsal email kontrolü ve danışman oluşturma
    /// </summary>
    /// <param name="model">Danışman kayıt form verileri</param>
    /// <returns>Başarılıysa onay bekleme sayfası</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> AdvisorRegister(AdvisorRegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return await FillUniversitiesAsync(model);
        }

        // Not: Bu kontrol değiştirilmeli - gmail.com yerine edu.tr olmalı
        if (!model.Email.EndsWith("gmail.com"))
        {
            ModelState.AddModelError("Email", "Lütfen üniversitenizin kurumsal e-posta adresini giriniz.");
            return await FillUniversitiesAsync(model);
        }

        var university = await _universityService.GetUniversityByNameAsync(model.UniversityName);

        if (university == null)
        {
            ModelState.AddModelError("UniversityName", "Seçilen üniversite geçersiz.");
            return await FillUniversitiesAsync(model);
        }

        // Yeni danışman kullanıcısı oluştur
        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            UniversityId = university.Id,
            PasswordHash = PasswordSecurity.HashPassword(model.Password),
            Role = UserRole.Advisor,
            IsActive = false, // Aktivasyon bekliyor
        };

        try
        {
            await _userService.AddUserAsync(user);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View("~/Views/Account/Registration/Advisor.cshtml", model);
        }

        return RedirectToAction("WaitingForApproval");
    }

    /// <summary>
    /// Çıkış yapma işlemi
    /// Cookie ve session temizleme
    /// </summary>
    /// <returns>Ana sayfaya yönlendirir</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Cookie authentication'ı temizle
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // Session verilerini temizle
        HttpContext.Session.Clear();
        
        return RedirectToAction("Index", "Home"); 
    }

    /// <summary>
    /// Şirket kayıt formu sayfası
    /// Şirket ve şirket temsilcisi bilgileri için form
    /// </summary>
    /// <returns>Şirket kayıt formu</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult CompanyRegister()
    {
        var model = new CompanyRegisterViewModel();
        return View("~/Views/Account/Registration/Company.cshtml", model);
    }

    /// <summary>
    /// Şirket kayıt POST işlemi
    /// Şirket entity'si ve şirket kullanıcısını birlikte oluşturma
    /// </summary>
    /// <param name="model">Şirket kayıt form verileri</param>
    /// <returns>Başarılıysa onay bekleme sayfası</returns>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompanyRegister(CompanyRegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Şirket entity'si oluştur
        var company = new Core.Entity.Company
        {
            CompanyName = model.CompanyName,
            Sector = model.Sector, // Sektör bilgisi
            Phone = model.Phone,
            TaxNumber = model.TaxNumber,
            Approved = false // Admin onayı bekliyor
        };

        // Şirket temsilcisi kullanıcısı oluştur
        var user = new User
        {
            FullName = model.CompanyUserFullName,
            Email = model.EmailUser,
            PasswordHash = PasswordSecurity.HashPassword(model.Password),
            Role = UserRole.Company,
            IsActive = false // Aktivasyon bekliyor
        };

        try
        {
            // Şirket ve kullanıcıyı birlikte oluştur (CompanyUser ilişkisi ile)
            await _companyService.AddCompanyWithUserAsync(company, user, model.RoleInCompany);
        }
        catch (InvalidOperationException)
        {
            ModelState.AddModelError("", "Bu e-posta adresiyle daha önce kayıt olunmuştur.");
            return View(model);
        }

        return RedirectToAction("WaitingForApproval", new { email = user.Email });
    }

    /// <summary>
    /// Onay bekleme sayfası
    /// Kayıt sonrası admin onayı veya email aktivasyonu bekleyen kullanıcılar için
    /// </summary>
    /// <param name="email">Kayıt olan kullanıcının email'i</param>
    /// <returns>Bekleme bildirimi sayfası</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult WaitingForApproval(string email)
    {
        ViewBag.Email = email;
        return View("~/Views/Account/Status/WaitingForApproval.cshtml");
    }

    /// <summary>
    /// Email aktivasyon işlemi
    /// Email'de gelen aktivasyon kodunu doğrulama
    /// </summary>
    /// <param name="code">Email'den gelen aktivasyon kodu</param>
    /// <returns>Başarılıysa login'e yönlendirir</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Activate(string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Aktivasyon kodu eksik.");

        try
        {
            await _userService.ActivateUserAsync(code);
            TempData["ActivationSuccess"] = "Hesabınız başarıyla aktifleştirildi. Giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ActivationError"] = ex.Message;
            return RedirectToAction("Login");
        }
    }

    /// <summary>
    /// Şifremi unuttum sayfası
    /// Email ile şifre sıfırlama talebini başlatma
    /// </summary>
    /// <returns>Şifre sıfırlama formu</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View("~/Views/Account/Password/Forgot.cshtml");
    }

    /// <summary>
    /// Şifremi unuttum POST işlemi
    /// Email'e şifre sıfırlama linki gönderme
    /// </summary>
    /// <param name="model">Şifre sıfırlama form verileri (email)</param>
    /// <returns>Başarılıysa login'e yönlendirir</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Account/Password/Forgot.cshtml", model);

        // Şifre sıfırlama URL'i (production'da domain değişmeli)
        var resetUrl = "https://localhost:7211/Account/ResetPassword";

        try
        {
            await _userService.SendForgetPasswordAsync(model.Email, resetUrl);
            TempData["ResetEmailSent"] = "Şifre sıfırlama e-postası gönderildi. Lütfen e-postanızı kontrol edin.";
            return RedirectToAction("Login");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View("~/Views/Account/Password/Forgot.cshtml", model);
        }
    }

    /// <summary>
    /// Şifre sıfırlama sayfası (GET)
    /// Email'den gelen token ile şifre sıfırlama formu
    /// </summary>
    /// <param name="token">Email'den gelen güvenlik token'ı</param>
    /// <param name="email">Şifresi sıfırlanacak kullanıcının email'i</param>
    /// <returns>Şifre sıfırlama formu</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            return BadRequest("Token veya e-posta eksik.");

        var model = new ResetPasswordViewModel
        {
            Token = token,
            Email = email
        };

        return View(model);
    }

    /// <summary>
    /// Şifre sıfırlama POST işlemi
    /// Token doğrulaması ve yeni şifre kaydetme
    /// </summary>
    /// <param name="model">Şifre sıfırlama form verileri (token, email, newPassword)</param>
    /// <returns>Başarılıysa aynı sayfada başarı mesajı</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) 
            return View("~/Views/Account/Password/Reset.cshtml", model);

        try
        {
            await _userService.ResetPasswordAsync(model.Token, model.NewPassword);
            TempData["ResetSuccess"] = "Şifreniz başarıyla değiştirildi. Giriş yapabilirsiniz.";
            return View("~/Views/Account/Password/Reset.cshtml", model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View("~/Views/Account/Password/Reset.cshtml", model);
        }
    }

    /// <summary>
    /// Helper method - Üniversite dropdown listesini doldurma
    /// Kayıt formlarında hata durumunda dropdown'ları yeniden yüklemek için
    /// </summary>
    /// <param name="model">Üniversite dropdown'ı olan base ViewModel</param>
    /// <returns>Doğru view ile model</returns>
    private async Task<IActionResult> FillUniversitiesAsync(UniversityRegisterBaseViewModel model)
    {
        // Üniversite listesini yeniden yükle
        model.Universities = (await _universityService.GetAllUniversities())
             .Select(u => new UniversityDropdownViewModel
             {
                 Id = u.Id,
                 Name = u.Name
             }).ToList();
        
        // Model tipine göre doğru view'ı döndür
        if (model is StudentRegisterViewModel)
            return View("~/Views/Account/Registration/Student.cshtml", model);
        else if (model is AdvisorRegisterViewModel)
            return View("~/Views/Account/Registration/Advisor.cshtml", model);
        
        return View(model);
    }
}

