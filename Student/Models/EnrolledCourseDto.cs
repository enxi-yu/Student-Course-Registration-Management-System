namespace StudentCourse.Student.Models
{
    public sealed class EnrolledCourseDto
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public decimal Credit { get; set; }
        public decimal CreditObtained { get; set; }
        public decimal? TotalScore { get; set; }
        public string GradeLevel { get; set; } = string.Empty;
        public decimal? Gpa { get; set; }
        public bool IsPassed { get; set; }
    }
}
