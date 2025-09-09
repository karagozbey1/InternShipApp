using Business.Interfaces;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Core.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebUI.Models.ViewModels.Student;
using WebUI.Controllers; 

namespace WebUI.Controllers.Student
{
    [Authorize]
    public class ApprovalController : BaseAppController
    {
        private readonly IInternshipDocumentService _documentService;
        private readonly IInternshipApprovalService _approvalService;
        private readonly IInternshipApplicationService _applicationService;
        private readonly IIUserService _userService;

        public ApprovalController(
            IInternshipDocumentService documentService,
            IInternshipApprovalService approvalService,
            IInternshipApplicationService applicationService,
            IIUserService userService)
        {
            _documentService = documentService;
            _approvalService = approvalService;
            _applicationService = applicationService;
            _userService = userService;
        }

        // GENEL INDEX SAYFASI
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Mevcut onay formlarýný listele
            var userId = GetCurrentUserId();
            var approvals = await _approvalService.GetApprovalFormsByStudentIdAsync(userId);
            
            // Explicitly return the view under Views/Student/Approval to match project layout
            return View("~/Views/Student/Approval/Index.cshtml", approvals);
        }

        // New: SentForms - show student's sent approval forms with modern UI
        [HttpGet]
        public async Task<IActionResult> SentForms()
        {
            try
            {
                var userId = GetCurrentUserId();
                var forms = await _approvalService.GetApprovalFormsByStudentIdAsync(userId);

                var vm = new ApprovalFormListViewModel();
                vm.ApprovalForms = new List<ApprovalFormViewModel>();

                foreach (var f in forms.OrderByDescending(x => x.CreatedAt))
                {
                    // fetch documents tied to the original application (if any)
                    List<InternshipDocument> docEntities = new List<InternshipDocument>();
                    if (f.ApplicationId > 0)
                    {
                        try
                        {
                            docEntities = await _documentService.GetDocumentsByApplicationIdAsync(f.ApplicationId);
                        }
                        catch
                        {
                            docEntities = new List<InternshipDocument>();
                        }
                    }

                    var docs = docEntities.Select(d => new DocumentVM
                    {
                        Id = d.Id,
                        Title = d.Title ?? d.FileName ?? "",
                        FileName = d.FileName ?? "",
                        FilePath = d.FilePath ?? "",
                        FileSize = d.FileSize ?? 0,
                        UploadedAt = d.UploadedAt
                    }).ToList();

                    var item = new ApprovalFormViewModel
                    {
                        Id = f.Id,
                        CompanyName = f.CompanyName ?? string.Empty,
                        OfferTitle = f.Application?.InternshipOffer?.Title ?? f.InternshipDescription ?? string.Empty,
                        StartDate = f.StartDate,
                        EndDate = f.EndDate,
                        InternshipCoordinator = f.InternshipCoordinator ?? string.Empty,
                        CoordinatorPhone = f.CoordinatorPhone ?? string.Empty,
                        Status = f.Status,
                        CreatedAt = f.CreatedAt,
                        StudentNote = f.StudentNote ?? string.Empty,
                        AdvisorNote = f.AdvisorNote ?? string.Empty,
                        RejectionReason = f.RejectionReason ?? string.Empty,
                        AdvisorName = f.Advisor?.FullName ?? string.Empty,
                        AdvisorDepartment = f.Advisor?.Department ?? string.Empty,
                        Documents = docs
                    };

                    vm.ApprovalForms.Add(item);
                }

                vm.TotalCount = vm.ApprovalForms.Count;
                vm.PendingCount = vm.ApprovalForms.Count(a => a.Status == InternshipApprovalStatus.Pending);
                vm.ApprovedCount = vm.ApprovalForms.Count(a => a.Status == InternshipApprovalStatus.Approved);
                vm.RejectedCount = vm.ApprovalForms.Count(a => a.Status == InternshipApprovalStatus.Rejected);

                return View("~/Views/Student/Approval/SentForms.cshtml", vm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SentForms error: {ex}");
                TempData["ErrorMessage"] = "Gönderilen baþvurular yüklenirken hata oluþtu.";
                return RedirectToAction("Index");
            }
        }

        // ====== STEP 1: DOCUMENT SELECTION ======
        [HttpGet]
        public async Task<IActionResult> Step1Documents()
        {
            try
            {
                var userId = GetCurrentUserId();
                var docs = await _documentService.GetDocumentsByStudentIdAsync(userId);

                System.Diagnostics.Debug.WriteLine($"?? Step 1: User ID: {userId}");
                System.Diagnostics.Debug.WriteLine($"?? Step 1: Documents found: {docs.Count}");

                var vm = new ModernApprovalViewModel
                {
                    StudentDocuments = docs.Select(d => new StudentDocumentVM
                    {
                        Id = d.Id,
                        Title = d.Title ?? "",
                        DocumentType = d.DocumentType.ToString(),
                        FileSize = d.FileSize ?? 0,
                        UploadedAt = d.UploadedAt
                    }).ToList()
                };

                return View("~/Views/Student/Approval/Step1Documents.cshtml", vm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? Step1Documents Error: {ex}");
                TempData["ErrorMessage"] = "Belge sayfasý yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step1Documents(int[] SelectedDocumentIds)
        {
            if (SelectedDocumentIds == null || SelectedDocumentIds.Length == 0)
            {
                TempData["ErrorMessage"] = "En az bir belge seçmelisiniz.";
                return RedirectToAction("Step1Documents");
            }

            // Session'a kaydet
            HttpContext.Session.SetString("SelectedDocumentIds", string.Join(",", SelectedDocumentIds));
            
            System.Diagnostics.Debug.WriteLine($"?? Step 1 Completed: {SelectedDocumentIds.Length} documents selected");
            
            return RedirectToAction("Step2Company");
        }

        // ====== STEP 2: COMPANY SELECTION ======
        [HttpGet]
        public async Task<IActionResult> Step2Company()
        {
            try
            {
                var userId = GetCurrentUserId();
                var approvedApps = await _applicationService.GetApprovedByStudentIdAsync(userId);

                // All existing approval forms for this student
                List<InternshipApprovalForm> existingForms;
                try { existingForms = await _approvalService.GetApprovalFormsByStudentIdAsync(userId) ?? new(); }
                catch { existingForms = new List<InternshipApprovalForm>(); }

                var usedIds = existingForms.Where(f => f.ApplicationId > 0).Select(f => f.ApplicationId).ToHashSet();

                var selectableApps = new List<InternshipApplication>();
                foreach (var app in approvedApps)
                {
                    var hasForm = usedIds.Contains(app.Id);
                    bool canByService = false;
                    try { canByService = await _approvalService.CanCreateApprovalFormAsync(app.Id); } catch { }

                    System.Diagnostics.Debug.WriteLine($"Step2Company: AppId={app.Id} HasForm={hasForm} ServiceCanCreate={canByService} Status={app.Status}");

                    // We only allow applications that have NO existing form AND service also says can create
                    if (!hasForm && canByService)
                        selectableApps.Add(app);
                }

                System.Diagnostics.Debug.WriteLine($"Step2Company: totalApproved={approvedApps.Count} selectable={selectableApps.Count}");

                var vm = new ModernApprovalViewModel
                {
                    ApprovedApplications = selectableApps.Select(a => new ApprovedApplicationVM
                    {
                        Id = a.Id,
                        CompanyName = a.InternshipOffer?.Company?.CompanyName ?? "Test Þirket",
                        Position = a.InternshipOffer?.Title ?? "Test Pozisyon",
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        Status = a.Status.ToString(),
                        CreatedAt = a.CreatedAt
                    }).ToList()
                };

                return View("~/Views/Student/Approval/Step2Company.cshtml", vm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Step2Company Error: {ex}");
                TempData["ErrorMessage"] = "Firma sayfasý yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction("Step1Documents");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step2Company(int SelectedApplicationId, string CompanyName, string StartDate, string EndDate, string Position)
        {
            if (SelectedApplicationId <= 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir firma seçin.";
                return RedirectToAction("Step2Company");
            }

            // Session'a kaydet
            HttpContext.Session.SetInt32("SelectedApplicationId", SelectedApplicationId);
            HttpContext.Session.SetString("CompanyName", CompanyName ?? "");
            HttpContext.Session.SetString("StartDate", StartDate ?? "");
            HttpContext.Session.SetString("EndDate", EndDate ?? "");
            HttpContext.Session.SetString("Position", Position ?? "");
            
            System.Diagnostics.Debug.WriteLine($"?? Step 2 Completed: Application {SelectedApplicationId} selected");
            
            return RedirectToAction("Step3Advisor");
        }

        // ====== STEP 3: ADVISOR SELECTION ======
        [HttpGet]
        public IActionResult Step3Advisor()
        {
            var vm = new ModernApprovalViewModel();
            return View("~/Views/Student/Approval/Step3Advisor.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step3Advisor(int SelectedAdvisorId)
        {
            if (SelectedAdvisorId <= 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir danýþman seçin.";
                return RedirectToAction("Step3Advisor");
            }

            // Session'a kaydet
            HttpContext.Session.SetInt32("SelectedAdvisorId", SelectedAdvisorId);
            
            System.Diagnostics.Debug.WriteLine($"????? Step 3 Completed: Advisor {SelectedAdvisorId} selected");
            
            return RedirectToAction("Step4Review");
        }

        // ====== STEP 4: REVIEW AND SUBMIT ======
        [HttpGet]
        public async Task<IActionResult> Step4Review()
        {
            var selectedDocIds = HttpContext.Session.GetString("SelectedDocumentIds");
            var selectedAppId = HttpContext.Session.GetInt32("SelectedApplicationId");
            var selectedAdvId = HttpContext.Session.GetInt32("SelectedAdvisorId");

            if (string.IsNullOrEmpty(selectedDocIds) || !selectedAppId.HasValue || !selectedAdvId.HasValue)
            {
                TempData["ErrorMessage"] = "Önceki adýmlarý tamamlamanýz gerekiyor.";
                return RedirectToAction("Step1Documents");
            }

            var vm = new ModernApprovalViewModel();

            try
            {
                var docIds = selectedDocIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => int.TryParse(s, out var v) ? v : 0)
                                           .Where(i => i > 0)
                                           .ToArray();

                vm.SelectedDocuments = new List<DocumentVM>();
                foreach (var id in docIds)
                {
                    var d = await _documentService.GetDocumentByIdAsync(id);
                    if (d != null)
                    {
                        vm.SelectedDocuments.Add(new DocumentVM
                        {
                            Id = d.Id,
                            Title = d.Title ?? d.FileName ?? string.Empty,
                            FileName = d.FileName ?? string.Empty,
                            FilePath = d.FilePath ?? string.Empty,
                            FileSize = d.FileSize ?? 0,
                            UploadedAt = d.UploadedAt
                        });
                    }
                }
                vm.SelectedDocumentIds = docIds.ToList();

                // Baþvuru
                try
                {
                    var app = await _applicationService.GetApplicationWithDetailsAsync(selectedAppId.Value);
                    if (app != null)
                    {
                        vm.SelectedCompany = new ApprovedApplicationVM
                        {
                            Id = app.Id,
                            ApplicationId = app.Id,
                            CompanyName = app.InternshipOffer?.Company?.CompanyName ?? string.Empty,
                            Position = app.InternshipOffer?.Title ?? string.Empty,
                            StartDate = app.StartDate,
                            EndDate = app.EndDate,
                            Status = app.Status.ToString(),
                            CreatedAt = app.CreatedAt
                        };
                        vm.SelectedApplicationId = app.Id;
                    }
                }
                catch { }

                // Danýþman
                try
                {
                    var adv = await _userService.GetUserByIdAsync(selectedAdvId.Value);
                    if (adv != null)
                    {
                        vm.SelectedAdvisor = new AdvisorVM
                        {
                            Id = adv.Id,
                            FullName = adv.FullName ?? string.Empty,
                            Email = adv.Email ?? string.Empty,
                            Department = adv.Department ?? string.Empty,
                            University = adv.University?.Name ?? string.Empty
                        };
                        vm.SelectedAdvisorId = adv.Id;
                    }
                }
                catch { }
            }
            catch
            {
                TempData["ErrorMessage"] = "Önizleme hazýrlanýrken bir hata oluþtu.";
                return RedirectToAction("Step1Documents");
            }

            return View("~/Views/Student/Approval/Step4Review.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step4Submit(string StudentNote)
        {
            try
            {
                var selectedDocIds = HttpContext.Session.GetString("SelectedDocumentIds");
                var selectedAppId = HttpContext.Session.GetInt32("SelectedApplicationId");
                var selectedAdvId = HttpContext.Session.GetInt32("SelectedAdvisorId");

                if (string.IsNullOrEmpty(selectedDocIds) || !selectedAppId.HasValue || !selectedAdvId.HasValue)
                {
                    TempData["ErrorMessage"] = "Önceki adýmlarý tamamlamanýz gerekiyor.";
                    return RedirectToAction("Step1Documents");
                }

                var userId = GetCurrentUserId();

                var approval = new InternshipApprovalForm
                {
                    StudentId = userId,
                    ApplicationId = selectedAppId.Value,
                    AdvisorId = selectedAdvId.Value,
                    StudentNote = StudentNote ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    Status = InternshipApprovalStatus.Pending,
                    CompanyName = HttpContext.Session.GetString("CompanyName") ?? "Þirket",
                    StartDate = DateTime.TryParse(HttpContext.Session.GetString("StartDate"), out var startDate) ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc) : DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.TryParse(HttpContext.Session.GetString("EndDate"), out var endDate) ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc) : DateTime.UtcNow.AddMonths(2),
                    WeeklyWorkHours = 40,
                    InternshipDescription = HttpContext.Session.GetString("Position") ?? "Staj Pozisyonu"
                };

                await _approvalService.CreateApprovalFormAsync(approval);

                var documentIds = selectedDocIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(s => int.TryParse(s, out var v) ? v : 0)
                                                 .Where(i => i > 0)
                                                 .ToArray();
                foreach (var docId in documentIds)
                {
                    await _documentService.LinkDocumentToApprovalAsync(docId, approval.Id);
                }

                // Session temizle
                foreach (var key in new[] { "SelectedDocumentIds", "SelectedApplicationId", "SelectedAdvisorId", "CompanyName", "StartDate", "EndDate", "Position" })
                    HttpContext.Session.Remove(key);

                TempData["SuccessMessage"] = "Onay formu baþarýyla gönderildi. Danýþman onayý bekleniyor.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["ErrorMessage"] = "Onay formu gönderilirken bir hata oluþtu. Lütfen daha sonra tekrar deneyin.";
                return RedirectToAction("Step4Review");
            }
        }

        // ====== LEGACY METHODS (COMPATIBILITY) ======
        [HttpGet]
        public async Task<IActionResult> ModernCreate()
        {
            // Yeni step-based sistemine redirect
            return RedirectToAction("Step1Documents");
        }

        // ADVISOR SEARCH (AJAX)
        [HttpGet]
        public async Task<IActionResult> SearchAdvisors(string query)
        {
            try
            {
                var userId = GetCurrentUserId();
                var student = await _userService.GetUserWithUniversityAsync(userId);
                if (student == null || student.UniversityId == null)
                    return Json(new object[0]);

                var advisors = await _userService.GetUsersByRoleAsync(UserRole.Advisor);

                // Filter by same university and optional same department
                var filtered = advisors.Where(a => a.UniversityId == student.UniversityId);
                if (!string.IsNullOrEmpty(student.Department))
                {
                    filtered = filtered.Where(a => string.Equals(a.Department, student.Department, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(a.Department));
                }

                if (!string.IsNullOrWhiteSpace(query))
                {
                    filtered = filtered.Where(a =>
                        (!string.IsNullOrEmpty(a.FullName) && a.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(a.Email) && a.Email.Contains(query, StringComparison.OrdinalIgnoreCase)));
                }

                var result = filtered
                    .OrderBy(a => a.FullName)
                    .Select(a => new {
                        id = a.Id,
                        fullName = a.FullName ?? string.Empty,
                        email = a.Email ?? string.Empty,
                        department = a.Department ?? string.Empty
                    }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new object[0]);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var forms = await _approvalService.GetApprovalFormsByStudentIdAsync(userId);
                var form = forms.FirstOrDefault(x => x.Id == id);
                if (form == null) return NotFound();

                List<InternshipDocument> docEntities = new List<InternshipDocument>();
                if (form.ApplicationId > 0)
                {
                    try
                    {
                        docEntities = await _documentService.GetDocumentsByApplicationIdAsync(form.ApplicationId);
                    }
                    catch { docEntities = new List<InternshipDocument>(); }
                }

                var vm = new ApprovalFormViewModel
                {
                    Id = form.Id,
                    CompanyName = form.CompanyName ?? "",
                    OfferTitle = form.Application?.InternshipOffer?.Title ?? form.InternshipDescription ?? "",
                    StartDate = form.StartDate,
                    EndDate = form.EndDate,
                    InternshipCoordinator = form.InternshipCoordinator ?? "",
                    CoordinatorPhone = form.CoordinatorPhone ?? "",
                    CoordinatorEmail = form.CoordinatorEmail ?? "",
                    Status = form.Status,
                    CreatedAt = form.CreatedAt,
                    StudentNote = form.StudentNote ?? "",
                    AdvisorNote = form.AdvisorNote ?? "",
                    RejectionReason = form.RejectionReason ?? "",
                    AdvisorName = form.Advisor?.FullName ?? "",
                    AdvisorDepartment = form.Advisor?.Department ?? "",
                    ApprovedAt = form.ApprovedAt,
                    RejectedAt = form.RejectedAt,
                    Documents = docEntities.Select(d => new DocumentVM
                    {
                        Id = d.Id,
                        Title = d.Title ?? d.FileName ?? "",
                        FileName = d.FileName ?? "",
                        FilePath = d.FilePath ?? "",
                        FileSize = d.FileSize ?? 0,
                        UploadedAt = d.UploadedAt
                    }).ToList()
                };

                // populate student display info if available
                try
                {
                    if (form.StudentId > 0)
                    {
                        var student = await _userService.GetUserWithUniversityAsync(form.StudentId);
                        if (student != null)
                        {
                            vm.StudentName = student.FullName;
                            vm.StudentEmail = student.Email;
                            vm.StudentDepartment = student.Department;
                            vm.StudentGrade = student.Grade;
                            vm.StudentUniversity = student.University?.Name;
                        }
                    }

                    // populate advisor contact info: prefer form.Advisor navigation, otherwise fetch by id
                    if (form.Advisor != null)
                    {
                        vm.AdvisorName = form.Advisor.FullName ?? vm.AdvisorName;
                        vm.AdvisorDepartment = form.Advisor.Department ?? vm.AdvisorDepartment;
                        vm.AdvisorEmail = form.Advisor.Email ?? vm.AdvisorEmail;
                        vm.AdvisorPhone = form.Advisor.Phone ?? vm.AdvisorPhone;
                    }
                    else if (form.AdvisorId > 0)
                    {
                        var adv = await _userService.GetUserByIdAsync(form.AdvisorId.Value);
                        if (adv != null)
                        {
                            vm.AdvisorName = adv.FullName ?? vm.AdvisorName;
                            vm.AdvisorDepartment = adv.Department ?? vm.AdvisorDepartment;
                            vm.AdvisorEmail = adv.Email ?? vm.AdvisorEmail;
                            vm.AdvisorPhone = adv.Phone ?? vm.AdvisorPhone;
                        }
                    }
                }
                catch
                {
                    // ignore student/advisor loading failure
                }

                return View("~/Views/Student/Approval/Details.cshtml", vm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Details error: {ex}");
                TempData["ErrorMessage"] = "Onay formu detaylarý yüklenirken hata oluþtu.";
                return RedirectToAction("SentForms");
            }
        }
    }
}
