using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Order;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Order
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllNotificationsByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
        }

        public async Task<List<Notification>> GetNotificationsByTypeAsync(int userId, string type)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && n.Type == type && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetNotificationsByPriorityAsync(int userId, int priority)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && n.Priority == priority && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                _context.Notifications.Update(notification);
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            _context.Notifications.UpdateRange(unreadNotifications);
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsDeleted = true;
            }
        }

        public async Task DeleteExpiredNotificationsAsync()
        {
            var expiredNotifications = await _context.Notifications
                .Where(n => n.ExpiresAt.HasValue && n.ExpiresAt.Value <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var notification in expiredNotifications)
            {
                notification.IsDeleted = true;
            }

            _context.Notifications.UpdateRange(expiredNotifications);
        }

        public async Task<List<Notification>> GetNotificationsByEntityAsync(int userId, string entityType, int entityId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId 
                           && n.RelatedEntityType == entityType 
                           && n.RelatedEntityId == entityId 
                           && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetGroupedNotificationsAsync(int userId, string groupKey)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId 
                           && n.GroupKey == groupKey 
                           && n.IsGrouped 
                           && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetRecentNotificationsAsync(int userId, int count = 10)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetPaginatedNotificationsAsync(int userId, int page, int pageSize)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}