namespace StudentCourse.Models
{
    public sealed class UserSession
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string RealName { get; set; } = string.Empty;

        public int UserType { get; set; }

        public string StudentNo { get; set; } = string.Empty;

        public string Major { get; set; } = string.Empty;

        public string Grade { get; set; } = string.Empty;

        public bool IsLoggedIn { get; set; }
    }
}
