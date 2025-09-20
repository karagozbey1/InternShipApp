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
    // Staj başvuru servisi
    public class InternshipApplicationService : IInternshipApplicationService
    {
        // Veritabanı işlemleri
        private readonly IUnitOfWork _unitOfWork;
        
        // Bildirim servisi
        private readonly IINotificationService _notificationService;

        // Kurucu metod
        public InternshipApplicationService(IUnitOfWork unitOfWork, IINotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        // Öğrencinin başvurularını getirir
        public async Task<List<InternshipApplication>> GetApplicationsByStudentIdAsync(int studentId)
        {
            if (studentId <= 0)
                throw new ArgumentException("Geçersiz Öğrenci ID", nameof(studentId));

            return await _unitOfWork.InternshipApplications.GetApplicationsByStudentIdAsync(studentId);
        }

        // İlana yapılan başvuruları getirir
        public async Task<List<InternshipApplication>> GetApplicationsByOfferIdAsync(int offerId)
        {
            if (offerId <= 0)
                throw new ArgumentException("Geçersiz Staj Kimliği ID", nameof(offerId));

            return await _unitOfWork.InternshipApplications.GetApplicationsByOfferIdAsync(offerId);
        }

        // Detaylı başvuru bilgisi getirir
        public async Task<InternshipApplication?> GetApplicationWithDetailsAsync(int applicationId)
        {
            if (applicationId <= 0)
                throw new ArgumentException("Geçersiz Başvuru ID", nameof(applicationId));

            return await _unitOfWork.InternshipApplications.GetApplicationWithDetailsAsync(applicationId);
        }

        // Duruma göre başvuruları getirir
        public async Task<List<InternshipApplication>> GetApplicationsByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status) || !Enum.TryParse<CoreApplicationStatus>(status, out _))
                throw new ArgumentException("Durum Geçersiz", nameof(status));

            return await _unitOfWork.InternshipApplications.GetApplicationsByStatusAsync(status);
        }

        // Yeni başvuru ekler
        public async Task<InternshipApplication> AddApplicationAsync(InternshipApplication application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            if (application.StudentId <= 0)
                throw new ArgumentException("Geçersiz öğrenci kimliği", nameof(application.StudentId));

            if (application.InternshipOfferId <= 0)
                throw new ArgumentException("Geçersiz staj teklifi kimliği", nameof(application.InternshipOfferId));

            // Tekrar başvuru kontrolü
            var existingApplication = await _unitOfWork.InternshipApplications
                .GetApplicationsByStudentIdAsync(application.StudentId);
            
            if (existingApplication.Any(app => app.InternshipOfferId == application.InternshipOfferId &&
                                             app.Status != CoreApplicationStatus.Rejected))
            {
                throw new InvalidOperationException("Öğrencinin bu teklif için zaten aktif bir başvuru var");
            }

            // İlan kontrolü
            var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(application.InternshipOfferId);
            if (offer == null)
                throw new InvalidOperationException("Staj teklifi mevcut değil");
            
            if (offer.ApplicationDeadline < DateTime.UtcNow)
                throw new InvalidOperationException("Staj teklifinin süresi dolmuş");

            application.Status = CoreApplicationStatus.Pending;
            application.CreatedAt = DateTime.UtcNow;

            DiagnosticsLogger.Log("AddApplication_BeforeSave", new { application.StudentId, application.InternshipOfferId, application.StartDate, application.EndDate });

            await _unitOfWork.InternshipApplications.AddAsync(application);

            try
            {
                var saveResult = await _unitOfWork.SaveAsync();
                DiagnosticsLogger.Log("AddApplication_AfterSave", new { saveResult, applicationId = application.Id });

                // Şirkete bildirim gönder
                try
                {
                    var companyUsers = await _unitOfWork.CompanyUsers.GetUsersByCompanyIdAsync(offer.CompanyId);

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
                    catch { }

                    var student = await _unitOfWork.Users.GetByIdAsync(application.StudentId);
                    var studentName = student?.FullName ?? "Bir öğrenci";

                    var title = "Yeni Başvuru";
                    var message = $"{studentName} ilanınıza '{offer.Title}' için başvurdu.";

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
                DiagnosticsLogger.Log("AddApplication_DbUpdateException", new { application.StudentId, application.InternshipOfferId, application.StartDate, application.EndDate }, dbEx);

                try
                {
                    var logPath = Path.Combine(Path.GetTempPath(), "AddApplication_DbUpdateError.log");
                    var details = $"{DateTime.UtcNow:o} - DbUpdateException while saving application:\n{dbEx}\nInner: {dbEx.InnerException}\nPayload: StudentId={application.StudentId}, OfferId={application.InternshipOfferId}, Start={application.StartDate}, End={application.EndDate}\n\n";
                    File.AppendAllText(logPath, details);
                }
                catch { }

                var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new InvalidOperationException($"Veritabanı hatası: {innerMsg}", dbEx);
            }

            return application;
        }

        // Başvuru durumu günceller
        public async Task UpdateApplicationStatusAsync(int applicationId, string newStatus, int actingUserId, string actingUserRole)
        {
            if (applicationId <= 0)
                throw new ArgumentException("Geçersiz başvuru kimliği", nameof(applicationId));

            if (string.IsNullOrWhiteSpace(newStatus) || !Enum.TryParse<CoreApplicationStatus>(newStatus, out var statusEnum))
                throw new ArgumentException("Geçersiz durum", nameof(newStatus));

            var application = await _unitOfWork.InternshipApplications.GetByIdAsync(applicationId);
            if (application == null)
                throw new InvalidOperationException("Başvuru bulunamadı");

            if (application.Status == CoreApplicationStatus.Rejected && statusEnum == CoreApplicationStatus.Pending)
                throw new InvalidOperationException("Reddedilen başvuru beklemeye alınamaz");

            // Yetki kontrolü
            if (actingUserRole.Equals("Company", StringComparison.OrdinalIgnoreCase))
            {
                var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(application.InternshipOfferId);
                if (offer == null) throw new InvalidOperationException("İlgili ilan bulunamadı");

                var company = await _unitOfWork.Companies.GetByIdAsync(offer.CompanyId);
                if (company == null) throw new InvalidOperationException("İlgili şirket bulunamadı");

                var userCompany = await _unitOfWork.CompanyUsers.GetCompanyByUserIdAsync(actingUserId);
                if (userCompany == null || userCompany.CompanyId != company.CompanyId)
                    throw new UnauthorizedAccessException("Bu işlemi gerçekleştirme yetkiniz yok.");
            }
            else if (!actingUserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Bu işlemi gerçekleştirme yetkiniz yok.");
            }

             // Kontenjan kontrolü
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

                 // Dönem kaydı
                 if (offer.PeriodId > 0)
                 {
                     var periodId = offer.PeriodId;
                     
                     var isInPeriod = await _unitOfWork.InternshipPeriods.IsStudentInPeriodAsync(periodId, application.StudentId);
                     if (!isInPeriod)
                     {
                         await _unitOfWork.InternshipPeriods.AddStudentToPeriodAsync(periodId, application.StudentId);
                     }
                 }
             }

             application.Status = statusEnum;
             application.UpdatedAt = DateTime.UtcNow;
             
             _unitOfWork.InternshipApplications.Update(application);
             await _unitOfWork.SaveAsync();

             // Öğrenciye bildirim
             try
             {
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
             catch { }
         }

         // Yorum ekler
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

         // Öğrenci başvurularını getirir
         public async Task<List<InternshipApplication>> GetStudentApplicationsAsync(int studentId)
         {
             if (studentId <= 0)
                 throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

             return await _unitOfWork.InternshipApplications
                 .GetApplicationsByStudentIdAsync(studentId);
         }

         // Başvuru geri çeker
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

         // Başvuru siler
         public async Task DeleteApplicationAsync(int applicationId)
         {
             if (applicationId <= 0)
                 throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

             var application = await _unitOfWork.InternshipApplications.GetByIdAsync(applicationId);
             if (application == null)
                 throw new InvalidOperationException("Başvuru bulunamadı");

             if (application.Status != CoreApplicationStatus.Rejected)
                 throw new InvalidOperationException("Sadece geri çekilmiş başvurular silinebilir.");

             // İlişkili belgeleri ayır
             var docs = await _unitOfWork.InternshipDocuments.Where(d => d.ApplicationId == applicationId);
             foreach (var d in docs)
             {
                 d.ApplicationId = null;
                 _unitOfWork.InternshipDocuments.Update(d);
             }

             _unitOfWork.InternshipApplications.Remove(application);
             await _unitOfWork.SaveAsync();
         }

         // Kimlik ile başvuru getirir
         public async Task<InternshipApplication> GetByIdAsync(int applicationId)
         {
             if (applicationId <= 0)
                 throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

             var app = await _unitOfWork.InternshipApplications.GetApplicationWithDetailsAsync(applicationId);
             if (app == null)
                 throw new InvalidOperationException("Başvuru bulunamadı");

             return app;
         }

         // Onaylı başvuruları getirir
         public async Task<List<InternshipApplication>> GetApprovedByStudentIdAsync(int studentId)
         {
             return await _unitOfWork.InternshipApplications
                 .GetApprovedByStudentIdAsync(studentId);
         }
     }
 }