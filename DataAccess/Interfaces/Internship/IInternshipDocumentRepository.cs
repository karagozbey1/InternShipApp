using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Internship
{
    public interface IInternshipDocumentRepository : IRepository<InternshipDocument>
    {
        Task<List<InternshipDocument>> GetDocumentsByApplicationIdAsync(int applicationId);
    }
}