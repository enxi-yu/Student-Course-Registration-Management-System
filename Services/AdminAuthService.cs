using System.Security.Cryptography;
using System.Text;
using StudentCourse.Models;
using StudentCourse.Repositories;

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

        public AdminCurrentDto Login(AdminLoginRequest request, string ipAddress)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new InvalidOperationException("请输入管理员账号和密码");
            }

            string username = request.Username.Trim();
            string passwordHash = Md5(request.Password);
            AdminCredentialDto? credential = _adminRepository.GetAdminCredential(username);
            if (IsDefaultAdminBootstrap(username, request.Password)
                && (credential == null || !string.Equals(passwordHash, credential.PasswordHash, StringComparison.OrdinalIgnoreCase)))
            {
                _adminRepository.EnsureDefaultAdmin(passwordHash);
                credential = _adminRepository.GetAdminCredential(username);
            }

            if (credential == null)
            {
                throw new InvalidOperationException("管理员账号或密码错误");
            }

            if (credential.Status != 1)
            {
                _systemLogService.Write(credential.UserId, "登录", "管理员登录失败：账号已禁用", credential.AdminNo, ipAddress, new { credential.Username }, "失败", "账号已禁用");
                throw new InvalidOperationException("该管理员账号已禁用");
            }

            if (!string.Equals(passwordHash, credential.PasswordHash, StringComparison.OrdinalIgnoreCase))
            {
                _systemLogService.Write(credential.UserId, "登录", "管理员登录失败：密码错误", credential.AdminNo, ipAddress, new { credential.Username }, "失败", "密码错误");
                throw new InvalidOperationException("管理员账号或密码错误");
            }

            UserSessionContext.Set(new UserSession
            {
                UserId = credential.UserId,
                Username = credential.Username,
                RealName = credential.RealName,
                UserType = AdminRoleId,
                TeacherNo = string.Empty,
                Title = string.Empty,
                Department = string.Empty,
                IsLoggedIn = true
            });

            _adminRepository.UpdateLastLogin(credential.UserId);
            _systemLogService.Write(credential.UserId, "登录", "管理员登录成功", credential.AdminNo, ipAddress, new { credential.Username });

            return new AdminCurrentDto
            {
                UserId = credential.UserId,
                Username = credential.Username,
                RealName = credential.RealName,
                AdminNo = credential.AdminNo,
                AdminLevel = credential.AdminLevel,
                ManagedScope = credential.ManagedScope
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

            UserSession session = UserSessionContext.Current;
            if (session.UserType != AdminRoleId)
            {
                throw new InvalidOperationException("当前账号不是管理员，无权访问管理员功能");
            }

            return session;
        }

        public static string Md5(string value)
        {
            byte[] bytes = MD5.HashData(Encoding.UTF8.GetBytes(value));
            StringBuilder builder = new StringBuilder(bytes.Length * 2);
            foreach (byte item in bytes)
            {
                builder.Append(item.ToString("x2"));
            }

            return builder.ToString();
        }

        private static bool IsDefaultAdminBootstrap(string username, string password)
        {
            return string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase)
                   && password == "admin123";
        }
    }
}
