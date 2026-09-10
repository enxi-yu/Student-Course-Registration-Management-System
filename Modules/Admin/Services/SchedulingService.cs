using System.Text.RegularExpressions;
using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class SchedulingService
    {
        private readonly SchedulingRepository _repository;
        private readonly SystemLogService _logs;

        public SchedulingService(SchedulingRepository repository, SystemLogService logs)
        {
            _repository = repository;
            _logs = logs;
        }

        public SchedulingLookupDto<CourseOptionDto> SearchCourses(string? keyword, int page, int pageSize)
        {
            AdminAuthService.RequireAdminSession();
            return _repository.SearchCourses(keyword, Math.Max(1, page), Math.Clamp(pageSize, 1, 50));
        }

        public SchedulingLookupDto<TeacherOptionDto> SearchTeachers(string? keyword, int page, int pageSize)
        {
            AdminAuthService.RequireAdminSession();
            return _repository.SearchTeachers(keyword, Math.Max(1, page), Math.Clamp(pageSize, 1, 50));
        }
        public IList<ScheduleRowDto> GetSchedules(string? semester) { 
            AdminAuthService.RequireAdminSession();
            return _repository.GetSchedules(semester); 
        }
        
        public ScheduleDetailDto GetSchedule(int classId) { 
            AdminAuthService.RequireAdminSession();
            return _repository.GetSchedule(classId); 
        }

        public ScheduleRowDto Create(SchedulingInput input, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            Validate(input);
            ScheduleRowDto result = _repository.Create(input);
            _logs.WriteCurrent("新增", "新增课程排课", Convert.ToString(result.ClassId), ipAddress, input);
            return result;
        }

        public ScheduleRowDto Update(int classId, SchedulingInput input, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            Validate(input);
            ScheduleRowDto result = _repository.Update(classId, input);
            _logs.WriteCurrent("修改", "修改课程排课", Convert.ToString(classId), ipAddress, input);
            return result;
        }

        public void Delete(int classId, string ipAddress)
        {
            AdminAuthService.RequireAdminSession();
            _repository.Delete(classId);
            _logs.WriteCurrent("删除", "删除课程排课", Convert.ToString(classId), ipAddress, null);
        }

        private static void Validate(SchedulingInput input)
        {
            if (input == null) throw new InvalidOperationException("排课信息不能为空");
            if (input.CourseId <= 0) throw new InvalidOperationException("请选择课程");
            if (string.IsNullOrWhiteSpace(input.TeacherNo)) throw new InvalidOperationException("请选择任课教师");
            if (!Regex.IsMatch(input.Semester ?? "", @"^\d{4}-\d{4}-[12]$")) throw new InvalidOperationException("请选择正确的学年学期");
            if (string.IsNullOrWhiteSpace(input.ClassName)) throw new InvalidOperationException("教学班名称不能为空");
            if (input.Capacity <= 0) throw new InvalidOperationException("课程容量必须大于0");
            if (input.Times == null || input.Times.Count == 0) throw new InvalidOperationException("至少添加一条上课时间");
            foreach (ScheduleTimeInput time in input.Times)
            {
                if (time.Weekday < 1 || time.Weekday > 7) throw new InvalidOperationException("星期选择不正确");
                if (time.StartPeriod < 1 || time.EndPeriod > 10 || time.StartPeriod > time.EndPeriod) throw new InvalidOperationException("上课节次必须在1至10节之间");
                if (time.StartWeek < 1 || time.EndWeek > 30 || time.StartWeek > time.EndWeek) throw new InvalidOperationException("上课周次不正确");
                if (string.IsNullOrWhiteSpace(time.Classroom)) throw new InvalidOperationException("教室不能为空");
            }
            for (int i = 0; i < input.Times.Count; i++)
            {
                for (int j = i + 1; j < input.Times.Count; j++)
                {
                    ScheduleTimeInput left = input.Times[i];
                    ScheduleTimeInput right = input.Times[j];
                    bool overlaps = left.Weekday == right.Weekday
                        && left.StartPeriod <= right.EndPeriod && left.EndPeriod >= right.StartPeriod
                        && left.StartWeek <= right.EndWeek && left.EndWeek >= right.StartWeek;
                    if (overlaps) throw new InvalidOperationException("同一教学班的上课时间不能相互重叠");
                }
            }
        }
    }
}
