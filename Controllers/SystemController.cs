using Microsoft.AspNetCore.Mvc;
using StudentCourse.Infrastructure;
using StudentCourse.Services;

namespace StudentCourse.Controllers
{
    [ApiController]
    public sealed class SystemController : ControllerBase
    {
        [HttpGet("api/system/ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "pong", time = DateTime.Now });
        }

        [HttpGet("api/system/database")]
        public IActionResult TestDatabase()
        {
            return Ok(DbConnectionFactory.TestConnection());
        }

        [HttpPost("api/dev/session/teacher")]
        public IActionResult UseMockTeacherSession()
        {
            return Ok(UserSessionContext.UseDevelopmentTeacherSession());
        }

        [HttpPost("api/dev/session/student")]
        public IActionResult UseMockStudentSession()
        {
            return Ok(UserSessionContext.UseDevelopmentStudentSession());
        }

        [HttpPost("api/auth/logout")]
        public IActionResult Logout()
        {
            UserSessionContext.Clear();
            return Ok(new { loggedOut = true });
        }
    }
}
