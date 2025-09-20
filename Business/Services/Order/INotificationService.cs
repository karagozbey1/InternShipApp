using Business.Interfaces.Order;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Business.Services.Order
{
    // Bildirim servisi
    public class INotificationService : IINotificationService
    {
        // Veritabanı işlemleri
        private readonly IUnitOfWork _unitOfWork;

        // Kurucu metod
        public INotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Kimlik ile bildirim getirir
        public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
        {
            return await _unitOfWork.Notifications.GetByIdAsync(notificationId);
        }

        // Okunmamış bildirimleri getirir
        public async Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(int userId)
        {
            return await _unitOfWork.Notifications.GetUnreadNotificationsByUserIdAsync(userId);
        }

        // Tüm bildirimleri getirir
        public async Task<List<Notification>> GetAllNotificationsByUserIdAsync(int userId)
        {
            return await _unitOfWork.Notifications.GetAllNotificationsByUserIdAsync(userId);
        }

        // Okunmamış bildirim sayısını getirir
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
        }

        // Türe göre bildirimleri getirir
        public async Task<List<Notification>> GetNotificationsByTypeAsync(int userId, string type)
        {
            return await _unitOfWork.Notifications.GetNotificationsByTypeAsync(userId, type);
        }

        // Önceliğe göre bildirimleri getirir
        public async Task<List<Notification>> GetNotificationsByPriorityAsync(int userId, int priority)
        {
            return await _unitOfWork.Notifications.GetNotificationsByPriorityAsync(userId, priority);
        }

        // Basit bildirim gönderir
        public async Task<Notification> SendNotificationAsync(int userId, string title, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "Info",
                Priority = 1,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveAsync();
            return notification;
        }

        // Gelişmiş bildirim oluşturur
        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            
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

        // Bildirimi okundu işaretler
        public async Task MarkAsReadAsync(int notificationId)
        {
            await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
            
            var entity = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (entity != null && !entity.IsRead)
            {
                entity.IsRead = true;
                entity.ReadAt = DateTime.UtcNow;
            }
            await _unitOfWork.SaveAsync();
        }

        // Tümünü okundu işaretler
        public async Task MarkAllAsReadAsync(int userId)
        {
            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            
            var unread = await _unitOfWork.Notifications.GetUnreadNotificationsByUserIdAsync(userId);
            if (unread.Any())
            {
                foreach (var n in unread)
                {
                    n.IsRead = true;
                    n.ReadAt = DateTime.UtcNow;
                }
            }
            await _unitOfWork.SaveAsync();
        }

        // Bildirim siler
        public async Task DeleteNotificationAsync(int notificationId)
        {
            await _unitOfWork.Notifications.DeleteNotificationAsync(notificationId);
            await _unitOfWork.SaveAsync();
        }

        // Süresi dolmuş bildirimleri siler
        public async Task DeleteExpiredNotificationsAsync()
        {
            await _unitOfWork.Notifications.DeleteExpiredNotificationsAsync();
            await _unitOfWork.SaveAsync();
        }

        // İlişkili öğeye göre bildirimleri getirir
        public async Task<List<Notification>> GetNotificationsByEntityAsync(int userId, string entityType, int entityId)
        {
            return await _unitOfWork.Notifications.GetNotificationsByEntityAsync(userId, entityType, entityId);
        }

        // Gruplanmış bildirimleri getirir
        public async Task<List<Notification>> GetGroupedNotificationsAsync(int userId, string groupKey)
        {
            return await _unitOfWork.Notifications.GetGroupedNotificationsAsync(userId, groupKey);
        }

        // Son bildirimleri getirir
        public async Task<List<Notification>> GetRecentNotificationsAsync(int userId, int count = 10)
        {
            return await _unitOfWork.Notifications.GetRecentNotificationsAsync(userId, count);
        }

        // Sayfalı bildirimleri getirir
        public async Task<List<Notification>> GetPaginatedNotificationsAsync(int userId, int page, int pageSize)
        {
            return await _unitOfWork.Notifications.GetPaginatedNotificationsAsync(userId, page, pageSize);
        }

        // Son aktiviteleri getirir
        public async Task<List<Notification>> GetRecentActivitiesByUserAsync(int userId, int limit = 10)
        {
            return await _unitOfWork.Notifications.GetRecentNotificationsAsync(userId, limit);
        }

        // Başvuru bildirimi oluşturur
        public async Task CreateApplicationNotificationAsync(int userId, string title, string message, int applicationId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "Application",
                Priority = 2,
                RelatedEntityType = "Application",
                RelatedEntityId = applicationId,
                IconClass = "fas fa-file-alt",
                ActionUrl = $"/Application/Details/{applicationId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }

        // Belge bildirimi oluşturur
        public async Task CreateDocumentNotificationAsync(int userId, string title, string message, int documentId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "Document",
                Priority = 2,
                RelatedEntityType = "Document",
                RelatedEntityId = documentId,
                IconClass = "fas fa-file",
                ActionUrl = $"/Document/Details/{documentId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }

        // Mesaj bildirimi oluşturur
        public async Task CreateMessageNotificationAsync(int userId, string title, string message, int messageId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "Message",
                Priority = 3,
                RelatedEntityType = "ChatMessage",
                RelatedEntityId = messageId,
                IconClass = "fas fa-comment",
                ActionUrl = "/Chat",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }

        // Sistem bildirimi oluşturur
        public async Task CreateSystemNotificationAsync(int userId, string title, string message, int priority = 1)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "System",
                Priority = priority,
                IconClass = priority >= 3 ? "fas fa-exclamation-triangle" : "fas fa-info-circle",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }

        // Grup bildirimi oluşturur
        public async Task CreateGroupNotificationAsync(List<int> userIds, string title, string message, string groupKey, string type = "Info")
        {
            var groupId = Guid.NewGuid().ToString();
            
            foreach (var userId in userIds)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Priority = 2,
                    GroupKey = groupKey,
                    IsGrouped = true,
                    IconClass = "fas fa-users",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _unitOfWork.Notifications.AddAsync(notification);
            }
            
            await _unitOfWork.SaveAsync();
        }
    }
}
