using System;

namespace Core.Entity
{
    // Mesaj eki
    public class MessageAttachment
    {
        // Kimlik numarasý
        public int Id { get; set; }
        
        // Mesaj kimliði
        public int ChatMessageId { get; set; }
        
        // Mesaj
        public ChatMessage ChatMessage { get; set; } = null!;
        
        // Dosya adý
        public string FileName { get; set; } = null!;
        
        // Orijinal dosya adý
        public string OriginalFileName { get; set; } = null!;
        
        // Dosya yolu
        public string FilePath { get; set; } = null!;
        
        // Ýçerik türü
        public string ContentType { get; set; } = null!;
        
        // Dosya boyutu
        public long FileSize { get; set; }
        
        // Dosya türü
        public string FileType { get; set; } = null!;
        
        // Yüklenme zamaný
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        // Silinmiþ mi
        public bool IsDeleted { get; set; } = false;
        
        // Geniþlik
        public int? Width { get; set; }
        
        // Yükseklik
        public int? Height { get; set; }
        
        // Küçük resim yolu
        public string? ThumbnailPath { get; set; }
        
        // Sayfa sayýsý
        public int? PageCount { get; set; }
        
        // Süre (saniye)
        public int? Duration { get; set; }
    }
}