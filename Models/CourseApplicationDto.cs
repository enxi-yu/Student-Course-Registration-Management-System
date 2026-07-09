namespace StudentCourse.Models
{
    public sealed class CourseApplicationDto
    {
        public string ApplicationId { get; set; } = string.Empty;

        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string CourseType { get; set; } = string.Empty;

        public decimal Credit { get; set; }

        public int TotalHours { get; set; }

        public string Department { get; set; } = string.Empty;

        public string CourseDesc { get; set; } = string.Empty;

        public string TargetMajor { get; set; } = string.Empty;

        public string TargetGrade { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string ApplyTime { get; set; } = string.Empty;

        public string ReviewRemark { get; set; } = string.Empty;
    }
}