using Core.Entity;

namespace Business.Interfaces.Internship
{
    public interface IInternshipPeriodService
    {
        Task<InternshipPeriod?> GetActivePeriodByStudentAsync(int studentId);
        Task<int> GetUnreadCountByUserIdAsync(int userId);
        Task<List<InternshipPeriod>> GetStudentCalendarEventsAsync(int studentId);
        Task<InternshipPeriod?> GetPeriodByIdAsync(int periodId);
        Task<List<InternshipPeriod>> GetAllPeriodsAsync();
        Task<List<InternshipPeriod>> GetActivePeriodsAsync();
        Task<InternshipPeriod> AddPeriodAsync(InternshipPeriod period);
        Task<int> GetCompletionRateAsync(int periodId);
        Task UpdatePeriodAsync(InternshipPeriod period);
        Task DeletePeriodAsync(int periodId);
    }
}