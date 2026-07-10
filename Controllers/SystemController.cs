using Microsoft.AspNetCore.Mvc;
using StudentCourse.Infrastructure;
using StudentCourse.Models;
using StudentCourse.Services;

namespace StudentCourse.Controllers
{
    [ApiController]
    public sealed class SystemController : ControllerBase
    {
        private readonly AccountService _accountService;

        public SystemController(AccountService accountService)
        {
            _accountService = accountService;
        }

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

        [HttpPost("api/auth/password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                return Ok(_accountService.ChangePassword(request));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "数据库连接失败，请稍后重试或检查 Oracle 配置。", detail = ex.Message });
            }
        }
    }
}
