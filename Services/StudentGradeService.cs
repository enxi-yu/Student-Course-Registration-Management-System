using System.Collections.Generic;
using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class StudentGradeService
    {
        private readonly StudentGradeRepository _gradeRepository;
        private readonly StudentProfileRepository _profileRepository;

        public StudentGradeService()
            : this(new StudentGradeRepository(), new StudentProfileRepository())
        {
        }

        public StudentGradeService(StudentGradeRepository gradeRepository, StudentProfileRepository profileRepository)
        {
            _gradeRepository = gradeRepository;
            _profileRepository = profileRepository;
        }

        public List<EnrolledCourseDto> GetEnrolledCourses()
        {
            StudentInfo student = GetCurrentStudent();
            return _gradeRepository.GetEnrolledCourses(student.StudentNo);
        }

        public GpaSummaryDto GetGpaSummary()
        {
            StudentInfo student = GetCurrentStudent();
            return _profileRepository.GetGpaSummary(student.StudentNo);
        }

        private StudentInfo GetCurrentStudent()
        {
            var profileService = new StudentProfileService();
            return profileService.GetCurrentStudent();
        }
    }
}
