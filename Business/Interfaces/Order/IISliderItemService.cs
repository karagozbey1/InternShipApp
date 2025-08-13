using Core.Entity;

namespace Business.Interfaces.Order
{
    public interface IISliderItemService
    {
        Task<SliderItem?> GetSliderItemByIdAsync(int itemId);
        Task<List<SliderItem>> GetActiveItemsForSliderAsync(int sliderId);
        Task<List<SliderItem>> GetCurrentlyDisplayedItemsForSliderAsync(int sliderId);
        Task<SliderItem> AddSliderItemAsync(SliderItem item);
        Task UpdateSliderItemAsync(SliderItem item);
        Task DeleteSliderItemAsync(int itemId);
    }
}