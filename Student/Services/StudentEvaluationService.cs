using System.Collections.Generic;
using StudentCourse.Student.Models;
using StudentCourse.Student.Repositories;

namespace StudentCourse.Student.Services
{
    public sealed class StudentEvaluationService
    {
        private readonly StudentEvaluationRepository _repository;

        public StudentEvaluationService()
            : this(new StudentEvaluationRepository())
        {
        }

        public StudentEvaluationService(StudentEvaluationRepository repository)
        {
            _repository = repository;
        }

        public List<CourseEvaluationDto> GetEvaluableCourses()
        {
            StudentInfo student = GetCurrentStudent();
            return _repository.GetEvaluableCourses(student.StudentNo);
        }

        public void SubmitEvaluation(int classId, int d1, int d2, int d3, int d4, string comment)
        {
            if (d1 < 1 || d1 > 5 || d2 < 1 || d2 > 5 || d3 < 1 || d3 > 5 || d4 < 1 || d4 > 5)
            {
                throw new InvalidOperationException("所有各项评分均必须在1-5之间");
            }

            StudentInfo student = GetCurrentStudent();
            _repository.SubmitEvaluation(student.StudentNo, classId, d1, d2, d3, d4, comment);
        }

        public List<CourseEvaluationDto> GetEvaluationHistory()
        {
            StudentInfo student = GetCurrentStudent();
            return _repository.GetEvaluationHistory(student.StudentNo);
        }

        private StudentInfo GetCurrentStudent()
        {
            var profileService = new StudentProfileService();
            return profileService.GetCurrentStudent();
        }
    }
}
