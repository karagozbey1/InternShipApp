using Business.Interfaces.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebUI.Controllers;
using WebUI.Models.ViewModels.Chat;

namespace WebUI.Controllers.Student
{
    [Authorize]
    public class NotificationController : BaseAppController
    {
        private readonly IINotificationService _notificationService;

        public NotificationController(IINotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetPaginatedNotificationsAsync(userId, page, pageSize);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

            var model = new NotificationListViewModel
            {
                Notifications = notifications.Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    Priority = n.Priority,
                    IconClass = n.IconClass ?? GetDefaultIcon(n.Type),
                    ActionUrl = n.ActionUrl,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt,
                    RelatedEntityType = n.RelatedEntityType,
                    RelatedEntityId = n.RelatedEntityId
                }).ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                UnreadCount = unreadCount
            };

            return View("~/Views/Student/Notification/Index.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Json(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                return Json(new { unreadCount = 0 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecent(int count = 5)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetRecentNotificationsAsync(userId, count);
                
                var result = notifications.Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    type = n.Type,
                    iconClass = n.IconClass ?? GetDefaultIcon(n.Type),
                    actionUrl = n.ActionUrl,
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    timeAgo = GetTimeAgo(n.CreatedAt)
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Bildirim okundu olarak iþaretlenirken hata oluþtu." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                await _notificationService.MarkAllAsReadAsync(userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Bildirimler okundu olarak iþaretlenirken hata oluþtu." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _notificationService.DeleteNotificationAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Bildirim silinirken hata oluþtu." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByType(string type)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetNotificationsByTypeAsync(userId, type);
                
                var result = notifications.Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    Priority = n.Priority,
                    IconClass = n.IconClass ?? GetDefaultIcon(n.Type),
                    ActionUrl = n.ActionUrl,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new List<NotificationViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByPriority(int priority)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetNotificationsByPriorityAsync(userId, priority);
                
                var result = notifications.Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    Priority = n.Priority,
                    IconClass = n.IconClass ?? GetDefaultIcon(n.Type),
                    ActionUrl = n.ActionUrl,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new List<NotificationViewModel>());
            }
        }

        // Helper Methods
        private string GetDefaultIcon(string type)
        {
            return type switch
            {
                "Application" => "fas fa-file-alt",
                "Document" => "fas fa-file",
                "Message" => "fas fa-comment",
                "System" => "fas fa-cog",
                "Warning" => "fas fa-exclamation-triangle",
                "Error" => "fas fa-times-circle",
                "Success" => "fas fa-check-circle",
                _ => "fas fa-bell"
            };
        }

        private string GetTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.UtcNow - createdAt;
            
            if (timeSpan.TotalMinutes < 1)
                return "Az önce";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} dakika önce";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} saat önce";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} gün önce";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} hafta önce";
            
            return createdAt.ToString("dd.MM.yyyy");
        }
    }

    // ViewModels for Notifications
    public class NotificationListViewModel
    {
        public List<NotificationViewModel> Notifications { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
        public bool HasNextPage => (CurrentPage * PageSize) < TotalCount;
        public bool HasPreviousPage => CurrentPage > 1;
    }

    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string IconClass { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        
        // UI Properties
        public string TimeAgo { get; set; } = string.Empty;
        public string PriorityClass => GetPriorityClass();
        public string TypeClass => GetTypeClass();
        
        private string GetPriorityClass()
        {
            return Priority switch
            {
                4 => "border-red-500 bg-red-50",
                3 => "border-orange-500 bg-orange-50",
                2 => "border-blue-500 bg-blue-50",
                _ => "border-gray-300 bg-gray-50"
            };
        }
        
        private string GetTypeClass()
        {
            return Type switch
            {
                "Error" => "text-red-600",
                "Warning" => "text-orange-600",
                "Success" => "text-green-600",
                "Application" => "text-blue-600",
                "Message" => "text-purple-600",
                _ => "text-gray-600"
            };
        }
    }
}