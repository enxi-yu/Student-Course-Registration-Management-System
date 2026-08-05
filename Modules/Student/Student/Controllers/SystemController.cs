using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourse.Infrastructure;

namespace StudentCourse.Student.Controllers
{
    [ApiController]
    public sealed class SystemController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("api/system/ping")]
        public IActionResult Ping() => Ok(new { message = "pong", time = DateTime.UtcNow });

        [Authorize(Roles = "Student")]
        [HttpGet("api/system/database")]
        public IActionResult TestDatabase() => Ok(DbConnectionFactory.TestConnection());
    }
}
