namespace StudentCourse.Models
{
    public sealed class ScoreDto
    {
        public string StudentNo { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public int ClassId { get; set; }

        public decimal? TotalScore { get; set; }

        public string GradeLevel { get; set; } = string.Empty;

        public decimal? Gpa { get; set; }

        public decimal CreditObtained { get; set; }

        public string UpdateRemark { get; set; } = string.Empty;

        public string UpdateTime { get; set; } = string.Empty;
    }
}
