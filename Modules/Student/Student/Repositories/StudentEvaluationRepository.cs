using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using StudentCourse.Infrastructure;
using StudentCourse.Student.Models;

namespace StudentCourse.Student.Repositories
{
    public sealed class StudentEvaluationRepository
    {
        public List<CourseEvaluationDto> GetEvaluableCourses(string studentNo)
        {
            var courses = new List<CourseEvaluationDto>();

            const string sql = @"
                SELECT TO_CHAR(c.course_id) AS course_code,
                       c.course_name,
                       s.semester,
                       c.credit,
                       tc.class_id,
                       NVL(u.real_name, '未分配') AS teacher_name,
                       CASE WHEN ss.total_score IS NOT NULL THEN 1 ELSE 0 END AS is_graded,
                       CASE WHEN ce.eval_id IS NOT NULL THEN 1 ELSE 0 END AS has_evaluated,
                       ce.eval_score AS rating,
                       ce.eval_content AS eval_comment,
                       TO_CHAR(ce.eval_time, 'YYYY-MM-DD HH24:MI:SS') AS evaluation_date
                  FROM course_select cs
                  JOIN teaching_class tc ON tc.class_id = cs.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  LEFT JOIN teacher t ON t.teacher_no = tc.teacher_no
                  LEFT JOIN ""user"" u ON t.user_id = u.user_id
                  LEFT JOIN student_score ss ON ss.class_id = cs.class_id AND ss.student_no = cs.student_no
                  LEFT JOIN course_evaluation ce ON ce.class_id = cs.class_id AND ce.student_no = cs.student_no
                 WHERE cs.student_no = :studentNo
                 ORDER BY s.semester DESC, c.course_name";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new CourseEvaluationDto
                        {
                            ClassId = SafeGetInt(reader["class_id"]),
                            CourseCode = SafeGetString(reader["course_code"]),
                            CourseName = SafeGetString(reader["course_name"]),
                            TeacherName = SafeGetString(reader["teacher_name"]),
                            Semester = SafeGetString(reader["semester"]),
                            Credit = SafeGetDecimal(reader["credit"]),
                            HasEvaluated = SafeGetInt(reader["has_evaluated"]) == 1,
                            Rating = reader["rating"] == DBNull.Value ? null : SafeGetDecimal(reader["rating"]),
                            Comment = SafeGetString(reader["eval_comment"]),
                            EvaluationDate = SafeGetString(reader["evaluation_date"]),
                            IsGraded = SafeGetInt(reader["is_graded"]) == 1
                        });
                    }
                }
            }

            return courses;
        }

        public void SubmitEvaluation(string studentNo, int classId, int d1, int d2, int d3, int d4, string comment)
        {
            const string updateSql = @"
                UPDATE course_evaluation
                   SET d1_score = :d1, d2_score = :d2, d3_score = :d3, d4_score = :d4,
                       eval_score = :evalScore, eval_content = :evalContent, eval_time = SYSDATE
                 WHERE student_no = :studentNo AND class_id = :classId";
            const string insertSql = @"
                INSERT INTO course_evaluation
                    (eval_id, student_no, class_id, d1_score, d2_score, d3_score, d4_score, eval_score, eval_content, eval_time)
                VALUES (:evalId, :studentNo, :classId, :d1, :d2, :d3, :d4, :evalScore, :evalContent, SYSDATE)";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    decimal evalScore = Math.Round((d1 + d2 + d3 + d4) / 4.0m, 1);
                    using OracleCommand update = CreateCommand(connection, updateSql);
                    update.Transaction = transaction;
                    AddEvaluationParameters(update, studentNo, classId, d1, d2, d3, d4, evalScore, comment, includeId: false);
                    if (update.ExecuteNonQuery() == 0)
                    {
                        using OracleCommand insert = CreateCommand(connection, insertSql);
                        insert.Transaction = transaction;
                        AddEvaluationParameters(insert, studentNo, classId, d1, d2, d3, d4, evalScore, comment, includeId: true);
                        insert.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private static void AddEvaluationParameters(OracleCommand command, string studentNo, int classId, int d1, int d2, int d3, int d4, decimal evalScore, string comment, bool includeId)
        {
            if (includeId) command.Parameters.Add("evalId", OracleDbType.Varchar2).Value = Guid.NewGuid().ToString("N");
            command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
            command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
            command.Parameters.Add("d1", OracleDbType.Int32).Value = d1;
            command.Parameters.Add("d2", OracleDbType.Int32).Value = d2;
            command.Parameters.Add("d3", OracleDbType.Int32).Value = d3;
            command.Parameters.Add("d4", OracleDbType.Int32).Value = d4;
            command.Parameters.Add("evalScore", OracleDbType.Decimal).Value = evalScore;
            command.Parameters.Add("evalContent", OracleDbType.Varchar2, 500).Value = string.IsNullOrWhiteSpace(comment) ? (object)DBNull.Value : comment.Trim();
        }

        public List<CourseEvaluationDto> GetEvaluationHistory(string studentNo)
        {
            var evaluations = new List<CourseEvaluationDto>();

            const string sql = @"
                SELECT tc.class_id,
                        TO_CHAR(c.course_id) AS course_code,
                        c.course_name,
                        NVL(u.real_name, '未分配') AS teacher_name,
                        s.semester,
                        c.credit,
                        ce.eval_score AS rating,
                        ce.eval_content AS eval_comment,
                        TO_CHAR(ce.eval_time, 'YYYY-MM-DD HH24:MI:SS') AS evaluation_date
                FROM course_evaluation ce
                JOIN teaching_class tc ON tc.class_id = ce.class_id
                JOIN section s ON s.section_id = tc.section_id
                JOIN course c ON c.course_id = s.course_id
                LEFT JOIN teacher t ON t.teacher_no = tc.teacher_no
                LEFT JOIN ""user"" u ON t.user_id = u.user_id
                WHERE ce.student_no = :studentNo
                ORDER BY ce.eval_time DESC";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        evaluations.Add(new CourseEvaluationDto
                        {
                            ClassId = SafeGetInt(reader["class_id"]),
                            CourseCode = SafeGetString(reader["course_code"]),
                            CourseName = SafeGetString(reader["course_name"]),
                            TeacherName = SafeGetString(reader["teacher_name"]),
                            Semester = SafeGetString(reader["semester"]),
                            Credit = SafeGetDecimal(reader["credit"]),
                            Rating = SafeGetDecimal(reader["rating"]),
                            Comment = SafeGetString(reader["eval_comment"]),
                            EvaluationDate = SafeGetString(reader["evaluation_date"]),
                            HasEvaluated = true,
                            IsGraded = true
                        });
                    }
                }
            }

            return evaluations;
        }

        private static OracleCommand CreateCommand(OracleConnection connection, string sql)
        {
            OracleCommand command = new OracleCommand(sql, connection);
            command.BindByName = true;
            return command;
        }

        public static string SafeGetString(object value)
        {
            if (value is Oracle.ManagedDataAccess.Types.OracleClob clob) return clob.IsNull ? string.Empty : clob.Value;
            return value == DBNull.Value ? string.Empty : value?.ToString() ?? string.Empty;
        }

        public static decimal SafeGetDecimal(object value)
        {
            if (value == DBNull.Value) return 0m;
            return Convert.ToDecimal(value);
        }

        public static int SafeGetInt(object value)
        {
            if (value == DBNull.Value) return 0;
            return Convert.ToInt32(value);
        }
    }
}
