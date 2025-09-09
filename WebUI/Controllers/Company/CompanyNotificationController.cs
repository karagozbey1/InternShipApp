using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Business.Interfaces.Com;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Business.Interfaces.Chat;
using Business.Interfaces.Order;

namespace WebUI.Controllers.Company
{
    [Authorize(Roles = "Company")]
    [Route("Company/Notification")] // /Company/Notification
    public class CompanyNotificationController : BaseCompanyController
    {
        public CompanyNotificationController(
            IICompanyService companyService,
            IInternshipOfferService offerService,
            IInternshipApplicationService applicationService,
            IIUserService userService,
            IINotificationService notificationService,
            IIChatService chatService)
            : base(companyService, offerService, applicationService, userService, notificationService, chatService) { }

        [HttpGet("UnreadCount")]
        public async Task<IActionResult> GetUnreadNotificationCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Json(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CompanyNotificationController error: {ex}");
                return Json(new { unreadCount = 0 });
            }
        }
    }
}
