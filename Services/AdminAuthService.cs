using StudentCourse.Models;
using StudentCourse.Repositories;
using StudentCourse.Shared.Security;

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

        public const int SystemAdminLevel=0;
        public const int CourseAdminLevel=1;

        public static UserSession RequireSystemAdmin()
        {
            UserSession session=RequireAdminSession();
            if(session.AdminLevel!=SystemAdminLevel)
            {
                throw new InvalidOperationException("当前账号不是系统管理员，无权访问该功能");
            }
            return session;
        }

        public static UserSession RequireCourseAdmin()
        {
            UserSession session=RequireAdminSession();
            if(session.AdminLevel!=CourseAdminLevel)
            {
                throw new InvalidOperationException("当前账号不是教务管理员，无权访问该功能");
            }
            return session;
        }

    }
}
