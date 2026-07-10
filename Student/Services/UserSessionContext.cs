using StudentCourse.Student.Models;

namespace StudentCourse.Student.Services
{
    public static class UserSessionContext
    {
        public static UserSession? Current { get; private set; }

        public static bool HasSession
        {
            get { return Current is { IsLoggedIn: true }; }
        }

        public static void Set(UserSession session)
        {
            Current = session;
        }

        public static void Clear()
        {
            Current = null;
        }

        public static UserSession UseDevelopmentStudentSession()
        {
            Current = new UserSession
            {
                UserId = 9101,
                Username = "S2024001",
                RealName = "学生测试账号",
                UserType = 0,
                StudentNo = "S2024001",
                Major = "计算机科学与技术",
                Grade = "2024",
                IsLoggedIn = true
            };

            return Current;
        }

        public static UserSession UseDevelopmentTeacherSession()
        {
            Current = new UserSession
            {
                UserId = 9001,
                Username = "T001",
                RealName = "张教授",
                UserType = 1,
                IsLoggedIn = true
            };

            return Current;
        }
    }

    /// <summary>
    /// 学生端共享的会话校验工具方法，各业务 Service 均依赖此类。
    /// </summary>
    public static class StudentSessionHelper
    {
        public static UserSession RequireStudentSession()
        {
            if (!UserSessionContext.HasSession || UserSessionContext.Current is null)
            {
                throw new System.InvalidOperationException("请先登录");
            }

            UserSession session = UserSessionContext.Current;
            if (session.UserType != 0)
            {
                throw new System.InvalidOperationException("当前账号不是学生，无权访问学生端");
            }

            return session;
        }

        public static void FillSessionFromStudent(UserSession session, StudentInfo info)
        {
            if (info == null) return;

            session.UserId = info.UserId == 0 ? session.UserId : info.UserId;
            session.Username = string.IsNullOrWhiteSpace(info.Username) ? session.Username : info.Username;
            session.RealName = string.IsNullOrWhiteSpace(info.RealName) ? session.RealName : info.RealName;
            session.StudentNo = string.IsNullOrWhiteSpace(info.StudentNo) ? session.StudentNo : info.StudentNo;
            session.Major = string.IsNullOrWhiteSpace(info.Major) ? session.Major : info.Major;
            session.Grade = string.IsNullOrWhiteSpace(info.Grade) ? session.Grade : info.Grade;
        }
    }
}
