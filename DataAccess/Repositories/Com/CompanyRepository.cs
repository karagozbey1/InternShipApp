using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Com;
using Microsoft.EntityFrameworkCore;


namespace DataAccess.Repositories.Com
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
       

        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            
        }

        public async Task<List<Company>> SearchCompaniesByNameAsync(string name)
        {
            return await _context.Companies
                .Where(c => c.CompanyName.Contains(name))
                .ToListAsync();
        }

        public async Task<List<Company>> GetApprovedCompaniesAsync()
        {
            return await _context.Companies
                .Where(c => c.Approved)
                .ToListAsync();
        }

        public async Task<Company?> GetCompanyWithDetailsAsync(int companyId)
        {
            return await _context.Companies
                .Include(c => c.CompanyUsers)
                .ThenInclude(cu => cu.User)
                .Include(c => c.InternshipOffers)
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);
        }

        public async Task<Company?> GetCompanyByNameAsync(string companyName)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyName == companyName);
        }

        public async Task<List<Company>> GetCompaniesWithActiveOffersAsync()
        {
            return await _context.Companies
                .Where(c => c.InternshipOffers!.Any(o => o.IsActive && (o.ApplicationDeadline == null || o.ApplicationDeadline >= DateTime.UtcNow)))
                .ToListAsync();
        }

        public Task<Company?> GetCompanyByUserIdAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
}