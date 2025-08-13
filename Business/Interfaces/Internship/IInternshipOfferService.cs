using Core.Entity;

namespace Business.Interfaces.Internship
{
    public interface IInternshipOfferService
    {
        Task<List<InternshipOffer>> GetAllActiveOffersAsync();
        Task<InternshipOffer?> GetOfferWithCompanyAsync(int offerId);
        Task<List<InternshipOffer>> GetOffersByCompanyIdAsync(int companyId);
        Task<List<InternshipOffer>> SearchOffersAsync(string keyword, string? city, string? field);
        Task<InternshipOffer> AddOfferAsync(InternshipOffer offer);
        Task UpdateOfferAsync(InternshipOffer offer);
        Task DeleteOfferAsync(int offerId);
    }
}