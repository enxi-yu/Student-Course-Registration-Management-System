using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class AdminCourseService
    {
        private readonly AdminRepository _adminRepository;
        private readonly SystemLogService _systemLogService;

        public AdminCourseService(AdminRepository adminRepository, SystemLogService systemLogService)
        {
            _adminRepository = adminRepository;
            _systemLogService = systemLogService;
        }

        public IList<CourseDto> GetCourses()
        {
            AdminAuthService.RequireAdminSession();
            return _adminRepository.GetCourses();
        }

        public CourseDto GetCourse(int courseId)
        {
            AdminAuthService.RequireAdminSession();
            CourseDto? course = _adminRepository.GetCourseById(courseId);
            if (course == null)
            {
                throw new InvalidOperationException("课程不存在");
            }
            return course;
        }

        public CourseDto CreateCourse(CourseDto input, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            ValidateCourseInput(input);

            CourseDto created = _adminRepository.InsertCourse(input);
            _systemLogService.WriteCurrent("新增", "发布新课程", Convert.ToString(created.CourseId), ipAddress, input);
            return created;
        }

        public CourseDto UpdateCourse(int courseId, CourseDto input, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            ValidateCourseInput(input);

            CourseDto? current = _adminRepository.GetCourseById(courseId);
            if (current == null)
            {
                throw new InvalidOperationException("课程不存在");
            }

            CourseDto updated = _adminRepository.UpdateCourse(courseId, input);
            _systemLogService.WriteCurrent("修改", "更新课程信息", Convert.ToString(courseId), ipAddress, new { current, input });
            return updated;
        }

        public void DeleteCourse(int courseId, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();

            CourseDto? current = _adminRepository.GetCourseById(courseId);
            if (current == null)
            {
                throw new InvalidOperationException("课程不存在");
            }

            try
            {
                _adminRepository.DeleteCourse(courseId);
                _systemLogService.WriteCurrent("删除", "删除课程", Convert.ToString(courseId), ipAddress, current);
            }
            catch (InvalidOperationException ex)
            {
                _systemLogService.WriteCurrent("删除", "删除课程失败", Convert.ToString(courseId), ipAddress, current, "失败", ex.Message);
                throw;
            }
        }

        private static void ValidateCourseInput(CourseDto input)
        {
            if (input == null)
            {
                throw new InvalidOperationException("课程信息不能为空");
            }

            if (string.IsNullOrWhiteSpace(input.CourseName))
            {
                throw new InvalidOperationException("课程名称不能为空");
            }

            if (string.IsNullOrWhiteSpace(input.CourseType))
            {
                throw new InvalidOperationException("课程类型不能为空");
            }
        }
    }
}