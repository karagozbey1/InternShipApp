using Business.Interfaces.Internship;
using Core.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WebUI.Controllers
{
    [Route("Document")]
    public class DocumentApiController : Controller
    {
        private readonly IInternshipDocumentService _documentService;
        private readonly IInternshipApprovalService _approvalService;

        public DocumentApiController(
            IInternshipDocumentService documentService,
            IInternshipApprovalService approvalService)
        {
            _documentService = documentService;
            _approvalService = approvalService;
        }

        // Öðretmen approval details sayfasý için AJAX endpoint
        [HttpGet]
        [Route("GetStudentDocuments")]
        public async Task<IActionResult> GetStudentDocuments(int? approvalId = null)
        {
            try
            {
                List<InternshipDocument> documents;
                
                if (approvalId.HasValue && approvalId.Value > 0)
                {
                    // Approval ID'si verilmiþse o approval'a baðlý belgeleri getir
                    System.Diagnostics.Debug.WriteLine($"?? GetStudentDocuments for ApprovalId: {approvalId.Value}");
                    
                    // Ýlk olarak approval form'u bulalým
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
                    // Normal öðrenci belgeleri (mevcut login gerek)
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                    var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                    if (userId > 0)
                    {
                        documents = await _documentService.GetDocumentsByStudentIdAsync(userId);
                    }
                    else
                    {
                        return Json(new List<object>());
                    }
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
    }
}