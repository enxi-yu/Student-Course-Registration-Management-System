using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class SelectionBatchService
    {
        private readonly AdminRepository _adminRepository;
        private readonly SystemLogService _systemLogService;

        public SelectionBatchService(AdminRepository adminRepository, SystemLogService systemLogService)
        {
            _adminRepository = adminRepository;
            _systemLogService = systemLogService;
        }

        public IList<SelectionBatchDto> GetBatches()
        {
            AdminAuthService.RequireAdminSession();
            return _adminRepository.GetBatches();
        }

        public SelectionBatchDto Create(SelectionBatchInput input, string ipAddress)
        {
            Validate(input);
            int status = NormalizeStatus(input);
            SelectionBatchDto batch = _adminRepository.InsertBatch(input, status);
            _systemLogService.WriteCurrent("新增", "新增选课批次", Convert.ToString(batch.BatchId), ipAddress, new { batch.BatchId, batch.BatchName, batch.StartTime, batch.EndTime, batch.Status });
            return batch;
        }

        public SelectionBatchDto Update(int batchId, SelectionBatchInput input, string ipAddress)
        {
            Validate(input);
            int status = NormalizeStatus(input);
            SelectionBatchDto batch = _adminRepository.UpdateBatch(batchId, input, status);
            _systemLogService.WriteCurrent("修改", "修改选课批次", Convert.ToString(batch.BatchId), ipAddress, new { batch.BatchId, batch.BatchName, batch.StartTime, batch.EndTime, batch.Status });
            return batch;
        }

        private static void Validate(SelectionBatchInput input)
        {
            if (input == null)
            {
                throw new InvalidOperationException("请求参数不能为空");
            }

            if (string.IsNullOrWhiteSpace(input.BatchName))
            {
                throw new InvalidOperationException("批次名称不能为空");
            }

            if (input.StartTime == default || input.EndTime == default)
            {
                throw new InvalidOperationException("开始时间和结束时间不能为空");
            }

            if (input.StartTime >= input.EndTime)
            {
                throw new InvalidOperationException("开始时间必须早于结束时间");
            }

            if (input.Status.HasValue && (input.Status.Value < 0 || input.Status.Value > 2))
            {
                throw new InvalidOperationException("批次状态只能为 0、1、2");
            }
        }

        private static int NormalizeStatus(SelectionBatchInput input)
        {
            if (input.Status.HasValue)
            {
                return input.Status.Value;
            }

            DateTime now = DateTime.Now;
            if (now < input.StartTime)
            {
                return 0;
            }

            return now <= input.EndTime ? 1 : 2;
        }
    }
}
