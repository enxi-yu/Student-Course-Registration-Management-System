namespace StudentCourse.Models
{
    public sealed class TeacherClassDto
    {
        public int ClassId { get; set; }

        public string ClassName { get; set; }

        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public string Semester { get; set; }

        public decimal Credit { get; set; }

        public int TotalHours { get; set; }

        public int Capacity { get; set; }

        public int SelectedCount { get; set; }

        public string Department { get; set; }

        public string Description { get; set; }
    }
}
