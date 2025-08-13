using System;

namespace Core.Entity
{
    public class InternshipDocument
    {
        public int Id { get; set; }

        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string DocumentType { get; set; } = null!; // Bu satırı ekledik
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int ApplicationId { get; set; }
        public InternshipApplication InternshipApplication { get; set; } = null!;
        public string ?Status { get; set; }
    }
}