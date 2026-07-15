namespace StudentCourse.Student.Models
{
    public sealed class CourseEvaluationDto
    {
        public int ClassId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public decimal Credit { get; set; }
        public int? Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string EvaluationDate { get; set; } = string.Empty;
        public bool HasEvaluated { get; set; }
        public bool IsGraded { get; set; }
    }

    public sealed class SubmitEvaluationRequest
    {
        public int ClassId { get; set; }
        public int D1Score { get; set; } // 教学质量
        public int D2Score { get; set; } // 作业量
        public int D3Score { get; set; } // 课堂氛围
        public int D4Score { get; set; } // 教师态度
        public string Comment { get; set; } = string.Empty;
    }
}