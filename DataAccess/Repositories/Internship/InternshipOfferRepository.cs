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
    public class InternshipOfferRepository : Repository<InternshipOffer>, IInternshipOfferRepository
    {
        

        public InternshipOfferRepository(ApplicationDbContext context) : base(context)
        {
          
        }

        public async Task<List<InternshipOffer>> GetAllActiveOffersAsync()
        {
            return await _context.InternshipOffers
                .Include(o => o.Company)
                .Where(o => o.IsActive && (o.ApplicationDeadline == null || o.ApplicationDeadline >= DateTime.UtcNow))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<InternshipOffer?> GetOfferWithCompanyAsync(int offerId)
        {
            return await _context.InternshipOffers
                .Include(o => o.Company)
                .FirstOrDefaultAsync(o => o.Id == offerId);
        }

        public async Task<List<InternshipOffer>> GetOffersByCompanyIdAsync(int companyId)
        {
            return await _context.InternshipOffers
                .Where(o => o.CompanyId == companyId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<InternshipOffer>> SearchOffersAsync(string keyword, string? city, string? field)
        {
            var query = _context.InternshipOffers
                .Include(o => o.Company)
                .Where(o => o.IsActive && (o.ApplicationDeadline == null || o.ApplicationDeadline >= DateTime.UtcNow))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(o => o.Title.Contains(keyword) || o.Description!.Contains(keyword));

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(o => o.City == city);

            if (!string.IsNullOrWhiteSpace(field))
                query = query.Where(o => o.Field == field);

            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<List<InternshipOffer>> GetOffersByFieldAsync(string field)
        {
            return await _context.InternshipOffers
                                 .Where(o => o.Field == field)
                                 .ToListAsync();
        }

        public async Task<InternshipOffer?> GetOfferWithCompanyAndPeriodAsync(int offerId)
        {
            return await _context.InternshipOffers
                                 .Include(o => o.Company)
                                 .Include(o => o.Period)
                                 .FirstOrDefaultAsync(o => o.Id == offerId);
        }
    }
}