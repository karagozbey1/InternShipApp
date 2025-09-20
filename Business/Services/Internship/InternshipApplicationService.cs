using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;
using CoreApplicationStatus = Core.Entity.InternshipApplicationStatus;
using Business.Interfaces.Order;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Business.Utils;

namespace Business.Services.Internship
{
    /// <summary>
    /// Staj başvuru işlemleri servis sınıfı
    /// Öğrencilerin staj başvurularını yönetir, durum güncellemeleri yapar
    /// Şirketlerin başvuruları değerlendirme süreçlerini koordine eder
    /// </summary>
    public class InternshipApplicationService : IInternshipApplicationService
    {
        /// <summary>
        /// Veritabanı işlemleri için Unit of Work pattern'i
        /// Tüm repository'lere merkezi erişim sağlar
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        
        /// <summary>
        /// Bildirim servisi - başvuru durumu değişikliklerinde kullanıcıları bilgilendirir
        /// E-posta ve sistem bildirimleri gönderir
        /// </summary>
        private readonly IINotificationService _notificationService;

        /// <summary>
        /// InternshipApplicationService constructor
        /// Bağımlılıkları dependency injection ile alır
        /// </summary>
        /// <param name="unitOfWork">Veritabanı işlemleri için Unit of Work</param>
        /// <param name="notificationService">Bildirim gönderim servisi</param>
        public InternshipApplicationService(IUnitOfWork unitOfWork, IINotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Belirli bir öğrencinin tüm başvurularını getirir
        /// Öğrenci dashboard'ında başvuru geçmişini göstermek için kullanılır
        /// </summary>
        /// <param name="studentId">Öğrenci ID'si</param>
        /// <returns>Öğrencinin başvuru listesi</returns>
        /// <exception cref="ArgumentException">Geçersiz öğrenci ID durumunda</exception>
        public async Task<List<InternshipApplication>> GetApplicationsByStudentIdAsync(int studentId)
        {
            if (studentId <= 0)
                {
                throw new ArgumentException("Geçersiz Öğrenci ID", nameof(studentId));
            }

            return await _unitOfWork.InternshipApplications.GetApplicationsByStudentIdAsync(studentId);
        }

        /// <summary>
        /// Belirli bir staj teklifine yapılan tüm başvuruları getirir
        /// Şirket temsilcilerinin başvuru listesini görmesi için kullanılır
        /// </summary>
        /// <param name="offerId">Staj teklifi ID'si</param>
        /// <returns>İlana yapılan başvuru listesi</returns>
        /// <exception cref="ArgumentException">Geçersiz teklif ID durumunda</exception>
        public async Task<List<InternshipApplication>> GetApplicationsByOfferIdAsync(int offerId)
        {
            if (offerId <= 0)
                throw new ArgumentException("Geçersiz Staj Kimliği ID", nameof(offerId));

            return await _unitOfWork.InternshipApplications.GetApplicationsByOfferIdAsync(offerId);
        }

        /// <summary>
        /// Başvuru detaylarını ilişkili verilerle birlikte getirir
        /// Şirket, öğrenci, teklif bilgilerini include eder
        /// </summary>
        /// <param name="applicationId">Başvuru ID'si</param>
        /// <returns>Detaylı başvuru bilgisi</returns>
        /// <exception cref="ArgumentException">Geçersiz başvuru ID durumunda</exception>
        public async Task<InternshipApplication?> GetApplicationWithDetailsAsync(int applicationId)
        {
            if (applicationId <= 0)
                throw new ArgumentException("Geçersiz Başvuru ID", nameof(applicationId));

            return await _unitOfWork.InternshipApplications.GetApplicationWithDetailsAsync(applicationId);
        }

        /// <summary>
        /// Belirli durumdaki tüm başvuruları getirir
        /// Admin panelinde filtreleme için kullanılır
        /// </summary>
        /// <param name="status">Başvuru durumu (Pending, Approved, Rejected)</param>
        /// <returns>Belirtilen durumdaki başvuru listesi</returns>
        /// <exception cref="ArgumentException">Geçersiz durum durumunda</exception>
        public async Task<List<InternshipApplication>> GetApplicationsByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status) || !Enum.TryParse<CoreApplicationStatus>(status, out _))
                throw new ArgumentException("Durum Geçersiz", nameof(status));

            return await _unitOfWork.InternshipApplications.GetApplicationsByStatusAsync(status);
        }

        /// <summary>
        /// Yeni bir staj başvurusu oluşturur
        /// İş mantığı kontrollerini yapar ve bildirim gönderir
        /// </summary>
        /// <param name="application">Oluşturulacak başvuru bilgileri</param>
        /// <returns>Oluşturulan başvuru</returns>
        /// <exception cref="ArgumentNullException">Başvuru null ise</exception>
        /// <exception cref="ArgumentException">Geçersiz öğrenci veya teklif ID'si</exception>
        /// <exception cref="InvalidOperationException">İş kuralı ihlali durumunda</exception>
        public async Task<InternshipApplication> AddApplicationAsync(InternshipApplication application)
        {
            // İş mantığı: Başvuru oluşturmadan önce öğrenciyi ve staj teklifini doğrula
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            if (application.StudentId <= 0)
                throw new ArgumentException("Geçersiz öğrenci kimliği", nameof(application.StudentId));

            if (application.InternshipOfferId <= 0)
                throw new ArgumentException("Geçersiz staj teklifi kimliği", nameof(application.InternshipOfferId));

            // Öğrencinin bu teklif için zaten aktif bir başvuru olup olmadığını kontrol et
            var existingApplication = await _unitOfWork.InternshipApplications
                .GetApplicationsByStudentIdAsync(application.StudentId);
            
            if (existingApplication.Any(app => app.InternshipOfferId == application.InternshipOfferId &&
                                             app.Status != CoreApplicationStatus.Rejected))
            {
                throw new InvalidOperationException("Öğrencinin bu teklif için zaten aktif bir başvuru var");
            }

            // Staj teklifinin var olup olmadığını ve geçerli olup olmadığını kontrol et
            var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(application.InternshipOfferId);
            if (offer == null)
                throw new InvalidOperationException("Staj teklifi mevcut değil");
            
            if (offer.ApplicationDeadline < DateTime.UtcNow)
                throw new InvalidOperationException("Staj teklifinin süresi dolmuş");

            // Başvuruyu hazırla
            application.Status = CoreApplicationStatus.Pending;
            application.CreatedAt = DateTime.UtcNow;

            // Kaydetmeden önce debug bilgileri logla
            DiagnosticsLogger.Log("AddApplication_BeforeSave", new { application.StudentId, application.InternshipOfferId, application.StartDate, application.EndDate });

            await _unitOfWork.InternshipApplications.AddAsync(application);

            try
            {
                var saveResult = await _unitOfWork.SaveAsync();
                DiagnosticsLogger.Log("AddApplication_AfterSave", new { saveResult, applicationId = application.Id });

                // Şirketteki tüm kullanıcıları bildirime dahil et
                try
                {
                    // Teklifin sahibi şirketin kullanıcılarını al
                    var companyUsers = await _unitOfWork.CompanyUsers.GetUsersByCompanyIdAsync(offer.CompanyId);

                    // Debug logging - geçici dosyaya yaz
                    try
                    {
                        var logPath = Path.Combine(Path.GetTempPath(), "NotifyCompany_Debug.log");
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine($"[{DateTime.UtcNow:o}] Notify attempt for OfferId={offer.Id}, CompanyId={offer.CompanyId}, ApplicationId={application.Id}");
                        sb.AppendLine($"Found companyUsers count: {companyUsers?.Count ?? 0}");
                        if (companyUsers != null && companyUsers.Any())
                        {
                            sb.AppendLine("UserIds: " + string.Join(',', companyUsers.Select(cu => cu.UserId)));
                        }
                        System.IO.File.AppendAllText(logPath, sb.ToString());
                    }
                    catch { /* Logging hatalarını görmezden gel */ }

                    // Öğrenci bilgilerini al (gönderen adı için)
                    var student = await _unitOfWork.Users.GetByIdAsync(application.StudentId);
                    var studentName = student?.FullName ?? "Bir öğrenci";

                    var title = "Yeni Başvuru";
                    var message = $"{studentName} ilanınıza '{offer.Title}' için başvurdu.";

                    // Grup bildirimi oluştur
                    try
                    {
                        var userIds = companyUsers?.Select(cu => cu.UserId).Distinct().ToList() ?? new List<int>();
                        if (userIds.Any())
                        {
                            var groupKey = $"company_{offer.CompanyId}_applications";
                            await _notificationService.CreateGroupNotificationAsync(userIds, title, message, groupKey, "Application");

                            try
                            {
                                var logPath = Path.Combine(Path.GetTempPath(), "NotifyCompany_Debug.log");
                                System.IO.File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Group notification created for CompanyId={offer.CompanyId}, UsersCount={userIds.Count}\n");
                            }
                            catch { }
                        }
                        else
                        {
                            try
                            {
                                var logPath = Path.Combine(Path.GetTempPath(), "NotifyCompany_Debug.log");
                                System.IO.File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] No company users found to notify for CompanyId={offer.CompanyId}\n");
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        DiagnosticsLogger.Log("CreateGroupNotification_Error", new { companyId = offer.CompanyId, ex = ex.ToString() });
                        try
                        {
                            var logPath = Path.Combine(Path.GetTempPath(), "NotifyCompany_Debug.log");
                            System.IO.File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Group notification ERROR for CompanyId={offer.CompanyId}: {ex.Message}\n");
                        }
                        catch { }
                    }
                }
                catch (Exception notifyEx)
                {
                    DiagnosticsLogger.Log("NotifyCompanyUsers_Error", notifyEx.ToString());
                    try
                    {
                        var logPath = Path.Combine(Path.GetTempPath(), "NotifyCompany_Debug.log");
                        System.IO.File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] NotifyCompany ERROR: {notifyEx}\n");
                    }
                    catch { }
                }
            }
            catch (DbUpdateException dbEx)
            {
                // Detaylı hata bilgilerini logla
                DiagnosticsLogger.Log("AddApplication_DbUpdateException", new { application.StudentId, application.InternshipOfferId, application.StartDate, application.EndDate }, dbEx);

                // Hızlı erişim için geçici dosyaya da yaz
                try
                {
                    var logPath = Path.Combine(Path.GetTempPath(), "AddApplication_DbUpdateError.log");
                    var details = $"{DateTime.UtcNow:o} - DbUpdateException while saving application:\n{dbEx}\nInner: {dbEx.InnerException}\nPayload: StudentId={application.StudentId}, OfferId={application.InternshipOfferId}, Start={application.StartDate}, End={application.EndDate}\n\n";
                    File.AppendAllText(logPath, details);
                }
                catch { /* Logging hatalarını görmezden gel */ }

                var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new InvalidOperationException($"Veritabanı hatası: {innerMsg}", dbEx);
            }

            return application;
        }

        /// <summary>
        /// Başvuru durumunu günceller (Onaylama/Reddetme)
        /// Yetki kontrolü yapar ve bildirim gönderir
        /// </summary>
        /// <param name="applicationId">Güncellenecek başvuru ID'si</param>
        /// <param name="newStatus">Yeni durum (Approved/Rejected)</param>
        /// <param name="actingUserId">İşlemi yapan kullanıcı ID'si</param>
        /// <param name="actingUserRole">İşlemi yapan kullanıcının rolü</param>
        /// <exception cref="ArgumentException">Geçersiz parametreler durumunda</exception>
        /// <exception cref="InvalidOperationException">İş kuralı ihlali durumunda</exception>
        /// <exception cref="UnauthorizedAccessException">Yetkisiz işlem durumunda</exception>
        public async Task UpdateApplicationStatusAsync(int applicationId, string newStatus, int actingUserId, string actingUserRole)
        {
            // İş mantığı: Durum geçişini doğrula
            if (applicationId <= 0)
                throw new ArgumentException("Geçersiz başvuru kimliği", nameof(applicationId));

            if (string.IsNullOrWhiteSpace(newStatus) || !Enum.TryParse<CoreApplicationStatus>(newStatus, out var statusEnum))
                throw new ArgumentException("Geçersiz durum", nameof(newStatus));

            var application = await _unitOfWork.InternshipApplications.GetByIdAsync(applicationId);
            if (application == null)
                throw new InvalidOperationException("Başvuru bulunamadı");

            if (application.Status == CoreApplicationStatus.Rejected && statusEnum == CoreApplicationStatus.Pending)
                throw new InvalidOperationException("Reddedilen başvuru beklemeye alınamaz");

            // Yetki kontrolü: kimler durum değiştirebilir?
            // - Şirket: Kendi ilanlarına yapılan başvuruların durumunu değiştirebilir
            // - Admin: Tüm durumları değiştirebilir
            // - Danışman: Opsiyonel, burada işlenmiyor
            if (actingUserRole.Equals("Company", StringComparison.OrdinalIgnoreCase))
            {
                // Şirketin ilan sahibi olduğunu doğrula
                var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(application.InternshipOfferId);
                if (offer == null) throw new InvalidOperationException("İlgili ilan bulunamadı");

                var company = await _unitOfWork.Companies.GetByIdAsync(offer.CompanyId);
                if (company == null) throw new InvalidOperationException("İlgili şirket bulunamadı");

                // Şirket kullanıcı ID'sini bul
                var userCompany = await _unitOfWork.CompanyUsers.GetCompanyByUserIdAsync(actingUserId);
                if (userCompany == null || userCompany.CompanyId != company.CompanyId)
                    throw new UnauthorizedAccessException("Bu işlemi gerçekleştirme yetkiniz yok.");
            }
            else if (!actingUserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Diğer roller izinli değil
                throw new UnauthorizedAccessException("Bu işlemi gerçekleştirme yetkiniz yok.");
            }

             // Eğer yeni durum Approved ise kontenjan kontrolü yap
             if (statusEnum == CoreApplicationStatus.Approved)
             {
                 var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(application.InternshipOfferId);
                 if (offer == null)
                     throw new InvalidOperationException("İlgili ilan bulunamadı");

                 int quota = 1;
                 if (offer.AvailablePositions > 0)
                 {
                     quota = offer.AvailablePositions;
                 }

                 var applicationsForOffer = await _unitOfWork.InternshipApplications.GetApplicationsByOfferIdAsync(application.InternshipOfferId);
                 var approvedCount = applicationsForOffer.Count(a => a.Status == CoreApplicationStatus.Approved);

                 if (approvedCount >= quota)
                 {
                     throw new InvalidOperationException("Bu ilan için kontenjan dolmuştur. Daha fazla başvuru onaylanamaz.");
                 }

                 // Eğer ilan ilişkili bir dönem varsa, öğrenciyi o döneme ekle (many-to-many)
                 if (offer.PeriodId > 0)
                 {
                     var periodId = offer.PeriodId;
                     var isInPeriod = await _unitOfWork.InternshipPeriods.IsStudentInPeriodAsync(periodId, application.StudentId);
                     if (!isInPeriod)
                     {
                         await _unitOfWork.InternshipPeriods.AddStudentToPeriodAsync(periodId, application.StudentId);
                         // Başvuru güncellendikten sonra Save çağrılacak, join entry'yi persist edecek
                     }
                 }
             }

             application.Status = statusEnum;
             application.UpdatedAt = DateTime.UtcNow;
             _unitOfWork.InternshipApplications.Update(application);
             await _unitOfWork.SaveAsync();

             // Durum değişikliği hakkında öğrenciye bildirim gönder
             try
             {
                 if (statusEnum == CoreApplicationStatus.Approved)
                 {
                     await _notificationService.CreateApplicationNotificationAsync(application.StudentId, "Başvurunuz Onaylandı", $"Başvurunuz '{application.InternshipOffer?.Title ?? "pozisyon"}' şirket tarafından onaylandı.", application.Id);
                 }
                 else if (statusEnum == CoreApplicationStatus.Rejected)
                 {
                     await _notificationService.CreateApplicationNotificationAsync(application.StudentId, "Başvurunuz Reddedildi", $"Başvurunuz '{application.InternshipOffer?.Title ?? "pozisyon"}' şirket tarafından reddedildi.", application.Id);
                 }
             }
             catch
             {
                 // Bildirim hatası ana akışı engellememeli
             }
         }

         /// <summary>
         /// Başvuruya yorum ekler - şirket, öğrenci veya danışman yorumu
         /// </summary>
         public async Task AddCommentAsync(InternshipComment comment)
         {
             if (comment == null)
                 throw new ArgumentNullException(nameof(comment));

             if (string.IsNullOrWhiteSpace(comment.Content))
                 throw new ArgumentException("Yorum içeriği boş olamaz", nameof(comment.Content));

             if (comment.ApplicationId <= 0)
                 throw new ArgumentException("Geçersiz başvuru kimliği", nameof(comment.ApplicationId));

             if (comment.SenderUserId <= 0)
                 throw new ArgumentException("Geçersiz gönderen kullanıcı kimliği", nameof(comment.SenderUserId));

             var application = await _unitOfWork.InternshipApplications.GetByIdAsync(comment.ApplicationId);
             if (application == null)
                 throw new InvalidOperationException("Başvuru bulunamadı");
         
             
             comment.SentAt = DateTime.UtcNow;
             await _unitOfWork.InternshipComments.AddCommentAsync(comment);
             await _unitOfWork.SaveAsync();
         }

         /// <summary>
         /// Öğrencinin başvurularını getirir - dashboard için
         /// </summary>
         public async Task<List<InternshipApplication>> GetStudentApplicationsAsync(int studentId)
         {
             if (studentId <= 0)
                 throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

             return await _unitOfWork.InternshipApplications
                 .GetApplicationsByStudentIdAsync(studentId);
         }

         /// <summary>
         /// Başvuruyu geri çeker - öğrenci tarafından iptal edilir
         /// </summary>
         public async Task<bool> WithdrawApplicationAsync(int applicationId)
         {
             if (applicationId <= 0)
                 throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

             var application = await _unitOfWork.InternshipApplications.GetByIdAsync(applicationId);
             if (application == null)
                 throw new InvalidOperationException("Başvuru bulunamadı");

             if (application.Status == CoreApplicationStatus.Approved)
                 throw new InvalidOperationException("Onaylanan başvuru geri çekilemez");

             application.Status = CoreApplicationStatus.Rejected;
             application.UpdatedAt = DateTime.UtcNow;
             _unitOfWork.InternshipApplications.Update(application);
             await _unitOfWork.SaveAsync();
             return true;
         }

         /// <summary>
         /// Başvuruyu kalıcı olarak siler - sadece reddedilen başvurular
         /// </summary>
         public async Task DeleteApplicationAsync(int applicationId)
         {
             if (applicationId <= 0)
                 throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

             var application = await _unitOfWork.InternshipApplications.GetByIdAsync(applicationId);
             if (application == null)
                 throw new InvalidOperationException("Başvuru bulunamadı");

             // Sadece geri çekilmiş/reddedilmiş başvuruların silinmesine izin ver
             if (application.Status != CoreApplicationStatus.Rejected)
                 throw new InvalidOperationException("Sadece geri çekilmiş başvurular silinebilir.");

             // İlişkili belge bağlantılarını temizle (dosyaları silme)
             var docs = await _unitOfWork.InternshipDocuments.Where(d => d.ApplicationId == applicationId);
             foreach (var d in docs)
             {
                 d.ApplicationId = null;
                 _unitOfWork.InternshipDocuments.Update(d);
             }

             _unitOfWork.InternshipApplications.Remove(application);
             await _unitOfWork.SaveAsync();
         }

         /// <summary>
         /// ID'ye göre başvuru getirir - detay sayfası için
         /// </summary>
         public async Task<InternshipApplication> GetByIdAsync(int applicationId)
         {
             if (applicationId <= 0)
                 throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

             var app = await _unitOfWork.InternshipApplications.GetApplicationWithDetailsAsync(applicationId);
             if (app == null)
                 throw new InvalidOperationException("Başvuru bulunamadı");

             return app;
         }

         /// <summary>
         /// Öğrencinin onaylanmış başvurularını getirir - staj formu için
         /// </summary>
         public async Task<List<InternshipApplication>> GetApprovedByStudentIdAsync(int studentId)
         {
             return await _unitOfWork.InternshipApplications
                 .GetApprovedByStudentIdAsync(studentId);
         }
     }
 }