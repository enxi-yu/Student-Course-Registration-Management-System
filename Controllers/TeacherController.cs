using Microsoft.AspNetCore.Mvc;
using StudentCourse.Models;
using StudentCourse.Services;

namespace StudentCourse.Controllers
{
    [ApiController]
    public sealed class TeacherController : ControllerBase
    {
        private readonly TeacherService _teacherService;
        private readonly TeacherStudentService _teacherStudentService;
        private readonly CourseApplicationService _courseApplicationService;
        private readonly ScoreService _scoreService;
        private readonly ExportService _exportService;

        public TeacherController(
            TeacherService teacherService,
            TeacherStudentService teacherStudentService,
            CourseApplicationService courseApplicationService,
            ScoreService scoreService,
            ExportService exportService)
        {
            _teacherService = teacherService;
            _teacherStudentService = teacherStudentService;
            _courseApplicationService = courseApplicationService;
            _scoreService = scoreService;
            _exportService = exportService;
        }

        [HttpGet("api/teacher/current")]
        public IActionResult GetCurrentTeacher()
        {
            return SafeOk(() => _teacherService.GetCurrentTeacher());
        }

        [HttpGet("api/teacher/dashboard")]
        public IActionResult GetDashboard([FromQuery] string? semester)
        {
            return SafeOk(() => _teacherService.GetDashboard(semester ?? string.Empty));
        }

        [HttpGet("api/teacher/courses")]
        public IActionResult GetMyCourses([FromQuery] string? semester)
        {
            return SafeOk(() => _teacherService.GetMyCourses(semester ?? string.Empty));
        }

        [HttpGet("api/teacher/schedule")]
        public IActionResult GetMySchedule([FromQuery] string? semester)
        {
            return SafeOk(() => _teacherService.GetMySchedule(semester ?? string.Empty));
        }

        [HttpGet("api/teacher/classes/{classId:int}/students")]
        public IActionResult GetClassStudents(int classId)
        {
            return SafeOk(() => _teacherStudentService.GetClassStudents(classId));
        }

        [HttpGet("api/teacher/classes/{classId:int}/students/export")]
        public IActionResult ExportClassStudentsCsv(int classId)
        {
            try
            {
                IList<StudentListDto> students = _teacherStudentService.GetClassStudents(classId);
                byte[] bytes = _exportService.BuildClassStudentsCsv(students);
                return File(bytes, "text/csv; charset=utf-8", $"class_students_{classId}.csv");
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

        [HttpGet("api/teacher/course-applications")]
        public IActionResult GetCourseApplications()
        {
            return SafeOk(() => _courseApplicationService.GetMine());
        }

        [HttpPost("api/teacher/course-applications")]
        public IActionResult SubmitCourseApplication([FromBody] CourseApplicationInput input)
        {
            return SafeOk(() => _courseApplicationService.Submit(input));
        }

        [HttpGet("api/teacher/classes/{classId:int}/scores")]
        public IActionResult GetScoreSheet(int classId)
        {
            return SafeOk(() => _scoreService.GetScoreSheet(classId));
        }

        [HttpPost("api/teacher/scores")]
        public IActionResult SaveScore([FromBody] ScoreSaveRequest request)
        {
            return SafeOk(() => _scoreService.SaveScore(request));
        }

        [HttpPost("api/teacher/classes/{classId:int}/scores/batch")]
        public IActionResult BatchSaveScores(int classId, [FromBody] ScoreSaveRequest[] rows)
        {
            return SafeOk(() => _scoreService.BatchSaveScores(classId, rows));
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
                return StatusCode(500, new { message = "数据库连接失败，请稍后重试或检查 Oracle 配置。", detail = ex.Message });
            }
        }

    }
}

