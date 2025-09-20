using Business.Interfaces;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Core.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models.ViewModels.Student;
using System.Security.Claims;
using Business.Interfaces.Order;

namespace WebUI.Controllers.Student
{
    [Authorize(Roles = "Student")]
    public class DiaryController : Controller
    {
        private readonly IInternshipApplicationService _applicationService;
        private readonly IIUserService _userService;
        private readonly IDiaryService _diaryService;
        private readonly IInternshipDiaryService _internshipDiaryService;
        private readonly IINotificationService _notificationService;

        public DiaryController(
            IInternshipApplicationService applicationService,
            IIUserService userService,
            IDiaryService diaryService,
            IInternshipDiaryService internshipDiaryService,
            IINotificationService notificationService)
        {
            _applicationService = applicationService;
            _diaryService = diaryService;
            _userService = userService;
            _internshipDiaryService = internshipDiaryService;
            _notificationService = notificationService;
        }

        // Helper to get all diary entries for a student (primary DiaryEntries first, then fall back to InternshipDiary.Entries)
        private async Task<List<DiaryEntry>> GetStudentAllDiaryEntriesAsync(int studentId)
        {
            var entries = (await _diaryService.GetDiaryEntriesByStudentIdAsync(studentId)).ToList();
            if (entries != null && entries.Any()) return entries.OrderByDescending(e => e.Date).ToList();

            // Fallback: collect from internship diaries (they include Entries)
            var diaries = await _internshipDiaryService.GetDiariesByStudentIdAsync(studentId);
            var collected = new List<DiaryEntry>();
            if (diaries != null && diaries.Any())
            {
                foreach (var d in diaries)
                {
                    if (d.Entries != null && d.Entries.Any()) collected.AddRange(d.Entries);
                }
            }

            return collected.OrderByDescending(e => e.Date).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> Index(int week = 0)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetUserByIdAsync(userId);
                var applications = await _applicationService.GetApplicationsByStudentIdAsync(userId);
                var activeInternship = applications.FirstOrDefault(a => a.Status == InternshipApplicationStatus.Approved);

                // Gerçek staj defteri girdilerini veritabanýndan al (fallback to InternshipDiary if none)
                var diaryEntities = await GetStudentAllDiaryEntriesAsync(userId);
                var diaryEntries = diaryEntities
                    .OrderByDescending(d => d.Date)
                    .Select(MapToVm)
                    .ToList();

                // Ýstatistikleri hesapla
                var stats = CalculateStats(diaryEntries, activeInternship?.StartDate, activeInternship?.EndDate);
                
                // Haftalýk view için
                var weeklyEntries = GetWeeklyEntries(diaryEntries, week);

                var vm = new DiaryListViewModel
                {
                    DiaryEntries = diaryEntries,
                    CreateModel = new DiaryCreateViewModel { Date = DateTime.Today },
                    Stats = stats,
                    WeeklyEntries = weeklyEntries,
                    CurrentWeek = week,
                    TotalWeeks = CalculateTotalWeeks(activeInternship?.StartDate ?? DateTime.Today, activeInternship?.EndDate ?? DateTime.Today.AddDays(60)),
                    InternshipStartDate = activeInternship?.StartDate ?? DateTime.Today,
                    InternshipEndDate = activeInternship?.EndDate ?? DateTime.Today.AddDays(60),
                    CompanyName = activeInternship?.InternshipOffer?.Company?.CompanyName ?? "-",
                    Department = activeInternship?.InternshipOffer?.Field ?? "-"
                };
                // Allow creating diary entries only if there is an approved internship
                vm.CanCreateDiary = activeInternship != null;

                // If there is an approved application, try to find or create an InternshipDiary for it
                if (activeInternship != null)
                {
                    try
                    {
                        // Try to find an existing diary linked to this student's approved application
                        var diaries = await _internshipDiaryService.GetDiariesByStudentIdAsync(userId);
                        var linkedDiary = diaries.FirstOrDefault(d => d.InternshipStartDate.HasValue && d.InternshipStartDate.Value == activeInternship.StartDate && d.InternshipEndDate.HasValue && d.InternshipEndDate.Value == activeInternship.EndDate);
                        if (linkedDiary == null)
                        {
                            // create a diary automatically for the approved internship
                            var newDiary = new Core.Entity.InternshipDiary
                            {
                                StudentId = userId,
                                Title = activeInternship.InternshipOffer?.Title ?? "Staj Defteri",
                                CompanyName = activeInternship.InternshipOffer?.Company?.CompanyName,
                                Department = activeInternship.InternshipOffer?.Field,
                                InternshipStartDate = activeInternship.StartDate,
                                InternshipEndDate = activeInternship.EndDate,
                                ApplicationId = activeInternship.Id,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                IsRequired = true,
                                IsCompleted = false
                            };
                            var created = await _internshipDiaryService.CreateDiaryAsync(newDiary);
                            if (created != null)
                            {
                                vm.ActiveDiaryId = created.Id;

                                // send notification to advisor if available
                                try
                                {
                                    var advisorId = activeInternship?.AdvisorId ?? 0;
                                    if (advisorId <= 0)
                                    {
                                        // try to find advisor from user profile if present
                                        var studentWithUni = await _userService.GetUserWithUniversityAsync(userId);
                                        // studentWithUni may have AdvisorId or Advisor navigation depending on model
                                        // best-effort: if property exists named AdvisorId, use reflection
                                        if (studentWithUni != null)
                                        {
                                            var prop = studentWithUni.GetType().GetProperty("AdvisorId");
                                            if (prop != null)
                                            {
                                                var val = prop.GetValue(studentWithUni);
                                                if (val is int aid && aid > 0) advisorId = aid;
                                            }
                                        }
                                    }

                                    if (advisorId > 0)
                                    {
                                        await _notificationService.CreateNotificationAsync(new Notification
                                        {
                                            UserId = advisorId,
                                            Title = "Yeni Staj Defteri Oluþturuldu",
                                            Message = $"{user.FullName} için yeni bir staj defteri oluþturuldu. Baþlýk: {created.Title}",
                                            Type = "Diary",
                                            IsRead = false,
                                            CreatedAt = DateTime.UtcNow
                                        });
                                    }
                                }
                                catch
                                {
                                    // swallow notification errors
                                }
                            }
                        }
                        else
                        {
                            vm.ActiveDiaryId = linkedDiary.Id;
                        }
                    }
                    catch { }
                }

                // Find approved applications without an associated diary to show in "cart"
                try
                {
                    var allApps = await _applicationService.GetApplicationsByStudentIdAsync(userId);
                    var approved = allApps.Where(a => a.Status == InternshipApplicationStatus.Approved).ToList();
                    var diaries = await _internshipDiaryService.GetDiariesByStudentIdAsync(userId);
                    var appsWithoutDiary = approved.Where(a => !diaries.Any(d => d.ApplicationId.HasValue && d.ApplicationId.Value == a.Id))
                                                 .Select(a => new WebUI.Models.ViewModels.Student.ApplicationItemVM
                                                 {
                                                     Id = a.Id,
                                                     CompanyName = a.InternshipOffer?.Company?.CompanyName ?? "",
                                                     Position = a.InternshipOffer?.Title ?? "",
                                                     StartDate = a.StartDate,
                                                     EndDate = a.EndDate,
                                                     Status = a.Status,
                                                     AppliedDate = a.CreatedAt,
                                                     Description = a.StudentNote ?? "",
                                                 }).ToList();

                    vm.EligibleApplicationsForDiary = appsWithoutDiary;
                }
                catch { }

                return View("~/Views/Student/Diary/Index.cshtml", vm);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Staj defteri yüklenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Student/Diary/Index.cshtml", new DiaryListViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEntry(DiaryCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm gerekli alanlarý doldurun.";
                return RedirectToAction(nameof(Index));
            }
            // Only allow creating entries if student has an approved internship
            var userId = GetCurrentUserId();
            var applications = await _applicationService.GetApplicationsByStudentIdAsync(userId);
            var activeInternship = applications.FirstOrDefault(a => a.Status == InternshipApplicationStatus.Approved);
            if (activeInternship == null)
            {
                TempData["ErrorMessage"] = "Staj defteri oluþturmak için onaylanmýþ bir staj baþvurunuz olmalýdýr.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                // Find or create the InternshipDiary linked to the approved application
                int diaryId = 0;
                try
                {
                    var diaries = await _internshipDiaryService.GetDiariesByStudentIdAsync(userId);
                    var linkedDiary = diaries.FirstOrDefault(d => d.InternshipStartDate.HasValue && d.InternshipStartDate.Value == activeInternship.StartDate && d.InternshipEndDate.HasValue && d.InternshipEndDate.Value == activeInternship.EndDate);
                    if (linkedDiary == null)
                    {
                        var newDiary = new Core.Entity.InternshipDiary
                        {
                            StudentId = userId,
                            Title = activeInternship.InternshipOffer?.Title ?? "Staj Defteri",
                            CompanyName = activeInternship.InternshipOffer?.Company?.CompanyName,
                            Department = activeInternship.InternshipOffer?.Field,
                            InternshipStartDate = activeInternship.StartDate,
                            InternshipEndDate = activeInternship.EndDate,
                            ApplicationId = activeInternship.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            IsRequired = true,
                            IsCompleted = false
                        };
                        var created = await _internshipDiaryService.CreateDiaryAsync(newDiary);
                        diaryId = created?.Id ?? 0;

                    if (created != null)
                    {
                        try
                        {
                            var advisorId = activeInternship?.AdvisorId ?? 0;
                            if (advisorId > 0)
                            {
                                var studentEntity = await _userService.GetUserByIdAsync(userId);
                                await _notificationService.CreateNotificationAsync(new Notification
                                {
                                    UserId = advisorId,
                                    Title = "Yeni Staj Defteri Oluþturuldu",
                                    Message = $"{studentEntity?.FullName ?? "Öðrenci"} için yeni bir staj defteri oluþturuldu.",
                                    Type = "Diary",
                                    IsRead = false,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                        catch { }
                    }
                    }
                    else
                    {
                        diaryId = linkedDiary.Id;
                    }
                }
                catch
                {
                    diaryId = 0;
                }

                if (diaryId <= 0)
                {
                    TempData["ErrorMessage"] = "Staj defteri bulunamadý veya oluþturulamadý.";
                    return RedirectToAction(nameof(Index));
                }

                var entity = new DiaryEntry
                {
                    StudentId = userId,
                    DiaryId = diaryId,
                    Title = model.Title,
                    Content = model.Content,
                    Date = DateTime.SpecifyKind(model.Date, DateTimeKind.Utc),
                    CreatedAt = DateTime.UtcNow,
                    WorkingHours = model.WorkingHours,
                    Category = (int)model.Category,
                    Notes = model.Notes,
                    LearnedTopics = model.LearnedTopics,
                    Challenges = model.Challenges,
                    Solutions = model.Solutions,
                    IsApproved = false
                };

                await _internshipDiaryService.AddEntryToDiaryAsync(entity);
                TempData["SuccessMessage"] = "Staj defteri girdisi baþarýyla oluþturuldu.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Girdi oluþturulurken hata oluþtu: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> WeeklyReport(int week)
        {
            try
            {
                var userId = GetCurrentUserId();
                var diaryEntities = await GetStudentAllDiaryEntriesAsync(userId);
                var vmEntries = diaryEntities.Select(MapToVm).ToList();
                var weeklyEntries = GetWeeklyEntries(vmEntries, week);

                var startDate = DateTime.Today.AddDays(-7 * (week + 1));
                var endDate = startDate.AddDays(6);

                var weeklyReport = new DiaryWeeklyReportViewModel
                {
                    WeekNumber = week + 1,
                    WeekStartDate = startDate,
                    WeekEndDate = endDate,
                    StartDate = startDate,
                    EndDate = endDate,
                    Entries = weeklyEntries,
                    TotalHours = weeklyEntries.Sum(e => e.WorkingHours),
                    Summary = GenerateWeeklySummary(weeklyEntries),
                    WeeklySummary = GenerateWeeklySummary(weeklyEntries),
                    Achievements = "Bu hafta baþarýyla tamamlanan görevler.",
                    NextWeekGoals = "Gelecek hafta için hedefler."
                };

                return View("~/Views/Student/Diary/WeeklyReport.cshtml", weeklyReport);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Haftalýk rapor yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            try
            {
                var entry = await _diaryService.GetByIdAsync(id);
                var userId = GetCurrentUserId();
                if (entry == null || entry.StudentId != userId)
                {
                    TempData["ErrorMessage"] = "Girdi bulunamadý veya yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }
                await _diaryService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Staj defteri girdisi silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Girdi silinirken hata oluþtu: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ExportPdf()
        {
            TempData["InfoMessage"] = "PDF export özelliði yakýnda eklenecek.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetEntryDetail(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var entry = await _diaryService.GetByIdAsync(id);
                // If not found in primary entries, try to find in internship diary entries
                if (entry == null)
                {
                    var diaries = await _internshipDiaryService.GetDiariesByStudentIdAsync(userId);
                    if (diaries != null)
                    {
                        foreach (var d in diaries)
                        {
                            var found = d.Entries?.FirstOrDefault(e => e.Id == id);
                            if (found != null)
                            {
                                entry = found;
                                break;
                            }
                        }
                    }
                }
                
                 if (entry == null || entry.StudentId != userId)
                 {
                     return NotFound("Girdi bulunamadý veya yetkiniz yok.");
                 }

                var viewModel = MapToVm(entry);
                return PartialView("_DiaryEntryDetail", viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest("Detay yüklenirken hata oluþtu: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Cards(int week = 0)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetUserByIdAsync(userId);
                var applications = await _applicationService.GetApplicationsByStudentIdAsync(userId);
                var activeInternship = applications.FirstOrDefault(a => a.Status == InternshipApplicationStatus.Approved);

                // Gerçek staj defteri girdilerini veritabanýndan al (fallback to InternshipDiary if none)
                var diaryEntities = await GetStudentAllDiaryEntriesAsync(userId);
                var diaryEntries = diaryEntities
                    .OrderByDescending(d => d.Date)
                    .Select(MapToVm)
                    .ToList();

                // Ýstatistikleri hesapla
                var stats = CalculateStats(diaryEntries, activeInternship?.StartDate, activeInternship?.EndDate);
                
                // Haftalýk view için
                var weeklyEntries = GetWeeklyEntries(diaryEntries, week);

                var vm = new DiaryListViewModel
                {
                    DiaryEntries = diaryEntries,
                    CreateModel = new DiaryCreateViewModel { Date = DateTime.Today },
                    Stats = stats,
                    WeeklyEntries = weeklyEntries,
                    CurrentWeek = week,
                    TotalWeeks = CalculateTotalWeeks(activeInternship?.StartDate ?? DateTime.Today, activeInternship?.EndDate ?? DateTime.Today.AddDays(60)),
                    InternshipStartDate = activeInternship?.StartDate ?? DateTime.Today,
                    InternshipEndDate = activeInternship?.EndDate ?? DateTime.Today.AddDays(60),
                    CompanyName = activeInternship?.InternshipOffer?.Company?.CompanyName ?? "-",
                    Department = activeInternship?.InternshipOffer?.Field ?? "-"
                };
                vm.CanCreateDiary = activeInternship != null;

                return View("~/Views/Student/Diary/Cards.cshtml", vm);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Staj defteri yüklenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Student/Diary/Cards.cshtml", new DiaryListViewModel());
            }
        }

        // Mapping & helpers
        private DiaryEntryViewModel MapToVm(DiaryEntry e) => new()
        {
            Id = e.Id,
            StudentId = e.StudentId,
            Title = e.Title,
            Content = e.Content,
            Date = e.Date,
            CreatedAt = e.CreatedAt,
            WorkingHours = e.WorkingHours,
            Category = (DiaryCategory)e.Category,
            Notes = e.Notes ?? string.Empty,
            LearnedTopics = e.LearnedTopics ?? string.Empty,
            Challenges = e.Challenges ?? string.Empty,
            Solutions = e.Solutions ?? string.Empty,
            IsApproved = e.IsApproved,
            AdvisorFeedback = e.AdvisorFeedback ?? string.Empty,
            ApprovedAt = e.ApprovedAt
        };

        private DiaryStatsViewModel CalculateStats(List<DiaryEntryViewModel> entries, DateTime? start, DateTime? end)
        {
            var totalEntries = entries.Count;
            var totalHours = entries.Sum(e => e.WorkingHours);
            var categoryStats = entries.GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.Count());

            var totalPlannedDays = start.HasValue && end.HasValue ? (end.Value.Date - start.Value.Date).TotalDays + 1 : 0;
            var uniqueDays = entries.Select(e => e.Date.Date).Distinct().Count();

            return new DiaryStatsViewModel
            {
                TotalEntries = totalEntries,
                ApprovedEntries = entries.Count(e => e.IsApproved),
                PendingEntries = entries.Count(e => !e.IsApproved),
                TotalHours = totalHours,
                TotalWorkingHours = totalHours,
                TotalDays = uniqueDays,
                WeeklyAverageHours = totalEntries > 0 ? totalHours / Math.Max(1, (int)(totalPlannedDays / 7)) : 0,
                AverageDailyHours = uniqueDays > 0 ? totalHours / uniqueDays : 0,
                CompletionRate = totalPlannedDays > 0 ? (decimal)(uniqueDays / totalPlannedDays * 100) : 0,
                CompletionPercentage = totalPlannedDays > 0 ? (int)Math.Min(100, (uniqueDays / totalPlannedDays) * 100) : 0,
                FirstEntryDate = entries.OrderBy(e => e.Date).FirstOrDefault()?.Date,
                LastEntryDate = entries.OrderByDescending(e => e.Date).FirstOrDefault()?.Date,
                CategoryDistribution = categoryStats
            };
        }

        private int CalculateTotalWeeks(DateTime start, DateTime end)
        {
            var totalDays = (end - start).TotalDays;
            return (int)Math.Ceiling(totalDays / 7);
        }

        private List<DiaryEntryViewModel> GetWeeklyEntries(List<DiaryEntryViewModel> allEntries, int week)
        {
            if (week < 0) return new List<DiaryEntryViewModel>();

            var startDate = DateTime.Today.AddDays(-7 * (week + 1));
            var endDate = startDate.AddDays(6);

            return allEntries.Where(e => e.Date >= startDate && e.Date <= endDate).ToList();
        }

        private string GenerateWeeklySummary(List<DiaryEntryViewModel> entries)
        {
            var totalHours = entries.Sum(e => e.WorkingHours);
            var approvedEntries = entries.Count(e => e.IsApproved);
            var pendingEntries = entries.Count(e => !e.IsApproved);
            var uniqueDates = entries.Select(e => e.Date.Date).Distinct().Count();

            return $"Bu hafta toplam {totalHours} saatlik çalýþma yapýldý. ({approvedEntries} onaylý, {pendingEntries} beklemede)";
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}
