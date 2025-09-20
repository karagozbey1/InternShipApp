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
    [Route("Company/Interns")] // /Company/Interns
    public class CompanyInternsController : BaseCompanyController
    {
        public CompanyInternsController(
            IICompanyService companyService,
            IInternshipOfferService offerService,
            IInternshipApplicationService applicationService,
            IIUserService userService,
            Business.Interfaces.Order.IINotificationService notificationService,
            Business.Interfaces.Chat.IIChatService chatService)
            : base(companyService, offerService, applicationService, userService, notificationService, chatService) { }

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

        // Intern detail page
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
                return View("~/Views/Company/Interns/Detail.cshtml", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Stajyer detayý yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction("List");
            }
        }

        // Export to Excel
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

                // Create Excel content (simplified - would use EPPlus or similar in real implementation)
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

        // Reports page
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

                var model = new CompanyReportsViewModel
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

                return View("~/Views/Company/Reports/Index.cshtml", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Raporlar yüklenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Company/Reports/Index.cshtml", new CompanyReportsViewModel());
            }
        }

        // Chat redirection - Company users will be redirected to Student Chat area
        [HttpGet("Chat/{studentId}")]
        public async Task<IActionResult> StartChat(int studentId)
        {
            try
            {
                // Get current company user ID
                var currentUserId = GetCurrentUserId();
                
                // Verify student exists
                var student = await _userService.GetUserByIdAsync(studentId);
                if (student == null)
                {
                    TempData["ErrorMessage"] = "Öðrenci bulunamadý.";
                    return RedirectToAction("List");
                }

                // Redirect to the Student Chat controller with userId parameter
                // This will allow the company user to chat with the specific student
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
                .GroupBy(a => a.Student.Department)
                .Select(g => new DepartmentDataItem
                {
                    Department = g.Key,
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
}
