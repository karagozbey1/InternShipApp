using System;
using System.Collections.Generic;

namespace Core.Entity
{
    // Kullanıcı rollerini tanımlayan enum
    // Sistemdeki farklı kullanıcı tiplerini ve yetkilerini belirler
    // Role-based access control (RBAC) için temel yapı
    public enum UserRole 
    { 
        // Öğrenci rolü - staj arayan üniversite öğrencileri
        // Başvuru yapma, belge yükleme, staj takip yetkilerine sahip
        Student, 
        
        // Akademik danışman - öğrenci danışmanları ve akademik personel  
        // Onay süreçleri, değerlendirme ve mentörlük yetkilerine sahip
        Advisor, 
        
        // Şirket temsilcisi - staj pozisyonu sunan kurum çalışanları
        // İlan yayınlama, başvuru değerlendirme yetkilerine sahip
        Company, 
        
        // Sistem yöneticisi - platform yöneticileri
        // Tüm sistem işlemlerine ve kullanıcı yönetimine sahip
        Admin 
    }

    // Ana kullanıcı entity'si - tüm sistem kullanıcılarının ortak bilgileri
    // Authentication, authorization ve profile management için temel sınıf
    // Polymorphic davranış sergiler - role göre farklı özelliklere sahip
    public class User
    {
        // Primary key - kullanıcının benzersiz sistem tanımlayıcısı
        // Auto-increment, tüm ilişkisel tablolarda foreign key olarak kullanılır
        public int Id { get; set; }
        
        // Kullanıcının görünen tam adı - UI'da gösterilecek isim
        // "Ad Soyad" formatında, profil ve listelerde kullanılır
        // Authentication'dan ayrı tutulan display name
        public string FullName { get; set; } = null!;
        
        // Unique email adresi - sistem giriş kimliği
        // Username yerine email kullanılır, daha user-friendly
        // Notification ve communication için de kullanılır
        public string Email { get; set; } = null!;
        
        // Hash'lenmiş şifre - güvenlik için plain text saklanmaz
        // Salt ile birlikte hash'lenir, rainbow table attack'lerden korunur
        // BCrypt, Argon2 gibi strong hashing algorithm'ları kullanılmalı
        public string PasswordHash { get; set; } = null!;
        
        // Hesap aktivasyon durumu - email verification kontrolü
        // false: hesap kullanılamaz, giriş yapamaz
        // Email onayı veya admin approval sonrası true olur
        public bool IsActive { get; set; } = false;
        
        // Email aktivasyon kodu - hesap doğrulama için
        // Rastgele generate edilen unique token
        // Single-use, expire time'ı olan güvenli kod
        public string? ActivationCode { get; set; }
        
        // Aktivasyon kodunun son geçerlilik tarihi
        // Security measure - eski kodların kullanımını engeller
        // Genellikle 24-48 saat arası expire time
        public DateTime? ActivationCodeExpireAt { get; set; }
        
        // Kullanıcının sistem rolü - authorization'da kullanılır
        // Enum değeri, middleware'larda role-based filtering için
        // Claims-based authentication'da claim olarak eklenir
        public UserRole Role { get; set; }
        
        // Foreign key - bağlı olunan üniversite (öğrenci/danışman için)
        // Nullable - şirket kullanıcıları için geçerli değil
        // Academic institution binding için kritik
        public int? UniversityId { get; set; }
        
        // Navigation property - üniversite bilgileri
        // Lazy loading veya explicit loading ile doldurulur
        // Academic context ve reporting için kullanılır
        public University? University { get; set; }
        
        // Hesap oluşturulma zamanı - audit trail için
        // Registration timestamp, user analytics için değerli
        // UTC format'ında saklanmalı
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // İletişim telefon numarası - opsiyonel contact bilgisi
        // Emergency contact veya verification için kullanılabilir
        // Format validation yapılmalı (+90 555 123 45 67)
        public string? Phone { get; set; }

        // Akademik/mesleki departman bilgisi
        // Öğrenciler için: okudukları bölüm ("Bilgisayar Mühendisliği")
        // Danışmanlar için: çalıştıkları department
        public string? Department { get; set; }
        
        // Akademik veya mesleki unvan
        // "Dr.", "Prof.", "Doç.", "Öğr. Gör." gibi akademik unvanlar
        // Professional title'lar için de kullanılabilir
        public string? Title { get; set; }

        // Öğrenci numarası - sadece Student rolü için
        // Üniversite tarafından verilen unique identifier
        // Academic transcript ve official documentation için
        public string? StudentNumber { get; set; }
        
        // Akademik sınıf seviyesi - öğrenci için grade info
        // 1,2,3,4 (lisans) veya 5,6 (yüksek lisans) değerleri
        // Staj eligibility kontrolünde kullanılır
        public int? Grade { get; set; }

        // Şifre sıfırlama token'ı - "forgot password" işlemi için
        // Temporary, single-use, time-limited güvenli token
        // Email ile gönderilen reset link'inde kullanılır
        public string? PasswordResetToken { get; set; }
        
        // Reset token'ının expiry zamanı - güvenlik önlemi
        // Genellikle 1-2 saat gibi kısa süre
        // Expired token'lar invalid sayılır
        public DateTime? PasswordResetTokenExpireAt { get; set; }

        // Navigation property - öğrenci olarak yapılan başvurular
        // One-to-many relationship, bir öğrenci birçok başvuru yapabilir
        // Student dashboard'da başvuru geçmişi için kullanılır
        public ICollection<InternshipApplication>? StudentApplications { get; set; }
        
        // Navigation property - danışman olarak değerlendirilen başvurular
        // Academic supervision relationship
        // Approval workflow'unda advisor assignment için
        public ICollection<InternshipApplication>? AdvisorApplications { get; set; }
        
        // Navigation property - gönderilen yorumlar ve mesajlar
        // Communication history tracking
        // User'ın platform içi aktivite geçmişi
        public ICollection<InternshipComment>? SentComments { get; set; }
        
        // Navigation property - alınan sistem bildirimleri
        // Push notifications, email alerts, in-app messages
        // User engagement ve communication için kritik
        public ICollection<Notification>? Notifications { get; set; }
        
        // Navigation property - katılınan chat odaları
        // Many-to-many relationship via UserChatRoom junction table
        // Real-time messaging functionality için
        public ICollection<UserChatRoom>? ChatRooms { get; set; }
        
        // Navigation property - gönderilen chat mesajları
        // Message history ve communication tracking
        // Moderation ve audit için gerekli
        public ICollection<ChatMessage>? SentMessages { get; set; }
        
        // Navigation property - şirket bağlantıları
        // Company representatives için company-user many-to-many
        // Multiple company affiliation desteği
        public ICollection<CompanyUser>? CompanyUsers { get; set; }
        
        // Navigation property - oluşturulan staj dönemleri
        // Academic calendar management
        // Genellikle advisor'lar tarafından manage edilir
        public ICollection<InternshipPeriod>? InternshipPeriods { get; set; }
        
        // Navigation property - katılınan staj dönemleri  
        // Student enrollment in academic periods
        // Many-to-many via InternshipPeriodStudent junction
        public ICollection<InternshipPeriodStudent>? InternshipPeriodStudents { get; set; }
    }
}
