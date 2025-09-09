using Business.Interfaces.Internship;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.Entity;
using WebUI.Controllers;

namespace WebUI.Controllers.Student
{
    [Authorize]
    public class StatisticsController : BaseAppController
    {
        private readonly IInternshipApplicationService _applicationService;
        private readonly IInternshipPeriodService _periodService;

        public StatisticsController(IInternshipApplicationService applicationService, IInternshipPeriodService periodService)
        {
            _applicationService = applicationService;
            _periodService = periodService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var apps = await _applicationService.GetApplicationsByStudentIdAsync(userId);

            int total = apps.Count;
            int approved = apps.Count(a => a.Status == InternshipApplicationStatus.Approved);
            int pending = apps.Count(a => a.Status == InternshipApplicationStatus.Pending);
            int rejected = apps.Count(a => a.Status == InternshipApplicationStatus.Rejected);

            // Success rate: approved / (approved + rejected)
            int denom = Math.Max(approved + rejected, 1);
            int successRate = (int)Math.Round(approved * 100.0 / denom);

            ViewBag.TotalApplications = total;
            ViewBag.Approved = approved;
            ViewBag.Pending = pending;
            ViewBag.Rejected = rejected;
            ViewBag.SuccessRate = successRate;

            return View("~/Views/Student/Statistics.cshtml");
        }
    }
}
