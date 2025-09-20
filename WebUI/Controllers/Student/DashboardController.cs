using Business.Interfaces.Chat;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Business.Interfaces.Order;
using Business.Interfaces; // for IDiaryService
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebUI.Controllers;
using WebUI.Models.ViewModels.Student.ViewModels;
using Core.Entity;

namespace WebUI.Controllers.Student
{
    [Authorize]
    public class DashboardController : BaseAppController
    {
        private readonly IIUserService _userService;
        private readonly IInternshipApplicationService _applicationService;
        private readonly IInternshipDocumentService _documentService;
        private readonly IInternshipPeriodService _periodService;
        private readonly IINotificationService _notificationService;
        private readonly IIChatService _chatService;
        private readonly IDiaryService _diaryService;
        private readonly IInternshipDiaryService _internshipDiaryService;

        public DashboardController(
            IIUserService userService,
            IInternshipApplicationService applicationService,
            IInternshipDocumentService documentService,
            IInternshipPeriodService periodService,
            IINotificationService notificationService,
            IIChatService chatService,
            IDiaryService diaryService,
            IInternshipDiaryService internshipDiaryService)
        {
            _userService = userService;
            _applicationService = applicationService;
            _documentService = documentService;
            _periodService = periodService;
            _notificationService = notificationService;
            _chatService = chatService;
            _diaryService = diaryService;
            _internshipDiaryService = internshipDiaryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try 
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetUserWithUniversityAsync(userId);
                if (user == null) 
                {
                    TempData["ErrorMessage"] = "Kullanıcı bilgileri bulunamadı.";
                    return RedirectToAction("Login", "Account");
                }

                var applications = await _applicationService.GetApplicationsByStudentIdAsync(userId);
                var notifications = await _notificationService.GetAllNotificationsByUserIdAsync(userId);
                var diaryEntries = (await _diaryService.GetDiaryEntriesByStudentIdAsync(userId)).ToList();
                var diaryDocuments = await _documentService.GetStudentDiaryEntriesAsync(userId);
                var activeInternship = applications.FirstOrDefault(a => a.Status == InternshipApplicationStatus.Approved);

                var (totalDays, completedDays) = CalculateInternshipDays(activeInternship, diaryEntries);

                // Ana defter (kullanıcı seçimi cookie) kontrolü + defter listesi
                var studentDiaries = await _internshipDiaryService.GetDiariesByStudentIdAsync(userId) ?? new List<InternshipDiary>();
                bool hasAnyDiary = studentDiaries.Any();
                if (Request.Cookies.TryGetValue("MainDiaryId", out var mainDiaryStr) && int.TryParse(mainDiaryStr, out var mainDiaryId))
                {
                    var mainDiary = await _internshipDiaryService.GetDiaryWithEntriesAsync(mainDiaryId);
                    if (mainDiary != null && mainDiary.StudentId == userId)
                    {
                        hasAnyDiary = true;
                        var mainEntries = mainDiary.Entries?.ToList() ?? new List<DiaryEntry>();
                        if (mainDiary.InternshipStartDate.HasValue && mainDiary.InternshipEndDate.HasValue)
                        {
                            totalDays = 0;
                            for (var d = mainDiary.InternshipStartDate.Value.Date; d <= mainDiary.InternshipEndDate.Value.Date; d = d.AddDays(1))
                                if (d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                                    totalDays++;
                        }
                        else if (mainDiary.TotalDays.HasValue)
                        {
                            totalDays = mainDiary.TotalDays.Value;
                        }
                        completedDays = mainEntries.Select(e => e.Date.Date).Distinct().Count();
                    }
                }

                var processStage = DetermineProcessStage(applications, activeInternship, totalDays, completedDays, hasAnyDiary);
                var processProgress = GetProcessProgress(processStage);
                var recentActivities = await GenerateRecentActivities(userId, applications, diaryEntries, notifications);
                var completedInternships = applications.Count(a => a.Status == InternshipApplicationStatus.Approved);
                var activeApplicationsCount = applications.Count(a => a.Status == InternshipApplicationStatus.Pending);
                var pendingDocuments = diaryDocuments.Count(d => d.Status == "Pending");
                var unreadNotifications = notifications.Count(n => !n.IsRead);

                var progressPercentage = 0;
                if (totalDays > 0)
                {
                    progressPercentage = Math.Min(100, (int)Math.Round(completedDays * 100.0 / totalDays));
                }
                else if (activeInternship != null && activeInternship.StartDate != default && activeInternship.EndDate != default)
                {
                    var totalInternshipDays = (activeInternship.EndDate - activeInternship.StartDate).TotalDays;
                    var passedDays = (DateTime.Now - activeInternship.StartDate).TotalDays;
                    progressPercentage = Math.Max(0, Math.Min(100, (int)((passedDays / totalInternshipDays) * 100)));
                }

                var viewModel = new DashboardViewModel
                {
                    User = user,
                    TotalInternshipDays = totalDays,
                    CompletedInternshipDays = completedDays,
                    CurrentProcessStage = processStage,
                    ProcessProgress = processProgress,
                    ProcessDescription = GetProcessDescription(processStage),
                    RecentActivities = recentActivities,
                    CompletedInternships = completedInternships,
                    ActiveApplications = activeApplicationsCount,
                    PendingDocuments = pendingDocuments,
                    UnreadNotifications = unreadNotifications,
                    ProgressPercentage = progressPercentage,
                    RecentApplications = applications.OrderByDescending(a => a.CreatedAt).Take(3).ToList(),
                    RecentNotifications = notifications.OrderByDescending(n => n.CreatedAt).Take(5).ToList(),
                    ActiveInternship = activeInternship
                };

                return View("~/Views/Student/Dashboard/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Dashboard yüklenirken bir hata oluştu: " + ex.Message;
                return View("~/Views/Student/Dashboard/Index.cshtml", new DashboardViewModel());
            }
        }

        private (int totalDays, int completedDays) CalculateInternshipDays(InternshipApplication? activeInternship, List<DiaryEntry> diaryEntries)
        {
            if (activeInternship == null) return (0, 0);
            var totalDays = 0;
            for (var d = activeInternship.StartDate.Date; d <= activeInternship.EndDate.Date; d = d.AddDays(1))
                if (d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                    totalDays++;
            var completedDays = diaryEntries.Count(d => d.Date >= activeInternship.StartDate && d.Date <= activeInternship.EndDate);
            return (totalDays, completedDays);
        }

        private string DetermineProcessStage(List<InternshipApplication> applications, InternshipApplication? activeInternship, int totalDays, int completedDays, bool hasDiary)
        {
            // Hiç başvuru yok
            if (!applications.Any()) return "Başvuru Süresinde";
            // Bekleyen başvurular varsa
            if (applications.Any(a => a.Status == InternshipApplicationStatus.Pending)) return "Firma Onay Aşamasında";
            // Onaylı başvuru yoksa komisyon/son onay süreci
            if (activeInternship == null) return hasDiary ? "Defter Hazır" : "Staj Komisyonu Onayı";

            var today = DateTime.Today;
            // Onaylı staj var; henüz başlamadı
            if (today < activeInternship.StartDate.Date) return hasDiary ? "Defter Hazır" : "Staj Başlayacak";
            // Gün hesabı %100 ise veya tarih geçtiyse tamamlandı
            if ((totalDays > 0 && completedDays >= totalDays) || today > activeInternship.EndDate.Date) return "Staj Tamamlandı";
            // Başladı ama bitmedi
            return "Staj Devam Ediyor";
        }

        private int GetProcessProgress(string stage) => stage switch
        {
            "Başvuru Süresinde" => 10,
            "Firma Onay Aşamasında" => 25,
            "Staj Komisyonu Onayı" => 40,
            "Defter Hazır" => 55,
            "Staj Başlayacak" => 65,
            "Staj Devam Ediyor" => 85,
            "Staj Tamamlandı" => 100,
            "Onaylandı" => 75,
            _ => 20
        };

        private string GetProcessDescription(string stage) => stage switch
        {
            "Başvuru Süresinde" => "Staj başvurunuzu yapabilirsiniz",
            "Firma Onay Aşamasında" => "Başvurunuz firma tarafından değerlendiriliyor",
            "Staj Komisyonu Onayı" => "Komisyon onayı bekleniyor",
            "Defter Hazır" => "Staj defteri oluşturuldu, başlangıç bekleniyor",
            "Staj Başlayacak" => "Stajınız yakında başlıyor",
            "Staj Devam Ediyor" => "Stajınız devam ediyor",
            "Staj Tamamlandı" => "Tebrikler! Stajınızı tamamladınız",
            "Onaylandı" => "Başvurunuz onaylandı",
            _ => "Staj sürecinize devam edin"
        };

        private async Task<List<ActivityItem>> GenerateRecentActivities(int userId, List<InternshipApplication> applications, List<DiaryEntry> diaryEntries, List<Notification> notifications)
        {
            var activities = new List<ActivityItem>();
            foreach (var app in applications.OrderByDescending(a => a.UpdatedAt).Take(2))
            {
                var statusText = app.Status switch
                {
                    InternshipApplicationStatus.Pending => "başvuru yapıldı",
                    InternshipApplicationStatus.Approved => "başvuru onaylandı",
                    InternshipApplicationStatus.Rejected => "başvuru reddedildi",
                    _ => "durum güncellendi"
                };
                activities.Add(new ActivityItem
                {
                    Title = $"{app.InternshipOffer?.Company?.CompanyName} {statusText}",
                    TimeText = GetTimeAgo(app.UpdatedAt),
                    Time = app.UpdatedAt,
                    Type = app.Status == InternshipApplicationStatus.Approved ? "success" : app.Status == InternshipApplicationStatus.Rejected ? "error" : "warning",
                    Icon = app.Status == InternshipApplicationStatus.Approved ? "fas fa-check-circle" : app.Status == InternshipApplicationStatus.Rejected ? "fas fa-times-circle" : "fas fa-clock",
                    CreatedAt = app.UpdatedAt
                });
            }
            foreach (var entry in diaryEntries.OrderByDescending(e => e.Date).Take(2))
            {
                activities.Add(new ActivityItem
                {
                    Title = $"Staj defteri eklendi - {entry.Date:dd MMM}",
                    TimeText = GetTimeAgo(entry.CreatedAt),
                    Time = entry.CreatedAt,
                    Type = "info",
                    Icon = "fas fa-book",
                    CreatedAt = entry.CreatedAt
                });
            }
            var important = notifications.Where(n => n.Type == "Application" || n.Type == "Document").OrderByDescending(n => n.CreatedAt).Take(1);
            foreach (var n in important)
            {
                activities.Add(new ActivityItem
                {
                    Title = n.Title,
                    TimeText = GetTimeAgo(n.CreatedAt),
                    Time = n.CreatedAt,
                    Type = n.Type == "Application" ? "info" : "warning",
                    Icon = n.IconClass ?? "fas fa-bell",
                    CreatedAt = n.CreatedAt
                });
            }
            return activities.OrderByDescending(a => a.CreatedAt).Take(4).ToList();
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var ts = DateTime.UtcNow - dateTime;
            if (ts.TotalMinutes < 1) return "Az önce";
            if (ts.TotalMinutes < 60) return $"{(int)ts.TotalMinutes} dakika önce";
            if (ts.TotalHours < 24) return $"{(int)ts.TotalHours} saat önce";
            if (ts.TotalDays < 7) return $"{(int)ts.TotalDays} gün önce";
            return dateTime.ToString("dd MMM yyyy");
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                var applications = await _applicationService.GetApplicationsByStudentIdAsync(userId);
                var notifications = await _notificationService.GetAllNotificationsByUserIdAsync(userId);
                var documents = await _documentService.GetStudentDiaryEntriesAsync(userId);
                var stats = new
                {
                    completedInternships = applications.Count(a => a.Status == InternshipApplicationStatus.Approved),
                    activeApplications = applications.Count(a => a.Status == InternshipApplicationStatus.Pending),
                    pendingDocuments = documents.Count(d => d.Status == "Pending"),
                    unreadNotifications = notifications.Count(n => !n.IsRead)
                };
                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProgress()
        {
            try
            {
                var userId = GetCurrentUserId();
                var applications = await _applicationService.GetApplicationsByStudentIdAsync(userId);
                var activeInternship = applications.FirstOrDefault(a => a.Status == InternshipApplicationStatus.Approved);
                var progressPercentage = 0;
                var statusText = "Staj başlamamış";
                if (activeInternship != null && activeInternship.StartDate != default && activeInternship.EndDate != default)
                {
                    var totalDays = (activeInternship.EndDate - activeInternship.StartDate).TotalDays;
                    var passedDays = (DateTime.Now - activeInternship.StartDate).TotalDays;
                    progressPercentage = Math.Max(0, Math.Min(100, (int)((passedDays / totalDays) * 100)));
                    statusText = progressPercentage switch
                    {
                        >= 100 => "Staj tamamlandı",
                        > 0 => $"Staj devam ediyor (%{progressPercentage})",
                        _ => "Staj başlayacak"
                    };
                }
                return Json(new { progress = progressPercentage, status = statusText });
            }
            catch (Exception ex)
            {
                return Json(new { progress = 0, status = "Hata: " + ex.Message });
            }
        }

        private async Task EnsureDailySummaryNotificationAsync(int userId, List<Notification> existing, string key, bool condition, string message)
        {
            if (!condition) return;
            var today = DateTime.UtcNow.Date;
            if (existing.Any(n => n.Type == key && n.CreatedAt.Date == today)) return;
            await _notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = "Günlük Özet",
                Message = message,
                Type = key,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                IconClass = "fas fa-info-circle"
            });
        }
    }
}