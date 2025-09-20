using System;

namespace Core.Entity
{
    // Defter girişi
    public class DiaryEntry
    {
        // Kimlik numarası
        public int Id { get; set; }
        
        // Öğrenci kimliği
        public int StudentId { get; set; }
        
        // Öğrenci
        public User? Student { get; set; }

        // Defter kimliği
        public int DiaryId { get; set; }
        
        // Defter
        public InternshipDiary? Diary { get; set; }

        // Başlık
        public string Title { get; set; } = null!;
        
        // İçerik
        public string Content { get; set; } = null!;
        
        // Tarih
        public DateTime Date { get; set; }
        
        // Oluşturulma zamanı
        public DateTime CreatedAt { get; set; }

        // Çalışma saatleri
        public decimal WorkingHours { get; set; } = 0;
        
        // Kategori
        public int Category { get; set; } = 1;
        
        // Notlar
        public string? Notes { get; set; }
        
        // Öğrenilen konular
        public string? LearnedTopics { get; set; }
        
        // Zorluklar
        public string? Challenges { get; set; }
        
        // Çözümler
        public string? Solutions { get; set; }
        
        // Onaylandı mı
        public bool IsApproved { get; set; }
        
        // Danışman geribildirim
        public string? AdvisorFeedback { get; set; }
        
        // Onaylanma zamanı
        public DateTime? ApprovedAt { get; set; }
    }
}
