using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;


namespace Business.Services.Internship;

/// <summary>
/// Staj belgeleri yönetim servisi
/// Öğrenci belgelerinin yüklenmesi, güncellenmesi, silinmesi ve listelenmesi işlemlerini yürütür
/// </summary>
public class InternshipDocumentService : IInternshipDocumentService
{
    // Veritabanı işlemleri için unit of work pattern
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Constructor - dependency injection ile UnitOfWork'ü al
    /// </summary>
    /// <param name="unitOfWork">Veritabanı işlemleri için unit of work</param>
    public InternshipDocumentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Belge ID'sine göre tek bir belge getir
    /// </summary>
    /// <param name="documentId">Aranacak belgenin ID'si</param>
    /// <returns>Bulunan belge veya null</returns>
    public async Task<InternshipDocument?> GetDocumentByIdAsync(int documentId)
    {
        return await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
    }

    /// <summary>
    /// Belirli bir başvuruya ait tüm belgeleri getir
    /// </summary>
    /// <param name="applicationId">Başvuru ID'si</param>
    /// <returns>Başvuruya ait belgeler listesi</returns>
    public async Task<List<InternshipDocument>> GetDocumentsByApplicationIdAsync(int applicationId)
    {
        return await _unitOfWork.InternshipDocuments.GetDocumentsByApplicationIdAsync(applicationId);
    }

    /// <summary>
    /// Belirli bir öğrenciye ait tüm belgeleri getir
    /// </summary>
    /// <param name="studentId">Öğrenci ID'si</param>
    /// <returns>Öğrenciye ait belgeler listesi</returns>
    public async Task<List<InternshipDocument>> GetDocumentsByStudentIdAsync(int studentId)
    {
        // Parametre validasyonu
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        return await _unitOfWork.InternshipDocuments.GetDocumentsByStudentIdAsync(studentId);
    }

    /// <summary>
    /// Öğrencinin başvuruya bağlı olmayan (bağımsız) belgelerini getir
    /// Profil belgeleri, CV, transkript gibi genel belgeler
    /// </summary>
    /// <param name="studentId">Öğrenci ID'si</param>
    /// <returns>Bağımsız belgeler listesi</returns>
    public async Task<List<InternshipDocument>> GetIndependentDocumentsByStudentIdAsync(int studentId)
    {
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        return await _unitOfWork.InternshipDocuments.GetIndependentDocumentsByStudentIdAsync(studentId);
    }

    /// <summary>
    /// Bir belgeyi onay formuna bağla
    /// Danışman onay sürecinde kullanılır
    /// </summary>
    /// <param name="documentId">Belge ID'si</param>
    /// <param name="approvalFormId">Onay formu ID'si</param>
    public async Task LinkDocumentToApprovalAsync(int documentId, int approvalFormId)
    {
        // Parametre validasyonları
        if (documentId <= 0)
            throw new ArgumentException("Geçersiz belge ID", nameof(documentId));
        
        if (approvalFormId <= 0)
            throw new ArgumentException("Geçersiz onay formu ID", nameof(approvalFormId));

        // Belgeyi onay formuna bağla
        await _unitOfWork.InternshipDocuments.LinkDocumentToApprovalAsync(documentId, approvalFormId);
        await _unitOfWork.SaveAsync();
    }

    /// <summary>
    /// Yeni belge ekleme metodu
    /// Dosya yükleme işleminden sonra veritabanına kaydetmek için kullanılır
    /// </summary>
    /// <param name="document">Eklenecek belge entity'si</param>
    /// <returns>Eklenen belge (ID ile birlikte)</returns>
    public async Task<InternshipDocument> AddDocumentAsync(InternshipDocument document)
    {
        // Null kontrolü
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        // Zorunlu alanların kontrolü
        if (string.IsNullOrWhiteSpace(document.FileName))
            throw new ArgumentException("Dosya adı boş olamaz", nameof(document.FileName));

        if (string.IsNullOrWhiteSpace(document.FilePath))
            throw new ArgumentException("Dosya yolu boş olamaz", nameof(document.FilePath));

        if (string.IsNullOrWhiteSpace(document.DocumentType))
            throw new ArgumentException("Dosya türü boş olamaz", nameof(document.DocumentType));

        // Alan uzunluk kontrolleri (veritabanı limit aşımı önlemi)
        if (document.FileName.Length > 500)
            throw new ArgumentException("Dosya adı çok uzun (max 500 karakter)", nameof(document.FileName));

        if (document.FilePath.Length > 1000)
            throw new ArgumentException("Dosya yolu çok uzun (max 1000 karakter)", nameof(document.FilePath));

        if (document.DocumentType.Length > 100)
            throw new ArgumentException("Dosya türü çok uzun (max 100 karakter)", nameof(document.DocumentType));

        // Opsiyonel alanların uzunluk kontrolü
        if (!string.IsNullOrEmpty(document.Description) && document.Description.Length > 1000)
            throw new ArgumentException("Açıklama çok uzun (max 1000 karakter)", nameof(document.Description));

        if (!string.IsNullOrEmpty(document.ContentType) && document.ContentType.Length > 200)
            throw new ArgumentException("Content type çok uzun (max 200 karakter)", nameof(document.ContentType));

        if (!string.IsNullOrEmpty(document.Status) && document.Status.Length > 50)
            throw new ArgumentException("Status çok uzun (max 50 karakter)", nameof(document.Status));

        // En az bir sahiplik ID'si olmalı (hangi entity'ye ait olduğu)
        if (!document.StudentId.HasValue && !document.ApplicationId.HasValue && !document.CompanyId.HasValue)
            throw new ArgumentException("En az bir ID (StudentId, ApplicationId veya CompanyId) belirtilmelidir");

        try
        {
            // Yüklenme zamanını sistem zamanı olarak ayarla
            document.UploadedAt = DateTime.UtcNow;
            
            // Veritabanına ekle
            await _unitOfWork.InternshipDocuments.AddAsync(document);
            await _unitOfWork.SaveAsync();
            
            return document;
        }
        catch (Exception ex)
        {
            // Veritabanı hatası durumunda anlamlı hata mesajı fırlat
            throw new InvalidOperationException("Belge veritabanına kaydedilirken bir hata oluştu.", ex);
        }
    }

    /// <summary>
    /// Mevcut belgeyi güncelleme metodu
    /// Belge adı, açıklama gibi meta verileri güncellemek için
    /// </summary>
    /// <param name="document">Güncellenecek belge</param>
    public async Task UpdateDocumentAsync(InternshipDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        try
        {
            // Belgeyi güncelle ve değişiklikleri kaydet
            _unitOfWork.InternshipDocuments.Update(document);
            await _unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Belge güncellenirken bir hata oluştu.", ex);
        }
    }

    /// <summary>
    /// Belge silme metodu
    /// Hem veritabanından hem de fiziksel dosyayı silmek için kullanılır
    /// </summary>
    /// <param name="documentId">Silinecek belgenin ID'si</param>
    public async Task DeleteDocumentAsync(int documentId)
    {
        // Belgeyi veritabanından bul
        var document = await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
        if (document != null)
        {
            // Belgeyi sil ve değişiklikleri kaydet
            _unitOfWork.InternshipDocuments.Remove(document);
            await _unitOfWork.SaveAsync();
            
            // NOT: Fiziksel dosya silme işlemi burada yapılabilir
            // File.Delete(document.FilePath) gibi
        }
    }

    /// <summary>
    /// Şirketin yüklediği belgeleri getir
    /// Şirket profil belgeleri, sözleşmeler vb.
    /// </summary>
    /// <param name="companyId">Şirket ID'si</param>
    /// <returns>Şirkete ait belgeler listesi</returns>
    public async Task<List<InternshipDocument>> GetCompanyDocumentsAsync(int companyId)
    {
        if (companyId <= 0)
            throw new ArgumentException("Geçersiz şirket ID", nameof(companyId));

        return await _unitOfWork.InternshipDocuments.GetCompanyDocumentsAsync(companyId);
    }

    /// <summary>
    /// Şirket adına yeni belge ekleme
    /// Şirket sözleşmeleri, profil dokümanları için
    /// </summary>
    /// <param name="companyId">Şirket ID'si</param>
    /// <param name="fileName">Dosya adı</param>
    /// <param name="description">Belge açıklaması</param>
    public async Task AddCompanyDocumentAsync(int companyId, string fileName, string description)
    {
        // Parametre validasyonları
        if (companyId <= 0)
            throw new ArgumentException("Geçersiz şirket ID", nameof(companyId));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Dosya adı boş olamaz", nameof(fileName));

        // Şirketin varlığını kontrol et
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new InvalidOperationException("Şirket bulunamadı");

        // Dosya uzantısını çıkar
        var fileExtension = Path.GetExtension(fileName)?.TrimStart('.');
        
        // Yeni belge entity'si oluştur
        var doc = new InternshipDocument
        {
            CompanyId = companyId,
            FileName = fileName,
            FilePath = Path.Combine("uploads", "companies", companyId.ToString(), fileName), // Şirket klasörü
            ContentType = "application/octet-stream", // Genel content type
            Description = description,
            UploadedAt = DateTime.UtcNow,
            DocumentType = fileExtension ?? string.Empty
        };

        // Veritabanına ekle
        await _unitOfWork.InternshipDocuments.AddAsync(doc);
        await _unitOfWork.SaveAsync();
    }

    /// <summary>
    /// Öğrencinin staj defteri girişlerini getir
    /// Günlük staj aktiviteleri ve raporlar
    /// </summary>
    /// <param name="studentId">Öğrenci ID'si</param>
    /// <returns>Staj defteri girişleri listesi</returns>
    public async Task<List<InternshipDocument>> GetStudentDiaryEntriesAsync(int studentId)
    {
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        return await _unitOfWork.InternshipDocuments
            .GetDiaryEntriesByStudentIdAsync(studentId);
    }

    /// <summary>
    /// Yeni staj defteri girişi oluştur
    /// Öğrencinin günlük aktivite kayıtları için
    /// </summary>
    /// <param name="entry">Staj defteri girişi</param>
    /// <returns>İşlem başarılı mı?</returns>
    public async Task<bool> CreateDiaryEntryAsync(InternshipDocument entry)
    {
        // Null kontrolü
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        // Öğrenci ID kontrolü
        if (entry.Id <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(entry.Id));

        // Dosya adı kontrolü
        if (string.IsNullOrWhiteSpace(entry.FileName))
            throw new ArgumentException("Dosya adı boş olamaz", nameof(entry.FileName));

        // Meta verileri ayarla
        entry.UploadedAt = DateTime.UtcNow;
        entry.DocumentType = "Diary"; // Staj defteri girdisi olarak işaretle
        
        // Veritabanına ekle
        await _unitOfWork.InternshipDocuments.AddAsync(entry);
        await _unitOfWork.SaveAsync();
        
        return true;
    }

    /// <summary>
    /// Öğrencinin staj defteri girişlerini getir (alternatif metod)
    /// </summary>
    /// <param name="studentId">Öğrenci ID'si</param>
    /// <returns>Staj defteri girişleri</returns>
    public Task<List<InternshipDocument>> GetDiaryEntriesByStudentIdAsync(int studentId)
    {
        return _unitOfWork.InternshipDocuments.GetDiaryEntriesByStudentIdAsync(studentId);
    }

    /// <summary>
    /// Onay formuna ait belgeleri getir
    /// Danışman onay sürecinde hangi belgelerin ekli olduğunu görmek için
    /// </summary>
    /// <param name="approvalFormId">Onay formu ID'si</param>
    /// <returns>Onay formuna bağlı belgeler</returns>
    public async Task<List<InternshipDocument>> GetDocumentsByApprovalFormIdAsync(int approvalFormId)
    {
        if (approvalFormId <= 0)
            throw new ArgumentException("Geçersiz onay formu ID", nameof(approvalFormId));
        
        return await _unitOfWork.InternshipDocuments.GetDocumentsByApprovalFormIdAsync(approvalFormId);
    }

    /// <summary>
    /// Bir belgeyi başvuruya bağla
    /// Öğrenci başvuru yaparken belgelerini eklemek için kullanılır
    /// </summary>
    /// <param name="documentId">Belge ID'si</param>
    /// <param name="applicationId">Başvuru ID'si</param>
    public async Task LinkDocumentToApplicationAsync(int documentId, int applicationId)
    {
        // Parametre validasyonları
        if (documentId <= 0)
            throw new ArgumentException("Geçersiz belge ID", nameof(documentId));
        
        if (applicationId <= 0)
            throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

        // Belgeyi veritabanından getir
        var document = await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
        if (document != null)
        {
            // Belgeyi başvuruya bağla
            document.ApplicationId = applicationId;
            _unitOfWork.InternshipDocuments.Update(document);
            await _unitOfWork.SaveAsync();
        }
    }

    /// <summary>
    /// Bir belgenin başvuru ile bağlantısını kaldır
    /// Öğrenci belgesini başvurudan çıkarmak istediğinde kullanılır
    /// </summary>
    /// <param name="documentId">Belge ID'si</param>
    /// <param name="applicationId">Başvuru ID'si</param>
    public async Task UnlinkDocumentFromApplicationAsync(int documentId, int applicationId)
    {
        // Parametre validasyonları
        if (documentId <= 0)
            throw new ArgumentException("Geçersiz belge ID", nameof(documentId));
        
        if (applicationId <= 0)
            throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

        // Belgeyi veritabanından getir
        var document = await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
        
        // Belge var ve doğru başvuruya bağlıysa bağlantıyı kaldır
        if (document != null && document.ApplicationId == applicationId)
        {
            document.ApplicationId = null; // Bağlantıyı kaldır
            _unitOfWork.InternshipDocuments.Update(document);
            await _unitOfWork.SaveAsync();
        }
    }
}