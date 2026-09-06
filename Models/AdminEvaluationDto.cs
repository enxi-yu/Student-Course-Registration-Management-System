namespace StudentCourse.Models
{
    public sealed class AdminEvaluationDto
    {
        public string StudentNo { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public int D1Score { get; set; }
        public int D2Score { get; set; }
        public int D3Score { get; set; }
        public int D4Score { get; set; }
        public decimal EvalScore { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string EvaluationTime { get; set; } = string.Empty;
    }
}
