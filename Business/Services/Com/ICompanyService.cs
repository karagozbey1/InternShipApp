using Business.Interfaces.Com;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Com
{
    public class ICompanyService : IICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ICompanyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    }
}