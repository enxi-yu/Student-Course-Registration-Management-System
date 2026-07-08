using System.Collections.Generic;

namespace StudentCourse.Models
{
    public sealed class CourseDetailDto
    {
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CourseType { get; set; } = string.Empty;
        public decimal Credit { get; set; }
        public int TotalHours { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int SelectedCount { get; set; }
        public int Remaining { get { return Capacity - SelectedCount; } }
        public List<ScheduleItemDto> Schedule { get; set; } = new();
        public string Description { get; set; } = string.Empty;
    }
}
