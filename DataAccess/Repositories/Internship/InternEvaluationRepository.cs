using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Internship;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Internship;

public class InternEvaluationRepository : Repository<InternEvaluation>, IInternEvaluationRepository
{
    public InternEvaluationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<InternEvaluation?> GetByApplicationIdAsync(int applicationId)
    {
        return await _context.InternEvaluations
            .Include(e => e.Student)
            .Include(e => e.Company)
            .Include(e => e.EvaluatorUser)
            .FirstOrDefaultAsync(e => e.ApplicationId == applicationId);
    }

    public async Task<List<InternEvaluation>> GetByStudentIdAsync(int studentId)
    {
        return await _context.InternEvaluations
            .Include(e => e.Company)
            .Include(e => e.Application)
                .ThenInclude(a => a.InternshipOffer)
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<InternEvaluation>> GetByCompanyIdAsync(int companyId)
    {
        return await _context.InternEvaluations
            .Include(e => e.Student)
            .Include(e => e.Application)
                .ThenInclude(a => a.InternshipOffer)
            .Where(e => e.CompanyId == companyId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<InternEvaluation?> GetWithDetailsAsync(int evaluationId)
    {
        return await _context.InternEvaluations
            .Include(e => e.Student)
            .Include(e => e.Company)
            .Include(e => e.EvaluatorUser)
            .Include(e => e.Application)
                .ThenInclude(a => a.InternshipOffer)
            .FirstOrDefaultAsync(e => e.Id == evaluationId);
    }

    public async Task<bool> ExistsForApplicationAsync(int applicationId)
    {
        return await _context.InternEvaluations
            .AnyAsync(e => e.ApplicationId == applicationId);
    }
}
