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
                SELECT tc.class_id,
                       TO_CHAR(c.course_id) AS course_code,
                       c.course_name,
                       NVL(t.real_name, '未分配') AS teacher_name,
                       s.semester,
                       c.credit,
                       ce.rating,
                       ce.comment,
                       TO_CHAR(ce.evaluation_date, 'YYYY-MM-DD HH24:MI:SS') AS evaluation_date
                  FROM course_select cs
                  JOIN teaching_class tc ON tc.class_id = cs.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  LEFT JOIN teacher t ON t.teacher_no = tc.teacher_no
                  LEFT JOIN course_evaluation ce ON ce.student_no = cs.student_no AND ce.class_id = cs.class_id
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
                        int? rating = null;
                        if (reader["rating"] != DBNull.Value)
                        {
                            rating = SafeGetInt(reader["rating"]);
                        }

                        courses.Add(new CourseEvaluationDto
                        {
                            ClassId = SafeGetInt(reader["class_id"]),
                            CourseCode = SafeGetString(reader["course_code"]),
                            CourseName = SafeGetString(reader["course_name"]),
                            TeacherName = SafeGetString(reader["teacher_name"]),
                            Semester = SafeGetString(reader["semester"]),
                            Credit = SafeGetDecimal(reader["credit"]),
                            Rating = rating,
                            Comment = SafeGetString(reader["comment"]),
                            EvaluationDate = SafeGetString(reader["evaluation_date"]),
                            HasEvaluated = reader["rating"] != DBNull.Value
                        });
                    }
                }
            }

            return courses;
        }

        public void SubmitEvaluation(string studentNo, int classId, int rating, string comment)
        {
            const string mergeSql = @"
                MERGE INTO course_evaluation ce
                USING (SELECT :studentNo AS student_no, :classId AS class_id FROM dual) src
                   ON (ce.student_no = src.student_no AND ce.class_id = src.class_id)
                 WHEN MATCHED THEN
                      UPDATE SET ce.rating = :rating,
                                 ce.comment = :comment,
                                 ce.evaluation_date = SYSDATE
                 WHEN NOT MATCHED THEN
                      INSERT (evaluation_id, student_no, class_id, rating, comment, evaluation_date)
                      VALUES (course_evaluation_seq.NEXTVAL, :studentNo, :classId, :rating, :comment, SYSDATE)";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, mergeSql))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                command.Parameters.Add("rating", OracleDbType.Int32).Value = rating;
                command.Parameters.Add("comment", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(comment) ? DBNull.Value : comment;
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
                       NVL(t.real_name, '未分配') AS teacher_name,
                       s.semester,
                       c.credit,
                       ce.rating,
                       ce.comment,
                       TO_CHAR(ce.evaluation_date, 'YYYY-MM-DD HH24:MI:SS') AS evaluation_date
                  FROM course_evaluation ce
                  JOIN teaching_class tc ON tc.class_id = ce.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  LEFT JOIN teacher t ON t.teacher_no = tc.teacher_no
                 WHERE ce.student_no = :studentNo
                 ORDER BY ce.evaluation_date DESC";

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
                            Comment = SafeGetString(reader["comment"]),
                            EvaluationDate = SafeGetString(reader["evaluation_date"]),
                            HasEvaluated = true
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
