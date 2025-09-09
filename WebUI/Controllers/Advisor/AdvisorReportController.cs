using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Interfaces.Order;
using Business.Interfaces.UserRepose;
using Business.Interfaces.Internship;
using Business.Interfaces;
using WebUI.Models.ViewModels.Advisor;
using Core.Entity;

namespace WebUI.Controllers.Advisor
{
    [Authorize(Roles = "Advisor")]
    [Route("Advisor/[controller]")]
    public class AdvisorReportController : Controller
    {
        private readonly IIUserService _userService;
        private readonly IInternshipApplicationService _applicationService;
        private readonly IDiaryService _diaryService;
        private readonly IINotificationService _notificationService;

        public AdvisorReportController(
            IIUserService userService,
            IInternshipApplicationService applicationService,
            IDiaryService diaryService,
            IINotificationService notificationService)
        {
            _userService = userService;
            _applicationService = applicationService;
            _diaryService = diaryService;
            _notificationService = notificationService;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(string period = "thisMonth", string reportType = "overview")
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);

                if (advisor?.UniversityId == null)
                {
                    TempData["ErrorMessage"] = "Danýþman bilgileri eksik.";
                    return RedirectToAction("Index", "AdvisorDashboard");
                }

                // Get date range based on period
                var (startDate, endDate) = GetDateRange(period);

                // Get students from same university/department
                var allStudents = await _userService.GetAllStudentsAsync();
                var students = GetFilteredStudents(allStudents, advisor);

                // Generate reports based on type
                AdvisorReportViewModel viewModel;

                switch (reportType?.ToLower())
                {
                    case "applications":
                        viewModel = await GenerateApplicationReport(students, startDate, endDate, period);
                        break;
                    case "diaries":
                        viewModel = await GenerateDiaryReport(students, startDate, endDate, period);
                        break;
                    case "students":
                        viewModel = await GenerateStudentReport(students, startDate, endDate, period);
                        break;
                    default:
                        viewModel = await GenerateOverviewReport(students, startDate, endDate, period);
                        break;
                }

                viewModel.CurrentPeriod = period;
                viewModel.CurrentReportType = reportType;
                viewModel.AdvisorInfo = new AdvisorInfo
                {
                    UniversityName = advisor.University?.Name ?? "Bilinmiyor",
                    Department = advisor.Department ?? "Bölüm Belirtilmemiþ",
                    TotalStudentsInDepartment = students.Count,
                    StudentsWithDiaries = students.Count(s => _diaryService.GetDiaryEntriesByStudentIdAsync(s.Id).Result.Any())
                };

                return View("~/Views/Advisor/Report/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Rapor oluþturulurken hata oluþtu: " + ex.Message;
                return View("~/Views/Advisor/Report/Index.cshtml", new AdvisorReportViewModel());
            }
        }

        private async Task<AdvisorReportViewModel> GenerateOverviewReport(List<User> students, DateTime startDate, DateTime endDate, string period)
        {
            var viewModel = new AdvisorReportViewModel();

            // Application statistics
            var applicationStats = new List<ApplicationStatistic>();
            var diaryStats = new List<DiaryStatistic>();
            var activityData = new List<ActivityDataItem>();

            foreach (var student in students)
            {
                var applications = await _applicationService.GetApplicationsByStudentIdAsync(student.Id);
                var periodApplications = applications.Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate);

                if (periodApplications.Any())
                {
                    applicationStats.Add(new ApplicationStatistic
                    {
                        StudentName = student.FullName,
                        TotalApplications = periodApplications.Count(),
                        ApprovedApplications = periodApplications.Count(a => a.Status == InternshipApplicationStatus.Approved),
                        PendingApplications = periodApplications.Count(a => a.Status == InternshipApplicationStatus.Pending),
                        RejectedApplications = periodApplications.Count(a => a.Status == InternshipApplicationStatus.Rejected)
                    });
                }

                var diaryEntries = await _diaryService.GetDiaryEntriesByStudentIdAsync(student.Id);
                var periodEntries = diaryEntries.Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate);

                if (periodEntries.Any())
                {
                    diaryStats.Add(new DiaryStatistic
                    {
                        StudentName = student.FullName,
                        TotalEntries = periodEntries.Count(),
                        ApprovedEntries = periodEntries.Count(d => d.IsApproved),
                        PendingEntries = periodEntries.Count(d => !d.IsApproved),
                        TotalHours = periodEntries.Sum(d => (double)d.WorkingHours)
                    });
                }
            }

            // Generate daily activity data for charts
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                var dayApplications = 0;
                var dayDiaryEntries = 0;

                foreach (var student in students)
                {
                    var applications = await _applicationService.GetApplicationsByStudentIdAsync(student.Id);
                    dayApplications += applications.Count(a => a.CreatedAt.Date == currentDate.Date);

                    var diaryEntries = await _diaryService.GetDiaryEntriesByStudentIdAsync(student.Id);
                    dayDiaryEntries += diaryEntries.Count(d => d.CreatedAt.Date == currentDate.Date);
                }

                activityData.Add(new ActivityDataItem
                {
                    Date = currentDate,
                    Applications = dayApplications,
                    DiaryEntries = dayDiaryEntries,
                    Description = $"{currentDate:dd/MM} - {dayApplications} baþvuru, {dayDiaryEntries} defter girdisi",
                    ActivityType = "Daily",
                    Count = dayApplications + dayDiaryEntries
                });

                currentDate = currentDate.AddDays(1);
            }

            viewModel.ApplicationStatistics = applicationStats;
            viewModel.DiaryStatistics = diaryStats;
            viewModel.ActivityData = activityData;

            // Summary statistics
            viewModel.Summary = new ReportSummary
            {
                TotalStudents = students.Count,
                ActiveStudents = students.Count(s => applicationStats.Any(a => a.StudentName == s.FullName) || 
                                                    diaryStats.Any(d => d.StudentName == s.FullName)),
                TotalApplications = applicationStats.Sum(a => a.TotalApplications),
                TotalDiaryEntries = diaryStats.Sum(d => d.TotalEntries),
                TotalWorkingHours = (decimal)diaryStats.Sum(d => d.TotalHours),
                Period = period
            };

            return viewModel;
        }

        private async Task<AdvisorReportViewModel> GenerateApplicationReport(List<User> students, DateTime startDate, DateTime endDate, string period)
        {
            var viewModel = new AdvisorReportViewModel();
            var applicationStats = new List<ApplicationStatistic>();

            foreach (var student in students)
            {
                var applications = await _applicationService.GetApplicationsByStudentIdAsync(student.Id);
                var periodApplications = applications.Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate);

                if (periodApplications.Any())
                {
                    applicationStats.Add(new ApplicationStatistic
                    {
                        StudentName = student.FullName,
                        StudentEmail = student.Email,
                        TotalApplications = periodApplications.Count(),
                        ApprovedApplications = periodApplications.Count(a => a.Status == InternshipApplicationStatus.Approved),
                        PendingApplications = periodApplications.Count(a => a.Status == InternshipApplicationStatus.Pending),
                        RejectedApplications = periodApplications.Count(a => a.Status == InternshipApplicationStatus.Rejected),
                        LastApplicationDate = periodApplications.OrderByDescending(a => a.CreatedAt).FirstOrDefault()?.CreatedAt
                    });
                }
            }

            viewModel.ApplicationStatistics = applicationStats;
            return viewModel;
        }

        private async Task<AdvisorReportViewModel> GenerateDiaryReport(List<User> students, DateTime startDate, DateTime endDate, string period)
        {
            var viewModel = new AdvisorReportViewModel();
            var diaryStats = new List<DiaryStatistic>();

            foreach (var student in students)
            {
                var diaryEntries = await _diaryService.GetDiaryEntriesByStudentIdAsync(student.Id);
                var periodEntries = diaryEntries.Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate);

                if (periodEntries.Any())
                {
                    diaryStats.Add(new DiaryStatistic
                    {
                        StudentName = student.FullName,
                        StudentEmail = student.Email,
                        TotalEntries = periodEntries.Count(),
                        ApprovedEntries = periodEntries.Count(d => d.IsApproved),
                        PendingEntries = periodEntries.Count(d => !d.IsApproved),
                        TotalHours = periodEntries.Sum(d => (double)d.WorkingHours),
                        AverageHoursPerEntry = periodEntries.Any() ? periodEntries.Average(d => (double)d.WorkingHours) : 0,
                        LastEntryDate = periodEntries.OrderByDescending(d => d.CreatedAt).FirstOrDefault()?.CreatedAt
                    });
                }
            }

            viewModel.DiaryStatistics = diaryStats;
            return viewModel;
        }

        private async Task<AdvisorReportViewModel> GenerateStudentReport(List<User> students, DateTime startDate, DateTime endDate, string period)
        {
            var viewModel = new AdvisorReportViewModel();
            var studentReports = new List<StudentReportData>();

            foreach (var student in students)
            {
                var applications = await _applicationService.GetApplicationsByStudentIdAsync(student.Id);
                var periodApplications = applications.Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate);

                var diaryEntries = await _diaryService.GetDiaryEntriesByStudentIdAsync(student.Id);
                var periodEntries = diaryEntries.Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate);

                var lastActivityDates = new List<DateTime>();
                if (periodApplications.Any())
                    lastActivityDates.Add(periodApplications.Max(a => a.CreatedAt));
                if (periodEntries.Any())
                    lastActivityDates.Add(periodEntries.Max(d => d.CreatedAt));

                studentReports.Add(new StudentReportData
                {
                    StudentName = student.FullName,
                    StudentEmail = student.Email,
                    Department = student.Department ?? "Belirtilmemiþ",
                    TotalApplications = periodApplications.Count(),
                    TotalDiaryEntries = periodEntries.Count(),
                    TotalWorkingHours = (decimal)periodEntries.Sum(d => (double)d.WorkingHours),
                    LastActivity = lastActivityDates.Any() ? lastActivityDates.Max() : null,
                    IsActive = periodApplications.Any() || periodEntries.Any()
                });
            }

            viewModel.StudentReports = studentReports.Select(sr => new StudentReportItem
            {
                StudentName = sr.StudentName,
                StudentEmail = sr.StudentEmail,
                Department = sr.Department,
                TotalApplications = sr.TotalApplications,
                TotalDiaryEntries = sr.TotalDiaryEntries,
                TotalWorkingHours = sr.TotalWorkingHours,
                LastActivity = sr.LastActivity,
                IsActive = sr.IsActive,
                ActivityStatus = sr.IsActive ? "Aktif" : "Pasif"
            }).ToList();
            return viewModel;
        }

        private (DateTime startDate, DateTime endDate) GetDateRange(string period)
        {
            var today = DateTime.Today;
            return period?.ToLower() switch
            {
                "today" => (today, today.AddDays(1).AddTicks(-1)),
                "thisweek" => (today.AddDays(-(int)today.DayOfWeek), today.AddDays(7 - (int)today.DayOfWeek).AddTicks(-1)),
                "thismonth" => (new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1).AddTicks(-1)),
                "thisyear" => (new DateTime(today.Year, 1, 1), new DateTime(today.Year, 12, 31, 23, 59, 59)),
                "lastmonth" => (new DateTime(today.Year, today.Month, 1).AddMonths(-1), new DateTime(today.Year, today.Month, 1).AddTicks(-1)),
                "last3months" => (today.AddMonths(-3), today.AddDays(1).AddTicks(-1)),
                _ => (new DateTime(today.Year, today.Month, 1), today.AddDays(1).AddTicks(-1))
            };
        }

        private List<User> GetFilteredStudents(List<User> allStudents, User advisor)
        {
            var studentsFromSameUniversity = allStudents.Where(s => s.UniversityId == advisor.UniversityId).ToList();
            
            if (string.IsNullOrEmpty(advisor.Department))
            {
                return studentsFromSameUniversity;
            }
            else
            {
                var students = studentsFromSameUniversity.Where(s => 
                    !string.IsNullOrEmpty(s.Department) &&
                    s.Department.Equals(advisor.Department, StringComparison.OrdinalIgnoreCase)
                ).ToList();
                
                if (!students.Any())
                {
                    students = studentsFromSameUniversity.Where(s => string.IsNullOrEmpty(s.Department)).ToList();
                }
                
                return students;
            }
        }

        [HttpGet]
        [Route("Export/{type}")]
        public async Task<IActionResult> Export(string type, string period = "thisMonth", string format = "excel")
        {
            try
            {
                // Bu method Excel/PDF export iþlemlerini handle edecek
                // Þimdilik placeholder
                TempData["InfoMessage"] = $"{format.ToUpper()} export özelliði yakýnda eklenecek.";
                return RedirectToAction("Index", new { period, reportType = type });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Export iþlemi sýrasýnda hata oluþtu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}