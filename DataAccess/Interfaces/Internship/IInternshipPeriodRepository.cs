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
        Task<InternshipPeriod?> GetActivePeriodByStudentAsync(int studentId);
        Task<int> GetCompletedDaysCountAsync(int periodId);
        Task<List<InternshipPeriod>> GetPeriodsByStudentIdAsync(int studentId);

        // New methods for many-to-many management
        Task<bool> IsStudentInPeriodAsync(int periodId, int studentId);
        Task AddStudentToPeriodAsync(int periodId, int studentId);
    }
}