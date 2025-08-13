using System.Security.Claims;
using Core.Entity;

namespace WebUI.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static UserRole GetUserRole(this ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

            if (roleClaim == null)
                return UserRole.Student;

            if (Enum.TryParse<UserRole>(roleClaim, out var role))
                return role;

            return UserRole.Student;
        }
    }
}