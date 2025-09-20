using System;
using System.Collections.Generic;

namespace Core.Entity
{
    // Kullanıcı türlerini belirler
    public enum UserRole 
    { 
        // Öğrenci
        Student, 
        
        // Danışman
        Advisor, 
        
        // Şirket temsilcisi
        Company, 
        
        // Yönetici
        Admin 
    }

    // Kullanıcı bilgileri
    public class User
    {
        // Kimlik numarası
        public int Id { get; set; }
        
        // Ad soyad
        public string FullName { get; set; } = null!;
        
        // E-posta adresi
        public string Email { get; set; } = null!;
        
        // Şifrelenmiş şifre
        public string PasswordHash { get; set; } = null!;
        
        // Hesap aktif mi
        public bool IsActive { get; set; } = false;
        
        // E-posta doğrulama kodu
        public string? ActivationCode { get; set; }
        
        // Doğrulama kodu son tarihi
        public DateTime? ActivationCodeExpireAt { get; set; }
        
        // Kullanıcı türü
        public UserRole Role { get; set; }
        
        // Bağlı üniversite kimliği
        public int? UniversityId { get; set; }
        
        // Üniversite bilgileri
        public University? University { get; set; }
        
        // Hesap oluşturulma tarihi
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Telefon numarası
        public string? Phone { get; set; }

        // Bölüm adı
        public string? Department { get; set; }
        
        // Unvan
        public string? Title { get; set; }

        // Öğrenci numarası
        public string? StudentNumber { get; set; }
        
        // Sınıf seviyesi
        public int? Grade { get; set; }

        // Şifre sıfırlama kodu
        public string? PasswordResetToken { get; set; }
        
        // Şifre sıfırlama son tarihi
        public DateTime? PasswordResetTokenExpireAt { get; set; }

        // Öğrenci başvuruları
        public ICollection<InternshipApplication>? StudentApplications { get; set; }
        
        // Danışman değerlendirmeleri
        public ICollection<InternshipApplication>? AdvisorApplications { get; set; }
        
        // Gönderilen yorumlar
        public ICollection<InternshipComment>? SentComments { get; set; }
        
        // Bildirimler
        public ICollection<Notification>? Notifications { get; set; }
        
        // Sohbet odaları
        public ICollection<UserChatRoom>? ChatRooms { get; set; }
        
        // Gönderilen mesajlar
        public ICollection<ChatMessage>? SentMessages { get; set; }
        
        // Şirket bağlantıları
        public ICollection<CompanyUser>? CompanyUsers { get; set; }
        
        // Oluşturulan staj dönemleri
        public ICollection<InternshipPeriod>? InternshipPeriods { get; set; }
        
        // Katılınan staj dönemleri
        public ICollection<InternshipPeriodStudent>? InternshipPeriodStudents { get; set; }
    }
}
