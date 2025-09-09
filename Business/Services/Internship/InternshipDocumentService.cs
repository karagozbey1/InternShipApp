using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;


namespace Business.Services.Internship;

public class InternshipDocumentService : IInternshipDocumentService
{
    private readonly IUnitOfWork _unitOfWork;

    public InternshipDocumentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<InternshipDocument?> GetDocumentByIdAsync(int documentId)
    {
        return await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
    }

    public async Task<List<InternshipDocument>> GetDocumentsByApplicationIdAsync(int applicationId)
    {
        return await _unitOfWork.InternshipDocuments.GetDocumentsByApplicationIdAsync(applicationId);
    }

    public async Task<List<InternshipDocument>> GetDocumentsByStudentIdAsync(int studentId)
    {
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        return await _unitOfWork.InternshipDocuments.GetDocumentsByStudentIdAsync(studentId);
    }

    public async Task<List<InternshipDocument>> GetIndependentDocumentsByStudentIdAsync(int studentId)
    {
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        return await _unitOfWork.InternshipDocuments.GetIndependentDocumentsByStudentIdAsync(studentId);
    }

    public async Task LinkDocumentToApprovalAsync(int documentId, int approvalFormId)
    {
        if (documentId <= 0)
            throw new ArgumentException("Geçersiz belge ID", nameof(documentId));
        
        if (approvalFormId <= 0)
            throw new ArgumentException("Geçersiz onay formu ID", nameof(approvalFormId));

        await _unitOfWork.InternshipDocuments.LinkDocumentToApprovalAsync(documentId, approvalFormId);
        await _unitOfWork.SaveAsync();
    }

    public async Task<InternshipDocument> AddDocumentAsync(InternshipDocument document)
    {
        // Validation kontrolleri
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        if (string.IsNullOrWhiteSpace(document.FileName))
            throw new ArgumentException("Dosya adı boş olamaz", nameof(document.FileName));

        if (string.IsNullOrWhiteSpace(document.FilePath))
            throw new ArgumentException("Dosya yolu boş olamaz", nameof(document.FilePath));

        if (string.IsNullOrWhiteSpace(document.DocumentType))
            throw new ArgumentException("Dosya türü boş olamaz", nameof(document.DocumentType));

        // Dosya adı uzunlık kontrolü
        if (document.FileName.Length > 500)
            throw new ArgumentException("Dosya adı çok uzun (max 500 karakter)", nameof(document.FileName));

        // FilePath uzunluk kontrolü
        if (document.FilePath.Length > 1000)
            throw new ArgumentException("Dosya yolu çok uzun (max 1000 karakter)", nameof(document.FilePath));

        // DocumentType uzunluk kontrolü
        if (document.DocumentType.Length > 100)
            throw new ArgumentException("Dosya türü çok uzun (max 100 karakter)", nameof(document.DocumentType));

        // Description uzunluk kontrolü
        if (!string.IsNullOrEmpty(document.Description) && document.Description.Length > 1000)
            throw new ArgumentException("Açıklama çok uzun (max 1000 karakter)", nameof(document.Description));

        // ContentType uzunluk kontrolü
        if (!string.IsNullOrEmpty(document.ContentType) && document.ContentType.Length > 200)
            throw new ArgumentException("Content type çok uzun (max 200 karakter)", nameof(document.ContentType));

        // Status uzunluk kontrolü
        if (!string.IsNullOrEmpty(document.Status) && document.Status.Length > 50)
            throw new ArgumentException("Status çok uzun (max 50 karakter)", nameof(document.Status));

        // En az bir ID olmalı (StudentId, ApplicationId veya CompanyId)
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
            // Log detailed error
            System.Diagnostics.Debug.WriteLine($"AddDocumentAsync error: {ex}");
            throw new InvalidOperationException("Belge veritabanına kaydedilirken bir hata oluştu.", ex);
        }
    }

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
            System.Diagnostics.Debug.WriteLine($"UpdateDocumentAsync error: {ex}");
            throw new InvalidOperationException("Belge güncellenirken bir hata oluştu.", ex);
        }
    }

    public async Task DeleteDocumentAsync(int documentId)
    {
        var document = await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
        if (document != null)
        {
            _unitOfWork.InternshipDocuments.Remove(document);
            await _unitOfWork.SaveAsync();
        }
    }

    public async Task<List<InternshipDocument>> GetCompanyDocumentsAsync(int companyId)
    {
        if (companyId <= 0)
            throw new ArgumentException("Geçersiz şirket ID", nameof(companyId));

        return await _unitOfWork.InternshipDocuments.GetCompanyDocumentsAsync(companyId);

    }


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

    public async Task<List<InternshipDocument>> GetStudentDiaryEntriesAsync(int studentId)
    {
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        
        return await _unitOfWork.InternshipDocuments
            .GetDiaryEntriesByStudentIdAsync(studentId);
    }

    public async Task<bool> CreateDiaryEntryAsync(InternshipDocument entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        if (entry.Id <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(entry.Id));

        if (string.IsNullOrWhiteSpace(entry.FileName))
            throw new ArgumentException("Dosya adı boş olamaz", nameof(entry.FileName));

        entry.UploadedAt = DateTime.UtcNow;
        entry.DocumentType = "Diary"; // staj defteri girdisi olarak işaretliyoruz
        await _unitOfWork.InternshipDocuments.AddAsync(entry);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public Task<List<InternshipDocument>> GetDiaryEntriesByStudentIdAsync(int studentId)
    {
        return _unitOfWork.InternshipDocuments.GetDiaryEntriesByStudentIdAsync(studentId);
    }

    // Yeni: Onay formu belgeleri
    public async Task<List<InternshipDocument>> GetDocumentsByApprovalFormIdAsync(int approvalFormId)
    {
        if (approvalFormId <= 0)
            throw new ArgumentException("Geçersiz onay formu ID", nameof(approvalFormId));
        return await _unitOfWork.InternshipDocuments.GetDocumentsByApprovalFormIdAsync(approvalFormId);
    }
}