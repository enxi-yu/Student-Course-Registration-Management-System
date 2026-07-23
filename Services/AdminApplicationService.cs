using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class AdminApplicationService
    {
        private readonly AdminRepository _adminRepository;
        private readonly SystemLogService _systemLogService;

        public AdminApplicationService(AdminRepository adminRepository, SystemLogService systemLogService)
        {
            _adminRepository = adminRepository;
            _systemLogService = systemLogService;
        }

        public IList<CourseApplicationDto> GetApplications()
        {
            AdminAuthService.RequireAdminSession();
            return _adminRepository.GetApplications();
        }

        public CourseApplicationDto GetApplication(string applyId)
        {
            AdminAuthService.RequireAdminSession();
            CourseApplicationDto? application = _adminRepository.GetApplicationById(applyId);
            if (application == null)
            {
                throw new InvalidOperationException("申请不存在");
            }
            return application;
        }

        public CourseApplicationDto ApproveApplication(string applyId, ApprovalRequest request, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();

            if (request == null)
            {
                throw new InvalidOperationException("请求参数不能为空");
            }

            if (string.IsNullOrEmpty(request.Status))
            {
                throw new InvalidOperationException("审批状态不能为空");
            }

            if (request.Status != "通过" && request.Status != "驳回")
            {
                throw new InvalidOperationException("审批状态只能为'通过'或'驳回'");
            }

            CourseApplicationDto updated = _adminRepository.ApproveApplication(applyId, request.Status, request.Comment);
            _systemLogService.WriteCurrent("审批", $"开课申请{request.Status}", applyId, ipAddress, new { applyId, request });
            return updated;
        }
    }
}