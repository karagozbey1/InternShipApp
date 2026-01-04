using Core.Entity;

namespace Business.Interfaces.Internship;

public interface IInternEvaluationService
{
    Task<InternEvaluation?> GetByIdAsync(int id);
    Task<InternEvaluation?> GetByApplicationIdAsync(int applicationId);
    Task<List<InternEvaluation>> GetByStudentIdAsync(int studentId);
    Task<List<InternEvaluation>> GetByCompanyIdAsync(int companyId);
    Task<InternEvaluation?> GetWithDetailsAsync(int evaluationId);
    Task<bool> ExistsForApplicationAsync(int applicationId);
    Task<InternEvaluation> CreateEvaluationAsync(InternEvaluation evaluation);
    Task<bool> UpdateEvaluationAsync(InternEvaluation evaluation);
    Task<bool> MarkAsViewedByStudentAsync(int evaluationId);
}
