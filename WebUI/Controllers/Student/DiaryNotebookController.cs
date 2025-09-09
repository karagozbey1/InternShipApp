using Business.Interfaces;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Core.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models.ViewModels.Student;
using StudentViewModels = WebUI.Models.ViewModels.Student.ViewModels;

namespace WebUI.Controllers.Student
{
    [Authorize(Roles = "Student")]
    public class DiaryNotebookController : Controller
    {
        private readonly IInternshipDiaryService _diaryService;
        private readonly IDiaryService _entryService;
        private readonly IIUserService _userService;
        private readonly IInternshipApplicationService _applicationService;

        public DiaryNotebookController(
            IInternshipDiaryService diaryService,
            IDiaryService entryService,
            IIUserService userService,
            IInternshipApplicationService applicationService)
        {
            _diaryService = diaryService;
            _entryService = entryService;
            _userService = userService;
            _applicationService = applicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                var diaries = await _diaryService.GetDiariesByStudentIdAsync(userId);

                var viewModel = new StudentViewModels.DiaryNotebookListViewModel
                {
                    Diaries = diaries.Select(d => new StudentViewModels.DiaryNotebookViewModel
                    {
                        Id = d.Id,
                        Title = d.Title,
                        Description = d.Description,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt,
                        CompanyName = d.CompanyName,
                        Department = d.Department,
                        InternshipStartDate = d.InternshipStartDate,
                        InternshipEndDate = d.InternshipEndDate,
                        TotalDays = d.TotalDays,
                        IsRequired = d.IsRequired,
                        IsCompleted = d.IsCompleted,
                        TotalEntries = d.Entries?.Count ?? 0,
                        CompletedDays = d.Entries?.Select(e => e.Date.Date).Distinct().Count() ?? 0,
                        LastEntryDate = d.Entries?.OrderByDescending(e => e.Date).FirstOrDefault()?.Date,
                        CompletionPercentage = CalculateCompletionPercentage(d)
                    }).ToList()
                };

                return View("~/Views/Student/DiaryNotebook/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Defterler yüklenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Student/DiaryNotebook/Index.cshtml", new StudentViewModels.DiaryNotebookListViewModel());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Student/DiaryNotebook/Create.cshtml", new StudentViewModels.CreateDiaryNotebookViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentViewModels.CreateDiaryNotebookViewModel model, int? applicationId)
        {
            try
            {
                var userId = GetCurrentUserId();

                // If an applicationId is provided, try to load application and prefill model fields
                if (applicationId.HasValue)
                {
                    var app = await _applicationService.GetByIdAsync(applicationId.Value);
                    if (app == null || app.StudentId != userId)
                    {
                        TempData["ErrorMessage"] = "Geçersiz baþvuru bilgisi.";
                        return RedirectToAction(nameof(Index));
                    }

                    if (app.Status != InternshipApplicationStatus.Approved)
                    {
                        TempData["ErrorMessage"] = "Sadece onaylanmýþ baþvurular için defter oluþturulabilir.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Prefill model values from application when they are empty
                    if (string.IsNullOrWhiteSpace(model.Title))
                        model.Title = app.InternshipOffer?.Title ?? "Staj Defteri";

                    if (string.IsNullOrWhiteSpace(model.CompanyName))
                        model.CompanyName = app.InternshipOffer?.Company?.CompanyName;

                    if (string.IsNullOrWhiteSpace(model.Department))
                        model.Department = app.InternshipOffer?.Field;

                    if (!model.InternshipStartDate.HasValue || model.InternshipStartDate == default)
                        model.InternshipStartDate = app.StartDate;

                    if (!model.InternshipEndDate.HasValue || model.InternshipEndDate == default)
                        model.InternshipEndDate = app.EndDate;

                    // store application id in a temp field on model if available
                    model.ApplicationId = applicationId.Value;
                }

                // Boolean handling for IsRequired field
                if (Request.Form.ContainsKey("IsRequired"))
                {
                    var isRequiredValues = Request.Form["IsRequired"].ToArray();
                    model.IsRequired = isRequiredValues.Any(v => v == "true" || v == "on");
                }

                // ModelState cleanup
                if (ModelState.ContainsKey("IsRequired"))
                {
                    ModelState.Remove("IsRequired");
                }

                // Title validation
                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    ModelState.AddModelError("Title", "Defter baþlýðý zorunludur.");
                }

                if (!ModelState.IsValid)
                {
                    return View("~/Views/Student/DiaryNotebook/Create.cshtml", model);
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanýcý bulunamadý. Lütfen tekrar giriþ yapýn.";
                    return View("~/Views/Student/DiaryNotebook/Create.cshtml", model);
                }

                // PostgreSQL için UTC DateTime
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (model.InternshipStartDate.HasValue)
                {
                    startDate = DateTime.SpecifyKind(model.InternshipStartDate.Value, DateTimeKind.Utc);
                }

                if (model.InternshipEndDate.HasValue)
                {
                    endDate = DateTime.SpecifyKind(model.InternshipEndDate.Value, DateTimeKind.Utc);
                }

                var diary = new InternshipDiary
                {
                    StudentId = userId,
                    Title = model.Title?.Trim() ?? "Staj Defteri",
                    Description = model.Description?.Trim(),
                    CompanyName = model.CompanyName?.Trim(),
                    Department = model.Department?.Trim(),
                    InternshipStartDate = startDate,
                    InternshipEndDate = endDate,
                    TotalDays = model.TotalDays,
                    IsRequired = model.IsRequired,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ApplicationId = model.ApplicationId > 0 ? model.ApplicationId : null
                };

                await _diaryService.CreateDiaryAsync(diary);
                TempData["SuccessMessage"] = "Staj defteri baþarýyla oluþturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Defter oluþturulurken hata oluþtu: " + ex.Message;
                return View("~/Views/Student/DiaryNotebook/Create.cshtml", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var diary = await _diaryService.GetDiaryWithEntriesAsync(id);

                if (diary == null)
                {
                    TempData["ErrorMessage"] = "Defter bulunamadý.";
                    return RedirectToAction(nameof(Index));
                }

                if (diary.StudentId != userId)
                {
                    TempData["ErrorMessage"] = "Bu deftere eriþim yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                var entries = diary.Entries?.Select(e => MapToEntryViewModel(e)).ToList() ?? new List<DiaryEntryViewModel>();

                var viewModel = new StudentViewModels.DiaryNotebookDetailViewModel
                {
                    Diary = new StudentViewModels.DiaryNotebookViewModel
                    {
                        Id = diary.Id,
                        Title = diary.Title,
                        Description = diary.Description,
                        CreatedAt = diary.CreatedAt,
                        UpdatedAt = diary.UpdatedAt,
                        CompanyName = diary.CompanyName,
                        Department = diary.Department,
                        InternshipStartDate = diary.InternshipStartDate,
                        InternshipEndDate = diary.InternshipEndDate,
                        TotalDays = diary.TotalDays,
                        IsRequired = diary.IsRequired,
                        IsCompleted = diary.IsCompleted,
                        TotalEntries = entries.Count,
                        CompletedDays = entries.Select(e => e.Date.Date).Distinct().Count(),
                        LastEntryDate = entries.OrderByDescending(e => e.Date).FirstOrDefault()?.Date,
                        CompletionPercentage = CalculateCompletionPercentage(diary)
                    },
                    Entries = entries.OrderBy(e => e.Date).ToList(),
                    CreateEntryModel = new DiaryCreateViewModel 
                    { 
                        Title = "",
                        Content = "",
                        Date = DateTime.Today
                    },
                    Stats = CalculateStatsForViewModels(entries, diary.InternshipStartDate, diary.InternshipEndDate)
                };

                return View("~/Views/Student/DiaryNotebook/Detail.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Defter detayý yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEntry(int diaryId, [FromForm] DiaryCreateViewModel model)
        {
            try
            {
                // Date default deðeri UTC olarak set et
                if (model.Date == default(DateTime))
                {
                    model.Date = DateTime.UtcNow.Date;
                }

                // Validation
                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    return Json(new { success = false, message = "Baþlýk alaný zorunludur." });
                }

                if (string.IsNullOrWhiteSpace(model.Content))
                {
                    return Json(new { success = false, message = "Ýçerik alaný zorunludur." });
                }

                var userId = GetCurrentUserId();
                var entry = new DiaryEntry
                {
                    DiaryId = diaryId,
                    StudentId = userId,
                    Title = model.Title.Trim(),
                    Content = model.Content.Trim(),
                    Date = DateTime.SpecifyKind(model.Date, DateTimeKind.Utc),
                    WorkingHours = model.WorkingHours,
                    Category = (int)model.Category,
                    Notes = model.Notes?.Trim(),
                    LearnedTopics = model.LearnedTopics?.Trim(),
                    Challenges = model.Challenges?.Trim(),
                    Solutions = model.Solutions?.Trim(),
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _diaryService.AddEntryToDiaryAsync(entry);
                return Json(new { success = true, message = "Defter girdisi baþarýyla eklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Girdi eklenirken hata oluþtu: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEntry(int entryId, int diaryId)
        {
            try
            {
                await _diaryService.DeleteEntryAsync(entryId);
                TempData["SuccessMessage"] = "Defter girdisi silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Girdi silinirken hata oluþtu: " + ex.Message;
            }

            return RedirectToAction(nameof(Detail), new { id = diaryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var diary = await _diaryService.GetDiaryWithEntriesAsync(id);

                if (diary == null)
                {
                    TempData["ErrorMessage"] = "Defter bulunamadý.";
                    return RedirectToAction(nameof(Index));
                }

                if (diary.StudentId != userId)
                {
                    TempData["ErrorMessage"] = "Bu defteri silme yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                await _diaryService.DeleteDiaryAsync(id);
                TempData["SuccessMessage"] = "Staj defteri silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Defter silinirken hata oluþtu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCompletion(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var diary = await _diaryService.GetDiaryWithEntriesAsync(id);

                if (diary == null || diary.StudentId != userId)
                {
                    TempData["ErrorMessage"] = "Defter bulunamadý veya yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                await _diaryService.UpdateDiaryStatusAsync(id, !diary.IsCompleted);
                TempData["SuccessMessage"] = diary.IsCompleted ? "Defter tekrar aktif hale getirildi." : "Defter tamamlandý olarak iþaretlendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Durum güncellenirken hata oluþtu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMain(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var diary = await _diaryService.GetDiaryWithEntriesAsync(id);
                if (diary == null || diary.StudentId != userId)
                {
                    return Json(new { success = false, message = "Defter bulunamadý veya yetkiniz yok." });
                }

                Response.Cookies.Append("MainDiaryId", id.ToString(), new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddMonths(1),
                    HttpOnly = false,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });

                return Json(new { success = true, message = "Ana staj defteri olarak ayarlandý." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private decimal CalculateCompletionPercentage(InternshipDiary diary)
        {
            if (diary.TotalDays == null || diary.TotalDays <= 0)
                return 0;

            var completedDays = diary.Entries?.Select(e => e.Date.Date).Distinct().Count() ?? 0;
            return Math.Round((decimal)completedDays / diary.TotalDays.Value * 100, 1);
        }

        private DiaryEntryViewModel MapToEntryViewModel(DiaryEntry entry) => new()
        {
            Id = entry.Id,
            DiaryId = entry.DiaryId,
            Title = entry.Title,
            Content = entry.Content,
            Date = entry.Date,
            CreatedAt = entry.CreatedAt,
            WorkingHours = entry.WorkingHours,
            Category = (Core.Entity.DiaryCategory)entry.Category,
            Notes = entry.Notes ?? string.Empty,
            LearnedTopics = entry.LearnedTopics ?? string.Empty,
            Challenges = entry.Challenges ?? string.Empty,
            Solutions = entry.Solutions ?? string.Empty,
            IsApproved = entry.IsApproved,
            AdvisorFeedback = entry.AdvisorFeedback ?? string.Empty,
            ApprovedAt = entry.ApprovedAt
        };

        private DiaryStatsViewModel CalculateStats(List<DiaryEntryViewModel> entries, DateTime? start, DateTime? end)
        {
            var totalEntries = entries.Count;
            var totalHours = entries.Sum(e => e.WorkingHours);
            var uniqueDays = entries.Select(e => e.Date.Date).Distinct().Count();
            var totalPlannedDays = start.HasValue && end.HasValue ? (end.Value.Date - start.Value.Date).TotalDays + 1 : 0;

            return new DiaryStatsViewModel
            {
                TotalEntries = totalEntries,
                ApprovedEntries = entries.Count(e => e.IsApproved),
                PendingEntries = entries.Count(e => !e.IsApproved),
                TotalWorkingHours = totalHours,
                TotalDays = uniqueDays,
                CompletionRate = totalPlannedDays > 0 ? (decimal)(uniqueDays / totalPlannedDays * 100) : 0,
                FirstEntryDate = entries.OrderBy(e => e.Date).FirstOrDefault()?.Date,
                LastEntryDate = entries.OrderByDescending(e => e.Date).FirstOrDefault()?.Date
            };
        }

        private DiaryStatsViewModel CalculateStatsForViewModels(List<DiaryEntryViewModel> entries, DateTime? start, DateTime? end)
        {
            var totalEntries = entries.Count;
            var totalHours = entries.Sum(e => e.WorkingHours);
            var uniqueDays = entries.Select(e => e.Date.Date).Distinct().Count();
            var totalPlannedDays = start.HasValue && end.HasValue ? (end.Value.Date - start.Value.Date).TotalDays + 1 : 0;

            return new DiaryStatsViewModel
            {
                TotalEntries = totalEntries,
                ApprovedEntries = entries.Count(e => e.IsApproved),
                PendingEntries = entries.Count(e => !e.IsApproved),
                TotalHours = totalHours,
                TotalWorkingHours = totalHours,
                TotalDays = uniqueDays,
                CompletionRate = totalPlannedDays > 0 ? (decimal)(uniqueDays / totalPlannedDays * 100) : 0,
                CompletionPercentage = totalPlannedDays > 0 ? (int)Math.Min(100, (uniqueDays / totalPlannedDays) * 100) : 0,
                AverageDailyHours = uniqueDays > 0 ? totalHours / uniqueDays : 0,
                FirstEntryDate = entries.OrderBy(e => e.Date).FirstOrDefault()?.Date,
                LastEntryDate = entries.OrderByDescending(e => e.Date).FirstOrDefault()?.Date,
                WeeklyAverageHours = 0,
                CategoryDistribution = new Dictionary<DiaryCategory, int>()
            };
        }

        private List<string> ExtractTechnologies(IEnumerable<DiaryEntryViewModel> entries)
        {
            var bag = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in entries.Select(e => e.LearnedTopics).Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                foreach (var token in t.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var key = token.Trim();
                    if (key.Length == 0) continue;
                    bag[key] = bag.ContainsKey(key) ? bag[key] + 1 : 1;
                }
            }
            return bag.OrderByDescending(k => k.Value).Take(10).Select(k => k.Key).ToList();
        }
    }
}