using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Internship
{
    public interface IInternshipApplicationRepository : IRepository<InternshipApplication>
    {
        Task<List<InternshipApplication>> GetApplicationsByStudentIdAsync(int studentId);
        
        Task<List<InternshipApplication>> GetApplicationsByOfferIdAsync(int offerId);
        
        Task<InternshipApplication?> GetApplicationWithDetailsAsync(int applicationId);

        Task<List<InternshipApplication>> GetApprovedByStudentIdAsync(int studentId);
        
        Task<List<InternshipApplication>> GetApplicationsByStatusAsync(string status);
        
        Task<List<InternshipApplication>> GetApplicationsByAdvisorIdAsync(int advisorId);
        
        // University bilgilerini almak için yardımcı metodlar (PostgreSQL uyumluluğu için)
        Task<University?> GetStudentUniversityAsync(int studentId);
        
        Task<Dictionary<int, University?>> GetStudentsUniversitiesAsync(List<int> studentIds);
    }
}