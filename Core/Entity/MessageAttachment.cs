using System;

namespace Core.Entity
{
    public class MessageAttachment
    {
        public int Id { get; set; }
        public int ChatMessageId { get; set; }
        public ChatMessage ChatMessage { get; set; } = null!;
        
        public string FileName { get; set; } = null!;
        public string OriginalFileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSize { get; set; }
        public string FileType { get; set; } = null!; // Image, Document, Video, Audio, Other
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        
        // Image specific properties
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? ThumbnailPath { get; set; }
        
        // Document specific properties
        public int? PageCount { get; set; }
        
        // Video/Audio specific properties
        public int? Duration { get; set; } // in seconds
    }
}