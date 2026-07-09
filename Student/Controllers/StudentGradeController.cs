using Microsoft.AspNetCore.Mvc;
using StudentCourse.Student.Services;

namespace StudentCourse.Student.Controllers
{
    /// <summary>
    /// 模块三：成绩查询与课程评价
    /// </summary>
    [ApiController]
    public sealed class StudentGradeController : ControllerBase
    {
        private readonly StudentGradeService _service;

        public StudentGradeController(StudentGradeService service)
        {
            _service = service;
        }

        [HttpGet("api/student/grades")]
        public IActionResult GetEnrolledCourses()
        {
            return SafeOk(() => _service.GetEnrolledCourses());
        }

        [HttpGet("api/student/gpa")]
        public IActionResult GetGpaSummary()
        {
            return SafeOk(() => _service.GetGpaSummary());
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
