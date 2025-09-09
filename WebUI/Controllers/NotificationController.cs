using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Business.Interfaces.Order;
using Core.Entity;

namespace WebUI.Controllers
{
    [Authorize(Roles = "Student")]
    [Route("[controller]")] // /Notification
    public class NotificationController : BaseAppController
    {
        private readonly IINotificationService _notificationService;
        public NotificationController(IINotificationService notificationService) => _notificationService = notificationService;

        [HttpGet("")] [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var unread = await _notificationService.GetUnreadCountAsync(userId);
            return View("~/Views/Student/Notification/Index.cshtml", new NotificationListViewModel { UnreadCount = unread });
        }

        [HttpGet("Filter")] 
        public async Task<IActionResult> Filter(string type = "all")
        {
            var userId = GetCurrentUserId();
            List<Notification> list = type?.ToLower() switch
            {
                "unread" => await _notificationService.GetUnreadNotificationsByUserIdAsync(userId),
                "application" => await _notificationService.GetNotificationsByTypeAsync(userId, "Application"),
                "document" => await _notificationService.GetNotificationsByTypeAsync(userId, "Document"),
                "message" => await _notificationService.GetNotificationsByTypeAsync(userId, "Message"),
                "system" => await _notificationService.GetNotificationsByTypeAsync(userId, "System"),
                _ => await _notificationService.GetAllNotificationsByUserIdAsync(userId)
            };
            var result = list.OrderByDescending(n=>n.CreatedAt).Select(n=> new { n.Id, n.Title, n.Message, n.Type, n.IsRead, n.CreatedAt, n.ActionUrl, n.Priority });
            return Json(result);
        }

        [HttpPost("MarkAsRead/{id}")] 
        public async Task<IActionResult> MarkAsRead(int id)
        { await _notificationService.MarkAsReadAsync(id); return Json(new { success = true }); }

        [HttpPost("MarkAllAsRead")] 
        public async Task<IActionResult> MarkAllAsRead()
        { var userId = GetCurrentUserId(); await _notificationService.MarkAllAsReadAsync(userId); return Json(new { success = true }); }

        [HttpPost("Delete/{id}")] 
        public async Task<IActionResult> Delete(int id)
        { await _notificationService.DeleteNotificationAsync(id); return Json(new { success = true }); }

        [HttpGet("GetUnreadCount")] 
        public async Task<IActionResult> GetUnreadCount()
        { var userId = GetCurrentUserId(); var count = await _notificationService.GetUnreadCountAsync(userId); return Json(new { unreadCount = count }); }
    }

    public class NotificationListViewModel { public int UnreadCount { get; set; } }
}
