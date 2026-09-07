namespace StudentCourse.Models
{
    public sealed class SchedulingInput
    {
        public int CourseId { get; set; }
        public string TeacherNo { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public IList<ScheduleTimeInput> Times { get; set; } = new List<ScheduleTimeInput>();
    }

    public sealed class ScheduleTimeInput
    {
        public int Weekday { get; set; }
        public int StartPeriod { get; set; }
        public int EndPeriod { get; set; }
        public int StartWeek { get; set; }
        public int EndWeek { get; set; }
        public string Classroom { get; set; } = string.Empty;
    }

    public sealed class ScheduleRowDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string TeacherNo { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int SelectedCount { get; set; }
        public int TotalHours { get; set; }
        public string ScheduleText { get; set; } = string.Empty;
    }

    public sealed class ScheduleDetailDto
    {
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseDepartment { get; set; } = string.Empty;
        public string TeacherNo { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string TeacherDepartment { get; set; } = string.Empty;
        public string TeacherTitle { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int SelectedCount { get; set; }
        public IList<ScheduleTimeInput> Times { get; set; } = new List<ScheduleTimeInput>();
    }

    public sealed class SchedulingLookupDto<T>
    {
        public IList<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
    }

    public sealed class CourseOptionDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public sealed class TeacherOptionDto
    {
        public string TeacherNo { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
