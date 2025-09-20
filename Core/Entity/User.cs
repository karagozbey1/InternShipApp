using System;
using System.Collections.Generic;

namespace Core.Entity
{
    /// <summary>
    /// Kullanıcı rolünü belirten enum
    /// Sistemdeki farklı kullanıcı türlerini tanımlar
    /// </summary>
    public enum UserRole 
    { 
        /// <summary>
        /// Öğrenci - Staj arayan üniversite öğrencileri
        /// </summary>
        Student, 
        
        /// <summary>
        /// Danışman - Akademik personel, öğrenci danışmanları
        /// </summary>
        Advisor, 
        
        /// <summary>
        /// Şirket - Staj pozisyonu sunan kurumların temsilcileri
        /// </summary>
        Company, 
        
        /// <summary>
        /// Admin - Sistem yöneticileri
        /// </summary>
        Admin 
    }

    /// <summary>
    /// Kullanıcı entity'si - Sistemdeki tüm kullanıcı türlerini temsil eder
    /// Öğrenci, danışman, şirket temsilcisi ve admin hesapları için ortak alan
    /// Role alanına göre farklı özellikler ve yetkiler aktif olur
    /// </summary>
    public class User
    {
        /// <summary>
        /// Kullanıcının benzersiz tanımlayıcısı
        /// Primary key olarak kullanılır
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Kullanıcının tam adı
        /// Profilde ve listelerde görünecek isim
        /// Zorunlu alan
        /// </summary>
        public string FullName { get; set; } = null!;
        
        /// <summary>
        /// Kullanıcının e-posta adresi
        /// Giriş yaparken kullanılan benzersiz kimlik
        /// Zorunlu alan
        /// </summary>
        public string Email { get; set; } = null!;
        
        /// <summary>
        /// Kullanıcının hash'lenmiş şifresi
        /// Güvenlik için düz metin olarak saklanmaz
        /// Zorunlu alan
        /// </summary>
        public string PasswordHash { get; set; } = null!;
        
        /// <summary>
        /// Hesabın aktif olup olmadığını belirtir
        /// False ise kullanıcı giriş yapamaz
        /// E-posta doğrulaması sonrası true olur
        /// </summary>
        public bool IsActive { get; set; } = false;
        
        /// <summary>
        /// E-posta aktivasyon kodu
        /// Hesap doğrulaması için kullanılır
        /// Opsiyonel - aktivasyon sonrası null olabilir
        /// </summary>
        public string? ActivationCode { get; set; }
        
        /// <summary>
        /// Aktivasyon kodunun son geçerlilik tarihi
        /// Bu tarihten sonra kod geçersiz olur
        /// </summary>
        public DateTime? ActivationCodeExpireAt { get; set; }
        
        /// <summary>
        /// Kullanıcının sistem içindeki rolü
        /// Student, Advisor, Company veya Admin
        /// Yetkileri bu role göre belirlenir
        /// </summary>
        public UserRole Role { get; set; }
        
        /// <summary>
        /// Kullanıcının bağlı olduğu üniversitenin ID'si
        /// Sadece öğrenci ve danışmanlar için gerekli
        /// Opsiyonel - şirket kullanıcıları için null
        /// </summary>
        public int? UniversityId { get; set; }
        
        /// <summary>
        /// Kullanıcının bağlı olduğu üniversite
        /// Navigation property - üniversite bilgilerine erişim için
        /// </summary>
        public University? University { get; set; }
        
        /// <summary>
        /// Hesabın oluşturulma tarihi
        /// Otomatik olarak şu anki zaman atanır
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Kullanıcının telefon numarası
        /// İletişim için opsiyonel bilgi
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Kullanıcının bölümü/departmanı
        /// Öğrenciler için: Okudukları bölüm
        /// Danışmanlar için: Çalıştıkları bölüm
        /// </summary>
        public string? Department { get; set; }
        
        /// <summary>
        /// Kullanıcının unvanı/title'ı
        /// Danışmanlar için: Dr., Prof., Doç. vb.
        /// Diğer roller için opsiyonel
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Öğrenci numarası
        /// Sadece öğrenciler için gerekli
        /// Üniversite tarafından verilen benzersiz numara
        /// </summary>
        public string? StudentNumber { get; set; }
        
        /// <summary>
        /// Öğrencinin sınıf/grade bilgisi
        /// 1, 2, 3, 4. sınıf veya yüksek lisans
        /// Sadece öğrenciler için kullanılır
        /// </summary>
        public int? Grade { get; set; }

        /// <summary>
        /// Şifre sıfırlama token'ı
        /// "Şifremi Unuttum" işlemi için kullanılır
        /// Geçici olarak saklanır
        /// </summary>
        public string? PasswordResetToken { get; set; }
        
        /// <summary>
        /// Şifre sıfırlama token'ının son geçerlilik tarihi
        /// Bu tarihten sonra token geçersiz olur
        /// </summary>
        public DateTime? PasswordResetTokenExpireAt { get; set; }

        /// <summary>
        /// Kullanıcının öğrenci olarak yaptığı başvurular
        /// Navigation property - öğrenci başvuru listesi
        /// </summary>
        public ICollection<InternshipApplication>? StudentApplications { get; set; }
        
        /// <summary>
        /// Kullanıcının danışman olarak değerlendirdiği başvurular
        /// Navigation property - danışman değerlendirme listesi
        /// </summary>
        public ICollection<InternshipApplication>? AdvisorApplications { get; set; }
        
        /// <summary>
        /// Kullanıcının gönderdiği yorumlar
        /// Navigation property - tüm yorum listesi
        /// </summary>
        public ICollection<InternshipComment>? SentComments { get; set; }
        
        /// <summary>
        /// Kullanıcıya gelen bildirimler
        /// Navigation property - bildirim listesi
        /// </summary>
        public ICollection<Notification>? Notifications { get; set; }
        
        /// <summary>
        /// Kullanıcının üye olduğu sohbet odaları
        /// Navigation property - chat odası listesi
        /// </summary>
        public ICollection<UserChatRoom>? ChatRooms { get; set; }
        
        /// <summary>
        /// Kullanıcının gönderdiği mesajlar
        /// Navigation property - mesaj listesi
        /// </summary>
        public ICollection<ChatMessage>? SentMessages { get; set; }
        
        /// <summary>
        /// Kullanıcının şirket ilişkileri
        /// Şirket temsilcileri için company-user bağlantıları
        /// </summary>
        public ICollection<CompanyUser>? CompanyUsers { get; set; }
        
        /// <summary>
        /// Kullanıcının tanımladığı staj dönemleri
        /// Genellikle danışmanlar tarafından oluşturulur
        /// </summary>
        public ICollection<InternshipPeriod>? InternshipPeriods { get; set; }
        
        /// <summary>
        /// Kullanıcının katıldığı staj dönemleri
        /// Öğrenciler hangi dönemlere kayıt olmuş
        /// </summary>
        public ICollection<InternshipPeriodStudent>? InternshipPeriodStudents { get; set; }
    }
}
