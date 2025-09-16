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

public class AccountController : Controller
{
    private readonly IIUserService _userService;
    private readonly IICompanyService _companyService;
    private readonly IIUniversityService _universityService;

    public AccountController(IIUserService userService, IICompanyService ıCompanyService, IIUniversityService _UniversityService)
    {
        _userService = userService;
        _companyService = ıCompanyService;
        this._universityService = _UniversityService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string role, string? returnUrl = null)
    {
        ViewBag.Role = role;
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userService.GetUserByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Hesabınız aktif değil. Lütfen onay/aktivasyon işlemini tamamlayın.");
            return View(model);
        }

        if (!PasswordSecurity.VerifyPassword(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
            return View(model);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // RememberMe durumuna göre farklı süreler ayarla
        var authProps = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true,
            ExpiresUtc = model.RememberMe 
                ? DateTimeOffset.UtcNow.AddDays(30)  // 30 gün hatırla
                : DateTimeOffset.UtcNow.AddHours(8)   // 8 saat normal oturum
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProps
        );

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserEmail", user.Email ?? string.Empty);
        HttpContext.Session.SetString("UserFullName", user.FullName ?? string.Empty);
        HttpContext.Session.SetString("UserRole", user.Role.ToString());

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        if (user.Role == UserRole.Advisor)
            return RedirectToAction("Index", "AdvisorDashboard");
        else if (user.Role == UserRole.Company)
            return RedirectToAction("Index", "Company");
        else if (user.Role == UserRole.Student)
            return RedirectToAction("Index", "Dashboard");

        TempData["LoginSuccess"] = "Başarıyla giriş yaptınız.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterOptions()
    {
        return View();
    }

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

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> StudentRegister(StudentRegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return await FillUniversitiesAsync(model);
        }

        if (!model.Email.EndsWith("edu.tr"))
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

        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            PasswordHash = PasswordSecurity.HashPassword(model.Password),
            Role = UserRole.Student,
            IsActive = false,
            UniversityId = university.Id
        };

        try
        {
            await _userService.AddUserAsync(user);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return await FillUniversitiesAsync(model);
        }

        return RedirectToAction("WaitingForApproval", new { email = model.Email });
    }

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
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> AdvisorRegister(AdvisorRegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return await FillUniversitiesAsync(model);
        }

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

        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            UniversityId = university.Id,
            PasswordHash = PasswordSecurity.HashPassword(model.Password),
            Role = UserRole.Advisor,
            IsActive = false,
        };

        try
        {
            await _userService.AddUserAsync(user);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }

        return RedirectToAction("WaitingForApproval");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home"); 
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult CompanyRegister()
    {
        var model = new CompanyRegisterViewModel();
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompanyRegister(CompanyRegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var company = new Core.Entity.Company
        {
            CompanyName = model.CompanyName,
            Sector = model.Sector, // SectorList.ToString() yerine Sector
            Phone = model.Phone,
            TaxNumber = model.TaxNumber,
            Approved = false
        };

        var user = new User
        {
            FullName = model.CompanyUserFullName,
            Email = model.EmailUser,
            PasswordHash = PasswordSecurity.HashPassword(model.Password),
            Role = UserRole.Company,
            IsActive = false
        };

        try
        {
            await _companyService.AddCompanyWithUserAsync(company, user, model.RoleInCompany);
        }
        catch (InvalidOperationException)
        {
            ModelState.AddModelError("", "Bu e-posta adresiyle daha önce kayıt olunmuştur.");
            return View(model);
        }

        return RedirectToAction("WaitingForApproval", new { email = user.Email });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult WaitingForApproval(string email)
    {
        ViewBag.Email = email;
        return View();
    }

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

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

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
            return View(model);
        }
    }

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

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            await _userService.ResetPasswordAsync(model.Token, model.NewPassword);
            TempData["ResetSuccess"] = "Şifreniz başarıyla değiştirildi. Giriş yapabilirsiniz.";
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    private async Task<IActionResult> FillUniversitiesAsync(UniversityRegisterBaseViewModel model)
    {
        model.Universities = (await _universityService.GetAllUniversities())
             .Select(u => new UniversityDropdownViewModel
             {
                 Id = u.Id,
                 Name = u.Name
             }).ToList();
        return View(model);
    }
}

