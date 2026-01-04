using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using System.Security.Claims;

namespace WebUI.Controllers.Student
{
    [Authorize(Roles = "Student")]
    public class EvaluationController : Controller
    {
        private readonly IInternEvaluationService _evaluationService;
        private readonly IInternshipApplicationService _applicationService;
        private readonly IIUserService _userService;

        public EvaluationController(
            IInternEvaluationService evaluationService,
            IInternshipApplicationService applicationService,
            IIUserService userService)
        {
            _evaluationService = evaluationService;
            _applicationService = applicationService;
            _userService = userService;
        }

        // Öðrencinin tüm deðerlendirmelerini listele
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                var evaluations = await _evaluationService.GetByStudentIdAsync(userId);

                var viewModels = evaluations.Select(e => new StudentEvaluationViewModel
                {
                    Id = e.Id,
                    CompanyName = e.Company?.CompanyName ?? "Bilinmeyen Þirket",
                    Position = e.Application?.InternshipOffer?.Field ?? "Bilinmeyen Pozisyon",
                    AverageRating = e.AverageRating,
                    TechnicalSkillsRating = e.TechnicalSkillsRating,
                    CommunicationRating = e.CommunicationRating,
                    TeamworkRating = e.TeamworkRating,
                    PunctualityRating = e.PunctualityRating,
                    InitiativeRating = e.InitiativeRating,
                    ProblemSolvingRating = e.ProblemSolvingRating,
                    OverallRating = e.OverallRating,
                    StrengthsComment = e.StrengthsComment,
                    ImprovementAreasComment = e.ImprovementAreasComment,
                    GeneralComment = e.GeneralComment,
                    RecommendationComment = e.RecommendationComment,
                    WouldRecommendForHire = e.WouldRecommendForHire,
                    WouldAcceptAgain = e.WouldAcceptAgain,
                    IsViewed = e.IsViewedByStudent,
                    CreatedAt = e.CreatedAt,
                    StartDate = e.Application?.StartDate ?? DateTime.MinValue,
                    EndDate = e.Application?.EndDate ?? DateTime.MinValue
                }).ToList();

                return View("~/Views/Student/Evaluation/Index.cshtml", viewModels);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Deðerlendirmeler yüklenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Student/Evaluation/Index.cshtml", new List<StudentEvaluationViewModel>());
            }
        }

        // Tek deðerlendirme detayý
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var evaluation = await _evaluationService.GetWithDetailsAsync(id);

                if (evaluation == null || evaluation.StudentId != userId)
                {
                    TempData["ErrorMessage"] = "Deðerlendirme bulunamadý.";
                    return RedirectToAction("Index");
                }

                // Görüldü olarak iþaretle
                if (!evaluation.IsViewedByStudent)
                {
                    await _evaluationService.MarkAsViewedByStudentAsync(id);
                }

                var viewModel = new StudentEvaluationDetailViewModel
                {
                    Id = evaluation.Id,
                    CompanyName = evaluation.Company?.CompanyName ?? "Bilinmeyen Þirket",
                    Position = evaluation.Application?.InternshipOffer?.Field ?? "Bilinmeyen Pozisyon",
                    JobTitle = evaluation.Application?.InternshipOffer?.Title ?? "Bilinmeyen",
                    AverageRating = evaluation.AverageRating,
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
                    CreatedAt = evaluation.CreatedAt,
                    StartDate = evaluation.Application?.StartDate ?? DateTime.MinValue,
                    EndDate = evaluation.Application?.EndDate ?? DateTime.MinValue
                };

                return View("~/Views/Student/Evaluation/View.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Deðerlendirme yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // Okunmamýþ deðerlendirme sayýsý
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var evaluations = await _evaluationService.GetByStudentIdAsync(userId);
                var unreadCount = evaluations.Count(e => !e.IsViewedByStudent);
                return Json(new { success = true, count = unreadCount });
            }
            catch
            {
                return Json(new { success = false, count = 0 });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }

    // View Modelleri
    public class StudentEvaluationViewModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public string Position { get; set; } = "";
        public decimal AverageRating { get; set; }
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
        public bool IsViewed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class StudentEvaluationDetailViewModel : StudentEvaluationViewModel
    {
        public string JobTitle { get; set; } = "";
    }
}
