using Core.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Interfaces.Order
{
    public interface IISliderService
    {
        Task<Slider?> GetSliderByIdAsync(int sliderId);
        Task<Slider?> GetSliderByNameAsync(string name);
        Task<List<Slider>> GetActiveSlidersWithItemsAsync();
        Task<Slider> AddSliderAsync(Slider slider);
        Task UpdateSliderAsync(Slider slider);
        Task DeleteSliderAsync(int sliderId);
    }
}