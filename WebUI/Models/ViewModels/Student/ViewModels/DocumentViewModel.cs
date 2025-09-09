using System.ComponentModel.DataAnnotations;
using WebUI.Models.ViewModels.Student;

namespace WebUI.Models.ViewModels.Student.ViewModels
{
    public class DocumentListViewModel
    {
        public List<Document> Documents { get; set; } = new();
        public DocumentUploadViewModel UploadModel { get; set; } = new();
    }

    public class DocumentUploadViewModel
    {
        [Required(ErrorMessage = "Dosya seçimi zorunludur")]
        public IFormFile? File { get; set; }

        [Required(ErrorMessage = "Belge türü seçimi zorunludur")]
        public DocumentType Type { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Baþlýk zorunludur")]
        [StringLength(200, ErrorMessage = "Baþlýk en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty;
    }
}
