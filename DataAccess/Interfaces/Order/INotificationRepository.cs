using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Order
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(int userId);
        Task<List<Notification>> GetAllNotificationsByUserIdAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<List<Notification>> GetNotificationsByTypeAsync(int userId, string type);
        Task<List<Notification>> GetNotificationsByPriorityAsync(int userId, int priority);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteNotificationAsync(int notificationId);
        Task DeleteExpiredNotificationsAsync();
        Task<List<Notification>> GetNotificationsByEntityAsync(int userId, string entityType, int entityId);
        Task<List<Notification>> GetGroupedNotificationsAsync(int userId, string groupKey);
        Task<List<Notification>> GetRecentNotificationsAsync(int userId, int count = 10);
        Task<List<Notification>> GetPaginatedNotificationsAsync(int userId, int page, int pageSize);
    }
}