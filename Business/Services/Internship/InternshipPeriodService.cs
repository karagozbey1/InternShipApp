using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Internship
{
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
    }
}