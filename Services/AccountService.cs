using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class AccountService
    {
        private readonly AccountRepository _accountRepository;

        public AccountService()
            : this(new AccountRepository())
        {
        }

        public AccountService(AccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public object ChangePassword(ChangePasswordRequest request)
        {
            UserSession session = RequireSession();
            Validate(request);

            bool changed = _accountRepository.ChangePassword(
                session.UserId,
                request.OldPassword.Trim(),
                request.NewPassword.Trim());

            if (!changed)
            {
                throw new InvalidOperationException("原密码不正确，修改失败。");
            }

            return new { Changed = true };
        }

        private static UserSession RequireSession()
        {
            if (!UserSessionContext.HasSession)
            {
                throw new InvalidOperationException("请先登录");
            }

            return UserSessionContext.Current;
        }

        private static void Validate(ChangePasswordRequest request)
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

            if (request.NewPassword.Trim().Length < 6)
            {
                throw new InvalidOperationException("新密码长度不能少于 6 位。");
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
