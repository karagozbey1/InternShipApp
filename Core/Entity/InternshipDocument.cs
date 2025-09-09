using System;

namespace Core.Entity;

public class InternshipDocument
{

    public int Id { get; set; }
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public string DocumentType { get; set; } = null!;
    public string? ContentType { get; set; } 
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    // Yeni eklenen alan
    public string? Title { get; set; } // Belge başlık adı
    // Başvuru ile ilişkili belgeler için (nullable - başvuru bağımsız belgeler için null)
    public int? ApplicationId { get; set; }
    public InternshipApplication? InternshipApplication { get; set; }
    // Şirket belgeleri için (nullable)
    public int? CompanyId { get; set; } 
    public Company? Company { get; set; } 
    // Öğrenci ile direkt ilişki (başvuru bağımsız belgeler için)
    public int? StudentId { get; set; }
    public User? Student { get; set; }
    public string? Status { get; set; }
    public long? FileSize { get; set; }

    // Yeni: onay formu ile ilişki
    public int? ApprovalFormId { get; set; }
    public InternshipApprovalForm? ApprovalForm { get; set; }
}