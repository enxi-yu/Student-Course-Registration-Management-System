using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class AdminUserService
    {
        private readonly AdminRepository _adminRepository;
        private readonly SystemLogService _systemLogService;

        public AdminUserService(AdminRepository adminRepository, SystemLogService systemLogService)
        {
            _adminRepository = adminRepository;
            _systemLogService = systemLogService;
        }

        public IList<AdminStudentDto> GetStudents(string? keyword)
        {
            AdminAuthService.RequireAdminSession();
            return _adminRepository.GetStudents(keyword);
        }

        public AdminStudentDto CreateStudent(AdminUserInput input, string ipAddress)
        {
            ValidateCommon(input, requirePassword: true);
            Require(input.StudentNo, "学号不能为空");
            Require(input.Major, "专业不能为空");
            Require(input.Grade, "年级不能为空");
            ValidateGpa(input.AvgGpa);

            AdminStudentDto student = _adminRepository.InsertStudent(input, AdminAuthService.Md5(input.Password));
            _systemLogService.WriteCurrent("新增", "新增学生账号", student.StudentNo, ipAddress, new { student.UserId, student.StudentNo, student.RealName });
            return student;
        }

        public AdminStudentDto UpdateStudent(int userId, AdminUserInput input, string ipAddress)
        {
            ValidateCommon(input, requirePassword: false);
            Require(input.StudentNo, "学号不能为空");
            Require(input.Major, "专业不能为空");
            Require(input.Grade, "年级不能为空");
            ValidateGpa(input.AvgGpa);

            AdminStudentDto student = _adminRepository.UpdateStudent(userId, input);
            _systemLogService.WriteCurrent("修改", "修改学生信息", student.StudentNo, ipAddress, new { student.UserId, student.StudentNo, student.RealName });
            return student;
        }

        public IList<AdminTeacherDto> GetTeachers(string? keyword)
        {
            AdminAuthService.RequireAdminSession();
            return _adminRepository.GetTeachers(keyword);
        }

        public AdminTeacherDto CreateTeacher(AdminUserInput input, string ipAddress)
        {
            ValidateCommon(input, requirePassword: true);
            Require(input.TeacherNo, "教师工号不能为空");
            Require(input.Title, "职称不能为空");
            Require(input.Department, "所属院系不能为空");

            AdminTeacherDto teacher = _adminRepository.InsertTeacher(input, AdminAuthService.Md5(input.Password));
            _systemLogService.WriteCurrent("新增", "新增教师账号", teacher.TeacherNo, ipAddress, new { teacher.UserId, teacher.TeacherNo, teacher.RealName });
            return teacher;
        }

        public AdminTeacherDto UpdateTeacher(int userId, AdminUserInput input, string ipAddress)
        {
            ValidateCommon(input, requirePassword: false);
            Require(input.TeacherNo, "教师工号不能为空");
            Require(input.Title, "职称不能为空");
            Require(input.Department, "所属院系不能为空");

            AdminTeacherDto teacher = _adminRepository.UpdateTeacher(userId, input);
            _systemLogService.WriteCurrent("修改", "修改教师信息", teacher.TeacherNo, ipAddress, new { teacher.UserId, teacher.TeacherNo, teacher.RealName });
            return teacher;
        }

        public void DisableUser(int userId, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            _adminRepository.SetUserStatus(userId, 0);
            _systemLogService.WriteCurrent("修改", "禁用用户账号", Convert.ToString(userId), ipAddress, new { userId, status = 0 });
        }

        public void EnableUser(int userId, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            _adminRepository.SetUserStatus(userId, 1);
            _systemLogService.WriteCurrent("修改", "启用用户账号", Convert.ToString(userId), ipAddress, new { userId, status = 1 });
        }

        public void ResetPassword(int userId, ResetPasswordRequest request, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            if (request == null || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new InvalidOperationException("新密码不能为空");
            }

            _adminRepository.ResetPassword(userId, AdminAuthService.Md5(request.Password));
            _systemLogService.WriteCurrent("修改", "重置用户密码", Convert.ToString(userId), ipAddress, new { userId });
        }

        private static void ValidateCommon(AdminUserInput input, bool requirePassword)
        {
            if (input == null)
            {
                throw new InvalidOperationException("请求参数不能为空");
            }

            Require(input.Username, "登录账号不能为空");
            Require(input.RealName, "姓名不能为空");

            if (requirePassword)
            {
                Require(input.Password, "登录密码不能为空");
            }

            if (!string.IsNullOrWhiteSpace(input.Phone) && input.Phone.Trim().Length != 11)
            {
                throw new InvalidOperationException("联系电话必须为 11 位");
            }

            if (!string.IsNullOrWhiteSpace(input.Email) && !input.Email.Contains("@"))
            {
                throw new InvalidOperationException("电子邮箱格式不正确");
            }
        }

        private static void ValidateGpa(decimal value)
        {
            if (value < 0 || value > 5)
            {
                throw new InvalidOperationException("平均绩点必须在 0.00 到 5.00 之间");
            }
        }

        private static void Require(string? value, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
