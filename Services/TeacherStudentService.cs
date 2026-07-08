using System.Collections.Generic;
using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class TeacherStudentService
    {
        private readonly TeachingClassRepository _teachingClassRepository;
        private readonly StudentListRepository _studentListRepository;

        public TeacherStudentService()
            : this(new TeachingClassRepository(), new StudentListRepository())
        {
        }

        public TeacherStudentService(TeachingClassRepository teachingClassRepository, StudentListRepository studentListRepository)
        {
            _teachingClassRepository = teachingClassRepository;
            _studentListRepository = studentListRepository;
        }

        public IList<StudentListDto> GetClassStudents(int classId)
        {
            string teacherNo = RequireClassAccess(classId);
            return _studentListRepository.GetClassStudents(teacherNo, classId);
        }

        public string RequireClassAccess(int classId)
        {
            if (classId <= 0)
            {
                throw new System.InvalidOperationException("教学班编号不正确");
            }

            UserSession session = TeacherService.RequireTeacherSession();
            if (!_teachingClassRepository.TeacherOwnsClass(session.TeacherNo, classId))
            {
                throw new System.InvalidOperationException("无权访问该教学班");
            }

            return session.TeacherNo;
        }
    }
}
