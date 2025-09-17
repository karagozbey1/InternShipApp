using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Internship;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Internship
{
    public class OfferFavoriteRepository : Repository<OfferFavorite>, IOfferFavoriteRepository
    {
        public OfferFavoriteRepository(ApplicationDbContext context) : base(context) {}

        public Task<OfferFavorite?> GetByUserAndOfferAsync(int studentId, int offerId)
        {
            return _context.Set<OfferFavorite>().FirstOrDefaultAsync(f => f.StudentId == studentId && f.OfferId == offerId);
        }

        public Task<List<OfferFavorite>> GetByUserAsync(int studentId)
        {
            return _context.Set<OfferFavorite>()
                            .Include(f => f.Offer)!.ThenInclude(o => o!.Company)
                            .Where(f => f.StudentId == studentId)
                            .OrderByDescending(f => f.CreatedAt)
                            .ToListAsync();
        }

        public async Task RemoveByUserAndOfferAsync(int studentId, int offerId)
        {
            var fav = await GetByUserAndOfferAsync(studentId, offerId);
            if (fav != null) { _context.Set<OfferFavorite>().Remove(fav); }
        }
    }
}
