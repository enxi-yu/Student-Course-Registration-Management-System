namespace StudentCourse.Models
{
    public sealed class CourseApplicationInput
    {
        public string? CourseName { get; set; }
        public string? CourseType { get; set; }
        public decimal Credit { get; set; }
        public string? Textbook { get; set; }
        public string? CourseSummary { get; set; }
    }
}
