namespace StudentCourse.Models
{
    public sealed class TeacherDashboardDto
    {
        public string TeacherName { get; set; }

        public string TeacherNo { get; set; }

        public string Title { get; set; }

        public string Department { get; set; }

        public int ClassCount { get; set; }

        public int CourseCount { get; set; }

        public int StudentCount { get; set; }

        public int PendingScoreCount { get; set; }
    }
}
