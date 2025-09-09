using Business.Interfaces.Com;
using Business.Interfaces.UserRepose;
using Core.Entity;
using DataAccess.Interfaces.UserRepose;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Business.Services.Com
{
    public class ICompanyService : IICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IICompanyUserService _companyUserService;
        private readonly IIUserService _userRepository;

        public ICompanyService(IUnitOfWork unitOfWork, IICompanyUserService _companyUserService, IIUserService ıUserService)
        {
            _unitOfWork = unitOfWork;
            this._companyUserService = _companyUserService;
            _userRepository = ıUserService;
        }

        public async Task<Company?> GetCompanyByIdAsync(int companyId)
        {
            return await _unitOfWork.Companies.GetByIdAsync(companyId);
        }

        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            var company = await _unitOfWork.Companies.GetAllAsync();
            return company.ToList();
        }

        public async Task<List<Company>> GetApprovedCompaniesAsync()
        {
            return await _unitOfWork.Companies.GetApprovedCompaniesAsync();
        }

        public async Task<Company?> GetCompanyByNameAsync(string companyName)
        {
            return await _unitOfWork.Companies.GetCompanyByNameAsync(companyName);
        }

        public async Task<List<Company>> GetCompaniesWithActiveOffersAsync()
        {
            return await _unitOfWork.Companies.GetCompaniesWithActiveOffersAsync();
        }

        public async Task<List<Company>> SearchCompaniesByNameAsync(string name)
        {
            return await _unitOfWork.Companies.SearchCompaniesByNameAsync(name);
        }

        public async Task<Company> AddCompanyAsync(Company company)
        {
            company.Approved = false;
            await _unitOfWork.Companies.AddAsync(company);
            await _unitOfWork.SaveAsync();
            return company;
        }

        public async Task UpdateCompanyAsync(Company company)
        {
            _unitOfWork.Companies.Update(company);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteCompanyAsync(int companyId)
        {
       
            var companyUsers = await _unitOfWork.CompanyUsers.GetUsersByCompanyIdAsync(companyId);
            if (companyUsers != null && companyUsers.Count > 0)
            {
                foreach (var cu in companyUsers)
                {
                
                    var user = await _unitOfWork.Users.GetByIdAsync(cu.UserId);
                    if (user != null)
                    {
                        _unitOfWork.Users.Remove(user);
                    }
                }
                await _unitOfWork.SaveAsync();
            }
      
            var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
            if (company != null)
            {
                _unitOfWork.Companies.Remove(company);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task ApproveCompanyAsync(int companyId)
        {
            var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
            if (company != null)
            {
                company.Approved = true;
                _unitOfWork.Companies.Update(company);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task AddCompanyWithUserAsync(Company company, User user, string roleInCompany)
        {
            var existingUser = await _unitOfWork.Users.GetUserByEmailAsync(user.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Bu e-posta adresiyle kayıtlı kullanıcı zaten var.");

            await _unitOfWork.Companies.AddAsync(company);
            await _unitOfWork.SaveAsync();

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();

            var companyUser = new CompanyUser
            {
                CompanyId = company.CompanyId,
                UserId = user.Id,
                RoleInCompany = roleInCompany
            };
            await _unitOfWork.CompanyUsers.AddAsync(companyUser);
            await _unitOfWork.SaveAsync();
        }

       
        public async Task<Company?> GetCompanyByUserIdAsync(int userId)
        {
            var companyUsers = await _unitOfWork.CompanyUsers.GetCompaniesByUserIdAsync(userId);
            if (companyUsers != null && companyUsers.Count > 0)
            {
                var companyId = companyUsers.First().CompanyId;
                return await _unitOfWork.Companies.GetByIdAsync(companyId);
            }
            return null;
        }

        
     
        public async Task AddCompanyDocumentAsync(int companyId, string fileName, string description)
        {
            // basit: gerçek dosya stream işlemini ayrı bir storage servisine taşın
            var doc = new InternshipDocument
            {
                CompanyId = companyId,
                FileName = fileName,
                ContentType = "application/octet-stream",
                Description = description,
                UploadedAt = DateTime.UtcNow
            };
            await _unitOfWork.InternshipDocuments.AddAsync(doc);
            await _unitOfWork.SaveAsync();
        }

        public async Task<Company?> GetCompanyWithDetailsAsync(int companyId)
        {
            return await _unitOfWork.Companies.GetCompanyWithDetailsAsync(companyId);
        }
    }
}
