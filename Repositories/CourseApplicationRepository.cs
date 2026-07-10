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
        public CourseApplicationDto Insert(string teacherNo, CourseApplicationInput input)
        {
            string applyId = Guid.NewGuid().ToString("N");

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                if (HasExtendedColumns(connection))
                {
                    InsertExtended(connection, applyId, teacherNo, input);
                }
                else
                {
                    InsertLegacy(connection, applyId, teacherNo, input);
                }

                return GetById(connection, teacherNo, applyId);
            }
        }

        public IList<CourseApplicationDto> GetByTeacher(string teacherNo)
        {
            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                return HasExtendedColumns(connection)
                    ? GetByTeacherExtended(connection, teacherNo)
                    : GetByTeacherLegacy(connection, teacherNo);
            }
        }

        public bool HasActiveCourseName(string teacherNo, string courseName)
        {
            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                string sql = @"
                    SELECT COUNT(*)
                      FROM course_application
                     WHERE teacher_no = :teacherNo
                       AND course_name = :courseName
                       AND NVL(status, '待审核') <> '驳回'";

                using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
                {
                    command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                    command.Parameters.Add("courseName", OracleDbType.Varchar2).Value = courseName.Trim();
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }
        private static void InsertExtended(OracleConnection connection, string applyId, string teacherNo, CourseApplicationInput input)
        {
            const string sql = @"
                INSERT INTO course_application (
                    apply_id,
                    teacher_no,
                    course_name,
                    course_type,
                    credit,
                    total_hours,
                    target_major,
                    target_grade,
                    description,
                    apply_time,
                    status
                ) VALUES (
                    :applyId,
                    :teacherNo,
                    :courseName,
                    :courseType,
                    :credit,
                    :totalHours,
                    :targetMajor,
                    :targetGrade,
                    :description,
                    SYSDATE,
                    '待审核'
                )";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                AddCommonParameters(command, applyId, teacherNo, input);
                command.Parameters.Add("targetMajor", OracleDbType.Varchar2).Value = FirstText(input.TargetMajor, input.Department);
                command.Parameters.Add("targetGrade", OracleDbType.Varchar2).Value = SafeString(input.TargetGrade);
                command.Parameters.Add("description", OracleDbType.Clob).Value = FirstText(input.Description, input.CourseSummary);
                command.ExecuteNonQuery();
            }
        }

        private static void InsertLegacy(OracleConnection connection, string applyId, string teacherNo, CourseApplicationInput input)
        {
            List<string> columns = new List<string> { "apply_id", "teacher_no", "course_name", "credit", "total_hours", "apply_time", "status" };
            List<string> values = new List<string> { ":applyId", ":teacherNo", ":courseName", ":credit", ":totalHours", "SYSDATE", ":status" };

            AddOptionalInsertColumn(connection, columns, values, "course_type", ":courseType");
            AddOptionalInsertColumn(connection, columns, values, "department", ":department");
            AddOptionalInsertColumn(connection, columns, values, "textbook", ":textbook");
            AddOptionalInsertColumn(connection, columns, values, "course_summary", ":courseSummary");
            AddOptionalInsertColumn(connection, columns, values, "teaching_plan", ":courseSummary");

            string sql = "INSERT INTO course_application (" + string.Join(", ", columns) + ") VALUES (" + string.Join(", ", values) + ")";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                AddCommonParameters(command, applyId, teacherNo, input);
                command.Parameters.Add("status", OracleDbType.Varchar2).Value = "待审核";
                AddIfUsed(command, sql, "department", OracleDbType.Varchar2, FirstText(input.Department, input.TargetMajor));
                AddIfUsed(command, sql, "textbook", OracleDbType.Varchar2, SafeString(input.Textbook));
                AddIfUsed(command, sql, "courseSummary", OracleDbType.Clob, FirstText(input.CourseSummary, input.Description));
                command.ExecuteNonQuery();
            }
        }

        private static CourseApplicationDto GetById(OracleConnection connection, string teacherNo, string applyId)
        {
            IList<CourseApplicationDto> applications = HasExtendedColumns(connection)
                ? GetByTeacherExtended(connection, teacherNo, applyId)
                : GetByTeacherLegacy(connection, teacherNo, applyId);

            return applications.Count == 0 ? null : applications[0];
        }

        private static IList<CourseApplicationDto> GetByTeacherExtended(OracleConnection connection, string teacherNo, string applyId = null)
        {
            string reviewColumn = ExistingColumn(connection, "review_remark", "approve_comment");
            string approveTimeColumn = ExistingColumn(connection, "approve_time", "review_time");

            string sql = @"
                SELECT apply_id,
                       course_name,
                       course_type,
                       credit,
                       total_hours,
                       target_major AS department,
                       target_grade AS textbook,
                       description AS course_summary,
                       status,
                       TO_CHAR(apply_time, 'YYYY-MM-DD HH24:MI:SS') AS apply_time,
                       " + DateSelect(approveTimeColumn, "approve_time") + @",
                       " + TextSelect(reviewColumn, "review_remark") + @"
                  FROM course_application
                 WHERE teacher_no = :teacherNo";

            if (!string.IsNullOrWhiteSpace(applyId))
            {
                sql += " AND apply_id = :applyId";
            }

            sql += " ORDER BY apply_time DESC";
            return QueryApplications(connection, sql, teacherNo, applyId);
        }

        private static IList<CourseApplicationDto> GetByTeacherLegacy(OracleConnection connection, string teacherNo, string applyId = null)
        {
            string courseTypeColumn = ExistingColumn(connection, "course_type");
            string departmentColumn = ExistingColumn(connection, "department", "target_major");
            string textbookColumn = ExistingColumn(connection, "textbook", "target_grade");
            string summaryColumn = ExistingColumn(connection, "course_summary", "description", "teaching_plan");
            string approveTimeColumn = ExistingColumn(connection, "approve_time", "review_time");
            string reviewColumn = ExistingColumn(connection, "approve_comment", "review_remark");

            string sql = @"
                SELECT apply_id,
                       course_name,
                       " + TextSelect(courseTypeColumn, "course_type") + @",
                       credit,
                       total_hours,
                       " + TextSelect(departmentColumn, "department") + @",
                       " + TextSelect(textbookColumn, "textbook") + @",
                       " + TextSelect(summaryColumn, "course_summary") + @",
                       status,
                       TO_CHAR(apply_time, 'YYYY-MM-DD HH24:MI:SS') AS apply_time,
                       " + DateSelect(approveTimeColumn, "approve_time") + @",
                       " + TextSelect(reviewColumn, "review_remark") + @"
                  FROM course_application
                 WHERE teacher_no = :teacherNo";

            if (!string.IsNullOrWhiteSpace(applyId))
            {
                sql += " AND apply_id = :applyId";
            }

            sql += " ORDER BY apply_time DESC";
            return QueryApplications(connection, sql, teacherNo, applyId);
        }

        private static IList<CourseApplicationDto> QueryApplications(OracleConnection connection, string sql, string teacherNo, string applyId)
        {
            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
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
                    string department = Convert.ToString(reader["department"]);
                    string textbook = Convert.ToString(reader["textbook"]);
                    string courseSummary = ReadText(reader["course_summary"]);
                    string reviewRemark = Convert.ToString(reader["review_remark"]);

                    applications.Add(new CourseApplicationDto
                    {
                        ApplicationId = Convert.ToString(reader["apply_id"]),
                        CourseName = Convert.ToString(reader["course_name"]),
                        CourseType = Convert.ToString(reader["course_type"]),
                        Credit = ToDecimal(reader["credit"]),
                        TotalHours = ToInt32(reader["total_hours"]),
                        TargetMajor = department,
                        TargetGrade = textbook,
                        Description = courseSummary,
                        Department = department,
                        Textbook = textbook,
                        CourseSummary = courseSummary,
                        Status = Convert.ToString(reader["status"]),
                        ApplyTime = Convert.ToString(reader["apply_time"]),
                        ApproveTime = Convert.ToString(reader["approve_time"]),
                        ReviewRemark = reviewRemark,
                        ApproveComment = reviewRemark
                    });
                }
            }

            return applications;
        }

        private static void AddCommonParameters(OracleCommand command, string applyId, string teacherNo, CourseApplicationInput input)
        {
            command.Parameters.Add("applyId", OracleDbType.Varchar2).Value = applyId;
            command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
            command.Parameters.Add("courseName", OracleDbType.Varchar2).Value = input.CourseName.Trim();
            command.Parameters.Add("courseType", OracleDbType.Varchar2).Value = SafeString(input.CourseType);
            command.Parameters.Add("credit", OracleDbType.Decimal).Value = input.Credit;
            command.Parameters.Add("totalHours", OracleDbType.Int32).Value = input.TotalHours;
        }

        private static bool HasExtendedColumns(OracleConnection connection)
        {
            return ColumnExists(connection, "COURSE_APPLICATION", "TARGET_MAJOR")
                   && ColumnExists(connection, "COURSE_APPLICATION", "TARGET_GRADE")
                   && ColumnExists(connection, "COURSE_APPLICATION", "DESCRIPTION");
        }

        private static void AddOptionalInsertColumn(OracleConnection connection, IList<string> columns, IList<string> values, string columnName, string parameterName)
        {
            if (ColumnExists(connection, "COURSE_APPLICATION", columnName))
            {
                columns.Add(columnName);
                values.Add(parameterName);
            }
        }

        private static void AddIfUsed(OracleCommand command, string sql, string parameterName, OracleDbType type, object value)
        {
            if (sql.IndexOf(":" + parameterName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                command.Parameters.Add(parameterName, type).Value = value ?? DBNull.Value;
            }
        }

        private static string ExistingColumn(OracleConnection connection, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (ColumnExists(connection, "COURSE_APPLICATION", columnName))
                {
                    return columnName;
                }
            }

            return string.Empty;
        }

        private static string TextSelect(string columnName, string alias)
        {
            return string.IsNullOrWhiteSpace(columnName)
                ? "CAST(NULL AS VARCHAR2(4000)) AS " + alias
                : columnName + " AS " + alias;
        }

        private static string DateSelect(string columnName, string alias)
        {
            return string.IsNullOrWhiteSpace(columnName)
                ? "CAST(NULL AS VARCHAR2(20)) AS " + alias
                : "TO_CHAR(" + columnName + ", 'YYYY-MM-DD HH24:MI:SS') AS " + alias;
        }

        private static bool ColumnExists(OracleConnection connection, string tableName, string columnName)
        {
            const string sql = @"
                SELECT COUNT(*)
                  FROM user_tab_columns
                 WHERE table_name = :tableName
                   AND column_name = :columnName";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("tableName", OracleDbType.Varchar2).Value = tableName.ToUpperInvariant();
                command.Parameters.Add("columnName", OracleDbType.Varchar2).Value = columnName.ToUpperInvariant();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static string FirstText(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim() : SafeString(second);
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
            if (clob != null)
            {
                return clob.Value;
            }

            return Convert.ToString(value);
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