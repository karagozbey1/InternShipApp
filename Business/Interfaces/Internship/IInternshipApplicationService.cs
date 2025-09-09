using Core.Entity;

namespace Business.Interfaces.Internship
{
    public interface IInternshipApplicationService
    {
        Task<List<InternshipApplication>> GetApplicationsByStudentIdAsync(int studentId);
        Task<List<InternshipApplication>> GetApplicationsByOfferIdAsync(int offerId);
        Task<InternshipApplication?> GetApplicationWithDetailsAsync(int applicationId);
        Task<List<InternshipApplication>> GetApplicationsByStatusAsync(string status);
        Task<InternshipApplication> AddApplicationAsync(InternshipApplication application);
        Task UpdateApplicationStatusAsync(int applicationId, string newStatus, int actingUserId, string actingUserRole);
        Task AddCommentAsync(InternshipComment comment);
        Task<List<InternshipApplication>> GetStudentApplicationsAsync(int studentId);
        Task<bool> WithdrawApplicationAsync(int applicationId);
        Task<InternshipApplication> GetByIdAsync(int applicationId);
        Task<List<InternshipApplication>> GetApprovedByStudentIdAsync(int studentId);
        Task DeleteApplicationAsync(int applicationId);
    }
}