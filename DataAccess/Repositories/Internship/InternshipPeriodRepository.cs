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
    public class InternshipPeriodRepository : Repository<InternshipPeriod>, IInternshipPeriodRepository
    {
   

        public InternshipPeriodRepository(ApplicationDbContext context) : base(context)
        {
         
        }

        public async Task<List<InternshipPeriod>> GetActivePeriodsAsync()
        {
            return await _context.InternshipPeriods
                                 .Where(p => p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                                 .ToListAsync();
        }

        public async Task<InternshipPeriod?> GetPeriodByNameAsync(string periodName)
        {
            return await _context.InternshipPeriods
                                 .FirstOrDefaultAsync(p => p.PeriodName == periodName);
        }
    }
}