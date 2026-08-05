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
                    teaching_plan,
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
                    :description,
                    SYSDATE,
                    '待审核'
                )";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                AddApplicationParameters(command, applyId, teacherNo, input);
                command.ExecuteNonQuery();
            }
        }

        private static void InsertLegacy(OracleConnection connection, string applyId, string teacherNo, CourseApplicationInput input)
        {
            const string sql = @"
                INSERT INTO course_application (
                    apply_id,
                    teacher_no,
                    course_name,
                    credit,
                    total_hours,
                    teaching_plan,
                    textbook,
                    apply_time,
                    status
                ) VALUES (
                    :applyId,
                    :teacherNo,
                    :courseName,
                    :credit,
                    :totalHours,
                    :teachingPlan,
                    :textbook,
                    SYSDATE,
                    '待审核'
                )";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("applyId", OracleDbType.Varchar2).Value = applyId;
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                command.Parameters.Add("courseName", OracleDbType.Varchar2).Value = input.CourseName.Trim();
                command.Parameters.Add("credit", OracleDbType.Decimal).Value = input.Credit;
                command.Parameters.Add("totalHours", OracleDbType.Int32).Value = input.TotalHours;
                command.Parameters.Add("teachingPlan", OracleDbType.Clob).Value = BuildLegacyPlan(input);
                command.Parameters.Add("textbook", OracleDbType.Varchar2).Value = DBNull.Value;
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
            string reviewColumn = ColumnExists(connection, "COURSE_APPLICATION", "REVIEW_REMARK")
                ? "review_remark"
                : "approve_comment";

            string sql = @"
                SELECT apply_id,
                       course_name,
                       course_type,
                       credit,
                       total_hours,
                       target_major,
                       target_grade,
                       description,
                       status,
                       TO_CHAR(apply_time, 'YYYY-MM-DD HH24:MI:SS') AS apply_time,
                       " + reviewColumn + @" AS review_remark
                  FROM course_application
                 WHERE teacher_no = :teacherNo";

            if (!string.IsNullOrWhiteSpace(applyId))
            {
                sql += " AND apply_id = :applyId";
            }

            sql += " ORDER BY apply_time DESC";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                if (!string.IsNullOrWhiteSpace(applyId))
                {
                    command.Parameters.Add("applyId", OracleDbType.Varchar2).Value = applyId;
                }

                return ReadApplications(command, true);
            }
        }

        private static IList<CourseApplicationDto> GetByTeacherLegacy(OracleConnection connection, string teacherNo, string applyId = null)
        {
            string sql = @"
                SELECT apply_id,
                       course_name,
                       credit,
                       total_hours,
                       teaching_plan,
                       status,
                       TO_CHAR(apply_time, 'YYYY-MM-DD HH24:MI:SS') AS apply_time,
                       approve_comment AS review_remark
                  FROM course_application
                 WHERE teacher_no = :teacherNo";

            if (!string.IsNullOrWhiteSpace(applyId))
            {
                sql += " AND apply_id = :applyId";
            }

            sql += " ORDER BY apply_time DESC";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                if (!string.IsNullOrWhiteSpace(applyId))
                {
                    command.Parameters.Add("applyId", OracleDbType.Varchar2).Value = applyId;
                }

                return ReadApplications(command, false);
            }
        }

        private static IList<CourseApplicationDto> ReadApplications(OracleCommand command, bool extended)
        {
            List<CourseApplicationDto> applications = new List<CourseApplicationDto>();

            using (OracleDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    applications.Add(new CourseApplicationDto
                    {
                        ApplicationId = Convert.ToString(reader["apply_id"]),
                        CourseName = Convert.ToString(reader["course_name"]),
                        CourseType = extended ? Convert.ToString(reader["course_type"]) : string.Empty,
                        Credit = ToDecimal(reader["credit"]),
                        TotalHours = ToInt32(reader["total_hours"]),
                        TargetMajor = extended ? Convert.ToString(reader["target_major"]) : string.Empty,
                        TargetGrade = extended ? Convert.ToString(reader["target_grade"]) : string.Empty,
                        Description = extended ? ReadText(reader["description"]) : ReadText(reader["teaching_plan"]),
                        Status = Convert.ToString(reader["status"]),
                        ApplyTime = Convert.ToString(reader["apply_time"]),
                        ReviewRemark = Convert.ToString(reader["review_remark"])
                    });
                }
            }

            return applications;
        }

        private static void AddApplicationParameters(OracleCommand command, string applyId, string teacherNo, CourseApplicationInput input)
        {
            command.Parameters.Add("applyId", OracleDbType.Varchar2).Value = applyId;
            command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
            command.Parameters.Add("courseName", OracleDbType.Varchar2).Value = input.CourseName.Trim();
            command.Parameters.Add("courseType", OracleDbType.Varchar2).Value = SafeString(input.CourseType);
            command.Parameters.Add("credit", OracleDbType.Decimal).Value = input.Credit;
            command.Parameters.Add("totalHours", OracleDbType.Int32).Value = input.TotalHours;
            command.Parameters.Add("targetMajor", OracleDbType.Varchar2).Value = SafeString(input.TargetMajor);
            command.Parameters.Add("targetGrade", OracleDbType.Varchar2).Value = SafeString(input.TargetGrade);
            command.Parameters.Add("description", OracleDbType.Clob).Value = SafeString(input.Description);
        }

        private static bool HasExtendedColumns(OracleConnection connection)
        {
            return ColumnExists(connection, "COURSE_APPLICATION", "COURSE_TYPE")
                   && ColumnExists(connection, "COURSE_APPLICATION", "TARGET_MAJOR")
                   && ColumnExists(connection, "COURSE_APPLICATION", "TARGET_GRADE")
                   && ColumnExists(connection, "COURSE_APPLICATION", "DESCRIPTION");
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

        private static string BuildLegacyPlan(CourseApplicationInput input)
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
