using Core.Entity;

namespace WebUI.Models.ViewModels.Student
{
    public enum DocumentType
    {
        CV,
        Transcript,
        ApplicationForm,
        Other
    }

    public class Document
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public long FileSize { get; set; }
        public DocumentType Type { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? Title { get; set; }
    }
}