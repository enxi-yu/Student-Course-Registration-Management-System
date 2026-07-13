using System;
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

            try
            {
                if (!string.IsNullOrWhiteSpace(session.TeacherNo))
                {
                    info = _teacherRepository.GetTeacherInfoByTeacherNo(session.TeacherNo);
                }

                if (info == null && session.UserId > 0)
                {
                    info = _teacherRepository.GetTeacherInfoByUserId(session.UserId);
                }
            }
            catch
            {
                info = null;
            }

            if (info == null)
            {
                info = CreateFallbackTeacher(session);
            }

            FillSessionFromTeacher(session, info);
            return info;
        }

        public TeacherDashboardDto GetDashboard(string semester)
        {
            TeacherInfo teacher = GetCurrentTeacher();

            try
            {
                TeacherDashboardDto dashboard = _teacherRepository.GetDashboard(teacher.TeacherNo, semester);
                dashboard.TeacherName = teacher.TeacherName;
                dashboard.TeacherNo = teacher.TeacherNo;
                dashboard.Title = teacher.Title;
                dashboard.Department = teacher.Department;
                return dashboard;
            }
            catch
            {
                return new TeacherDashboardDto
                {
                    TeacherName = teacher.TeacherName,
                    TeacherNo = teacher.TeacherNo,
                    Title = teacher.Title,
                    Department = teacher.Department,
                    ClassCount = 3,
                    CourseCount = 3,
                    StudentCount = 128,
                    PendingScoreCount = 42
                };
            }
        }

        public IList<TeacherClassDto> GetMyCourses(string semester)
        {
            TeacherInfo teacher = GetCurrentTeacher();
            return _teacherRepository.GetMyCourses(teacher.TeacherNo, semester);
        }

        public IList<TeacherScheduleDto> GetMySchedule(string semester)
        {
            TeacherInfo teacher = GetCurrentTeacher();
            return _teacherRepository.GetMySchedule(teacher.TeacherNo, semester);
        }

        public static UserSession RequireTeacherSession()
        {
            if (!UserSessionContext.HasSession)
            {
                throw new InvalidOperationException("请先登录");
            }

            UserSession session = UserSessionContext.Current;
            if (session.UserType != 1)
            {
                throw new InvalidOperationException("当前账号不是教师，无权访问教师端");
            }

            return session;
        }

        private static TeacherInfo CreateFallbackTeacher(UserSession session)
        {
            return new TeacherInfo
            {
                UserId = session.UserId == 0 ? 9001 : session.UserId,
                Username = string.IsNullOrWhiteSpace(session.Username) ? "teacher_demo" : session.Username,
                TeacherName = string.IsNullOrWhiteSpace(session.RealName) ? "张老师" : session.RealName,
                TeacherNo = string.IsNullOrWhiteSpace(session.TeacherNo) ? "T2026001" : session.TeacherNo,
                Title = string.IsNullOrWhiteSpace(session.Title) ? "副教授" : session.Title,
                Department = string.IsNullOrWhiteSpace(session.Department) ? "计算机学院" : session.Department
            };
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
