    using Business.Interfaces;
    using Business.Services.Interfaces;
    using Core.Entity;
    using DataAccess.Unit.Interface;


    namespace Business.Education
    {
        public class UniversityService : IIUniversityService
        {
            private readonly IUnitOfWork _uow;

            public UniversityService(IUnitOfWork uow)
            {
                _uow = uow;
            }

            public async Task<IEnumerable<University>> GetAllByProvinceAsync() =>
                await _uow.Universities.GetAllWithProvinceAsync();

            public async Task<IEnumerable<University>> GetAllUniversities()
            {
                return await _uow.Universities.GetAllAsync();
            }

            public async Task<IEnumerable<University>> GetByProvinceIdAsync(int provinceId) =>
                await _uow.Universities.GetByProvinceIdAsync(provinceId);

            public async Task<University?> GetByIdAsync(int id) =>
                await _uow.Universities.GetByIdAsync(id);

            public async Task<University?> GetByUserIdAsync(int userId) =>
                await _uow.Universities.GetByUserIdAsync(userId);

            public async Task AddAUniversitysync(University uni)
            {
                await _uow.Universities.AddAsync(uni);
                await _uow.SaveAsync();
            }

            public async Task UpdateUniversitysyncAsync(University uni)
            {
                _uow.Universities.Update(uni);
                await _uow.SaveAsync();
            }

            public async Task DeleteUniversitysyncAsync(int id)
            {
                var e = await _uow.Universities.GetByIdAsync(id);
                if (e != null)
                {
                    _uow.Universities.Remove(e);
                    await _uow.SaveAsync();
                }
            }

            public async Task<University?> GetUniversityByNameAsync(string name)
            {
              return await _uow.Universities.GetUniversityByNameAsync(name);
            }

      
        }
    }
