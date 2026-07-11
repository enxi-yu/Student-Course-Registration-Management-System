using Microsoft.AspNetCore.Mvc;
using StudentCourse.Student.Services;
using StudentCourse.Student.Models;

namespace StudentCourse.Student.Controllers
{
    [ApiController]
    public sealed class StudentGradeController : ControllerBase
    {
        private readonly StudentGradeService _service;
        private readonly StudentEvaluationService _evaluationService;

        public StudentGradeController(StudentGradeService service, StudentEvaluationService evaluationService)
        {
            _service = service;
            _evaluationService = evaluationService;
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

        [HttpGet("api/student/evaluations")]
        public IActionResult GetEvaluableCourses()
        {
            return SafeOk(() => _evaluationService.GetEvaluableCourses());
        }

        [HttpPost("api/student/evaluations")]
        public IActionResult SubmitEvaluation([FromBody] SubmitEvaluationRequest request)
        {
            return SafeOk(() =>
            {
                _evaluationService.SubmitEvaluation(request.ClassId, request.Rating, request.Comment);
                return new { success = true };
            });
        }

        [HttpGet("api/student/evaluations/history")]
        public IActionResult GetEvaluationHistory()
        {
            return SafeOk(() => _evaluationService.GetEvaluationHistory());
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
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
