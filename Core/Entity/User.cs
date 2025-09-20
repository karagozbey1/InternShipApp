using System;
using System.Collections.Generic;

namespace Core.Entity
{
    // Kullanıcı rollerini tanımlayan enum - sistemdeki farklı kullanıcı tiplerini belirler
    public enum UserRole 
    { 
        // Öğrenci rolü - staj arayan üniversite öğrencileri
        Student, 
        
        // Akademik danışman - öğrenci danışmanları ve akademik personel  
        Advisor, 
        
        // Şirket temsilcisi - staj pozisyonu sunan kurum çalışanları
        Company, 
        
        // Sistem yöneticisi - platform yöneticileri
        Admin 
    }

    // Ana kullanıcı entity'si - tüm sistem kullanıcılarının ortak bilgileri
    public class User
    {
        // Primary key - kullanıcının benzersiz sistem tanımlayıcısı
        public int Id { get; set; }
        
        // Kullanıcının görünen tam adı - UI'da gösterilecek isim
        public string FullName { get; set; } = null!;
        
        // Unique email adresi - sistem giriş kimliği
        public string Email { get; set; } = null!;
        
        // Hash'lenmiş şifre - güvenlik için plain text saklanmaz
        public string PasswordHash { get; set; } = null!;
        
        // Hesap aktivasyon durumu - email verification kontrolü
        public bool IsActive { get; set; } = false;
        
        // Email aktivasyon kodu - hesap doğrulama için
        public string? ActivationCode { get; set; }
        
        // Aktivasyon kodunun son geçerlilik tarihi - güvenlik önlemi
        public DateTime? ActivationCodeExpireAt { get; set; }
        
        // Kullanıcının sistem rolü - authorization'da kullanılır
        public UserRole Role { get; set; }
        
        // Foreign key - bağlı olunan üniversite (öğrenci/danışman için)
        public int? UniversityId { get; set; }
        
        // Navigation property - üniversite bilgileri
        public University? University { get; set; }
        
        // Hesap oluşturulma zamanı - audit trail için
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // İletişim telefon numarası - opsiyonel contact bilgisi
        public string? Phone { get; set; }

        // Akademik/mesleki departman bilgisi
        public string? Department { get; set; }
        
        // Akademik veya mesleki unvan
        public string? Title { get; set; }

        // Öğrenci numarası - sadece Student rolü için
        public string? StudentNumber { get; set; }
        
        // Akademik sınıf seviyesi - öğrenci için grade info
        public int? Grade { get; set; }

        // Şifre sıfırlama token'ı - "forgot password" işlemi için
        public string? PasswordResetToken { get; set; }
        
        // Reset token'ının expiry zamanı - güvenlik önlemi
        public DateTime? PasswordResetTokenExpireAt { get; set; }

        // Navigation property - öğrenci olarak yapılan başvurular
        public ICollection<InternshipApplication>? StudentApplications { get; set; }
        
        // Navigation property - danışman olarak değerlendirilen başvurular
        public ICollection<InternshipApplication>? AdvisorApplications { get; set; }
        
        // Navigation property - gönderilen yorumlar ve mesajlar
        public ICollection<InternshipComment>? SentComments { get; set; }
        
        // Navigation property - alınan sistem bildirimleri
        public ICollection<Notification>? Notifications { get; set; }
        
        // Navigation property - katılınan chat odaları
        public ICollection<UserChatRoom>? ChatRooms { get; set; }
        
        // Navigation property - gönderilen chat mesajları
        public ICollection<ChatMessage>? SentMessages { get; set; }
        
        // Navigation property - şirket bağlantıları
        public ICollection<CompanyUser>? CompanyUsers { get; set; }
        
        // Navigation property - oluşturulan staj dönemleri
        public ICollection<InternshipPeriod>? InternshipPeriods { get; set; }
        
        // Navigation property - katılınan staj dönemleri
        public ICollection<InternshipPeriodStudent>? InternshipPeriodStudents { get; set; }
    }
}
