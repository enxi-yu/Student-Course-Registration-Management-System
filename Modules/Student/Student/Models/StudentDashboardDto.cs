using System.Collections.Generic;

namespace StudentCourse.Student.Models
{
    public sealed class StudentDashboardDto
    {
        public StudentInfo Profile { get; set; } = new();
        public List<ScheduleItemDto> TodayCourses { get; set; } = new();
        public int CurrentSemesterCourseCount { get; set; }
        public decimal CurrentSemesterCredit { get; set; }
        public GpaSummaryDto GpaSummary { get; set; } = new();
    }
}
