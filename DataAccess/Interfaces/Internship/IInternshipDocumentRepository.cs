using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Internship
{
    public interface IInternshipDocumentRepository : IRepository<InternshipDocument>
    {
        Task<List<InternshipDocument>> GetCompanyDocumentsAsync(int companyId);
        Task<List<InternshipDocument>> GetDiaryEntriesByStudentIdAsync(int studentId);
        Task<List<InternshipDocument>> GetDocumentsByApplicationIdAsync(int applicationId);
        Task<List<InternshipDocument>> GetDocumentsByStudentIdAsync(int studentId);
        Task<List<InternshipDocument>> GetIndependentDocumentsByStudentIdAsync(int studentId);
        Task LinkDocumentToApprovalAsync(int documentId, int approvalFormId);
        Task<List<InternshipDocument>> GetDocumentsByApprovalFormIdAsync(int approvalFormId);
    }
}