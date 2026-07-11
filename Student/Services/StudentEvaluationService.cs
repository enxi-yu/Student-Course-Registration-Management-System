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

        public void SubmitEvaluation(int classId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                throw new InvalidOperationException("评分必须在1-5之间");
            }

            StudentInfo student = GetCurrentStudent();
            _repository.SubmitEvaluation(student.StudentNo, classId, rating, comment);
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
