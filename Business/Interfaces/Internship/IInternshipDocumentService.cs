using Core.Entity;

namespace Business.Interfaces.Internship
{
    public interface IInternshipDocumentService
    {
        Task<List<InternshipDocument>> GetCompanyDocumentsAsync(int companyId);
        Task AddCompanyDocumentAsync(int companyId, string fileName, string description);
        Task<List<InternshipDocument>> GetStudentDiaryEntriesAsync(int studentId);
        Task<bool> CreateDiaryEntryAsync(InternshipDocument entry);
        Task<InternshipDocument?> GetDocumentByIdAsync(int documentId);
        Task<List<InternshipDocument>> GetDocumentsByApplicationIdAsync(int applicationId);
        Task<InternshipDocument> AddDocumentAsync(InternshipDocument document);
        Task UpdateDocumentAsync(InternshipDocument document);
        Task DeleteDocumentAsync(int documentId);
        Task<List<InternshipDocument>> GetDiaryEntriesByStudentIdAsync(int studentId);
        Task<List<InternshipDocument>> GetDocumentsByStudentIdAsync(int studentId);
        Task<List<InternshipDocument>> GetIndependentDocumentsByStudentIdAsync(int studentId);
        Task LinkDocumentToApprovalAsync(int documentId, int approvalFormId);
        Task<List<InternshipDocument>> GetDocumentsByApprovalFormIdAsync(int approvalFormId);
        Task LinkDocumentToApplicationAsync(int documentId, int applicationId);
        Task UnlinkDocumentFromApplicationAsync(int documentId, int applicationId);
    }
}