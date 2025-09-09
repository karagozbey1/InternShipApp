using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Internship
{
    public class InternshipOfferService : IInternshipOfferService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InternshipOfferService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<InternshipOffer>> GetAllActiveOffersAsync()
        {
            return await _unitOfWork.InternshipOffers.GetAllActiveOffersAsync();
        }

        public async Task<InternshipOffer?> GetOfferWithCompanyAsync(int offerId)
        {
            // Repository'deki özel metodu kullanarak staj teklifini şirket bilgisiyle birlikte getirir
            return await _unitOfWork.InternshipOffers.GetOfferWithCompanyAsync(offerId);
        }

        public async Task<List<InternshipOffer>> GetOffersByCompanyIdAsync(int companyId)
        {
            // Repository'deki özel metodu kullanarak belirli bir şirkete ait staj tekliflerini getirir
            return await _unitOfWork.InternshipOffers.GetOffersByCompanyIdAsync(companyId);
        }

        public async Task<List<InternshipOffer>> SearchOffersAsync(string keyword, string? city, string? field)
        {
            return await _unitOfWork.InternshipOffers.SearchOffersAsync(keyword, city, field);
        }

        public async Task<InternshipOffer> AddOfferAsync(InternshipOffer offer)
        {
            // İş mantığı: Yeni staj teklifi eklemeden önce gerekli kontroller yapılabilir.
            // Örneğin: İlgili şirketin var olup olmadığı veya teklif tarih aralığının geçerliliği
            await _unitOfWork.InternshipOffers.AddAsync(offer);
            await _unitOfWork.SaveAsync(); 
            return offer;
        }

        public async Task UpdateOfferAsync(InternshipOffer offer)
        {
            _unitOfWork.InternshipOffers.Update(offer);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteOfferAsync(int offerId)
        {
            var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(offerId);
            if (offer != null)
            {
                _unitOfWork.InternshipOffers.Remove(offer);
                await _unitOfWork.SaveAsync();
            }
        }

        // Changed: return offer with Company (and Period) navigation loaded
        public async Task<InternshipOffer?> GetByIdAsync(int id)
        {
            // Use repository method that includes Company and Period
            var offer = await _unitOfWork.InternshipOffers.GetOfferWithCompanyAndPeriodAsync(id);
            if (offer != null) return offer;

            // Fallback to basic GetByIdAsync if specialized method not available
            return await _unitOfWork.InternshipOffers.GetByIdAsync(id);
        }
    }
}