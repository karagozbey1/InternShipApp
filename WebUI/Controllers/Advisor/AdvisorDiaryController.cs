using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Interfaces;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Business.Interfaces.Order;
using WebUI.Models.ViewModels.Advisor;
using Core.Entity;
using System.Security.Claims;

namespace WebUI.Controllers.Advisor
{
    [Authorize(Roles = "Advisor")]
    [Route("Advisor/[controller]")]
    public class AdvisorDiaryController : Controller
    {
        private readonly IDiaryService _diaryService;
        private readonly IIUserService _userService;
        private readonly IInternshipApplicationService _applicationService;
        private readonly IINotificationService _notificationService;
        private readonly IInternshipDiaryService _internshipDiaryService;

        public AdvisorDiaryController(
            IDiaryService diaryService,
            IIUserService userService,
            IInternshipApplicationService applicationService,
            IINotificationService notificationService,
            IInternshipDiaryService internshipDiaryService)
        {
            _diaryService = diaryService;
            _userService = userService;
            _applicationService = applicationService;
            _notificationService = notificationService;
            _internshipDiaryService = internshipDiaryService;
        }

        // Helper to get effective university id from a User entity (use UniversityId if set, otherwise loaded navigation)
        private int? GetEffectiveUniversityId(User? u)
        {
            if (u == null) return null;
            if (u.UniversityId != null && u.UniversityId > 0) return u.UniversityId;
            return u.University?.Id;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(DiaryFilters filters, int page = 1)
        {
            var advisorId = GetCurrentUserId();
            var advisor = await _userService.GetUserWithUniversityAsync(advisorId);

            var advisorUni = GetEffectiveUniversityId(advisor);
            if (advisorUni == null)
            {
                TempData["ErrorMessage"] = "Danýþman bilgileri eksik.";
                return RedirectToAction("Index", "AdvisorDashboard");
            }

            var viewModel = new AdvisorDiaryViewModel
            {
                Filters = filters,
                CurrentPage = page,
                PageSize = 10
            };

            try
            {
                var allStudents = await _userService.GetAllStudentsAsync();
                var studentsFromSameUniversity = allStudents.Where(s => GetEffectiveUniversityId(s) == advisorUni).ToList();

                List<User> students;
                if (string.IsNullOrEmpty(advisor?.Department))
                {
                    students = studentsFromSameUniversity;
                    TempData["InfoMessage"] = $"Danýþman bölüm bilgisi eksik. Ayný üniversitedeki tüm öðrenciler gösteriliyor. ({studentsFromSameUniversity.Count} öðrenci)";
                }
                else
                {
                    students = studentsFromSameUniversity.Where(s =>
                        !string.IsNullOrEmpty(s.Department) &&
                        s.Department.Equals(advisor.Department, StringComparison.OrdinalIgnoreCase)
                    ).ToList();

                    if (!students.Any())
                    {
                        var studentsWithoutDepartment = studentsFromSameUniversity.Where(s => string.IsNullOrEmpty(s.Department)).ToList();
                        if (studentsWithoutDepartment.Any())
                        {
                            students = studentsWithoutDepartment;
                            TempData["InfoMessage"] = $"Ayný bölümden öðrenci bulunamadý. Bölüm bilgisi olmayan öðrenciler gösteriliyor. ({studentsWithoutDepartment.Count} öðrenci)";
                        }
                    }
                }

                var studentDiaryGroups = new List<StudentDiaryGroup>();
                foreach (var student in students)
                {
                    var applications = await _applicationService.GetApplicationsByStudentIdAsync(student.Id);
                    var diariesAll = await _internshipDiaryService.GetDiariesByStudentIdAsync(student.Id) ?? new List<InternshipDiary>();

                    // Sadece approved baþvuru ile iliþkili zorunlu defterleri al
                    var approvedApps = applications.Where(a => a.Status == InternshipApplicationStatus.Approved).ToList();
                    if (!approvedApps.Any()) continue;

                    // Her approved + zorunlu defter çifti için ayrý grup
                    foreach (var app in approvedApps)
                    {
                        var relatedDiaries = diariesAll
                            .Where(d => d.IsRequired && d.ApplicationId == app.Id)
                            .ToList();

                        if (!relatedDiaries.Any()) continue;

                        foreach (var diary in relatedDiaries)
                        {
                            var appStartDate = app.StartDate.Date;
                            var appEndDate = app.EndDate.Date;
                            var startTol = appStartDate.AddDays(-1);
                            var endTol = appEndDate.AddDays(1);

                            // Primary entries (global) bu daily context için kullanýlmýyor, sadece diary.Entries baz alýnacak.
                            var effectiveEntries = new List<DiaryEntry>();

                            if (diary.Entries != null && diary.Entries.Any())
                            {
                                effectiveEntries.AddRange(diary.Entries.Where(e => e.Date.Date >= startTol && e.Date.Date <= endTol));
                                if (!effectiveEntries.Any())
                                {
                                    // Tarih filtresinden dolayý boþ kaldýysa tüm entries'i ekle
                                    effectiveEntries.AddRange(diary.Entries);
                                }
                            }

                            if (!effectiveEntries.Any()) continue; // Bu defterde hiç giriþ yoksa gösterme

                            var entriesWithStatus = effectiveEntries.Select(entry => new DiaryEntryWithStatus
                            {
                                Entry = entry,
                                StatusText = entry.IsApproved ? "Onaylandý" : "Beklemede",
                                StatusColor = entry.IsApproved ? "success" : "warning"
                            }).ToList();

                            studentDiaryGroups.Add(new StudentDiaryGroup
                            {
                                Student = student,
                                DiaryEntries = entriesWithStatus,
                                TotalEntries = effectiveEntries.Count,
                                PendingEntries = effectiveEntries.Count(d => !d.IsApproved),
                                ApprovedEntries = effectiveEntries.Count(d => d.IsApproved),
                                LastEntryDate = effectiveEntries.OrderByDescending(d => d.Date).FirstOrDefault()?.Date,
                                InternshipCompany = app.InternshipOffer?.Company?.CompanyName ?? diary.CompanyName ?? string.Empty,
                                InternshipStartDate = app.StartDate,
                                InternshipEndDate = app.EndDate,
                                DiaryId = diary.Id,
                                DiaryTitle = diary.Title,
                                IsRequired = diary.IsRequired
                            });
                        }
                    }
                }

                var filteredGroups = ApplyFilters(studentDiaryGroups, filters);

                viewModel.TotalItems = filteredGroups.Count();
                viewModel.StudentDiaries = filteredGroups
                    .Skip((page - 1) * viewModel.PageSize)
                    .Take(viewModel.PageSize)
                    .ToList();

                viewModel.AdvisorInfo = new AdvisorInfo
                {
                    UniversityName = advisor?.University?.Name ?? "Bilinmiyor",
                    Department = advisor?.Department ?? "Bölüm Belirtilmemiþ",
                    TotalStudentsInDepartment = students.Count,
                    StudentsWithDiaries = studentDiaryGroups.Count
                };
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Staj defterleri yüklenirken hata oluþtu: " + ex.Message;
            }

            return View("~/Views/Advisor/Diary/Index.cshtml", viewModel);
        }

        [HttpGet("StudentDiary/{studentId:int}")]
        public async Task<IActionResult> StudentDiary(int studentId)
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
                var student = await _userService.GetUserByIdAsync(studentId);

                var advisorUni = GetEffectiveUniversityId(advisor);
                var studentUni = GetEffectiveUniversityId(student);
                if (advisorUni == null || studentUni == null)
                { TempData["ErrorMessage"] = "Kullanýcý bilgileri eksik."; return RedirectToAction("Index"); }
                if (advisorUni != studentUni)
                { TempData["ErrorMessage"] = "Bu öðrencinin staj defterini görüntüleme yetkiniz yok."; return RedirectToAction("Index"); }
                if (!string.IsNullOrEmpty(advisor.Department) && !string.IsNullOrEmpty(student.Department) && !advisor.Department.Equals(student.Department, StringComparison.OrdinalIgnoreCase))
                { TempData["ErrorMessage"] = "Bu öðrencinin staj defterini görüntüleme yetkiniz yok."; return RedirectToAction("Index"); }

                var applications = await _applicationService.GetApplicationsByStudentIdAsync(studentId);
                var diariesAll = await _internshipDiaryService.GetDiariesByStudentIdAsync(studentId) ?? new List<InternshipDiary>();
                var approvedApp = applications.FirstOrDefault(a => a.Status == InternshipApplicationStatus.Approved && diariesAll.Any(d => d.IsRequired && d.ApplicationId == a.Id))
                                 ?? applications.FirstOrDefault(a => a.Status == InternshipApplicationStatus.Approved);
                if (approvedApp == null)
                { TempData["ErrorMessage"] = "Öðrencinin onaylanmýþ stajý bulunmuyor."; return RedirectToAction("Index"); }

                var startTol = approvedApp.StartDate.Date.AddDays(-1);
                var endTol = approvedApp.EndDate.Date.AddDays(1);

                var diaryEntries = (await _diaryService.GetDiaryEntriesByStudentIdAsync(studentId))
                    .Where(e => e.Date.Date >= startTol && e.Date.Date <= endTol)
                    .ToList();

                if (!diaryEntries.Any())
                {
                    var requiredDiaries = diariesAll.Where(d => d.IsRequired && d.ApplicationId == approvedApp.Id).ToList();
                    var collected = new List<DiaryEntry>();
                    foreach (var d in requiredDiaries)
                    {
                        if (d.Entries != null && d.Entries.Any())
                            collected.AddRange(d.Entries.Where(e => e.Date.Date >= startTol && e.Date.Date <= endTol));
                    }
                    diaryEntries = collected;

                    if (!diaryEntries.Any())
                    {
                        // Fallback tüm girdiler
                        foreach (var d in requiredDiaries)
                        {
                            if (d.Entries != null && d.Entries.Any()) diaryEntries.AddRange(d.Entries);
                        }
                    }
                }

                var entriesWithStatus = diaryEntries.Select(entry => new DiaryEntryWithStatus
                {
                    Entry = entry,
                    StatusText = entry.IsApproved ? "Onaylandý" : "Beklemede",
                    StatusColor = entry.IsApproved ? "success" : "warning"
                }).OrderByDescending(e => e.Entry.Date).ToList();

                var viewModel = new AdvisorDiaryDetailViewModel
                {
                    Student = student,
                    DiaryEntries = entriesWithStatus,
                    InternshipCompany = approvedApp.InternshipOffer?.Company,
                    Application = approvedApp,
                    Statistics = new DiaryStatistics
                    {
                        TotalEntries = diaryEntries.Count,
                        ApprovedEntries = diaryEntries.Count(d => d.IsApproved),
                        PendingEntries = diaryEntries.Count(d => !d.IsApproved),
                        CompletionRate = diaryEntries.Any() ? Math.Round((decimal)diaryEntries.Count(d => d.IsApproved) / diaryEntries.Count() * 100, 1) : 0,
                        FirstEntryDate = diaryEntries.OrderBy(d => d.Date).FirstOrDefault()?.Date,
                        LastEntryDate = diaryEntries.OrderByDescending(d => d.Date).FirstOrDefault()?.Date
                    }
                };

                return View("~/Views/Advisor/Diary/StudentDiary.cshtml", viewModel);
            }
            catch (Exception ex)
            { TempData["ErrorMessage"] = "Staj defteri yüklenirken hata oluþtu: " + ex.Message; return RedirectToAction("Index"); }
        }

        [HttpGet("EntryDetail/{entryId:int}")]
        public async Task<IActionResult> EntryDetail(int entryId)
        {
            try
            {
                var entry = await _diaryService.GetByIdAsync(entryId);
                if (entry == null)
                {
                    TempData["ErrorMessage"] = "Staj defteri giriþi bulunamadý.";
                    return RedirectToAction("Index");
                }

                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
                var student = await _userService.GetUserByIdAsync(entry.StudentId);

                var advisorUni = GetEffectiveUniversityId(advisor);
                var studentUni = GetEffectiveUniversityId(student);
                if (advisorUni == null || studentUni == null || advisorUni != studentUni)
                {
                    TempData["ErrorMessage"] = "Bu giriþi görüntüleme yetkiniz yok. (Farklý üniversite)";
                    return RedirectToAction("Index");
                }

                if (!string.IsNullOrEmpty(advisor.Department) && !string.IsNullOrEmpty(student.Department) &&
                    !advisor.Department.Equals(student.Department, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "Bu giriþi görüntüleme yetkiniz yok. (Farklý bölüm)";
                    return RedirectToAction("Index");
                }

                var allEntries = (await _diaryService.GetDiaryEntriesByStudentIdAsync(entry.StudentId)).OrderBy(e => e.Date).ToList();
                var currentIndex = allEntries.FindIndex(e => e.Id == entryId);

                var viewModel = new DiaryEntryDetailViewModel
                {
                    Entry = entry,
                    Student = student,
                    CanTakeAction = !entry.IsApproved,
                    PreviousEntryDate = currentIndex > 0 ? allEntries[currentIndex - 1].Date : null,
                    NextEntryDate = currentIndex < allEntries.Count - 1 ? allEntries[currentIndex + 1].Date : null
                };

                return View("~/Views/Advisor/Diary/EntryDetail.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Staj defteri giriþi yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost("ApproveEntry")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveEntry(int? entryId = null, int? id = null, string feedback = "")
        {
            try
            {
                var actualEntryId = entryId ?? id;
                if (!actualEntryId.HasValue)
                {
                    TempData["ErrorMessage"] = "Girdi ID'si belirtilmedi.";
                    return RedirectToAction("Index");
                }

                var entry = await _diaryService.GetByIdAsync(actualEntryId.Value);
                if (entry == null)
                {
                    TempData["ErrorMessage"] = "Staj defteri giriþi bulunamadý.";
                    return RedirectToAction("Index");
                }

                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
                var student = await _userService.GetUserByIdAsync(entry.StudentId);

                var advisorUni = GetEffectiveUniversityId(advisor);
                var studentUni = GetEffectiveUniversityId(student);
                if (advisorUni == null || studentUni == null || advisorUni != studentUni)
                {
                    TempData["ErrorMessage"] = "Bu iþlemi yapma yetkiniz yok. (Farklý üniversite)";
                    return RedirectToAction("Index");
                }

                if (!string.IsNullOrEmpty(advisor.Department) && !string.IsNullOrEmpty(student.Department) &&
                    !advisor.Department.Equals(student.Department, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "Bu iþlemi yapma yetkiniz yok. (Farklý bölüm)";
                    return RedirectToAction("Index");
                }

                if (entry.IsApproved)
                {
                    TempData["InfoMessage"] = "Bu giriþ zaten onaylanmýþ.";
                    return RedirectToAction("StudentDiary", new { studentId = entry.StudentId });
                }

                entry.IsApproved = true;
                entry.AdvisorFeedback = feedback;
                entry.ApprovedAt = DateTime.UtcNow;

                await _diaryService.UpdateAsync(entry);

                if (student != null)
                {
                    await _notificationService.CreateNotificationAsync(new Notification
                    {
                        UserId = student.Id,
                        Title = "Staj Defteri Onaylandý",
                        Message = $"{entry.Date:dd.MM.yyyy} tarihli staj defteri giriþiniz onaylandý. {(!string.IsNullOrEmpty(feedback) ? "Danýþman geri bildirimi: " + feedback : "")}",
                        Type = "Diary",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                TempData["SuccessMessage"] = "Staj defteri giriþi baþarýyla onaylandý.";
                return RedirectToAction("StudentDiary", new { studentId = entry.StudentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ýþlem sýrasýnda hata oluþtu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost("RejectEntry")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectEntry(DiaryActionViewModel model)
        {
            try
            {
                var entry = await _diaryService.GetByIdAsync(model.DiaryEntryId);
                if (entry == null)
                {
                    TempData["ErrorMessage"] = "Staj defteri giriþi bulunamadý.";
                    return RedirectToAction("Index");
                }

                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
                var student = await _userService.GetUserByIdAsync(entry.StudentId);

                var advisorUni = GetEffectiveUniversityId(advisor);
                var studentUni = GetEffectiveUniversityId(student);
                if (advisorUni == null || studentUni == null || advisorUni != studentUni)
                {
                    TempData["ErrorMessage"] = "Bu iþlemi yapma yetkiniz yok. (Farklý üniversite)";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(model.Feedback))
                {
                    TempData["ErrorMessage"] = "Red nedeni belirtilmelidir.";
                    return RedirectToAction("StudentDiary", new { studentId = entry.StudentId });
                }

                entry.AdvisorFeedback = model.Feedback;
                entry.IsApproved = false;

                await _diaryService.UpdateAsync(entry);

                if (student != null)
                {
                    await _notificationService.CreateNotificationAsync(new Notification
                    {
                        UserId = student.Id,
                        Title = "Staj Defteri Geri Dönüþ",
                        Message = $"{entry.Date:dd.MM.yyyy} tarihli staj defteri giriþiniz için geri dönüþ yapýldý. Danýþman yorumu: {model.Feedback}",
                        Type = "Diary",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                TempData["SuccessMessage"] = "Geri dönüþ baþarýyla gönderildi.";
                return RedirectToAction("StudentDiary", new { studentId = entry.StudentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ýþlem sýrasýnda hata oluþtu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet("GetPendingCount")]
        public async Task<IActionResult> GetPendingCount()
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
                var advisorUni = GetEffectiveUniversityId(advisor);
                if (advisorUni == null) return Json(new { pendingCount = 0 });

                var allStudents = await _userService.GetAllStudentsAsync();
                var studentsFromSameUniversity = allStudents.Where(s => GetEffectiveUniversityId(s) == advisorUni).ToList();

                List<User> students;
                if (string.IsNullOrEmpty(advisor.Department))
                    students = studentsFromSameUniversity;
                else
                {
                    students = studentsFromSameUniversity.Where(s => !string.IsNullOrEmpty(s.Department) && s.Department.Equals(advisor.Department, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!students.Any()) students = studentsFromSameUniversity.Where(s => string.IsNullOrEmpty(s.Department)).ToList();
                }

                var pendingCount = 0;
                foreach (var student in students)
                {
                    var entries = await _diaryService.GetDiaryEntriesByStudentIdAsync(student.Id);
                    pendingCount += entries.Count(e => !e.IsApproved);
                }

                return Json(new { pendingCount });
            }
            catch
            {
                return Json(new { pendingCount = 0 });
            }
        }

        [HttpGet("GetStudentDiaryEntries/{studentId:int}")]
        public async Task<IActionResult> GetStudentDiaryEntries(int studentId, int? diaryId)
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
                var student = await _userService.GetUserByIdAsync(studentId);
                var advisorUni = GetEffectiveUniversityId(advisor);
                var studentUni = GetEffectiveUniversityId(student);
                if (advisorUni == null || studentUni == null) return Json(new { success = false, message = "Kullanýcý bilgileri eksik." });
                if (advisorUni != studentUni) return Json(new { success = false, message = "Yetkisiz eriþim." });
                if (!string.IsNullOrEmpty(advisor.Department) && !string.IsNullOrEmpty(student.Department) && !advisor.Department.Equals(student.Department, StringComparison.OrdinalIgnoreCase))
                    return Json(new { success = false, message = "Yetkisiz eriþim." });

                var applications = await _applicationService.GetApplicationsByStudentIdAsync(studentId);
                var diariesAll = await _internshipDiaryService.GetDiariesByStudentIdAsync(studentId) ?? new List<InternshipDiary>();
                var approvedApps = applications.Where(a => a.Status == InternshipApplicationStatus.Approved).ToList();
                if (!approvedApps.Any()) return Json(new { success = false, message = "Onaylanmýþ staj yok." });

                // Belirli bir diaryId varsa sadece onu iþle
                InternshipDiary? targetDiary = null;
                if (diaryId.HasValue)
                {
                    targetDiary = diariesAll.FirstOrDefault(d => d.Id == diaryId.Value);
                    if (targetDiary == null) return Json(new { success = false, message = "Defter bulunamadý." });
                    if (!targetDiary.IsRequired) return Json(new { success = false, message = "Gönüllü deftere eriþim yok." });
                    var relatedApp = approvedApps.FirstOrDefault(a => a.Id == targetDiary.ApplicationId);
                    if (relatedApp == null) return Json(new { success = false, message = "Defter onaylý stajla iliþkisiz." });

                    var startTol = relatedApp.StartDate.Date.AddDays(-1);
                    var endTol = relatedApp.EndDate.Date.AddDays(1);
                    var entries = (targetDiary.Entries ?? new List<DiaryEntry>())
                        .Where(e => e.Date.Date >= startTol && e.Date.Date <= endTol)
                        .ToList();
                    if (!entries.Any() && targetDiary.Entries != null && targetDiary.Entries.Any()) entries = targetDiary.Entries.ToList();

                    var dataEntries = entries.Select(entry => new {
                        id = entry.Id,
                        title = entry.Title ?? string.Empty,
                        content = entry.Content ?? string.Empty,
                        date = entry.Date.ToString("yyyy-MM-dd"),
                        workingHours = entry.WorkingHours,
                        learnedTopics = entry.LearnedTopics ?? string.Empty,
                        challenges = entry.Challenges ?? string.Empty,
                        advisorFeedback = entry.AdvisorFeedback ?? string.Empty,
                        isApproved = entry.IsApproved,
                        createdAt = entry.CreatedAt
                    }).OrderByDescending(e => e.date).ToList();

                    return Json(new { success = true, entries = dataEntries, diaryId = targetDiary.Id, diaryTitle = targetDiary.Title });
                }

                // Eski davranýþ (diaryId belirtilmezse ilk zorunlu defter) - uyumluluk için
                var firstDiary = diariesAll.FirstOrDefault(d => d.IsRequired && approvedApps.Any(a => a.Id == d.ApplicationId));
                if (firstDiary == null) return Json(new { success = false, message = "Zorunlu defter bulunamadý." });
                var firstApp = approvedApps.First(a => a.Id == firstDiary.ApplicationId);
                var sTol = firstApp.StartDate.Date.AddDays(-1);
                var eTol = firstApp.EndDate.Date.AddDays(1);
                var fEntries = (firstDiary.Entries ?? new List<DiaryEntry>())
                    .Where(e => e.Date.Date >= sTol && e.Date.Date <= eTol)
                    .ToList();
                if (!fEntries.Any() && firstDiary.Entries != null && firstDiary.Entries.Any()) fEntries = firstDiary.Entries.ToList();

                var fData = fEntries.Select(entry => new {
                    id = entry.Id,
                    title = entry.Title ?? string.Empty,
                    content = entry.Content ?? string.Empty,
                    date = entry.Date.ToString("yyyy-MM-dd"),
                    workingHours = entry.WorkingHours,
                    learnedTopics = entry.LearnedTopics ?? string.Empty,
                    challenges = entry.Challenges ?? string.Empty,
                    advisorFeedback = entry.AdvisorFeedback ?? string.Empty,
                    isApproved = entry.IsApproved,
                    createdAt = entry.CreatedAt
                }).OrderByDescending(e => e.date).ToList();

                return Json(new { success = true, entries = fData, diaryId = firstDiary.Id, diaryTitle = firstDiary.Title });
            }
            catch (Exception ex)
            { return Json(new { success = false, message = "Hata: " + ex.Message }); }
        }

        private IEnumerable<StudentDiaryGroup> ApplyFilters(IEnumerable<StudentDiaryGroup> groups, DiaryFilters filters)
        {
            var query = groups.AsQueryable();

            if (!string.IsNullOrEmpty(filters.StudentName))
            {
                query = query.Where(g => g.Student.FullName.Contains(filters.StudentName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(filters.Status))
            {
                if (filters.Status == "pending")
                    query = query.Where(g => g.PendingEntries > 0);
                else if (filters.Status == "approved")
                    query = query.Where(g => g.ApprovedEntries > 0 && g.PendingEntries == 0);
            }

            if (filters.StartDate.HasValue)
                query = query.Where(g => g.LastEntryDate >= filters.StartDate.Value);

            if (filters.EndDate.HasValue)
                query = query.Where(g => g.LastEntryDate <= filters.EndDate.Value);

            switch (filters.SortBy?.ToLower())
            {
                case "student":
                    query = filters.SortOrder == "asc"
                        ? query.OrderBy(g => g.Student.FullName)
                        : query.OrderByDescending(g => g.Student.FullName);
                    break;
                case "entries":
                    query = filters.SortOrder == "asc"
                        ? query.OrderBy(g => g.TotalEntries)
                        : query.OrderByDescending(g => g.TotalEntries);
                    break;
                case "pending":
                    query = filters.SortOrder == "asc"
                        ? query.OrderBy(g => g.PendingEntries)
                        : query.OrderByDescending(g => g.PendingEntries);
                    break;
                default:
                    query = filters.SortOrder == "asc"
                        ? query.OrderBy(g => g.LastEntryDate)
                        : query.OrderByDescending(g => g.LastEntryDate);
                    break;
            }

            return query;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}