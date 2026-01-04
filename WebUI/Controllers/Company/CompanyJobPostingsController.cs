using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models.Company;
using Business.Interfaces.Com;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Core.Entity;
using DataAccess.Interfaces.Education;
using System.Globalization;

namespace WebUI.Controllers.Company
{
    [Authorize(Roles = "Company")]
    [Route("Company/JobPostings")] // Temel yol
    public class CompanyJobPostingsController : BaseCompanyController
    {
        private readonly IInternshipPeriodService _periodService;
        private readonly IProvinceRepository _provinceRepo;

        public CompanyJobPostingsController(
            IICompanyService companyService,
            IInternshipOfferService offerService,
            IInternshipApplicationService applicationService,
            IIUserService userService,
            Business.Interfaces.Order.IINotificationService notificationService,
            Business.Interfaces.Chat.IIChatService chatService,
            IInternshipPeriodService periodService,
            IProvinceRepository provinceRepo)
            : base(companyService, offerService, applicationService, userService, notificationService, chatService)
        {
            _periodService = periodService;
            _provinceRepo = provinceRepo;
        }

        // Liste
        [HttpGet("")]
        public async Task<IActionResult> List(string searchTerm, JobPostingType? typeFilter, bool? isActiveFilter)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            var offers = await _offerService.GetOffersByCompanyIdAsync(companyId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                offers = offers.Where(o =>
                    (o.Title ?? "").Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (o.Field ?? "").Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (o.City ?? "").Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            if (isActiveFilter.HasValue)
                offers = offers.Where(o => o.IsActive == isActiveFilter.Value).ToList();

            var list = new List<JobPostingItemViewModel>();
            foreach (var offer in offers)
            {
                var apps = await _applicationService.GetApplicationsByOfferIdAsync(offer.Id);
                list.Add(new JobPostingItemViewModel
                {
                    Id = offer.Id,
                    Title = offer.Title,
                    Position = offer.Field,
                    JobType = offer.JobType switch
                    {
                        OfferJobType.Job => JobPostingType.Job,
                        OfferJobType.Both => JobPostingType.Both,
                        _ => JobPostingType.Internship
                    },
                    Location = offer.City,
                    Salary = offer.MonthlySalary,
                    SalaryCurrency = offer.MonthlySalaryCurrency,
                    StartDate = offer.Period?.StartDate ?? DateTime.UtcNow,
                    EndDate = offer.Period?.EndDate ?? DateTime.UtcNow.AddDays(60),
                    ApplicationDeadline = offer.ApplicationDeadline ?? DateTime.UtcNow.AddDays(30),
                    Quota = offer.AvailablePositions > 0 ? offer.AvailablePositions : 1,
                    ApplicationCount = apps.Count,
                    IsActive = offer.IsActive,
                    CreatedDate = offer.CreatedAt
                });
            }

            var model = new JobPostingListViewModel
            {
                SearchTerm = searchTerm ?? string.Empty,
                TypeFilter = typeFilter,
                IsActiveFilter = isActiveFilter,
                JobPostings = list.OrderByDescending(j => j.CreatedDate).ToList()
            };
            return View("~/Views/Company/JobPostings/Index.cshtml", model);
        }

        // Oluþtur / Düzenle (GET)
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int? id) => await LoadCreateEditModel(id);

        // Oluþtur / Düzenle (POST)
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobPostingCreateViewModel model) => await SaveCreateEditModel(model);

        private async Task<IActionResult> LoadCreateEditModel(int? id)
        {
            var model = new JobPostingCreateViewModel
            {
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(90),
                ApplicationDeadline = DateTime.UtcNow.AddDays(15)
            };
            model.AvailablePeriods = await _periodService.GetActivePeriodsAsync();
            if (!model.AvailablePeriods.Any())
            {
                var def = new InternshipPeriod
                {
                    PeriodName = "2025 Güz",
                    StartDate = DateTime.UtcNow.AddDays(30),
                    EndDate = DateTime.UtcNow.AddDays(120)
                };
                await _periodService.AddPeriodAsync(def);
                model.AvailablePeriods = await _periodService.GetActivePeriodsAsync();
            }
            model.AvailableDurations = Enum.GetValues(typeof(OfferDurationType)).Cast<OfferDurationType>().ToList();
            model.AvailableLocations = (await _provinceRepo.GetAllAsync()).ToList();

            if (id.HasValue && id.Value > 0)
            {
                var offer = await _offerService.GetByIdAsync(id.Value);
                var companyId = await GetCurrentCompanyIdAsync();
                if (offer == null || offer.CompanyId != companyId)
                {
                    TempData["ErrorMessage"] = "Ýlan bulunamadý veya yetkiniz yok.";
                    return RedirectToAction("List");
                }
                model.Id = offer.Id;
                model.Title = offer.Title;
                model.Description = offer.Description ?? string.Empty;
                model.Requirements = offer.Requirements ?? string.Empty;
                model.Position = offer.Field;
                model.Quota = offer.AvailablePositions;
                model.IsActive = offer.IsActive;
                model.ApplicationDeadline = offer.ApplicationDeadline ?? DateTime.UtcNow.AddDays(15);
                model.Duration = offer.Duration;
                model.JobType = offer.JobType switch
                {
                    OfferJobType.Job => JobPostingType.Job,
                    OfferJobType.Both => JobPostingType.Both,
                    _ => JobPostingType.Internship
                };
                model.MonthlySalary = offer.MonthlySalary;
                model.MonthlySalaryCurrency = offer.MonthlySalaryCurrency;
                var matched = model.AvailableLocations.FirstOrDefault(p => string.Equals(p.Name?.Trim(), offer.City?.Trim(), StringComparison.OrdinalIgnoreCase));
                if (matched != null) model.LocationId = matched.Id;
                if (offer.PeriodId > 0)
                {
                    // Offer uses a predefined period -> set DateMode accordingly
                    model.UsePeriod = true; model.PeriodId = offer.PeriodId;
                    model.DateMode = DateSelectionMode.Period;
                }
                else
                {
                    // Offer had explicit start/end -> treat as fixed range
                    model.UsePeriod = false;
                    model.DateMode = DateSelectionMode.FixedRange;
                    model.StartDate = offer.Period?.StartDate ?? DateTime.UtcNow.AddDays(30);
                    model.EndDate = offer.Period?.EndDate ?? DateTime.UtcNow.AddDays(90);
                }
            }
            return View("~/Views/Company/JobPostings/Create.cshtml", model);
        }

        private async Task<IActionResult> SaveCreateEditModel(JobPostingCreateViewModel model)
        {
            try
            {
                model.AvailableDurations = Enum.GetValues(typeof(OfferDurationType)).Cast<OfferDurationType>().ToList();
                model.AvailableLocations = (await _provinceRepo.GetAllAsync()).ToList();
                model.AvailablePeriods = await _periodService.GetActivePeriodsAsync();

                // Ensure server uses DateMode to determine whether to use a period or manual dates
                model.UsePeriod = model.DateMode == DateSelectionMode.Period;

                if (!ModelState.IsValid)
                {
                    // Collect model state errors to show to the user
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage)
                        .Where(m => !string.IsNullOrWhiteSpace(m))
                        .ToList();
                    if (errors.Any()) TempData["ErrorMessage"] = string.Join("; ", errors);
                    return View("~/Views/Company/JobPostings/Create.cshtml", model);
                }

                DateTime start; DateTime end;
                if (model.UsePeriod)
                {
                    if (model.PeriodId <= 0)
                    {
                        ModelState.AddModelError("PeriodId", "Dönem seçimi zorunludur.");
                        TempData["ErrorMessage"] = "Dönem seçimi zorunludur.";
                        return View("~/Views/Company/JobPostings/Create.cshtml", model);
                    }
                    var period = await _periodService.GetPeriodByIdAsync(model.PeriodId);
                    if (period == null)
                    {
                        ModelState.AddModelError("PeriodId", "Seçilen dönem bulunamadý.");
                        TempData["ErrorMessage"] = "Seçilen dönem bulunamadý.";
                        return View("~/Views/Company/JobPostings/Create.cshtml", model);
                    }
                    start = period.StartDate; end = period.EndDate;
                }
                else
                {
                    start = model.StartDate; end = model.EndDate;
                    if (end <= start)
                    {
                        ModelState.AddModelError("EndDate", "Bitiþ tarihi baþlangýçtan sonra olmalýdýr.");
                        TempData["ErrorMessage"] = "Bitiþ tarihi baþlangýçtan sonra olmalýdýr.";
                        return View("~/Views/Company/JobPostings/Create.cshtml", model);
                    }
                }
                if (model.ApplicationDeadline >= start)
                {
                    ModelState.AddModelError("ApplicationDeadline", "Son baþvuru tarihi baþlangýçtan önce olmalýdýr.");
                    TempData["ErrorMessage"] = "Son baþvuru tarihi baþlangýçtan önce olmalýdýr.";
                    return View("~/Views/Company/JobPostings/Create.cshtml", model);
                }
                var companyId = await GetCurrentCompanyIdAsync();
                if (companyId <= 0)
                {
                    TempData["ErrorMessage"] = "Þirket hesabýnýz bulunamadý.";
                    return View("~/Views/Company/JobPostings/Create.cshtml", model);
                }
                var cityName = (await _provinceRepo.GetByIdAsync(model.LocationId))?.Name ?? string.Empty;

                if (model.Id > 0)
                {
                    var existing = await _offerService.GetByIdAsync(model.Id);
                    if (existing == null || existing.CompanyId != companyId)
                    {
                        TempData["ErrorMessage"] = "Ýlan bulunamadý veya yetkiniz yok.";
                        return RedirectToAction("List");
                    }
                    existing.Title = model.Title;
                    existing.Description = model.Description;
                    existing.Requirements = model.Requirements;
                    existing.City = cityName;
                    existing.Field = model.Position;
                    existing.AvailablePositions = model.Quota;
                    existing.IsActive = model.IsActive;
                    existing.ApplicationDeadline = model.ApplicationDeadline.ToUniversalTime();
                    existing.Duration = model.Duration.GetValueOrDefault(OfferDurationType.Kisa);
                    existing.JobType = model.JobType switch
                    {
                        JobPostingType.Job => OfferJobType.Job,
                        JobPostingType.Both => OfferJobType.Both,
                        _ => OfferJobType.Internship
                    };
                    existing.MonthlySalary = model.MonthlySalary;
                    existing.MonthlySalaryCurrency = model.MonthlySalaryCurrency;
                    if (model.UsePeriod)
                        existing.PeriodId = model.PeriodId;
                    else
                    {
                        var p = new InternshipPeriod
                        {
                            PeriodName = $"{model.StartDate:yyyy-MM-dd} - {model.EndDate:yyyy-MM-dd}",
                            StartDate = DateTime.SpecifyKind(model.StartDate.Date, DateTimeKind.Utc),
                            EndDate = DateTime.SpecifyKind(model.EndDate.Date, DateTimeKind.Utc)
                        };
                        var added = await _periodService.AddPeriodAsync(p);
                        existing.PeriodId = added.PeriodId;
                    }
                    await _offerService.UpdateOfferAsync(existing);
                    TempData["CompanySuccessMessage"] = "Ýlan güncellendi.";
                    return RedirectToAction("List");
                }

                var offer = new InternshipOffer
                {
                    Title = model.Title,
                    Description = model.Description,
                    Requirements = model.Requirements,
                    City = cityName,
                    Field = model.Position,
                    AvailablePositions = model.Quota,
                    IsActive = model.IsActive,
                    CompanyId = companyId,
                    Duration = model.Duration.GetValueOrDefault(OfferDurationType.Kisa),
                    JobType = model.JobType switch
                    {
                        JobPostingType.Job => OfferJobType.Job,
                        JobPostingType.Both => OfferJobType.Both,
                        _ => OfferJobType.Internship
                    },
                    MonthlySalary = model.MonthlySalary,
                    MonthlySalaryCurrency = model.MonthlySalaryCurrency,
                    CreatedAt = DateTime.UtcNow
                };

                // normalize application deadline to UTC
                var appDeadline = model.ApplicationDeadline;
                if (appDeadline.Kind == DateTimeKind.Unspecified)
                    appDeadline = DateTime.SpecifyKind(appDeadline, DateTimeKind.Utc);
                else if (appDeadline.Kind == DateTimeKind.Local)
                    appDeadline = appDeadline.ToUniversalTime();

                offer.ApplicationDeadline = appDeadline;

                if (model.UsePeriod) offer.PeriodId = model.PeriodId; else
                {
                    var np = new InternshipPeriod
                    {
                        PeriodName = $"{model.StartDate:yyyy-MM-dd} - {model.EndDate:yyyy-MM-dd}",
                        StartDate = DateTime.SpecifyKind(model.StartDate.Date, DateTimeKind.Utc),
                        EndDate = DateTime.SpecifyKind(model.EndDate.Date, DateTimeKind.Utc)
                    };
                    var added = await _periodService.AddPeriodAsync(np);
                    offer.PeriodId = added.PeriodId;
                }
                await _offerService.AddOfferAsync(offer);
                TempData["CompanySuccessMessage"] = "Ýlan oluþturuldu.";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                // Surface exception to user and keep them on the form for correction
                TempData["CompanyErrorMessage"] = "Kaydetme sýrasýnda hata: " + ex.Message;
                return View("~/Views/Company/JobPostings/Create.cshtml", model);
            }
        }

        // Durum deðiþtir
        [HttpPost("ToggleStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleJobStatus(int jobId, bool isActive)
        {
            try
            {
                var companyId = await GetCurrentCompanyIdAsync();
                var offer = await _offerService.GetByIdAsync(jobId);
                if (offer == null || offer.CompanyId != companyId)
                    return Json(new { success = false, message = "Ýlan bulunamadý veya yetkiniz yok." });
                offer.IsActive = isActive;
                await _offerService.UpdateOfferAsync(offer);
                return Json(new { success = true, message = "Ýlan durumu güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Sil
        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJobPosting(int jobId)
        {
            try
            {
                var companyId = await GetCurrentCompanyIdAsync();
                var offer = await _offerService.GetByIdAsync(jobId);
                if (offer == null || offer.CompanyId != companyId)
                    return Json(new { success = false, message = "Ýlan bulunamadý veya yetkiniz yok." });
                var apps = await _applicationService.GetApplicationsByOfferIdAsync(jobId);
                if (apps.Any())
                    return Json(new { success = false, message = "Bu ilana baþvuru olduðu için silinemez." });
                await _offerService.DeleteOfferAsync(jobId);
                return Json(new { success = true, message = "Ýlan silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Hýzlý AJAX güncelleme
        [HttpPost("UpdateAjax")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateJobPostingAjax()
        {
            try
            {
                var form = Request.Form;
                if (!int.TryParse(form["Id"], out var id) || id <= 0)
                    return Json(new { success = false, message = "Geçersiz ilan ID" });
                var existing = await _offerService.GetByIdAsync(id);
                var companyId = await GetCurrentCompanyIdAsync();
                if (existing == null || existing.CompanyId != companyId)
                    return Json(new { success = false, message = "Ýlan bulunamadý veya yetkiniz yok" });
                existing.Title = form["Title"].ToString();
                existing.Field = form["Position"].ToString();
                if (int.TryParse(form["Quota"], out var q)) existing.AvailablePositions = q;
                if (decimal.TryParse(form["MonthlySalary"], out var sal)) existing.MonthlySalary = sal;
                if (DateTime.TryParse(form["ApplicationDeadline"], out var d)) existing.ApplicationDeadline = DateTime.SpecifyKind(d, DateTimeKind.Utc);
                existing.Description = form["Description"].ToString();
                existing.IsActive = form["IsActive"].FirstOrDefault() == "on";
                if (int.TryParse(form["PeriodId"], out var pid) && pid > 0) existing.PeriodId = pid;
                await _offerService.UpdateOfferAsync(existing);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Detay partial
        [HttpGet("Detail")]
        public async Task<IActionResult> JobPostingDetail(int id)
        {
            var offer = await _offerService.GetByIdAsync(id);
            var companyId = await GetCurrentCompanyIdAsync();
            if (offer == null || offer.CompanyId != companyId)
                return Content("Ýlan bulunamadý veya yetkiniz yok.");
            var apps = await _applicationService.GetApplicationsByOfferIdAsync(offer.Id);
            var model = new JobPostingDetailViewModel
            {
                Id = offer.Id,
                Title = offer.Title,
                Position = offer.Field,
                JobType = offer.JobType switch
                {
                    OfferJobType.Job => JobPostingType.Job,
                    OfferJobType.Both => JobPostingType.Both,
                    _ => JobPostingType.Internship
                },
                Location = offer.City,
                Salary = offer.MonthlySalary,
                SalaryCurrency = offer.MonthlySalaryCurrency,
                StartDate = offer.Period?.StartDate ?? DateTime.UtcNow,
                EndDate = offer.Period?.EndDate ?? DateTime.UtcNow.AddDays(60),
                ApplicationDeadline = offer.ApplicationDeadline ?? DateTime.UtcNow.AddDays(30),
                Quota = offer.AvailablePositions > 0 ? offer.AvailablePositions : 1,
                ApplicationCount = apps.Count,
                IsActive = offer.IsActive,
                CreatedDate = offer.CreatedAt,
                Description = offer.Description ?? string.Empty,
                Requirements = offer.Requirements ?? string.Empty,
                CompanyName = (await _companyService.GetCompanyByIdAsync(offer.CompanyId))?.CompanyName ?? string.Empty
            };
            return PartialView("~/Views/Company/JobPostings/_JobPostingDetailPartial.cshtml", model);
        }

        // Ýstatistik
        [HttpGet("Stats")]
        public async Task<IActionResult> GetOfferApplicationStats()
        {
            try
            {
                var companyId = await GetCurrentCompanyIdAsync();
                var offers = await _offerService.GetOffersByCompanyIdAsync(companyId);
                var result = new List<object>();
                foreach (var offer in offers)
                {
                    var apps = await _applicationService.GetApplicationsByOfferIdAsync(offer.Id);
                    result.Add(new
                    {
                        OfferId = offer.Id,
                        OfferTitle = offer.Title,
                        ApplicationsCount = apps.Count,
                        ApplicationStatuses = apps.GroupBy(a => a.Status).ToDictionary(g => g.Key.ToString(), g => g.Count())
                    });
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Dönem ekleme
        [HttpPost("AddPeriod")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPeriodAjax([FromBody] AddPeriodRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.PeriodName) || req.StartDate == default || req.EndDate == default)
                    return Json(new { success = false, message = "Eksik alan" });
                if (req.EndDate <= req.StartDate) return Json(new { success = false, message = "Bitiþ tarihi baþlangýçtan sonra olmalý" });
                var period = new InternshipPeriod
                {
                    PeriodName = req.PeriodName.Trim(),
                    StartDate = DateTime.SpecifyKind(req.StartDate.Date, DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(req.EndDate.Date, DateTimeKind.Utc)
                };
                var added = await _periodService.AddPeriodAsync(period);
                return Json(new
                {
                    success = true,
                    periodId = added.PeriodId,
                    periodName = added.PeriodName,
                    start = added.StartDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                    end = added.EndDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class AddPeriodRequest
    {
        public string PeriodName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
