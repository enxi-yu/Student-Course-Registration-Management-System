using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using StudentCourse.Infrastructure;
using StudentCourse.Models;

namespace StudentCourse.Repositories
{
    public sealed class TeacherRepository
    {
        public TeacherInfo GetTeacherInfoByUserId(int userId)
        {
            const string sql = @"
                SELECT u.user_id,
                       u.username,
                       u.real_name,
                       t.teacher_no,
                       t.title,
                       t.department,
                       u.phone,
                       u.email
                  FROM ""user"" u
                  JOIN teacher t ON t.user_id = u.user_id
                 WHERE u.user_id = :userId";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapTeacherInfo(reader) : null;
                }
            }
        }

        public TeacherInfo GetTeacherInfoByTeacherNo(string teacherNo)
        {
            const string sql = @"
                SELECT u.user_id,
                       u.username,
                       u.real_name,
                       t.teacher_no,
                       t.title,
                       t.department,
                       u.phone,
                       u.email
                  FROM teacher t
                  JOIN ""user"" u ON u.user_id = t.user_id
                 WHERE t.teacher_no = :teacherNo";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapTeacherInfo(reader) : null;
                }
            }
        }

        public TeacherDashboardDto GetDashboard(string teacherNo, string semester)
        {
            string normalizedSemester = (semester ?? string.Empty).Trim();
            string semesterAlias = GetSemesterAlias(normalizedSemester);
            const string sql = @"
                SELECT COUNT(DISTINCT tc.class_id) AS class_count,
                       COUNT(DISTINCT s.course_id) AS course_count,
                       COUNT(cs.select_id) AS student_count,
                       SUM(
                           CASE
                               WHEN cs.select_id IS NOT NULL
                                    AND NOT EXISTS (
                                        SELECT 1
                                          FROM student_score ss
                                         WHERE ss.class_id = cs.class_id
                                           AND ss.student_no = cs.student_no
                                           AND ss.total_score IS NOT NULL
                                    )
                               THEN 1
                               ELSE 0
                           END
                       ) AS pending_score_count
                  FROM teaching_class tc
                  JOIN section s ON s.section_id = tc.section_id
                  LEFT JOIN course_select cs ON cs.class_id = tc.class_id
                 WHERE tc.teacher_no = :teacherNo
                   AND (s.semester = :semester OR s.semester = :semesterAlias)";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                command.Parameters.Add("semester", OracleDbType.Varchar2).Value = normalizedSemester;
                command.Parameters.Add("semesterAlias", OracleDbType.Varchar2).Value = semesterAlias;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return new TeacherDashboardDto();
                    }

                    return new TeacherDashboardDto
                    {
                        ClassCount = ToInt32(reader["class_count"]),
                        CourseCount = ToInt32(reader["course_count"]),
                        StudentCount = ToInt32(reader["student_count"]),
                        PendingScoreCount = ToInt32(reader["pending_score_count"])
                    };
                }
            }
        }

        private static string GetSemesterAlias(string semester)
        {
            string[] parts = semester.Split('-');
            if (parts.Length != 3 || !int.TryParse(parts[0], out int startYear))
            {
                return semester;
            }

            return parts[2] == "1" ? $"{startYear}-fall" : $"{startYear + 1}-spring";
        }

        public IList<TeacherClassDto> GetMyCourses(string teacherNo, string semester)
        {
            const string sql = @"
                SELECT tc.class_id,
                       tc.class_name,
                       c.course_id,
                       c.course_name,
                       s.semester,
                       c.credit,
                       c.total_hours,
                       tc.capacity,
                       tc.selected_count,
                       c.department,
                       c.course_desc
                  FROM teaching_class tc
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                 WHERE tc.teacher_no = :teacherNo
                   AND (:semester IS NULL OR s.semester = :semester)
                 ORDER BY s.semester DESC, c.course_name, tc.class_name";

            List<TeacherClassDto> courses = new List<TeacherClassDto>();

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                command.Parameters.Add("semester", OracleDbType.Varchar2).Value =
                    string.IsNullOrWhiteSpace(semester) ? (object)DBNull.Value : semester.Trim();

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new TeacherClassDto
                        {
                            ClassId = ToInt32(reader["class_id"]),
                            ClassName = Convert.ToString(reader["class_name"]),
                            CourseId = ToInt32(reader["course_id"]),
                            CourseName = Convert.ToString(reader["course_name"]),
                            Semester = Convert.ToString(reader["semester"]),
                            Credit = ToDecimal(reader["credit"]),
                            TotalHours = ToInt32(reader["total_hours"]),
                            Capacity = ToInt32(reader["capacity"]),
                            SelectedCount = ToInt32(reader["selected_count"]),
                            Department = Convert.ToString(reader["department"]),
                            Description = ReadText(reader["course_desc"])
                        });
                    }
                }
            }

            return courses;
        }

        public IList<TeacherScheduleDto> GetMySchedule(string teacherNo, string semester)
        {
            const string sql = @"
                SELECT ct.time_id,
                       tc.class_id,
                       tc.class_name,
                       c.course_id,
                       c.course_name,
                       s.semester,
                       ct.weekday,
                       ct.start_period,
                       ct.end_period,
                       ct.week_range,
                       ct.classroom,
                       c.credit,
                       c.total_hours
                  FROM course_time ct
                  JOIN teaching_class tc ON tc.class_id = ct.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                 WHERE tc.teacher_no = :teacherNo
                   AND (:semester IS NULL OR s.semester = :semester)
                 ORDER BY s.semester DESC, ct.weekday, ct.start_period, c.course_name, tc.class_name";

            List<TeacherScheduleDto> schedule = new List<TeacherScheduleDto>();

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                command.Parameters.Add("semester", OracleDbType.Varchar2).Value =
                    string.IsNullOrWhiteSpace(semester) ? (object)DBNull.Value : semester.Trim();

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schedule.Add(new TeacherScheduleDto
                        {
                            TimeId = ToInt32(reader["time_id"]),
                            ClassId = ToInt32(reader["class_id"]),
                            ClassName = Convert.ToString(reader["class_name"]),
                            CourseId = ToInt32(reader["course_id"]),
                            CourseName = Convert.ToString(reader["course_name"]),
                            Semester = Convert.ToString(reader["semester"]),
                            Weekday = ToInt32(reader["weekday"]),
                            StartPeriod = ToInt32(reader["start_period"]),
                            EndPeriod = ToInt32(reader["end_period"]),
                            WeekRange = Convert.ToString(reader["week_range"]),
                            Classroom = Convert.ToString(reader["classroom"]),
                            Credit = ToDecimal(reader["credit"]),
                            TotalHours = ToInt32(reader["total_hours"])
                        });
                    }
                }
            }

            return schedule;
        }

        public void UpdateContactInfo(int userId, string phone, string email)
        {
            const string sql = @"
                UPDATE ""user""
                   SET phone = :phone,
                       email = :email
                 WHERE user_id = :userId";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("phone", OracleDbType.Varchar2).Value =
                    string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone;
                command.Parameters.Add("email", OracleDbType.Varchar2).Value =
                    string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email;
                command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                if (command.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException("未找到当前用户，无法修改个人资料。");
                }
            }
        }

        public IList<TeacherEvaluationDto> GetEvaluations(string teacherNo, string semester)
        {
            const string sql = @"
                SELECT tc.class_id, tc.class_name, c.course_name, s.semester,
                       ce.d1_score, ce.d2_score, ce.d3_score, ce.d4_score,
                       ce.eval_score, ce.eval_content,
                       TO_CHAR(ce.eval_time, 'YYYY-MM-DD HH24:MI') evaluation_time
                  FROM course_evaluation ce
                  JOIN teaching_class tc ON tc.class_id = ce.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                 WHERE tc.teacher_no = :teacherNo
                   AND (:semester IS NULL OR s.semester = :semester)
                 ORDER BY s.semester DESC, c.course_name, tc.class_name, ce.eval_time DESC";
            var rows = new List<TeacherEvaluationDto>();
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
            command.Parameters.Add("semester", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(semester) ? (object)DBNull.Value : semester.Trim();
            using OracleDataReader reader = command.ExecuteReader();
            while (reader.Read()) rows.Add(new TeacherEvaluationDto
            {
                ClassId = ToInt32(reader["class_id"]), ClassName = Convert.ToString(reader["class_name"]) ?? "",
                CourseName = Convert.ToString(reader["course_name"]) ?? "", Semester = Convert.ToString(reader["semester"]) ?? "",
                D1Score = ToInt32(reader["d1_score"]), D2Score = ToInt32(reader["d2_score"]), D3Score = ToInt32(reader["d3_score"]), D4Score = ToInt32(reader["d4_score"]),
                EvalScore = ToDecimal(reader["eval_score"]), Comment = ReadText(reader["eval_content"]), EvaluationTime = Convert.ToString(reader["evaluation_time"]) ?? ""
            });
            return rows;
        }

        private static OracleCommand CreateCommand(OracleConnection connection, string sql)
        {
            OracleCommand command = new OracleCommand(sql, connection);
            command.BindByName = true;
            return command;
        }

        private static TeacherInfo MapTeacherInfo(OracleDataReader reader)
        {
            return new TeacherInfo
            {
                UserId = ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]),
                TeacherName = Convert.ToString(reader["real_name"]),
                TeacherNo = Convert.ToString(reader["teacher_no"]),
                Title = Convert.ToString(reader["title"]),
                Department = Convert.ToString(reader["department"]),
                Phone = Convert.ToString(reader["phone"]) ?? string.Empty,
                Email = Convert.ToString(reader["email"]) ?? string.Empty
            };
        }

        private static int ToInt32(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(value);
        }

        private static decimal ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0m;
            }

            return Convert.ToDecimal(value);
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
    }
}
