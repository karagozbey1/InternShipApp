using Core.Entity;
using DataAccess.GenericRepository.Ýnternface;

namespace DataAccess.Interfaces.Internship;

public interface IInternEvaluationRepository : IRepository<InternEvaluation>
{
    Task<InternEvaluation?> GetByApplicationIdAsync(int applicationId);
    Task<List<InternEvaluation>> GetByStudentIdAsync(int studentId);
    Task<List<InternEvaluation>> GetByCompanyIdAsync(int companyId);
    Task<InternEvaluation?> GetWithDetailsAsync(int evaluationId);
    Task<bool> ExistsForApplicationAsync(int applicationId);
}
