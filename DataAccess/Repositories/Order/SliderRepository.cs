using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Order;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Order
{
    public class SliderRepository : Repository<Slider>, ISliderRepository
    {
        public SliderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Slider>> GetActiveSlidersAsync()
        {
            return await _context.Sliders
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<Slider?> GetSliderWithItemsAsync(int sliderId)
        {
            return await _context.Sliders
                .Include(s => s.Items!.Where(item => item.IsActive)
                    .OrderBy(item => item.DisplayOrder))
                .FirstOrDefaultAsync(s => s.Id == sliderId);
        }

        public async Task<Slider?> GetSliderByNameAsync(string name)
        {
            return await _context.Sliders
                .FirstOrDefaultAsync(s => s.Name == name);
        }
    }
}