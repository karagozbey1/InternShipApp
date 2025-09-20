using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Internship
{
    // Yorum veri erişim arabirimi
    public interface IInternshipCommentRepository : IRepository<InternshipComment>
    {
        // Başvuru yorumlarını getirir
        Task<List<InternshipComment>> GetCommentsByApplicationIdAsync(int applicationId);
        
        // Yorum ekler
        Task AddCommentAsync(InternshipComment comment);
    }
}