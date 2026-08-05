using System.Security.Claims;
using StudentCourse.Models;

namespace StudentCourse.Services
{
    public static class UserSessionContext
    {
        private static IHttpContextAccessor? _accessor;
        public static void Configure(IHttpContextAccessor accessor) => _accessor = accessor;
        public static UserSession? Current => FromPrincipal(_accessor?.HttpContext?.User);
        public static bool HasSession => Current is not null;

        private static UserSession? FromPrincipal(ClaimsPrincipal? principal)
        {
            if (principal?.Identity?.IsAuthenticated != true) return null;
            int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            return new UserSession
            {
                UserId = userId,
                Username = principal.Identity.Name ?? string.Empty,
                RealName = principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                UserType = principal.IsInRole("Teacher") ? 1 : -1,
                TeacherNo = principal.FindFirstValue("teacher_no") ?? string.Empty,
                Title = principal.FindFirstValue("title") ?? string.Empty,
                Department = principal.FindFirstValue("department") ?? string.Empty,
                IsLoggedIn = true
            };
        }
    }
}
