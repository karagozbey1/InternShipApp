using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Internship
{
    public interface IInternshipPeriodRepository : IRepository<InternshipPeriod>
    {
        Task<List<InternshipPeriod>> GetActivePeriodsAsync();
        Task<InternshipPeriod?> GetPeriodByNameAsync(string periodName);
    }
}