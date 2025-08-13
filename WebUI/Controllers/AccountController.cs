using Business.Interfaces.UserRepose;
using Business.OrderMethod;
using Core.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebUI.Models.Account;

namespace StajTakipSistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly IIUserService _userService;

        public AccountController(IIUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login(string role )
        {
            ViewBag.Role = role;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetUserByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Hesabınız aktif değil. Lütfen admin onayını bekleyin veya e-posta aktivasyonunu tamamlayın.");
                return View(model);
            }


            var hashedInputPassword = PasswordSecurity.HashPassword(model.Password);

            if (user.PasswordHash != hashedInputPassword)
            {
                ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
                return View(model);
            }

            //cookie ile kimlik doğrulama işlemi

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }
      
        
        [HttpGet]
        public IActionResult RegisterOptions()
        {
            return View();
        }




        [HttpGet]
        public IActionResult StudentRegister()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StudentRegister(StudentRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!model.Email.EndsWith("edu.tr"))
            {
                ModelState.AddModelError("Email", "Lütfen üniversitenizin kurumsal e-posta adresini giriniz.");
                return View(model);
            }

           
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = model.Password,
                Role = UserRole.Student,
                IsActive = false,
                UniversityName = model.UniversityName 
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


            return RedirectToAction("WaitingForApproval", new { email = model.Email });
        }

        [HttpGet]
        public IActionResult AdvisorRegister()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdvisorRegister(AdvisorRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!model.Email.EndsWith("edu.tr"))
            {
                ModelState.AddModelError("Email", "Lütfen üniversitenizin kurumsal e-posta adresini giriniz.");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = model.Password,
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

        [HttpGet]
        public IActionResult CompanyRegister()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> CompanyRegister(CompanyRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);


            var user = new User
            {
                FullName = model.CompanyFullName,
                Email = model.Email,
                PasswordHash = model.Password, 
                Role = UserRole.Company,
                IsActive = false, 
                UniversityName = null
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

        [HttpGet]
        public IActionResult WaitingForApproval(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpGet]
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

    }

}
