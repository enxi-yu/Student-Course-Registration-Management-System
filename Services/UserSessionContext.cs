using StudentCourse.Models;

namespace StudentCourse.Services
{
    public static class UserSessionContext
    {
        public static UserSession Current { get; private set; }

        public static bool HasSession
        {
            get { return Current != null && Current.IsLoggedIn; }
        }

        public static void Set(UserSession session)
        {
            Current = session;
        }

        public static void Clear()
        {
            Current = null;
        }

        public static UserSession UseDevelopmentTeacherSession()
        {
            Current = new UserSession
            {
                UserId = 9001,
                Username = "teacher_demo",
                RealName = "张老师",
                UserType = 1,
                TeacherNo = "T001",
                Title = "讲师",
                Department = "计算机科学与技术学院",
                IsLoggedIn = true
            };

            return Current;
        }

        public static UserSession UseDevelopmentStudentSession()
        {
            Current = new UserSession
            {
                UserId = 9101,
                Username = "student_demo",
                RealName = "学生测试账号",
                UserType = 0,
                IsLoggedIn = true
            };

            return Current;
        }
    }
}
