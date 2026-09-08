using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using StudentCourse.Infrastructure;
using StudentCourse.Models;

namespace StudentCourse.Repositories
{
    public sealed class CourseApplicationRepository
    {
        public CourseApplicationDto Insert(string teacherNo, CourseApplicationInput input)
        {
            string applyId = Guid.NewGuid().ToString("N");

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            const string sql = @"
                INSERT INTO course_application (
                    apply_id,
                    teacher_no,
                    course_name,
                    course_type,
                    credit,
                    textbook,
                    course_summary,
                    apply_time,
                    status
                ) VALUES (
                    :applyId,
                    :teacherNo,
                    :courseName,
                    :courseType,
                    :credit,
                    :textbook,
                    :courseSummary,
                    SYSDATE,
                    '待审核'
                )";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("applyId", OracleDbType.Varchar2).Value = applyId;
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                command.Parameters.Add("courseName", OracleDbType.Varchar2).Value = input.CourseName!.Trim();
                command.Parameters.Add("courseType", OracleDbType.Varchar2).Value = input.CourseType!.Trim();
                command.Parameters.Add("credit", OracleDbType.Decimal).Value = input.Credit;
                command.Parameters.Add("textbook", OracleDbType.Varchar2).Value = DbValue(input.Textbook);
                command.Parameters.Add("courseSummary", OracleDbType.Clob).Value = DbValue(input.CourseSummary);
                command.ExecuteNonQuery();
            }

            return GetById(connection, teacherNo, applyId);
        }

        public IList<CourseApplicationDto> GetByTeacher(string teacherNo)
        {
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            return GetByTeacherCore(connection, teacherNo);
        }

        public bool HasActiveCourseName(string teacherNo, string courseName)
        {
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            const string sql = @"
                SELECT COUNT(*)
                  FROM course_application
                 WHERE teacher_no = :teacherNo
                   AND course_name = :courseName
                   AND NVL(status, '待审核') <> '驳回'";

            using OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql);
            command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
            command.Parameters.Add("courseName", OracleDbType.Varchar2).Value = courseName.Trim();
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        private static CourseApplicationDto GetById(OracleConnection connection, string teacherNo, string applyId)
        {
            IList<CourseApplicationDto> applications = GetByTeacherCore(connection, teacherNo, applyId);
            if (applications.Count == 0)
            {
                throw new InvalidOperationException("开课申请保存后读取失败。");
            }

            return applications[0];
        }

        private static IList<CourseApplicationDto> GetByTeacherCore(OracleConnection connection, string teacherNo, string? applyId = null)
        {
            string sql = @"
                SELECT apply_id,
                       teacher_no,
                       course_name,
                       course_type,
                       credit,
                       textbook,
                       course_summary,
                       TO_CHAR(apply_time, 'YYYY-MM-DD HH24:MI:SS') AS apply_time,
                       status,
                       TO_CHAR(approve_time, 'YYYY-MM-DD HH24:MI:SS') AS approve_time,
                       approve_comment
                  FROM course_application
                 WHERE teacher_no = :teacherNo";

            if (!string.IsNullOrWhiteSpace(applyId))
            {
                sql += " AND apply_id = :applyId";
            }

            sql += " ORDER BY apply_time DESC";

            using OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql);
            command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
            if (!string.IsNullOrWhiteSpace(applyId))
            {
                command.Parameters.Add("applyId", OracleDbType.Varchar2).Value = applyId;
            }

            List<CourseApplicationDto> applications = new List<CourseApplicationDto>();
            using OracleDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                applications.Add(new CourseApplicationDto
                {
                    ApplicationId = Convert.ToString(reader["apply_id"]) ?? string.Empty,
                    TeacherNo = Convert.ToString(reader["teacher_no"]) ?? string.Empty,
                    CourseName = Convert.ToString(reader["course_name"]) ?? string.Empty,
                    CourseType = Convert.ToString(reader["course_type"]) ?? string.Empty,
                    Credit = ToDecimal(reader["credit"]),
                    Textbook = Convert.ToString(reader["textbook"]) ?? string.Empty,
                    CourseSummary = ReadText(reader["course_summary"]),
                    ApplyTime = Convert.ToString(reader["apply_time"]) ?? string.Empty,
                    Status = Convert.ToString(reader["status"]) ?? string.Empty,
                    ApproveTime = Convert.ToString(reader["approve_time"]) ?? string.Empty,
                    ApproveComment = Convert.ToString(reader["approve_comment"]) ?? string.Empty
                });
            }

            return applications;
        }

        private static object DbValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
        }

        private static decimal ToDecimal(object value)
        {
            return value == null || value == DBNull.Value ? 0m : Convert.ToDecimal(value);
        }

        private static string ReadText(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            OracleClob? clob = value as OracleClob;
            return clob == null ? Convert.ToString(value) ?? string.Empty : clob.Value;
        }
    }
}
