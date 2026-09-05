using System;
using Microsoft.AspNetCore.Mvc;
using StudentCourse.Student.Services;

namespace StudentCourse.Student.Controllers
{
    /// <summary>
    /// 模块二：选课中心与课表管理
    /// </summary>
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Student")]
    public sealed class CourseSelectionController : ControllerBase
    {
        private readonly CourseSelectionService _service;

        public CourseSelectionController(CourseSelectionService service)
        {
            _service = service;
        }

        [HttpGet("api/student/selection-batches")]
        public IActionResult GetSelectionBatches() => SafeOk(() => _service.GetSelectionBatches());

        [HttpGet("api/student/courses/available")]
        public IActionResult GetAvailableCourses([FromQuery] int batchId)
        {
            return SafeOk(() => _service.GetAvailableCourses(batchId));
        }

        [HttpGet("api/student/courses/{classId:int}")]
        public IActionResult GetCourseDetail(int classId)
        {
            return SafeOk(() => _service.GetCourseDetail(classId));
        }

        [HttpPost("api/student/courses/select")]
        public IActionResult SelectCourse([FromBody] SelectCourseRequest request)
        {
            return SafeOk(() => _service.SelectCourse(request.ClassId, request.BatchId));
        }

        [HttpPost("api/student/courses/drop")]
        public IActionResult DropCourse([FromBody] DropCourseRequest request)
        {
            return SafeOk(() => _service.DropCourse(request.ClassId));
        }

        [HttpGet("api/student/schedule")]
        public IActionResult GetWeeklySchedule([FromQuery] string? semester)
        {
            return SafeOk(() => _service.GetWeeklySchedule(semester ?? ""));
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
                return StatusCode(500, new { message = "服务暂不可用，请稍后重试。", traceId = HttpContext.TraceIdentifier });
            }
        }
    }

    public sealed class SelectCourseRequest
    {
        public int ClassId { get; set; }
        public int BatchId { get; set; }
    }

    public sealed class DropCourseRequest
    {
        public int ClassId { get; set; }
    }
}
