using Microsoft.AspNetCore.Authentication;
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
        public IActionResult Ping()
        {
            return Ok(new { message = "pong", time = DateTime.UtcNow });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("api/system/database")]
        public IActionResult TestDatabase()
        {
            return Ok(DbConnectionFactory.TestConnection());
        }

        [AllowAnonymous]
        [HttpPost("api/auth/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok(new { loggedOut = true });
        }
    }
}
