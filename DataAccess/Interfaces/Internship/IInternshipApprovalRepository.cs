using Core.Entity;
using DataAccess.GenericRepository.Ýnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Internship
{
    public interface IInternshipApprovalRepository : IRepository<InternshipApprovalForm>
    {
        Task<List<InternshipApprovalForm>> GetApprovalFormsByStudentIdAsync(int studentId);
        Task<List<InternshipApprovalForm>> GetApprovalFormsByAdvisorIdAsync(int advisorId);
        Task<List<InternshipApprovalForm>> GetApprovalFormsByStatusAsync(InternshipApprovalStatus status);
        Task<InternshipApprovalForm?> GetApprovalFormWithDetailsAsync(int formId);
        Task<InternshipApprovalForm?> GetApprovalFormByApplicationIdAsync(int applicationId);
        Task<List<InternshipApprovalForm>> GetPendingApprovalFormsAsync();
    }
}