using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;


namespace Business.Services.Internship;

// Belge yönetim servisi
public class InternshipDocumentService : IInternshipDocumentService
{
    // Veritabanı işlemleri
    private readonly IUnitOfWork _unitOfWork;

    // Kurucu metod
    public InternshipDocumentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Kimlik ile belge getirir
    public async Task<InternshipDocument?> GetDocumentByIdAsync(int documentId)
    {
        return await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
    }

    // Başvuru belgelerini getirir
    public async Task<List<InternshipDocument>> GetDocumentsByApplicationIdAsync(int applicationId)
    {
        return await _unitOfWork.InternshipDocuments.GetDocumentsByApplicationIdAsync(applicationId);
    }

    // Öğrenci belgelerini getirir
    public async Task<List<InternshipDocument>> GetDocumentsByStudentIdAsync(int studentId)
    {
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        return await _unitOfWork.InternshipDocuments.GetDocumentsByStudentIdAsync(studentId);
    }

    // Genel öğrenci belgelerini getirir
    public async Task<List<InternshipDocument>> GetIndependentDocumentsByStudentIdAsync(int studentId)
    {
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        return await _unitOfWork.InternshipDocuments.GetIndependentDocumentsByStudentIdAsync(studentId);
    }

    // Belgeyi onay formuna bağlar
    public async Task LinkDocumentToApprovalAsync(int documentId, int approvalFormId)
    {
        if (documentId <= 0)
            throw new ArgumentException("Geçersiz belge ID", nameof(documentId));
        
        if (approvalFormId <= 0)
            throw new ArgumentException("Geçersiz onay formu ID", nameof(approvalFormId));

        await _unitOfWork.InternshipDocuments.LinkDocumentToApprovalAsync(documentId, approvalFormId);
        await _unitOfWork.SaveAsync();
    }

    // Yeni belge ekler
    public async Task<InternshipDocument> AddDocumentAsync(InternshipDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        if (string.IsNullOrWhiteSpace(document.FileName))
            throw new ArgumentException("Dosya adı boş olamaz", nameof(document.FileName));

        if (string.IsNullOrWhiteSpace(document.FilePath))
            throw new ArgumentException("Dosya yolu boş olamaz", nameof(document.FilePath));

        if (string.IsNullOrWhiteSpace(document.DocumentType))
            throw new ArgumentException("Dosya türü boş olamaz", nameof(document.DocumentType));

        if (document.FileName.Length > 500)
            throw new ArgumentException("Dosya adı çok uzun (max 500 karakter)", nameof(document.FileName));

        if (document.FilePath.Length > 1000)
            throw new ArgumentException("Dosya yolu çok uzun (max 1000 karakter)", nameof(document.FilePath));

        if (document.DocumentType.Length > 100)
            throw new ArgumentException("Dosya türü çok uzun (max 100 karakter)", nameof(document.DocumentType));

        if (!string.IsNullOrEmpty(document.Description) && document.Description.Length > 1000)
            throw new ArgumentException("Açıklama çok uzun (max 1000 karakter)", nameof(document.Description));

        if (!string.IsNullOrEmpty(document.ContentType) && document.ContentType.Length > 200)
            throw new ArgumentException("Content type çok uzun (max 200 karakter)", nameof(document.ContentType));

        if (!string.IsNullOrEmpty(document.Status) && document.Status.Length > 50)
            throw new ArgumentException("Status çok uzun (max 50 karakter)", nameof(document.Status));

        if (!document.StudentId.HasValue && !document.ApplicationId.HasValue && !document.CompanyId.HasValue)
            throw new ArgumentException("En az bir ID (StudentId, ApplicationId veya CompanyId) belirtilmelidir");

        try
        {
            document.UploadedAt = DateTime.UtcNow;
            
            await _unitOfWork.InternshipDocuments.AddAsync(document);
            await _unitOfWork.SaveAsync();
            
            return document;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Belge veritabanına kaydedilirken bir hata oluştu.", ex);
        }
    }

    // Belge günceller
    public async Task UpdateDocumentAsync(InternshipDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        try
        {
            _unitOfWork.InternshipDocuments.Update(document);
            await _unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Belge güncellenirken bir hata oluştu.", ex);
        }
    }

    // Belge siler
    public async Task DeleteDocumentAsync(int documentId)
    {
        var document = await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
        if (document != null)
        {
            _unitOfWork.InternshipDocuments.Remove(document);
            await _unitOfWork.SaveAsync();
        }
    }

    // Şirket belgelerini getirir
    public async Task<List<InternshipDocument>> GetCompanyDocumentsAsync(int companyId)
    {
        if (companyId <= 0)
            throw new ArgumentException("Geçersiz şirket ID", nameof(companyId));

        return await _unitOfWork.InternshipDocuments.GetCompanyDocumentsAsync(companyId);
    }

    // Şirket belgesi ekler
    public async Task AddCompanyDocumentAsync(int companyId, string fileName, string description)
    {
        if (companyId <= 0)
            throw new ArgumentException("Geçersiz şirket ID", nameof(companyId));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Dosya adı boş olamaz", nameof(fileName));

        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new InvalidOperationException("Şirket bulunamadı");

        var fileExtension = Path.GetExtension(fileName)?.TrimStart('.');
        
        var doc = new InternshipDocument
        {
            CompanyId = companyId,
            FileName = fileName,
            FilePath = Path.Combine("uploads", "companies", companyId.ToString(), fileName),
            ContentType = "application/octet-stream",
            Description = description,
            UploadedAt = DateTime.UtcNow,
            DocumentType = fileExtension ?? string.Empty
        };

        await _unitOfWork.InternshipDocuments.AddAsync(doc);
        await _unitOfWork.SaveAsync();
    }

    // Defter girişlerini getirir
    public async Task<List<InternshipDocument>> GetStudentDiaryEntriesAsync(int studentId)
    {
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        return await _unitOfWork.InternshipDocuments
            .GetDiaryEntriesByStudentIdAsync(studentId);
    }

    // Defter girişi oluşturur
    public async Task<bool> CreateDiaryEntryAsync(InternshipDocument entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        if (entry.Id <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(entry.Id));

        if (string.IsNullOrWhiteSpace(entry.FileName))
            throw new ArgumentException("Dosya adı boş olamaz", nameof(entry.FileName));

        entry.UploadedAt = DateTime.UtcNow;
        entry.DocumentType = "Diary";
        
        await _unitOfWork.InternshipDocuments.AddAsync(entry);
        await _unitOfWork.SaveAsync();
        
        return true;
    }

    // Defter girişlerini getirir
    public Task<List<InternshipDocument>> GetDiaryEntriesByStudentIdAsync(int studentId)
    {
        return _unitOfWork.InternshipDocuments.GetDiaryEntriesByStudentIdAsync(studentId);
    }

    // Onay formu belgelerini getirir
    public async Task<List<InternshipDocument>> GetDocumentsByApprovalFormIdAsync(int approvalFormId)
    {
        if (approvalFormId <= 0)
            throw new ArgumentException("Geçersiz onay formu ID", nameof(approvalFormId));
        
        return await _unitOfWork.InternshipDocuments.GetDocumentsByApprovalFormIdAsync(approvalFormId);
    }

    // Belgeyi başvuruya bağlar
    public async Task LinkDocumentToApplicationAsync(int documentId, int applicationId)
    {
        if (documentId <= 0)
            throw new ArgumentException("Geçersiz belge ID", nameof(documentId));
        
        if (applicationId <= 0)
            throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

        var document = await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
        if (document != null)
        {
            document.ApplicationId = applicationId;
            _unitOfWork.InternshipDocuments.Update(document);
            await _unitOfWork.SaveAsync();
        }
    }

    // Belge bağlantısını kaldırır
    public async Task UnlinkDocumentFromApplicationAsync(int documentId, int applicationId)
    {
        if (documentId <= 0)
            throw new ArgumentException("Geçersiz belge ID", nameof(documentId));
        
        if (applicationId <= 0)
            throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

        var document = await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
        
        if (document != null && document.ApplicationId == applicationId)
        {
            document.ApplicationId = null;
            _unitOfWork.InternshipDocuments.Update(document);
            await _unitOfWork.SaveAsync();
        }
    }
}