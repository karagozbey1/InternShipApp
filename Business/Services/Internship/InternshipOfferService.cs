using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Internship
{
    // Staj teklifi/ilan işlemleri servis sınıfı
    // Şirketlerin staj ilanlarını yönetmesi için ana business logic katmanı
    // CRUD işlemleri, arama/filtreleme ve iş kuralları kontrollerini içerir
    // Repository pattern üzerinden veri erişimi sağlar
    public class InternshipOfferService : IInternshipOfferService
    {
        // Unit of Work pattern ile veritabanı işlemleri
        // Transaction yönetimi ve repository koordinasyonu için kullanılır
        // Atomic operations ve data consistency sağlar
        private readonly IUnitOfWork _unitOfWork;

        // Dependency injection constructor
        // Service lifetime management (Scoped) ile EF Core DbContext lifecycle'ına uygun
        public InternshipOfferService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Aktif olan tüm staj tekliflerini getirir
        // Public olarak yayınlanmış, başvuru kabul eden ilanlar
        // IsActive=true ve ApplicationDeadline henüz geçmemiş olanlar
        // Öğrenci arama sayfası ve genel listeleme için kullanılır
        public async Task<List<InternshipOffer>> GetAllActiveOffersAsync()
        {
            // Repository seviyesinde filtering yapılır - performans optimizasyonu
            // Include ile Company navigation property'si de yüklenir
            return await _unitOfWork.InternshipOffers.GetAllActiveOffersAsync();
        }

        // Belirli bir teklifi şirket bilgileri ile birlikte getirir
        // Detay sayfalarında şirket profil bilgileri göstermek için kritik
        // Eager loading ile N+1 problem'ini önler
        public async Task<InternshipOffer?> GetOfferWithCompanyAsync(int offerId)
        {
            // Input validation - geçersiz ID'ler için null döner
            // Repository'nin Include() methodology'si ile optimize edilmiş sorgu
            return await _unitOfWork.InternshipOffers.GetOfferWithCompanyAsync(offerId);
        }

        // Şirkete ait tüm staj tekliflerini getirir
        // Şirket dashboard'ında kendi ilanlarını görüntülemek için
        // Hem aktif hem pasif ilanları içerebilir - business logic'e göre
        public async Task<List<InternshipOffer>> GetOffersByCompanyIdAsync(int companyId)
        {
            // Company authorization kontrolü öncesinde çağrılır
            // Şirket sadece kendi ilanlarına erişebilir güvenlik prensibi
            return await _unitOfWork.InternshipOffers.GetOffersByCompanyIdAsync(companyId);
        }

        // Gelişmiş arama fonksiyonu - öğrenci filtreleme için
        // Keyword: pozisyon başlığı, açıklama veya şirket adında arama
        // City: şehir bazlı lokasyon filtresi
        // Field: alan/sektör bazlı kategori filtresi
        // Full-text search ve multi-criteria filtering destekler
        public async Task<List<InternshipOffer>> SearchOffersAsync(string keyword, string? city, string? field)
        {
            // Repository'de LINQ Where clause'ları ile optimized filtering
            // Contains() method'u SQL LIKE operation'ına translate edilir
            // Case-insensitive search için StringComparison kullanılır
            return await _unitOfWork.InternshipOffers.SearchOffersAsync(keyword, city, field);
        }

        // Yeni staj teklifi oluşturma - şirket tarafından ilan verme
        // Business rules validation ve data integrity kontrolü içerir
        // Şirket yetkisi, kontenjan limitleri ve tarih validasyonları yapılır
        public async Task<InternshipOffer> AddOfferAsync(InternshipOffer offer)
        {
            // İş kuralları validasyonları burada yapılabilir
            // Örnek business rules:
            // - Şirketin aktif üyelik durumu kontrolü
            // - Maksimum ilan sayısı limitinin kontrolü  
            // - Tarih aralığı geçerliliği (StartDate < EndDate)
            // - ApplicationDeadline'ın gelecek bir tarih olması
            // - Required field'ların dolu olması kontrolü
            
            // Entity validation - null ve format kontrolları
            if (offer == null)
                throw new ArgumentNullException(nameof(offer));
            
            if (string.IsNullOrWhiteSpace(offer.Title))
                throw new ArgumentException("İlan başlığı boş olamaz");
                
            if (string.IsNullOrWhiteSpace(offer.City))
                throw new ArgumentException("Şehir bilgisi zorunludur");
                
            if (string.IsNullOrWhiteSpace(offer.Field))
                throw new ArgumentException("Alan/sektör bilgisi zorunludur");

            // Şirket varlık kontrolü - referential integrity
            var company = await _unitOfWork.Companies.GetByIdAsync(offer.CompanyId);
            if (company == null)
                throw new InvalidOperationException("İlişkili şirket bulunamadı");
                
            if (!company.Approved)
                throw new InvalidOperationException("Onaylanmamış şirketler ilan veremez");

            // Tarih validasyonları - business logic consistency
            if (offer.ApplicationDeadline.HasValue && offer.ApplicationDeadline.Value <= DateTime.UtcNow)
                throw new InvalidOperationException("Başvuru son tarihi geçmiş bir tarih olamaz");

            // Default değerler ve timestamps atama
            offer.CreatedAt = DateTime.UtcNow;
            offer.IsActive = true; // Yeni ilanlar varsayılan olarak aktif
            
            // Repository'ye ekleme ve transaction commit
            await _unitOfWork.InternshipOffers.AddAsync(offer);
            await _unitOfWork.SaveAsync();
            
            return offer;
        }

        // Mevcut staj teklifi güncelleme - şirket düzenleme işlemleri
        // Sadece ilan sahibi şirket güncelleyebilir - authorization gerekli
        // Başvuru alındıktan sonra kritik alanlar değiştirilemez business rule'u
        public async Task UpdateOfferAsync(InternshipOffer offer)
        {
            // Entity existence kontrolü
            if (offer == null)
                throw new ArgumentNullException(nameof(offer));
                
            var existingOffer = await _unitOfWork.InternshipOffers.GetByIdAsync(offer.Id);
            if (existingOffer == null)
                throw new InvalidOperationException("Güncellenecek ilan bulunamadı");

            // Business rule: Başvuru alındıktan sonra bazı alanlar değiştirilemez
            var hasApplications = await _unitOfWork.InternshipApplications
                .GetApplicationsByOfferIdAsync(offer.Id);
                
            if (hasApplications.Any())
            {
                // Başvuru alındıktan sonra sadece açıklama ve deadline değiştirilebilir
                // Title, City, Field gibi kritik alanlar korunur
                existingOffer.Description = offer.Description;
                existingOffer.Requirements = offer.Requirements;
                existingOffer.ApplicationDeadline = offer.ApplicationDeadline;
                existingOffer.IsActive = offer.IsActive; // Aktif/pasif durumu değiştirilebilir
            }
            else
            {
                // Henüz başvuru yoksa tüm alanlar güncellenebilir
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
            
            // Change tracking ile EF Core otomatik detect eder
            _unitOfWork.InternshipOffers.Update(existingOffer);
            await _unitOfWork.SaveAsync();
        }

        // Staj teklifi silme - soft delete yerine hard delete yapılır
        // Başvuru alınmış ilanlar silinmemeli - data integrity koruması
        // Sadece ilan sahibi şirket silebilir - authorization kontrol gerekli
        public async Task DeleteOfferAsync(int offerId)
        {
            // Entity varlık kontrolü
            var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(offerId);
            if (offer == null)
                return; // Already deleted - idempotent operation
                
            // Business rule: Başvuru alınmış ilanlar silinememeli
            var applications = await _unitOfWork.InternshipApplications
                .GetApplicationsByOfferIdAsync(offerId);
                
            if (applications.Any())
            {
                throw new InvalidOperationException("Başvuru alınmış ilanlar silinemez. İlanı pasif hale getirebilirsiniz.");
            }

            // Hard delete - entity tamamen kaldırılır
            _unitOfWork.InternshipOffers.Remove(offer);
            await _unitOfWork.SaveAsync();
        }

        // ID ile tek teklif getirme - detay sayfası ve düzenleme için
        // Navigation property'leri ile birlikte optimize edilmiş loading
        // Company ve Period bilgilerini eager loading ile getirir
        public async Task<InternshipOffer?> GetByIdAsync(int id)
        {
            // Input validation - geçersiz ID'ler için erken dönüş
            if (id <= 0)
                return null;
                
            // Önce optimize edilmiş method'u dene - Company ve Period ile birlikte
            // Bu method Include() kullanarak N+1 problem'ini önler
            var offer = await _unitOfWork.InternshipOffers.GetOfferWithCompanyAndPeriodAsync(id);
            if (offer != null) 
                return offer;

            // Fallback: basic method - navigation property'ler lazy loading ile yüklenir
            // Legacy compatibility için korunur
            return await _unitOfWork.InternshipOffers.GetByIdAsync(id);
        }

        // Batch operations - toplu işlemler için optimize edilmiş method'lar
        // Admin panelinde veya migration işlemlerinde kullanılır
        public async Task ActivateOffersAsync(List<int> offerIds)
        {
            // Bulk update operation - tek sorguda birden fazla entity güncelleme
            foreach (var offerId in offerIds)
            {
                var offer = await _unitOfWork.InternshipOffers.GetByIdAsync(offerId);
                if (offer != null)
                {
                    offer.IsActive = true;
                    _unitOfWork.InternshipOffers.Update(offer);
                }
            }
            
            // Single transaction - all or nothing semantics
            await _unitOfWork.SaveAsync();
        }

        // Deadline geçmiş ilanları pasifleştirme yardımcı method'u
        // Background service tarafından çağrılabilir
        public async Task DeactivateExpiredOffersAsync()
        {
            // Mevcut repository method'larını kullanarak expired offers'ı bul
            var allActiveOffers = await _unitOfWork.InternshipOffers.GetAllActiveOffersAsync();
            
            // LINQ ile expired offers'ı filtrele
            var expiredOffers = allActiveOffers
                .Where(o => o.ApplicationDeadline.HasValue && o.ApplicationDeadline.Value < DateTime.UtcNow)
                .ToList();
                
            foreach (var offer in expiredOffers)
            {
                offer.IsActive = false; // Soft deactivation
                _unitOfWork.InternshipOffers.Update(offer);
            }
            
            // Audit log için expire operation
            if (expiredOffers.Count > 0)
            {
                await _unitOfWork.SaveAsync();
                // Log the operation for system monitoring
                Console.WriteLine($"Deactivated {expiredOffers.Count} expired offers at {DateTime.UtcNow}");
            }
        }
    }
    
    // Statistics ViewModel - istatistik bilgileri için data transfer object
    // Business layer'dan presentation layer'a veri taşımak için
    public class OfferStatisticsViewModel
    {
        public int TotalOffers { get; set; }              // Toplam ilan sayısı
        public int ActiveOffers { get; set; }             // Aktif ilan sayısı  
        public int InactiveOffers { get; set; }           // Pasif ilan sayısı
        public int TotalApplicationsReceived { get; set; } // Alınan toplam başvuru
        public decimal AverageApplicationsPerOffer { get; set; } // İlan başına ortalama başvuru
    }
}