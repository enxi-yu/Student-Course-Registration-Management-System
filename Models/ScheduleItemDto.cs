namespace StudentCourse.Models
{
    public sealed class ScheduleItemDto
    {
        public int ClassId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Classroom { get; set; } = string.Empty;
        public int Weekday { get; set; }
        public int StartPeriod { get; set; }
        public int EndPeriod { get; set; }
        public string WeekRange { get; set; } = string.Empty;
    }
}
