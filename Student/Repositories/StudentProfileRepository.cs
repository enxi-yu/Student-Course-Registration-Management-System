using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using StudentCourse.Infrastructure;
using StudentCourse.Student.Models;

namespace StudentCourse.Student.Repositories
{
    public sealed class StudentProfileRepository
    {
        public StudentInfo? GetStudentInfoByUserId(int userId)
        {
            const string sql = @"
                SELECT u.user_id,
                       u.username,
                       u.real_name,
                       s.student_no,
                       s.major,
                       s.grade,
                       u.phone,
                       u.email,
                       s.avg_gpa,
                       s.credit_finished
                  FROM ""user"" u
                  JOIN student s ON s.user_id = u.user_id
                 WHERE u.user_id = :userId";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapStudentInfo(reader) : null;
                }
            }
        }

        public StudentInfo? GetStudentInfoByStudentNo(string studentNo)
        {
            const string sql = @"
                SELECT u.user_id,
                       u.username,
                       u.real_name,
                       s.student_no,
                       s.major,
                       s.grade,
                       u.phone,
                       u.email,
                       s.avg_gpa,
                       s.credit_finished
                  FROM student s
                  JOIN ""user"" u ON u.user_id = s.user_id
                 WHERE s.student_no = :studentNo";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapStudentInfo(reader) : null;
                }
            }
        }

        public StudentDashboardDto GetDashboard(string studentNo)
        {
            var dashboard = new StudentDashboardDto();

            dashboard.Profile = GetStudentInfoByStudentNo(studentNo) ?? new StudentInfo();

            const string semesterSql = @"
                SELECT COUNT(DISTINCT cs.class_id) AS course_count,
                       NVL(SUM(c.credit), 0) AS total_credit
                  FROM course_select cs
                  JOIN teaching_class tc ON tc.class_id = cs.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                 WHERE cs.student_no = :studentNo
                   AND s.semester = (SELECT MAX(semester) FROM section)";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, semesterSql))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        dashboard.CurrentSemesterCourseCount = Convert.ToInt32(reader["course_count"] ?? 0);
                        dashboard.CurrentSemesterCredit = Convert.ToDecimal(reader["total_credit"] ?? 0m);
                    }
                }
            }

            dashboard.TodayCourses = GetTodaySchedule(studentNo);
            dashboard.GpaSummary = GetGpaSummary(studentNo);

            return dashboard;
        }

        public GpaSummaryDto GetGpaSummary(string studentNo)
        {
            var summary = new GpaSummaryDto();

            const string gpaSql = @"
                SELECT NVL(SUM(ss.credit_obtained), 0) AS total_credits,
                       NVL(AVG(CASE WHEN ss.gpa > 0 THEN ss.gpa END), 0) AS avg_gpa,
                       COUNT(*) AS total_courses
                  FROM student_score ss
                 WHERE ss.student_no = :studentNo";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, gpaSql))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        summary.TotalCreditsFinished = Convert.ToDecimal(reader["total_credits"] ?? 0m);
                        summary.AvgGpa = Math.Round(Convert.ToDecimal(reader["avg_gpa"] ?? 0m), 2);
                        summary.TotalCourses = Convert.ToInt32(reader["total_courses"] ?? 0);
                    }
                }
            }

            summary.CreditTrend = GetCreditTrend(studentNo);
            return summary;
        }

        private List<CreditTrendItem> GetCreditTrend(string studentNo)
        {
            var trend = new List<CreditTrendItem>();

            const string trendSql = @"
                SELECT s.semester,
                       SUM(ss.credit_obtained) AS credits
                  FROM student_score ss
                  JOIN teaching_class tc ON tc.class_id = ss.class_id
                  JOIN section s ON s.section_id = tc.section_id
                 WHERE ss.student_no = :studentNo
                   AND ss.credit_obtained > 0
                 GROUP BY s.semester
                 ORDER BY s.semester";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, trendSql))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        trend.Add(new CreditTrendItem
                        {
                            Semester = Convert.ToString(reader["semester"]) ?? string.Empty,
                            Credits = Convert.ToDecimal(reader["credits"] ?? 0m)
                        });
                    }
                }
            }

            return trend;
        }

        private List<ScheduleItemDto> GetTodaySchedule(string studentNo)
        {
            var items = new List<ScheduleItemDto>();

            const string sql = @"
                SELECT tc.class_id,
                       c.course_name,
                       tc.class_name,
                       u.real_name AS teacher_name,
                       ct.classroom,
                       ct.weekday,
                       ct.start_period,
                       ct.end_period,
                       ct.week_range
                  FROM course_select cs
                  JOIN teaching_class tc ON tc.class_id = cs.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  JOIN course_time ct ON ct.class_id = tc.class_id
                  JOIN teacher t ON t.teacher_no = tc.teacher_no
                  JOIN ""user"" u ON u.user_id = t.user_id
                 WHERE cs.student_no = :studentNo
                   AND ct.weekday = TO_NUMBER(TO_CHAR(SYSDATE, 'D'))
                 ORDER BY ct.start_period";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(MapScheduleItem(reader));
                    }
                }
            }

            return items;
        }

        private static OracleCommand CreateCommand(OracleConnection connection, string sql)
        {
            OracleCommand command = new OracleCommand(sql, connection);
            command.BindByName = true;
            return command;
        }

        private static StudentInfo MapStudentInfo(OracleDataReader reader)
        {
            return new StudentInfo
            {
                UserId = SafeGetInt(reader["user_id"]),
                Username = SafeGetString(reader["username"]),
                StudentNo = SafeGetString(reader["student_no"]),
                RealName = SafeGetString(reader["real_name"]),
                Major = SafeGetString(reader["major"]),
                Grade = SafeGetString(reader["grade"]),
                Phone = SafeGetString(reader["phone"]),
                Email = SafeGetString(reader["email"]),
                AvgGpa = SafeGetDecimal(reader["avg_gpa"]),
                CreditFinished = SafeGetDecimal(reader["credit_finished"])
            };
        }

        private static ScheduleItemDto MapScheduleItem(OracleDataReader reader)
        {
            return new ScheduleItemDto
            {
                ClassId = SafeGetInt(reader["class_id"]),
                CourseName = SafeGetString(reader["course_name"]),
                ClassName = SafeGetString(reader["class_name"]),
                TeacherName = SafeGetString(reader["teacher_name"]),
                Classroom = SafeGetString(reader["classroom"]),
                Weekday = SafeGetInt(reader["weekday"]),
                StartPeriod = SafeGetInt(reader["start_period"]),
                EndPeriod = SafeGetInt(reader["end_period"]),
                WeekRange = SafeGetString(reader["week_range"])
            };
        }

        internal static int SafeGetInt(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return Convert.ToInt32(value);
        }

        internal static decimal SafeGetDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            return Convert.ToDecimal(value);
        }

        internal static string SafeGetString(object value)
        {
            if (value == null || value == DBNull.Value) return string.Empty;
            return Convert.ToString(value) ?? string.Empty;
        }

        internal static string ReadClob(object value)
        {
            if (value == null || value == DBNull.Value) return string.Empty;
            if (value is OracleClob clob) return clob.Value ?? string.Empty;
            return Convert.ToString(value) ?? string.Empty;
        }
    }
}
