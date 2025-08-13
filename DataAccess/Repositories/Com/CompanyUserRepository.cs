using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Com;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Com
{
    public class CompanyUserRepository : Repository<CompanyUser>, ICompanyUserRepository
    {
        public CompanyUserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<CompanyUser>> GetUsersByCompanyIdAsync(int companyId)
        {
            return await _context.CompanyUsers
                .Include(cu => cu.User)
                .Where(cu => cu.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task<List<CompanyUser>> GetCompaniesByUserIdAsync(int userId)
        {
            return await _context.CompanyUsers
                .Include(cu => cu.Company)
                .Where(cu => cu.UserId == userId)
                .ToListAsync();
        }

        public async Task<CompanyUser?> GetCompanyUserAsync(int companyId, int userId)
        {
            return await _context.CompanyUsers
                .FirstOrDefaultAsync(cu => cu.CompanyId == companyId && cu.UserId == userId);
        }
    }
}