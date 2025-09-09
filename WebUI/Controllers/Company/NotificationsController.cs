using Microsoft.AspNetCore.Mvc;
using Business.Interfaces.Order;
using Core.Entity;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;

namespace WebUI.Controllers.Company
{
    [Route("Company/[controller]")]
    public class NotificationsController : Controller
    {
        private readonly IINotificationService _notificationService;

        public NotificationsController(IINotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetAllNotificationsByUserIdAsync(userId);
            return View("~/Views/Company/Notifications/Index.cshtml", notifications);
        }

        [HttpGet("UnreadCount")]
        public async Task<IActionResult> GetUnreadNotificationCount()
        {
            var userId = GetCurrentUserId();
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { unreadCount });
        }

        [HttpPost("MarkAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            return Ok();
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> Filter(string type)
        {
            var userId = GetCurrentUserId();
            List<Notification> notifications;

            switch (type)
            {
                case "unread":
                    notifications = await _notificationService.GetUnreadNotificationsByUserIdAsync(userId);
                    break;
                case "application":
                    notifications = await _notificationService.GetNotificationsByTypeAsync(userId, "Application");
                    break;
                case "diary":
                    notifications = await _notificationService.GetNotificationsByTypeAsync(userId, "Diary");
                    break;
                case "system":
                    notifications = await _notificationService.GetNotificationsByTypeAsync(userId, "System");
                    break;
                default:
                    notifications = await _notificationService.GetAllNotificationsByUserIdAsync(userId);
                    break;
            }

            return Json(notifications);
        }
    }
}