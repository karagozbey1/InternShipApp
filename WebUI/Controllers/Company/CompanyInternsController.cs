using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc; 
using WebUI.Models.Company; 
using Business.Interfaces.Com; 
using Business.Interfaces.Internship; 
using Business.Interfaces.UserRepose; 
using Core.Entity; 

namespace WebUI.Controllers.Company
{
    [Authorize(Roles = "Company")]
    [Route("Company/Interns")] 
    public class CompanyInternsController : BaseCompanyController
    {
        private readonly IInternEvaluationService _evaluationService;

        public CompanyInternsController(
            IICompanyService companyService,
            IInternshipOfferService offerService,
            IInternshipApplicationService applicationService,
            IIUserService userService,
            Business.Interfaces.Order.IINotificationService notificationService,
            Business.Interfaces.Chat.IIChatService chatService,
            IInternEvaluationService evaluationService)
            : base(companyService, offerService, applicationService, userService, notificationService, chatService) 
        { 
            _evaluationService = evaluationService;
        }

        // Aktif stajyerlerin listesini getirip görüntüleyen action.
        [HttpGet("")]
        public async Task<IActionResult> List()
        {
            try
            {
                var companyId = await GetCurrentCompanyIdAsync();
                var offers = await _offerService.GetOffersByCompanyIdAsync(companyId);
                var allApps = new List<InternshipApplication>();
                foreach (var offer in offers)
                {
                    var apps = await _applicationService.GetApplicationsByOfferIdAsync(offer.Id);
                    allApps.AddRange(apps);
                }
                var activeInterns = allApps
                    .Where(a => a.Status == InternshipApplicationStatus.Approved && a.EndDate > DateTime.Now)
                    .Select(a => MapToActiveInternViewModel(a))
                    .ToList();
                return View("~/Views/Company/Interns/Index.cshtml", activeInterns);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Aktif stajyerler yüklenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Company/Interns/Index.cshtml", new List<ActiveInternViewModel>());
            }
        }

        // Belirtilen baþvuru id'sine ait stajyer detayýný yükleyen action.
        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var application = await _applicationService.GetApplicationWithDetailsAsync(id);
                if (application == null)
                {
                    TempData["ErrorMessage"] = "Stajyer bulunamadý.";
                    return RedirectToAction("List");
                }

                var companyId = await GetCurrentCompanyIdAsync();
                if (application.InternshipOffer?.CompanyId != companyId)
                {
                    TempData["ErrorMessage"] = "Yetkiniz yok.";
                    return RedirectToAction("List");
                }

                var model = MapToInternDetailViewModel(application);
                
                // Mevcut deðerlendirmeyi kontrol et
                var existingEvaluation = await _evaluationService.GetByApplicationIdAsync(id);
                model.HasEvaluation = existingEvaluation != null;
                model.Evaluation = existingEvaluation != null ? MapToEvaluationViewModel(existingEvaluation) : null;
                
                return View("~/Views/Company/Interns/Detail.cshtml", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Stajyer detayý yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction("List");
            }
        }

        // Deðerlendirme kaydetme
        [HttpPost("Evaluate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Evaluate([FromBody] EvaluationCreateModel model)
        {
            try
            {
                if (model == null)
                    return Json(new { success = false, message = "Geçersiz veri." });

                var companyId = await GetCurrentCompanyIdAsync();
                var userId = GetCurrentUserId();

                // Baþvuruyu kontrol et
                var application = await _applicationService.GetApplicationWithDetailsAsync(model.ApplicationId);
                if (application == null || application.InternshipOffer?.CompanyId != companyId)
                    return Json(new { success = false, message = "Baþvuru bulunamadý veya yetkiniz yok." });

                // Mevcut deðerlendirme var mý kontrol et
                var existingEvaluation = await _evaluationService.GetByApplicationIdAsync(model.ApplicationId);
                
                if (existingEvaluation != null)
                {
                    // Güncelle
                    existingEvaluation.TechnicalSkillsRating = model.TechnicalSkillsRating;
                    existingEvaluation.CommunicationRating = model.CommunicationRating;
                    existingEvaluation.TeamworkRating = model.TeamworkRating;
                    existingEvaluation.PunctualityRating = model.PunctualityRating;
                    existingEvaluation.InitiativeRating = model.InitiativeRating;
                    existingEvaluation.ProblemSolvingRating = model.ProblemSolvingRating;
                    existingEvaluation.OverallRating = model.OverallRating;
                    existingEvaluation.StrengthsComment = model.StrengthsComment;
                    existingEvaluation.ImprovementAreasComment = model.ImprovementAreasComment;
                    existingEvaluation.GeneralComment = model.GeneralComment;
                    existingEvaluation.RecommendationComment = model.RecommendationComment;
                    existingEvaluation.WouldRecommendForHire = model.WouldRecommendForHire;
                    existingEvaluation.WouldAcceptAgain = model.WouldAcceptAgain;
                    existingEvaluation.UpdatedAt = DateTime.UtcNow;

                    await _evaluationService.UpdateEvaluationAsync(existingEvaluation);
                    
                    return Json(new { success = true, message = "Deðerlendirme güncellendi.", isUpdate = true });
                }
                else
                {
                    // Yeni oluþtur
                    var evaluation = new InternEvaluation
                    {
                        ApplicationId = model.ApplicationId,
                        EvaluatorUserId = userId,
                        StudentId = application.StudentId,
                        CompanyId = companyId,
                        TechnicalSkillsRating = model.TechnicalSkillsRating,
                        CommunicationRating = model.CommunicationRating,
                        TeamworkRating = model.TeamworkRating,
                        PunctualityRating = model.PunctualityRating,
                        InitiativeRating = model.InitiativeRating,
                        ProblemSolvingRating = model.ProblemSolvingRating,
                        OverallRating = model.OverallRating,
                        StrengthsComment = model.StrengthsComment,
                        ImprovementAreasComment = model.ImprovementAreasComment,
                        GeneralComment = model.GeneralComment,
                        RecommendationComment = model.RecommendationComment,
                        WouldRecommendForHire = model.WouldRecommendForHire,
                        WouldAcceptAgain = model.WouldAcceptAgain,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _evaluationService.CreateEvaluationAsync(evaluation);

                    // Öðrenciye bildirim gönder
                    try
                    {
                        var company = await _companyService.GetCompanyByIdAsync(companyId);
                        await _notificationService.CreateNotificationAsync(new Notification
                        {
                            UserId = application.StudentId,
                            Title = "Staj Deðerlendirmesi Yapýldý",
                            Message = $"{company?.CompanyName ?? "Firma"} tarafýndan staj performansýnýz deðerlendirildi. Detaylarý görüntülemek için týklayýn.",
                            Type = "Evaluation",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow,
                            IconClass = "fas fa-star",
                            ActionUrl = $"/Student/Evaluation/View/{evaluation.Id}"
                        });
                    }
                    catch { }

                    return Json(new { success = true, message = "Deðerlendirme kaydedildi.", isUpdate = false });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // Deðerlendirme detayýný getir
        [HttpGet("GetEvaluation/{applicationId}")]
        public async Task<IActionResult> GetEvaluation(int applicationId)
        {
            try
            {
                var companyId = await GetCurrentCompanyIdAsync();
                var application = await _applicationService.GetApplicationWithDetailsAsync(applicationId);
                
                if (application == null || application.InternshipOffer?.CompanyId != companyId)
                    return Json(new { success = false, message = "Baþvuru bulunamadý veya yetkiniz yok." });

                var evaluation = await _evaluationService.GetByApplicationIdAsync(applicationId);
                
                if (evaluation == null)
                    return Json(new { success = true, exists = false });

                return Json(new { 
                    success = true, 
                    exists = true,
                    evaluation = new
                    {
                        evaluation.Id,
                        evaluation.TechnicalSkillsRating,
                        evaluation.CommunicationRating,
                        evaluation.TeamworkRating,
                        evaluation.PunctualityRating,
                        evaluation.InitiativeRating,
                        evaluation.ProblemSolvingRating,
                        evaluation.OverallRating,
                        evaluation.StrengthsComment,
                        evaluation.ImprovementAreasComment,
                        evaluation.GeneralComment,
                        evaluation.RecommendationComment,
                        evaluation.WouldRecommendForHire,
                        evaluation.WouldAcceptAgain,
                        evaluation.AverageRating,
                        CreatedAt = evaluation.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                        UpdatedAt = evaluation.UpdatedAt?.ToString("dd.MM.yyyy HH:mm")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Aktif stajyerleri CSV formatýnda dýþa aktaran action.
        [HttpGet("Export")]
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                var companyId = await GetCurrentCompanyIdAsync();
                var offers = await _offerService.GetOffersByCompanyIdAsync(companyId);
                var allApps = new List<InternshipApplication>();
                foreach (var offer in offers)
                {
                    var apps = await _applicationService.GetApplicationsByOfferIdAsync(offer.Id);
                    allApps.AddRange(apps);
                }
                var activeInterns = allApps
                    .Where(a => a.Status == InternshipApplicationStatus.Approved && a.EndDate > DateTime.Now)
                    .ToList();

                var csv = "Ad Soyad,Pozisyon,Baþlangýç Tarihi,Bitiþ Tarihi,Kalan Süre,Ýlerleme\n";
                foreach (var intern in activeInterns)
                {
                    var remainingTime = CalculateRemainingTime(intern.EndDate);
                    var progress = CalculateProgress(intern.StartDate, intern.EndDate);
                    csv += $"{intern.Student?.FullName},{intern.InternshipOffer?.Field},{intern.StartDate:dd/MM/yyyy},{intern.EndDate:dd/MM/yyyy},{remainingTime},{progress:F1}%\n";
                }

                var fileName = $"Aktif_Stajyerler_{DateTime.Now:yyyyMMdd}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Dýþa aktarým hatasý: " + ex.Message;
                return RedirectToAction("List");
            }
        }

        // Þirket için rapor verilerini toplayýp view model'e dönüþtürerek rapor sayfasýný döndüren action.
        [HttpGet("Reports")]
        public async Task<IActionResult> Reports()
        {
            try
            {
                var companyId = await GetCurrentCompanyIdAsync();
                var offers = await _offerService.GetOffersByCompanyIdAsync(companyId);
                var allApps = new List<InternshipApplication>();
                foreach (var offer in offers)
                {
                    var apps = await _applicationService.GetApplicationsByOfferIdAsync(offer.Id);
                    allApps.AddRange(apps);
                }

                var companyModel = new CompanyReportsViewModel
                {
                    TotalApplications = allApps.Count,
                    ApprovedApplications = allApps.Count(a => a.Status == InternshipApplicationStatus.Approved),
                    ActiveInterns = allApps.Count(a => a.Status == InternshipApplicationStatus.Approved && a.EndDate > DateTime.Now),
                    CompletedInterns = allApps.Count(a => a.Status == InternshipApplicationStatus.Approved && a.EndDate <= DateTime.Now),
                    ThisMonthApplications = allApps.Count(a => a.CreatedAt.Month == DateTime.Now.Month && a.CreatedAt.Year == DateTime.Now.Year),
                    ApprovalRate = allApps.Count > 0 ? (decimal)allApps.Count(a => a.Status == InternshipApplicationStatus.Approved) / allApps.Count * 100 : 0,
                    MonthlyData = GetMonthlyData(allApps),
                    DepartmentData = GetDepartmentData(allApps),
                    StatusData = GetStatusData(allApps)
                };

                var vm = new WebUI.Models.ViewModels.Company.CompanyReportViewModel
                {
                    TotalApplications = companyModel.TotalApplications,
                    ApprovedApplications = companyModel.ApprovedApplications,
                    PendingApplications = Math.Max(0, companyModel.TotalApplications - companyModel.ApprovedApplications - companyModel.CompletedInterns),
                    RejectedApplications = 0,
                    ThisMonthApplications = companyModel.ThisMonthApplications,
                    ActiveInterns = companyModel.ActiveInterns,
                    ApprovalRate = companyModel.ApprovalRate,
                    CompletedInterns = companyModel.CompletedInterns,
                    MonthlyData = new List<WebUI.Models.ViewModels.Company.MonthlyApplicationData>(),
                    DepartmentData = new List<WebUI.Models.ViewModels.Company.DepartmentApplicationData>(),
                    StatusData = new List<WebUI.Models.ViewModels.Company.StatusApplicationData>(),
                    RecentActivities = new List<WebUI.Models.ViewModels.Company.RecentActivityData>()
                };

                return View("~/Views/Company/Reports/Index.cshtml", vm);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Raporlar yüklenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Company/Reports/Index.cshtml", new WebUI.Models.ViewModels.Company.CompanyReportViewModel());
            }
        }

        // Belirtilen öðrenci ile sohbet baþlatmak veya sohbet sayfasýna yönlendirmek için kullanýlan action.
        [HttpGet("Chat/{studentId}")]
        public async Task<IActionResult> StartChat(int studentId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var student = await _userService.GetUserByIdAsync(studentId);
                if (student == null)
                {
                    TempData["ErrorMessage"] = "Öðrenci bulunamadý.";
                    return RedirectToAction("List");
                }

                return RedirectToAction("Index", "Chat", new { area = "", userId = studentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Sohbet baþlatýlýrken hata oluþtu: " + ex.Message;
                return RedirectToAction("List");
            }
        }

        #region Helper Methods
        private InternDetailViewModel MapToInternDetailViewModel(InternshipApplication application)
        {
            var remainingTime = CalculateRemainingTime(application.EndDate);
            var progress = CalculateProgress(application.StartDate, application.EndDate);
            
            return new InternDetailViewModel
            {
                Id = application.Id,
                StudentId = application.StudentId,
                StudentName = application.Student?.FullName ?? "Bilinmeyen",
                StudentEmail = application.Student?.Email ?? "Bilinmeyen",
                StudentPhone = application.Student?.Phone ?? "Belirtilmemiþ",
                StudentNumber = application.Student?.StudentNumber ?? "Belirtilmemiþ",
                University = application.Student?.University?.Name ?? "Bilinmeyen",
                Department = application.Student?.Department ?? "Bilinmeyen",
                Position = application.InternshipOffer?.Field ?? "Bilinmeyen Pozisyon",
                JobTitle = application.InternshipOffer?.Title ?? "Bilinmeyen",
                StartDate = application.StartDate,
                EndDate = application.EndDate,
                RemainingTime = remainingTime,
                Progress = progress,
                Status = "Aktif",
                StudentClass = application.Student?.Grade is int g and > 0 ? g + ". sýnýf" : "Bilinmeyen",
                CoverLetter = application.StudentNote,
                Documents = application.InternshipDocuments?.Select(d => new DocumentItemViewModel
                {
                    Id = d.Id,
                    Title = d.Title ?? d.FileName,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    FileSize = d.FileSize ?? 0,
                    DocumentType = d.DocumentType ?? "Other",
                    UploadedAt = d.UploadedAt
                }).ToList() ?? new List<DocumentItemViewModel>()
            };
        }

        private EvaluationViewModel MapToEvaluationViewModel(InternEvaluation evaluation)
        {
            return new EvaluationViewModel
            {
                Id = evaluation.Id,
                TechnicalSkillsRating = evaluation.TechnicalSkillsRating,
                CommunicationRating = evaluation.CommunicationRating,
                TeamworkRating = evaluation.TeamworkRating,
                PunctualityRating = evaluation.PunctualityRating,
                InitiativeRating = evaluation.InitiativeRating,
                ProblemSolvingRating = evaluation.ProblemSolvingRating,
                OverallRating = evaluation.OverallRating,
                StrengthsComment = evaluation.StrengthsComment,
                ImprovementAreasComment = evaluation.ImprovementAreasComment,
                GeneralComment = evaluation.GeneralComment,
                RecommendationComment = evaluation.RecommendationComment,
                WouldRecommendForHire = evaluation.WouldRecommendForHire,
                WouldAcceptAgain = evaluation.WouldAcceptAgain,
                AverageRating = evaluation.AverageRating,
                CreatedAt = evaluation.CreatedAt,
                UpdatedAt = evaluation.UpdatedAt
            };
        }

        private string CalculateRemainingTime(DateTime endDate)
        {
            var remaining = endDate.Date - DateTime.Now.Date;
            if (remaining.TotalDays <= 0)
                return "Tamamlandý";

            var totalDays = (int)remaining.TotalDays;
            var months = totalDays / 30;
            var days = totalDays % 30;

            if (months > 0)
                return $"{months} ay {days} gün";
            else
                return $"{days} gün";
        }

        private decimal CalculateProgress(DateTime startDate, DateTime endDate)
        {
            var totalDays = (endDate.Date - startDate.Date).TotalDays;
            var completedDays = (DateTime.Now.Date - startDate.Date).TotalDays;
            
            if (totalDays <= 0) return 100;
            if (completedDays <= 0) return 0;
            if (completedDays >= totalDays) return 100;
            
            return (decimal)(completedDays / totalDays * 100);
        }

        private List<MonthlyDataItem> GetMonthlyData(List<InternshipApplication> applications)
        {
            var monthlyData = new List<MonthlyDataItem>();
            for (int i = 5; i >= 0; i--)
            {
                var month = DateTime.Now.AddMonths(-i);
                var monthApps = applications.Where(a => a.CreatedAt.Month == month.Month && a.CreatedAt.Year == month.Year).ToList();
                monthlyData.Add(new MonthlyDataItem
                {
                    Month = month.ToString("MMM yyyy", new System.Globalization.CultureInfo("tr-TR")),
                    Applications = monthApps.Count,
                    Approvals = monthApps.Count(a => a.Status == InternshipApplicationStatus.Approved)
                });
            }
            return monthlyData;
        }

        private List<DepartmentDataItem> GetDepartmentData(List<InternshipApplication> applications)
        {
            return applications
                .Where(a => !string.IsNullOrEmpty(a.Student?.Department))
                .GroupBy(a => a.Student!.Department)
                .Select(g => new DepartmentDataItem
                {
                    Department = g.Key!,
                    Count = g.Count()
                })
                .OrderByDescending(d => d.Count)
                .Take(10)
                .ToList();
        }

        private List<StatusDataItem> GetStatusData(List<InternshipApplication> applications)
        {
            return new List<StatusDataItem>
            {
                new StatusDataItem { Status = "Onaylandý", Count = applications.Count(a => a.Status == InternshipApplicationStatus.Approved) },
                new StatusDataItem { Status = "Beklemede", Count = applications.Count(a => a.Status == InternshipApplicationStatus.Pending) },
                new StatusDataItem { Status = "Reddedildi", Count = applications.Count(a => a.Status == InternshipApplicationStatus.Rejected) }
            };
        }
        #endregion
    }

    // Deðerlendirme oluþturma modeli
    public class EvaluationCreateModel
    {
        public int ApplicationId { get; set; }
        public int TechnicalSkillsRating { get; set; }
        public int CommunicationRating { get; set; }
        public int TeamworkRating { get; set; }
        public int PunctualityRating { get; set; }
        public int InitiativeRating { get; set; }
        public int ProblemSolvingRating { get; set; }
        public int OverallRating { get; set; }
        public string? StrengthsComment { get; set; }
        public string? ImprovementAreasComment { get; set; }
        public string? GeneralComment { get; set; }
        public string? RecommendationComment { get; set; }
        public bool WouldRecommendForHire { get; set; }
        public bool WouldAcceptAgain { get; set; }
    }
}
