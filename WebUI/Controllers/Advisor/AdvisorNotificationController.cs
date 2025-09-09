using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Interfaces.Order;
using Business.Interfaces.UserRepose;
using Business.Interfaces.Internship;
using Business.Interfaces;
using WebUI.Models.ViewModels.Advisor;
using Core.Entity;

namespace WebUI.Controllers.Advisor
{
    [Authorize(Roles = "Advisor")]
    [Route("Advisor/[controller]")]
    public class AdvisorNotificationController : Controller
    {
        private readonly IINotificationService _notificationService;
        private readonly IIUserService _userService;
        private readonly IInternshipApplicationService _applicationService;
        private readonly IDiaryService _diaryService;

        public AdvisorNotificationController(
            IINotificationService notificationService,
            IIUserService userService,
            IInternshipApplicationService applicationService,
            IDiaryService diaryService)
        {
            _notificationService = notificationService;
            _userService = userService;
            _applicationService = applicationService;
            _diaryService = diaryService;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(string type = "all", int page = 1, int pageSize = 10)
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var advisor = await _userService.GetUserWithUniversityAsync(advisorId);

                if (advisor?.UniversityId == null)
                {
                    TempData["ErrorMessage"] = "Danýþman bilgileri eksik.";
                    return RedirectToAction("Index", "AdvisorDashboard");
                }

                // Get notifications based on type filter
                List<Notification> notifications;
                
                switch (type?.ToLower())
                {
                    case "unread":
                        notifications = await _notificationService.GetUnreadNotificationsByUserIdAsync(advisorId);
                        break;
                    case "application":
                        notifications = await _notificationService.GetNotificationsByTypeAsync(advisorId, "Application");
                        break;
                    case "diary":
                        notifications = await _notificationService.GetNotificationsByTypeAsync(advisorId, "Diary");
                        break;
                    case "system":
                        notifications = await _notificationService.GetNotificationsByTypeAsync(advisorId, "System");
                        break;
                    default:
                        notifications = await _notificationService.GetAllNotificationsByUserIdAsync(advisorId);
                        break;
                }

                // Apply pagination
                var totalCount = notifications.Count;
                var paginatedNotifications = notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Get statistics
                var unreadCount = await _notificationService.GetUnreadCountAsync(advisorId);
                var todayCount = notifications.Count(n => n.CreatedAt.Date == DateTime.Today);
                var thisWeekCount = notifications.Count(n => n.CreatedAt >= DateTime.Today.AddDays(-7));

                var viewModel = new AdvisorNotificationViewModel
                {
                    Notifications = paginatedNotifications,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount,
                    CurrentFilter = type,
                    Statistics = new NotificationStatistics
                    {
                        UnreadCount = unreadCount,
                        TodayCount = todayCount,
                        ThisWeekCount = thisWeekCount,
                        TotalCount = notifications.Count
                    }
                };

                return View("~/Views/Advisor/Notification/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Bildirimler yüklenirken hata oluþtu: " + ex.Message;
                return View("~/Views/Advisor/Notification/Index.cshtml", new AdvisorNotificationViewModel());
            }
        }

        [HttpPost]
        [Route("MarkAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var advisorId = GetCurrentUserId();
                await _notificationService.MarkAllAsReadAsync(advisorId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _notificationService.DeleteNotificationAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetUnreadCount")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var unreadCount = await _notificationService.GetUnreadCountAsync(advisorId);
                return Json(new { unreadCount });
            }
            catch
            {
                return Json(new { unreadCount = 0 });
            }
        }

        [HttpGet]
        [Route("GetRecent")]
        public async Task<IActionResult> GetRecent(int count = 5)
        {
            try
            {
                var advisorId = GetCurrentUserId();
                var notifications = await _notificationService.GetRecentNotificationsAsync(advisorId, count);
                
                var result = notifications.Select(n => new {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    type = n.Type,
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                    actionUrl = n.ActionUrl,
                    iconClass = GetNotificationIcon(n.Type)
                });

                return Json(new { success = true, notifications = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("Filter")]
        public async Task<IActionResult> Filter(string type = "all")
        {
            try
            {
                var advisorId = GetCurrentUserId();
                List<Notification> notifications = type?.ToLower() switch
                {
                    "unread" => await _notificationService.GetUnreadNotificationsByUserIdAsync(advisorId),
                    "application" => await _notificationService.GetNotificationsByTypeAsync(advisorId, "Application"),
                    "diary" => await _notificationService.GetNotificationsByTypeAsync(advisorId, "Diary"),
                    "system" => await _notificationService.GetNotificationsByTypeAsync(advisorId, "System"),
                    _ => await _notificationService.GetAllNotificationsByUserIdAsync(advisorId)
                };

                var result = notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new
                    {
                        id = n.Id,
                        title = n.Title,
                        message = n.Message,
                        type = n.Type,
                        isRead = n.IsRead,
                        createdAt = n.CreatedAt,
                        actionUrl = n.ActionUrl,
                        priority = n.Priority
                    });

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private string GetNotificationIcon(string type)
        {
            return type?.ToLower() switch
            {
                "application" => "fas fa-file-alt",
                "diary" => "fas fa-book",
                "system" => "fas fa-cog",
                "warning" => "fas fa-exclamation-triangle",
                "error" => "fas fa-times-circle",
                "success" => "fas fa-check-circle",
                _ => "fas fa-bell"
            };
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}