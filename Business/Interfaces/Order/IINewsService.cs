using Core.Entity;

namespace Business.Interfaces.Order
{
    public interface IINewsService
    {
        Task<News?> GetNewsByIdAsync(int newsId);
        Task<List<News>> GetAllNewsAsync();
        Task<List<News>> GetRecentNewsAsync(int count);
        Task<List<News>> GetNewsByCategoryAsync(string category);
        Task<List<News>> SearchNewsAsync(string keyword);
        Task<News> AddNewsAsync(News news);
        Task UpdateNewsAsync(News news);
        Task DeleteNewsAsync(int newsId);
    }
}