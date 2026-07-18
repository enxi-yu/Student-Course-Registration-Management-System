using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class AdminClassService
    {
        private readonly AdminRepository _adminRepository;
        private readonly SystemLogService _systemLogService;

        public AdminClassService(AdminRepository adminRepository, SystemLogService systemLogService)
        {
            _adminRepository = adminRepository;
            _systemLogService = systemLogService;
        }

        public IList<AdminClassDto> GetClasses(string? keyword)
        {
            AdminAuthService.RequireAdminSession();
            return _adminRepository.GetClasses(keyword);
        }

        public AdminClassDto UpdateCapacity(int classId, CapacityUpdateRequest request, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            if (request == null)
            {
                throw new InvalidOperationException("请求参数不能为空");
            }

            AdminClassDto? current = _adminRepository.GetClassById(classId);
            if (current == null)
            {
                throw new InvalidOperationException("教学班不存在");
            }

            if (request.Capacity < current.SelectedCount)
            {
                _systemLogService.WriteCurrent("修改", "调整课程容量失败", Convert.ToString(classId), ipAddress, new
                {
                    classId,
                    oldCapacity = current.Capacity,
                    request.Capacity,
                    current.SelectedCount,
                    request.Remark
                }, "失败", "新容量小于已选人数");

                throw new InvalidOperationException("新容量不能小于已选人数");
            }

            AdminClassDto updated = _adminRepository.UpdateClassCapacity(classId, request.Capacity);
            _systemLogService.WriteCurrent("修改", "调整课程容量", Convert.ToString(classId), ipAddress, new
            {
                classId,
                oldCapacity = current.Capacity,
                newCapacity = updated.Capacity,
                updated.SelectedCount,
                request.Remark
            });

            return updated;
        }
    }
}
