using Business.Interfaces;
using Business.Services.Interfaces;
using Core.Entity;
using DataAccess.Unit.Interface;


namespace Business.Education
{
    public class ProvinceService : IIProvinceService
    {
        private readonly IUnitOfWork _uow;

        public ProvinceService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<Province>> GetAllAsync() =>
            await _uow.Provinces.GetAllAsync();

        public async Task<IEnumerable<Province>> GetByStartingLetterAsync(char letter) =>
            await _uow.Provinces.GetByStartingLetterAsync(letter);

        public async Task AddAsync(Province province)
        {
            await _uow.Provinces.AddAsync(province);
            await _uow.SaveAsync();
        }
    }
}
