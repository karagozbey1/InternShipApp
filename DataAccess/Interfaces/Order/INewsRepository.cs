using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Order
{
    public interface INewsRepository : IRepository<News>
    {
        // En son yayınlanan haberleri getiren metot
        Task<List<News>> GetRecentNewsAsync(int count);

        Task<List<News>> GetNewsByCategoryAsync(string category);

        Task<List<News>> SearchNewsAsync(string keyword);
        Task<List<News>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}