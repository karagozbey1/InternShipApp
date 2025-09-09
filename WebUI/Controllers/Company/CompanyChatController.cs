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
    [Route("Company/Chat")] // /Company/Chat
    public class CompanyChatController : BaseCompanyController
    {
        public CompanyChatController(
            IICompanyService companyService,
            IInternshipOfferService offerService,
            IInternshipApplicationService applicationService,
            IIUserService userService,
            IINotificationService notificationService,
            IIChatService chatService)
            : base(companyService, offerService, applicationService, userService, notificationService, chatService) { }

        public class CreateChatWithStudentRequest { public int StudentId { get; set; } }

        [HttpPost("CreateOrGetWithStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrGetChatWithStudent([FromBody] CreateChatWithStudentRequest req)
        {
            try
            {
                var companyUserId = GetCurrentUserId();
                if (req == null || req.StudentId <= 0) return Json(new { success = false });
                var chatRoom = await _chatService.CreateOrGetChatRoomAsync(companyUserId, req.StudentId);
                if (chatRoom != null) return Json(new { success = true, conversationId = chatRoom.Id });
                return Json(new { success = false });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }
}
