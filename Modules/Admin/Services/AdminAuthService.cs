using StudentCourse.Models;
using StudentCourse.Repositories;
using StudentCourse.Shared.Security;
using System.Text.RegularExpressions;

namespace StudentCourse.Services
{
    public sealed class AdminAuthService
    {
        private const int AdminRoleId = 2;

        private readonly AdminRepository _adminRepository;
        private readonly SystemLogService _systemLogService;

        public AdminAuthService(AdminRepository adminRepository, SystemLogService systemLogService)
        {
            _adminRepository = adminRepository;
            _systemLogService = systemLogService;
        }

        public AdminCurrentDto Login(LoginRequest request, string ipAddress)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username)) throw new InvalidOperationException("用户名不能为空");
            if (string.IsNullOrWhiteSpace(request.Password)) throw new InvalidOperationException("密码不能为空");

            string username = request.Username.Trim();
            if (!username.StartsWith("A", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("管理员账号必须以 A 开头");
            }
            AdminCredentialDto? credential = _adminRepository.GetAdminCredential(username);

            if (credential == null)
            {
                throw new InvalidOperationException("用户名不存在");
            }

            if (credential.Status != 1)
            {
                _systemLogService.Write(credential.UserId, "登录", "管理员登录失败：账号已禁用", credential.AdminNo, ipAddress, new { credential.Username }, "失败", "账号已禁用");
                throw new InvalidOperationException("该管理员账号已禁用");
            }

            if (!PasswordHash.Verify(credential.PasswordHash, request.Password, out bool upgrade))
            {
                _systemLogService.Write(credential.UserId, "登录", "管理员登录失败：密码错误", credential.AdminNo, ipAddress, new { credential.Username }, "失败", "密码错误");
                throw new InvalidOperationException("密码错误");
            }

            if (upgrade)
            {
                _adminRepository.ResetPassword(credential.UserId, PasswordHash.Hash(request.Password));
            }

            _adminRepository.UpdateLastLogin(credential.UserId);
            _systemLogService.Write(credential.UserId, "登录", "管理员登录成功", credential.AdminNo, ipAddress, new { credential.Username });

            return new AdminCurrentDto
            {
                UserId = credential.UserId,
                Username = credential.Username,
                RealName = credential.RealName,
                AdminNo = credential.AdminNo,
                AdminLevel = credential.AdminLevel,
                Department = credential.Department
            };
        }

        public AdminCurrentDto GetCurrent()
        {
            UserSession session = RequireAdminSession();
            AdminCurrentDto? current = _adminRepository.GetCurrentAdmin(session.UserId);
            if (current == null)
            {
                throw new InvalidOperationException("当前管理员账号不存在");
            }

            return current;
        }

        public AdminDashboardDto GetDashboard()
        {
            UserSession session = RequireAdminSession();
            AdminDashboardDto? dashboard = _adminRepository.GetDashboard(session.UserId);
            if (dashboard == null)
            {
                throw new InvalidOperationException("当前管理员账号不存在");
            }

            return dashboard;
        }

        public IList<AdminPermissionDto> GetPermissions()
        {
            RequireAdminSession();
            return _adminRepository.GetPermissions(AdminRoleId);
        }

        public static UserSession RequireAdminSession()
        {
            if (!UserSessionContext.HasSession)
            {
                throw new InvalidOperationException("请先登录管理员账号");
            }

            UserSession session = UserSessionContext.Current!;
            if (session.UserType != AdminRoleId)
            {
                throw new InvalidOperationException("当前账号不是管理员，无权访问管理员功能");
            }

            return session;
        }

        public const int SuperAdminLevel=0;

        public static string? GetDepartmentScope()
        {
            UserSession session = RequireAdminSession();
            if (session.AdminLevel == SuperAdminLevel)
            {
                return null;
            }

            string department = (session.Department ?? string.Empty).Trim();
            if (department.Length == 0)
            {
                throw new InvalidOperationException("当前教务管理员未配置所属学院，请联系超级管理员");
            }

            return department;
        }

        public static void EnsureDepartmentAccess(string? resourceDepartment, string resourceName)
        {
            string? department = GetDepartmentScope();
            if (department == null)
            {
                return;
            }

            if (!string.Equals(department, (resourceDepartment ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"无权操作其他学院的{resourceName}");
            }
        }

        public static UserSession RequireSuperAdmin()
        {
            UserSession session=RequireAdminSession();
            if(session.AdminLevel!=SuperAdminLevel)
            {
                throw new InvalidOperationException("当前账号不是超级管理员，无权访问该功能");
            }
            return session;
        }

        public AdminCurrentDto UpdateProfile(UpdateAdminProfileRequest request)
        {
            RequireAdminSession();
            if (request == null)
            {
                throw new InvalidOperationException("个人资料请求不能为空。");
            }

            AdminCurrentDto admin = GetCurrent();
            string phone = (request.Phone ?? string.Empty).Trim();
            string email = (request.Email ?? string.Empty).Trim();

            if (!string.IsNullOrEmpty(phone) && !Regex.IsMatch(phone, @"^\d{11}$"))
            {
                throw new InvalidOperationException("手机号必须为 11 位数字。");
            }

            if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new InvalidOperationException("邮箱格式不正确。");
            }

            _adminRepository.UpdateProfile(admin.UserId, phone, email);
            admin.Phone = phone;
            admin.Email = email;
            return admin;
        }

        public object UpdatePassword(UpdateAdminPasswordRequest request)
        {
            UserSession session =RequireAdminSession();
            Validate(request);

            string? storedPassword = _adminRepository.GetAdminPassword(session.UserId);
            if( !PasswordHash.Verify(storedPassword, request.OldPassword.Trim(), out _))
            {
                throw new InvalidOperationException("原密码不正确，修改失败");
            }
            _adminRepository.ResetPassword(session.UserId,PasswordHash.Hash(request.NewPassword.Trim()));
            return new { Changed = true };
        }       

        private static void Validate(UpdateAdminPasswordRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("修改密码请求不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.OldPassword))
            {
                throw new InvalidOperationException("原密码不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                throw new InvalidOperationException("新密码不能为空。");
            }

            if (request.NewPassword.Trim().Length < 6 || request.NewPassword.Trim().Length > 20)
            {
                throw new InvalidOperationException("新密码长度必须为 6 到 20 位。");
            }

            if (request.NewPassword.Trim() != (request.ConfirmPassword ?? string.Empty).Trim())
            {
                throw new InvalidOperationException("两次输入的新密码不一致。");
            }

            if (request.OldPassword.Trim() == request.NewPassword.Trim())
            {
                throw new InvalidOperationException("新密码不能和原密码相同。");
            }
        }

    }
}
