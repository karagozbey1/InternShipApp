using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Order
{
    public interface ISliderRepository : IRepository<Slider>
    {
        
        Task<List<Slider>> GetActiveSlidersAsync();

        // Bir slider'ı, tüm öğeleri ile birlikte getirir.
        Task<Slider?> GetSliderWithItemsAsync(int sliderId);
       
        Task<Slider?> GetSliderByNameAsync(string name);
    }
}