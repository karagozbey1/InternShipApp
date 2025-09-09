using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Internship
{
    public interface IInternshipCommentRepository : IRepository<InternshipComment>
    {
        Task<List<InternshipComment>> GetCommentsByApplicationIdAsync(int applicationId);
        Task AddCommentAsync(InternshipComment comment);
      
    }
}