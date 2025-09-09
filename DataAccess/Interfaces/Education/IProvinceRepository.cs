using Core.Entity;
using DataAccess.GenericRepository.İnternface;

namespace DataAccess.Interfaces.Education;

public interface IProvinceRepository : IRepository<Province>
{
    Task<IEnumerable<Province>> GetByStartingLetterAsync(char letter);
}