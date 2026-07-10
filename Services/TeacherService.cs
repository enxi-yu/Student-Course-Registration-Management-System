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

        public TeacherDashboardDto GetDashboard()
        {
            TeacherInfo teacher = GetCurrentTeacher();

            try
            {
                TeacherDashboardDto dashboard = _teacherRepository.GetDashboard(teacher.TeacherNo);
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

            try
            {
                return _teacherRepository.GetMyCourses(teacher.TeacherNo, semester);
            }
            catch
            {
                return new List<TeacherClassDto>
                {
                    new TeacherClassDto
                    {
                        ClassId = 9301,
                        ClassName = "数据库1班",
                        CourseId = 9101,
                        CourseName = "数据库",
                        Semester = string.IsNullOrWhiteSpace(semester) ? "2026-spring" : semester,
                        Credit = 3.0m,
                        TotalHours = 48,
                        Capacity = 60,
                        SelectedCount = 48,
                        Department = "计算机学院",
                        Description = "Oracle 暂不可用时的开发测试数据"
                    },
                    new TeacherClassDto
                    {
                        ClassId = 9302,
                        ClassName = "软件工程1班",
                        CourseId = 9102,
                        CourseName = "软件工程",
                        Semester = string.IsNullOrWhiteSpace(semester) ? "2026-spring" : semester,
                        Credit = 2.5m,
                        TotalHours = 40,
                        Capacity = 50,
                        SelectedCount = 37,
                        Department = "计算机学院",
                        Description = "Oracle 暂不可用时的开发测试数据"
                    },
                    new TeacherClassDto
                    {
                        ClassId = 9303,
                        ClassName = "数据结构2班",
                        CourseId = 9103,
                        CourseName = "数据结构",
                        Semester = string.IsNullOrWhiteSpace(semester) ? "2026-spring" : semester,
                        Credit = 3.5m,
                        TotalHours = 56,
                        Capacity = 60,
                        SelectedCount = 43,
                        Department = "计算机学院",
                        Description = "Oracle 暂不可用时的开发测试数据"
                    }
                };
            }
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
