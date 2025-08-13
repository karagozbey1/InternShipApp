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
    public class SliderItemRepository : Repository<SliderItem>, ISliderItemRepository
    {
        public SliderItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<SliderItem>> GetActiveItemsBySliderIdAsync(int sliderId)
        {
            return await _context.SliderItems
                .Where(si => si.SliderId == sliderId && si.IsActive)
                .OrderBy(si => si.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<SliderItem>> SearchItemsByTitleAsync(string keyword)
        {
            return await _context.SliderItems
                .Where(si => si.Title.Contains(keyword))
                .ToListAsync();
        }

        public async Task<List<SliderItem>> GetCurrentlyDisplayedItemsAsync(int sliderId)
        {
            var now = DateTime.UtcNow;
            return await _context.SliderItems
                .Where(si => si.SliderId == sliderId && si.IsActive &&
                             (si.StartDisplay == null || si.StartDisplay <= now) &&
                             (si.EndDisplay == null || si.EndDisplay >= now))
                .OrderBy(si => si.DisplayOrder)
                .ToListAsync();
        }
    }
}