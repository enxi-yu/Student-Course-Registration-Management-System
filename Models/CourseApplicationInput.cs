namespace StudentCourse.Models
{
    public sealed class CourseApplicationInput
    {
        public string CourseName { get; set; }

        public string CourseType { get; set; }

        public decimal Credit { get; set; }

        public int TotalHours { get; set; }

        public string TargetMajor { get; set; }

        public string TargetGrade { get; set; }

        public string Description { get; set; }
    }
}
