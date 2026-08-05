using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourse.Models;
using StudentCourse.Services;

namespace StudentCourse.Controllers
{
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public sealed class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;
        public AccountController(AccountService accountService) => _accountService = accountService;

        [HttpPost("api/auth/password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try { return Ok(_accountService.ChangePassword(request)); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
