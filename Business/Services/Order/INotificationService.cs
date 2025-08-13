using Business.Interfaces.Order;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Order
{
    public class INotificationService : IINotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public INotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
        {
            return await _unitOfWork.Notifications.GetByIdAsync(notificationId);
        }

        public async Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(int userId)
        {
            return await _unitOfWork.Notifications.GetUnreadNotificationsByUserIdAsync(userId);
        }

        public async Task<List<Notification>> GetAllNotificationsByUserIdAsync(int userId)
        {
            return await _unitOfWork.Notifications.GetAllNotificationsByUserIdAsync(userId);
        }

        public async Task<Notification> SendNotificationAsync(int userId, string title, string message)
        {
            // İş mantığı: Bildirim gönderme
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveAsync();
            return notification;
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                _unitOfWork.Notifications.Update(notification);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unreadNotifications = await _unitOfWork.Notifications.GetUnreadNotificationsByUserIdAsync(userId);
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                _unitOfWork.Notifications.Update(notification);
            }
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification != null)
            {
                _unitOfWork.Notifications.Remove(notification);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}