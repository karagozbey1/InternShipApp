using Business.Interfaces.Order;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Order
{
    public class INewsService : IINewsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public INewsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<News?> GetNewsByIdAsync(int newsId)
        {
            return await _unitOfWork.News.GetByIdAsync(newsId);
        }

        public async Task<List<News>> GetAllNewsAsync()
        {
            var News= await _unitOfWork.News.GetAllAsync();
            return News.ToList();
        }

        public async Task<List<News>> GetRecentNewsAsync(int count)
        {
            return await _unitOfWork.News.GetRecentNewsAsync(count);
        }

        public async Task<List<News>> GetNewsByCategoryAsync(string category)
        {
            return await _unitOfWork.News.GetNewsByCategoryAsync(category);
        }

        public async Task<List<News>> SearchNewsAsync(string keyword)
        {
            return await _unitOfWork.News.SearchNewsAsync(keyword);
        }

        public async Task<News> AddNewsAsync(News news)
        {
            // İş mantığı: Haber ekleme öncesi başlık ve içerik doğrulama gibi kontroller eklenebilir.
            news.PublishDate = DateTime.UtcNow;
            await _unitOfWork.News.AddAsync(news);
            await _unitOfWork.SaveAsync();
            return news;
        }

        public async Task UpdateNewsAsync(News news)
        {
            _unitOfWork.News.Update(news);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteNewsAsync(int newsId)
        {
            var news = await _unitOfWork.News.GetByIdAsync(newsId);
            if (news != null)
            {
                _unitOfWork.News.Remove(news);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}