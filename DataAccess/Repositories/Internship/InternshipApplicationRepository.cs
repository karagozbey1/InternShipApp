using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Internship;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Internship
{
    public class InternshipApplicationRepository : Repository<InternshipApplication>, IInternshipApplicationRepository
    {
        public InternshipApplicationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<InternshipApplication>> GetApplicationsByStudentIdAsync(int studentId)
        {
            if (studentId <= 0)
                throw new ArgumentException("Geçersiz öğrenci kimliği", nameof(studentId));

            try
            {
                Console.WriteLine($"[DEBUG] GetApplicationsByStudentIdAsync called for studentId: {studentId}");
                
                // Test basic query first without includes
                var basicApplications = await _context.InternshipApplications
                    .Where(app => app.StudentId == studentId)
                    .ToListAsync();
                
                Console.WriteLine($"[DEBUG] Found {basicApplications.Count} basic applications");

                // Then try with just InternshipOffer include
                var withOfferApplications = await _context.InternshipApplications
                    .Where(app => app.StudentId == studentId)
                    .Include(a => a.InternshipOffer)
                        .ThenInclude(o => o.Company)
                    .ToListAsync();
                
                Console.WriteLine($"[DEBUG] Found {withOfferApplications.Count} applications with offer");

                // Finally try with Student include but without University
                var withStudentApplications = await _context.InternshipApplications
                    .Where(app => app.StudentId == studentId)
                    .Include(a => a.InternshipOffer)
                        .ThenInclude(o => o.Company)
                    .Include(a => a.Student)
                    .ToListAsync();
                
                Console.WriteLine($"[DEBUG] Found {withStudentApplications.Count} applications with student (no university)");

                // Test University access separately
                try
                {
                    Console.WriteLine("[DEBUG] Testing university table access...");
                    var universityCount = await _context.Universities.CountAsync();
                    Console.WriteLine($"[DEBUG] University count: {universityCount}");
                    
                    // Now try the full query with University include
                    Console.WriteLine("[DEBUG] Attempting full query with University include...");
                    return await _context.InternshipApplications
                        .Where(app => app.StudentId == studentId)
                        .Include(a => a.InternshipOffer)
                            .ThenInclude(o => o.Company)
                        .Include(a => a.Student)
                            .ThenInclude(s => s.University)
                        .ToListAsync();
                }
                catch (Exception universityEx)
                {
                    Console.WriteLine($"[ERROR] University access failed: {universityEx.Message}");
                    Console.WriteLine($"[ERROR] Stack trace: {universityEx.StackTrace}");
                    
                    // Fallback: return without University include
                    return withStudentApplications;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetApplicationsByStudentIdAsync failed: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // University bilgisini ayrı olarak getiren yardımcı metod - Interface requirement
        public async Task<University?> GetStudentUniversityAsync(int studentId)
        {
            try
            {
                var student = await _context.Users
                    .Include(u => u.University)
                    .FirstOrDefaultAsync(u => u.Id == studentId);

                return student?.University;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetStudentUniversityAsync failed: {ex.Message}");
                return null;
            }
        }

        // University bilgisini ayrı olarak getiren yardımcı metod - çoklu student - Interface requirement
        public async Task<Dictionary<int, University?>> GetStudentsUniversitiesAsync(List<int> studentIds)
        {
            try
            {
                var students = await _context.Users
                    .Where(u => studentIds.Contains(u.Id))
                    .Include(u => u.University)
                    .Select(u => new { u.Id, u.University })
                    .ToListAsync();

                return students.ToDictionary(s => s.Id, s => s.University);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetStudentsUniversitiesAsync failed: {ex.Message}");
                return new Dictionary<int, University?>();
            }
        }

        public async Task<List<InternshipApplication>> GetApplicationsByOfferIdAsync(int offerId)
        {
            if (offerId <= 0)
                throw new ArgumentException("Geçersiz teklif kimliği", nameof(offerId));

            try
            {
                Console.WriteLine($"[DEBUG] GetApplicationsByOfferIdAsync called for offerId: {offerId}");
                
                // Try without University include first
                var withoutUniversity = await _context.InternshipApplications
                    .Where(app => app.InternshipOfferId == offerId && app.Status != InternshipApplicationStatus.Rejected)
                    .Include(a => a.Student)
                    .Include(a => a.InternshipOffer)
                        .ThenInclude(o => o.Company)
                    .ToListAsync();
                
                Console.WriteLine($"[DEBUG] Found {withoutUniversity.Count} applications without university");

                // Try with University include
                return await _context.InternshipApplications
                    .Where(app => app.InternshipOfferId == offerId && app.Status != InternshipApplicationStatus.Rejected)
                    .Include(a => a.Student)
                        .ThenInclude(s => s.University)
                    .Include(a => a.InternshipOffer)
                        .ThenInclude(o => o.Company)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetApplicationsByOfferIdAsync failed: {ex.Message}");
                
                // Fallback: return without University include
                return await _context.InternshipApplications
                    .Where(app => app.InternshipOfferId == offerId && app.Status != InternshipApplicationStatus.Rejected)
                    .Include(a => a.Student)
                    .Include(a => a.InternshipOffer)
                        .ThenInclude(o => o.Company)
                    .ToListAsync();
            }
        }

        public async Task<InternshipApplication?> GetApplicationWithDetailsAsync(int applicationId)
        {
            if (applicationId <= 0)
                throw new ArgumentException("Geçersiz başvuru kimliği", nameof(applicationId));

            try
            {
                return await _context.InternshipApplications
                    .Where(a => a.Id == applicationId)
                    .Include(a => a.Student)
                        .ThenInclude(s => s.University)
                    .Include(a => a.InternshipOffer)
                        .ThenInclude(o => o.Company)
                    .Include(a => a.Advisor)
                    .Include(a => a.InternshipDocuments)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetApplicationWithDetailsAsync failed: {ex.Message}");
                
                // Fallback: return without University include
                return await _context.InternshipApplications
                    .Where(a => a.Id == applicationId)
                    .Include(a => a.Student)
                    .Include(a => a.InternshipOffer)
                        .ThenInclude(o => o.Company)
                    .Include(a => a.Advisor)
                    .Include(a => a.InternshipDocuments)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<List<InternshipApplication>> GetApplicationsByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Geçersiz durum", nameof(status));

            if (!Enum.TryParse<InternshipApplicationStatus>(status, true, out var parsedStatus))
                throw new ArgumentException("Geçersiz başvuru durumu", nameof(status));

            var statusValue = (int)parsedStatus;

            return await _context.InternshipApplications
                .Where(app => (int)app.Status == statusValue)
                .ToListAsync();
        }

        public async Task<List<InternshipApplication>> GetApplicationsByAdvisorIdAsync(int advisorId)
        {
            if (advisorId <= 0)
                throw new ArgumentException("Geçersiz danışman kimliği", nameof(advisorId));

            return await _context.InternshipApplications
                .Where(a => a.AdvisorId == advisorId)
                .ToListAsync();
        }
        
        public async Task<List<InternshipApplication>> GetApprovedByStudentIdAsync(int studentId)
        {
            return await _context.InternshipApplications
                .Where(x => x.StudentId == studentId && x.Status == InternshipApplicationStatus.Approved)
                .Include(x => x.InternshipOffer)
                    .ThenInclude(o => o.Company)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
