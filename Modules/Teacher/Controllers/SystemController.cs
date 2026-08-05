using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourse.Infrastructure;

namespace StudentCourse.Controllers
{
    [ApiController]
    public sealed class SystemController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("api/system/ping")]
        public IActionResult Ping() => Ok(new { message = "pong", time = DateTime.UtcNow });

        [Authorize(Roles = "Teacher")]
        [HttpGet("api/system/database")]
        public IActionResult TestDatabase() => Ok(DbConnectionFactory.TestConnection());
    }
}
