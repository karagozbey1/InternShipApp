using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Interfaces;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Business.Interfaces.Order;
using WebUI.Models.ViewModels.Advisor;
using WebUI.Models.ViewModels.Education;

using Core.Entity;

namespace WebUI.Controllers.Advisor
{
    [Authorize(Roles = "Advisor")]
    public class AdvisorDashboardController : Controller
    {
        private readonly IInternshipApplicationService _applicationService;
        private readonly IDiaryService _diaryService;
        private readonly IIUserService _userService;
        private readonly IINotificationService _notificationService;

        public AdvisorDashboardController(
            IInternshipApplicationService applicationService,
            IDiaryService diaryService,
            IIUserService userService,
            IINotificationService notificationService)
        {
            _applicationService = applicationService;
            _diaryService = diaryService;
            _userService = userService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var advisorId = GetCurrentUserId();
            var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
            
            if (advisor?.UniversityId == null)
            {
                TempData["ErrorMessage"] = "Danýþman bilgileri eksik.";
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new AdvisorDashboardViewModel();
            
            try
            {
                // Get advisor's students (same university)
                var allStudents = await _userService.GetAllStudentsAsync();
                var students = allStudents?.Where(s => s.UniversityId == advisor.UniversityId)?.ToList() ?? new List<User>();
                var studentIds = students.Select(s => s.Id).ToList();

                // Get statistics with error handling
                var allApplications = new List<InternshipApplication>();
                foreach (var studentId in studentIds)
                {
                    try
                    {
                        var apps = await _applicationService.GetApplicationsByStudentIdAsync(studentId);
                        if (apps != null)
                        {
                            allApplications.AddRange(apps);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting applications for student {studentId}: {ex.Message}");
                    }
                }
                
                var pendingApps = allApplications.Where(a => a.Status == InternshipApplicationStatus.Pending).ToList();
                
                var allDiaryEntries = new List<DiaryEntry>();
                foreach (var studentId in studentIds)
                {
                    try
                    {
                        var entries = await _diaryService.GetDiaryEntriesByStudentIdAsync(studentId);
                        if (entries != null)
                        {
                            allDiaryEntries.AddRange(entries);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting diary entries for student {studentId}: {ex.Message}");
                    }
                }
                
                var pendingDiaryEntries = allDiaryEntries.Where(d => !d.IsApproved).ToList();
                var completedInternships = allApplications.Where(a => a.Status == InternshipApplicationStatus.Approved).ToList();

                // Get notifications
                var notifications = await _notificationService.GetAllNotificationsByUserIdAsync(advisorId);
                var unreadNotifications = notifications.Where(n => !n.IsRead).ToList();

                // Set counts
                viewModel.PendingApplicationsCount = Math.Max(0, pendingApps.Count);
                viewModel.MyStudentsCount = Math.Max(0, studentIds.Count);
                viewModel.PendingDiaryEntriesCount = Math.Max(0, pendingDiaryEntries.Count);
                viewModel.CompletedInternshipsCount = Math.Max(0, completedInternships.Count);
                viewModel.UnreadNotificationsCount = Math.Max(0, unreadNotifications.Count);
                viewModel.AdvisorInfo = advisor;
                viewModel.UniversityName = advisor.University?.Name ?? "";

                // Dynamic content
                viewModel.RecentNotifications = notifications?.OrderByDescending(n => n.CreatedAt).Take(5).ToList() ?? new List<Notification>();

                // Recent activities
                viewModel.RecentActivities = await GetRecentActivities(advisorId, studentIds);
                
                // Recent applications
                viewModel.RecentApplications = pendingApps.Take(5).OrderByDescending(a => a.CreatedAt).ToList();
                
                // Recent diary entries
                viewModel.RecentDiaryEntries = pendingDiaryEntries.Take(5).OrderByDescending(d => d.Date).ToList();

                // Academic year info
                viewModel.AcademicYear = GetAcademicYearInfo();

                // Additional dynamic statistics
                try
                {
                    var now = DateTime.Now;
                    var monthStart = new DateTime(now.Year, now.Month, 1);
                    var weekStart = now.Date.AddDays(-(int)(now.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)now.DayOfWeek - 1)); // Hafta pazartesi baþlasýn

                    var thisMonthApplications = allApplications.Count(a => a.CreatedAt >= monthStart);
                    var thisMonthApprovals = allApplications.Count(a => a.Status == InternshipApplicationStatus.Approved && (a.UpdatedAt >= monthStart || a.CreatedAt >= monthStart));
                    var thisWeekDiaryEntries = allDiaryEntries.Count(d => d.Date >= weekStart);
                    var approvalRate = allApplications.Count > 0 ? (decimal)completedInternships.Count / allApplications.Count * 100m : 0m;
                    var activeStudents = students.Count(s => allApplications.Any(a => a.StudentId == s.Id && a.Status == InternshipApplicationStatus.Approved));

                    viewModel.Statistics = new AdvisorStatistics
                    {
                        ThisMonthApplications = thisMonthApplications,
                        ThisMonthApprovals = thisMonthApprovals,
                        ThisWeekDiaryEntries = thisWeekDiaryEntries,
                        ApprovalRate = Math.Round(approvalRate, 1),
                        ActiveStudents = activeStudents
                    };
                }
                catch
                {
                    viewModel.Statistics = new AdvisorStatistics();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Dashboard verileri yüklenirken hata oluþtu: " + ex.Message;
                
                // Set safe default values
                viewModel.PendingApplicationsCount = 0;
                viewModel.MyStudentsCount = 0;
                viewModel.PendingDiaryEntriesCount = 0;
                viewModel.CompletedInternshipsCount = 0;
                viewModel.UnreadNotificationsCount = 0;
                viewModel.AdvisorInfo = advisor;
                viewModel.UniversityName = advisor.University?.Name ?? "";
                viewModel.RecentActivities = new List<AdvisorActivityItem>();
                viewModel.RecentApplications = new List<InternshipApplication>();
                viewModel.RecentDiaryEntries = new List<DiaryEntry>();
                viewModel.RecentNotifications = new List<Notification>();
                viewModel.AcademicYear = new AcademicYearInfo();
            }

            return View("~/Views/Advisor/Dashboard/Index.cshtml", viewModel);
        }

        private AcademicYearInfo GetAcademicYearInfo()
        {
            var now = DateTime.Now;
            var academicYear = now.Month >= 9 ? $"{now.Year}-{now.Year + 1}" : $"{now.Year - 1}-{now.Year}";
            var semester = now.Month >= 2 && now.Month <= 6 ? "Bahar Dönemi" : "Güz Dönemi";
            
            var semesterStart = now.Month >= 9 ? new DateTime(now.Year, 9, 1) : 
                               new DateTime(now.Year, 2, 1);
            var semesterEnd = now.Month >= 9 ? new DateTime(now.Year + 1, 1, 31) : 
                             new DateTime(now.Year, 6, 30);
            
            var weeksRemaining = Math.Max(0, (int)((semesterEnd - now).TotalDays / 7));
            
            return new AcademicYearInfo
            {
                CurrentSemester = semester,
                AcademicYear = academicYear,
                SemesterStartDate = semesterStart,
                SemesterEndDate = semesterEnd,
                WeeksRemaining = weeksRemaining,
                InternshipPeriod = now.Month >= 6 && now.Month <= 8 ? "Yaz Stajý Dönemi" : "Staj Dýþý Dönem"
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserByIdAsync(advisorId);
                
                if (advisor?.UniversityId == null)
                {
                    return Json(new { error = "Danýþman bilgileri bulunamadý" });
                }

                var allStudents = await _userService.GetAllStudentsAsync();
                var students = allStudents.Where(s => s.UniversityId == advisor.UniversityId).ToList();
                var studentIds = students.Select(s => s.Id).ToList();
                
                var allApplications = new List<InternshipApplication>();
                foreach (var studentId in studentIds)
                {
                    try
                    {
                        var apps = await _applicationService.GetApplicationsByStudentIdAsync(studentId);
                        if (apps != null)
                        {
                            allApplications.AddRange(apps);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting applications for student {studentId}: {ex.Message}");
                    }
                }
                
                var pendingApps = allApplications.Where(a => a.Status == InternshipApplicationStatus.Pending).Count();
                
                var allDiaryEntries = new List<DiaryEntry>();
                foreach (var studentId in studentIds)
                {
                    try
                    {
                        var entries = await _diaryService.GetDiaryEntriesByStudentIdAsync(studentId);
                        if (entries != null)
                        {
                            allDiaryEntries.AddRange(entries);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting diary entries for student {studentId}: {ex.Message}");
                    }
                }
                
                var pendingDiaries = allDiaryEntries.Where(d => !d.IsApproved).Count();
                var completedInternships = allApplications.Where(a => a.Status == InternshipApplicationStatus.Approved).Count();

                // Get notifications
                var notifications = await _notificationService.GetAllNotificationsByUserIdAsync(advisorId);
                var unreadNotifications = notifications.Count(n => !n.IsRead);

                // Approval rate
                var approvalRate = allApplications.Count > 0 ? (decimal)completedInternships / allApplications.Count * 100m : 0m;

                return Json(new
                {
                    pendingApplications = Math.Max(0, pendingApps),
                    pendingDiaries = Math.Max(0, pendingDiaries),
                    totalStudents = Math.Max(0, studentIds.Count),
                    completedInternships = Math.Max(0, completedInternships),
                    unreadNotifications = Math.Max(0, unreadNotifications),
                    approvalRate = Math.Round(approvalRate, 1)
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    error = ex.Message,
                    pendingApplications = 0,
                    pendingDiaries = 0,
                    totalStudents = 0,
                    completedInternships = 0,
                    unreadNotifications = 0
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var notifications = await _notificationService.GetAllNotificationsByUserIdAsync(advisorId);
                var unreadCount = notifications.Count(n => !n.IsRead);
                return Json(new { unreadCount = unreadCount });
            }
            catch (Exception ex)
            {
                return Json(new { unreadCount = 0, error = ex.Message });
            }
        }

        private async Task<List<AdvisorActivityItem>> GetRecentActivities(int advisorId, List<int> studentIds)
        {
            var activities = new List<AdvisorActivityItem>();

            try
            {
                var limitedStudentIds = studentIds.Take(5).ToList();
                
                var allApplications = new List<InternshipApplication>();
                foreach (var studentId in limitedStudentIds)
                {
                    try
                    {
                        var apps = await _applicationService.GetApplicationsByStudentIdAsync(studentId);
                        if (apps != null)
                        {
                            allApplications.AddRange(apps.Take(1));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting applications for student {studentId}: {ex.Message}");
                    }
                }

                foreach (var app in allApplications.OrderByDescending(a => a.UpdatedAt).Take(2))
                {
                    try
                    {
                        var student = await _userService.GetUserByIdAsync(app.StudentId);
                        if (student != null)
                        {
                            activities.Add(new AdvisorActivityItem
                            {
                                Title = "Yeni Staj Baþvurusu",
                                Description = $"{student.FullName} tarafýndan staj baþvurusu yapýldý",
                                CreatedAt = app.CreatedAt,
                                Type = "application",
                                Icon = "fas fa-file-alt",
                                Color = "blue",
                                StudentName = student.FullName ?? "Bilinmeyen",
                                RelatedId = app.Id
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting student info for application {app.Id}: {ex.Message}");
                    }
                }

                var allDiaryEntries = new List<DiaryEntry>();
                foreach (var studentId in limitedStudentIds)
                {
                    try
                    {
                        var entries = await _diaryService.GetDiaryEntriesByStudentIdAsync(studentId);
                        if (entries != null)
                        {
                            allDiaryEntries.AddRange(entries.Take(1));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting diary entries for student {studentId}: {ex.Message}");
                    }
                }

                foreach (var entry in allDiaryEntries.OrderByDescending(d => d.CreatedAt).Take(2))
                {
                    try
                    {
                        var student = await _userService.GetUserByIdAsync(entry.StudentId);
                        if (student != null)
                        {
                            activities.Add(new AdvisorActivityItem
                            {
                                Title = "Yeni Staj Defteri Giriþi",
                                Description = $"{student.FullName} staj defteri güncellemesi yaptý",
                                CreatedAt = entry.CreatedAt,
                                Type = "diary",
                                Icon = "fas fa-book-open",
                                Color = "green",
                                StudentName = student.FullName ?? "Bilinmeyen",
                                RelatedId = entry.Id
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting student info for diary entry {entry.Id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recent activities: {ex.Message}");
            }

            // Return max 3 activities, ordered by date
            return activities.OrderByDescending(a => a.CreatedAt).Take(3).ToList();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}