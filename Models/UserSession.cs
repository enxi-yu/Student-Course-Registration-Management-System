namespace StudentCourse.Models
{
    public sealed class UserSession
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public string RealName { get; set; }

        public int UserType { get; set; }

        public string TeacherNo { get; set; }

        public string Title { get; set; }

        public string Department { get; set; }

        public bool IsLoggedIn { get; set; }
    }
}
