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
                       CASE WHEN ss.total_score IS NOT NULL THEN 1 ELSE 0 END AS is_graded 
                  FROM course_select cs
                  JOIN teaching_class tc ON tc.class_id = cs.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  LEFT JOIN teacher t ON t.teacher_no = tc.teacher_no
                  LEFT JOIN ""user"" u ON t.user_id = u.user_id
                  LEFT JOIN student_score ss ON ss.class_id = cs.class_id AND ss.student_no = cs.student_no
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
                            HasEvaluated = false,
                            IsGraded = SafeGetInt(reader["is_graded"]) == 1
                        });
                    }
                }
            }

            return courses;
        }

        public void SubmitEvaluation(string studentNo, int classId, int d1, int d2, int d3, int d4, string comment)
        {
            const string mergeSql = @"
                MERGE INTO course_evaluation ce
                USING (SELECT :studentNo AS student_no, :classId AS class_id FROM dual) src
                   ON (ce.student_no = src.student_no AND ce.class_id = src.class_id)
                 WHEN MATCHED THEN
                      UPDATE SET ce.d1_score = :d1,
                                 ce.d2_score = :d2,
                                 ce.d3_score = :d3,
                                 ce.d4_score = :d4,
                                 ce.eval_score = :evalScore,
                                 ce.eval_content = :comment,
                                 ce.eval_time = SYSDATE
                 WHEN NOT MATCHED THEN
                      INSERT (eval_id, student_no, class_id, d1_score, d2_score, d3_score, d4_score, eval_score, eval_content, eval_time)
                      VALUES (:evalId, :studentNo, :classId, :d1, :d2, :d3, :d4, :evalScore, :comment, SYSDATE)";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, mergeSql))
            {
                decimal evalScore = (d1 + d2 + d3 + d4) / 4.0m;

                string evalId = Guid.NewGuid().ToString("N").Substring(0, 32);
                command.Parameters.Add("evalId", OracleDbType.Varchar2).Value = evalId;
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;

                command.Parameters.Add("d1", OracleDbType.Int32).Value = d1;
                command.Parameters.Add("d2", OracleDbType.Int32).Value = d2;
                command.Parameters.Add("d3", OracleDbType.Int32).Value = d3;
                command.Parameters.Add("d4", OracleDbType.Int32).Value = d4;

                command.Parameters.Add("evalScore", OracleDbType.Decimal).Value = evalScore;
                command.Parameters.Add("comment", OracleDbType.Clob).Value = string.IsNullOrEmpty(comment) ? (object)DBNull.Value : comment;

                command.ExecuteNonQuery();
            }
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
                            Rating = SafeGetInt(reader["rating"]),
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