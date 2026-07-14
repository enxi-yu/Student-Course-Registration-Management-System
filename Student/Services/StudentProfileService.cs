using StudentCourse.Student.Models;
using StudentCourse.Student.Repositories;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace StudentCourse.Student.Services
{
    public sealed class StudentProfileService
    {
        private readonly StudentProfileRepository _repository;

        public StudentProfileService()
            : this(new StudentProfileRepository())
        {
        }

        public StudentProfileService(StudentProfileRepository repository)
        {
            _repository = repository;
        }

        public StudentInfo GetCurrentStudent()
        {
            UserSession session = StudentSessionHelper.RequireStudentSession();
            StudentInfo? info = null;

            if (!string.IsNullOrWhiteSpace(session.StudentNo))
            {
                info = _repository.GetStudentInfoByStudentNo(session.StudentNo);
            }

            if (info == null && session.UserId > 0)
            {
                info = _repository.GetStudentInfoByUserId(session.UserId);
            }

            if (info == null)
            {
                info = new StudentInfo
                {
                    UserId = session.UserId,
                    Username = session.Username,
                    RealName = session.RealName,
                    StudentNo = session.StudentNo,
                    Major = session.Major,
                    Grade = session.Grade
                };
            }

            StudentSessionHelper.FillSessionFromStudent(session, info);
            return info;
        }

        public StudentDashboardDto GetDashboard()
        {
            StudentInfo student = GetCurrentStudent();
            return _repository.GetDashboard(student.StudentNo);
        }

        public StudentInfo UpdateProfile(UpdateStudentProfileRequest request)
        {
            UserSession session = StudentSessionHelper.RequireStudentSession();
            StudentInfo student = GetCurrentStudent();

            string phone = Normalize(request.Phone);
            string email = Normalize(request.Email);

            if (!string.IsNullOrEmpty(phone) && !Regex.IsMatch(phone, @"^\d{11}$"))
            {
                throw new InvalidOperationException("手机号必须为 11 位数字。");
            }

            if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new InvalidOperationException("邮箱格式不正确。");
            }

            _repository.UpdateContactInfo(student.UserId, phone, email);
            student.Phone = phone;
            student.Email = email;
            StudentSessionHelper.FillSessionFromStudent(session, student);
            return student;
        }

        public OperationResultDto ChangePassword(ChangePasswordRequest request)
        {
            StudentInfo student = GetCurrentStudent();

            string oldPassword = request.OldPassword ?? string.Empty;
            string newPassword = request.NewPassword ?? string.Empty;
            string confirmPassword = request.ConfirmPassword ?? string.Empty;

            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                throw new InvalidOperationException("请输入原密码。");
            }

            if (newPassword.Length < 6 || newPassword.Length > 20)
            {
                throw new InvalidOperationException("新密码长度必须为 6 到 20 位。");
            }

            if (newPassword != confirmPassword)
            {
                throw new InvalidOperationException("两次输入的新密码不一致。");
            }

            string storedPassword = _repository.GetPasswordHash(student.UserId);
            if (!PasswordMatches(storedPassword, oldPassword))
            {
                throw new InvalidOperationException("原密码不正确。");
            }

            _repository.UpdatePassword(student.UserId, FormatNewPassword(storedPassword, newPassword));
            return new OperationResultDto
            {
                Success = true,
                Message = "密码修改成功。"
            };
        }

        private static string Normalize(string? value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static bool PasswordMatches(string storedPassword, string inputPassword)
        {
            return string.Equals(storedPassword, inputPassword, StringComparison.Ordinal)
                || string.Equals(storedPassword, Md5Hex(inputPassword), StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatNewPassword(string storedPassword, string newPassword)
        {
            return IsMd5Hex(storedPassword) ? Md5Hex(newPassword) : newPassword;
        }

        private static bool IsMd5Hex(string value)
        {
            return value.Length == 32 && value.All(Uri.IsHexDigit);
        }

        private static string Md5Hex(string value)
        {
            byte[] bytes = MD5.HashData(Encoding.UTF8.GetBytes(value));
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
