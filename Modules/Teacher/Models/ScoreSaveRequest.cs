namespace StudentCourse.Models
{
    public sealed class ScoreSaveRequest
    {
        public int ClassId { get; set; }

        public string StudentNo { get; set; } = string.Empty;

        public decimal TotalScore { get; set; }

        public string UpdateRemark { get; set; } = string.Empty;
    }
}
