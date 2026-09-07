using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using StudentCourse.Models;
using StudentCourse.Services;
using System.Security.Claims;

namespace StudentCourse.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public sealed class AdminController : ControllerBase
    {
        private readonly AdminAuthService _adminAuthService;
        private readonly AdminUserService _adminUserService;
        private readonly SelectionBatchService _selectionBatchService;
        private readonly AdminClassService _adminClassService;
        private readonly SystemLogService _systemLogService;
        private readonly AdminCourseService _adminCourseService;
        private readonly AdminApplicationService _adminApplicationService;
        private readonly AdminSelectionService _adminSelectionService;
        private readonly SchedulingService _schedulingService;
        private readonly SystemLogExportService _systemLogExportService;
        private readonly EvaluationService _evaluationService;
        private readonly EvaluationExportService _evaluationExportService;

        public AdminController(
            AdminAuthService adminAuthService,
            AdminUserService adminUserService,
            SelectionBatchService selectionBatchService,
            AdminClassService adminClassService,
            SystemLogService systemLogService,
            AdminCourseService adminCourseService,
            AdminApplicationService adminApplicationService,
            AdminSelectionService adminSelectionService,
            SchedulingService schedulingService,
            SystemLogExportService systemLogExportService,
            EvaluationService evaluationService,
            EvaluationExportService evaluationExportService)
        {
            _adminAuthService = adminAuthService;
            _adminUserService = adminUserService;
            _selectionBatchService = selectionBatchService;
            _adminClassService = adminClassService;
            _systemLogService = systemLogService;
            _adminCourseService = adminCourseService;
            _adminApplicationService = adminApplicationService;
            _adminSelectionService = adminSelectionService;
            _schedulingService = schedulingService;
            _systemLogExportService = systemLogExportService;
            _evaluationService = evaluationService;
            _evaluationExportService = evaluationExportService;
        }

        [HttpGet("evaluations")]
        public IActionResult GetEvaluations([FromQuery] string? keyword, [FromQuery] string? semester) => SafeOk(() => _evaluationService.Search(keyword, semester));

        [HttpGet("evaluations/export")]
        public IActionResult ExportEvaluations([FromQuery] string? keyword, [FromQuery] string? semester)
        {
            try { var rows=_evaluationService.Search(keyword,semester); return File(_evaluationExportService.BuildExcel(rows),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",$"course_evaluations_{DateTime.Now:yyyyMMdd}.xlsx"); }
            catch(InvalidOperationException ex){ return BadRequest(new { message=ex.Message }); }
        }

        [HttpGet("courses")]
        public IActionResult GetCourses([FromQuery] string? keyword,[FromQuery] string? coursetype)
        {
            return SafeOk(() => _adminCourseService.GetCourses(keyword,coursetype));
        }

        [HttpGet("courses/{courseId:int}")]
        public IActionResult GetCourse(int courseId)
        {
            return SafeOk(() => _adminCourseService.GetCourse(courseId));
        }

        [HttpPost("courses")]
        public IActionResult CreateCourse([FromBody] CourseDto input)
        {
            return SafeOk(() => _adminCourseService.CreateCourse(input, ClientIp()));
        }

        [HttpPut("courses/{courseId:int}")]
        public IActionResult UpdateCourse(int courseId, [FromBody] CourseDto input)
        {
            return SafeOk(() => _adminCourseService.UpdateCourse(courseId, input, ClientIp()));
        }

        [HttpDelete("courses/{courseId:int}")]
        public IActionResult DeleteCourse(int courseId)
        {
            return SafeOk(() =>
            {
                _adminCourseService.DeleteCourse(courseId, ClientIp());
                return new { deleted = true };
            });
        }

        [HttpGet("applications")]
        public IActionResult GetApplications([FromQuery] string? keyword,[FromQuery] string? status)
        {
            return SafeOk(() => _adminApplicationService.GetApplications(keyword,status));
        }

        [HttpGet("applications/{applyId}")]
        public IActionResult GetApplication(string applyId)
        {
            return SafeOk(() => _adminApplicationService.GetApplication(applyId));
        }

        [HttpPut("applications/{applyId}/approve")]
        public IActionResult ApproveApplication(string applyId, [FromBody] ApprovalRequest request)
        {
            return SafeOk(() => _adminApplicationService.ApproveApplication(applyId, request, ClientIp()));
        }

        [HttpGet("current")]
        public IActionResult GetCurrent()
        {
            return SafeOk(() => _adminAuthService.GetCurrent());
        }

        [HttpGet("scheduling/courses")]
        public IActionResult SearchSchedulingCourses([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => SafeOk(() => _schedulingService.SearchCourses(keyword, page, pageSize));

        [HttpGet("scheduling/teachers")]
        public IActionResult SearchSchedulingTeachers([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => SafeOk(() => _schedulingService.SearchTeachers(keyword, page, pageSize));

        [HttpGet("scheduling")]
        public IActionResult GetSchedules([FromQuery] string? semester) => SafeOk(() => _schedulingService.GetSchedules(semester));

        [HttpGet("scheduling/{classId:int}")]
        public IActionResult GetSchedule(int classId) => SafeOk(() => _schedulingService.GetSchedule(classId));

        [HttpPost("scheduling")]
        public IActionResult CreateSchedule([FromBody] SchedulingInput input) => SafeOk(() => _schedulingService.Create(input, ClientIp()));

        [HttpPut("scheduling/{classId:int}")]
        public IActionResult UpdateSchedule(int classId, [FromBody] SchedulingInput input) => SafeOk(() => _schedulingService.Update(classId, input, ClientIp()));

        [HttpDelete("scheduling/{classId:int}")]
        public IActionResult DeleteSchedule(int classId)
        {
            return SafeOk(() => { _schedulingService.Delete(classId, ClientIp()); return new { deleted = true }; });
        }

        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            return SafeOk(() => _adminAuthService.GetDashboard());
        }

        [HttpGet("permissions")]
        public IActionResult GetPermissions()
        {
            return SafeOk(() => _adminAuthService.GetPermissions());
        }

        [HttpGet("students")]
        public IActionResult GetStudents([FromQuery] string? keyword)
        {
            return SafeOk(() => _adminUserService.GetStudents(keyword));
        }

        [HttpPost("students")]
        public IActionResult CreateStudent([FromBody] AdminUserInput input)
        {
            return SafeOk(() => _adminUserService.CreateStudent(input, ClientIp()));
        }

        [HttpPut("students/{userId:int}")]
        public IActionResult UpdateStudent(int userId, [FromBody] AdminUserInput input)
        {
            return SafeOk(() => _adminUserService.UpdateStudent(userId, input, ClientIp()));
        }

        [HttpPut("students/{userId:int}/disable")]
        public IActionResult DisableStudent(int userId)
        {
            return SafeOk(() =>
            {
                _adminUserService.DisableUser(userId, ClientIp());
                return new { disabled = true };
            });
        }

        [HttpPut("students/{userId:int}/enable")]
        public IActionResult EnableStudent(int userId)
        {
            return SafeOk(() =>
            {
                _adminUserService.EnableUser(userId, ClientIp());
                return new { enabled = true };
            });
        }

        [HttpPut("students/{userId:int}/password")]
        public IActionResult ResetStudentPassword(int userId, [FromBody] ResetPasswordRequest request)
        {
            return SafeOk(() =>
            {
                _adminUserService.ResetPassword(userId, request, ClientIp());
                return new { reset = true };
            });
        }

        [HttpGet("teachers")]
        public IActionResult GetTeachers([FromQuery] string? keyword)
        {
            return SafeOk(() => _adminUserService.GetTeachers(keyword));
        }

        [HttpPost("teachers")]
        public IActionResult CreateTeacher([FromBody] AdminUserInput input)
        {
            return SafeOk(() => _adminUserService.CreateTeacher(input, ClientIp()));
        }

        [HttpPut("teachers/{userId:int}")]
        public IActionResult UpdateTeacher(int userId, [FromBody] AdminUserInput input)
        {
            return SafeOk(() => _adminUserService.UpdateTeacher(userId, input, ClientIp()));
        }

        [HttpPut("teachers/{userId:int}/disable")]
        public IActionResult DisableTeacher(int userId)
        {
            return SafeOk(() =>
            {
                _adminUserService.DisableUser(userId, ClientIp());
                return new { disabled = true };
            });
        }

        [HttpPut("teachers/{userId:int}/enable")]
        public IActionResult EnableTeacher(int userId)
        {
            return SafeOk(() =>
            {
                _adminUserService.EnableUser(userId, ClientIp());
                return new { enabled = true };
            });
        }

        [HttpPut("teachers/{userId:int}/password")]
        public IActionResult ResetTeacherPassword(int userId, [FromBody] ResetPasswordRequest request)
        {
            return SafeOk(() =>
            {
                _adminUserService.ResetPassword(userId, request, ClientIp());
                return new { reset = true };
            });
        }

        [HttpGet("batches")]
        public IActionResult GetBatches()
        {
            return SafeOk(() => _selectionBatchService.GetBatches());
        }

        [HttpPost("batches")]
        public IActionResult CreateBatch([FromBody] SelectionBatchInput input)
        {
            return SafeOk(() => _selectionBatchService.Create(input, ClientIp()));
        }

        [HttpPut("batches/{batchId:int}")]
        public IActionResult UpdateBatch(int batchId, [FromBody] SelectionBatchInput input)
        {
            return SafeOk(() => _selectionBatchService.Update(batchId, input, ClientIp()));
        }

        [HttpPut("batches/{batchId:int}/end")]
        public IActionResult EndBatch(int batchId)
        {
            return SafeOk(() => _selectionBatchService.End(batchId, ClientIp()));
        }

        [HttpGet("classes")]
        public IActionResult GetClasses([FromQuery] string? keyword)
        {
            return SafeOk(() => _adminClassService.GetClasses(keyword));
        }

        [HttpPut("classes/{classId:int}/capacity")]
        public IActionResult UpdateCapacity(int classId, [FromBody] CapacityUpdateRequest request)
        {
            return SafeOk(() => _adminClassService.UpdateCapacity(classId, request, ClientIp()));
        }

        [HttpGet("logs")]
        public IActionResult GetLogs([FromQuery] string? keyword, [FromQuery] string? operationType, [FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return SafeOk(() => _systemLogService.GetLogs(keyword, operationType, startTime, endTime, page, pageSize));
        }

        [HttpGet("batches/{batchId:int}/offerings")]
        public IActionResult GetBatchOfferings(int batchId)
        {
            return SafeOk(() => _selectionBatchService.GetOfferings(batchId));
        }

        [HttpPut("batches/{batchId:int}/offerings")]
        public IActionResult SaveBatchOfferings(int batchId, [FromBody] SaveBatchOfferingsRequest request)
        {
            return SafeOk(() => { _selectionBatchService.SaveOfferings(batchId, request, ClientIp()); return new { saved = true }; });
        }

        [HttpGet("logs/export")]
        public IActionResult ExportLogs([FromQuery] string? keyword, [FromQuery] string? operationType, [FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime)
        {
            try
            {
                IList<SystemLogDto> logs = _systemLogService.GetLogsForExport(keyword, operationType, startTime, endTime);
                if (logs.Count == 0) return BadRequest(new { message = "当前筛选条件下没有可导出的日志" });
                return File(_systemLogExportService.BuildExcel(logs), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"system_logs_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet("selection/classes")]
        public IActionResult GetSelectionClasses([FromQuery] string? semester, [FromQuery] string? keyword)
        {
            return SafeOk(() => _adminSelectionService.GetSelectableClasses(semester, keyword));
        }

        [HttpGet("selection/students/{studentNo}/batches")]
        public IActionResult GetSelectionBatchesForStudent(string studentNo) => SafeOk(() => _adminSelectionService.GetBatchesForStudent(studentNo));

        [HttpGet("selection/students/{studentNo}/schedule")]
        public IActionResult GetSelectionStudentSchedule(string studentNo) => SafeOk(() => _adminSelectionService.GetStudentSchedule(studentNo));

        [HttpGet("selection/students/{studentNo}/classes")]
        public IActionResult GetAllSelectionClassesForStudent(string studentNo) => SafeOk(() => _adminSelectionService.GetAllClassesForStudent(studentNo));

        [HttpGet("selection/class-schedules")]
        public IActionResult GetAllSelectionClassSchedules() => SafeOk(() => _adminSelectionService.GetAllClassSchedules());

        [HttpGet("selection/students/{studentNo}/batches/{batchId:int}/classes")]
        public IActionResult GetBatchClassesForStudent(string studentNo,int batchId) => SafeOk(() => _adminSelectionService.GetBatchClassesForStudent(studentNo,batchId));

        [HttpGet("selection/students/{studentNo}/enrollments")]
        public IActionResult GetStudentEnrollments(string studentNo, [FromQuery] string? semester)
        {
            return SafeOk(() => _adminSelectionService.GetStudentEnrollments(studentNo, semester));
        }

        [HttpPost("selection/students/{studentNo}/classes/{classId:int}")]
        public IActionResult SelectForStudent(string studentNo, int classId, [FromQuery] bool force = false)
        {
            return SafeOk(() => _adminSelectionService.SelectForStudent(studentNo, classId, force, ClientIp()));
        }

        [HttpDelete("selection/students/{studentNo}/classes/{classId:int}")]
        public IActionResult DropForStudent(string studentNo, int classId)
        {
            return SafeOk(() => _adminSelectionService.DropForStudent(studentNo, classId, ClientIp()));
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
            catch (Oracle.ManagedDataAccess.Client.OracleException ex)
            {
                Console.Error.WriteLine($"Oracle error {ex.Number} during {HttpContext.Request.Method} {HttpContext.Request.Path}: {ex.Message}");
                return StatusCode(500, new { message = "服务暂不可用，请稍后重试。", traceId = HttpContext.TraceIdentifier });
            }
        }

        private string ClientIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
    }
}
