using Core.Entity;

namespace Business.Interfaces.Internship
{
    public interface IInternshipCommentService
    {
        Task<InternshipComment?> GetCommentByIdAsync(int commentId);
        Task<List<InternshipComment>> GetCommentsByApplicationIdAsync(int applicationId);
        Task<InternshipComment> AddCommentAsync(InternshipComment comment);
        Task DeleteCommentAsync(int commentId);
    }
}