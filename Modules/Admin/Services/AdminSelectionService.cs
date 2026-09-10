using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    // 管理员代学生选课业务：权限校验、操作日志
    public sealed class AdminSelectionService
    {
        private readonly AdminSelectionRepository _selectionRepository;
        private readonly SystemLogService _systemLogService;

        public AdminSelectionService(AdminSelectionRepository selectionRepository, SystemLogService systemLogService)
        {
            _selectionRepository = selectionRepository;
            _systemLogService = systemLogService;
        }

        // 查询可选教学班列表
        public IList<AdminSelectionClassDto> GetSelectableClasses(string? semester, string? keyword)
        {
            string? department = AdminAuthService.GetDepartmentScope();
            return _selectionRepository.GetSelectableClasses(semester, keyword, department);
        }

        public IList<AdminSelectionBatchDto> GetBatchesForStudent(string studentNo){
            AdminAuthService.RequireAdminSession();
            return _selectionRepository.GetBatchesForStudent(studentNo);
        }
        public IList<AdminStudentScheduleDto> GetStudentSchedule(string studentNo){
            string? department = AdminAuthService.GetDepartmentScope();
            return _selectionRepository.GetStudentSchedule(studentNo, department);
        }
        public IList<AdminSelectionClassDto> GetAllClassesForStudent(string studentNo){
            string? department = AdminAuthService.GetDepartmentScope();
            return _selectionRepository.GetAllClassesForStudent(studentNo, department);
        }
        public IList<AdminStudentScheduleDto> GetAllClassSchedules(){
            string? department = AdminAuthService.GetDepartmentScope();
            return _selectionRepository.GetAllClassSchedules(department);
        }
        public IList<AdminSelectionClassDto> GetBatchClassesForStudent(string studentNo,int batchId){
            string? department = AdminAuthService.GetDepartmentScope();
            return _selectionRepository.GetBatchClassesForStudent(studentNo, batchId, department);
        }

        // 查询某学生已选课程
        public IList<AdminEnrollmentDto> GetStudentEnrollments(string studentNo, string? semester)
        {
            string? department = AdminAuthService.GetDepartmentScope();
            return _selectionRepository.GetStudentEnrollments(studentNo, semester, department);
        }

        // 管理员代学生选课
        public AdminSelectionResultDto SelectForStudent(string studentNo, int classId, bool force, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            AdminAuthService.EnsureDepartmentAccess(_selectionRepository.GetClassDepartment(classId), "代选课程");

            AdminSelectionResultDto result = _selectionRepository.SelectForStudent(studentNo, classId, force);

            _systemLogService.WriteCurrent(
                force ? "代选课(超员)" : "代选课",
                result.Success ? "管理员代学生选课" : "管理员代学生选课失败",
                $"{studentNo}:{classId}",
                ipAddress,
                new { studentNo, classId, force, result.Success, result.Message, result.RequireCapacityConfirm, conflicts = result.ConflictCourses },
                result.Success ? string.Empty : result.Message);

            return result;
        }

        // 管理员代学生退课
        public AdminSelectionResultDto DropForStudent(string studentNo, int classId, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            AdminAuthService.EnsureDepartmentAccess(_selectionRepository.GetClassDepartment(classId), "代退课程");

            AdminSelectionResultDto result = _selectionRepository.DropForStudent(studentNo, classId);

            _systemLogService.WriteCurrent(
                "代退课",
                result.Success ? "管理员代学生退课" : "管理员代学生退课失败",
                $"{studentNo}:{classId}",
                ipAddress,
                new { studentNo, classId, result.Success, result.Message },
                result.Success ? "成功" : "失败",
                result.Success ? string.Empty : result.Message);

            return result;
        }
    }
}
