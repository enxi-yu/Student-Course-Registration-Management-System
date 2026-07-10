namespace StudentCourse.Models
{
    public sealed class TeacherScheduleDto
    {
        public int TimeId { get; set; }

        public int ClassId { get; set; }

        public string ClassName { get; set; }

        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public string Semester { get; set; }

        public int Weekday { get; set; }

        public int StartPeriod { get; set; }

        public int EndPeriod { get; set; }

        public string WeekRange { get; set; }

        public string Classroom { get; set; }

        public decimal Credit { get; set; }

        public int TotalHours { get; set; }
    }
}
