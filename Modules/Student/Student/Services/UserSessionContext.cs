using System.Security.Claims;
using StudentCourse.Student.Models;

namespace StudentCourse.Student.Services
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
                UserType = principal.IsInRole("Student") ? 0 : -1,
                StudentNo = principal.FindFirstValue("student_no") ?? string.Empty,
                Major = principal.FindFirstValue("major") ?? string.Empty,
                Grade = principal.FindFirstValue("grade") ?? string.Empty,
                IsLoggedIn = true
            };
        }
    }

    public static class StudentSessionHelper
    {
        public static UserSession RequireStudentSession()
        {
            UserSession? session = UserSessionContext.Current;
            if (session is null || session.UserType != 0) throw new InvalidOperationException("请以学生账号登录");
            return session;
        }

        public static void FillSessionFromStudent(UserSession session, StudentInfo info) { }
    }
}
