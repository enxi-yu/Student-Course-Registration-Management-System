using System.Collections.Generic;
using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class TeacherService
    {
        private readonly TeacherRepository _teacherRepository;

        public TeacherService()
            : this(new TeacherRepository())
        {
        }

        public TeacherService(TeacherRepository teacherRepository)
        {
            _teacherRepository = teacherRepository;
        }

        public TeacherInfo GetCurrentTeacher()
        {
            UserSession session = RequireTeacherSession();
            TeacherInfo info = null;

            if (!string.IsNullOrWhiteSpace(session.TeacherNo))
            {
                info = _teacherRepository.GetTeacherInfoByTeacherNo(session.TeacherNo);
            }

            if (info == null && session.UserId > 0)
            {
                info = _teacherRepository.GetTeacherInfoByUserId(session.UserId);
            }

            if (info == null)
            {
                info = new TeacherInfo
                {
                    UserId = session.UserId,
                    Username = session.Username,
                    TeacherName = session.RealName,
                    TeacherNo = session.TeacherNo,
                    Title = session.Title,
                    Department = session.Department
                };
            }

            FillSessionFromTeacher(session, info);
            return info;
        }

        public TeacherDashboardDto GetDashboard()
        {
            TeacherInfo teacher = GetCurrentTeacher();
            TeacherDashboardDto dashboard = _teacherRepository.GetDashboard(teacher.TeacherNo);

            dashboard.TeacherName = teacher.TeacherName;
            dashboard.TeacherNo = teacher.TeacherNo;
            dashboard.Title = teacher.Title;
            dashboard.Department = teacher.Department;

            return dashboard;
        }

        public IList<TeacherClassDto> GetMyCourses(string semester)
        {
            TeacherInfo teacher = GetCurrentTeacher();
            return _teacherRepository.GetMyCourses(teacher.TeacherNo, semester);
        }

        public static UserSession RequireTeacherSession()
        {
            if (!UserSessionContext.HasSession)
            {
                throw new System.InvalidOperationException("请先登录");
            }

            UserSession session = UserSessionContext.Current;
            if (session.UserType != 1)
            {
                throw new System.InvalidOperationException("当前账号不是教师，无权访问教师端");
            }

            return session;
        }

        private static void FillSessionFromTeacher(UserSession session, TeacherInfo info)
        {
            if (info == null)
            {
                return;
            }

            session.UserId = info.UserId == 0 ? session.UserId : info.UserId;
            session.Username = string.IsNullOrWhiteSpace(info.Username) ? session.Username : info.Username;
            session.RealName = string.IsNullOrWhiteSpace(info.TeacherName) ? session.RealName : info.TeacherName;
            session.TeacherNo = string.IsNullOrWhiteSpace(info.TeacherNo) ? session.TeacherNo : info.TeacherNo;
            session.Title = string.IsNullOrWhiteSpace(info.Title) ? session.Title : info.Title;
            session.Department = string.IsNullOrWhiteSpace(info.Department) ? session.Department : info.Department;
        }
    }
}
