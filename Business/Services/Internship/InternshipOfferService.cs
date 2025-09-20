using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Internship
{
    // Staj ilan işlemleri servisi
    public class InternshipOfferService : IInternshipOfferService
    {
        // Veritabanı işlemleri
        private readonly IUnitOfWork _unitOfWork;

        // Kurucu metod
        public InternshipOfferService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Aktif staj ilanlarını getirir
        public async Task<List<InternshipOffer>> GetAllActiveOffersAsync()
        {
            return await _unitOfWork.InternshipOffers.GetAllActiveOffersAsync();
        }

        // İlanı şirket bilgileri ile getirir
        public async Task<InternshipOffer?> GetOfferWithCompanyAsync(int offerId)
        {
            return await _unitOfWork.InternshipOffers.GetOfferWithCompanyAsync(offerId);
        }

        // Şirketin tüm ilanlarını getirir
        public async Task<List<InternshipOffer>> GetOffersByCompanyIdAsync(int companyId)
        {
            return await _unitOfWork.InternshipOffers.GetOffersByCompanyIdAsync(companyId);
        }

        // İlan arama yapar
        public async Task<List<InternshipOffer>> SearchOffersAsync(string keyword, string? city, string? field)
        {
            return await _unitOfWork.InternshipOffers.SearchOffersAsync(keyword, city, field);
        }

        // Yeni ilan ekler
        public async Task<InternshipOffer> AddOfferAsync(InternshipOffer offer)
        {
            // Gerekli kontroller
            if (offer == null)
                throw new ArgumentNullException(nameof(offer));
            
            if (string.IsNullOrWhiteSpace(offer.Title))
                throw new ArgumentException("İlan başlığı boş olamaz");
                
            if (string.IsNullOrWhiteSpace(offer.City))
                throw new ArgumentException("Şehir bilgisi zorunludur");
                
            if (string.IsNullOrWhiteSpace(offer.Field))
                throw new ArgumentException("Alan bilgisi zorunludur");

            // Şirket kontrolü
            var company = await _unitOfWork.Companies.GetByIdAsync(offer.CompanyId);
            if (company == null)
                throw new InvalidOperationException("İlişkili şirket bulunamadı");
                
            if (!company.Approved)
                throw new InvalidOperationException("Onaylanmamış şirketler ilan veremez");

            // Tarih kontrolü
            if (offer.ApplicationDeadline.HasValue && offer.ApplicationDeadline.Value <= DateTime.UtcNow)
                throw new InvalidOperationException("Başvuru son tarihi geçmiş bir tarih olamaz");

            // Varsayılan değerler
            offer.CreatedAt = DateTime.UtcNow;
            offer.IsActive = true;
            
            // Kaydet
            await _unitOfWork.InternshipOffers.AddAsync(offer);
            await _unitOfWork.SaveAsync();
            
            return offer;
        }

        // İlan günceller
        public async Task UpdateOfferAsync(InternshipOffer offer)
        {
            if (offer == null)
                throw new ArgumentNullException(nameof(offer));
                
            var existingOffer = await _unitOfWork.InternshipOffers.GetByIdAsync(offer.Id);
            if (existingOffer == null)
                throw new InvalidOperationException("Güncellenecek ilan bulunamadı");

            // Başvuru var mı kontrol et
            var hasApplications = await _unitOfWork.InternshipApplications
                .GetApplicationsByOfferIdAsync(offer.Id);
                
            if (hasApplications.Any())
            {
                // Başvuru varsa sadece bu alanlar değiştirilebilir
                existingOffer.Description = offer.Description;
                existingOffer.Requirements = offer.Requirements;
                existingOffer.ApplicationDeadline = offer.ApplicationDeadline;
                existingOffer.IsActive = offer.IsActive;
            }
            else
            {
                // Başvuru yoksa tüm alanlar değiştirilebilir
                existingOffer.Title = offer.Title;
                existingOffer.Description = offer.Description;
                existingOffer.Requirements = offer.Requirements;
                existingOffer.City = offer.City;
                existingOffer.Field = offer.Field;
                existingOffer.AvailablePositions = offer.AvailablePositions;
                existingOffer.ApplicationDeadline = offer.ApplicationDeadline;
                existingOffer.IsActive = offer.IsActive;
                existingOffer.MonthlySalary = offer.MonthlySalary;
                existingOffer.MonthlySalaryCurrency = offer.MonthlySalaryCurrency;
            }
            
            _unitOfWork.InternshipOffers.Update(existingOffer);
            await _unitOfWork.SaveAsync();
        }

        // İlan siler
        public async Task DeleteOfferAsync(int offerId)
        {
            var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(offerId);
            if (offer == null)
                return;
                
            // Başvuru var mı kontrol et
            var applications = await _unitOfWork.InternshipApplications
                .GetApplicationsByOfferIdAsync(offerId);
                
            if (applications.Any())
            {
                throw new InvalidOperationException("Başvuru alınmış ilanlar silinemez. İlanı pasif hale getirebilirsiniz.");
            }

            _unitOfWork.InternshipOffers.Remove(offer);
            await _unitOfWork.SaveAsync();
        }

        // Kimlik ile ilan getirir
        public async Task<InternshipOffer?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;
                
            var offer = await _unitOfWork.InternshipOffers.GetOfferWithCompanyAndPeriodAsync(id);
            if (offer != null) 
                return offer;

            return await _unitOfWork.InternshipOffers.GetByIdAsync(id);
        }

        // Toplu ilan aktifleştirme
        public async Task ActivateOffersAsync(List<int> offerIds)
        {
            foreach (var offerId in offerIds)
            {
                var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(offerId);
                if (offer != null)
                {
                    offer.IsActive = true;
                    _unitOfWork.InternshipOffers.Update(offer);
                }
            }
            
            await _unitOfWork.SaveAsync();
        }

        // Süresi dolmuş ilanları pasifleştirir
        public async Task DeactivateExpiredOffersAsync()
        {
            var allActiveOffers = await _unitOfWork.InternshipOffers.GetAllActiveOffersAsync();
            
            var expiredOffers = allActiveOffers
                .Where(o => o.ApplicationDeadline.HasValue && o.ApplicationDeadline.Value < DateTime.UtcNow)
                .ToList();
                
            foreach (var offer in expiredOffers)
            {
                offer.IsActive = false;
                _unitOfWork.InternshipOffers.Update(offer);
            }
            
            if (expiredOffers.Count > 0)
            {
                await _unitOfWork.SaveAsync();
                Console.WriteLine($"{expiredOffers.Count} ilan pasifleştirildi - {DateTime.UtcNow}");
            }
        }
    }
    
    // İlan istatistikleri
    public class OfferStatisticsViewModel
    {
        // Toplam ilan sayısı
        public int TotalOffers { get; set; }
        
        // Aktif ilan sayısı
        public int ActiveOffers { get; set; }
        
        // Pasif ilan sayısı
        public int InactiveOffers { get; set; }
        
        // Alınan toplam başvuru
        public int TotalApplicationsReceived { get; set; }
        
        // İlan başına ortalama başvuru
        public decimal AverageApplicationsPerOffer { get; set; }
    }
}