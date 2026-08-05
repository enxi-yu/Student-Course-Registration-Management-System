namespace StudentCourse.Models
{
    public sealed class CourseApplicationInput
    {
        public string? CourseName { get; set; }
        public decimal Credit { get; set; }
        public int TotalHours { get; set; }
        public string? Textbook { get; set; }
        public string? CourseSummary { get; set; }
        public string? CourseType { get; set; }
        public string? Department { get; set; }

        // 兼容旧字段，必须带 ?
        public string? TargetMajor { get; set; }
        public string? TargetGrade { get; set; }
        public string? Description { get; set; }
    }
}