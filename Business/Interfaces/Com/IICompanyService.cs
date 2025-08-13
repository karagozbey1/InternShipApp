using Core.Entity;

namespace Business.Interfaces.Com
{
    public interface IICompanyService
    {
        Task<Company?> GetCompanyByIdAsync(int companyId);
        Task<List<Company>> GetAllCompaniesAsync();
        Task<List<Company>> GetApprovedCompaniesAsync();
        Task<Company?> GetCompanyByNameAsync(string companyName);
        Task<List<Company>> GetCompaniesWithActiveOffersAsync();
        Task<List<Company>> SearchCompaniesByNameAsync(string name);
        Task<Company> AddCompanyAsync(Company company);
        Task UpdateCompanyAsync(Company company);
        Task DeleteCompanyAsync(int companyId);
        Task ApproveCompanyAsync(int companyId);
    }
}