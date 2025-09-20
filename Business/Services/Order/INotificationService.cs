using Business.Interfaces.Order;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Business.Services.Order
{
    // Bildirim yönetimi servisi - sistem içi notification sistemi
    // Kullanıcılara çeşitli etkinlikler hakkında bilgilendirimler gönderir
    // E-posta, push notification ve in-app message desteği sağlar
    // Real-time communication ve user engagement için kritik servis
    public class INotificationService : IINotificationService
    {
        // Unit of Work pattern - veritabanı işlemleri koordinasyonu
        // Notification CRUD operations ve related entity management
        // Transaction consistency ve atomic operations için
        private readonly IUnitOfWork _unitOfWork;

        // Dependency injection constructor
        // Scoped lifetime ile HttpContext bazında instance yönetimi
        public INotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ID ile tek bildirim getirme - detay sayfası ve mark as read için
        // Null dönable - existence check calling code'da yapılmalı
        public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
        {
            // Input validation repository seviyesinde yapılır
            // Primary key lookup - en hızlı query tipi
            return await _unitOfWork.Notifications.GetByIdAsync(notificationId);
        }

        // Kullanıcının okunmamış bildirimlerini getirir
        // Badge sayısı ve urgent notifications için kullanılır
        // Real-time güncellemeler için SignalR ile entegre edilebilir
        public async Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(int userId)
        {
            // IsRead=false filtresi ile optimized query
            // Index kullanımı için UserId + IsRead composite index önerilisr
            return await _unitOfWork.Notifications.GetUnreadNotificationsByUserIdAsync(userId);
        }

        // Kullanıcının tüm bildirimlerini getirir - notification center için
        // Sayfalama (pagination) ile birlikte kullanılması performance için önemli
        // Read/Unread mixed content - notification history görüntüleme
        public async Task<List<Notification>> GetAllNotificationsByUserIdAsync(int userId)
        {
            // Chronological order (CreatedAt DESC) ile sıralı getirir
            // Large datasets için pagination wrapper method'ları tercih edilmeli
            return await _unitOfWork.Notifications.GetAllNotificationsByUserIdAsync(userId);
        }

        // Okunmamış bildirim sayısını döner - UI badge için optimize
        // Count query - sadece sayısal değer döner, hafızada entity yüklemez
        // High-frequency call - caching layer eklenmesi düşünülebilir
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            // SELECT COUNT(*) query - memory efficient
            // Dashboard ve header notification badge'leri için kullanılır
            return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
        }

        // Tip bazlı bildirim filtreleme - kategori tabanlı görüntüleme
        // Type: Application, Document, Message, System, Approval vb.
        // Notification center'da tab-based filtering için kullanılır
        public async Task<List<Notification>> GetNotificationsByTypeAsync(int userId, string type)
        {
            // WHERE UserId = @userId AND Type = @type query
            // Composite index (UserId, Type) performance artırır
            return await _unitOfWork.Notifications.GetNotificationsByTypeAsync(userId, type);
        }

        // Öncelik seviyesi bazlı filtreleme - priority management
        // Priority: 1=Low, 2=Medium, 3=High, 4=Critical
        // Critical notifications öncelikli gösterim için kullanılır
        public async Task<List<Notification>> GetNotificationsByPriorityAsync(int userId, int priority)
        {
            // Priority-based filtering - urgent notifications için
            // Email/SMS trigger'ları yüksek öncelik için aktivelenebiir
            return await _unitOfWork.Notifications.GetNotificationsByPriorityAsync(userId, priority);
        }

        // Basit bildirim gönderme - quick notification helper method
        // Default values ile hızlı bildirim oluşturma
        // Internal system events için kullanışlı shortcut method
        public async Task<Notification> SendNotificationAsync(int userId, string title, string message)
        {
            // Default configuration ile notification entity oluştur
            // Type=Info, Priority=Low - genel bilgilendirmeler için
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "Info",                    // Default type - informational
                Priority = 1,                     // Low priority - casual notifications
                IsRead = false,                   // New notifications always unread
                CreatedAt = DateTime.UtcNow       // Server timestamp
            };

            // Repository'ye kaydet ve transaction commit
            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveAsync();
            return notification;
        }

        // Gelişmiş bildirim oluşturma - full configuration support
        // Custom properties ile kapsamlı notification management
        // Business-specific requirements için flexible method
        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            // Timestamp otomatik atanır - audit trail için kritik
            notification.CreatedAt = DateTime.UtcNow;
            
            // Entity validation burada yapılabilir
            if (notification.UserId <= 0)
                throw new ArgumentException("Geçersiz kullanıcı ID");
                
            if (string.IsNullOrWhiteSpace(notification.Title))
                throw new ArgumentException("Bildirim başlığı boş olamaz");
                
            if (string.IsNullOrWhiteSpace(notification.Message))
                throw new ArgumentException("Bildirim içeriği boş olamaz");

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveAsync();
            return notification;
        }

        // Bildirimi okundu olarak işaretleme - user interaction tracking
        // Read/unread state management - engagement metrics için önemli
        // Single notification mark as read functionality
        public async Task MarkAsReadAsync(int notificationId)
        {
            // Primary repository method - optimized update query
            await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
            
            // Fallback mechanism - repository implementation eksikliği için
            // Bazı repository'ler ReadAt timestamp'ini set etmiyor olabilir
            var entity = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (entity != null && !entity.IsRead)
            {
                entity.IsRead = true;
                entity.ReadAt = DateTime.UtcNow;     // Read timestamp - analytics için
            }
            await _unitOfWork.SaveAsync();
        }

        // Kullanıcının tüm bildirimlerini okundu işaretleme - bulk operation
        // "Mark all as read" functionality için optimize edilmiş method
        // Bulk update ile performance improvement sağlar
        public async Task MarkAllAsReadAsync(int userId)
        {
            // Bulk update query - WHERE UserId = @userId SET IsRead = true
            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            
            // Fallback/guarantee mechanism - consistency insurance
            // Repository implementation'ın reliable olmadığı durumlarda safety net
            var unread = await _unitOfWork.Notifications.GetUnreadNotificationsByUserIdAsync(userId);
            if (unread.Any())
            {
                foreach (var n in unread)
                {
                    n.IsRead = true;
                    n.ReadAt = DateTime.UtcNow;      // Audit trail
                }
            }
            await _unitOfWork.SaveAsync();
        }

        // Bildirim silme - user privacy ve data management
        // Soft delete yerine hard delete policy - notification data retention
        // GDPR compliance ve user data management için
        public async Task DeleteNotificationAsync(int notificationId)
        {
            // Repository method ile entity removal
            await _unitOfWork.Notifications.DeleteNotificationAsync(notificationId);
            await _unitOfWork.SaveAsync();
        }

        // Süresi dolmuş bildirimleri temizleme - automated cleanup
        // Background job veya scheduled task tarafından çağrılır
        // Database bloat prevention ve performance optimization
        public async Task DeleteExpiredNotificationsAsync()
        {
            // Bulk delete operation - WHERE CreatedAt < (NOW - retention_period)
            // Retention policy: genellikle 30-90 gün arası
            await _unitOfWork.Notifications.DeleteExpiredNotificationsAsync();
            await _unitOfWork.SaveAsync();
        }

        // Entity-specific bildirim getirme - related content linking
        // EntityType: Application, Document, ChatMessage vb.
        // Entity detail sayfalarında related notifications için
        public async Task<List<Notification>> GetNotificationsByEntityAsync(int userId, string entityType, int entityId)
        {
            // Related entity filtering - deep linking support
            // Example: ApplicationId=123 için tüm notifications
            return await _unitOfWork.Notifications.GetNotificationsByEntityAsync(userId, entityType, entityId);
        }

        // Gruplanmış bildirimler - batch notification management
        // GroupKey ile ilişkili bildirimleri tek seferde yönetme
        // Example: "company_123_applications" grup anahtarı
        public async Task<List<Notification>> GetGroupedNotificationsAsync(int userId, string groupKey)
        {
            // Group-based notification retrieval - batch operations
            // Departman/proje bazlı notification management için
            return await _unitOfWork.Notifications.GetGroupedNotificationsAsync(userId, groupKey);
        }

        // Son N bildirimi getirme - dashboard preview için
        // Recent activity feed ve quick access functionality
        // Performance için default limit ile controlled query
        public async Task<List<Notification>> GetRecentNotificationsAsync(int userId, int count = 10)
        {
            // TOP N query with ORDER BY CreatedAt DESC
            // Dashboard widgets ve notification preview için optimize
            return await _unitOfWork.Notifications.GetRecentNotificationsAsync(userId, count);
        }

        // Sayfalanmış bildirim getirme - large dataset management
        // Pagination support ile memory efficient browsing
        // Notification center'da infinite scroll için kullanılır
        public async Task<List<Notification>> GetPaginatedNotificationsAsync(int userId, int page, int pageSize)
        {
            // OFFSET/LIMIT query - pagination implementation
            // Skip = (page-1) * pageSize, Take = pageSize logic
            return await _unitOfWork.Notifications.GetPaginatedNotificationsAsync(userId, page, pageSize);
        }

        // Son aktiviteleri getirme - activity timeline için
        // GetRecentNotificationsAsync'a delegate eden wrapper method
        // API consistency ve semantic clarity için
        public async Task<List<Notification>> GetRecentActivitiesByUserAsync(int userId, int limit = 10)
        {
            // Alias method - semantic clarity için farklı isim
            // Activity feed terminology ile API consistency
            return await _unitOfWork.Notifications.GetRecentNotificationsAsync(userId, limit);
        }

        // ==================== SPECIALIZED NOTIFICATION BUILDERS ====================
        // Helper methods - belirli bildirim tiplerini oluşturmak için
        // Pre-configured templates ile consistent notification creation

        // Başvuru bildirimi oluşturma - application workflow notifications
        // İlan başvurusu, durum değişikliği vb. için specialized builder
        // Automatic routing ve icon assignment ile enhanced UX
        public async Task CreateApplicationNotificationAsync(int userId, string title, string message, int applicationId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "Application",                        // Categorical classification
                Priority = 2,                               // Medium priority - important but not urgent
                RelatedEntityType = "Application",          // Entity relationship
                RelatedEntityId = applicationId,            // Deep linking support
                IconClass = "fas fa-file-alt",             // UI icon - FontAwesome CSS class
                ActionUrl = $"/Application/Details/{applicationId}", // Direct navigation URL
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }

        // Belge bildirimi oluşturma - document management notifications
        // Belge yükleme, onay, reddetme süreçleri için
        // Document workflow automation support
        public async Task CreateDocumentNotificationAsync(int userId, string title, string message, int documentId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "Document",                          // Document-specific type
                Priority = 2,                               // Standard document priority
                RelatedEntityType = "Document",             // Entity reference
                RelatedEntityId = documentId,
                IconClass = "fas fa-file",                 // Document icon
                ActionUrl = $"/Document/Details/{documentId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }

        // Mesaj bildirimi oluşturma - chat/messaging system notifications
        // Real-time messaging alerts ve chat activity tracking
        // Communication workflow enhancement
        public async Task CreateMessageNotificationAsync(int userId, string title, string message, int messageId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "Message",                           // Communication type
                Priority = 3,                               // High priority - immediate attention
                RelatedEntityType = "ChatMessage",          // Message entity reference
                RelatedEntityId = messageId,
                IconClass = "fas fa-comment",              // Chat/message icon
                ActionUrl = "/Chat",                       // Chat interface navigation
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }

        // Sistem bildirimi oluşturma - administrative/system notifications
        // Maintenance, policy changes, system announcements için
        // Priority-based icon assignment logic
        public async Task CreateSystemNotificationAsync(int userId, string title, string message, int priority = 1)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "System",                            // Administrative type
                Priority = priority,                        // Variable priority input
                // Dynamic icon assignment based on priority level
                IconClass = priority >= 3 ? "fas fa-exclamation-triangle" : "fas fa-info-circle",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }

        // Grup bildirimi oluşturma - mass notification broadcast
        // Multiple users'a aynı anda bildirim gönderme
        // Department-wide, company-wide announcements için
        // Batch operation ile performance optimization
        public async Task CreateGroupNotificationAsync(List<int> userIds, string title, string message, string groupKey, string type = "Info")
        {
            // Unique group identifier - related notifications'ı bağlamak için
            var groupId = Guid.NewGuid().ToString();
            
            // Batch notification creation - her kullanıcı için ayrı notification
            foreach (var userId in userIds)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,                            // Configurable type
                    Priority = 2,                           // Standard group priority
                    GroupKey = groupKey,                    // Group association key
                    IsGrouped = true,                       // Group notification flag
                    IconClass = "fas fa-users",            // Group/team icon
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                
                // Add to context - batch insert için memory'de topla
                await _unitOfWork.Notifications.AddAsync(notification);
            }
            
            // Single transaction - atomic group notification creation
            await _unitOfWork.SaveAsync();
        }
    }
}
