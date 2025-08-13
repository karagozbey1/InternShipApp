using Business.Interfaces.Com;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Com
{
    public class ICompanyUserService : IICompanyUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ICompanyUserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CompanyUser>> GetUsersInCompanyAsync(int companyId)
        {
            return await _unitOfWork.CompanyUsers.GetUsersByCompanyIdAsync(companyId);
        }

        public async Task<List<CompanyUser>> GetCompaniesForUserAsync(int userId)
        {
            return await _unitOfWork.CompanyUsers.GetCompaniesByUserIdAsync(userId);
        }

        public async Task<CompanyUser> AddUserToCompanyAsync(int companyId, int userId, string? roleInCompany)
        {
            if (await _unitOfWork.CompanyUsers.GetCompanyUserAsync(companyId, userId) != null)
            {
                throw new InvalidOperationException("User is already a member of this company.");
            }

            var companyUser = new CompanyUser
            {
                CompanyId = companyId,
                UserId = userId,
                RoleInCompany = roleInCompany
            };
            await _unitOfWork.CompanyUsers.AddAsync(companyUser);
            await _unitOfWork.SaveAsync();
            return companyUser;
        }

        public async Task RemoveUserFromCompanyAsync(int companyId, int userId)
        {
            var companyUser = await _unitOfWork.CompanyUsers.GetCompanyUserAsync(companyId, userId);
            if (companyUser != null)
            {
                _unitOfWork.CompanyUsers.Remove(companyUser);
                await _unitOfWork.SaveAsync ();
            }
        }

        public async Task UpdateUserRoleInCompanyAsync(int companyId, int userId, string newRole)
        {
            var companyUser = await _unitOfWork.CompanyUsers.GetCompanyUserAsync(companyId, userId);
            if (companyUser != null)
            {
                companyUser.RoleInCompany = newRole;
                _unitOfWork.CompanyUsers.Update(companyUser);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}