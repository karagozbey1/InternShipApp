using Business.Interfaces.Order;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Order
{
    public class ISliderService : IISliderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ISliderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Slider?> GetSliderByIdAsync(int sliderId)
        {
            return await _unitOfWork.Sliders.GetByIdAsync(sliderId);
        }

        public async Task<Slider?> GetSliderByNameAsync(string name)
        {
            return await _unitOfWork.Sliders.GetSliderByNameAsync(name);
        }

        public async Task<List<Slider>> GetActiveSlidersWithItemsAsync()
        {
            var activeSliders = await _unitOfWork.Sliders.GetActiveSlidersAsync();

            // Her slider için öğelerini de yükleyelim
            foreach (var slider in activeSliders)
            {
                slider.Items = await _unitOfWork.SliderItems.GetCurrentlyDisplayedItemsAsync(slider.Id);
            }

            return activeSliders;
        }

        public async Task<Slider> AddSliderAsync(Slider slider)
        {
            await _unitOfWork.Sliders.AddAsync(slider);
            await _unitOfWork.SaveAsync();
            return slider;
        }

        public async Task UpdateSliderAsync(Slider slider)
        {
            _unitOfWork.Sliders.Update(slider);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteSliderAsync(int sliderId)
        {
            var slider = await _unitOfWork.Sliders.GetByIdAsync(sliderId);
            if (slider != null)
            {
                _unitOfWork.Sliders.Remove(slider);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}