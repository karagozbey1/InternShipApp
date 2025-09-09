using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace DataAccess.Interfaces.Com
{
   
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<List<Company>> SearchCompaniesByNameAsync(string name);
        Task<List<Company>> GetApprovedCompaniesAsync();
        Task<Company?> GetCompanyWithDetailsAsync(int companyId);
        Task<Company?> GetCompanyByNameAsync(string companyName);
        Task<List<Company>> GetCompaniesWithActiveOffersAsync();
        Task<Company?> GetCompanyByUserIdAsync(int userId);
    }
}