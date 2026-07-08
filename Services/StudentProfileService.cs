using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class StudentProfileService
    {
        private readonly StudentProfileRepository _repository;

        public StudentProfileService()
            : this(new StudentProfileRepository())
        {
        }

        public StudentProfileService(StudentProfileRepository repository)
        {
            _repository = repository;
        }

        public StudentInfo GetCurrentStudent()
        {
            UserSession session = StudentSessionHelper.RequireStudentSession();
            StudentInfo? info = null;

            if (!string.IsNullOrWhiteSpace(session.StudentNo))
            {
                info = _repository.GetStudentInfoByStudentNo(session.StudentNo);
            }

            if (info == null && session.UserId > 0)
            {
                info = _repository.GetStudentInfoByUserId(session.UserId);
            }

            if (info == null)
            {
                info = new StudentInfo
                {
                    UserId = session.UserId,
                    Username = session.Username,
                    RealName = session.RealName,
                    StudentNo = session.StudentNo,
                    Major = session.Major,
                    Grade = session.Grade
                };
            }

            StudentSessionHelper.FillSessionFromStudent(session, info);
            return info;
        }

        public StudentDashboardDto GetDashboard()
        {
            StudentInfo student = GetCurrentStudent();
            return _repository.GetDashboard(student.StudentNo);
        }
    }
}
