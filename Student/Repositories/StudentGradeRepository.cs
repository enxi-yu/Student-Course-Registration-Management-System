using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using StudentCourse.Infrastructure;
using StudentCourse.Student.Models;

namespace StudentCourse.Student.Repositories
{
    public sealed class StudentGradeRepository
    {
        public List<EnrolledCourseDto> GetEnrolledCourses(string studentNo)
        {
            var courses = new List<EnrolledCourseDto>();

            const string sql = @"
                SELECT TO_CHAR(c.course_id) AS course_code,
                       c.course_name,
                       s.semester,
                       c.credit,
                       ss.credit_obtained,
                       ss.total_score,
                       ss.grade_level,
                       ss.gpa
                  FROM student_score ss
                  JOIN teaching_class tc ON tc.class_id = ss.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                 WHERE ss.student_no = :studentNo
                 ORDER BY s.semester DESC, c.course_name";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new EnrolledCourseDto
                        {
                            CourseCode = StudentProfileRepository.SafeGetString(reader["course_code"]),
                            CourseName = StudentProfileRepository.SafeGetString(reader["course_name"]),
                            Semester = StudentProfileRepository.SafeGetString(reader["semester"]),
                            Credit = StudentProfileRepository.SafeGetDecimal(reader["credit"]),
                            CreditObtained = StudentProfileRepository.SafeGetDecimal(reader["credit_obtained"]),
                            TotalScore = reader["total_score"] == DBNull.Value
                                ? (decimal?)null
                                : StudentProfileRepository.SafeGetDecimal(reader["total_score"]),
                            GradeLevel = StudentProfileRepository.SafeGetString(reader["grade_level"]),
                            Gpa = reader["gpa"] == DBNull.Value
                                ? (decimal?)null
                                : StudentProfileRepository.SafeGetDecimal(reader["gpa"]),
                            IsPassed = StudentProfileRepository.SafeGetDecimal(reader["credit_obtained"]) > 0
                        });
                    }
                }
            }

            return courses;
        }

        public GpaSummaryDto GetGpaSummary(string studentNo)
        {
            var summary = new GpaSummaryDto();

            const string overallSql = @"
                SELECT NVL(SUM(ss.credit_obtained), 0) AS total_credits,
                       NVL(
                           SUM(CASE WHEN ss.gpa IS NOT NULL THEN ss.gpa * c.credit ELSE 0 END)
                           / NULLIF(SUM(CASE WHEN ss.gpa IS NOT NULL THEN c.credit ELSE 0 END), 0),
                           0
                       ) AS avg_gpa,
                       COUNT(CASE WHEN ss.gpa IS NOT NULL THEN 1 END) AS total_courses
                  FROM student_score ss
                  JOIN teaching_class tc ON tc.class_id = ss.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                 WHERE ss.student_no = :studentNo";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                using (OracleCommand command = CreateCommand(connection, overallSql))
                {
                    command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            summary.TotalCreditsFinished = StudentProfileRepository.SafeGetDecimal(reader["total_credits"]);
                            summary.AvgGpa = Math.Round(StudentProfileRepository.SafeGetDecimal(reader["avg_gpa"]), 2);
                            summary.TotalCourses = StudentProfileRepository.SafeGetInt(reader["total_courses"]);
                        }
                    }
                }

                foreach (SemesterGpaItem item in GetSemesterGpas(connection, studentNo))
                {
                    summary.SemesterGpas.Add(item);
                    summary.CreditTrend.Add(new CreditTrendItem
                    {
                        Semester = item.Semester,
                        Credits = item.Credits
                    });
                }
            }

            return summary;
        }

        private static List<SemesterGpaItem> GetSemesterGpas(OracleConnection connection, string studentNo)
        {
            var items = new List<SemesterGpaItem>();

            const string semesterSql = @"
                SELECT s.semester,
                       NVL(SUM(ss.credit_obtained), 0) AS credits,
                       NVL(
                           SUM(CASE WHEN ss.gpa IS NOT NULL THEN ss.gpa * c.credit ELSE 0 END)
                           / NULLIF(SUM(CASE WHEN ss.gpa IS NOT NULL THEN c.credit ELSE 0 END), 0),
                           0
                       ) AS avg_gpa,
                       COUNT(CASE WHEN ss.gpa IS NOT NULL THEN 1 END) AS courses
                  FROM student_score ss
                  JOIN teaching_class tc ON tc.class_id = ss.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                 WHERE ss.student_no = :studentNo
                 GROUP BY s.semester
                 ORDER BY s.semester DESC";

            using (OracleCommand command = CreateCommand(connection, semesterSql))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new SemesterGpaItem
                        {
                            Semester = StudentProfileRepository.SafeGetString(reader["semester"]),
                            Credits = StudentProfileRepository.SafeGetDecimal(reader["credits"]),
                            AvgGpa = Math.Round(StudentProfileRepository.SafeGetDecimal(reader["avg_gpa"]), 2),
                            Courses = StudentProfileRepository.SafeGetInt(reader["courses"])
                        });
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
    }
}
