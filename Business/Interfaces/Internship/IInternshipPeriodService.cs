using Core.Entity;

namespace Business.Interfaces.Internship
{
    public interface IInternshipPeriodService
    {
        Task<InternshipPeriod?> GetPeriodByIdAsync(int periodId);
        Task<List<InternshipPeriod>> GetAllPeriodsAsync();
        Task<List<InternshipPeriod>> GetActivePeriodsAsync();
        Task<InternshipPeriod> AddPeriodAsync(InternshipPeriod period);
        Task UpdatePeriodAsync(InternshipPeriod period);
        Task DeletePeriodAsync(int periodId);
    }
}