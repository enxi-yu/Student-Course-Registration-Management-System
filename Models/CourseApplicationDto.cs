namespace StudentCourse.Models
{
    public sealed class CourseApplicationDto
    {
        public string ApplyId { get; set; } = string.Empty;

        public string ApplicationId { get; set; } = string.Empty;

        public string TeacherNo { get; set; } = string.Empty;

        public string CourseName { get; set; } = string.Empty;

        public decimal Credit { get; set; }

        public int TotalHours { get; set; }

        public string Textbook { get; set; } = string.Empty;

        public string CourseSummary { get; set; } = string.Empty;

        public string CourseType { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string ApplyTime { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string ApproveTime { get; set; } = string.Empty;

        public string ApproveComment { get; set; } = string.Empty;

        public string TargetMajor { get; set; } = string.Empty;

        public string TargetGrade { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ReviewRemark { get; set; } = string.Empty;
    }
}