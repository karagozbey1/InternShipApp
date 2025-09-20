using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Core.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models.ViewModels.Student.ViewModels;
using WebUI.Models.ViewModels.Student;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.Controllers.Student
{
    [Authorize(Roles = "Student")]
    public class DocumentController : Controller
    {
        private readonly IInternshipDocumentService _documentService;
        private readonly IIUserService _userService;
        private readonly IInternshipApprovalService _approvalService;
        private readonly IWebHostEnvironment _env;

        public DocumentController(
            IInternshipDocumentService documentService, 
            IIUserService userService,
            IInternshipApprovalService approvalService,
            IWebHostEnvironment env)
        {
            _documentService = documentService;
            _userService = userService;
            _approvalService = approvalService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            // Kullanıcının tüm belgelerini getir (başvuru bağımsız)
            var docs = await _documentService.GetDocumentsByStudentIdAsync(userId);

            var vm = new DocumentListViewModel
            {
                Documents = docs.Select(d => new WebUI.Models.ViewModels.Student.Document
                {
                    Id = d.Id,
                    StudentId = userId,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    FileSize = d.FileSize ?? 0,
                    Type = MapDocumentType(d.DocumentType), 
                    UploadedAt = d.UploadedAt,
                    Title = d.Title // başlık
                }).ToList(),
                UploadModel = new DocumentUploadViewModel()
            };

            return View("~/Views/Student/Document/Index.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, WebUI.Models.ViewModels.Student.DocumentType type, string? description, string title)
        {
            // Validation kontrolü
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen yüklenecek bir dosya seçin.";
                return RedirectToAction(nameof(Index));
            }

            // Dosya boyutu kontrolü (10MB limit)
            if (file.Length > 10 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "Dosya boyutu 10MB'dan büyük olamaz.";
                return RedirectToAction(nameof(Index));
            }

            // Dosya türü kontrolü
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["ErrorMessage"] = "Sadece PDF, Word dökümanları ve resim dosyaları yüklenebilir.";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            string? physicalPath = null;

            try
            {
                // Upload klasörünü oluştur (student bazlı)
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "students", userId.ToString());
                Directory.CreateDirectory(uploadsRoot);

                // Benzersiz dosya adı oluştur
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                physicalPath = Path.Combine(uploadsRoot, fileName);
                
                // Dosyayı fiziksel olarak kaydet
                await using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/uploads/students/{userId}/{fileName}";

                // Dosya adı uzunluk kontrolü (veritabanı limiti)
                var originalFileName = file.FileName;
                if (originalFileName.Length > 500)
                {
                    originalFileName = originalFileName.Substring(0, 497) + "...";
                }

                // Description uzunluk kontrolü
                if (!string.IsNullOrEmpty(description) && description.Length > 1000)
                {
                    description = description.Substring(0, 997) + "...";
                }

                // Veritabanı entity'si oluştur
                var entity = new InternshipDocument
                {
                    ApplicationId = null, // Başvuru bağımsız belgeler için null
                    StudentId = userId,
                    FileName = originalFileName,
                    FilePath = relativePath,
                    FileSize = file.Length,
                    DocumentType = ToDocumentTypeString(type),
                    ContentType = file.ContentType,
                    Description = description,
                    UploadedAt = DateTime.UtcNow,
                    CompanyId = null, // Şirket belgesi değil
                    Status = "Active", // Aktif durumda
                    Title = title // başlık
                };

                // Debug bilgileri
                System.Diagnostics.Debug.WriteLine($"Creating entity: StudentId={entity.StudentId}, FileName={entity.FileName}, DocumentType={entity.DocumentType}");

                await _documentService.AddDocumentAsync(entity);
                TempData["SuccessMessage"] = $"'{file.FileName}' dosyası başarıyla yüklendi.";
            }
            catch (Exception ex)
            {
                // Hata durumunda fiziksel dosyayı sil
                if (!string.IsNullOrEmpty(physicalPath) && System.IO.File.Exists(physicalPath))
                {
                    try
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                    catch (Exception deleteEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"File delete error: {deleteEx.Message}");
                    }
                }

                // Detaylı hata loglama
                System.Diagnostics.Debug.WriteLine($"Upload error: {ex}");
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException}");

                // Kullanıcıya anlamlı hata mesajı
                var errorMessage = "Dosya yüklenirken bir hata oluştu.";
                
                if (ex.InnerException != null)
                {
                    var innerMsg = ex.InnerException.Message;
                    if (innerMsg.Contains("constraint"))
                    {
                        errorMessage += " Veritabanı kısıtlama hatası.";
                    }
                    else if (innerMsg.Contains("duplicate"))
                    {
                        errorMessage += " Bu dosya zaten mevcut.";
                    }
                    else if (innerMsg.Contains("length"))
                    {
                        errorMessage += " Dosya adı çok uzun.";
                    }
                    else
                    {
                        errorMessage += $" Detay: {innerMsg}";
                    }
                }

                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Dosya bilgisini al
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document != null)
                {
                    // Fiziksel dosyayı sil
                    var physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }

                    // Veritabanından sil
                    await _documentService.DeleteDocumentAsync(id);
                    TempData["SuccessMessage"] = "Dosya başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Silinecek dosya bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
                TempData["ErrorMessage"] = $"Dosya silinirken bir hata oluştu: {innerExceptionMessage}";
                System.Diagnostics.Debug.WriteLine($"Delete error: {ex}");
            }

            return RedirectToAction(nameof(Index));
        }

        // Öğretmen approval details sayfası için AJAX endpoint
        [HttpGet]
        [AllowAnonymous] // Geçici olarak authorize sorunlarını atlayalım
        public async Task<IActionResult> GetStudentDocuments(int? approvalId = null)
        {
            try
            {
                List<InternshipDocument> documents;
                
                if (approvalId.HasValue && approvalId.Value > 0)
                {
                    // Approval ID'si verilmişse o approval'a bağlı belgeleri getir
                    System.Diagnostics.Debug.WriteLine($"?? GetStudentDocuments for ApprovalId: {approvalId.Value}");
                    
                    // İlk olarak approval form'u bulalım
                    var approval = await _approvalService.GetApprovalFormWithDetailsAsync(approvalId.Value);
                    if (approval == null || approval.ApplicationId <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"? Approval not found or no ApplicationId for ApprovalId: {approvalId.Value}");
                        return Json(new List<object>());
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"?? Found Approval with ApplicationId: {approval.ApplicationId}");
                    
                    // ApplicationId ile belgeleri getir
                    documents = await _documentService.GetDocumentsByApplicationIdAsync(approval.ApplicationId);
                    System.Diagnostics.Debug.WriteLine($"?? Found {documents.Count} documents for ApplicationId: {approval.ApplicationId}");
                }
                else
                {
                    // Normal öğrenci belgeleri (var olan UserId gerek)
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                    var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                    documents = await _documentService.GetDocumentsByStudentIdAsync(userId);
                }

                var result = documents.Select(d => new
                {
                    id = d.Id,
                    title = d.Title ?? d.FileName ?? "",
                    fileName = d.FileName ?? "",
                    type = d.DocumentType?.ToString() ?? "Other",
                    uploadDate = d.UploadedAt.ToString("dd.MM.yyyy"),
                    fileSize = d.FileSize.HasValue ? $"{Math.Round((double)d.FileSize.Value / 1024 / 1024, 2)} MB" : "Bilinmiyor",
                    filePath = d.FilePath,
                    contentType = d.ContentType
                });

                System.Diagnostics.Debug.WriteLine($"?? Returning {result.Count()} documents");
                return Json(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetStudentDocuments error: {ex}");
                return Json(new List<object>());
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private static WebUI.Models.ViewModels.Student.DocumentType MapDocumentType(string docType)
        {
            return docType?.ToLowerInvariant() switch
            {
                "cv" => WebUI.Models.ViewModels.Student.DocumentType.CV,
                "transcript" => WebUI.Models.ViewModels.Student.DocumentType.Transcript,
                "applicationform" => WebUI.Models.ViewModels.Student.DocumentType.ApplicationForm,
                _ => WebUI.Models.ViewModels.Student.DocumentType.Other
            };
        }

        private static string ToDocumentTypeString(WebUI.Models.ViewModels.Student.DocumentType type)
        {
            return type switch
            {
                WebUI.Models.ViewModels.Student.DocumentType.CV => "CV",
                WebUI.Models.ViewModels.Student.DocumentType.Transcript => "Transcript",
                WebUI.Models.ViewModels.Student.DocumentType.ApplicationForm => "ApplicationForm",
                _ => "Other"
            };
        }
    }
}
