using Core.Entity;

namespace Business.Interfaces.Order
{
    public interface IINotificationService
    {
        Task<Notification?> GetNotificationByIdAsync(int notificationId);
        Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(int userId);
        Task<List<Notification>> GetAllNotificationsByUserIdAsync(int userId);
        Task<Notification> SendNotificationAsync(int userId, string title, string message);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteNotificationAsync(int notificationId);
    }
}