using Business.Interfaces.Internship;
using Business.Interfaces.Order;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Internship
{
    public class InternshipApprovalService : IInternshipApprovalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IINotificationService _notificationService;

        public InternshipApprovalService(IUnitOfWork unitOfWork, IINotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<List<InternshipApprovalForm>> GetApprovalFormsByStudentIdAsync(int studentId)
        {
            if (studentId <= 0)
                throw new ArgumentException("Geçersiz öðrenci ID", nameof(studentId));

            return await _unitOfWork.InternshipApprovals.GetApprovalFormsByStudentIdAsync(studentId);
        }

        public async Task<List<InternshipApprovalForm>> GetApprovalFormsByAdvisorIdAsync(int advisorId)
        {
            if (advisorId <= 0)
                throw new ArgumentException("Geçersiz danýþman ID", nameof(advisorId));

            return await _unitOfWork.InternshipApprovals.GetApprovalFormsByAdvisorIdAsync(advisorId);
        }

        public async Task<List<InternshipApprovalForm>> GetPendingApprovalFormsAsync()
        {
            return await _unitOfWork.InternshipApprovals.GetPendingApprovalFormsAsync();
        }

        public async Task<InternshipApprovalForm?> GetApprovalFormWithDetailsAsync(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Geçersiz form ID", nameof(formId));

            return await _unitOfWork.InternshipApprovals.GetApprovalFormWithDetailsAsync(formId);
        }

        public async Task<InternshipApprovalForm?> GetApprovalFormByApplicationIdAsync(int applicationId)
        {
            if (applicationId <= 0)
                throw new ArgumentException("Geçersiz baþvuru ID", nameof(applicationId));

            return await _unitOfWork.InternshipApprovals.GetApprovalFormByApplicationIdAsync(applicationId);
        }

        public async Task<InternshipApprovalForm> CreateApprovalFormAsync(InternshipApprovalForm form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            // Validation
            if (form.StudentId <= 0)
                throw new ArgumentException("Geçersiz öðrenci ID", nameof(form.StudentId));

            if (string.IsNullOrWhiteSpace(form.CompanyName))
                throw new ArgumentException("Þirket adý gerekli", nameof(form.CompanyName));

            if (form.StartDate >= form.EndDate)
                throw new ArgumentException("Baþlangýç tarihi bitiþ tarihinden önce olmalý");

            // Eðer ApplicationId > 0 ise normal baþvuru onay formu
            if (form.ApplicationId > 0)
            {
                // Bu baþvuru için zaten bir onay formu var mý kontrol et
                var existingForm = await _unitOfWork.InternshipApprovals.GetApprovalFormByApplicationIdAsync(form.ApplicationId);
                if (existingForm != null)
                    throw new InvalidOperationException("Bu baþvuru için zaten bir onay formu mevcut");

                // Baþvurunun onaylandýðýndan emin ol
                var application = await _unitOfWork.InternshipApplications.GetByIdAsync(form.ApplicationId);
                if (application == null)
                    throw new InvalidOperationException("Baþvuru bulunamadý");

                if (application.Status != InternshipApplicationStatus.Approved)
                    throw new InvalidOperationException("Sadece onaylanmýþ baþvurular için onay formu oluþturulabilir");
            }
            // ApplicationId = 0 ise direkt belge yükleme sonrasý onay formu

            // Danýþman bilgisini öðrenciden al
            var student = await _unitOfWork.Users.GetByIdAsync(form.StudentId);
            if (student?.UniversityId.HasValue == true)
            {
                // Ayný üniversiteden bir danýþman ata (basit implementasyon)
                var advisors = await _unitOfWork.Users.Where(u => 
                    u.Role == UserRole.Advisor && 
                    u.UniversityId == student.UniversityId);
                
                if (advisors.Any())
                {
                    form.AdvisorId = advisors.First().Id;
                }
            }

            // Ensure dates are UTC to avoid Npgsql timestamptz Kind issues
            form.StartDate = DateTime.SpecifyKind(form.StartDate, DateTimeKind.Utc);
            form.EndDate = DateTime.SpecifyKind(form.EndDate, DateTimeKind.Utc);
            form.CreatedAt = DateTime.UtcNow;
            form.Status = InternshipApprovalStatus.Pending;

            await _unitOfWork.InternshipApprovals.AddAsync(form);
            await _unitOfWork.SaveAsync();

            return form;
        }

        public async Task UpdateApprovalFormAsync(InternshipApprovalForm form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var existingForm = await _unitOfWork.InternshipApprovals.GetByIdAsync(form.Id);
            if (existingForm == null)
                throw new InvalidOperationException("Onay formu bulunamadý");

            if (existingForm.Status != InternshipApprovalStatus.Pending)
                throw new InvalidOperationException("Sadece beklemede olan formlar güncellenebilir");

            // Ensure dates remain UTC
            form.StartDate = DateTime.SpecifyKind(form.StartDate, DateTimeKind.Utc);
            form.EndDate = DateTime.SpecifyKind(form.EndDate, DateTimeKind.Utc);

            _unitOfWork.InternshipApprovals.Update(form);
            await _unitOfWork.SaveAsync();
        }

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

            // If this approval is linked to an application, set application status to Approved
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

                        // Create internship diary for application if not exists
                        var diaries = await _unitOfWork.InternshipDiaries.GetDiariesByStudentIdAsync(application.StudentId);
                        var existing = diaries.FirstOrDefault(d => d.ApplicationId.HasValue && d.ApplicationId.Value == application.Id);
                        if (existing == null)
                        {
                            // Try to get offer/company information for better diary metadata
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

                            // Send notification to student about new diary
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
                catch
                {
                    // swallow exceptions to avoid breaking approval flow; errors will be logged elsewhere
                }
            }
        }

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

        public async Task<bool> CanCreateApprovalFormAsync(int applicationId)
        {
            if (applicationId <= 0)
                return false;

            // Baþvuru onaylandý mý?
            var application = await _unitOfWork.InternshipApplications.GetByIdAsync(applicationId);
            if (application?.Status != InternshipApplicationStatus.Approved)
                return false;

            // Zaten bir onay formu var mý?
            var existingForm = await _unitOfWork.InternshipApprovals.GetApprovalFormByApplicationIdAsync(applicationId);
            if (existingForm == null)
                return true; // hiç form yok -> oluþturulabilir

            // Form varsa ve reddedilmiþse (advisor/öðretmen reddetmiþ) yeniden form oluþturulmasýna izin ver
            if (existingForm.Status == InternshipApprovalStatus.Rejected)
                return true;

            // Pending ya da Approved ise yeniden oluþturulamaz
            return false;
        }
    }
}