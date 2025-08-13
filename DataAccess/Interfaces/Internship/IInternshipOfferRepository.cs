using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Internship
{
    public interface IInternshipOfferRepository : IRepository<InternshipOffer>
    {
        Task<List<InternshipOffer>> GetAllActiveOffersAsync();
        Task<InternshipOffer?> GetOfferWithCompanyAsync(int offerId);
        Task<List<InternshipOffer>> GetOffersByCompanyIdAsync(int companyId);
        Task<List<InternshipOffer>> SearchOffersAsync(string keyword, string? city, string? field);
        Task<List<InternshipOffer>> GetOffersByFieldAsync(string field);
        Task<InternshipOffer?> GetOfferWithCompanyAndPeriodAsync(int offerId);
    }
}