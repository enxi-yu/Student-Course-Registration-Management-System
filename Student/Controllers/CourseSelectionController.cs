using Microsoft.AspNetCore.Mvc;
using StudentCourse.Student.Services;

namespace StudentCourse.Student.Controllers
{
    /// <summary>
    /// 模块二：选课中心与课表管理
    /// </summary>
    [ApiController]
    public sealed class CourseSelectionController : ControllerBase
    {
        private readonly CourseSelectionService _service;

        public CourseSelectionController(CourseSelectionService service)
        {
            _service = service;
        }

        [HttpGet("api/student/courses/available")]
        public IActionResult GetAvailableCourses([FromQuery] string? semester)
        {
            return SafeOk(() => _service.GetAvailableCourses(semester ?? GetCurrentSemester()));
        }

        [HttpGet("api/student/courses/{classId:int}")]
        public IActionResult GetCourseDetail(int classId)
        {
            return SafeOk(() => _service.GetCourseDetail(classId));
        }

        [HttpPost("api/student/courses/select")]
        public IActionResult SelectCourse([FromBody] SelectCourseRequest request)
        {
            return SafeOk(() => _service.SelectCourse(request.ClassId));
        }

        [HttpPost("api/student/courses/drop")]
        public IActionResult DropCourse([FromBody] DropCourseRequest request)
        {
            return SafeOk(() => _service.DropCourse(request.ClassId));
        }

        [HttpGet("api/student/schedule")]
        public IActionResult GetWeeklySchedule([FromQuery] string? semester)
        {
            return SafeOk(() => _service.GetWeeklySchedule(semester ?? GetCurrentSemester()));
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

        private static string GetCurrentSemester()
        {
            DateTime now = DateTime.Now;
            int year = now.Year;
            string term = now.Month >= 8 || now.Month <= 1 ? "1" : "2";
            int startYear = now.Month >= 8 ? year : year - 1;
            return $"{startYear}-{startYear + 1}-{term}";
        }
    }

    public sealed class SelectCourseRequest
    {
        public int ClassId { get; set; }
    }

    public sealed class DropCourseRequest
    {
        public int ClassId { get; set; }
    }
}
