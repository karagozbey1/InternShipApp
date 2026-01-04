using Business.Interfaces;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Core.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebUI.Models.ViewModels.Student;
using WebUI.Models.Student;
using WebUI.Controllers; 
using Microsoft.Extensions.Logging;

namespace WebUI.Controllers.Student
{
    [Authorize]
    public class ApprovalController : BaseAppController
    {
        private readonly IInternshipDocumentService _documentService;
        private readonly IInternshipApprovalService _approvalService;
        private readonly IInternshipApplicationService _applicationService;
        private readonly IIUserService _userService;
        private readonly ILogger<ApprovalController> _logger;

        public ApprovalController(
            IInternshipDocumentService documentService,
            IInternshipApprovalService approvalService,
            IInternshipApplicationService applicationService,
            IIUserService userService,
            ILogger<ApprovalController> logger)
        {
            _documentService = documentService;
            _approvalService = approvalService;
            _applicationService = applicationService;
            _userService = userService;
            _logger = logger;
        }

        // GENEL INDEX SAYFASI
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                var forms = await _approvalService.GetApprovalFormsByStudentIdAsync(userId);

                var vm = new Models.Student.ApprovalFormListViewModel();
                vm.ApprovalForms = new List<Models.Student.ApprovalFormItemVM>();

                foreach (var f in forms.OrderByDescending(x => x.CreatedAt))
                {
                    var item = new Models.Student.ApprovalFormItemVM
                    {
                        Id = f.Id,
                        CompanyName = f.CompanyName ?? string.Empty,
                        Position = f.InternshipDescription ?? f.Application?.InternshipOffer?.Title ?? string.Empty,
                        StartDate = f.StartDate,
                        EndDate = f.EndDate,
                        AdvisorName = f.Advisor?.FullName ?? string.Empty,
                        Status = f.Status.ToString(),
                        CreatedAt = f.CreatedAt,
                        StudentNote = f.StudentNote ?? string.Empty,
                        AdvisorComment = f.AdvisorNote ?? string.Empty,
                        DocumentCount = 0 // Bu ayrý bir sorgu gerektirir, þimdilik 0
                    };

                    vm.ApprovalForms.Add(item);
                }

                // Ýstatistikleri hesapla
                vm.Stats = new Models.Student.ApprovalFormStatsVM
                {
                    TotalCount = vm.ApprovalForms.Count,
                    PendingCount = vm.ApprovalForms.Count(a => a.Status == "Pending"),
                    ApprovedCount = vm.ApprovalForms.Count(a => a.Status == "Approved"),
                    RejectedCount = vm.ApprovalForms.Count(a => a.Status == "Rejected"),
                    DraftCount = 0 // Draft durumu varsa buraya eklenebilir
                };

                return View("~/Views/Student/Approval/Index.cshtml", vm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Index error: {ex}");
                TempData["ErrorMessage"] = "Onay formlarý yüklenirken hata oluþtu.";
                return View("~/Views/Student/Approval/Index.cshtml", new Models.Student.ApprovalFormListViewModel());
            }
        }

        // New: SentForms - show student's sent approval forms with modern UI
        [HttpGet]
        public async Task<IActionResult> SentForms()
        {
            try
            {
                var userId = GetCurrentUserId();
                var forms = await _approvalService.GetApprovalFormsByStudentIdAsync(userId);

                var vm = new WebUI.Models.ViewModels.Student.ApprovalFormListViewModel();
                vm.ApprovalForms = new List<WebUI.Models.ViewModels.Student.ApprovalFormViewModel>();

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

                    var docs = docEntities.Select(d => new WebUI.Models.ViewModels.Student.DocumentVM
                    {
                        Id = d.Id,
                        Title = d.Title ?? d.FileName ?? "",
                        FileName = d.FileName ?? "",
                        FilePath = d.FilePath ?? "",
                        FileSize = d.FileSize ?? 0,
                        UploadedAt = d.UploadedAt
                    }).ToList();

                    var item = new WebUI.Models.ViewModels.Student.ApprovalFormViewModel
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

                // Only consider application IDs that are tied to non-rejected forms as "used".
                // If a form was rejected by the advisor, the company should become selectable again.
                var usedIds = existingForms
                    .Where(f => f.ApplicationId > 0 && f.Status != Core.Entity.InternshipApprovalStatus.Rejected)
                    .Select(f => f.ApplicationId)
                    .ToHashSet();

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

                        // populate workplace address from the related company if available; fallback to session
                        try
                        {
                            var addr = app.InternshipOffer?.Company?.Address;
                            vm.WorkplaceAddress = !string.IsNullOrWhiteSpace(addr) ? addr : (HttpContext.Session.GetString("WorkplaceAddress") ?? string.Empty);
                        }
                        catch
                        {
                            vm.WorkplaceAddress = HttpContext.Session.GetString("WorkplaceAddress") ?? string.Empty;
                        }
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
                // Populate top-level display fields so view can show summary (compatibility with older views)
                vm.CompanyName = vm.SelectedCompany?.CompanyName ?? HttpContext.Session.GetString("CompanyName") ?? string.Empty;
                vm.InternshipDescription = vm.SelectedCompany?.Position ?? HttpContext.Session.GetString("Position") ?? string.Empty;
                vm.StartDate = vm.SelectedCompany?.StartDate ?? (DateTime.TryParse(HttpContext.Session.GetString("StartDate"), out var sd1) ? sd1 : DateTime.UtcNow);
                vm.EndDate = vm.SelectedCompany?.EndDate ?? (DateTime.TryParse(HttpContext.Session.GetString("EndDate"), out var ed1) ? ed1 : DateTime.UtcNow.AddMonths(2));
                vm.WeeklyWorkHours = vm.WeeklyWorkHours > 0 ? vm.WeeklyWorkHours : 40;
                vm.InternshipCoordinator = vm.InternshipCoordinator ?? string.Empty;
                vm.CoordinatorPhone = vm.CoordinatorPhone ?? string.Empty;
                vm.CoordinatorEmail = vm.CoordinatorEmail ?? string.Empty;
                vm.StudentNote = HttpContext.Session.GetString("StudentNote") ?? vm.StudentNote ?? string.Empty;
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
        public async Task<IActionResult> Step4Submit(string StudentNote, string SelectedDocumentIdsFromForm = null, int? SelectedApplicationIdFromForm = null, int? SelectedAdvisorIdFromForm = null)
        {
            _logger?.LogDebug("Step4Submit invoked. Incoming StudentNote length: {len}", (StudentNote ?? string.Empty).Length);
            try
            {
                var selectedDocIds = HttpContext.Session.GetString("SelectedDocumentIds") ?? SelectedDocumentIdsFromForm;
                var selectedAppId = HttpContext.Session.GetInt32("SelectedApplicationId") ?? SelectedApplicationIdFromForm;
                var selectedAdvId = HttpContext.Session.GetInt32("SelectedAdvisorId") ?? SelectedAdvisorIdFromForm;

                _logger?.LogDebug("Session/Form SelectedDocumentIds={docs}, SelectedApplicationId={appId}, SelectedAdvisorId={advId}", selectedDocIds, selectedAppId, selectedAdvId);

                if (string.IsNullOrEmpty(selectedDocIds) || !selectedAppId.HasValue || !selectedAdvId.HasValue)
                {
                    TempData["ErrorMessage"] = "Önceki adýmlarý tamamlamanýz gerekiyor.";
                    _logger?.LogWarning("Step4Submit aborted: missing data. Docs={docs} AppId={app} AdvId={adv}", selectedDocIds, selectedAppId, selectedAdvId);
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
                    CompanyName = HttpContext.Session.GetString("CompanyName") ?? Request.Form["CompanyName"].FirstOrDefault() ?? "Þirket",
                    StartDate = DateTime.TryParse(HttpContext.Session.GetString("StartDate") ?? Request.Form["StartDate"].FirstOrDefault(), out var startDate) ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc) : DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.TryParse(HttpContext.Session.GetString("EndDate") ?? Request.Form["EndDate"].FirstOrDefault(), out var endDate) ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc) : DateTime.UtcNow.AddMonths(2),
                    WeeklyWorkHours = 40,
                    InternshipDescription = HttpContext.Session.GetString("Position") ?? Request.Form["Position"].FirstOrDefault() ?? "Staj Pozisyonu"
                };

                _logger?.LogDebug("Creating approval form for user {uid}, app {appId}", userId, approval.ApplicationId);

                await _approvalService.CreateApprovalFormAsync(approval);

                var documentIds = (selectedDocIds ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(s => int.TryParse(s, out var v) ? v : 0)
                                                 .Where(i => i > 0)
                                                 .ToArray();
                foreach (var docId in documentIds)
                {
                    _logger?.LogDebug("Linking document {doc} to approval {approvalId}", docId, approval.Id);
                    await _documentService.LinkDocumentToApprovalAsync(docId, approval.Id);
                }

                // Clear session
                foreach (var key in new[] { "SelectedDocumentIds", "SelectedApplicationId", "SelectedAdvisorId", "CompanyName", "StartDate", "EndDate", "Position" })
                    HttpContext.Session.Remove(key);

                TempData["SuccessMessage"] = "Onay formu baþarýyla gönderildi! Danýþman onayý bekleniyor.";
                _logger?.LogInformation("Step4Submit completed successfully for approval id {id}", approval.Id);
                
                // Baþarýlý gönderim sonrasý SentForms sayfasýna yönlendir
                return RedirectToAction("SentForms");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Step4Submit ERROR");
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
        public async Task<IActionResult> Details(int id, long? t = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Fresh data almak için GetApprovalFormWithDetailsAsync kullan
                var form = await _approvalService.GetApprovalFormWithDetailsAsync(id);
                
                if (form == null || form.StudentId != userId)
                {
                    return NotFound();
                }

                System.Diagnostics.Debug.WriteLine($"?? Details - Fresh Form Data:");
                System.Diagnostics.Debug.WriteLine($"   - Form ID: {form.Id}");
                System.Diagnostics.Debug.WriteLine($"   - Status: {form.Status}");
                System.Diagnostics.Debug.WriteLine($"   - StudentNote: '{form.StudentNote}'");
                System.Diagnostics.Debug.WriteLine($"   - RevisionNote: '{form.RevisionNote}'");
                System.Diagnostics.Debug.WriteLine($"   - RejectionReason: '{form.RejectionReason}'");

                List<InternshipDocument> docEntities = new List<InternshipDocument>();
                if (form.ApplicationId > 0)
                {
                    try
                    {
                        docEntities = await _documentService.GetDocumentsByApplicationIdAsync(form.ApplicationId);
                        System.Diagnostics.Debug.WriteLine($"?? Found {docEntities.Count} documents for ApplicationId: {form.ApplicationId}");
                    }
                    catch (Exception ex) 
                    { 
                        System.Diagnostics.Debug.WriteLine($"? Document loading error: {ex.Message}");
                        docEntities = new List<InternshipDocument>(); 
                    }
                }

                var vm = new WebUI.Models.ViewModels.Student.ApprovalFormViewModel
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
                    RevisionNote = form.RevisionNote ?? "",
                    RevisedAt = form.RevisedAt,
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? Error loading student/advisor info: {ex.Message}");
                    
                }

                System.Diagnostics.Debug.WriteLine($"?? Details ViewModel Created:");
                System.Diagnostics.Debug.WriteLine($"   - VM Status: {vm.Status}");
                System.Diagnostics.Debug.WriteLine($"   - VM StudentNote: '{vm.StudentNote}'");
                System.Diagnostics.Debug.WriteLine($"   - VM RevisionNote: '{vm.RevisionNote}'");
                System.Diagnostics.Debug.WriteLine($"   - VM Documents Count: {vm.Documents.Count}");

                return View("~/Views/Student/Approval/Details.cshtml", vm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Details error: {ex}");
                TempData["ErrorMessage"] = "Onay formu detaylar? yüklenirken hata olu?tu.";
                return RedirectToAction("SentForms");
            }
        }

        [HttpGet]
        // revizyon sayfasý 
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var forms = await _approvalService.GetApprovalFormsByStudentIdAsync(userId);
                var form = forms.FirstOrDefault(x => x.Id == id);
                
                if (form == null)
                    return NotFound();
                
                // Revision durumunu kontrol et
                bool isNewRevision = form.Status == InternshipApprovalStatus.Revision;
                bool isLegacyRevision = !string.IsNullOrEmpty(form.RejectionReason) && form.RejectionReason.StartsWith("REVISION:");
                
                if (!isNewRevision && !isLegacyRevision)
                {
                    TempData["ErrorMessage"] = "Bu form düzenlenemez.";
                    return RedirectToAction("Details", new { id });
                    }

                // Mevcut belgeleri getir
                List<InternshipDocument> docEntities = new List<InternshipDocument>();
                if (form.ApplicationId > 0)
                {
                    try
                    {
                        docEntities = await _documentService.GetDocumentsByApplicationIdAsync(form.ApplicationId);
                    }
                    catch { docEntities = new List<InternshipDocument>(); }
                }

                // Öðrencinin tüm belgelerini getir (belge eklemek için)
                var allDocs = await _documentService.GetDocumentsByStudentIdAsync(userId);

                // Legacy revision notunu çýkar
                string revisionNote = "";
                if (isNewRevision && !string.IsNullOrEmpty(form.RevisionNote))
                {
                    revisionNote = form.RevisionNote;
                }
                else if (isLegacyRevision)
                {
                    revisionNote = form.RejectionReason.Substring("REVISION:".Length).Trim();
                }

                var vm = new Models.Student.EditApprovalViewModel
                {
                    Id = form.Id,
                    CompanyName = form.CompanyName ?? "",
                    Position = form.InternshipDescription ?? "",
                    StartDate = form.StartDate,
                    EndDate = form.EndDate,
                    WeeklyWorkHours = form.WeeklyWorkHours,
                    InternshipDescription = form.InternshipDescription ?? "",
                    StudentNote = form.StudentNote ?? "",
                    Status = form.Status.ToString(),
                    CanEdit = isNewRevision || isLegacyRevision,
                    RevisionNote = revisionNote,
                    
                    // Mevcut belgeler
                    CurrentDocuments = docEntities.Select(d => new DocumentViewM
                    {
                        Id = d.Id,
                        Title = d.Title ?? d.FileName ?? "",
                        FileName = d.FileName ?? "",
                        FilePath = d.FilePath ?? "",
                        FileSize = d.FileSize ?? 0,
                        DocumentType = d.DocumentType?.ToString() ?? "",
                        UploadedAt = d.UploadedAt
                    }).ToList(),
                    
                    // Tüm belgeler (seçim için)
                    AvailableDocuments = allDocs.Select(d => new DocumentViewM
                    {
                        Id = d.Id,
                        Title = d.Title ?? d.FileName ?? "",
                        FileName = d.FileName ?? "",
                        DocumentType = d.DocumentType?.ToString() ?? "",
                        FileSize = d.FileSize ?? 0,
                        UploadedAt = d.UploadedAt
                    }).ToList(),
                    
                    SelectedDocumentIds = docEntities.Select(d => d.Id).ToList()
                };

                return View("~/Views/Student/Approval/Edit.cshtml", vm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Edit error: {ex}");
                TempData["ErrorMessage"] = "Düzenleme sayfasý yüklenirken hata oluþtu.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditApprovalViewModel model)
        {
            System.Diagnostics.Debug.WriteLine("?? ==========EDIT POST STARTED========");
            System.Diagnostics.Debug.WriteLine($"?? Form ID: {model.Id}");
            System.Diagnostics.Debug.WriteLine($"?? Student Note: '{model.StudentNote}'");
            System.Diagnostics.Debug.WriteLine($"?? Selected Document IDs: [{string.Join(", ", model.SelectedDocumentIds ?? new List<int>())}]");
            System.Diagnostics.Debug.WriteLine($"?? Document Count: {model.SelectedDocumentIds?.Count ?? 0}");
            
            try
            {
                if (!ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("? ModelState is invalid!");
                    foreach (var error in ModelState)
                    {
                        System.Diagnostics.Debug.WriteLine($"   - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                    
                    // Model yeniden doldur
                    var currentUserId = GetCurrentUserId();
                    var allDocs = await _documentService.GetDocumentsByStudentIdAsync(currentUserId);
                    model.AvailableDocuments = allDocs.Select(d => new DocumentViewM
                    {
                        Id = d.Id,
                        Title = d.Title ?? d.FileName ?? "",
                        FileName = d.FileName ?? "",
                        DocumentType = d.DocumentType?.ToString() ?? "",
                        FileSize = d.FileSize ?? 0,
                        UploadedAt = d.UploadedAt
                    }).ToList();

                    return View("~/Views/Student/Approval/Edit.cshtml", model);
                }

                var userId = GetCurrentUserId();
                System.Diagnostics.Debug.WriteLine($"?? Current User ID: {userId}");
                
                var forms = await _approvalService.GetApprovalFormsByStudentIdAsync(userId);
                var form = forms.FirstOrDefault(x => x.Id == model.Id);
                
                if (form == null)
                {
                    System.Diagnostics.Debug.WriteLine("? Form not found!");
                    return NotFound();
                }

                System.Diagnostics.Debug.WriteLine($"?? Form found - Status: {form.Status}, ApplicationId: {form.ApplicationId}");

                // Revision durumunu kontrol et 
                bool isNewRevision = form.Status == InternshipApprovalStatus.Revision;
                bool isLegacyRevision = !string.IsNullOrEmpty(form.RejectionReason) && form.RejectionReason.StartsWith("REVISION:");
                
                System.Diagnostics.Debug.WriteLine($"?? Revision Status - New: {isNewRevision}, Legacy: {isLegacyRevision}");
                
                if (!isNewRevision && !isLegacyRevision)
                {
                    System.Diagnostics.Debug.WriteLine("? Form cannot be edited!");
                    TempData["ErrorMessage"] = "Bu form d?zenlenemez.";
                    return RedirectToAction("Details", new { id = model.Id });
                }

                System.Diagnostics.Debug.WriteLine($"?? BEFORE UPDATE:");
                System.Diagnostics.Debug.WriteLine($"   - Current StudentNote: '{form.StudentNote}'");
                System.Diagnostics.Debug.WriteLine($"   - Current Status: {form.Status}");
                System.Diagnostics.Debug.WriteLine($"   - Current RevisionNote: '{form.RevisionNote}'");

                // Sadece belgeleri ve student note'u güncelle
                form.StudentNote = model.StudentNote;
                
                // Status'u tekrar Pending yap (revizyon sonras?)
                form.Status = InternshipApprovalStatus.Pending;
                
                // Hem yeni hem eski revision formatýný temizle
                form.RevisionNote = null;
                form.RevisedAt = null;
                if (isLegacyRevision)
                {
                    form.RejectionReason = null;
                    form.RejectedAt = null;
                }

                System.Diagnostics.Debug.WriteLine($"?? AFTER CHANGES:");
                System.Diagnostics.Debug.WriteLine($"   - New StudentNote: '{form.StudentNote}'");
                System.Diagnostics.Debug.WriteLine($"   - New Status: {form.Status}");
                System.Diagnostics.Debug.WriteLine($"   - New RevisionNote: '{form.RevisionNote}'");

                System.Diagnostics.Debug.WriteLine("?? Updating approval form...");
                await _approvalService.UpdateApprovalFormAsync(form);
                System.Diagnostics.Debug.WriteLine("? Approval form updated successfully!");

                // Belge güncellemelerini yap
                if (model.SelectedDocumentIds != null && model.SelectedDocumentIds.Any() && form.ApplicationId > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"?? Updating documents for ApplicationId: {form.ApplicationId}");
                    System.Diagnostics.Debug.WriteLine($"?? New document selection: [{string.Join(", ", model.SelectedDocumentIds)}]");
                    
                    try
                    {
                        // ?nce mevcut belgeleri kald?r
                        var existingDocs = await _documentService.GetDocumentsByApplicationIdAsync(form.ApplicationId);
                        System.Diagnostics.Debug.WriteLine($"?? Found {existingDocs.Count} existing documents");
                        
                        foreach (var doc in existingDocs)
                        {
                            System.Diagnostics.Debug.WriteLine($"??? Unlinking document: {doc.Id} ({doc.Title})");
                            await _documentService.UnlinkDocumentFromApplicationAsync(doc.Id, form.ApplicationId);
                        }

                        // Yeni belgeleri ekle
                        foreach (var docId in model.SelectedDocumentIds)
                        {
                            System.Diagnostics.Debug.WriteLine($"?? Linking document: {docId}");
                            await _documentService.LinkDocumentToApplicationAsync(docId, form.ApplicationId);
                        }
                        
                        System.Diagnostics.Debug.WriteLine("? Document updates completed successfully!");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"? Document update error: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"? Stack trace: {ex.StackTrace}");
                       
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("?? No document updates needed (empty selection or invalid ApplicationId)");
                }

                System.Diagnostics.Debug.WriteLine("?? Edit operation completed successfully!");
                TempData["SuccessMessage"] = "Onay formu Baþarýyla Güncellendi ve tekrar de?erlendirilmeye g?nderildi.";

                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? CRITICAL ERROR in Edit POST: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"? Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "G?ncelleme s?ras?nda hata olu?tu.";
                return RedirectToAction("Details", new { id = model.Id });
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("?? ==========EDIT POST FINISHED========");


            }
        }

        // ====== DELETE APPROVAL FORM ======
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var form = await _approvalService.GetApprovalFormWithDetailsAsync(id);

                if (form == null)
                {
                    TempData["ErrorMessage"] = "Baþvuru bulunamadý.";
                    return RedirectToAction("SentForms");
                }

                // Yetki kontrolü - sadece kendi baþvurusunu silebilir
                if (form.StudentId != userId)
                {
                    TempData["ErrorMessage"] = "Bu iþlemi yapmaya yetkiniz yok.";
                    return RedirectToAction("SentForms");
                }

                // Sadece Pending, Revision veya Rejected durumundaki baþvurular silinebilir
                if (form.Status == InternshipApprovalStatus.Approved)
                {
                    TempData["ErrorMessage"] = "Onaylanmýþ baþvurular silinemez.";
                    return RedirectToAction("SentForms");
                }

                // Baþvuruyu sil
                await _approvalService.DeleteApprovalFormAsync(id);

                TempData["SuccessMessage"] = "Baþvuru baþarýyla silindi.";
                _logger?.LogInformation("Approval form {id} deleted by user {userId}", id, userId);

                return RedirectToAction("SentForms");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Delete approval form error for id {id}", id);
                TempData["ErrorMessage"] = "Baþvuru silinirken bir hata oluþtu.";
                return RedirectToAction("SentForms");
            }
        }
    }
}
