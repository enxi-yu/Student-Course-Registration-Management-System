using System.Collections.Generic;
using StudentCourse.Student.Models;
using StudentCourse.Student.Repositories;

namespace StudentCourse.Student.Services
{
    public sealed class CourseSelectionService
    {
        private readonly CourseSelectionRepository _repository;

        public CourseSelectionService()
            : this(new CourseSelectionRepository())
        {
        }

        public CourseSelectionService(CourseSelectionRepository repository)
        {
            _repository = repository;
        }

        public List<StudentSelectionBatchDto> GetSelectionBatches()
        {
            StudentInfo student=GetCurrentStudent(); return _repository.GetSelectionBatches(student.StudentNo);
        }

        public List<CourseSelectionDto> GetAvailableCourses(int batchId)
        {
            StudentInfo student = GetCurrentStudent();
            return _repository.GetAvailableCourses(student.StudentNo, batchId);
        }

        private static string ResolveSemester(string semester)
        {
            if (!string.IsNullOrWhiteSpace(semester)) return semester.Trim();
            DateTime now = DateTime.Now;
            int startYear = now.Month >= 8 ? now.Year : now.Year - 1;
            int term = now.Month >= 8 || now.Month == 1 ? 1 : 2;
            return $"{startYear}-{startYear + 1}-{term}";
        }

        public CourseDetailDto? GetCourseDetail(int classId)
        {
            StudentInfo student = GetCurrentStudent();
            return _repository.GetCourseDetail(student.StudentNo, classId);
        }

        public SelectionResultDto SelectCourse(int classId, int batchId)
        {
            StudentInfo student = GetCurrentStudent();
            return _repository.SelectCourse(student.StudentNo, classId, batchId);
        }

        public SelectionResultDto DropCourse(int classId)
        {
            StudentInfo student = GetCurrentStudent();
            return _repository.DropCourse(student.StudentNo, classId);
        }

        public List<ScheduleItemDto> GetWeeklySchedule(string semester)
        {
            StudentInfo student = GetCurrentStudent();
            return _repository.GetWeeklySchedule(student.StudentNo, semester);
        }

        private StudentInfo GetCurrentStudent()
        {
            var profileService = new StudentProfileService();
            return profileService.GetCurrentStudent();
        }
    }
}
