namespace StudentCourse.Models
{
    public sealed class AdminLoginRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    public sealed class AdminCurrentDto
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string RealName { get; set; } = string.Empty;

        public string AdminNo { get; set; } = string.Empty;

        public int AdminLevel { get; set; }

        public string ManagedScope { get; set; } = string.Empty;
    }

    public sealed class AdminCredentialDto
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string RealName { get; set; } = string.Empty;

        public string AdminNo { get; set; } = string.Empty;

        public int AdminLevel { get; set; }

        public string ManagedScope { get; set; } = string.Empty;

        public int Status { get; set; }
    }

    public sealed class AdminPermissionDto
    {
        public int PermissionId { get; set; }

        public string PermissionCode { get; set; } = string.Empty;

        public string PermissionName { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;
    }

    public sealed class AdminUserInput
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string RealName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int Status { get; set; } = 1;

        public string StudentNo { get; set; } = string.Empty;

        public string Major { get; set; } = string.Empty;

        public string Grade { get; set; } = string.Empty;

        public decimal AvgGpa { get; set; }

        public decimal CreditFinished { get; set; }

        public string TeacherNo { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;
    }

    public sealed class AdminStudentDto
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string RealName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int Status { get; set; }

        public string LastLogin { get; set; } = string.Empty;

        public string CreateTime { get; set; } = string.Empty;

        public string StudentNo { get; set; } = string.Empty;

        public string Major { get; set; } = string.Empty;

        public string Grade { get; set; } = string.Empty;

        public decimal AvgGpa { get; set; }

        public decimal CreditFinished { get; set; }
    }

    public sealed class AdminTeacherDto
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string RealName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int Status { get; set; }

        public string LastLogin { get; set; } = string.Empty;

        public string CreateTime { get; set; } = string.Empty;

        public string TeacherNo { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;
    }

    public sealed class ResetPasswordRequest
    {
        public string Password { get; set; } = string.Empty;
    }

    public sealed class SelectionBatchInput
    {
        public string BatchName { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int? Status { get; set; }
    }

    public sealed class SelectionBatchDto
    {
        public int BatchId { get; set; }

        public string BatchName { get; set; } = string.Empty;

        public string StartTime { get; set; } = string.Empty;

        public string EndTime { get; set; } = string.Empty;

        public int Status { get; set; }

        public string StatusText { get; set; } = string.Empty;
    }

    public sealed class AdminClassDto
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
    }

    public sealed class CapacityUpdateRequest
    {
        public int Capacity { get; set; }

        public string Remark { get; set; } = string.Empty;
    }

    public sealed class SystemLogDto
    {
        public string LogId { get; set; } = string.Empty;

        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string OperationType { get; set; } = string.Empty;

        public string OperationDesc { get; set; } = string.Empty;

        public string TargetId { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;

        public string RequestParams { get; set; } = string.Empty;

        public string ResultStatus { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public string LogTime { get; set; } = string.Empty;
    }

    public sealed class ApprovalRequest
    {
        public string Status { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;
    }
}
