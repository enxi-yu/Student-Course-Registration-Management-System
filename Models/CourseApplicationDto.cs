namespace StudentCourse.Models
{
    public sealed class CourseApplicationDto
    {
        public string ApplicationId { get; set; }

        public string TeacherNo { get; set; }

        public string CourseName { get; set; }

        public decimal Credit { get; set; }

        public int TotalHours { get; set; }

        public string Textbook { get; set; }

        public string CourseSummary { get; set; }

        public string CourseType { get; set; }

        public string Department { get; set; }

        public string ApplyTime { get; set; }

        public string Status { get; set; }

        public string ApproveTime { get; set; }

        public string ApproveComment { get; set; }
    }
}