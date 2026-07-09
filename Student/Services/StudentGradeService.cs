using System.Collections.Generic;
using StudentCourse.Student.Models;
using StudentCourse.Student.Repositories;

namespace StudentCourse.Student.Services
{
    public sealed class StudentGradeService
    {
        private readonly StudentGradeRepository _gradeRepository;

        public StudentGradeService()
            : this(new StudentGradeRepository())
        {
        }

        public StudentGradeService(StudentGradeRepository gradeRepository)
        {
            _gradeRepository = gradeRepository;
        }

        public List<EnrolledCourseDto> GetEnrolledCourses()
        {
            StudentInfo student = GetCurrentStudent();
            return _gradeRepository.GetEnrolledCourses(student.StudentNo);
        }

        public GpaSummaryDto GetGpaSummary()
        {
            StudentInfo student = GetCurrentStudent();
            return _gradeRepository.GetGpaSummary(student.StudentNo);
        }

        private StudentInfo GetCurrentStudent()
        {
            var profileService = new StudentProfileService();
            return profileService.GetCurrentStudent();
        }
    }
}
