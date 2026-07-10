using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using StudentCourse.Infrastructure;
using StudentCourse.Models;

namespace StudentCourse.Repositories
{
    public sealed class CourseApplicationRepository
    {
        private const string PendingStatus = "待审核";
        private static string GenerateApplyId(OracleConnection connection)
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string prefix = "CA" + datePart;

            const string sql = @"
        SELECT NVL(MAX(TO_NUMBER(SUBSTR(apply_id, 11))), 0) + 1
          FROM course_application
         WHERE apply_id LIKE :prefix";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("prefix", OracleDbType.Varchar2).Value = prefix + "%";

                int nextNo = Convert.ToInt32(command.ExecuteScalar());
                return prefix + nextNo.ToString("D4");
            }
        }
        public CourseApplicationDto Insert(string teacherNo, CourseApplicationInput input)
        {
            
            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                string applyId = GenerateApplyId(connection);
                const string sql = @"
                    INSERT INTO course_application (
                        apply_id,
                        teacher_no,
                        course_name,
                        credit,
                        total_hours,
                        textbook,
                        course_summary,
                        course_type,
                        department,
                        apply_time,
                        status
                    ) VALUES (
                        :applyId,
                        :teacherNo,
                        :courseName,
                        :credit,
                        :totalHours,
                        :textbook,
                        :courseSummary,
                        :courseType,
                        :department,
                        SYSDATE,
                        :status
                    )";

                using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
                {
                    command.Parameters.Add("applyId", OracleDbType.Varchar2).Value = applyId;
                    command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = SafeString(teacherNo);
                    command.Parameters.Add("courseName", OracleDbType.Varchar2).Value = SafeString(input.CourseName);
                    command.Parameters.Add("credit", OracleDbType.Decimal).Value = input.Credit;
                    command.Parameters.Add("totalHours", OracleDbType.Int32).Value = input.TotalHours;
                    command.Parameters.Add("textbook", OracleDbType.Varchar2).Value = ToDbNullable(input.Textbook);
                    command.Parameters.Add("courseSummary", OracleDbType.Clob).Value = ToDbNullable(input.CourseSummary);
                    command.Parameters.Add("courseType", OracleDbType.Varchar2).Value = SafeString(input.CourseType);
                    command.Parameters.Add("department", OracleDbType.Varchar2).Value = SafeString(input.Department);
                    command.Parameters.Add("status", OracleDbType.Varchar2).Value = PendingStatus;
                    command.ExecuteNonQuery();
                }

                return GetById(connection, teacherNo, applyId);
            }
        }

        public IList<CourseApplicationDto> GetByTeacher(string teacherNo)
        {
            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                return GetByTeacher(connection, teacherNo, null);
            }
        }

        public bool HasActiveCourseName(string teacherNo, string courseName)
        {
            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                const string sql = @"
                    SELECT COUNT(1)
                      FROM course_application
                     WHERE teacher_no = :teacherNo
                       AND TRIM(course_name) = TRIM(:courseName)
                       AND NVL(TRIM(status), :pendingStatus) NOT IN (:rejectedStatus1, :rejectedStatus2, :rejectedStatus3, :rejectedStatus4)";

                using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
                {
                    command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = SafeString(teacherNo);
                    command.Parameters.Add("courseName", OracleDbType.Varchar2).Value = SafeString(courseName);
                    command.Parameters.Add("pendingStatus", OracleDbType.Varchar2).Value = PendingStatus;
                    command.Parameters.Add("rejectedStatus1", OracleDbType.Varchar2).Value = "驳回";
                    command.Parameters.Add("rejectedStatus2", OracleDbType.Varchar2).Value = "已拒绝";
                    command.Parameters.Add("rejectedStatus3", OracleDbType.Varchar2).Value = "审核不通过";
                    command.Parameters.Add("rejectedStatus4", OracleDbType.Varchar2).Value = "不通过";
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        private static CourseApplicationDto GetById(OracleConnection connection, string teacherNo, string applyId)
        {
            IList<CourseApplicationDto> applications = GetByTeacher(connection, teacherNo, applyId);
            return applications.Count == 0 ? null : applications[0];
        }

        private static IList<CourseApplicationDto> GetByTeacher(OracleConnection connection, string teacherNo, string applyId)
        {
            string sql = @"
                SELECT apply_id,
                       teacher_no,
                       course_name,
                       credit,
                       total_hours,
                       textbook,
                       course_summary,
                       course_type,
                       department,
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

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = SafeString(teacherNo);
                if (!string.IsNullOrWhiteSpace(applyId))
                {
                    command.Parameters.Add("applyId", OracleDbType.Varchar2).Value = applyId;
                }

                return ReadApplications(command);
            }
        }

        private static IList<CourseApplicationDto> ReadApplications(OracleCommand command)
        {
            List<CourseApplicationDto> applications = new List<CourseApplicationDto>();

            using (OracleDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    applications.Add(new CourseApplicationDto
                    {
                        ApplicationId = Convert.ToString(reader["apply_id"]),
                        TeacherNo = Convert.ToString(reader["teacher_no"]),
                        CourseName = Convert.ToString(reader["course_name"]),
                        Credit = ToDecimal(reader["credit"]),
                        TotalHours = ToInt32(reader["total_hours"]),
                        Textbook = Convert.ToString(reader["textbook"]),
                        CourseSummary = ReadText(reader["course_summary"]),
                        CourseType = Convert.ToString(reader["course_type"]),
                        Department = Convert.ToString(reader["department"]),
                        ApplyTime = Convert.ToString(reader["apply_time"]),
                        Status = Convert.ToString(reader["status"]),
                        ApproveTime = Convert.ToString(reader["approve_time"]),
                        ApproveComment = Convert.ToString(reader["approve_comment"])
                    });
                }
            }

            return applications;
        }

        private static object ToDbNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        private static string SafeString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string ReadText(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            OracleClob clob = value as OracleClob;
            return clob != null ? clob.Value : Convert.ToString(value);
        }

        private static int ToInt32(object value)
        {
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private static decimal ToDecimal(object value)
        {
            return value == null || value == DBNull.Value ? 0m : Convert.ToDecimal(value);
        }
    }
}