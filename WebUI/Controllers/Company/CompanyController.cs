using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models.Company;
using Business.Interfaces.Com;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Business.Interfaces.Order;
using Business.Interfaces.Chat;
using Core.Entity;

namespace WebUI.Controllers.Company
{
    [Authorize(Roles = "Company")]
    [Route("Company")] // preserve original URL prefix
    public class CompanyController : BaseCompanyController
    {
        public CompanyController(
            IICompanyService companyService,
            IInternshipOfferService offerService,
            IInternshipApplicationService applicationService,
            IIUserService userService,
            IINotificationService notificationService,
            IIChatService chatService)
            : base(companyService, offerService, applicationService, userService, notificationService, chatService) { }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var companyId = await GetCurrentCompanyIdAsync();
                var company = await _companyService.GetCompanyByIdAsync(companyId);
                if (company == null)
                {
                    TempData["ErrorMessage"] = "Þirket bilgileri bulunamadý.";
                    return RedirectToAction("Login", "Account");
                }

                var offers = await _offerService.GetOffersByCompanyIdAsync(companyId);
                var allApplications = new List<InternshipApplication>();
                foreach (var offer in offers)
                {
                    var offerApplications = await _applicationService.GetApplicationsByOfferIdAsync(offer.Id);
                    allApplications.AddRange(offerApplications);
                }
                allApplications = allApplications.Where(a => a.Status != InternshipApplicationStatus.Rejected).ToList();
                var recentApplications = allApplications.OrderByDescending(a => a.CreatedAt).Take(5).ToList();
                var activeInterns = allApplications.Where(a => a.Status == InternshipApplicationStatus.Approved && a.EndDate > DateTime.Now).ToList();

                // Generate dynamic recent activities
                var recentActivities = await GenerateRecentActivitiesAsync(companyId, allApplications, offers.ToList());

                var model = new CompanyDashboardViewModel
                {
                    CompanyName = company.CompanyName,
                    Sector = company.Sector ?? string.Empty,
                    TotalJobPostings = offers.Count,
                    TotalApplications = allApplications.Count,
                    ActiveInterns = activeInterns.Count,
                    TotalInterns = allApplications.Count(a => a.Status == InternshipApplicationStatus.Approved),
                    RecentApplications = recentApplications.Select(MapToRecentApplicationViewModel).ToList(),
                    ActiveInternsList = activeInterns.Select(MapToActiveInternViewModel).ToList(),
                    RecentActivities = recentActivities
                };
                return View("~/Views/Company/Dashboard/Index.cshtml", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Dashboard yüklenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Company/Dashboard/Index.cshtml", new CompanyDashboardViewModel { CompanyName = "Þirket" });
            }
        }

        // New method to generate dynamic recent activities
        private async Task<List<CompanyActivityViewModel>> GenerateRecentActivitiesAsync(
            int companyId, 
            List<InternshipApplication> applications, 
            List<InternshipOffer> offers)
        {
            var activities = new List<CompanyActivityViewModel>();
            
            try
            {
                // Recent applications (last 7 days)
                var recentApps = applications
                    .Where(a => a.CreatedAt >= DateTime.Now.AddDays(-7))
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5);

                foreach (var app in recentApps)
                {
                    activities.Add(new CompanyActivityViewModel
                    {
                        Title = "Yeni Baþvuru",
                        Description = $"{app.Student?.FullName ?? "Bir öðrenci"} '{app.InternshipOffer?.Title ?? "pozisyon"}' ilanýnýza baþvurdu",
                        Icon = "fas fa-file-alt",
                        Color = "blue",
                        CreatedAt = app.CreatedAt,
                        ActivityType = "NewApplication",
                        RelatedId = app.Id
                    });
                }

                // Recent status changes (approved/rejected applications in last 7 days)
                var recentStatusChanges = applications
                    .Where(a => a.UpdatedAt >= DateTime.Now.AddDays(-7) && 
                               a.Status != InternshipApplicationStatus.Pending)
                    .OrderByDescending(a => a.UpdatedAt)
                    .Take(5);

                foreach (var app in recentStatusChanges)
                {
                    string statusText = app.Status == InternshipApplicationStatus.Approved ? "onaylandý" : "reddedildi";
                    string icon = app.Status == InternshipApplicationStatus.Approved ? "fas fa-check-circle" : "fas fa-times-circle";
                    string color = app.Status == InternshipApplicationStatus.Approved ? "green" : "red";

                    activities.Add(new CompanyActivityViewModel
                    {
                        Title = $"Baþvuru {statusText.ToUpperInvariant()}",
                        Description = $"{app.Student?.FullName ?? "Bir öðrenci"}'nin baþvurusu {statusText}",
                        Icon = icon,
                        Color = color,
                        CreatedAt = app.UpdatedAt,
                        ActivityType = "StatusChange",
                        RelatedId = app.Id
                    });
                }

                // Recent job postings (last 7 days)
                var recentOffers = offers
                    .Where(o => o.CreatedAt >= DateTime.Now.AddDays(-7))
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(3);

                foreach (var offer in recentOffers)
                {
                    activities.Add(new CompanyActivityViewModel
                    {
                        Title = "Yeni Ýlan Yayýnlandý",
                        Description = $"'{offer.Title}' pozisyonu için yeni ilan oluþturuldu",
                        Icon = "fas fa-briefcase",
                        Color = "purple",
                        CreatedAt = offer.CreatedAt,
                        ActivityType = "NewJobPosting",
                        RelatedId = offer.Id
                    });
                }

                // Active internships starting this week
                var weekStart = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
                var weekEnd = weekStart.AddDays(7);
                var startsThisWeek = applications
                    .Where(a => a.Status == InternshipApplicationStatus.Approved && 
                               a.StartDate >= weekStart && a.StartDate <= weekEnd)
                    .OrderBy(a => a.StartDate);

                foreach (var internship in startsThisWeek)
                {
                    activities.Add(new CompanyActivityViewModel
                    {
                        Title = "Staj Baþladý",
                        Description = $"{internship.Student?.FullName ?? "Bir öðrenci"} stajýna baþladý",
                        Icon = "fas fa-play-circle",
                        Color = "indigo",
                        CreatedAt = internship.StartDate,
                        ActivityType = "InternshipStarted",
                        RelatedId = internship.Id
                    });
                }

                // Internships ending this week
                var endsThisWeek = applications
                    .Where(a => a.Status == InternshipApplicationStatus.Approved && 
                               a.EndDate >= weekStart && a.EndDate <= weekEnd)
                    .OrderBy(a => a.EndDate);

                foreach (var internship in endsThisWeek)
                {
                    activities.Add(new CompanyActivityViewModel
                    {
                        Title = "Staj Tamamlanýyor",
                        Description = $"{internship.Student?.FullName ?? "Bir öðrenci"}'nin stajý bu hafta bitiyor",
                        Icon = "fas fa-graduation-cap",
                        Color = "orange",
                        CreatedAt = internship.EndDate,
                        ActivityType = "InternshipEnding",
                        RelatedId = internship.Id
                    });
                }

                // If no recent activities, add some system activities
                if (!activities.Any())
                {
                    activities.Add(new CompanyActivityViewModel
                    {
                        Title = "Hoþ Geldiniz",
                        Description = "Firma panelinize hoþ geldiniz. Aktiviteleriniz burada görünecek.",
                        Icon = "fas fa-home",
                        Color = "blue",
                        CreatedAt = DateTime.Now,
                        ActivityType = "Welcome"
                    });
                }

                // Sort by creation time and take most recent 10
                return activities
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                // Return default activity if error occurs
                return new List<CompanyActivityViewModel>
                {
                    new CompanyActivityViewModel
                    {
                        Title = "Sistem Bilgisi",
                        Description = "Aktiviteler yüklenirken bir sorun oluþtu.",
                        Icon = "fas fa-exclamation-triangle",
                        Color = "yellow",
                        CreatedAt = DateTime.Now,
                        ActivityType = "SystemError"
                    }
                };
            }
        }

        // New: AJAX endpoint for refreshing activities without full page reload
        [HttpGet("GetRecentActivities")]
        public async Task<JsonResult> GetRecentActivities()
        {
            try
            {
                var companyId = await GetCurrentCompanyIdAsync();
                var offers = await _offerService.GetOffersByCompanyIdAsync(companyId);
                var allApplications = new List<InternshipApplication>();
                
                foreach (var offer in offers)
                {
                    var offerApplications = await _applicationService.GetApplicationsByOfferIdAsync(offer.Id);
                    allApplications.AddRange(offerApplications);
                }
                
                allApplications = allApplications.Where(a => a.Status != InternshipApplicationStatus.Rejected).ToList();
                var recentActivities = await GenerateRecentActivitiesAsync(companyId, allApplications, offers.ToList());

                var result = recentActivities.Select(a => new
                {
                    title = a.Title,
                    description = a.Description,
                    icon = a.Icon,
                    color = a.Color,
                    timeAgo = a.TimeAgo,
                    activityType = a.ActivityType,
                    relatedId = a.RelatedId
                });

                return Json(new { success = true, activities = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // New: Get dashboard stats for AJAX refresh
        [HttpGet("GetDashboardStats")]
        public async Task<JsonResult> GetDashboardStats()
        {
            try
            {
                var companyId = await GetCurrentCompanyIdAsync();
                var offers = await _offerService.GetOffersByCompanyIdAsync(companyId);
                var allApplications = new List<InternshipApplication>();
                
                foreach (var offer in offers)
                {
                    var offerApplications = await _applicationService.GetApplicationsByOfferIdAsync(offer.Id);
                    allApplications.AddRange(offerApplications);
                }
                
                allApplications = allApplications.Where(a => a.Status != InternshipApplicationStatus.Rejected).ToList();
                var activeInterns = allApplications.Where(a => a.Status == InternshipApplicationStatus.Approved && a.EndDate > DateTime.Now).ToList();

                return Json(new
                {
                    success = true,
                    totalJobPostings = offers.Count,
                    totalApplications = allApplications.Count,
                    activeInterns = activeInterns.Count,
                    totalInterns = allApplications.Count(a => a.Status == InternshipApplicationStatus.Approved),
                    newApplicationsToday = allApplications.Count(a => a.CreatedAt.Date == DateTime.Today)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}