using Business.Interfaces.Internship;
using Business.Interfaces.Order;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Internship
{
    // Staj onay formu servisi
    public class InternshipApprovalService : IInternshipApprovalService
    {
        // Veritabaný iþlemleri
        private readonly IUnitOfWork _unitOfWork;
        
        // Bildirim servisi
        private readonly IINotificationService _notificationService;

        // Kurucu metod
        public InternshipApprovalService(IUnitOfWork unitOfWork, IINotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        // Öðrencinin onay formlarýný getirir
        public async Task<List<InternshipApprovalForm>> GetApprovalFormsByStudentIdAsync(int studentId)
        {
            if (studentId <= 0)
                throw new ArgumentException("Geçersiz öðrenci ID", nameof(studentId));

            return await _unitOfWork.InternshipApprovals.GetApprovalFormsByStudentIdAsync(studentId);
        }

        // Danýþmanýn deðerlendireceði formlarý getirir
        public async Task<List<InternshipApprovalForm>> GetApprovalFormsByAdvisorIdAsync(int advisorId)
        {
            if (advisorId <= 0)
                throw new ArgumentException("Geçersiz danýþman ID", nameof(advisorId));

            return await _unitOfWork.InternshipApprovals.GetApprovalFormsByAdvisorIdAsync(advisorId);
        }

        // Bekleyen onay formlarýný getirir
        public async Task<List<InternshipApprovalForm>> GetPendingApprovalFormsAsync()
        {
            return await _unitOfWork.InternshipApprovals.GetPendingApprovalFormsAsync();
        }

        // Detaylý onay formu getirir
        public async Task<InternshipApprovalForm?> GetApprovalFormWithDetailsAsync(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Geçersiz form ID", nameof(formId));

            return await _unitOfWork.InternshipApprovals.GetApprovalFormWithDetailsAsync(formId);
        }

        // Baþvuruya ait onay formu getirir
        public async Task<InternshipApprovalForm?> GetApprovalFormByApplicationIdAsync(int applicationId)
        {
            if (applicationId <= 0)
                throw new ArgumentException("Geçersiz baþvuru ID", nameof(applicationId));

            return await _unitOfWork.InternshipApprovals.GetApprovalFormByApplicationIdAsync(applicationId);
        }

        // Yeni onay formu oluþturur
        public async Task<InternshipApprovalForm> CreateApprovalFormAsync(InternshipApprovalForm form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            // Gerekli alan kontrolleri
            if (form.StudentId <= 0)
                throw new ArgumentException("Geçersiz öðrenci ID", nameof(form.StudentId));

            if (string.IsNullOrWhiteSpace(form.CompanyName))
                throw new ArgumentException("Þirket adý gerekli", nameof(form.CompanyName));

            if (form.StartDate >= form.EndDate)
                throw new ArgumentException("Baþlangýç tarihi bitiþ tarihinden önce olmalý");

            if (form.ApplicationId > 0)
            {
                // Zaten form var mý kontrol et
                var existingForm = await _unitOfWork.InternshipApprovals.GetApprovalFormByApplicationIdAsync(form.ApplicationId);
                if (existingForm != null)
                    throw new InvalidOperationException("Bu baþvuru için zaten bir onay formu mevcut");

                // Baþvuru durumu kontrol et
                var application = await _unitOfWork.InternshipApplications.GetByIdAsync(form.ApplicationId);
                if (application == null)
                    throw new InvalidOperationException("Baþvuru bulunamadý");

                if (application.Status != InternshipApplicationStatus.Approved)
                    throw new InvalidOperationException("Sadece onaylanmýþ baþvurular için onay formu oluþturulabilir");
            }

            // Danýþman atamasý
            var student = await _unitOfWork.Users.GetByIdAsync(form.StudentId);
            if (student?.UniversityId.HasValue == true)
            {
                var advisors = await _unitOfWork.Users.Where(u => 
                    u.Role == UserRole.Advisor && 
                    u.UniversityId == student.UniversityId);
                
                if (advisors.Any())
                {
                    form.AdvisorId = advisors.First().Id;
                }
            }

            // Tarih ayarlamalarý
            form.StartDate = DateTime.SpecifyKind(form.StartDate, DateTimeKind.Utc);
            form.EndDate = DateTime.SpecifyKind(form.EndDate, DateTimeKind.Utc);
            form.CreatedAt = DateTime.UtcNow;
            form.Status = InternshipApprovalStatus.Pending;

            await _unitOfWork.InternshipApprovals.AddAsync(form);
            await _unitOfWork.SaveAsync();

            return form;
        }

        // Onay formu günceller
        public async Task UpdateApprovalFormAsync(InternshipApprovalForm form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var existingForm = await _unitOfWork.InternshipApprovals.GetByIdAsync(form.Id);
            if (existingForm == null)
                throw new InvalidOperationException("Onay formu bulunamadý");

            if (existingForm.Status != InternshipApprovalStatus.Pending && 
                existingForm.Status != InternshipApprovalStatus.Revision)
            {
                throw new InvalidOperationException("Sadece beklemede olan veya revizyon durumundaki formlar güncellenebilir");
            }

            // Alanlarý güncelle
            existingForm.StudentNote = form.StudentNote;
            existingForm.Status = form.Status;
            existingForm.RevisionNote = form.RevisionNote;
            existingForm.RevisedAt = form.RevisedAt;
            existingForm.RejectionReason = form.RejectionReason;
            existingForm.RejectedAt = form.RejectedAt;
            
            if (form.StartDate != DateTime.MinValue)
                existingForm.StartDate = DateTime.SpecifyKind(form.StartDate, DateTimeKind.Utc);
            if (form.EndDate != DateTime.MinValue)
                existingForm.EndDate = DateTime.SpecifyKind(form.EndDate, DateTimeKind.Utc);

            if (!string.IsNullOrEmpty(form.CompanyName))
                existingForm.CompanyName = form.CompanyName;
            if (!string.IsNullOrEmpty(form.InternshipDescription))
                existingForm.InternshipDescription = form.InternshipDescription;
            if (form.WeeklyWorkHours > 0)
                existingForm.WeeklyWorkHours = form.WeeklyWorkHours;

            await _unitOfWork.SaveAsync();
        }

        // Formu onaylar
        public async Task ApproveFormAsync(int formId, string? advisorNote = null)
        {
            if (formId <= 0)
                throw new ArgumentException("Geçersiz form ID", nameof(formId));

            var form = await _unitOfWork.InternshipApprovals.GetByIdAsync(formId);
            if (form == null)
                throw new InvalidOperationException("Onay formu bulunamadý");

            if (form.Status != InternshipApprovalStatus.Pending)
                throw new InvalidOperationException("Sadece beklemede olan formlar onaylanabilir");

            form.Status = InternshipApprovalStatus.Approved;
            form.ApprovedAt = DateTime.UtcNow;
            form.AdvisorNote = advisorNote;

            _unitOfWork.InternshipApprovals.Update(form);
            await _unitOfWork.SaveAsync();

            // Baþvuru durumunu güncelle
            if (form.ApplicationId > 0)
            {
                try
                {
                    var application = await _unitOfWork.InternshipApplications.GetByIdAsync(form.ApplicationId);
                    if (application != null)
                    {
                        application.Status = InternshipApplicationStatus.Approved;
                        _unitOfWork.InternshipApplications.Update(application);
                        await _unitOfWork.SaveAsync();

                        // Staj defteri oluþtur
                        var diaries = await _unitOfWork.InternshipDiaries.GetDiariesByStudentIdAsync(application.StudentId);
                        var existing = diaries.FirstOrDefault(d => d.ApplicationId.HasValue && d.ApplicationId.Value == application.Id);
                        if (existing == null)
                        {
                            Core.Entity.InternshipOffer? offer = null;
                            try
                            {
                                offer = await _unitOfWork.InternshipOffers.GetByIdAsync(application.InternshipOfferId);
                            }
                            catch { }

                            var diary = new Core.Entity.InternshipDiary
                            {
                                StudentId = application.StudentId,
                                Title = offer?.Title ?? "Staj Defteri",
                                CompanyName = offer?.Company?.CompanyName,
                                Department = offer?.Field,
                                InternshipStartDate = application.StartDate,
                                InternshipEndDate = application.EndDate,
                                ApplicationId = application.Id,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                IsRequired = true,
                                IsCompleted = false
                            };

                            await _unitOfWork.InternshipDiaries.AddAsync(diary);
                            await _unitOfWork.SaveAsync();

                            // Bildirim gönder
                            try
                            {
                                await _notificationService.CreateNotificationAsync(new Notification
                                {
                                    UserId = application.StudentId,
                                    Title = "Staj Defteri Oluþturuldu",
                                    Message = $"Onaylanan baþvurunuz için '{diary.Title}' adlý staj defteri oluþturuldu.",
                                    Type = "Diary",
                                    RelatedEntityType = "Diary",
                                    RelatedEntityId = diary.Id,
                                    ActionUrl = $"/Student/DiaryNotebook/Detail/{diary.Id}",
                                    IsRead = false,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
        }

        // Formu reddeder
        public async Task RejectFormAsync(int formId, string rejectionReason, string? advisorNote = null)
        {
            if (formId <= 0)
                throw new ArgumentException("Geçersiz form ID", nameof(formId));

            if (string.IsNullOrWhiteSpace(rejectionReason))
                throw new ArgumentException("Red nedeni gerekli", nameof(rejectionReason));

            var form = await _unitOfWork.InternshipApprovals.GetByIdAsync(formId);
            if (form == null)
                throw new InvalidOperationException("Onay formu bulunamadý");

            if (form.Status != InternshipApprovalStatus.Pending)
                throw new InvalidOperationException("Sadece beklemede olan formlar reddedilebilir");

            form.Status = InternshipApprovalStatus.Rejected;
            form.RejectedAt = DateTime.UtcNow;
            form.RejectionReason = rejectionReason;
            form.AdvisorNote = advisorNote;

            _unitOfWork.InternshipApprovals.Update(form);
            await _unitOfWork.SaveAsync();
        }

        // Revizyon için geri gönderir
        public async Task ReturnForRevisionAsync(int formId, string revisionNote)
        {
            if (formId <= 0)
                throw new ArgumentException("Geçersiz form ID", nameof(formId));

            if (string.IsNullOrWhiteSpace(revisionNote))
                throw new ArgumentException("Revizyon notu gerekli", nameof(revisionNote));

            var form = await _unitOfWork.InternshipApprovals.GetByIdAsync(formId);
            if (form == null)
                throw new InvalidOperationException("Onay formu bulunamadý");

            if (form.Status != InternshipApprovalStatus.Pending)
                throw new InvalidOperationException("Sadece beklemede olan formlar revizyona gönderilebilir");

            form.Status = InternshipApprovalStatus.Revision;
            form.RevisedAt = DateTime.UtcNow;
            form.RevisionNote = revisionNote;

            _unitOfWork.InternshipApprovals.Update(form);
            await _unitOfWork.SaveAsync();
        }

        // Onay formu oluþturulabilir mi kontrol eder
        public async Task<bool> CanCreateApprovalFormAsync(int applicationId)
        {
            if (applicationId <= 0)
                return false;

            var application = await _unitOfWork.InternshipApplications.GetByIdAsync(applicationId);
            if (application?.Status != InternshipApplicationStatus.Approved)
                return false;

            var existingForm = await _unitOfWork.InternshipApprovals.GetApprovalFormByApplicationIdAsync(applicationId);
            if (existingForm == null)
                return true;

            if (existingForm.Status == InternshipApprovalStatus.Rejected)
                return true;

            return false;
        }
    }
}