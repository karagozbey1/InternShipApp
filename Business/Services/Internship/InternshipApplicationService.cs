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
    // Staj başvuru işlemlerini yöneten ana servis sınıfı
    // Bu sınıf öğrencilerin staj başvurularını yönetir, durum güncellemelerini yapar
    // Şirketlerin başvuruları değerlendirme süreçlerini koordine eder
    // Bildirim gönderimi, iş kuralları kontrolü ve veri doğrulama işlemlerini içerir
    public class InternshipApplicationService : IInternshipApplicationService
    {
        // Veritabanı işlemleri için Unit of Work pattern kullanılır
        // Bu sayede tüm repository'lere merkezi bir noktadan erişim sağlanır
        // Transaction yönetimi ve consistent veri işlemleri için kritiktir
        private readonly IUnitOfWork _unitOfWork;
        
        // Bildirim servisi - başvuru durumu değişikliklerinde kullanıcıları bilgilendirir
        // E-posta bildirimleri, sistem içi mesajlar ve push notification'lar gönderir
        // Async olarak çalışır böylece ana iş akışını engellemez
        private readonly IINotificationService _notificationService;

        // Dependency injection ile servislerin constructor'a enjekte edilmesi
        // Bu pattern test edilebilirlik ve loose coupling için önemli
        public InternshipApplicationService(IUnitOfWork unitOfWork, IINotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        // Belirli bir öğrencinin tüm başvurularını getiren method
        // Öğrenci dashboard'ında geçmiş başvuruları göstermek için kullanılır
        // İlişkili verilerle birlikte (şirket, pozisyon bilgileri) getirir
        public async Task<List<InternshipApplication>> GetApplicationsByStudentIdAsync(int studentId)
        {
            // Güvenlik kontrolü - negatif veya sıfır ID değerleri kabul edilmez
            if (studentId <= 0)
                {
                throw new ArgumentException("Geçersiz Öğrenci ID", nameof(studentId));
            }

            // Repository pattern üzerinden veritabanı sorgusu
            // EF Core Include() metodları ile related data da çekilir
            return await _unitOfWork.InternshipApplications.GetApplicationsByStudentIdAsync(studentId);
        }

        // Belirli bir staj ilanına yapılan tüm başvuruları getirir
        // Şirket temsilcilerinin başvuru listesini görmeleri için kullanılır
        // Başvuru sahibi öğrenci bilgileri ve belgeleri de dahil edilir
        public async Task<List<InternshipApplication>> GetApplicationsByOfferIdAsync(int offerId)
        {
            // İlan ID validasyonu - geçersiz değerler reddedilir
            if (offerId <= 0)
                throw new ArgumentException("Geçersiz Staj Kimliği ID", nameof(offerId));

            // Tek sorguda ilgili tüm başvurular çekilir
            // Performans için sayfalama (pagination) eklenebilir
            return await _unitOfWork.InternshipApplications.GetApplicationsByOfferIdAsync(offerId);
        }

        // Başvuru detaylarını tüm ilişkili verilerle birlikte getirir
        // Detay sayfasında gösterilecek şirket, öğrenci, teklif bilgilerini include eder
        // Navigation property'leri de yüklenir (eager loading)
        public async Task<InternshipApplication?> GetApplicationWithDetailsAsync(int applicationId)
        {
            // ID kontrolü - business logic'in en temel parçası
            if (applicationId <= 0)
                throw new ArgumentException("Geçersiz Başvuru ID", nameof(applicationId));

            // Include ile related entities yüklenir
            // Null dönebilir - calling code'da null check gerekli
            return await _unitOfWork.InternshipApplications.GetApplicationWithDetailsAsync(applicationId);
        }

        // Sistem genelinde belirli durumdaki başvuruları filtreler
        // Admin panelinde raporlama ve toplu işlemler için kullanılır
        // Enum validation ile tip güvenliği sağlanır
        public async Task<List<InternshipApplication>> GetApplicationsByStatusAsync(string status)
        {
            // String to enum parsing ile güvenli tip dönüşümü
            // Geçersiz status değerleri exception fırlatır
            if (string.IsNullOrWhiteSpace(status) || !Enum.TryParse<CoreApplicationStatus>(status, out _))
                throw new ArgumentException("Durum Geçersiz", nameof(status));

            // Database seviyesinde filtreleme yapılır
            // Index'li sorgular için performans optimizasyonu mümkün
            return await _unitOfWork.InternshipApplications.GetApplicationsByStatusAsync(status);
        }

        // Yeni staj başvurusu oluşturma - en kritik business logic
        // Çoklu validasyon kontrolü, duplicate check ve notification gönderimi içerir
        // Transaction içinde çalışır, hata durumunda rollback yapılır
        public async Task<InternshipApplication> AddApplicationAsync(InternshipApplication application)
        {
            // İş mantığı validasyonları - null object pattern koruması
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // Foreign key validasyonları - referential integrity için kritik
            if (application.StudentId <= 0)
                throw new ArgumentException("Geçersiz öğrenci kimliği", nameof(application.StudentId));

            if (application.InternshipOfferId <= 0)
                throw new ArgumentException("Geçersiz staj teklifi kimliği", nameof(application.InternshipOfferId));

            // Duplicate başvuru kontrolü - business rule enforcement
            // Aynı öğrencinin aynı ilana birden fazla aktif başvuru yapmasını engeller
            var existingApplication = await _unitOfWork.InternshipApplications
                .GetApplicationsByStudentIdAsync(application.StudentId);
            
            // LINQ kullanarak mevcut başvuruları filtrele
            // Rejected durumundaki başvurular tekrar yapılabilir
            if (existingApplication.Any(app => app.InternshipOfferId == application.InternshipOfferId &&
                                             app.Status != CoreApplicationStatus.Rejected))
            {
                throw new InvalidOperationException("Öğrencinin bu teklif için zaten aktif bir başvuru var");
            }

            // İlan geçerliliği kontrolü - foreign key integrity check
            // Related entity'nin varlığı ve business rules kontrolü
            var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(application.InternshipOfferId);
            if (offer == null)
                throw new InvalidOperationException("Staj teklifi mevcut değil");
            
            // Tarih validasyonu - deadline geçmiş ilanları engelle
            // Business rule: süresi dolmuş ilanlara başvuru kabul edilmez
            if (offer.ApplicationDeadline < DateTime.UtcNow)
                throw new InvalidOperationException("Staj teklifinin süresi dolmuş");

            // Entity'yi kaydetmek için hazırla
            // Default değerler atanır, timestamps set edilir
            application.Status = CoreApplicationStatus.Pending; // Yeni başvurular her zaman pending
            application.CreatedAt = DateTime.UtcNow; // Server zamanı kullanılır

            // Debug logging - production'da kaldırılabilir veya log level ayarlanabilir
            // Troubleshooting için önemli bilgileri logla
            DiagnosticsLogger.Log("AddApplication_BeforeSave", new { application.StudentId, application.InternshipOfferId, application.StartDate, application.EndDate });

            // Repository pattern ile entity'yi context'e ekle
            // Henüz database'e yazılmaz, sadece tracked olur
            await _unitOfWork.InternshipApplications.AddAsync(application);

            try
            {
                // Unit of Work pattern ile transaction commit
                // Tüm değişiklikler atomik olarak database'e yazılır
                var saveResult = await _unitOfWork.SaveAsync();
                DiagnosticsLogger.Log("AddApplication_AfterSave", new { saveResult, applicationId = application.Id });

                // Bildirim gönderimi - async olarak şirket kullanıcılarına haber ver
                // Bu işlem main business flow'u engellememelidir
                try
                {
                    // Şirketin tüm kullanıcılarını al (CompanyUser many-to-many relationship)
                    // İK personeli, staj koordinatörü gibi yetkili kişilere bildirim gönderilir
                    var companyUsers = await _unitOfWork.CompanyUsers.GetUsersByCompanyIdAsync(offer.CompanyId);

                    // Debug logging - notification troubleshooting için
                    // Geçici dosyaya yazılır, production'da log service kullanılmalı
                    try
                    {
                        var logPath = Path.Combine(Path.GetTempPath(), "NotifyCompany_Debug.log");
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine($"[{DateTime.UtcNow:o}] Notify attempt for OfferId={offer.Id}, CompanyId={offer.CompanyId}, ApplicationId={application.Id}");
                        sb.AppendLine($"Found companyUsers count: {companyUsers?.Count ?? 0}");
                        
                        // User ID'lerini logla - hangi kullanıcılara bildirim gönderildiğini trace et
                        if (companyUsers != null && companyUsers.Any())
                        {
                            sb.AppendLine("UserIds: " + string.Join(',', companyUsers.Select(cu => cu.UserId)));
                        }
                        System.IO.File.AppendAllText(logPath, sb.ToString());
                    }
                    catch { /* Logging hatalarını görmezden gel - main flow devam etmeli */ }

                    // Bildirim içeriği hazırlama - kullanıcı dostu mesaj oluştur
                    // Öğrenci adını ve pozisyon başlığını dinamik olarak ekle
                    var student = await _unitOfWork.Users.GetByIdAsync(application.StudentId);
                    var studentName = student?.FullName ?? "Bir öğrenci"; // Fallback değer

                    var title = "Yeni Başvuru";
                    var message = $"{studentName} ilanınıza '{offer.Title}' için başvurdu.";

                    // Grup bildirimi oluşturma - batch notification için optimize
                    // Tek tek bildirim yerine group notification daha efficient
                    try
                    {
                        // LINQ ile user ID'lerini al ve unique yap
                        var userIds = companyUsers?.Select(cu => cu.UserId).Distinct().ToList() ?? new List<int>();
                        
                        // En az bir alıcı varsa bildirim gönder
                        if (userIds.Any())
                        {
                            // Group key ile ilgili bildirimleri gruplayabilir
                            // Company başına ayrı grup oluşturarak organizasyon sağla
                            var groupKey = $"company_{offer.CompanyId}_applications";
                            await _notificationService.CreateGroupNotificationAsync(userIds, title, message, groupKey, "Application");

                            // Success logging - bildirim gönderim başarısını trace et
                            try
                            {
                                var logPath = Path.Combine(Path.GetTempPath(), "NotifyCompany_Debug.log");
                                System.IO.File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Group notification created for CompanyId={offer.CompanyId}, UsersCount={userIds.Count}\n");
                            }
                            catch { }
                        }
                        else
                        {
                            // Warning logging - bildirim alacak kullanıcı bulunamadığında
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
                        // Bildirim hatası ana işlemi etkilememelidir
                        // Error logging ile sorun trace edilebilir
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
                    // Notification service'teki genel hatalar
                    // Bu hatalar user'a gösterilmez, sadece loglanır
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
                // Database constraint violation veya connection errors
                // Detaylı hata bilgileri debugging için kaydedilir
                DiagnosticsLogger.Log("AddApplication_DbUpdateException", new { application.StudentId, application.InternshipOfferId, application.StartDate, application.EndDate }, dbEx);

                // Quick access için temp file'a da yaz
                // Development ortamında hızlı debug için kullanışlı
                try
                {
                    var logPath = Path.Combine(Path.GetTempPath(), "AddApplication_DbUpdateError.log");
                    var details = $"{DateTime.UtcNow:o} - DbUpdateException while saving application:\n{dbEx}\nInner: {dbEx.InnerException}\nPayload: StudentId={application.StudentId}, OfferId={application.InternshipOfferId}, Start={application.StartDate}, End={application.EndDate}\n\n";
                    File.AppendAllText(logPath, details);
                }
                catch { /* Logging hatalarını görmezden gel */ }

                // User-friendly error message oluştur
                // Inner exception'dan daha anlamlı mesaj çıkar
                var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new InvalidOperationException($"Veritabanı hatası: {innerMsg}", dbEx);
            }

            // Successfully created application'ı return et
            return application;
        }

        // Başvuru durumu güncelleme - kritik business operation
        // Authorization kontrolü, kontenjan yönetimi ve bildirim içerir
        // Şirket yetkilileri ve admin'ler bu işlemi yapabilir
        public async Task UpdateApplicationStatusAsync(int applicationId, string newStatus, int actingUserId, string actingUserRole)
        {
            // Input validation - method parameter kontrolü
            if (applicationId <= 0)
                throw new ArgumentException("Geçersiz başvuru kimliği", nameof(applicationId));

            // String to enum parsing ile type safety
            // Invalid status değerleri runtime'da exception fırlatır
            if (string.IsNullOrWhiteSpace(newStatus) || !Enum.TryParse<CoreApplicationStatus>(newStatus, out var statusEnum))
                throw new ArgumentException("Geçersiz durum", nameof(newStatus));

            // Entity varlık kontrolü - null reference protection
            var application = await _unitOfWork.InternshipApplications.GetByIdAsync(applicationId);
            if (application == null)
                throw new InvalidOperationException("Başvuru bulunamadı");

            // Business rule: Reddedilen başvuru tekrar pending'e alınamaz
            // Bu kural iş süreçlerini korur ve consistency sağlar
            if (application.Status == CoreApplicationStatus.Rejected && statusEnum == CoreApplicationStatus.Pending)
                throw new InvalidOperationException("Reddedilen başvuru beklemeye alınamaz");

            // Authorization kontrolü - role-based access control
            // Sadece yetkili kullanıcılar durum değiştirebilir
            if (actingUserRole.Equals("Company", StringComparison.OrdinalIgnoreCase))
            {
                // Şirket kullanıcısı sadece kendi ilanlarındaki başvuruları değiştirebilir
                // Cross-company access prevention için kritik güvenlik kontrolü
                var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(application.InternshipOfferId);
                if (offer == null) throw new InvalidOperationException("İlgili ilan bulunamadı");

                var company = await _unitOfWork.Companies.GetByIdAsync(offer.CompanyId);
                if (company == null) throw new InvalidOperationException("İlgili şirket bulunamadı");

                // CompanyUser relationship kontrolü - kullanıcının şirket üyesi olup olmadığı
                var userCompany = await _unitOfWork.CompanyUsers.GetCompanyByUserIdAsync(actingUserId);
                if (userCompany == null || userCompany.CompanyId != company.CompanyId)
                    throw new UnauthorizedAccessException("Bu işlemi gerçekleştirme yetkiniz yok.");
            }
            else if (!actingUserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Admin dışındaki diğer roller (Student, Advisor) bu işlemi yapamaz
                throw new UnauthorizedAccessException("Bu işlemi gerçekleştirme yetkiniz yok.");
            }

             // Kontenjan kontrolü - sadece Approved status için
             // Business rule: ilan kontenjanı dolduğunda yeni onay yapılamaz
             if (statusEnum == CoreApplicationStatus.Approved)
             {
                 // İlan bilgilerini tekrar çek - fresh data için
                 var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(application.InternshipOfferId);
                 if (offer == null)
                     throw new InvalidOperationException("İlgili ilan bulunamadı");

                 // Kontenjan hesaplama - default 1, explicit varsa o değer kullan
                 int quota = 1;
                 if (offer.AvailablePositions > 0)
                 {
                     quota = offer.AvailablePositions;
                 }

                 // Mevcut onaylı başvuru sayısını hesapla
                 // Count() yerine Where().Count() daha readable
                 var applicationsForOffer = await _unitOfWork.InternshipApplications.GetApplicationsByOfferIdAsync(application.InternshipOfferId);
                 var approvedCount = applicationsForOffer.Count(a => a.Status == CoreApplicationStatus.Approved);

                 // Quota kontrolü - business rule enforcement
                 if (approvedCount >= quota)
                 {
                     throw new InvalidOperationException("Bu ilan için kontenjan dolmuştur. Daha fazla başvuru onaylanamaz.");
                 }

                 // Period enrollment - Many-to-many relationship management
                 // Onaylanan öğrenci otomatik olarak staj dönemine kaydolur
                 if (offer.PeriodId > 0)
                 {
                     var periodId = offer.PeriodId;
                     
                     // Duplicate enrollment kontrolü - öğrenci zaten kayıtlı mı?
                     var isInPeriod = await _unitOfWork.InternshipPeriods.IsStudentInPeriodAsync(periodId, application.StudentId);
                     if (!isInPeriod)
                     {
                         // Junction table'a kayıt ekle
                         await _unitOfWork.InternshipPeriods.AddStudentToPeriodAsync(periodId, application.StudentId);
                         // SaveAsync() sonrası bu değişiklik de persist edilecek
                     }
                 }
             }

             // Entity update işlemleri - timestamp ve status güncelleme
             application.Status = statusEnum;
             application.UpdatedAt = DateTime.UtcNow; // Audit trail için önemli
             
             // Change tracking ile EF Core otomatik update detect eder
             _unitOfWork.InternshipApplications.Update(application);
             
             // Transaction commit - tüm değişiklikleri atomik olarak kaydet
             await _unitOfWork.SaveAsync();

             // Öğrenciye bildirim gönderimi - status change notification
             // Try-catch ile ana akışı korur, bildirim hatası operation'ı engellemez
             try
             {
                 // Status'e göre farklı bildirim mesajları
                 if (statusEnum == CoreApplicationStatus.Approved)
                 {
                     await _notificationService.CreateApplicationNotificationAsync(
                         application.StudentId, 
                         "Başvurunuz Onaylandı", 
                         $"Başvurunuz '{application.InternshipOffer?.Title ?? "pozisyon"}' şirket tarafından onaylandı.", 
                         application.Id);
                 }
                 else if (statusEnum == CoreApplicationStatus.Rejected)
                 {
                     await _notificationService.CreateApplicationNotificationAsync(
                         application.StudentId, 
                         "Başvurunuz Reddedildi", 
                         $"Başvurunuz '{application.InternshipOffer?.Title ?? "pozisyon"}' şirket tarafından reddedildi.", 
                         application.Id);
                 }
             }
             catch
             {
                 // Bildirim hatası ana akışı engellememelidir
                 // Silent fail - user operation successful olarak görür
             }
         }

         // Başvuruya yorum ekleme - communication feature
         // Şirket, öğrenci ve danışman arasında mesajlaşma sağlar
         public async Task AddCommentAsync(InternshipComment comment)
         {
             // Null protection - defensive programming
             if (comment == null)
                 throw new ArgumentNullException(nameof(comment));

             // Content validation - empty comments not allowed
             if (string.IsNullOrWhiteSpace(comment.Content))
                 throw new ArgumentException("Yorum içeriği boş olamaz", nameof(comment.Content));

             // Foreign key validations
             if (comment.ApplicationId <= 0)
                 throw new ArgumentException("Geçersiz başvuru kimliği", nameof(comment.ApplicationId));

             if (comment.SenderUserId <= 0)
                 throw new ArgumentException("Geçersiz gönderen kullanıcı kimliği", nameof(comment.SenderUserId));

             // Related entity existence check
             // Comment sadece var olan başvurulara eklenebilir
             var application = await _unitOfWork.InternshipApplications.GetByIdAsync(comment.ApplicationId);
             if (application == null)
                 throw new InvalidOperationException("Başvuru bulunamadı");
         
             // Timestamp otomatik atanır - server time kullanılır
             comment.SentAt = DateTime.UtcNow;
             
             // Repository pattern ile comment ekleme
             await _unitOfWork.InternshipComments.AddCommentAsync(comment);
             await _unitOfWork.SaveAsync();
         }

         // Öğrenci dashboard için başvuru listesi
         // GetApplicationsByStudentIdAsync ile aynı işlevi görür - method overloading
         public async Task<List<InternshipApplication>> GetStudentApplicationsAsync(int studentId)
         {
             // Input validation
             if (studentId <= 0)
                 throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

             // Delegate to main method - code reuse
             return await _unitOfWork.InternshipApplications
                 .GetApplicationsByStudentIdAsync(studentId);
         }

         // Başvuru geri çekme - öğrenci tarafından iptal
         // Business rule: sadece pending başvurular geri çekilebilir
         public async Task<bool> WithdrawApplicationAsync(int applicationId)
         {
             // ID validation
             if (applicationId <= 0)
                 throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

             // Entity existence check
             var application = await _unitOfWork.InternshipApplications.GetByIdAsync(applicationId);
             if (application == null)
                 throw new InvalidOperationException("Başvuru bulunamadı");

             // Business rule: onaylanmış başvuru geri çekilemez
             // Data integrity ve business process consistency için kritik
             if (application.Status == CoreApplicationStatus.Approved)
                 throw new InvalidOperationException("Onaylanan başvuru geri çekilemez");

             // Status'u Rejected'a çevir - soft delete pattern
             // Hard delete yerine status update kullanılır
             application.Status = CoreApplicationStatus.Rejected;
             application.UpdatedAt = DateTime.UtcNow; // Audit trail
             
             _unitOfWork.InternshipApplications.Update(application);
             await _unitOfWork.SaveAsync();
             
             return true; // Success indicator
         }

         // Başvuru silme - kalıcı silme işlemi
         // Sadece rejected durumundaki başvurular silinebilir
         public async Task DeleteApplicationAsync(int applicationId)
         {
             // ID validation
             if (applicationId <= 0)
                 throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

             // Entity existence check
             var application = await _unitOfWork.InternshipApplications.GetByIdAsync(applicationId);
             if (application == null)
                 throw new InvalidOperationException("Başvuru bulunamadı");

             // Business rule: sadece rejected başvurular silinebilir
             // Veri bütünlüğü ve iş süreçlerini koruma amaçlı
             if (application.Status != CoreApplicationStatus.Rejected)
                 throw new InvalidOperationException("Sadece geri çekilmiş başvurular silinebilir.");

             // Related documents cleanup - foreign key constraint handling
             // Belgeler silinmez sadece başvuru ilişkisi kaldırılır
             var docs = await _unitOfWork.InternshipDocuments.Where(d => d.ApplicationId == applicationId);
             foreach (var d in docs)
             {
                 d.ApplicationId = null; // Relationship break
                 _unitOfWork.InternshipDocuments.Update(d);
             }

             // Hard delete - entity completely removed
             _unitOfWork.InternshipApplications.Remove(application);
             await _unitOfWork.SaveAsync();
         }

         // ID ile başvuru getirme - detail sayfası için
         // GetApplicationWithDetailsAsync'a delegate eder
         public async Task<InternshipApplication> GetByIdAsync(int applicationId)
         {
             // Input validation
             if (applicationId <= 0)
                 throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

             // Detailed entity fetch - includes related data
             var app = await _unitOfWork.InternshipApplications.GetApplicationWithDetailsAsync(applicationId);
             if (app == null)
                 throw new InvalidOperationException("Başvuru bulunamadı");

             return app;
         }

         // Onaylanmış başvurular - staj formu ve raporlama için
         // Sadece approved status'taki başvurular döner
         public async Task<List<InternshipApplication>> GetApprovedByStudentIdAsync(int studentId)
         {
             // Repository'de filtering yapılır - database level where clause
             return await _unitOfWork.InternshipApplications
                 .GetApprovedByStudentIdAsync(studentId);
         }
     }
 }