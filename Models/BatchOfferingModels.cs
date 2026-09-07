namespace StudentCourse.Models;

public sealed class BatchOfferingDto
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public bool Selected { get; set; }
    public IList<string> Majors { get; set; } = new List<string>();
    public IList<string> Grades { get; set; } = new List<string>();
}

public sealed class BatchOfferingInput
{
    public int ClassId { get; set; }
    public IList<string> Majors { get; set; } = new List<string>();
    public IList<string> Grades { get; set; } = new List<string>();
}

public sealed class SaveBatchOfferingsRequest
{
    public IList<BatchOfferingInput> Offerings { get; set; } = new List<BatchOfferingInput>();
}
