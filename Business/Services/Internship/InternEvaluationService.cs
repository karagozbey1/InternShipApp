using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;

namespace Business.Services.Internship;

public class InternEvaluationService : IInternEvaluationService
{
    private readonly IUnitOfWork _unitOfWork;

    public InternEvaluationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<InternEvaluation?> GetByIdAsync(int id)
    {
        return await _unitOfWork.InternEvaluations.GetByIdAsync(id);
    }

    public async Task<InternEvaluation?> GetByApplicationIdAsync(int applicationId)
    {
        return await _unitOfWork.InternEvaluations.GetByApplicationIdAsync(applicationId);
    }

    public async Task<List<InternEvaluation>> GetByStudentIdAsync(int studentId)
    {
        return await _unitOfWork.InternEvaluations.GetByStudentIdAsync(studentId);
    }

    public async Task<List<InternEvaluation>> GetByCompanyIdAsync(int companyId)
    {
        return await _unitOfWork.InternEvaluations.GetByCompanyIdAsync(companyId);
    }

    public async Task<InternEvaluation?> GetWithDetailsAsync(int evaluationId)
    {
        return await _unitOfWork.InternEvaluations.GetWithDetailsAsync(evaluationId);
    }

    public async Task<bool> ExistsForApplicationAsync(int applicationId)
    {
        return await _unitOfWork.InternEvaluations.ExistsForApplicationAsync(applicationId);
    }

    public async Task<InternEvaluation> CreateEvaluationAsync(InternEvaluation evaluation)
    {
        evaluation.CreatedAt = DateTime.UtcNow;
        await _unitOfWork.InternEvaluations.AddAsync(evaluation);
        await _unitOfWork.SaveAsync();
        return evaluation;
    }

    public async Task<bool> UpdateEvaluationAsync(InternEvaluation evaluation)
    {
        evaluation.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.InternEvaluations.Update(evaluation);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<bool> MarkAsViewedByStudentAsync(int evaluationId)
    {
        var evaluation = await _unitOfWork.InternEvaluations.GetByIdAsync(evaluationId);
        if (evaluation == null) return false;

        evaluation.IsViewedByStudent = true;
        evaluation.ViewedAt = DateTime.UtcNow;
        _unitOfWork.InternEvaluations.Update(evaluation);
        await _unitOfWork.SaveAsync();
        return true;
    }
}
