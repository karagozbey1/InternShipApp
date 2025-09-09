using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;

namespace WebUI.Filters
{
    public class AuthExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is UnauthorizedAccessException)
            {
                // Sadece [Authorize] ile iþaretlenmiþ controller/action'larda çalýþsýn
                var controller = context.ActionDescriptor.RouteValues["controller"];
                var action = context.ActionDescriptor.RouteValues["action"];
                
               
                if (controller?.Equals("Home", StringComparison.OrdinalIgnoreCase) == true ||
                    controller?.Equals("Account", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return; 
                }
                
                // Login sayfasýna yönlendir
                context.Result = new RedirectToActionResult("Login", "Account", null);
                context.ExceptionHandled = true;
            }
        }
    }
}