using Core.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Interfaces.Internship
{
    public interface IInternshipApprovalService
    {
        Task<List<InternshipApprovalForm>> GetApprovalFormsByStudentIdAsync(int studentId);
        Task<List<InternshipApprovalForm>> GetApprovalFormsByAdvisorIdAsync(int advisorId);
        Task<List<InternshipApprovalForm>> GetPendingApprovalFormsAsync();
        Task<InternshipApprovalForm?> GetApprovalFormWithDetailsAsync(int formId);
        Task<InternshipApprovalForm?> GetApprovalFormByApplicationIdAsync(int applicationId);
        Task<InternshipApprovalForm> CreateApprovalFormAsync(InternshipApprovalForm form);
        Task UpdateApprovalFormAsync(InternshipApprovalForm form);
        Task DeleteApprovalFormAsync(int formId);
        Task ApproveFormAsync(int formId, string? advisorNote = null);
        Task RejectFormAsync(int formId, string rejectionReason, string? advisorNote = null);
        Task ReturnForRevisionAsync(int formId, string revisionNote);
        Task<bool> CanCreateApprovalFormAsync(int applicationId);
    }
}