using System.Collections.Generic;

namespace StudentCourse.Student.Models
{
    public sealed class GpaSummaryDto
    {
        public decimal TotalCreditsFinished { get; set; }
        public decimal AvgGpa { get; set; }
        public int TotalCourses { get; set; }
        public List<CreditTrendItem> CreditTrend { get; set; } = new();
        public List<SemesterGpaItem> SemesterGpas { get; set; } = new();
    }

    public sealed class CreditTrendItem
    {
        public string Semester { get; set; } = string.Empty;
        public decimal Credits { get; set; }
    }

    public sealed class SemesterGpaItem
    {
        public string Semester { get; set; } = string.Empty;
        public decimal AvgGpa { get; set; }
        public decimal Credits { get; set; }
        public int Courses { get; set; }
    }
}
