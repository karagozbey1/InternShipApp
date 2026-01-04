using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using System.Security.Claims;

namespace WebUI.Controllers.Advisor
{
    [Authorize(Roles = "Advisor")]
    [Route("Advisor/Evaluation")]
    public class AdvisorEvaluationController : Controller
    {
        private readonly IInternEvaluationService _evaluationService;
        private readonly IInternshipApplicationService _applicationService;
        private readonly IInternshipApprovalService _approvalService;
        private readonly IIUserService _userService;

        public AdvisorEvaluationController(
            IInternEvaluationService evaluationService,
            IInternshipApplicationService applicationService,
            IInternshipApprovalService approvalService,
            IIUserService userService)
        {
            _evaluationService = evaluationService;
            _applicationService = applicationService;
            _approvalService = approvalService;
            _userService = userService;
        }

        // Danýþmanýn öðrencilerinin deðerlendirmelerini listele
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);
                
                if (advisor == null)
                {
                    TempData["ErrorMessage"] = "Danýþman bilgileri bulunamadý.";
                    return RedirectToAction("Index", "AdvisorDashboard");
                }

                // Danýþmanýn onayladýðý baþvurularý bul (ApprovalForm'da AdvisorId ile eþleþenler)
                var approvalForms = await _approvalService.GetApprovalFormsByAdvisorIdAsync(advisorId);
                
                // Bu öðrencilerin deðerlendirmelerini getir
                var evaluations = new List<AdvisorEvaluationListViewModel>();
                
                foreach (var form in approvalForms.Where(f => f.Status == Core.Entity.InternshipApprovalStatus.Approved))
                {
                    var evaluation = await _evaluationService.GetByApplicationIdAsync(form.ApplicationId);
                    if (evaluation != null)
                    {
                        var student = await _userService.GetUserByIdAsync(evaluation.StudentId);
                        evaluations.Add(new AdvisorEvaluationListViewModel
                        {
                            Id = evaluation.Id,
                            ApplicationId = evaluation.ApplicationId,
                            StudentId = evaluation.StudentId,
                            StudentName = student?.FullName ?? "Bilinmeyen",
                            StudentNumber = student?.StudentNumber ?? "Belirtilmemiþ",
                            CompanyName = evaluation.Company?.CompanyName ?? "Bilinmeyen",
                            Position = evaluation.Application?.InternshipOffer?.Field ?? "Bilinmeyen",
                            AverageRating = evaluation.AverageRating,
                            OverallRating = evaluation.OverallRating,
                            IsViewedByStudent = evaluation.IsViewedByStudent,
                            CreatedAt = evaluation.CreatedAt,
                            StartDate = evaluation.Application?.StartDate ?? DateTime.MinValue,
                            EndDate = evaluation.Application?.EndDate ?? DateTime.MinValue
                        });
                    }
                }

                return View("~/Views/Advisor/Evaluation/Index.cshtml", evaluations.OrderByDescending(e => e.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Deðerlendirmeler yüklenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Advisor/Evaluation/Index.cshtml", new List<AdvisorEvaluationListViewModel>());
            }
        }

        // Deðerlendirme detayý (tüm kriterler)
        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var evaluation = await _evaluationService.GetWithDetailsAsync(id);

                if (evaluation == null)
                {
                    TempData["ErrorMessage"] = "Deðerlendirme bulunamadý.";
                    return RedirectToAction("Index");
                }

                // Bu deðerlendirmenin danýþmanýn öðrencisine ait olduðunu doðrula
                var approvalForm = await _approvalService.GetApprovalFormByApplicationIdAsync(evaluation.ApplicationId);
                if (approvalForm == null || approvalForm.AdvisorId != advisorId)
                {
                    TempData["ErrorMessage"] = "Bu deðerlendirmeye eriþim yetkiniz yok.";
                    return RedirectToAction("Index");
                }

                var student = await _userService.GetUserWithUniversityAsync(evaluation.StudentId);

                var viewModel = new AdvisorEvaluationDetailViewModel
                {
                    Id = evaluation.Id,
                    StudentId = evaluation.StudentId,
                    StudentName = student?.FullName ?? "Bilinmeyen",
                    StudentEmail = student?.Email ?? "",
                    StudentNumber = student?.StudentNumber ?? "Belirtilmemiþ",
                    University = student?.University?.Name ?? "Bilinmeyen",
                    Department = student?.Department ?? "Bilinmeyen",
                    CompanyName = evaluation.Company?.CompanyName ?? "Bilinmeyen",
                    Position = evaluation.Application?.InternshipOffer?.Field ?? "Bilinmeyen",
                    JobTitle = evaluation.Application?.InternshipOffer?.Title ?? "Bilinmeyen",
                    
                    // Tüm puanlar
                    TechnicalSkillsRating = evaluation.TechnicalSkillsRating,
                    CommunicationRating = evaluation.CommunicationRating,
                    TeamworkRating = evaluation.TeamworkRating,
                    PunctualityRating = evaluation.PunctualityRating,
                    InitiativeRating = evaluation.InitiativeRating,
                    ProblemSolvingRating = evaluation.ProblemSolvingRating,
                    OverallRating = evaluation.OverallRating,
                    AverageRating = evaluation.AverageRating,
                    
                    // Tüm yorumlar
                    StrengthsComment = evaluation.StrengthsComment,
                    ImprovementAreasComment = evaluation.ImprovementAreasComment,
                    GeneralComment = evaluation.GeneralComment,
                    RecommendationComment = evaluation.RecommendationComment,
                    
                    // Öneriler
                    WouldRecommendForHire = evaluation.WouldRecommendForHire,
                    WouldAcceptAgain = evaluation.WouldAcceptAgain,
                    
                    // Tarihler
                    CreatedAt = evaluation.CreatedAt,
                    UpdatedAt = evaluation.UpdatedAt,
                    StartDate = evaluation.Application?.StartDate ?? DateTime.MinValue,
                    EndDate = evaluation.Application?.EndDate ?? DateTime.MinValue,
                    
                    // Öðrenci görme durumu
                    IsViewedByStudent = evaluation.IsViewedByStudent,
                    ViewedAt = evaluation.ViewedAt
                };

                return View("~/Views/Advisor/Evaluation/Detail.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Deðerlendirme detayý yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }

    // View Modelleri
    public class AdvisorEvaluationListViewModel
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentNumber { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string Position { get; set; } = "";
        public decimal AverageRating { get; set; }
        public int OverallRating { get; set; }
        public bool IsViewedByStudent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class AdvisorEvaluationDetailViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public string StudentNumber { get; set; } = "";
        public string University { get; set; } = "";
        public string Department { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string Position { get; set; } = "";
        public string JobTitle { get; set; } = "";
        
        // Puanlar
        public int TechnicalSkillsRating { get; set; }
        public int CommunicationRating { get; set; }
        public int TeamworkRating { get; set; }
        public int PunctualityRating { get; set; }
        public int InitiativeRating { get; set; }
        public int ProblemSolvingRating { get; set; }
        public int OverallRating { get; set; }
        public decimal AverageRating { get; set; }
        
        // Yorumlar
        public string? StrengthsComment { get; set; }
        public string? ImprovementAreasComment { get; set; }
        public string? GeneralComment { get; set; }
        public string? RecommendationComment { get; set; }
        
        // Öneriler
        public bool WouldRecommendForHire { get; set; }
        public bool WouldAcceptAgain { get; set; }
        
        // Tarihler
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Öðrenci görme durumu
        public bool IsViewedByStudent { get; set; }
        public DateTime? ViewedAt { get; set; }
    }
}
