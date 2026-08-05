using System.Text.Json;
using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class SystemLogService
    {
        private readonly AdminRepository _adminRepository;

        public SystemLogService(AdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public IList<SystemLogDto> GetLogs(string? keyword, string? operationType, DateTime? startTime, DateTime? endTime)
        {
            AdminAuthService.RequireAdminSession();
            return _adminRepository.GetSystemLogs(keyword, operationType, startTime, endTime);
        }

        public void WriteCurrent(string operationType, string operationDesc, string targetId, string ipAddress, object? requestParams, string resultStatus = "成功", string errorMessage = "")
        {
            if (!UserSessionContext.HasSession)
            {
                return;
            }

            Write(UserSessionContext.Current.UserId, operationType, operationDesc, targetId, ipAddress, requestParams, resultStatus, errorMessage);
        }

        public void Write(int userId, string operationType, string operationDesc, string targetId, string ipAddress, object? requestParams, string resultStatus = "成功", string errorMessage = "")
        {
            if (userId <= 0)
            {
                return;
            }

            try
            {
                string requestJson = requestParams == null ? string.Empty : JsonSerializer.Serialize(requestParams);
                _adminRepository.InsertSystemLog(userId, operationType, operationDesc, targetId, ipAddress, requestJson, resultStatus, errorMessage);
            }
            catch
            {
                // Logging must not roll back the main administrator operation.
            }
        }
    }
}
