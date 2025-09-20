using Business.Interfaces.Internship;
using Business.Interfaces.Order;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Internship
{
    // Staj onay formu yönetim servisi - academic approval workflow coordinator
    // Öðrenci-Danýþman-Üniversite arasýndaki onay süreçlerini yönetir
    // Akademik kurallara uygun staj onayý business logic'ini implement eder
    // Multi-stage approval process ile document workflow integration
    public class InternshipApprovalService : IInternshipApprovalService
    {
        // Unit of Work pattern - database transaction coordination
        // Approval süreçlerinde atomic operations ensure etmek için kritik
        private readonly IUnitOfWork _unitOfWork;
        
        // Notification service - stakeholder communication
        // Onay süreçlerinde ilgili taraflara bildirim gönderim koordinasyonu
        private readonly IINotificationService _notificationService;

        // Dependency injection constructor - service composition
        // Scoped services ile request lifecycle management
        public InternshipApprovalService(IUnitOfWork unitOfWork, IINotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        // Öðrencinin onay formlarýný getirme - student dashboard integration
        // Kiþisel onay form history ve current status tracking
        // Chronological ordering ile son formlar önce gösterilir
        public async Task<List<InternshipApprovalForm>> GetApprovalFormsByStudentIdAsync(int studentId)
        {
            // Parameter validation - defensive programming approach
            if (studentId <= 0)
                throw new ArgumentException("Geçersiz öðrenci ID", nameof(studentId));

            // Repository call - WHERE StudentId = @studentId filtering
            // Include ile related entities (Advisor, Application) loading possible
            return await _unitOfWork.InternshipApprovals.GetApprovalFormsByStudentIdAsync(studentId);
        }

        // Danýþmanýn deðerlendireceði onay formlarýný getirme - advisor workbench
        // Academic supervision workflow için form assignment
        // University-based filtering ile sadece ayný kurum öðrencileri
        public async Task<List<InternshipApprovalForm>> GetApprovalFormsByAdvisorIdAsync(int advisorId)
        {
            // Input validation consistency
            if (advisorId <= 0)
                throw new ArgumentException("Geçersiz danýþman ID", nameof(advisorId));

            // Advisor-specific form retrieval - WHERE AdvisorId = @advisorId
            // Academic hierarchy respect ile institutional boundaries
            return await _unitOfWork.InternshipApprovals.GetApprovalFormsByAdvisorIdAsync(advisorId);
        }

        // Sistem geneli bekleyen onay formlarý - admin dashboard
        // University administrators için pending approval overview
        // Priority-based sorting ile urgent forms prioritization
        public async Task<List<InternshipApprovalForm>> GetPendingApprovalFormsAsync()
        {
            // System-wide pending forms - WHERE Status = 'Pending'
            // Admin panel bulk operations için optimized query
            return await _unitOfWork.InternshipApprovals.GetPendingApprovalFormsAsync();
        }

        // Detaylý onay formu getirme - form detail view için
        // Include operations ile full entity graph loading
        // Navigation properties (Student, Advisor, Application) with eager loading
        public async Task<InternshipApprovalForm?> GetApprovalFormWithDetailsAsync(int formId)
        {
            // Parameter validation
            if (formId <= 0)
                throw new ArgumentException("Geçersiz form ID", nameof(formId));

            // Detailed entity retrieval - comprehensive data loading
            // Include() methodology ile N+1 problem prevention
            return await _unitOfWork.InternshipApprovals.GetApprovalFormWithDetailsAsync(formId);
        }

        // Baþvuru bazlý onay formu arama - application-specific approval
        // One-to-one relationship navigation: Application -> ApprovalForm
        // Business rule: her application için max 1 approval form
        public async Task<InternshipApprovalForm?> GetApprovalFormByApplicationIdAsync(int applicationId)
        {
            // Input validation
            if (applicationId <= 0)
                throw new ArgumentException("Geçersiz baþvuru ID", nameof(applicationId));

            // Unique constraint based query - single result expected
            // WHERE ApplicationId = @applicationId LIMIT 1
            return await _unitOfWork.InternshipApprovals.GetApprovalFormByApplicationIdAsync(applicationId);
        }

        // Yeni onay formu oluþturma - core business operation
        // Multi-validation layer ile comprehensive rule enforcement
        // Academic workflow initialization ile automated advisor assignment
        public async Task<InternshipApprovalForm> CreateApprovalFormAsync(InternshipApprovalForm form)
        {
            // Null safety check
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            // Required field validations - business rule enforcement
            if (form.StudentId <= 0)
                throw new ArgumentException("Geçersiz öðrenci ID", nameof(form.StudentId));

            if (string.IsNullOrWhiteSpace(form.CompanyName))
                throw new ArgumentException("Þirket adý gerekli", nameof(form.CompanyName));

            // Date logic validation - internship period consistency
            if (form.StartDate >= form.EndDate)
                throw new ArgumentException("Baþlangýç tarihi bitiþ tarihinden önce olmalý");

            // Dual-path form creation logic
            // ApplicationId > 0: Normal application-based approval form
            // ApplicationId = 0: Direct document upload approval form
            if (form.ApplicationId > 0)
            {
                // Duplicate form prevention - business rule enforcement
                // One approval form per application constraint
                var existingForm = await _unitOfWork.InternshipApprovals.GetApprovalFormByApplicationIdAsync(form.ApplicationId);
                if (existingForm != null)
                    throw new InvalidOperationException("Bu baþvuru için zaten bir onay formu mevcut");

                // Application status validation - workflow prerequisite
                // Only approved applications can proceed to approval form stage
                var application = await _unitOfWork.InternshipApplications.GetByIdAsync(form.ApplicationId);
                if (application == null)
                    throw new InvalidOperationException("Baþvuru bulunamadý");

                if (application.Status != InternshipApplicationStatus.Approved)
                    throw new InvalidOperationException("Sadece onaylanmýþ baþvurular için onay formu oluþturulabilir");
            }
            // ApplicationId = 0 case: Direct document-based approval form
            // Skip application validation - standalone approval process

            // Automated advisor assignment - university-based matching
            // Academic hierarchy respect ile institutional boundaries
            var student = await _unitOfWork.Users.GetByIdAsync(form.StudentId);
            if (student?.UniversityId.HasValue == true)
            {
                // University-scoped advisor lookup - same institution constraint
                // Simple assignment algorithm: first available advisor
                var advisors = await _unitOfWork.Users.Where(u => 
                    u.Role == UserRole.Advisor && 
                    u.UniversityId == student.UniversityId);
                
                if (advisors.Any())
                {
                    // Automated assignment - could be enhanced with load balancing
                    form.AdvisorId = advisors.First().Id;
                }
            }

            // DateTime standardization - timezone consistency
            // UTC conversion prevents Npgsql timestamptz Kind issues
            form.StartDate = DateTime.SpecifyKind(form.StartDate, DateTimeKind.Utc);
            form.EndDate = DateTime.SpecifyKind(form.EndDate, DateTimeKind.Utc);
            
            // Default values assignment - initial state setup
            form.CreatedAt = DateTime.UtcNow;
            form.Status = InternshipApprovalStatus.Pending; // New forms always start pending

            // Database persistence - atomic transaction
            await _unitOfWork.InternshipApprovals.AddAsync(form);
            await _unitOfWork.SaveAsync();

            return form; // Return with auto-generated ID
        }

        // Onay formu güncelleme - revision workflow support
        // Change tracking ile selective field update
        // Business rule compliance ile status transition control
        public async Task UpdateApprovalFormAsync(InternshipApprovalForm form)
        {
            // Null safety check
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            // Debug logging - development/troubleshooting support
            System.Diagnostics.Debug.WriteLine($"UpdateApprovalFormAsync - Form ID: {form.Id}");
            System.Diagnostics.Debug.WriteLine($"Input Form - StudentNote: '{form.StudentNote}', Status: {form.Status}");

            // Fresh entity retrieval - change tracking setup
            // AsNoTracking problem resolution via explicit entity loading
            var existingForm = await _unitOfWork.InternshipApprovals.GetByIdAsync(form.Id);
            if (existingForm == null)
            {
                System.Diagnostics.Debug.WriteLine($"Existing form not found for ID: {form.Id}");
                throw new InvalidOperationException("Onay formu bulunamadý");
            }

            System.Diagnostics.Debug.WriteLine($"Existing Form BEFORE - StudentNote: '{existingForm.StudentNote}', Status: {existingForm.Status}");

            // Status transition validation - workflow rule enforcement
            // Only Pending and Revision status forms can be updated
            if (existingForm.Status != InternshipApprovalStatus.Pending && 
                existingForm.Status != InternshipApprovalStatus.Revision)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid status for update: {existingForm.Status}");
                throw new InvalidOperationException("Sadece beklemede olan veya revizyon durumundaki formlar güncellenebilir");
            }

            // Selective field update - change tracking optimization
            // Only update fields that are allowed to change
            existingForm.StudentNote = form.StudentNote;
            existingForm.Status = form.Status;
            existingForm.RevisionNote = form.RevisionNote;
            existingForm.RevisedAt = form.RevisedAt;
            existingForm.RejectionReason = form.RejectionReason;
            existingForm.RejectedAt = form.RejectedAt;
            
            // Conditional date updates - avoid overwriting with default values
            if (form.StartDate != DateTime.MinValue)
                existingForm.StartDate = DateTime.SpecifyKind(form.StartDate, DateTimeKind.Utc);
            if (form.EndDate != DateTime.MinValue)
                existingForm.EndDate = DateTime.SpecifyKind(form.EndDate, DateTimeKind.Utc);

            // Optional field updates - null/empty check protection
            if (!string.IsNullOrEmpty(form.CompanyName))
                existingForm.CompanyName = form.CompanyName;
            if (!string.IsNullOrEmpty(form.InternshipDescription))
                existingForm.InternshipDescription = form.InternshipDescription;
            if (form.WeeklyWorkHours > 0)
                existingForm.WeeklyWorkHours = form.WeeklyWorkHours;

            System.Diagnostics.Debug.WriteLine($"Existing Form AFTER UPDATE - StudentNote: '{existingForm.StudentNote}', Status: {existingForm.Status}");

            try 
            {
                // EF Core change tracking - automatic dirty checking
                // Entity Framework detects changes and generates appropriate SQL
                await _unitOfWork.SaveAsync();
                System.Diagnostics.Debug.WriteLine("SaveAsync completed successfully!");

                // Post-save verification - consistency check
                // Development debugging için update success confirmation
                var verifyForm = await _unitOfWork.InternshipApprovals.GetByIdAsync(form.Id);
                if (verifyForm != null)
                {
                    System.Diagnostics.Debug.WriteLine($"VERIFICATION - StudentNote: '{verifyForm.StudentNote}', Status: {verifyForm.Status}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("VERIFICATION FAILED - Form not found after update!");
                }
            }
            catch (Exception ex)
            {
                // Database error logging - detailed exception tracking
                System.Diagnostics.Debug.WriteLine($"SaveAsync ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SaveAsync STACK: {ex.StackTrace}");
                throw; // Re-throw for upper level handling
            }
        }

        // Onay formu onaylama - academic approval completion
        // Final approval step ile workflow termination
        // Cascading effects ile related entities status update
        public async Task ApproveFormAsync(int formId, string? advisorNote = null)
        {
            // Parameter validation
            if (formId <= 0)
                throw new ArgumentException("Geçersiz form ID", nameof(formId));

            // Entity existence validation
            var form = await _unitOfWork.InternshipApprovals.GetByIdAsync(formId);
            if (form == null)
                throw new InvalidOperationException("Onay formu bulunamadý");

            // Status transition validation - only pending forms can be approved
            if (form.Status != InternshipApprovalStatus.Pending)
                throw new InvalidOperationException("Sadece beklemede olan formlar onaylanabilir");

            // Status update - approval completion
            form.Status = InternshipApprovalStatus.Approved;
            form.ApprovedAt = DateTime.UtcNow; // Audit timestamp
            form.AdvisorNote = advisorNote;    // Optional advisor feedback

            // Primary form update
            _unitOfWork.InternshipApprovals.Update(form);
            await _unitOfWork.SaveAsync();

            // Cascading approval effects - related entity status propagation
            // If approval form is linked to application, update application status
            if (form.ApplicationId > 0)
            {
                try
                {
                    // Application status synchronization - workflow consistency
                    var application = await _unitOfWork.InternshipApplications.GetByIdAsync(form.ApplicationId);
                    if (application != null)
                    {
                        application.Status = InternshipApplicationStatus.Approved;
                        _unitOfWork.InternshipApplications.Update(application);
                        await _unitOfWork.SaveAsync();

                        // Automatic diary creation - post-approval workflow
                        // Create internship diary for approved application
                        var diaries = await _unitOfWork.InternshipDiaries.GetDiariesByStudentIdAsync(application.StudentId);
                        var existing = diaries.FirstOrDefault(d => d.ApplicationId.HasValue && d.ApplicationId.Value == application.Id);
                        
                        // Create diary only if not exists - idempotent operation
                        if (existing == null)
                        {
                            // Enhanced diary metadata - offer information integration
                            Core.Entity.InternshipOffer? offer = null;
                            try
                            {
                                offer = await _unitOfWork.InternshipOffers.GetByIdAsync(application.InternshipOfferId);
                            }
                            catch { /* Offer retrieval failure doesn't break diary creation */ }

                            // Diary entity construction - structured metadata
                            var diary = new Core.Entity.InternshipDiary
                            {
                                StudentId = application.StudentId,
                                Title = offer?.Title ?? "Staj Defteri",              // Dynamic or fallback title
                                CompanyName = offer?.Company?.CompanyName,           // Company context
                                Department = offer?.Field,                          // Department/field information
                                InternshipStartDate = application.StartDate,        // Period alignment
                                InternshipEndDate = application.EndDate,
                                ApplicationId = application.Id,                     // Relationship establishment
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                IsRequired = true,                                  // Mandatory diary
                                IsCompleted = false                                 // Initial incomplete state
                            };

                            // Diary persistence
                            await _unitOfWork.InternshipDiaries.AddAsync(diary);
                            await _unitOfWork.SaveAsync();

                            // Student notification - diary creation alert
                            // Inform student about new diary requirement
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
                            catch { /* Notification failure doesn't break main workflow */ }
                        }
                    }
                }
                catch
                {
                    // Exception swallowing - main approval flow protection
                    // Application/diary errors don't break primary approval operation
                    // Errors logged elsewhere in the system
                }
            }
        }

        // Onay formu reddetme - rejection workflow completion
        // Rejection reason mandatory ile transparent feedback
        // Terminal status - no further transitions possible
        public async Task RejectFormAsync(int formId, string rejectionReason, string? advisorNote = null)
        {
            // Parameter validations
            if (formId <= 0)
                throw new ArgumentException("Geçersiz form ID", nameof(formId));

            if (string.IsNullOrWhiteSpace(rejectionReason))
                throw new ArgumentException("Red nedeni gerekli", nameof(rejectionReason));

            // Entity validation
            var form = await _unitOfWork.InternshipApprovals.GetByIdAsync(formId);
            if (form == null)
                throw new InvalidOperationException("Onay formu bulunamadý");

            // Status transition validation - only pending forms can be rejected
            if (form.Status != InternshipApprovalStatus.Pending)
                throw new InvalidOperationException("Sadece beklemede olan formlar reddedilebilir");

            // Rejection completion - status and metadata update
            form.Status = InternshipApprovalStatus.Rejected;
            form.RejectedAt = DateTime.UtcNow;          // Audit timestamp
            form.RejectionReason = rejectionReason;     // Mandatory feedback
            form.AdvisorNote = advisorNote;             // Optional additional notes

            // Database persistence
            _unitOfWork.InternshipApprovals.Update(form);
            await _unitOfWork.SaveAsync();
        }

        // Revizyon talep etme - correction workflow initiation
        // Non-terminal rejection - student can revise and resubmit
        // Constructive feedback ile improvement guidance
        public async Task ReturnForRevisionAsync(int formId, string revisionNote)
        {
            // Parameter validations
            if (formId <= 0)
                throw new ArgumentException("Geçersiz form ID", nameof(formId));

            if (string.IsNullOrWhiteSpace(revisionNote))
                throw new ArgumentException("Revizyon notu gerekli", nameof(revisionNote));

            // Entity validation
            var form = await _unitOfWork.InternshipApprovals.GetByIdAsync(formId);
            if (form == null)
                throw new InvalidOperationException("Onay formu bulunamadý");

            // Status transition validation
            if (form.Status != InternshipApprovalStatus.Pending)
                throw new InvalidOperationException("Sadece beklemede olan formlar revizyona gönderilebilir");

            // Revision status assignment - enables student editing
            form.Status = InternshipApprovalStatus.Revision;
            form.RevisedAt = DateTime.UtcNow;           // Revision timestamp
            form.RevisionNote = revisionNote;          // Advisor feedback for corrections

            // Database persistence
            _unitOfWork.InternshipApprovals.Update(form);
            await _unitOfWork.SaveAsync();
        }

        // Onay formu oluþturma eligibility checker - business rule validation
        // Prevents duplicate forms ve ensures proper workflow sequence
        // Authorization ve state validation combined check
        public async Task<bool> CanCreateApprovalFormAsync(int applicationId)
        {
            // Invalid input check
            if (applicationId <= 0)
                return false;

            // Application status prerequisite - must be approved first
            var application = await _unitOfWork.InternshipApplications.GetByIdAsync(applicationId);
            if (application?.Status != InternshipApplicationStatus.Approved)
                return false; // Application not approved yet

            // Existing form check - prevent duplicates
            var existingForm = await _unitOfWork.InternshipApprovals.GetApprovalFormByApplicationIdAsync(applicationId);
            if (existingForm == null)
                return true; // No existing form - can create

            // Form exists - check if recreation is allowed
            // Business rule: rejected forms can be recreated
            // Allows student to retry after addressing rejection reasons
            if (existingForm.Status == InternshipApprovalStatus.Rejected)
                return true; // Rejected form - can recreate

            // Pending or Approved forms prevent new creation
            // Maintains single approval form per application constraint
            return false;
        }
    }
}