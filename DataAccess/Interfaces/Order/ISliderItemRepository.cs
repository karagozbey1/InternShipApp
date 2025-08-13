using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Order
{
    public interface ISliderItemRepository : IRepository<SliderItem>
    {
        
        Task<List<SliderItem>> GetActiveItemsBySliderIdAsync(int sliderId);
       
        Task<List<SliderItem>> SearchItemsByTitleAsync(string keyword);
      
        // Şu anda görünür olması gereken slider öğelerini getiren metot
        Task<List<SliderItem>> GetCurrentlyDisplayedItemsAsync(int sliderId);
    }
}