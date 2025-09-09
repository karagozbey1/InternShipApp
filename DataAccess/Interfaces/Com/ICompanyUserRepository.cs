using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Com
{
    public interface ICompanyUserRepository : IRepository<CompanyUser>
    {
        Task<List<CompanyUser>> GetUsersByCompanyIdAsync(int companyId);
        Task<List<CompanyUser>> GetCompaniesByUserIdAsync(int userId);
        Task<CompanyUser?> GetCompanyUserAsync(int companyId, int userId);
        Task<Company?> GetCompanyByUserIdAsync(int userId);
    }
}