using Microsoft.AspNetCore.Mvc;
using StudentCourse.Models;
using StudentCourse.Services;

namespace StudentCourse.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public sealed class AdminController : ControllerBase
    {
        private readonly AdminAuthService _adminAuthService;
        private readonly AdminUserService _adminUserService;
        private readonly SelectionBatchService _selectionBatchService;
        private readonly AdminClassService _adminClassService;
        private readonly SystemLogService _systemLogService;
        private readonly AdminCourseService _adminCourseService;
        private readonly AdminApplicationService _adminApplicationService;

        public AdminController(
            AdminAuthService adminAuthService,
            AdminUserService adminUserService,
            SelectionBatchService selectionBatchService,
            AdminClassService adminClassService,
            SystemLogService systemLogService,
            AdminCourseService adminCourseService,
            AdminApplicationService adminApplicationService)
        {
            _adminAuthService = adminAuthService;
            _adminUserService = adminUserService;
            _selectionBatchService = selectionBatchService;
            _adminClassService = adminClassService;
            _systemLogService = systemLogService;
            _adminCourseService = adminCourseService;
            _adminApplicationService = adminApplicationService;
        }

        [HttpGet("courses")]
        public IActionResult GetCourses()
        {
            return SafeOk(() => _adminCourseService.GetCourses());
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
        public IActionResult GetApplications()
        {
            return SafeOk(() => _adminApplicationService.GetApplications());
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

        [HttpPost("auth/login")]
        public IActionResult Login([FromBody] AdminLoginRequest request)
        {
            return SafeOk(() => _adminAuthService.Login(request, ClientIp()));
        }

        [HttpGet("current")]
        public IActionResult GetCurrent()
        {
            return SafeOk(() => _adminAuthService.GetCurrent());
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
        public IActionResult GetLogs([FromQuery] string? keyword, [FromQuery] string? operationType, [FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime)
        {
            return SafeOk(() => _systemLogService.GetLogs(keyword, operationType, startTime, endTime));
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
                return StatusCode(500, new { message = "数据库操作失败", detail = ex.Message });
            }
        }

        private string ClientIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
    }
}
