using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Internship;
using Microsoft.EntityFrameworkCore;


namespace DataAccess.Repositories.Internship
{
    public class InternshipApprovalRepository : Repository<InternshipApprovalForm>, IInternshipApprovalRepository
    {
        public InternshipApprovalRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<InternshipApprovalForm>> GetApprovalFormsByStudentIdAsync(int studentId)
        {
            if (studentId <= 0)
                return new List<InternshipApprovalForm>();

            return await _dbSet
                .Include(f => f.Application)
                    .ThenInclude(a => a!.InternshipOffer)
                        .ThenInclude(o => o!.Company)
                .Include(f => f.Student)
                .Include(f => f.Advisor)
                .Where(f => f.StudentId == studentId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync(); // AsNoTracking kald?r?ld? - fresh data i?in
        }

        public async Task<List<InternshipApprovalForm>> GetApprovalFormsByAdvisorIdAsync(int advisorId)
        {
            if (advisorId <= 0)
                return new List<InternshipApprovalForm>();

            return await _dbSet
                .Include(f => f.Application)
                    .ThenInclude(a => a!.InternshipOffer)
                        .ThenInclude(o => o!.Company)
                .Include(f => f.Student)
                .Include(f => f.Advisor)
                .Where(f => f.AdvisorId == advisorId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync(); // AsNoTracking kald?r?ld?
        }

        public async Task<List<InternshipApprovalForm>> GetApprovalFormsByStatusAsync(InternshipApprovalStatus status)
        {
            return await _dbSet
                .Include(f => f.Application)
                    .ThenInclude(a => a!.InternshipOffer)
                        .ThenInclude(o => o!.Company)
                .Include(f => f.Student)
                .Include(f => f.Advisor)
                .Where(f => f.Status == status)
                .OrderByDescending(f => f.CreatedAt)
                .AsNoTracking() // Sadece okuma i?in AsNoTracking kals?n
                .ToListAsync();
        }

        public async Task<InternshipApprovalForm?> GetApprovalFormWithDetailsAsync(int formId)
        {
            if (formId <= 0)
                return null;

            return await _dbSet
                .Include(f => f.Application)
                    .ThenInclude(a => a!.InternshipOffer)
                        .ThenInclude(o => o!.Company)
                .Include(f => f.Student)
                .Include(f => f.Advisor)
                .FirstOrDefaultAsync(f => f.Id == formId); // AsNoTracking kald?r?ld? - fresh data i?in
        }

        public async Task<InternshipApprovalForm?> GetApprovalFormByApplicationIdAsync(int applicationId)
        {
            if (applicationId <= 0)
                return null;

            return await _dbSet
                .Include(f => f.Application)
                    .ThenInclude(a => a!.InternshipOffer)
                        .ThenInclude(o => o!.Company)
                .Include(f => f.Student)
                .Include(f => f.Advisor)
                .AsNoTracking() // Sadece kontrol i?in AsNoTracking kals?n
                .FirstOrDefaultAsync(f => f.ApplicationId == applicationId);
        }

        public async Task<List<InternshipApprovalForm>> GetPendingApprovalFormsAsync()
        {
            return await _dbSet
                .Include(f => f.Application)
                    .ThenInclude(a => a!.InternshipOffer)
                        .ThenInclude(o => o!.Company)
                .Include(f => f.Student)
                .Include(f => f.Advisor)
                .Where(f => f.Status == InternshipApprovalStatus.Pending)
                .OrderByDescending(f => f.CreatedAt)
                .AsNoTracking() // Sadece okuma i?in AsNoTracking kals?n
                .ToListAsync();
        }
    }
}   