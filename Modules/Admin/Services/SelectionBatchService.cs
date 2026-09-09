using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class SelectionBatchService
    {
        private readonly AdminRepository _adminRepository;
        private readonly SystemLogService _systemLogService;
        private readonly BatchOfferingRepository _batchOfferingRepository;

        public SelectionBatchService(AdminRepository adminRepository, SystemLogService systemLogService, BatchOfferingRepository batchOfferingRepository)
        {
            _adminRepository = adminRepository;
            _systemLogService = systemLogService;
            _batchOfferingRepository = batchOfferingRepository;
        }

        public IList<BatchOfferingDto> GetOfferings(int batchId)
        {
            AdminAuthService.RequireAdminSession();
            if (_adminRepository.GetBatchById(batchId) == null) throw new InvalidOperationException("选课批次不存在");
            return _batchOfferingRepository.Get(batchId);
        }

        public void SaveOfferings(int batchId, SaveBatchOfferingsRequest request, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            if (_adminRepository.GetBatchById(batchId) == null) throw new InvalidOperationException("选课批次不存在");

            List<int> classIds = (request?.Offerings ?? new List<BatchOfferingInput>())
                                .Select(x => x.ClassId).Distinct().ToList();
            string? semester=null;
            foreach(int classId in classIds){
                AdminClassDto? teachingclass=  _adminRepository.GetClassById(classId);
                if(teachingclass==null)
                    throw new InvalidOperationException("教学班不存在");
                if(semester==null)
                    semester=teachingclass.Semester;
                else if(semester!=teachingclass.Semester)
                    throw new InvalidOperationException("同一选课批次中只能开放同一学期的教学班");
            }
            _batchOfferingRepository.Save(batchId, request?.Offerings ?? new List<BatchOfferingInput>());
            _systemLogService.WriteCurrent("修改", "配置批次释放课程", batchId.ToString(), ipAddress,
                new { BatchId = batchId, Count = request?.Offerings?.Count ?? 0 });
        }

        public IList<SelectionBatchDto> GetBatches()
        {
            AdminAuthService.RequireAdminSession();
            return _adminRepository.GetBatches();
        }

        public SelectionBatchDto Create(SelectionBatchInput input, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            Validate(input);
            int status = NormalizeStatus(input);
            SelectionBatchDto batch = _adminRepository.InsertBatch(input, status);
            _systemLogService.WriteCurrent("新增", "新增选课批次", Convert.ToString(batch.BatchId), ipAddress, new { batch.BatchId, batch.BatchName, batch.StartTime, batch.EndTime, batch.Status });
            return batch;
        }

        public SelectionBatchDto Update(int batchId, SelectionBatchInput input, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            Validate(input);
            int status = NormalizeStatus(input);
            SelectionBatchDto batch = _adminRepository.UpdateBatch(batchId, input, status);
            _systemLogService.WriteCurrent("修改", "修改选课批次", Convert.ToString(batch.BatchId), ipAddress, new { batch.BatchId, batch.BatchName, batch.StartTime, batch.EndTime, batch.Status });
            return batch;
        }

        public SelectionBatchDto End(int batchId, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            SelectionBatchDto batch = _adminRepository.EndBatch(batchId);
            _systemLogService.WriteCurrent("修改", "手动结束选课批次", batchId.ToString(), ipAddress, new { BatchId = batchId });
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

        }

        private static int NormalizeStatus(SelectionBatchInput input)
        {
            DateTime now = DateTime.Now;
            if (now < input.StartTime)
            {
                return 0;
            }

            return now <= input.EndTime ? 1 : 2;
        }
    }
}
