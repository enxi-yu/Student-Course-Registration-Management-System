using System.Collections.Generic;

namespace StudentCourse.Student.Models
{
    public sealed class SelectionResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> ConflictCourses { get; set; } = new();
    }
}
