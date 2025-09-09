using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;
using Microsoft.EntityFrameworkCore; 



namespace Business.Services.Internship;

public class InternshipPeriodService : IInternshipPeriodService
{
    private readonly IUnitOfWork _unitOfWork;

    public InternshipPeriodService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<InternshipPeriod?> GetPeriodByIdAsync(int periodId)
    {
        return await _unitOfWork.InternshipPeriods.GetByIdAsync(periodId);
    }

    public async Task<List<InternshipPeriod>> GetAllPeriodsAsync()
    {
        var Period = await _unitOfWork.InternshipPeriods.GetAllAsync();
        return Period.ToList();
    }

    public async Task<List<InternshipPeriod>> GetActivePeriodsAsync()
    {
        return await _unitOfWork.InternshipPeriods.GetActivePeriodsAsync();
    }

    public async Task<InternshipPeriod> AddPeriodAsync(InternshipPeriod period)
    {
        await _unitOfWork.InternshipPeriods.AddAsync(period);
        await _unitOfWork.SaveAsync();
        return period;
    }

    public async Task UpdatePeriodAsync(InternshipPeriod period)
    {
        _unitOfWork.InternshipPeriods.Update(period);
        await _unitOfWork.SaveAsync();
    }

    public async Task DeletePeriodAsync(int periodId)
    {
        var period = await _unitOfWork.InternshipPeriods.GetByIdAsync(periodId);
        if (period != null)
        {
            _unitOfWork.InternshipPeriods.Remove(period);
            await _unitOfWork.SaveAsync();
        }
    }

    public async Task<InternshipPeriod?> GetActivePeriodByStudentAsync(int studentId)
    {
        // IInternshipPeriodRepository'deki metodu çağırır
        return await _unitOfWork.InternshipPeriods.GetActivePeriodByStudentAsync(studentId);
    }

    public async Task<int> GetCompletionRateAsync(int periodId)
    {
        var period = await _unitOfWork.InternshipPeriods.GetByIdAsync(periodId);
        if (period == null) return 0;

        var completedDays = await _unitOfWork.InternshipPeriods.GetCompletedDaysCountAsync(periodId);
        var totalDays = (period.EndDate - period.StartDate).Days;
        if (totalDays <= 0) return 0;
        return (int)Math.Round((double)completedDays / totalDays * 100);
    }




    public async Task<int> GetUnreadCountByUserIdAsync(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Geçersiz kullanıcı ID", nameof(userId));

        
        return await _unitOfWork.InternshipComments.CountAsync(c => c.SenderUserId != userId && c.CommentStatus == InternshipComment.Status.Unread);
    }

    public async Task<List<InternshipPeriod>> GetStudentCalendarEventsAsync(int studentId)
    {
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        // Öğrencinin başvurduğu / devam eden staj dönemlerini getiriyoruz
        return await _unitOfWork.InternshipPeriods.GetPeriodsByStudentIdAsync(studentId);
    }
}