using Core.Entity;
using DataAccess.GenericRepository.İnternface;

namespace DataAccess.Interfaces.Education;

public interface IUniversityRepository : IRepository<University>
{
    Task<University?> GetUniversityByNameAsync(string name);
    Task<IEnumerable<University>> GetAllWithProvinceAsync();
    Task<IEnumerable<University>> GetByProvinceIdAsync(int provinceId);
    Task<University?> GetByUserIdAsync(int userId);
}