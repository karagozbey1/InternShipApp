using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Internship
{
    public interface IInternshipApplicationRepository : IRepository<InternshipApplication>
    {
        // belirli bir öğrenciye ait tüm staj başvurularını getirir.
        Task<List<InternshipApplication>> GetApplicationsByStudentIdAsync(int studentId);
        
        Task<List<InternshipApplication>> GetApplicationsByOfferIdAsync(int offerId);
        Task<InternshipApplication?> GetApplicationWithDetailsAsync(int applicationId);
        
        // Belirli bir duruma(status) sahip tüm staj başvurularını getirir.Örneğin, "Pending", "Approved", "Rejected" gibi durumlar olabilir.
        Task<List<InternshipApplication>> GetApplicationsByStatusAsync(string status);
        Task<List<InternshipApplication>> GetApplicationsByAdvisorIdAsync(int advisorId);
    }
}