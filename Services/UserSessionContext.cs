using System.Security.Claims;
using StudentCourse.Models;

namespace StudentCourse.Services
{
    /// <summary>Maps the authenticated request principal to the legacy service session model.</summary>
    public static class UserSessionContext
    {
        private static IHttpContextAccessor? _httpContextAccessor;

        public static UserSession? Current => FromPrincipal(_httpContextAccessor?.HttpContext?.User);

        public static bool HasSession => Current is not null;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private static UserSession? FromPrincipal(ClaimsPrincipal? principal)
        {
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            return new UserSession
            {
                UserId = userId,
                Username = principal.Identity.Name ?? string.Empty,
                RealName = principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                UserType = principal.IsInRole("Admin") ? 2 : principal.IsInRole("Teacher") ? 1 : 0,
                TeacherNo = principal.FindFirstValue("teacher_no") ?? string.Empty,
                Title = principal.FindFirstValue("title") ?? string.Empty,
                Department = principal.FindFirstValue("department") ?? string.Empty,
                AdminLevel=int.TryParse(principal.FindFirstValue("admin_level"),out int level)?level:-1,
                IsLoggedIn = true
            };
        }
    }
}
