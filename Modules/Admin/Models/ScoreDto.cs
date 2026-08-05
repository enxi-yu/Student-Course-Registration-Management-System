namespace StudentCourse.Models
{
    public sealed class ScoreDto
    {
        public string StudentNo { get; set; }

        public string StudentName { get; set; }

        public int ClassId { get; set; }

        public decimal? TotalScore { get; set; }

        public string GradeLevel { get; set; }

        public decimal? Gpa { get; set; }

        public decimal CreditObtained { get; set; }

        public string UpdateRemark { get; set; }

        public string UpdateTime { get; set; }
    }
}
