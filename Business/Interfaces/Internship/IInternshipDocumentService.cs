using Core.Entity;

namespace Business.Interfaces.Internship
{
    public interface IInternshipDocumentService
    {
        Task<InternshipDocument?> GetDocumentByIdAsync(int documentId);
        Task<List<InternshipDocument>> GetDocumentsByApplicationIdAsync(int applicationId);
        Task<InternshipDocument> AddDocumentAsync(InternshipDocument document);
        Task DeleteDocumentAsync(int documentId);
    }
}