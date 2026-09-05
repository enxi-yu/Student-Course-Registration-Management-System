namespace StudentCourse.Models
{
    public sealed class TeacherInfo
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public string TeacherName { get; set; }

        public string TeacherNo { get; set; }

        public string Title { get; set; }

        public string Department { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
