using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Interfaces.UserRepose;
using Business.Interfaces.Internship;
using Business.Interfaces;
using WebUI.Models.ViewModels.Advisor;
using Core.Entity;
using Business.Interfaces.Order;

namespace WebUI.Controllers.Advisor
{
    [Authorize(Roles = "Advisor")]
    public class AdvisorStudentController : Controller
    {
        private readonly IIUserService _userService;
        private readonly IInternshipApplicationService _applicationService;
        private readonly IInternshipDiaryService _diaryService;
        private readonly IINotificationService _notificationService;

        public AdvisorStudentController(IIUserService userService, IInternshipApplicationService applicationService, IInternshipDiaryService diaryService, IINotificationService notificationService)
        {
            _userService = userService;
            _applicationService = applicationService;
            _diaryService = diaryService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index([FromQuery] AdvisorStudentFilters filters, int page = 1)
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
                if (advisor?.UniversityId == null)
                {
                    TempData["ErrorMessage"] = "Danisman bilgileri eksik.";
                    return RedirectToAction("Index", "AdvisorDashboard");
                }

                var allStudents = await _userService.GetAllStudentsAsync();
                var scopedStudents = FilterStudentsForAdvisor(allStudents, advisor);

                var query = scopedStudents.AsQueryable();
                query = ApplyFilterQuery(query, filters);

                var list = query.ToList();
                var studentIds = list.Select(s => s.Id).ToList();

                var appCache = new Dictionary<int, List<InternshipApplication>>();
                var diaryCache = new Dictionary<int, List<DiaryEntry>>();
                foreach (var sid in studentIds)
                {
                    var apps = await _applicationService.GetApplicationsByStudentIdAsync(sid) ?? new List<InternshipApplication>();
                    appCache[sid] = apps;
                    var diaryEntries = (await _diaryService.GetDiariesByStudentIdAsync(sid))?
                        .SelectMany(d => d.Entries ?? new List<DiaryEntry>())
                        .ToList() ?? new List<DiaryEntry>();
                    diaryCache[sid] = diaryEntries;
                }

                var items = new List<AdvisorStudentItemViewModel>();
                foreach (var s in list)
                {
                    var apps = appCache[s.Id];
                    var diaries = diaryCache[s.Id];
                    var approvedApp = apps.FirstOrDefault(a => a.Status == InternshipApplicationStatus.Approved);
                    var internshipStatus = "Yok";
                    if (approvedApp != null)
                    {
                        if (DateTime.Now.Date < approvedApp.StartDate.Date) internshipStatus = "Baslayacak";
                        else if (DateTime.Now.Date > approvedApp.EndDate.Date) internshipStatus = "Tamamlandi";
                        else internshipStatus = "Devam";
                    }
                    else if (apps.Any(a => a.Status == InternshipApplicationStatus.Pending)) internshipStatus = "Bekliyor";

                    items.Add(new AdvisorStudentItemViewModel
                    {
                        Id = s.Id,
                        FullName = s.FullName ?? "",
                        Email = s.Email ?? "",
                        Department = s.Department,
                        Grade = s.Grade,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt,
                        TotalApplications = apps.Count,
                        ApprovedApplications = apps.Count(a => a.Status == InternshipApplicationStatus.Approved),
                        PendingApplications = apps.Count(a => a.Status == InternshipApplicationStatus.Pending),
                        RejectedApplications = apps.Count(a => a.Status == InternshipApplicationStatus.Rejected),
                        TotalDiaryEntries = diaries.Count,
                        PendingDiaryEntries = diaries.Count(d => !d.IsApproved),
                        ApprovedDiaryEntries = diaries.Count(d => d.IsApproved),
                        LastActivity = new[] { diaries.OrderByDescending(d=>d.Date).FirstOrDefault()?.Date, apps.OrderByDescending(a=>a.UpdatedAt).FirstOrDefault()?.UpdatedAt }.Where(x=>x.HasValue).Max(),
                        InternshipStatus = internshipStatus,
                        StudentNumber = s.StudentNumber,
                        UniversityName = advisor.University?.Name
                    });
                }

                items = ApplySort(items, filters);

                var vm = new AdvisorStudentListViewModel
                {
                    Filters = filters,
                    CurrentPage = page,
                    TotalItems = items.Count,
                    ActiveCount = items.Count(i => i.IsActive),
                    PassiveCount = items.Count(i => !i.IsActive),
                    WithApprovedInternship = items.Count(i => i.InternshipStatus == "Devam" || i.InternshipStatus == "Tamamlandi"),
                    WithPendingApplications = items.Count(i => i.PendingApplications > 0)
                };
                vm.Students = items.Skip((page - 1) * vm.PageSize).Take(vm.PageSize).ToList();
                return View("~/Views/Advisor/Student/Index.cshtml", vm);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ogrenci listesi yuklenirken hata olustu: " + ex.Message;
                return RedirectToAction("Index", "AdvisorDashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] AdvisorStudentFilters filters, int page = 1, int pageSize = 12)
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
                if (advisor?.UniversityId == null)
                    return Json(new { success = false, message = "Danisman bilgileri eksik" });

                var allStudents = await _userService.GetAllStudentsAsync();
                var scopedStudents = FilterStudentsForAdvisor(allStudents, advisor);
                var baseQuery = ApplyFilterQuery(scopedStudents.AsQueryable(), filters);
                var filteredStudents = baseQuery.ToList();
                var total = filteredStudents.Count;

                var skip = (page - 1) * pageSize;
                var pageStudents = filteredStudents.Skip(skip).Take(pageSize).ToList();

                var appCache = new Dictionary<int, List<InternshipApplication>>();
                var diaryCache = new Dictionary<int, List<DiaryEntry>>();
                foreach (var sid in pageStudents.Select(s => s.Id))
                {
                    var apps = await _applicationService.GetApplicationsByStudentIdAsync(sid) ?? new List<InternshipApplication>();
                    appCache[sid] = apps;
                    var diariesFlat = (await _diaryService.GetDiariesByStudentIdAsync(sid))?
                        .SelectMany(d => d.Entries ?? new List<DiaryEntry>())
                        .ToList() ?? new List<DiaryEntry>();
                    diaryCache[sid] = diariesFlat;
                }

                var items = new List<AdvisorStudentItemViewModel>();
                foreach (var s in pageStudents)
                {
                    var apps = appCache[s.Id];
                    var diaries = diaryCache[s.Id];
                    var approvedApp = apps.FirstOrDefault(a => a.Status == InternshipApplicationStatus.Approved);
                    var internshipStatus = "Yok";
                    if (approvedApp != null)
                    {
                        if (DateTime.Now.Date < approvedApp.StartDate.Date) internshipStatus = "Baslayacak";
                        else if (DateTime.Now.Date > approvedApp.EndDate.Date) internshipStatus = "Tamamlandi";
                        else internshipStatus = "Devam";
                    }
                    else if (apps.Any(a => a.Status == InternshipApplicationStatus.Pending)) internshipStatus = "Bekliyor";

                    items.Add(new AdvisorStudentItemViewModel
                    {
                        Id = s.Id,
                        FullName = s.FullName ?? string.Empty,
                        Email = s.Email ?? string.Empty,
                        Department = s.Department,
                        Grade = s.Grade,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt,
                        TotalApplications = apps.Count,
                        ApprovedApplications = apps.Count(a => a.Status == InternshipApplicationStatus.Approved),
                        PendingApplications = apps.Count(a => a.Status == InternshipApplicationStatus.Pending),
                        RejectedApplications = apps.Count(a => a.Status == InternshipApplicationStatus.Rejected),
                        TotalDiaryEntries = diaries.Count,
                        PendingDiaryEntries = diaries.Count(d => !d.IsApproved),
                        ApprovedDiaryEntries = diaries.Count(d => d.IsApproved),
                        LastActivity = new[] { diaries.OrderByDescending(d=>d.Date).FirstOrDefault()?.Date, apps.OrderByDescending(a=>a.UpdatedAt).FirstOrDefault()?.UpdatedAt }.Where(x=>x.HasValue).Max(),
                        InternshipStatus = internshipStatus,
                        StudentNumber = s.StudentNumber,
                        UniversityName = advisor.University?.Name
                    });
                }

                items = ApplySort(items, filters);

                return Json(new
                {
                    success = true,
                    page,
                    pageSize,
                    total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    students = items.Select(i => new
                    {
                        id = i.Id,
                        fullName = i.FullName,
                        email = i.Email,
                        department = i.Department,
                        grade = i.Grade,
                        isActive = i.IsActive,
                        lastActivity = i.LastActivity?.ToString("yyyy-MM-dd"),
                        totalApplications = i.TotalApplications,
                        approvedApplications = i.ApprovedApplications,
                        pendingApplications = i.PendingApplications,
                        rejectedApplications = i.RejectedApplications,
                        totalDiaryEntries = i.TotalDiaryEntries,
                        pendingDiaryEntries = i.PendingDiaryEntries,
                        approvedDiaryEntries = i.ApprovedDiaryEntries,
                        internshipStatus = i.InternshipStatus,
                        studentNumber = i.StudentNumber,
                        universityName = i.UniversityName
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class BulkNotificationRequest
        {
            public List<int> StudentIds { get; set; } = new();
            public string Title { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkNotify([FromBody] BulkNotificationRequest request)
        {
            try
            {
                if (request.StudentIds == null || !request.StudentIds.Any())
                    return Json(new { success = false, message = "Ogrenci secilmedi." });
                if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
                    return Json(new { success = false, message = "Baslik ve mesaj zorunludur." });

                int sent = 0;
                foreach (var sid in request.StudentIds.Distinct())
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(new Notification
                        {
                            UserId = sid,
                            Title = request.Title.Trim(),
                            Message = request.Message.Trim(),
                            Type = "AdvisorBulk",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow,
                            IconClass = "fas fa-bell"
                        });
                        sent++;
                    }
                    catch { }
                }
                return Json(new { success = true, sent });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var user = await _userService.GetUserWithUniversityAsync(id);
                if (user == null)
                    return Json(new { success = false, message = "Ogrenci bulunamadi" });

                var apps = await _applicationService.GetApplicationsByStudentIdAsync(id) ?? new List<InternshipApplication>();
                var diariesFlat = (await _diaryService.GetDiariesByStudentIdAsync(id))?
                    .SelectMany(d => d.Entries ?? new List<DiaryEntry>())
                    .ToList() ?? new List<DiaryEntry>();

                var approvedApp = apps.FirstOrDefault(a => a.Status == InternshipApplicationStatus.Approved);
                var internshipStatus = "Yok";
                if (approvedApp != null)
                {
                    if (DateTime.Now.Date < approvedApp.StartDate.Date) internshipStatus = "Baslayacak";
                    else if (DateTime.Now.Date > approvedApp.EndDate.Date) internshipStatus = "Tamamlandi";
                    else internshipStatus = "Devam";
                }
                else if (apps.Any(a => a.Status == InternshipApplicationStatus.Pending)) internshipStatus = "Bekliyor";

                return Json(new
                {
                    success = true,
                    student = new
                    {
                        id = user.Id,
                        fullName = user.FullName,
                        email = user.Email,
                        department = user.Department,
                        grade = user.Grade,
                        isActive = user.IsActive,
                        createdAt = user.CreatedAt.ToString("yyyy-MM-dd"),
                        studentNumber = user.StudentNumber,
                        universityName = user.University?.Name,
                        internshipStatus,
                        applicationCount = apps.Count,
                        approvedCount = apps.Count(a => a.Status == InternshipApplicationStatus.Approved),
                        pendingCount = apps.Count(a => a.Status == InternshipApplicationStatus.Pending),
                        rejectedCount = apps.Count(a => a.Status == InternshipApplicationStatus.Rejected),
                        diaryEntryCount = diariesFlat.Count,
                        pendingDiaryCount = diariesFlat.Count(d => !d.IsApproved),
                        approvedDiaryCount = diariesFlat.Count(d => d.IsApproved)
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("AdvisorStudent/GetDetail/{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            return await Detail(id);
        }

        [HttpGet]
        public async Task<IActionResult> StudentDetail(int id)
        {
            try
            {
                var user = await _userService.GetUserWithUniversityAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Ogrenci bulunamadi.";
                    return RedirectToAction("Index");
                }

                var apps = await _applicationService.GetApplicationsByStudentIdAsync(id) ?? new List<InternshipApplication>();
                var diariesFlat = (await _diaryService.GetDiariesByStudentIdAsync(id))?
                    .SelectMany(d => d.Entries ?? new List<DiaryEntry>())
                    .ToList() ?? new List<DiaryEntry>();

                var approvedApp = apps.FirstOrDefault(a => a.Status == InternshipApplicationStatus.Approved);
                var internshipStatus = "Yok";
                if (approvedApp != null)
                {
                    if (DateTime.Now.Date < approvedApp.StartDate.Date) internshipStatus = "Baslayacak";
                    else if (DateTime.Now.Date > approvedApp.EndDate.Date) internshipStatus = "Tamamlandi";
                    else internshipStatus = "Devam";
                }
                else if (apps.Any(a => a.Status == InternshipApplicationStatus.Pending)) internshipStatus = "Bekliyor";

                var vm = new AdvisorStudentDetailViewModel
                {
                    Student = new AdvisorStudentInfoViewModel
                    {
                        Id = user.Id,
                        FullName = user.FullName ?? "",
                        Email = user.Email ?? "",
                        Phone = user.Phone,
                        StudentNumber = user.StudentNumber,
                        Department = user.Department,
                        UniversityName = user.University?.Name,
                        Grade = user.Grade,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    },
                    TotalApplications = apps.Count,
                    ApprovedApplications = apps.Count(a => a.Status == InternshipApplicationStatus.Approved),
                    PendingApplications = apps.Count(a => a.Status == InternshipApplicationStatus.Pending),
                    RejectedApplications = apps.Count(a => a.Status == InternshipApplicationStatus.Rejected),
                    TotalDiaryEntries = diariesFlat.Count,
                    ApprovedDiaryEntries = diariesFlat.Count(d => d.IsApproved),
                    PendingDiaryEntries = diariesFlat.Count(d => !d.IsApproved),
                    InternshipStatus = internshipStatus,
                    LastActivity = new[] { diariesFlat.OrderByDescending(d => d.Date).FirstOrDefault()?.Date, apps.OrderByDescending(a => a.UpdatedAt).FirstOrDefault()?.UpdatedAt }.Where(x => x.HasValue).Max(),
                    DiaryEntries = diariesFlat.OrderByDescending(d => d.Date).ToList(),
                    Applications = apps.Select(a => new AdvisorApplicationCardViewModel
                    {
                        Id = a.Id,
                        CompanyName = a.InternshipOffer?.Company?.CompanyName ?? "Belirtilmemis",
                        Position = a.InternshipOffer?.Field ?? a.InternshipOffer?.Title ?? "Belirtilmemis",
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        WeeklyWorkHours = 40,
                        Status = MapApplicationStatusToApprovalStatus(a.Status),
                        CreatedAt = a.CreatedAt
                    }).ToList(),
                    Documents = new List<StudentDocumentViewModel>()
                };

                return View("~/Views/Advisor/Student/Detail.cshtml", vm);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Detay yuklenirken hata olustu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private Core.Entity.InternshipApprovalStatus MapApplicationStatusToApprovalStatus(InternshipApplicationStatus status)
        {
            return status switch
            {
                InternshipApplicationStatus.Approved => Core.Entity.InternshipApprovalStatus.Approved,
                InternshipApplicationStatus.Rejected => Core.Entity.InternshipApprovalStatus.Rejected,
                _ => Core.Entity.InternshipApprovalStatus.Pending
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotification(int studentId, string title, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
                    return Json(new { success = false, message = "Baslik ve mesaj zorunludur." });

                await _notificationService.CreateNotificationAsync(new Notification
                {
                    UserId = studentId,
                    Title = title.Trim(),
                    Message = message.Trim(),
                    Type = "AdvisorNotification",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    IconClass = "fas fa-bell"
                });

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private IQueryable<User> ApplyFilterQuery(IQueryable<User> query, AdvisorStudentFilters filters)
        {
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var term = filters.Search.Trim().ToLower();
                query = query.Where(s => (s.FullName ?? "").ToLower().Contains(term) || (s.Email ?? "").ToLower().Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(filters.Department))
            {
                query = query.Where(s => s.Department != null && s.Department.Equals(filters.Department, StringComparison.OrdinalIgnoreCase));
            }
            if (filters.Grade.HasValue)
            {
                query = query.Where(s => s.Grade == filters.Grade);
            }
            if (!string.IsNullOrWhiteSpace(filters.Status))
            {
                if (filters.Status == "active") query = query.Where(s => s.IsActive);
                else if (filters.Status == "passive") query = query.Where(s => !s.IsActive);
            }
            return query;
        }

        private List<AdvisorStudentItemViewModel> ApplySort(List<AdvisorStudentItemViewModel> items, AdvisorStudentFilters filters)
        {
            return filters.Sort switch
            {
                "name" => (filters.Order == "asc" ? items.OrderBy(i => i.FullName) : items.OrderByDescending(i => i.FullName)).ToList(),
                "applications" => (filters.Order == "asc" ? items.OrderBy(i => i.TotalApplications) : items.OrderByDescending(i => i.TotalApplications)).ToList(),
                "diaries" => (filters.Order == "asc" ? items.OrderBy(i => i.TotalDiaryEntries) : items.OrderByDescending(i => i.TotalDiaryEntries)).ToList(),
                "lastActivity" => (filters.Order == "asc" ? items.OrderBy(i => i.LastActivity) : items.OrderByDescending(i => i.LastActivity)).ToList(),
                _ => items.OrderByDescending(i => i.LastActivity).ToList()
            };
        }

        private List<User> FilterStudentsForAdvisor(IEnumerable<User> allStudents, User advisor)
        {
            var sameUni = allStudents.Where(s => s.UniversityId == advisor.UniversityId).ToList();
            if (string.IsNullOrWhiteSpace(advisor.Department))
                return sameUni;

            var sameDept = sameUni.Where(s => !string.IsNullOrWhiteSpace(s.Department) && s.Department.Equals(advisor.Department, StringComparison.OrdinalIgnoreCase)).ToList();
            if (sameDept.Any()) return sameDept;
            return sameUni.Where(s => string.IsNullOrWhiteSpace(s.Department)).ToList();
        }
    }
}
