using Core.Entity;

namespace Business.Interfaces.Com
{
    public interface IICompanyUserService
    {
        Task<List<CompanyUser>> GetUsersInCompanyAsync(int companyId);
        Task<List<CompanyUser>> GetCompaniesForUserAsync(int userId);
        Task<CompanyUser> AddUserToCompanyAsync(int companyId, int userId, string? roleInCompany);
        Task RemoveUserFromCompanyAsync(int companyId, int userId);
        Task UpdateUserRoleInCompanyAsync(int companyId, int userId, string newRole);
    }
}