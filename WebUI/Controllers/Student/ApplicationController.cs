using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Core.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models.ViewModels.Student;
using AppEntity = Core.Entity;

namespace WebUI.Controllers.Student
{
    [Authorize(Roles = "Student")]
    public class ApplicationController : Controller
    {
        private readonly IInternshipApplicationService _applicationService;
        private readonly IInternshipOfferService _offerService;
        private readonly IInternshipDocumentService _documentService;
        private readonly IInternshipApprovalService _approvalService;
        private readonly IInternshipDiaryService _internshipDiaryService;

        public ApplicationController(
            IInternshipApplicationService applicationService,
            IInternshipOfferService offerService,
            IInternshipDocumentService documentService,
            IInternshipApprovalService approvalService,
            IInternshipDiaryService internshipDiaryService)
        {
            _applicationService = applicationService;
            _offerService = offerService;
            _documentService = documentService;
            _approvalService = approvalService;
            _internshipDiaryService = internshipDiaryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, string? statusFilter)
        {
            var userId = GetCurrentUserId();
            var apps = await _applicationService.GetApplicationsByStudentIdAsync(userId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                apps = apps
                    .Where(a =>
                        (a.InternshipOffer?.Company?.CompanyName ?? "").Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (a.InternshipOffer?.Title ?? "").Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                apps = apps.Where(a => a.Status.ToString().Equals(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var vm = new ApplicationListViewModel
            {
                Applications = (await Task.WhenAll(apps.Select(async a => await MapToItemVm(a)))).ToList(),
                SearchTerm = searchTerm ?? "",
                StatusFilter = ParseCoreStatus(statusFilter)
            };

            return View("~/Views/Student/ApplicationView.cshtml", vm);
        }

        /// Aktif staj ilanlarını listeler - Yeni başvuru oluştur butonuna tıklandığında açılır
        [HttpGet]
        public async Task<IActionResult> Browse()
        {
            var offers = await _offerService.GetAllActiveOffersAsync();
            var vm = new OfferBrowseViewModel
            {
                Offers = offers.ToList()
            };

            return View("~/Views/Student/Application/Browse.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? offerId)
        {
            var offers = await _offerService.GetAllActiveOffersAsync();

            var available = offers.Select(o => new OfferSelectViewModel
            {
                OfferId = o.Id,
                Title = o.Title,
                CompanyName = o.Company?.CompanyName ?? ""
            }).ToList();

            // If an offerId was provided but it's not present in active offers (e.g. expired), fetch it and include so form can show it
            if (offerId.HasValue && offerId.Value > 0 && !available.Any(a => a.OfferId == offerId.Value))
            {
                var extra = await _offerService.GetByIdAsync(offerId.Value);
                if (extra != null)
                {
                    available.Insert(0, new OfferSelectViewModel
                    {
                        OfferId = extra.Id,
                        Title = extra.Title,
                        CompanyName = extra.Company?.CompanyName ?? ""
                    });
                }
            }

            var vm = new ApplicationCreateViewModel
            {
                AvailableOffers = available,
                SelectedOfferId = offerId ?? 0
            };

            return View("~/Views/Student/Application/Create.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AvailableOffers = (await _offerService.GetAllActiveOffersAsync())
                        .Select(o => new OfferSelectViewModel
                        {
                            OfferId = o.Id,
                            Title = o.Title,
                            CompanyName = o.Company?.CompanyName ?? ""
                        }).ToList();

                    return View("~/Views/Student/Application/Create.cshtml", model);
                }

                var userId = GetCurrentUserId();

                // Server-side date validation to avoid DB out-of-range errors
                var minSqlDate = new DateTime(1753, 1, 1);
                if (model.StartDate == default || model.EndDate == default)
                {
                    ModelState.AddModelError("", "Başlangıç ve bitiş tarihleri geçerli olmalıdır.");
                    model.AvailableOffers = (await _offerService.GetAllActiveOffersAsync())
                        .Select(o => new OfferSelectViewModel
                        {
                            OfferId = o.Id,
                            Title = o.Title,
                            CompanyName = o.Company?.CompanyName ?? ""
                        }).ToList();

                    return View("~/Views/Student/Application/Create.cshtml", model);
                }

                if (model.StartDate < minSqlDate || model.EndDate < minSqlDate)
                {
                    ModelState.AddModelError("", "Tarih veritabanının desteklediği aralıkta olmalıdır.");
                    model.AvailableOffers = (await _offerService.GetAllActiveOffersAsync())
                        .Select(o => new OfferSelectViewModel
                        {
                            OfferId = o.Id,
                            Title = o.Title,
                            CompanyName = o.Company?.CompanyName ?? ""
                        }).ToList();
                    return View("~/Views/Student/Application/Create.cshtml", model);
                }

                if (model.StartDate >= model.EndDate)
                {
                    ModelState.AddModelError("", "Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");
                    model.AvailableOffers = (await _offerService.GetAllActiveOffersAsync())
                        .Select(o => new OfferSelectViewModel
                        {
                            OfferId = o.Id,
                            Title = o.Title,
                            CompanyName = o.Company?.CompanyName ?? ""
                        }).ToList();
                    return View("~/Views/Student/Application/Create.cshtml", model);
                }

                // Offer kontrolü
                var offer = await _offerService.GetByIdAsync(model.SelectedOfferId);
                if (offer == null || !offer.IsActive || (offer.ApplicationDeadline.HasValue && offer.ApplicationDeadline.Value < DateTime.UtcNow))
                {
                    ModelState.AddModelError("", "Seçilen staj teklifi geçerli değil.");
                    model.AvailableOffers = (await _offerService.GetAllActiveOffersAsync())
                        .Select(o => new OfferSelectViewModel
                        {
                            OfferId = o.Id,
                            Title = o.Title,
                            CompanyName = o.Company?.CompanyName ?? ""
                        }).ToList();
                    return View("~/Views/Student/Application/Create.cshtml", model);
                }

                // Check if user already applied to this offer
                var existingApplications = await _applicationService.GetApplicationsByStudentIdAsync(userId);
                if (existingApplications.Any(a => a.InternshipOfferId == model.SelectedOfferId))
                {
                    TempData["ErrorMessage"] = "Bu pozisyona daha önce başvuru yapmışsınız.";
                    return RedirectToAction(nameof(Index));
                }

                var app = new AppEntity.InternshipApplication
                {
                    StudentId = userId,
                    InternshipOfferId = model.SelectedOfferId,
                    // Ensure dates are UTC to satisfy PostgreSQL timestamp with time zone
                    StartDate = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(model.EndDate, DateTimeKind.Utc),
                    Status = InternshipApplicationStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    StudentNote = model.Description,
                    AdvisorId = null, // Başlangıçta danışman atanmamış
                    IsCompulsory = model.IsCompulsory
                };

                Console.WriteLine($"Creating application: StudentId={app.StudentId}, OfferId={app.InternshipOfferId}");

                // Başvuruyu oluştur
                await _applicationService.AddApplicationAsync(app);

                Console.WriteLine($"Application created with ID: {app.Id}");

                // Başvuru oluşturulduktan sonra seçilen belgeleri başvuruya ilişkilendir
                if (model.SelectedDocumentIds != null && model.SelectedDocumentIds.Count > 0)
                {
                    try
                    {
                        foreach (var docId in model.SelectedDocumentIds)
                        {
                            var document = await _documentService.GetDocumentByIdAsync(docId);
                            if (document != null && document.StudentId == userId)
                            {
                                document.ApplicationId = app.Id;
                                await _documentService.UpdateDocumentAsync(document);
                            }
                        }
                        Console.WriteLine($"Linked {model.SelectedDocumentIds.Count} documents to application {app.Id}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Document linking error: {ex}");
                        // Belge ekleme hatası olsa bile başvuru oluşturmaya devam et
                    }
                }

                TempData["SuccessMessage"] = "Başvurunuz başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log full exception server-side to temp file for debugging
                try
                {
                    var logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CreateApplication_Error.log");
                    System.IO.File.AppendAllText(logPath, DateTime.UtcNow.ToString("o") + " - Application creation exception:\n" + ex.ToString() + "\n\n");
                }
                catch { }

                Console.WriteLine($"Application creation error: {ex}");
                Console.WriteLine($"Inner exception: {ex.InnerException}");

                // Provide detailed error to view in development
                ViewData["DetailedError"] = ex.ToString();

                // Detaylı hata mesajı
                var errorMessage = "Başvuru oluşturulurken bir hata oluştu.";
                if (ex.InnerException != null)
                {
                    var innerMsg = ex.InnerException.Message;
                    if (innerMsg.Contains("FOREIGN KEY constraint"))
                    {
                        errorMessage = "Geçersiz staj teklifi seçimi. Lütfen tekrar deneyin.";
                    }
                    else if (innerMsg.Contains("UNIQUE constraint"))
                    {
                        errorMessage = "Bu pozisyona daha önce başvuru yapmışsınız.";
                    }
                    else if (innerMsg.Contains("null"))
                    {
                        errorMessage = "Bazı zorunlu alanlar eksik. Lütfen formu kontrol edin.";
                    }
                    else
                    {
                        errorMessage += $" Detay: {innerMsg}";
                    }
                }

                TempData["ErrorMessage"] = errorMessage;

                // Model'i yeniden doldur
                model.AvailableOffers = (await _offerService.GetAllActiveOffersAsync())
                    .Select(o => new OfferSelectViewModel
                    {
                        OfferId = o.Id,
                        Title = o.Title,
                        CompanyName = o.Company?.CompanyName ?? ""
                    }).ToList();

                return View("~/Views/Student/Application/Create.cshtml", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var app = await _applicationService.GetApplicationWithDetailsAsync(id);
            if (app == null) return NotFound();

            var vm = await MapToDetailsVm(app);
            return View("~/Views/Student/Application/Details.cshtml", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(int id)
        {
            var ok = await _applicationService.WithdrawApplicationAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Başvuru geri çekildi." : "İşlem gerçekleştirilemedi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDocumentsToApplication(int applicationId, List<int> documentIds)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                if (applicationId <= 0)
                {
                    TempData["ErrorMessage"] = "Geçersiz başvuru ID'si.";
                    return RedirectToAction("Index");
                }

                if (documentIds == null || !documentIds.Any())
                {
                    TempData["ErrorMessage"] = "En az bir belge seçmelisiniz.";
                    return RedirectToAction("Details", new { id = applicationId });
                }
                
                // Başvurunun kullanıcıya ait olduğunu kontrol et
                var application = await _applicationService.GetApplicationWithDetailsAsync(applicationId);
                if (application == null || application.StudentId != userId)
                {
                    TempData["ErrorMessage"] = "Geçersiz başvuru.";
                    return RedirectToAction("Index");
                }

                // Belgelerin kullanıcıya ait olduğunu kontrol et
                foreach (var docId in documentIds)
                {
                    var document = await _documentService.GetDocumentByIdAsync(docId);
                    if (document == null || document.StudentId != userId)
                    {
                        TempData["ErrorMessage"] = "Geçersiz belge seçimi.";
                        return RedirectToAction("Details", new { id = applicationId });
                    }

                    // Belgeyi başvuruya ilişkilendir
                    document.ApplicationId = applicationId;
                    await _documentService.UpdateDocumentAsync(document);
                }

                TempData["SuccessMessage"] = "Belgeler başarıyla eklendi.";
                return RedirectToAction("Details", new { id = applicationId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddDocumentsToApplication error: {ex}");
                TempData["ErrorMessage"] = "Belgeler eklenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetApplicationDocuments(int applicationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Başvurunun kullanıcıya ait olduğunu kontrol et
                var application = await _applicationService.GetApplicationWithDetailsAsync(applicationId);
                if (application == null || application.StudentId != userId)
                {
                    return BadRequest("Geçersiz başvuru.");
                }

                var documents = await _documentService.GetDocumentsByApplicationIdAsync(applicationId);
                
                var result = documents.Select(d => new
                {
                    id = d.Id,
                    fileName = d.FileName,
                    title = d.Title,
                    filePath = d.FilePath,
                    fileSize = d.FileSize.HasValue ? $"{Math.Round((double)d.FileSize.Value / 1024 / 1024, 2)} MB" : "Bilinmiyor",
                    uploadDate = d.UploadedAt.ToString("dd.MM.yyyy")
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetApplicationDocuments error: {ex}");
                return Json(new List<object>());
            }
        }

        // ---------------- Helpers ----------------

        private async Task<ApplicationItemVM> MapToItemVm(AppEntity.InternshipApplication a)
        {
            var docCount = 0;
            try
            {
                if (a.InternshipDocuments != null)
                    docCount = a.InternshipDocuments.Count;
                else
                    docCount = (await _documentService.GetDocumentsByApplicationIdAsync(a.Id)).Count;
            }
            catch { }

            bool canCreateApproval = false;
            try { canCreateApproval = await _approvalService.CanCreateApprovalFormAsync(a.Id); } catch { }

            var advisorName = string.Empty;
            try
            {
                if (a.Advisor != null)
                {
                    // User entity'de FirstName/LastName yoksa Username kullan
                    var first = a.Advisor.GetType().GetProperty("FirstName")?.GetValue(a.Advisor)?.ToString();
                    var last = a.Advisor.GetType().GetProperty("LastName")?.GetValue(a.Advisor)?.ToString();
                    if (!string.IsNullOrWhiteSpace(first) || !string.IsNullOrWhiteSpace(last))
                        advisorName = ($"{first} {last}").Trim();
                    else
                        advisorName = a.Advisor.FullName ?? string.Empty;
                }
            }
            catch { }

            return new ApplicationItemVM
            {
                Id = a.Id,
                CompanyName = a.InternshipOffer?.Company?.CompanyName ?? "",
                Position = a.InternshipOffer?.Title ?? "",
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Status = a.Status,
                AppliedDate = a.CreatedAt,
                Description = a.StudentNote ?? "",
                DocumentCount = docCount,
                CanCreateApprovalForm = canCreateApproval,
                AdvisorName = advisorName,
                IsCompulsory = a.IsCompulsory
            };
        }

        private async Task<ApplicationDetailsViewModel> MapToDetailsVm(AppEntity.InternshipApplication a)
        {
            // Onay formu oluşturulabilir mi kontrol et
            var canCreateApprovalForm = await _approvalService.CanCreateApprovalFormAsync(a.Id);

            // Check if an InternshipDiary exists for this application
            int? diaryId = null;
            try
            {
                var diaries = await _internshipDiaryService.GetDiariesByStudentIdAsync(a.StudentId);
                var linked = diaries.FirstOrDefault(d => d.ApplicationId.HasValue && d.ApplicationId.Value == a.Id);
                if (linked != null) diaryId = linked.Id;
            }
            catch { }

            // Belgeler navigation gelmemişse servisten çek
            var documentsSource = a.InternshipDocuments;
            if (documentsSource == null)
            {
                try { documentsSource = await _documentService.GetDocumentsByApplicationIdAsync(a.Id); } catch { documentsSource = new List<Core.Entity.InternshipDocument>(); }
            }

            return new ApplicationDetailsViewModel
            {
                Id = a.Id,
                CompanyName = a.InternshipOffer?.Company?.CompanyName ?? "",
                Position = a.InternshipOffer?.Title ?? "",
                Description = a.StudentNote ?? a.InternshipOffer?.Description ?? "",
                AppliedDate = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Status = a.Status,
                CanCreateApprovalForm = canCreateApprovalForm,
                Documents = documentsSource?.Select(d => new DocumentViewModel
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    FileSize = d.FileSize ?? 0,
                    DocumentType = "Other",
                    UploadedAt = d.UploadedAt,
                    Title = d.Title
                }).ToList() ?? new List<DocumentViewModel>(),
                OfferDetails = a.InternshipOffer == null ? null : new OfferDetailsViewModel
                {
                    Id = a.InternshipOffer.Id,
                    Title = a.InternshipOffer.Title,
                    CompanyName = a.InternshipOffer.Company?.CompanyName ?? "",
                    Description = a.InternshipOffer.Description ?? "",
                    Requirements = a.InternshipOffer.Requirements ?? "",
                    Benefits = "",
                    StartDate = a.InternshipOffer.Period?.StartDate ?? a.StartDate,
                    EndDate = a.InternshipOffer.Period?.EndDate ?? a.EndDate,
                    ApplicationDeadline = a.InternshipOffer.ApplicationDeadline ?? DateTime.UtcNow,
                    WorkingHours = "",
                    Location = a.InternshipOffer.City ?? "",
                    IsActive = a.InternshipOffer.IsActive,
                    CreatedAt = a.InternshipOffer.CreatedAt,
                    // Salary mapping
                    Salary = a.InternshipOffer.MonthlySalary,
                    SalaryCurrency = a.InternshipOffer.MonthlySalaryCurrency
                },
                HasDiary = diaryId.HasValue,
                DiaryId = diaryId,
                IsCompulsory = a.IsCompulsory
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDiary(int id)
        {
            var userId = GetCurrentUserId();
            var app = await _applicationService.GetApplicationWithDetailsAsync(id);
            if (app == null || app.StudentId != userId)
            {
                TempData["ErrorMessage"] = "Başvuru bulunamadı veya yetkiniz yok.";
                return RedirectToAction(nameof(Index));
            }

            if (app.Status != InternshipApplicationStatus.Approved)
            {
                TempData["ErrorMessage"] = "Sadece onaylanmış başvurular için defter oluşturulabilir.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Check existing diary
            var diaries = await _internshipDiaryService.GetDiariesByStudentIdAsync(userId);
            var linked = diaries.FirstOrDefault(d => d.ApplicationId.HasValue && d.ApplicationId.Value == id);
            if (linked != null)
            {
                return RedirectToAction("Detail", "DiaryNotebook", new { id = linked.Id });
            }

            // Create diary
            var newDiary = new Core.Entity.InternshipDiary
            {
                StudentId = userId,
                Title = app.InternshipOffer?.Title ?? "Staj Defteri",
                CompanyName = app.InternshipOffer?.Company?.CompanyName,
                Department = app.InternshipOffer?.Field,
                InternshipStartDate = app.StartDate,
                InternshipEndDate = app.EndDate,
                ApplicationId = app.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsRequired = true,
                IsCompleted = false
            };

            var created = await _internshipDiaryService.CreateDiaryAsync(newDiary);
            if (created != null)
            {
                return RedirectToAction("Detail", "DiaryNotebook", new { id = created.Id });
            }

            TempData["ErrorMessage"] = "Defter oluşturulurken hata oluştu.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private static InternshipApplicationStatus? ParseCoreStatus(string? status)
        {
            return (status ?? "").ToLowerInvariant() switch
            {
                "approved" => InternshipApplicationStatus.Approved,
                "rejected" => InternshipApplicationStatus.Rejected,
                "pending" => InternshipApplicationStatus.Pending,
                _ => null
            };
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> OfferDetails(int id, string? source)
        {
            var offer = await _offerService.GetByIdAsync(id);
            if (offer == null)
            {
                TempData["ErrorMessage"] = "İlan bulunamadı.";
                return RedirectToAction(nameof(Browse));
            }

            // determine showEcom as before
            var showEcom = false;
            if (!string.IsNullOrEmpty(source))
            {
                var s = source.ToLowerInvariant();
                if (s == "home" || s == "student") showEcom = true;
            }
            else
            {
                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer) && (referer.Contains("/Home", StringComparison.OrdinalIgnoreCase) || referer.Contains("/Application", StringComparison.OrdinalIgnoreCase) || referer.Contains("/Student", StringComparison.OrdinalIgnoreCase)))
                {
                    showEcom = true;
                }
            }

            // related offers
            var related = new List<OfferSelectViewModel>();

            // Do not load related offers for company users — company should only see its own offer details
            if (!User.IsInRole("Company") && offer.CompanyId > 0)
            {
                var companyOffers = await _offerService.GetOffersByCompanyIdAsync(offer.CompanyId);
                related = companyOffers
                    .Where(o => o.Id != offer.Id && o.IsActive)
                    .OrderBy(o => Guid.NewGuid()) // randomize order so it won't always show the same items
                    .Take(5) // limit to 5 items
                    .Select(o => new OfferSelectViewModel
                    {
                        OfferId = o.Id,
                        Title = o.Title,
                        CompanyName = o.Company?.CompanyName ?? "",
                        City = o.City ?? "",
                        Field = o.Field ?? "",
                        ApplicationDeadline = o.ApplicationDeadline
                    }).ToList();
            }

            var model = new OfferDetailsViewModel
            {
                Id = offer.Id,
                Title = offer.Title,
                CompanyName = offer.Company?.CompanyName ?? "",
                Description = offer.Description ?? "",
                Requirements = offer.Requirements ?? "",
                Benefits = string.Empty,
                StartDate = offer.Period?.StartDate ?? DateTime.MinValue,
                EndDate = offer.Period?.EndDate ?? DateTime.MinValue,
                ApplicationDeadline = offer.ApplicationDeadline ?? DateTime.MinValue,
                WorkingHours = offer.Duration.ToString(),
                Location = offer.City ?? string.Empty,
                Position = offer.Field ?? string.Empty,
                Quota = offer.AvailablePositions > 0 ? offer.AvailablePositions : 1,
                ReferenceNumber = string.Empty,
                Tags = new List<string>(),
                CompanyPhone = offer.Company?.Phone ?? string.Empty,
                CompanyEmail = offer.Company?.Email ?? string.Empty,
                CompanyWebsite = offer.Company?.Website ?? string.Empty,
                CompanyAddress = offer.Company?.Address ?? string.Empty,
                IsActive = offer.IsActive,
                CreatedAt = offer.CreatedAt,
                Salary = offer.MonthlySalary,
                SalaryCurrency = offer.MonthlySalaryCurrency,
                RelatedOffers = related,
                ShowEcommerceMode = showEcom,
                CompanyDescription = offer.Company?.Description ?? ""
            };

            return View("~/Views/Student/Application/OfferDetails.cshtml", model);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/offers/{id}")]
        public async Task<IActionResult> GetOfferJson(int id)
        {
            var offer = await _offerService.GetByIdAsync(id);
            if (offer == null) return NotFound();

            var data = new
            {
                id = offer.Id,
                dateMode = (int)offer.DateMode,
                periodStart = offer.Period?.StartDate.ToString("yyyy-MM-dd"),
                periodEnd = offer.Period?.EndDate.ToString("yyyy-MM-dd"),
                fixedStart = offer.Period == null && offer.DateMode == Core.Entity.DateSelectionMode.FixedRange ? offer.Period?.StartDate.ToString("yyyy-MM-dd") : (offer.Period != null ? offer.Period.StartDate.ToString("yyyy-MM-dd") : null),
                fixedEnd = offer.Period == null && offer.DateMode == Core.Entity.DateSelectionMode.FixedRange ? offer.Period?.EndDate.ToString("yyyy-MM-dd") : (offer.Period != null ? offer.Period.EndDate.ToString("yyyy-MM-dd") : null),
                // Fallback: if offer has manual StartDate/EndDate in DB (not currently), include them
                // applicationDeadline
                applicationDeadline = offer.ApplicationDeadline?.ToString("yyyy-MM-dd")
            };

            return Json(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var app = await _applicationService.GetByIdAsync(id);
                if (app == null || app.StudentId != userId)
                {
                    TempData["ErrorMessage"] = "Başvuru bulunamadı veya yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                if (app.Status != InternshipApplicationStatus.Rejected)
                {
                    TempData["ErrorMessage"] = "Sadece geri çekilmiş başvurular silinebilir.";
                    return RedirectToAction(nameof(Index));
                }

                await _applicationService.DeleteApplicationAsync(id);
                TempData["SuccessMessage"] = "Başvuru başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete application error: {ex}");
                TempData["ErrorMessage"] = "Başvuru silinirken hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }

    // AJAX request model
    public class AddDocumentsRequest
    {
        public int ApplicationId { get; set; }
        public List<int> DocumentIds { get; set; } = new();
    }
}
