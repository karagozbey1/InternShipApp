using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc; 
using WebUI.Models.Company; 
using Business.Interfaces.Com; 
using Business.Interfaces.Internship; 
using Business.Interfaces.UserRepose; 
using Core.Entity; 

namespace WebUI.Controllers.Company
{
    // Þirket baþvuru yönetimi kontrolcüsü
    [Authorize(Roles = "Company")]
    [Route("Company/Applications")]
    public class CompanyApplicationsController : BaseCompanyController
    {
        // Kurucu metod
        public CompanyApplicationsController(
            IICompanyService companyService,
            IInternshipOfferService offerService,
            IInternshipApplicationService applicationService,
            IIUserService userService,
            Business.Interfaces.Order.IINotificationService notificationService,
            Business.Interfaces.Chat.IIChatService chatService)
            : base(companyService, offerService, applicationService, userService, notificationService, chatService) { }

        // Baþvuru listesi
        [HttpGet("")]
        public async Task<IActionResult> List(string searchTerm, ApplicationStatus? statusFilter, int? jobPostingIdFilter)
        {
            try
            {
                var companyId = await GetCurrentCompanyIdAsync();
                var offers = await _offerService.GetOffersByCompanyIdAsync(companyId);
                
                var allApplications = new List<InternshipApplication>();
                foreach (var offer in offers)
                {
                    var apps = await _applicationService.GetApplicationsByOfferIdAsync(offer.Id);
                    allApplications.AddRange(apps);
                }
                
                allApplications = allApplications.Where(a => a.Status != InternshipApplicationStatus.Rejected).ToList();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    allApplications = allApplications.Where(a =>
                        (a.Student?.FullName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.Student?.Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.InternshipOffer?.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
                }
                
                if (statusFilter.HasValue)
                {
                    var coreStatus = MapToCoreStatus(statusFilter.Value);
                    allApplications = allApplications.Where(a => a.Status == coreStatus).ToList();
                }
                
                if (jobPostingIdFilter.HasValue)
                {
                    allApplications = allApplications.Where(a => a.InternshipOfferId == jobPostingIdFilter.Value).ToList();
                }

                var model = new ApplicationListViewModel
                {
                    SearchTerm = searchTerm ?? string.Empty,
                    StatusFilter = statusFilter,
                    JobPostingIdFilter = jobPostingIdFilter,
                    Applications = allApplications.Select(MapToApplicationItemViewModel).OrderByDescending(a => a.ApplicationDate).ToList(),
                    AvailableJobPostings = offers.Select(o => new JobPostingSelectViewModel { Id = o.Id, Title = o.Title, Position = o.Field }).ToList()
                };
                
                return View("~/Views/Company/Applications/Index.cshtml", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Baþvurular yüklenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Company/Applications/Index.cshtml", new ApplicationListViewModel());
            }
        }

        // Baþvuru detayý
        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var application = await _applicationService.GetApplicationWithDetailsAsync(id);
                if (application == null)
                {
                    TempData["ErrorMessage"] = "Baþvuru bulunamadý.";
                    return RedirectToAction("List");
                }
                
                var companyId = await GetCurrentCompanyIdAsync();
                if (application.InternshipOffer?.CompanyId != companyId)
                {
                    TempData["ErrorMessage"] = "Yetkiniz yok.";
                    return RedirectToAction("List");
                }
                
                var model = MapToApplicationDetailViewModel(application);
                return View("~/Views/Company/Applications/Detail.cshtml", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Baþvuru detayý yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction("List");
            }
        }

        // Eski URL desteði
        [HttpGet("/CompanyApplications/ApplicationDetail/{id}")]
        public async Task<IActionResult> LegacyApplicationDetail(int id)
        {
            return await Detail(id);
        }

        // Baþvuru durumu güncelle
        [HttpPost("UpdateStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int id, ApplicationStatus status)
        {
            try
            {
                var application = await _applicationService.GetApplicationWithDetailsAsync(id);
                if (application == null)
                {
                    TempData["ErrorMessage"] = "Baþvuru bulunamadý.";
                    return RedirectToAction("List");
                }
                
                var companyId = await GetCurrentCompanyIdAsync();
                if (application.InternshipOffer?.CompanyId != companyId)
                {
                    TempData["ErrorMessage"] = "Yetkiniz yok.";
                    return RedirectToAction("List");
                }
                
                var coreStatus = MapToCoreStatus(status);
                var actingUserId = GetCurrentUserId();
                
                await _applicationService.UpdateApplicationStatusAsync(id, coreStatus.ToString(), actingUserId, "Company");
                
                TempData["SuccessMessage"] = "Baþvuru durumu güncellendi.";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Güncelleme hatasý: " + ex.Message;
                return RedirectToAction("List");
            }
        }

        // Baþvuruyu liste görünümü için hazýrlar
        private ApplicationItemViewModel MapToApplicationItemViewModel(InternshipApplication application)
        {
            return new ApplicationItemViewModel
            {
                Id = application.Id,
                StudentName = application.Student?.FullName ?? "Bilinmeyen",
                StudentEmail = application.Student?.Email ?? "Bilinmeyen",
                University = application.Student?.University?.Name ?? "Üniversite bilgisi eksik",
                Department = application.Student?.Department ?? "Bilinmeyen",
                JobTitle = application.InternshipOffer?.Title ?? "-",
                Status = MapToViewModelStatus(application.Status),
                ApplicationDate = application.CreatedAt,
                CoverLetter = application.StudentNote,
                CvPath = null
            };
        }

        // Baþvuruyu detay görünümü için hazýrlar
        private ApplicationDetailViewModel MapToApplicationDetailViewModel(InternshipApplication application)
        {
            string? cvPath = null;
            try
            {
                var cvDoc = application.InternshipDocuments?.FirstOrDefault(d => !string.IsNullOrEmpty(d.DocumentType) && d.DocumentType.Equals("CV", StringComparison.OrdinalIgnoreCase))
                    ?? application.InternshipDocuments?.FirstOrDefault(d => (d.Title ?? "").ToLower().Contains("cv") || (d.FileName ?? "").ToLower().Contains("cv"));
                if (cvDoc != null) cvPath = cvDoc.FilePath;
            }
            catch { }
            
            var documents = new List<DocumentItemViewModel>();
            try
            {
                if (application.InternshipDocuments != null)
                {
                    documents = application.InternshipDocuments.Select(d => new DocumentItemViewModel
                    {
                        Id = d.Id,
                        Title = d.Title ?? d.FileName,
                        FileName = d.FileName,
                        FilePath = d.FilePath,
                        FileSize = d.FileSize ?? 0,
                        DocumentType = d.DocumentType ?? "Other",
                        UploadedAt = d.UploadedAt
                    }).ToList();
                }
            }
            catch { }
            
            var universityName = application.Student?.University?.Name ?? "Bilinmeyen";
            var studentClass = application.Student?.Grade is int g and > 0 ? g + ". sýnýf" : "Bilinmeyen";
            
            return new ApplicationDetailViewModel
            {
                StudentId = application.StudentId,
                Id = application.Id,
                StudentName = application.Student?.FullName ?? "Bilinmeyen",
                StudentEmail = application.Student?.Email ?? "Bilinmeyen",
                StudentPhone = application.Student?.Phone ?? "Belirtilmemiþ",
                University = universityName,
                Department = application.Student?.Department ?? "Bilinmeyen",
                JobTitle = application.InternshipOffer?.Title ?? "-",
                JobDescription = application.InternshipOffer?.Description ?? "Açýklama yok",
                Status = MapToViewModelStatus(application.Status),
                ApplicationDate = application.CreatedAt,
                CoverLetter = application.StudentNote,
                CvPath = cvPath,
                Gpa = null,
                Skills = new List<string>(),
                Experiences = new List<ExperienceViewModel>(),
                Salary = application.InternshipOffer?.MonthlySalary,
                SalaryCurrency = application.InternshipOffer?.MonthlySalaryCurrency ?? CurrencyType.TRY,
                Documents = documents,
                StudentClass = studentClass
            };
        }

        // Durum dönüþtürücüler - baseclass'taki methodu gizler
        private new ApplicationStatus MapToViewModelStatus(InternshipApplicationStatus coreStatus) => coreStatus switch
        {
            InternshipApplicationStatus.Pending => ApplicationStatus.Pending,
            InternshipApplicationStatus.Approved => ApplicationStatus.Approved,
            InternshipApplicationStatus.Rejected => ApplicationStatus.Rejected,
            _ => ApplicationStatus.Pending
        };
        
        private InternshipApplicationStatus MapToCoreStatus(ApplicationStatus viewModelStatus) => viewModelStatus switch
        {
            ApplicationStatus.Pending => InternshipApplicationStatus.Pending,
            ApplicationStatus.Reviewing => InternshipApplicationStatus.Pending,
            ApplicationStatus.Approved => InternshipApplicationStatus.Approved,
            ApplicationStatus.Rejected => InternshipApplicationStatus.Rejected,
            ApplicationStatus.Cancelled => InternshipApplicationStatus.Rejected,
            _ => InternshipApplicationStatus.Pending
        };

        [HttpGet("/Student/CompanyApplications/Applications")]
        public async Task<IActionResult> LegacyApplicationsList()
        {
            return RedirectToAction("List");
        }

        [HttpGet("/CompanyApplications/Applications")]
        public async Task<IActionResult> LegacyApplicationsListAlt()
        {
            return RedirectToAction("List");
        }
    }
}
