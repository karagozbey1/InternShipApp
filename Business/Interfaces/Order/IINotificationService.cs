using Core.Entity;

namespace Business.Interfaces.Order
{
    public interface IINotificationService
    {
        Task<Notification?> GetNotificationByIdAsync(int notificationId);
        Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(int userId);
        Task<List<Notification>> GetAllNotificationsByUserIdAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<List<Notification>> GetNotificationsByTypeAsync(int userId, string type);
        Task<List<Notification>> GetNotificationsByPriorityAsync(int userId, int priority);
        Task<Notification> SendNotificationAsync(int userId, string title, string message);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteNotificationAsync(int notificationId);
        Task DeleteExpiredNotificationsAsync();
        Task<List<Notification>> GetNotificationsByEntityAsync(int userId, string entityType, int entityId);
        Task<List<Notification>> GetGroupedNotificationsAsync(int userId, string groupKey);
        Task<List<Notification>> GetRecentNotificationsAsync(int userId, int count = 10);
        Task<List<Notification>> GetPaginatedNotificationsAsync(int userId, int page, int pageSize);
        Task<List<Notification>> GetRecentActivitiesByUserAsync(int userId, int limit = 10);
        
        // Helper methods for creating specific notification types
        Task CreateApplicationNotificationAsync(int userId, string title, string message, int applicationId);
        Task CreateDocumentNotificationAsync(int userId, string title, string message, int documentId);
        Task CreateMessageNotificationAsync(int userId, string title, string message, int messageId);
        Task CreateSystemNotificationAsync(int userId, string title, string message, int priority = 1);
        Task CreateGroupNotificationAsync(List<int> userIds, string title, string message, string groupKey, string type = "Info");
    }
}