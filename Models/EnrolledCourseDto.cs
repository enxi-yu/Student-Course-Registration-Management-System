namespace StudentCourse.Models
{
    public sealed class EnrolledCourseDto
    {
        public string CourseName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public decimal Credit { get; set; }
        public decimal? TotalScore { get; set; }
        public string GradeLevel { get; set; } = string.Empty;
        public decimal? Gpa { get; set; }
    }
}
