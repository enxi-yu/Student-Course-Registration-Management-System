namespace StudentCourse.Models
{
    public class CourseDto
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string CourseType { get; set; } = string.Empty;

        public decimal Credit { get; set; }

        public int TotalHours { get; set; }

        public string Department { get; set; } = string.Empty;

        public string CourseDesc { get; set; } = string.Empty;
    }
}