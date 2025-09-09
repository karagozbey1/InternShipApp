using Core.Entity;


namespace Business.Services.Interfaces;

public interface IIUniversityService
{
    Task<IEnumerable<University>> GetAllByProvinceAsync();
    Task<IEnumerable<University>> GetAllUniversities();

    Task<IEnumerable<University>> GetByProvinceIdAsync(int provinceId);
    Task<University?> GetUniversityByNameAsync(string name);

    Task<University?> GetByIdAsync(int id);
    Task<University?> GetByUserIdAsync(int userId);
    Task AddAUniversitysync(University uni);
    Task UpdateUniversitysyncAsync(University uni);
    Task DeleteUniversitysyncAsync(int id);
}