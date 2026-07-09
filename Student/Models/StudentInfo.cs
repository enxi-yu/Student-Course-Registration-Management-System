namespace StudentCourse.Student.Models
{
    public sealed class StudentInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string StudentNo { get; set; } = string.Empty;
        public string RealName { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal AvgGpa { get; set; }
        public decimal CreditFinished { get; set; }
    }
}
