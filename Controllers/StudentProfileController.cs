using Microsoft.AspNetCore.Mvc;
using StudentCourse.Services;

namespace StudentCourse.Controllers
{
    /// <summary>
    /// 模块一：个人信息与首页仪表盘
    /// </summary>
    [ApiController]
    public sealed class StudentProfileController : ControllerBase
    {
        private readonly StudentProfileService _service;

        public StudentProfileController(StudentProfileService service)
        {
            _service = service;
        }

        [HttpGet("api/student/current")]
        public IActionResult GetCurrentStudent()
        {
            return SafeOk(() => _service.GetCurrentStudent());
        }

        [HttpGet("api/student/dashboard")]
        public IActionResult GetDashboard()
        {
            return SafeOk(() => _service.GetDashboard());
        }

        private IActionResult SafeOk<T>(Func<T> action)
        {
            try
            {
                return Ok(action());
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误：" + ex.Message });
            }
        }
    }
}
