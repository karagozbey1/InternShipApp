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
    public class InternshipCommentRepository : Repository<InternshipComment>, IInternshipCommentRepository
    {

        public InternshipCommentRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task AddCommentAsync(InternshipComment comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            await _context.InternshipComments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<InternshipComment>> GetCommentsByApplicationIdAsync(int applicationId)
        {
            return await _context.InternshipComments
                                 .Where(c => c.ApplicationId == applicationId)
                                 .OrderBy(c => c.SentAt)
                                 .ToListAsync();
        }
    }
}