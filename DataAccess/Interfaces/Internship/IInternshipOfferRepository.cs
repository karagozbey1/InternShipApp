using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Internship
{
    // İlan veri erişim arabirimi
    public interface IInternshipOfferRepository : IRepository<InternshipOffer>
    {
        // Aktif ilanları getirir
        Task<List<InternshipOffer>> GetAllActiveOffersAsync();
        
        // İlanı şirket ile getirir
        Task<InternshipOffer?> GetOfferWithCompanyAsync(int offerId);
        
        // Şirket ilanlarını getirir
        Task<List<InternshipOffer>> GetOffersByCompanyIdAsync(int companyId);
        
        // İlan arama yapar
        Task<List<InternshipOffer>> SearchOffersAsync(string keyword, string? city, string? field);
        
        // Alana göre ilanları getirir
        Task<List<InternshipOffer>> GetOffersByFieldAsync(string field);
        
        // İlanı şirket ve dönem ile getirir
        Task<InternshipOffer?> GetOfferWithCompanyAndPeriodAsync(int offerId);
    }
}