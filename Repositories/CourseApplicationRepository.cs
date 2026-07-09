using System;
using System.Collections.Generic;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using StudentCourse.Infrastructure;
using StudentCourse.Models;

namespace StudentCourse.Repositories
{
    public sealed class CourseApplicationRepository
    {
        private const string TableName = "COURSE_APPLICATION";
        private const string PendingStatus = "待审核";

        public CourseApplicationDto Insert(string teacherNo, CourseApplicationInput input)
        {
            string applyId = Guid.NewGuid().ToString("N");

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                HashSet<string> columns = GetColumns(connection, TableName);
                InsertApplication(connection, columns, applyId, teacherNo, input);
                return GetById(connection, columns, teacherNo, applyId);
            }
        }

        public IList<CourseApplicationDto> GetByTeacher(string teacherNo)
        {
            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                HashSet<string> columns = GetColumns(connection, TableName);
                return GetByTeacher(connection, columns, teacherNo, null);
            }
        }

        public bool HasActiveCourseName(string teacherNo, string courseName)
        {
            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                HashSet<string> columns = GetColumns(connection, TableName);
                return HasActiveCourseName(connection, columns, teacherNo, courseName);
            }
        }

        private static bool HasActiveCourseName(OracleConnection connection, HashSet<string> columns, string teacherNo, string courseName)
        {
            if (!columns.Contains("TEACHER_NO") || !columns.Contains("COURSE_NAME"))
            {
                throw new InvalidOperationException("course_application 表缺少 teacher_no 或 course_name 字段，无法校验重复开课申请。");
            }

            string statusColumn = FirstExisting(columns, "STATUS", "APPROVE_STATUS", "REVIEW_STATUS");
            string sql = @"
                SELECT COUNT(1)
                  FROM course_application
                 WHERE teacher_no = :teacherNo
                   AND TRIM(course_name) = TRIM(:courseName)";

            if (!string.IsNullOrWhiteSpace(statusColumn))
            {
                sql += " AND NVL(TRIM(" + statusColumn.ToLowerInvariant() + "), :pendingStatus) NOT IN (:rejectedStatus1, :rejectedStatus2, :rejectedStatus3, :rejectedStatus4)";
            }

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = SafeString(teacherNo);
                command.Parameters.Add("courseName", OracleDbType.Varchar2).Value = SafeString(courseName);

                if (!string.IsNullOrWhiteSpace(statusColumn))
                {
                    command.Parameters.Add("pendingStatus", OracleDbType.Varchar2).Value = PendingStatus;
                    command.Parameters.Add("rejectedStatus1", OracleDbType.Varchar2).Value = "已拒绝";
                    command.Parameters.Add("rejectedStatus2", OracleDbType.Varchar2).Value = "拒绝";
                    command.Parameters.Add("rejectedStatus3", OracleDbType.Varchar2).Value = "审核不通过";
                    command.Parameters.Add("rejectedStatus4", OracleDbType.Varchar2).Value = "不通过";
                }

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static void InsertApplication(OracleConnection connection, HashSet<string> columns, string applyId, string teacherNo, CourseApplicationInput input)
        {
            List<string> insertColumns = new List<string>();
            List<string> values = new List<string>();

            Add(insertColumns, values, columns, "APPLY_ID", "applyId");
            Add(insertColumns, values, columns, "APPLICATION_ID", "applyId");
            Add(insertColumns, values, columns, "TEACHER_NO", "teacherNo");
            Add(insertColumns, values, columns, "COURSE_NAME", "courseName");
            Add(insertColumns, values, columns, "COURSE_TYPE", "courseType");
            Add(insertColumns, values, columns, "TARGET_MAJOR", "targetMajor");
            Add(insertColumns, values, columns, "TARGET_GRADE", "targetGrade");
            Add(insertColumns, values, columns, "DESCRIPTION", "description");
            Add(insertColumns, values, columns, "TEACHING_PLAN", "teachingPlan");
            Add(insertColumns, values, columns, "TEXTBOOK", "textbook");
            Add(insertColumns, values, columns, "STATUS", "status");
            Add(insertColumns, values, columns, "APPROVE_STATUS", "status");
            Add(insertColumns, values, columns, "REVIEW_STATUS", "status");
            Add(insertColumns, values, columns, "CREDIT", "credit");
            Add(insertColumns, values, columns, "TOTAL_HOURS", "totalHours");
            Add(insertColumns, values, columns, "HOURS", "totalHours");

            AddDate(insertColumns, values, columns, "APPLY_TIME");
            AddDate(insertColumns, values, columns, "CREATE_TIME");
            AddDate(insertColumns, values, columns, "UPDATE_TIME");

            string sql = "INSERT INTO course_application (" + string.Join(", ", insertColumns) + ") VALUES (" + string.Join(", ", values) + ")";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                AddParameterIfUsed(command, sql, "applyId", OracleDbType.Varchar2, applyId);
                AddParameterIfUsed(command, sql, "teacherNo", OracleDbType.Varchar2, SafeString(teacherNo));
                AddParameterIfUsed(command, sql, "courseName", OracleDbType.Varchar2, SafeString(input.CourseName));
                AddParameterIfUsed(command, sql, "courseType", OracleDbType.Varchar2, SafeString(input.CourseType));
                AddParameterIfUsed(command, sql, "targetMajor", OracleDbType.Varchar2, SafeString(input.TargetMajor));
                AddParameterIfUsed(command, sql, "targetGrade", OracleDbType.Varchar2, SafeString(input.TargetGrade));
                AddParameterIfUsed(command, sql, "description", OracleDbType.Clob, SafeString(input.Description));
                AddParameterIfUsed(command, sql, "teachingPlan", OracleDbType.Clob, BuildTeachingPlan(input));
                AddParameterIfUsed(command, sql, "textbook", OracleDbType.Varchar2, DBNull.Value);
                AddParameterIfUsed(command, sql, "status", OracleDbType.Varchar2, PendingStatus);
                AddParameterIfUsed(command, sql, "credit", OracleDbType.Decimal, input.Credit);
                AddParameterIfUsed(command, sql, "totalHours", OracleDbType.Int32, input.TotalHours);
                command.ExecuteNonQuery();
            }
        }

        private static CourseApplicationDto GetById(OracleConnection connection, HashSet<string> columns, string teacherNo, string applyId)
        {
            IList<CourseApplicationDto> applications = GetByTeacher(connection, columns, teacherNo, applyId);
            return applications.Count == 0 ? null : applications[0];
        }

        private static IList<CourseApplicationDto> GetByTeacher(OracleConnection connection, HashSet<string> columns, string teacherNo, string applyId)
        {
            string idColumn = FirstExisting(columns, "APPLY_ID", "APPLICATION_ID");
            string reviewColumn = FirstExisting(columns, "REVIEW_REMARK", "APPROVE_COMMENT", "REMARK");
            string applyTimeColumn = FirstExisting(columns, "APPLY_TIME", "CREATE_TIME");
            string statusColumn = FirstExisting(columns, "STATUS", "APPROVE_STATUS", "REVIEW_STATUS");
            string descriptionColumn = FirstExisting(columns, "DESCRIPTION", "TEACHING_PLAN");
            string totalHoursColumn = FirstExisting(columns, "TOTAL_HOURS", "HOURS");

            string sql = @"
                SELECT " + SelectText(idColumn, "application_id") + @",
                       " + SelectText("COURSE_NAME", "course_name", columns) + @",
                       " + SelectText("COURSE_TYPE", "course_type", columns) + @",
                       " + SelectNumber("CREDIT", "credit", columns) + @",
                       " + SelectNumber(totalHoursColumn, "total_hours") + @",
                       " + SelectText("TARGET_MAJOR", "target_major", columns) + @",
                       " + SelectText("TARGET_GRADE", "target_grade", columns) + @",
                       " + SelectText(descriptionColumn, "description") + @",
                       " + SelectText(statusColumn, "status") + @",
                       " + SelectDate(applyTimeColumn, "apply_time") + @",
                       " + SelectText(reviewColumn, "review_remark") + @"
                  FROM course_application
                 WHERE teacher_no = :teacherNo";

            if (!string.IsNullOrWhiteSpace(applyId) && !string.IsNullOrWhiteSpace(idColumn))
            {
                sql += " AND " + idColumn.ToLowerInvariant() + " = :applyId";
            }

            sql += string.IsNullOrWhiteSpace(applyTimeColumn) ? " ORDER BY 1 DESC" : " ORDER BY " + applyTimeColumn.ToLowerInvariant() + " DESC";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                if (!string.IsNullOrWhiteSpace(applyId) && !string.IsNullOrWhiteSpace(idColumn))
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
                        ApplicationId = Convert.ToString(reader["application_id"]),
                        CourseName = Convert.ToString(reader["course_name"]),
                        CourseType = Convert.ToString(reader["course_type"]),
                        Credit = ToDecimal(reader["credit"]),
                        TotalHours = ToInt32(reader["total_hours"]),
                        TargetMajor = Convert.ToString(reader["target_major"]),
                        TargetGrade = Convert.ToString(reader["target_grade"]),
                        Description = ReadText(reader["description"]),
                        Status = Convert.ToString(reader["status"]),
                        ApplyTime = Convert.ToString(reader["apply_time"]),
                        ReviewRemark = Convert.ToString(reader["review_remark"])
                    });
                }
            }

            return applications;
        }

        private static HashSet<string> GetColumns(OracleConnection connection, string tableName)
        {
            const string sql = @"
                SELECT column_name
                  FROM user_tab_columns
                 WHERE table_name = :tableName";

            HashSet<string> columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("tableName", OracleDbType.Varchar2).Value = tableName.ToUpperInvariant();
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(Convert.ToString(reader["column_name"]));
                    }
                }
            }

            if (columns.Count == 0)
            {
                throw new InvalidOperationException("数据库中未找到 course_application 表，请先执行管理员端初始化脚本。");
            }

            return columns;
        }

        private static void Add(List<string> insertColumns, List<string> values, HashSet<string> columns, string columnName, string parameterName)
        {
            if (!columns.Contains(columnName))
            {
                return;
            }

            insertColumns.Add(columnName.ToLowerInvariant());
            values.Add(":" + parameterName);
        }

        private static void AddDate(List<string> insertColumns, List<string> values, HashSet<string> columns, string columnName)
        {
            if (!columns.Contains(columnName))
            {
                return;
            }

            insertColumns.Add(columnName.ToLowerInvariant());
            values.Add("SYSDATE");
        }

        private static void AddParameterIfUsed(OracleCommand command, string sql, string parameterName, OracleDbType dbType, object value)
        {
            if (!sql.Contains(":" + parameterName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            command.Parameters.Add(parameterName, dbType).Value = value ?? DBNull.Value;
        }

        private static string FirstExisting(HashSet<string> columns, params string[] names)
        {
            return names.FirstOrDefault(columns.Contains) ?? string.Empty;
        }

        private static string SelectText(string columnName, string alias, HashSet<string> columns = null)
        {
            if (string.IsNullOrWhiteSpace(columnName) || (columns != null && !columns.Contains(columnName)))
            {
                return "CAST(NULL AS VARCHAR2(4000)) AS " + alias;
            }

            return columnName.ToLowerInvariant() + " AS " + alias;
        }

        private static string SelectNumber(string columnName, string alias, HashSet<string> columns = null)
        {
            if (string.IsNullOrWhiteSpace(columnName) || (columns != null && !columns.Contains(columnName)))
            {
                return "CAST(NULL AS NUMBER) AS " + alias;
            }

            return columnName.ToLowerInvariant() + " AS " + alias;
        }

        private static string SelectDate(string columnName, string alias)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return "CAST(NULL AS VARCHAR2(20)) AS " + alias;
            }

            return "TO_CHAR(" + columnName.ToLowerInvariant() + ", 'YYYY-MM-DD HH24:MI:SS') AS " + alias;
        }

        private static string BuildTeachingPlan(CourseApplicationInput input)
        {
            return "课程类型：" + SafeString(input.CourseType) + "\n"
                   + "面向专业：" + SafeString(input.TargetMajor) + "\n"
                   + "面向年级：" + SafeString(input.TargetGrade) + "\n"
                   + "课程大纲：" + SafeString(input.Description);
        }

        private static string SafeString(string value)
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


