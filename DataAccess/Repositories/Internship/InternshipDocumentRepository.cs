using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Internship;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Internship
{
    public class InternshipDocumentRepository : Repository<InternshipDocument>, IInternshipDocumentRepository
    {

        public InternshipDocumentRepository(ApplicationDbContext context) : base(context)
        {
           
        }

        public async Task<List<InternshipDocument>> GetDocumentsByApplicationIdAsync(int applicationId)
        {
            return await _context.InternshipDocuments
                                 .Where(d => d.ApplicationId == applicationId)
                                 .ToListAsync();
        }
    }
}