using Business.Interfaces.Order;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Order
{
    public class ISliderItemService : IISliderItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ISliderItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SliderItem?> GetSliderItemByIdAsync(int itemId)
        {
            return await _unitOfWork.SliderItems.GetByIdAsync(itemId);
        }

        public async Task<List<SliderItem>> GetActiveItemsForSliderAsync(int sliderId)
        {
            return await _unitOfWork.SliderItems.GetActiveItemsBySliderIdAsync(sliderId);
        }

        public async Task<List<SliderItem>> GetCurrentlyDisplayedItemsForSliderAsync(int sliderId)
        {
            return await _unitOfWork.SliderItems.GetCurrentlyDisplayedItemsAsync(sliderId);
        }

        public async Task<SliderItem> AddSliderItemAsync(SliderItem item)
        {
            // İş mantığı: Yeni slider öğesi eklenmeden önce bağlı olduğu slider'ın varlığı kontrol edilebilir.
            var slider = await _unitOfWork.Sliders.GetByIdAsync(item.SliderId);
            if (slider == null)
            {
                throw new InvalidOperationException("Slider not found for this item.");
            }
            await _unitOfWork.SliderItems.AddAsync(item);
            await _unitOfWork.SaveAsync();
            return item;
        }

        public async Task UpdateSliderItemAsync(SliderItem item)
        {
            _unitOfWork.SliderItems.Update(item);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteSliderItemAsync(int itemId)
        {
            var item = await _unitOfWork.SliderItems.GetByIdAsync(itemId);
            if (item != null)
            {
                _unitOfWork.SliderItems.Remove(item);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}