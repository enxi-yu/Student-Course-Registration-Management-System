namespace StudentCourse.Student.Models;

public sealed class StudentSelectionBatchDto
{
    public int BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public int CourseCount { get; set; }
}
