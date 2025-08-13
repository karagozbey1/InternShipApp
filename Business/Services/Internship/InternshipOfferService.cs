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
            // Repository'deki özel metodu kullanarak aktif staj tekliflerini getirir
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
            // Repository'deki özel metodu kullanarak arama yapar
            return await _unitOfWork.InternshipOffers.SearchOffersAsync(keyword, city, field);
        }

        public async Task<InternshipOffer> AddOfferAsync(InternshipOffer offer)
        {
            // İş mantığı: Yeni staj teklifi eklemeden önce gerekli kontroller yapılabilir.
            // Örneğin: İlgili şirketin var olup olmadığı veya teklif tarih aralığının geçerliliği
            await _unitOfWork.InternshipOffers.AddAsync(offer);
            await _unitOfWork.SaveAsync(); // Değişiklikleri kaydet
            return offer;
        }

        public async Task UpdateOfferAsync(InternshipOffer offer)
        {
            // İş mantığı: Güncelleme öncesi yetkilendirme kontrolü yapılabilir.
            _unitOfWork.InternshipOffers.Update(offer);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteOfferAsync(int offerId)
        {
            var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(offerId);
            if (offer != null)
            {
                // İş mantığı: Silme işlemi öncesi ilgili başvuruların durumu kontrol edilebilir.
                _unitOfWork.InternshipOffers.Remove(offer);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}