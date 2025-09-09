using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Interfaces;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Business.Interfaces.Order;
using WebUI.Models.ViewModels.Advisor;
using Core.Entity;

namespace WebUI.Controllers.Advisor
{
    [Authorize(Roles = "Advisor")]
    public class AdvisorApplicationController : Controller
    {
        private readonly IInternshipApprovalService _approvalService;
        private readonly IIUserService _userService;
        private readonly IINotificationService _notificationService;

        public AdvisorApplicationController(
            IInternshipApprovalService approvalService,
            IIUserService userService,
            IINotificationService notificationService)
        {
            _approvalService = approvalService;
            _userService = userService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(string searchTerm = "", InternshipApprovalStatus? statusFilter = null)
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

                // ayný ünivertesideki ve  ayný bölümdeki öðrencileri al
                var allStudents = await _userService.GetAllStudentsAsync();
                var relevantStudents = allStudents
                    .Where(s => s.UniversityId == advisor.UniversityId && 
                               (string.IsNullOrEmpty(advisor.Department) || s.Department == advisor.Department))
                    .ToList();

                var studentIds = relevantStudents.Select(s => s.Id).ToList();

                // Get all approval forms from these students
                var allApprovalForms = new List<InternshipApprovalForm>();
                foreach (var studentId in studentIds)
                {
                    try
                    {
                        var forms = await _approvalService.GetApprovalFormsByStudentIdAsync(studentId);
                        if (forms != null)
                        {
                            allApprovalForms.AddRange(forms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting approval forms for student {studentId}: {ex.Message}");
                    }
                }

                // Apply filters
                var filteredForms = allApprovalForms.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredForms = filteredForms.Where(f => 
                        f.CompanyName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (f.Student != null && f.Student.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (f.InternshipCoordinator != null && f.InternshipCoordinator.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
                }

                if (statusFilter.HasValue)
                {
                    filteredForms = filteredForms.Where(f => f.Status == statusFilter.Value);
                }

                // Create view models
                var applicationViewModels = new List<AdvisorApplicationCardViewModel>();
                
                foreach (var form in filteredForms.OrderByDescending(f => f.CreatedAt))
                {
                    var student = relevantStudents.FirstOrDefault(s => s.Id == form.StudentId);
                    if (student != null)
                    {
                        applicationViewModels.Add(new AdvisorApplicationCardViewModel
                        {
                            Id = form.Id,
                            StudentName = student.FullName ?? "Bilinmiyor",
                            StudentEmail = student.Email ?? "",
                            StudentNumber = $"STU{student.Id:000000}", // Generate a student number
                            CompanyName = form.CompanyName ?? "Belirtilmemiþ",
                            Position = form.InternshipDescription ?? "Belirtilmemiþ",
                            StartDate = form.StartDate,
                            EndDate = form.EndDate,
                            WeeklyWorkHours = form.WeeklyWorkHours,
                            Status = form.Status,
                            CreatedAt = form.CreatedAt,
                            StudentNote = form.StudentNote,
                            AdvisorNote = form.AdvisorNote,
                            RejectionReason = form.RejectionReason,
                            WorkplaceAddress = form.WorkplaceAddress,
                            InternshipCoordinator = form.InternshipCoordinator,
                            CoordinatorPhone = form.CoordinatorPhone,
                            CoordinatorEmail = form.CoordinatorEmail,
                            ApprovedAt = form.ApprovedAt,
                            RejectedAt = form.RejectedAt
                        });
                    }
                }

                var viewModel = new AdvisorApplicationListViewModel
                {
                    Applications = applicationViewModels,
                    SearchTerm = searchTerm,
                    StatusFilter = statusFilter,
                    TotalCount = applicationViewModels.Count,
                    PendingCount = applicationViewModels.Count(a => a.Status == InternshipApprovalStatus.Pending),
                    ApprovedCount = applicationViewModels.Count(a => a.Status == InternshipApprovalStatus.Approved),
                    RejectedCount = applicationViewModels.Count(a => a.Status == InternshipApprovalStatus.Rejected)
                };

                return View("~/Views/Advisor/Application/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Baþvurular yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction("Index", "AdvisorDashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
                
                if (advisor?.UniversityId == null)
                {
                    return BadRequest("Danýþman bilgileri eksik.");
                }

                var approvalForm = await _approvalService.GetApprovalFormWithDetailsAsync(id);
                if (approvalForm == null)
                {
                    return NotFound("Baþvuru bulunamadý.");
                }

                var student = await _userService.GetUserByIdAsync(approvalForm.StudentId);
                if (student == null || student.UniversityId != advisor.UniversityId)
                {
                    return Forbid("Bu baþvuruya eriþim yetkiniz yok.");
                }

                var viewModel = new AdvisorApplicationCardViewModel
                {
                    Id = approvalForm.Id,
                    StudentName = student.FullName ?? "Bilinmiyor",
                    StudentEmail = student.Email ?? string.Empty,
                    StudentNumber = $"STU{student.Id:000000}",
                    CompanyName = approvalForm.CompanyName ?? "Belirtilmemiþ",
                    Position = approvalForm.InternshipDescription ?? "Belirtilmemiþ",
                    StartDate = approvalForm.StartDate,
                    EndDate = approvalForm.EndDate,
                    WeeklyWorkHours = approvalForm.WeeklyWorkHours,
                    Status = approvalForm.Status,
                    CreatedAt = approvalForm.CreatedAt,
                    StudentNote = approvalForm.StudentNote,
                    AdvisorNote = approvalForm.AdvisorNote,
                    RejectionReason = approvalForm.RejectionReason,
                    WorkplaceAddress = approvalForm.WorkplaceAddress,
                    InternshipCoordinator = approvalForm.InternshipCoordinator,
                    CoordinatorPhone = approvalForm.CoordinatorPhone,
                    CoordinatorEmail = approvalForm.CoordinatorEmail,
                    ApprovedAt = approvalForm.ApprovedAt,
                    RejectedAt = approvalForm.RejectedAt,
                    StudentDepartment = student.Department,
                    StudentUniversity = student.University?.Name,
                    StudentGrade = student.Grade
                };

                return Json(viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest("Detaylar yüklenirken hata oluþtu: " + ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveApplication(int id, string advisorNote = "")
        {
            try
            {
                await _approvalService.ApproveFormAsync(id, advisorNote);
                
                // Send notification to student
                var approvalForm = await _approvalService.GetApprovalFormWithDetailsAsync(id);
                if (approvalForm != null)
                {
                    await _notificationService.CreateNotificationAsync(new Notification
                    {
                        UserId = approvalForm.StudentId,
                        Title = "Staj Onay Formunuz Onaylandý",
                        Message = $"Staj onay formunuz danýþmanýnýz tarafýndan onaylandý. {(!string.IsNullOrEmpty(advisorNote) ? "Danýþman notu: " + advisorNote : "")}",
                        Type = "Approval",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                return Json(new { success = true, message = "Baþvuru baþarýyla onaylandý." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Onay iþlemi sýrasýnda hata oluþtu: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApplication(int id, string rejectionReason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rejectionReason))
                {
                    return Json(new { success = false, message = "Red nedeni belirtilmelidir." });
                }

                await _approvalService.RejectFormAsync(id, rejectionReason);
                
                // Send notification to student
                var approvalForm = await _approvalService.GetApprovalFormWithDetailsAsync(id);
                if (approvalForm != null)
                {
                    await _notificationService.CreateNotificationAsync(new Notification
                    {
                        UserId = approvalForm.StudentId,
                        Title = "Staj Onay Formunuz Reddedildi",
                        Message = $"Staj onay formunuz danýþmanýnýz tarafýndan reddedildi. Red nedeni: {rejectionReason}",
                        Type = "Approval",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                return Json(new { success = true, message = "Baþvuru reddedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Red iþlemi sýrasýnda hata oluþtu: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnForRevision(int id, string revisionNote)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(revisionNote))
                {
                    return Json(new { success = false, message = "Geri dönüþ notu belirtilmelidir." });
                }

                // For revision, we can use reject with a special note indicating it's for revision
                await _approvalService.RejectFormAsync(id, $"REVISION: {revisionNote}");
                
                // Send notification to student
                var approvalForm = await _approvalService.GetApprovalFormWithDetailsAsync(id);
                if (approvalForm != null)
                {
                    await _notificationService.CreateNotificationAsync(new Notification
                    {
                        UserId = approvalForm.StudentId,
                        Title = "Staj Onay Formunuz Revizyona Gönderildi",
                        Message = $"Staj onay formunuz revizyona gönderildi. Geri dönüþ notu: {revisionNote}",
                        Type = "Approval",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                return Json(new { success = true, message = "Baþvuru revizyona gönderildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Geri dönüþ iþlemi sýrasýnda hata oluþtu: " + ex.Message });
            }
        }

        public async Task<IActionResult> ApplicationDetail(int id)
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
                if (advisor?.UniversityId == null)
                {
                    TempData["ErrorMessage"] = "Danýþman bilgileri eksik.";
                    return RedirectToAction("Index");
                }

                var approvalForm = await _approvalService.GetApprovalFormWithDetailsAsync(id);
                if (approvalForm == null)
                {
                    TempData["ErrorMessage"] = "Baþvuru bulunamadý.";
                    return RedirectToAction("Index");
                }

                var student = await _userService.GetUserByIdAsync(approvalForm.StudentId);
                if (student == null || student.UniversityId != advisor.UniversityId)
                {
                    TempData["ErrorMessage"] = "Bu baþvuruya eriþim yetkiniz yok.";
                    return RedirectToAction("Index");
                }

                var viewModel = new AdvisorApplicationCardViewModel
                {
                    Id = approvalForm.Id,
                    StudentName = student.FullName ?? "Bilinmiyor",
                    StudentEmail = student.Email ?? string.Empty,
                    StudentNumber = $"STU{student.Id:000000}",
                    CompanyName = approvalForm.CompanyName ?? "Belirtilmemiþ",
                    Position = approvalForm.InternshipDescription ?? "Belirtilmemiþ",
                    StartDate = approvalForm.StartDate,
                    EndDate = approvalForm.EndDate,
                    WeeklyWorkHours = approvalForm.WeeklyWorkHours,
                    Status = approvalForm.Status,
                    CreatedAt = approvalForm.CreatedAt,
                    StudentNote = approvalForm.StudentNote,
                    AdvisorNote = approvalForm.AdvisorNote,
                    RejectionReason = approvalForm.RejectionReason,
                    WorkplaceAddress = approvalForm.WorkplaceAddress,
                    InternshipCoordinator = approvalForm.InternshipCoordinator,
                    CoordinatorPhone = approvalForm.CoordinatorPhone,
                    CoordinatorEmail = approvalForm.CoordinatorEmail,
                    ApprovedAt = approvalForm.ApprovedAt,
                    RejectedAt = approvalForm.RejectedAt,
                    StudentDepartment = student.Department,
                    StudentUniversity = student.University?.Name,
                    StudentGrade = student.Grade
                };

                return View("~/Views/Advisor/Application/ApplicationDetail.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Detaylar yüklenirken hata oluþtu: " + ex.Message;
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