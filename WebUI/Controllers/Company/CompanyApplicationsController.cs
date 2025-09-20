using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc; 
using WebUI.Models.Company; 
using Business.Interfaces.Com; 
using Business.Interfaces.Internship; 
using Business.Interfaces.UserRepose; 
using Core.Entity; 

namespace WebUI.Controllers.Company
{
    /// <summary>
    /// Þirket Baþvuru Yönetimi Controller'ý
    /// Þirketlerin aldýðý staj baþvurularýný görüntüleme, deðerlendirme ve durum güncelleme iþlemleri
    /// BaseCompanyController'dan türetilmiþ, þirket yetkilerini kontrol eder
    /// </summary>
    [Authorize(Roles = "Company")]
    [Route("Company/Applications")] // Temel rota: /Company/Applications
    public class CompanyApplicationsController : BaseCompanyController
    {
        /// <summary>
        /// CompanyApplicationsController constructor
        /// BaseCompanyController'ýn tüm servislerini alýr
        /// Baþvuru yönetimi için gerekli servisleri dependency injection ile saðlar
        /// </summary>
        /// <param name="companyService">Þirket iþlemleri servisi</param>
        /// <param name="offerService">Staj ilanlarý servisi</param>
        /// <param name="applicationService">Baþvuru iþlemleri servisi</param>
        /// <param name="userService">Kullanýcý iþlemleri servisi</param>
        /// <param name="notificationService">Bildirim servisi</param>
        /// <param name="chatService">Mesajlaþma servisi</param>
        public CompanyApplicationsController(
            IICompanyService companyService,
            IInternshipOfferService offerService,
            IInternshipApplicationService applicationService,
            IIUserService userService,
            Business.Interfaces.Order.IINotificationService notificationService,
            Business.Interfaces.Chat.IIChatService chatService)
            : base(companyService, offerService, applicationService, userService, notificationService, chatService) { }

        /// <summary>
        /// Þirketin aldýðý baþvurularý listeler
        /// Arama, filtreleme ve sýralama iþlevleri ile birlikte
        /// </summary>
        /// <param name="searchTerm">Öðrenci adý, email veya pozisyon aramasý</param>
        /// <param name="statusFilter">Baþvuru durumu filtresi (Pending, Approved, Rejected)</param>
        /// <param name="jobPostingIdFilter">Belirli ilan bazýnda filtreleme</param>
        /// <returns>Baþvuru listesi sayfasý</returns>
        [HttpGet("")]
        public async Task<IActionResult> List(string searchTerm, ApplicationStatus? statusFilter, int? jobPostingIdFilter)
        {
            try
            {
                // Mevcut þirketin ID'sini al
                var companyId = await GetCurrentCompanyIdAsync();
                
                // Þirketin tüm ilanlarýný getir
                var offers = await _offerService.GetOffersByCompanyIdAsync(companyId);
                
                // Her ilan için baþvurularý topla
                var allApplications = new List<InternshipApplication>();
                foreach (var offer in offers)
                {
                    var apps = await _applicationService.GetApplicationsByOfferIdAsync(offer.Id);
                    allApplications.AddRange(apps);
                }
                
                // Reddedilenleri hariç tut (eski mantýk)
                // Not: Bu iþ kuralý deðiþebilir, reddedilenleri de göstermek istenebilir
                allApplications = allApplications.Where(a => a.Status != InternshipApplicationStatus.Rejected).ToList();

                // Arama filtresi - öðrenci adý, email veya pozisyon baþlýðýnda arama
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    allApplications = allApplications.Where(a =>
                        (a.Student?.FullName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.Student?.Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.InternshipOffer?.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
                }
                
                // Durum filtresi - baþvuru durumuna göre filtreleme
                if (statusFilter.HasValue)
                {
                    var coreStatus = MapToCoreStatus(statusFilter.Value);
                    allApplications = allApplications.Where(a => a.Status == coreStatus).ToList();
                }
                
                // Ýlan bazýnda filtreleme - belirli bir pozisyonun baþvurularýný görme
                if (jobPostingIdFilter.HasValue)
                {
                    allApplications = allApplications.Where(a => a.InternshipOfferId == jobPostingIdFilter.Value).ToList();
                }

                // ViewModel oluþtur ve verileri dönüþtür
                var model = new ApplicationListViewModel
                {
                    SearchTerm = searchTerm ?? string.Empty,
                    StatusFilter = statusFilter,
                    JobPostingIdFilter = jobPostingIdFilter,
                    // Baþvurularý tarihe göre sýrala (en yeni üstte)
                    Applications = allApplications.Select(MapToApplicationItemViewModel).OrderByDescending(a => a.ApplicationDate).ToList(),
                    // Dropdown için mevcut ilanlarý hazýrla
                    AvailableJobPostings = offers.Select(o => new JobPostingSelectViewModel { Id = o.Id, Title = o.Title, Position = o.Field }).ToList()
                };
                
                return View("~/Views/Company/Applications/Index.cshtml", model);
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanýcýya bilgi ver ve boþ model döndür
                TempData["ErrorMessage"] = "Baþvurular yüklenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Company/Applications/Index.cshtml", new ApplicationListViewModel());
            }
        }

        /// <summary>
        /// Tek bir baþvurunun detaylarýný gösterir
        /// Öðrenci bilgileri, CV, belgeler ve deðerlendirme seçenekleri
        /// </summary>
        /// <param name="id">Görüntülenecek baþvuru ID'si</param>
        /// <returns>Baþvuru detay sayfasý</returns>
        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                // Baþvuru bilgilerini iliþkili verilerle birlikte getir
                var application = await _applicationService.GetApplicationWithDetailsAsync(id);
                if (application == null)
                {
                    TempData["ErrorMessage"] = "Baþvuru bulunamadý.";
                    return RedirectToAction("List");
                }
                
                // Güvenlik kontrolü - baþvurunun bu þirkete ait olduðunu doðrula
                var companyId = await GetCurrentCompanyIdAsync();
                if (application.InternshipOffer?.CompanyId != companyId)
                {
                    TempData["ErrorMessage"] = "Yetkiniz yok.";
                    return RedirectToAction("List");
                }
                
                // Entity'yi DetailViewModel'e dönüþtür
                var model = MapToApplicationDetailViewModel(application);
                return View("~/Views/Company/Applications/Detail.cshtml", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Baþvuru detayý yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction("List");
            }
        }

        /// <summary>
        /// Eski URL yapýsýný destekleme - geriye dönük uyumluluk
        /// Eski linkler: /CompanyApplications/ApplicationDetail/{id}
        /// </summary>
        /// <param name="id">Baþvuru ID'si</param>
        /// <returns>Yeni Detail action'ýna yönlendirir</returns>
        [HttpGet("/CompanyApplications/ApplicationDetail/{id}")]
        public async Task<IActionResult> LegacyApplicationDetail(int id)
        {
            // Eski linkleri yeni yapýya yönlendir
            return await Detail(id);
        }

        /// <summary>
        /// Baþvuru durumunu günceller (Onayla/Reddet)
        /// Þirket temsilcilerinin baþvurularý deðerlendirmesi için
        /// </summary>
        /// <param name="id">Güncellenecek baþvuru ID'si</param>
        /// <param name="status">Yeni durum (Approved/Rejected)</param>
        /// <returns>Liste sayfasýna geri döner</returns>
        [HttpPost("UpdateStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int id, ApplicationStatus status)
        {
            try
            {
                // Baþvuru varlýðýný ve yetki kontrolü
                var application = await _applicationService.GetApplicationWithDetailsAsync(id);
                if (application == null)
                {
                    TempData["ErrorMessage"] = "Baþvuru bulunamadý.";
                    return RedirectToAction("List");
                }
                
                // Güvenlik kontrolü - baþvurunun bu þirkete ait olduðunu doðrula
                var companyId = await GetCurrentCompanyIdAsync();
                if (application.InternshipOffer?.CompanyId != companyId)
                {
                    TempData["ErrorMessage"] = "Yetkiniz yok.";
                    return RedirectToAction("List");
                }
                
                // ViewModel status'unu Core Entity status'una dönüþtür
                var coreStatus = MapToCoreStatus(status);
                var actingUserId = GetCurrentUserId();
                
                // Durum güncelleme iþlemini gerçekleþtir
                // Bu iþlem kontenjan kontrolü, bildirim gönderme vs. yapar
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

        #region Mapping Helper Metodlar
        
        /// <summary>
        /// InternshipApplication entity'sini ApplicationItemViewModel'e dönüþtürür
        /// Liste sayfasýnda gösterim için özet bilgileri hazýrlar
        /// </summary>
        /// <param name="application">Dönüþtürülecek baþvuru entity'si</param>
        /// <returns>Liste için ViewModel</returns>
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
                CvPath = null // CV yolu detay sayfasýnda hesaplanýr
            };
        }

        /// <summary>
        /// InternshipApplication entity'sini ApplicationDetailViewModel'e dönüþtürür
        /// Detay sayfasýnda tam bilgileri göstermek için
        /// </summary>
        /// <param name="application">Dönüþtürülecek baþvuru entity'si</param>
        /// <returns>Detay için ViewModel</returns>
        private ApplicationDetailViewModel MapToApplicationDetailViewModel(InternshipApplication application)
        {
            // CV dosyasýný belgeler arasýndan bul
            string? cvPath = null;
            try
            {
                var cvDoc = application.InternshipDocuments?.FirstOrDefault(d => !string.IsNullOrEmpty(d.DocumentType) && d.DocumentType.Equals("CV", StringComparison.OrdinalIgnoreCase))
                    ?? application.InternshipDocuments?.FirstOrDefault(d => (d.Title ?? "").ToLower().Contains("cv") || (d.FileName ?? "").ToLower().Contains("cv"));
                if (cvDoc != null) cvPath = cvDoc.FilePath;
            }
            catch { }
            
            // Tüm belgeleri ViewModel'e dönüþtür
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
            
            // Öðrenci bilgilerini hazýrla
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
                Gpa = null, // GPA bilgisi þu anda entity'de yok
                Skills = new List<string>(), // Beceriler bilgisi þu anda entity'de yok
                Experiences = new List<ExperienceViewModel>(), // Deneyimler bilgisi þu anda entity'de yok
                Salary = application.InternshipOffer?.MonthlySalary,
                SalaryCurrency = application.InternshipOffer?.MonthlySalaryCurrency ?? CurrencyType.TRY,
                Documents = documents,
                StudentClass = studentClass
            };
        }

        /// <summary>
        /// Core Entity status'unu ViewModel status'una dönüþtürür
        /// Presentation layer ile business layer arasýnda mapping
        /// </summary>
        /// <param name="coreStatus">Core katmanýndaki InternshipApplicationStatus</param>
        /// <returns>ViewModel'deki ApplicationStatus</returns>
        private ApplicationStatus MapToViewModelStatus(InternshipApplicationStatus coreStatus) => coreStatus switch
        {
            InternshipApplicationStatus.Pending => ApplicationStatus.Pending,
            InternshipApplicationStatus.Approved => ApplicationStatus.Approved,
            InternshipApplicationStatus.Rejected => ApplicationStatus.Rejected,
            _ => ApplicationStatus.Pending
        };
        
        /// <summary>
        /// ViewModel status'unu Core Entity status'una dönüþtürür
        /// Presentation layer'dan business layer'a mapping
        /// </summary>
        /// <param name="viewModelStatus">ViewModel'deki ApplicationStatus</param>
        /// <returns>Core katmanýndaki InternshipApplicationStatus</returns>
        private InternshipApplicationStatus MapToCoreStatus(ApplicationStatus viewModelStatus) => viewModelStatus switch
        {
            ApplicationStatus.Pending => InternshipApplicationStatus.Pending,
            ApplicationStatus.Reviewing => InternshipApplicationStatus.Pending, // Reviewing -> Pending'e map
            ApplicationStatus.Approved => InternshipApplicationStatus.Approved,
            ApplicationStatus.Rejected => InternshipApplicationStatus.Rejected,
            ApplicationStatus.Cancelled => InternshipApplicationStatus.Rejected, // Cancelled -> Rejected'a map
            _ => InternshipApplicationStatus.Pending
        };
        
        #endregion

        #region Legacy URL Desteði - Geriye Dönük Uyumluluk
        
        /// <summary>
        /// Eski URL desteði: /Student/CompanyApplications/Applications
        /// Yanlýþ student linki doðru company linkine yönlendirir
        /// </summary>
        [HttpGet("/Student/CompanyApplications/Applications")]
        public async Task<IActionResult> LegacyApplicationsList()
        {
            // Yanlýþ student linki doðru company linkine yönlendir
            return RedirectToAction("List");
        }

        /// <summary>
        /// Eski URL desteði: /CompanyApplications/Applications  
        /// Eski link yapýsýný yeni yapýya yönlendirir
        /// </summary>
        [HttpGet("/CompanyApplications/Applications")]
        public async Task<IActionResult> LegacyApplicationsListAlt()
        {
            // Yanlýþ eski linki doðru yeni linkine yönlendir
            return RedirectToAction("List");
        }
        
        #endregion
    }
}
