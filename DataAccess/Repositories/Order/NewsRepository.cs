using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Order;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Order
{
    public class NewsRepository : Repository<News>, INewsRepository
    {
      

        public NewsRepository(ApplicationDbContext context) : base(context)
        {
          
        }

        public async Task<List<News>> GetRecentNewsAsync(int count)
        {
            return await _context.News
                .OrderByDescending(n => n.PublishDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<News>> GetNewsByCategoryAsync(string category)
        {
            return await _context.News
                .Where(n => n.Category == category)
                .OrderByDescending(n => n.PublishDate)
                .ToListAsync();
        }

        public async Task<List<News>> SearchNewsAsync(string keyword)
        {
            return await _context.News
                .Where(n => n.Title.Contains(keyword) || n.Content.Contains(keyword))
                .OrderByDescending(n => n.PublishDate)
                .ToListAsync();
        }

        public async Task<List<News>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.News
                .Where(n => n.PublishDate >= startDate && n.PublishDate <= endDate)
                .OrderByDescending(n => n.PublishDate)
                .ToListAsync();
        }
    }
}