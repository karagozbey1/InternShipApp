using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;


namespace Business.Services.Internship;

// Staj belgeleri yönetim servisi - document management ve file handling
// Öğrenci belgelerinin yüklenmesi, güncellenmesi, silinmesi işlemlerini koordine eder
// File system operations ile database consistency'sini sağlar
// Multi-entity document support: Student, Company, Application bazlı dosya yönetimi
public class InternshipDocumentService : IInternshipDocumentService
{
    // Unit of Work pattern - transaction yönetimi ve repository koordinasyonu
    // File operations ile database operations'ın atomik yapılması için kritik
    private readonly IUnitOfWork _unitOfWork;

    // Dependency injection constructor - service lifetime management
    // Scoped registration ile HttpContext bazında instance control
    public InternshipDocumentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ID ile tek belge getirme - file detail view ve download operations için
    // Null-safe return - calling code'da existence check gerekli
    // Primary key lookup - en performanslı query tipi
    public async Task<InternshipDocument?> GetDocumentByIdAsync(int documentId)
    {
        // Input validation repository seviyesinde yapılır
        // Entity framework change tracking ile memory'de cache'lenir
        return await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
    }

    // Başvuruya bağlı belgeleri getirme - application detail sayfası için
    // Foreign key relationship ile filtered query
    // Include operations ile related data loading possibility
    public async Task<List<InternshipDocument>> GetDocumentsByApplicationIdAsync(int applicationId)
    {
        // WHERE ApplicationId = @applicationId query
        // Index usage için ApplicationId column'ında index önerilisr
        return await _unitOfWork.InternshipDocuments.GetDocumentsByApplicationIdAsync(applicationId);
    }

    // Öğrenciye ait tüm belgeleri getirme - student document management
    // Personal document library ve file history için kullanılır
    // Pagination wrapper ile kullanılması large datasets için önerilisr
    public async Task<List<InternshipDocument>> GetDocumentsByStudentIdAsync(int studentId)
    {
        // Parameter validation - defensive programming approach
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        // Student ownership based filtering
        // Chronological ordering (UploadedAt DESC) ile son yüklenenler önce
        return await _unitOfWork.InternshipDocuments.GetDocumentsByStudentIdAsync(studentId);
    }

    // Öğrencinin genel belgelerini getirme - başvuru-bağımsız dosyalar
    // CV, transcript, certificate gibi reusable documents için
    // Application linking öncesi available documents listesi
    public async Task<List<InternshipDocument>> GetIndependentDocumentsByStudentIdAsync(int studentId)
    {
        // Input validation consistency
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        // WHERE StudentId = @studentId AND ApplicationId IS NULL query
        // Independent documents - reusable across multiple applications
        return await _unitOfWork.InternshipDocuments.GetIndependentDocumentsByStudentIdAsync(studentId);
    }

    // Belgeyi onay formuna bağlama - approval workflow integration
    // Danışman onay sürecinde document selection için kullanılır
    // Many-to-many relationship management through junction operations
    public async Task LinkDocumentToApprovalAsync(int documentId, int approvalFormId)
    {
        // Parameter validation - business rule enforcement
        if (documentId <= 0)
            throw new ArgumentException("Geçersiz belge ID", nameof(documentId));
        
        if (approvalFormId <= 0)
            throw new ArgumentException("Geçersiz onay formu ID", nameof(approvalFormId));

        // Repository method ile junction table management
        // Many-to-many relationship implementation
        await _unitOfWork.InternshipDocuments.LinkDocumentToApprovalAsync(documentId, approvalFormId);
        await _unitOfWork.SaveAsync();
    }

    // Yeni belge ekleme - file upload completion sonrası database registration
    // File system storage ile database record sync işlemi
    // Comprehensive validation ile data integrity assurance
    public async Task<InternshipDocument> AddDocumentAsync(InternshipDocument document)
    {
        // Null protection - defensive programming
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        // Required field validations - business rule enforcement
        if (string.IsNullOrWhiteSpace(document.FileName))
            throw new ArgumentException("Dosya adı boş olamaz", nameof(document.FileName));

        if (string.IsNullOrWhiteSpace(document.FilePath))
            throw new ArgumentException("Dosya yolu boş olamaz", nameof(document.FilePath));

        if (string.IsNullOrWhiteSpace(document.DocumentType))
            throw new ArgumentException("Dosya türü boş olamaz", nameof(document.DocumentType));

        // Database column length validations - constraint violation prevention
        // Prevents SQL exception ve improves user experience
        if (document.FileName.Length > 500)
            throw new ArgumentException("Dosya adı çok uzun (max 500 karakter)", nameof(document.FileName));

        if (document.FilePath.Length > 1000)
            throw new ArgumentException("Dosya yolu çok uzun (max 1000 karakter)", nameof(document.FilePath));

        if (document.DocumentType.Length > 100)
            throw new ArgumentException("Dosya türü çok uzun (max 100 karakter)", nameof(document.DocumentType));

        // Optional field length validations - consistency maintenance
        if (!string.IsNullOrEmpty(document.Description) && document.Description.Length > 1000)
            throw new ArgumentException("Açıklama çok uzun (max 1000 karakter)", nameof(document.Description));

        if (!string.IsNullOrEmpty(document.ContentType) && document.ContentType.Length > 200)
            throw new ArgumentException("Content type çok uzun (max 200 karakter)", nameof(document.ContentType));

        if (!string.IsNullOrEmpty(document.Status) && document.Status.Length > 50)
            throw new ArgumentException("Status çok uzun (max 50 karakter)", nameof(document.Status));

        // Ownership validation - business rule: her document bir entity'ye ait olmalı
        // StudentId, ApplicationId veya CompanyId'den en az biri set olmalı
        if (!document.StudentId.HasValue && !document.ApplicationId.HasValue && !document.CompanyId.HasValue)
            throw new ArgumentException("En az bir ID (StudentId, ApplicationId veya CompanyId) belirtilmelidir");

        try
        {
            // Timestamp assignment - audit trail için kritik
            document.UploadedAt = DateTime.UtcNow;
            
            // Repository pattern ile entity addition
            await _unitOfWork.InternshipDocuments.AddAsync(document);
            await _unitOfWork.SaveAsync();
            
            return document; // Auto-generated ID ile return
        }
        catch (Exception ex)
        {
            // Database error wrapping - user-friendly error messaging
            // Original exception preserved for debugging
            throw new InvalidOperationException("Belge veritabanına kaydedilirken bir hata oluştu.", ex);
        }
    }

    // Belge metadata güncelleme - file properties modification
    // Physical file değişmez, sadece database record'u update edilir
    // Title, description, status gibi meta fields için
    public async Task UpdateDocumentAsync(InternshipDocument document)
    {
        // Null safety check
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        try
        {
            // Change tracking ile modified properties detection
            _unitOfWork.InternshipDocuments.Update(document);
            await _unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            // Exception wrapping ile meaningful error message
            throw new InvalidOperationException("Belge güncellenirken bir hata oluştu.", ex);
        }
    }

    // Belge silme - both database record ve physical file cleanup
    // Two-phase deletion: database removal then file system cleanup
    // Orphaned file prevention için dikkatli implementation gerekli
    public async Task DeleteDocumentAsync(int documentId)
    {
        // Entity existence check - null-safe deletion
        var document = await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
        if (document != null)
        {
            // Database record removal - foreign key constraints handled
            _unitOfWork.InternshipDocuments.Remove(document);
            await _unitOfWork.SaveAsync();
            
            // Physical file cleanup notification
            // Actual file deletion burada yapılabilir veya background service'e delegate edilebilir
            // File.Delete(document.FilePath) - filesystem operation
            // Error handling gerekli: file may be locked, permissions, etc.
        }
    }

    // Şirket belgelerini getirme - company document management
    // Corporate documents, contracts, certificates için
    // Company profile ve legal document storage
    public async Task<List<InternshipDocument>> GetCompanyDocumentsAsync(int companyId)
    {
        // Parameter validation consistency
        if (companyId <= 0)
            throw new ArgumentException("Geçersiz şirket ID", nameof(companyId));

        // Company ownership filtering
        // WHERE CompanyId = @companyId query
        return await _unitOfWork.InternshipDocuments.GetCompanyDocumentsAsync(companyId);
    }

    // Şirket adına belge ekleme - corporate document upload
    // Company contracts, policies, legal documents için
    // Automatic path generation ile organized file structure
    public async Task AddCompanyDocumentAsync(int companyId, string fileName, string description)
    {
        // Parameter validation - required fields
        if (companyId <= 0)
            throw new ArgumentException("Geçersiz şirket ID", nameof(companyId));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Dosya adı boş olamaz", nameof(fileName));

        // Company existence validation - referential integrity
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new InvalidOperationException("Şirket bulunamadı");

        // File extension extraction - type determination
        var fileExtension = Path.GetExtension(fileName)?.TrimStart('.');
        
        // Entity construction - organized file structure
        var doc = new InternshipDocument
        {
            CompanyId = companyId,
            FileName = fileName,
            // Hierarchical folder structure: uploads/companies/{companyId}/filename
            FilePath = Path.Combine("uploads", "companies", companyId.ToString(), fileName),
            ContentType = "application/octet-stream", // Default MIME type
            Description = description,
            UploadedAt = DateTime.UtcNow,
            DocumentType = fileExtension ?? string.Empty // File type classification
        };

        // Database persistence
        await _unitOfWork.InternshipDocuments.AddAsync(doc);
        await _unitOfWork.SaveAsync();
    }

    // Staj defteri girişlerini getirme - internship diary entries
    // Daily activity reports, progress tracking için
    // Student's internship journey documentation
    public async Task<List<InternshipDocument>> GetStudentDiaryEntriesAsync(int studentId)
    {
        // Input validation
        if (studentId <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(studentId));

        // Specialized repository method - diary type filtering
        // WHERE StudentId = @studentId AND DocumentType = 'Diary'
        return await _unitOfWork.InternshipDocuments
            .GetDiaryEntriesByStudentIdAsync(studentId);
    }

    // Yeni staj defteri girişi oluşturma - daily progress entry
    // Student activity logging ve progress tracking
    // Structured diary entry creation ile consistent format
    public async Task<bool> CreateDiaryEntryAsync(InternshipDocument entry)
    {
        // Null safety
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        // ID validation - student ownership requirement
        if (entry.Id <= 0)
            throw new ArgumentException("Geçersiz öğrenci ID", nameof(entry.Id));

        // Required field validation
        if (string.IsNullOrWhiteSpace(entry.FileName))
            throw new ArgumentException("Dosya adı boş olamaz", nameof(entry.FileName));

        // Metadata standardization - consistent diary entry format
        entry.UploadedAt = DateTime.UtcNow;
        entry.DocumentType = "Diary"; // Type classification for filtering
        
        // Persistence operation
        await _unitOfWork.InternshipDocuments.AddAsync(entry);
        await _unitOfWork.SaveAsync();
        
        return true; // Success indicator
    }

    // Staj defteri girişleri - alternative method signature
    // Method overloading ile API flexibility
    // Delegate pattern ile code duplication prevention
    public Task<List<InternshipDocument>> GetDiaryEntriesByStudentIdAsync(int studentId)
    {
        // Direct repository call - no additional business logic
        // Thin service layer approach
        return _unitOfWork.InternshipDocuments.GetDiaryEntriesByStudentIdAsync(studentId);
    }

    // Onay formuna ait belgeleri getirme - approval process document listing
    // Danışman onay sürecinde attached documents review için
    // Document verification ve approval workflow support
    public async Task<List<InternshipDocument>> GetDocumentsByApprovalFormIdAsync(int approvalFormId)
    {
        // Parameter validation
        if (approvalFormId <= 0)
            throw new ArgumentException("Geçersiz onay formu ID", nameof(approvalFormId));
        
        // Junction table query - many-to-many relationship resolution
        // ApprovalForm-Document relationship navigation
        return await _unitOfWork.InternshipDocuments.GetDocumentsByApprovalFormIdAsync(approvalFormId);
    }

    // Belgeyi başvuruya bağlama - application document attachment
    // Student application process'inde document inclusion
    // Foreign key relationship establishment
    public async Task LinkDocumentToApplicationAsync(int documentId, int applicationId)
    {
        // Parameter validation
        if (documentId <= 0)
            throw new ArgumentException("Geçersiz belge ID", nameof(documentId));
        
        if (applicationId <= 0)
            throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

        // Entity retrieval ve relationship update
        var document = await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
        if (document != null)
        {
            // Foreign key assignment - relationship establishment
            document.ApplicationId = applicationId;
            _unitOfWork.InternshipDocuments.Update(document);
            await _unitOfWork.SaveAsync();
        }
    }

    // Belge-başvuru bağlantısını kaldırma - document detachment
    // Application modification process'inde document removal
    // Foreign key nullification ile relationship termination
    public async Task UnlinkDocumentFromApplicationAsync(int documentId, int applicationId)
    {
        // Parameter validation
        if (documentId <= 0)
            throw new ArgumentException("Geçersiz belge ID", nameof(documentId));
        
        if (applicationId <= 0)
            throw new ArgumentException("Geçersiz başvuru ID", nameof(applicationId));

        // Entity retrieval ve relationship verification
        var document = await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
        
        // Conditional unlinking - safety check for correct relationship
        // Only unlink if document is actually linked to specified application
        if (document != null && document.ApplicationId == applicationId)
        {
            document.ApplicationId = null; // Relationship termination
            _unitOfWork.InternshipDocuments.Update(document);
            await _unitOfWork.SaveAsync();
        }
    }
}