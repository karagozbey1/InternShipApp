using Core.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Interfaces;

public interface IIProvinceService
{

    Task<IEnumerable<Province>> GetAllAsync();
    Task<IEnumerable<Province>> GetByStartingLetterAsync(char letter);
    Task AddAsync(Province province);

}