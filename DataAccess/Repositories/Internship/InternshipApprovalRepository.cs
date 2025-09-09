using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Internship;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                .AsNoTracking() // Performans için tracking kapatýldý
                .ToListAsync();
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
                .AsNoTracking()
                .ToListAsync();
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
                .AsNoTracking()
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
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == formId);
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
                .AsNoTracking()
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
                .AsNoTracking()
                .ToListAsync();
        }
    }
}