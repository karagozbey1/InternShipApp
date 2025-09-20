using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Internship
{
    // Başvuru veri erişim arabirimi
    public interface IInternshipApplicationRepository : IRepository<InternshipApplication>
    {
        // Öğrenci başvurularını getirir
        Task<List<InternshipApplication>> GetApplicationsByStudentIdAsync(int studentId);
        
        // İlan başvurularını getirir
        Task<List<InternshipApplication>> GetApplicationsByOfferIdAsync(int offerId);
        
        // Detaylı başvuru bilgisi getirir
        Task<InternshipApplication?> GetApplicationWithDetailsAsync(int applicationId);

        // Onaylı başvuruları getirir
        Task<List<InternshipApplication>> GetApprovedByStudentIdAsync(int studentId);
        
        // Duruma göre başvuruları getirir
        Task<List<InternshipApplication>> GetApplicationsByStatusAsync(string status);
        
        // Danışman başvurularını getirir
        Task<List<InternshipApplication>> GetApplicationsByAdvisorIdAsync(int advisorId);
        
        // Öğrenci üniversitesini getirir
        Task<University?> GetStudentUniversityAsync(int studentId);
        
        // Öğrenci üniversitelerini toplu getirir
        Task<Dictionary<int, University?>> GetStudentsUniversitiesAsync(List<int> studentIds);
    }
}