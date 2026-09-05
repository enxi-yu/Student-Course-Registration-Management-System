namespace StudentCourse.Models
{
    public sealed class LoginRequest
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

    public sealed class AdminDashboardDto
    {
        public string Username { get; set; } = string.Empty;

        public string RealName { get; set; } = string.Empty;

        public string AdminNo { get; set; } = string.Empty;

        public int AdminLevel { get; set; }

        public string ManagedScope { get; set; } = string.Empty;

        public int CourseCount { get; set; }

        public int ClassCount { get; set; }

        public int ActiveUserCount { get; set; }

        public int PendingApplicationCount { get; set; }
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

    public sealed class PagedResultDto<T>
    {
        public IList<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
    }

    public sealed class ApprovalRequest
    {
        public string Status { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;
    }

    // 管理员代选课可选教学班列表项
    public sealed class AdminSelectionClassDto
    {
        public int ClassId { get; set; }

        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string CourseType { get; set; } = string.Empty;

        public decimal Credit { get; set; }

        public string Semester { get; set; } = string.Empty;

        public string TeacherNo { get; set; } = string.Empty;

        public string TeacherName { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public int SelectedCount { get; set; }

        public string ScheduleSummary { get; set; } = string.Empty;

        public bool IsSelected { get; set; }

        public int Remaining => Math.Max(0, Capacity - SelectedCount);
    }

    public sealed class AdminSelectionBatchDto
    {
        public int BatchId { get; set; }
        public string BatchName { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public int CourseCount { get; set; }
    }

    public sealed class AdminStudentScheduleDto
    {
        public int ClassId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int Weekday { get; set; }
        public int StartPeriod { get; set; }
        public int EndPeriod { get; set; }
        public string WeekRange { get; set; } = string.Empty;
        public string Classroom { get; set; } = string.Empty;
    }

    // 学生已选课程记录，用于代退课展示
    public sealed class AdminEnrollmentDto
    {
        public int SelectId { get; set; }

        public int ClassId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string CourseType { get; set; } = string.Empty;

        public decimal Credit { get; set; }

        public string Semester { get; set; } = string.Empty;

        public string TeacherName { get; set; } = string.Empty;

        public int? BatchId { get; set; }

        public string BatchName { get; set; } = string.Empty;

        public string ScheduleSummary { get; set; } = string.Empty;
    }

    // 代选/代退课结果
    public sealed class AdminSelectionResultDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public IList<string> ConflictCourses { get; set; } = new List<string>();

        // 前端二次确认：true=容量已满，用户确认后可 force=true 继续；
        // Success=false 且 RequireCapacityConfirm=true 时前端应弹"容量已满，是否超员扩选"
        public bool RequireCapacityConfirm { get; set; }

        // 仅当 RequireCapacityConfirm=true 时有意义：当前已选人数
        public int CurrentSelected { get; set; }

        // 仅当 RequireCapacityConfirm=true 时有意义：当前原容量上限
        public int CurrentCapacity { get; set; }
    }
}
