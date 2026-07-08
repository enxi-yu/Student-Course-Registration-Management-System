namespace StudentCourse.Models
{
    public sealed class CourseSelectionDto
    {
        public int ClassId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CourseType { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public decimal Credit { get; set; }
        public string ScheduleSummary { get; set; } = string.Empty;
        public int SelectedCount { get; set; }
        public int Capacity { get; set; }
        public int Remaining { get { return Capacity - SelectedCount; } }
        public bool IsSelected { get; set; }
    }
}
