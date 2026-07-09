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

        public List<CourseSelectionDto> GetAvailableCourses(string semester)
        {
            StudentInfo student = GetCurrentStudent();
            return _repository.GetAvailableCourses(student.StudentNo, semester);
        }

        public CourseDetailDto? GetCourseDetail(int classId)
        {
            return _repository.GetCourseDetail(classId);
        }

        public SelectionResultDto SelectCourse(int classId)
        {
            StudentInfo student = GetCurrentStudent();
            return _repository.SelectCourse(student.StudentNo, classId, 1);
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
