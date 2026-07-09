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
                SELECT c.course_name,
                       s.semester,
                       c.credit,
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
                            CourseName = StudentProfileRepository.SafeGetString(reader["course_name"]),
                            Semester = StudentProfileRepository.SafeGetString(reader["semester"]),
                            Credit = StudentProfileRepository.SafeGetDecimal(reader["credit"]),
                            TotalScore = reader["total_score"] == DBNull.Value
                                ? (decimal?)null
                                : StudentProfileRepository.SafeGetDecimal(reader["total_score"]),
                            GradeLevel = StudentProfileRepository.SafeGetString(reader["grade_level"]),
                            Gpa = reader["gpa"] == DBNull.Value
                                ? (decimal?)null
                                : StudentProfileRepository.SafeGetDecimal(reader["gpa"])
                        });
                    }
                }
            }

            return courses;
        }

        private static OracleCommand CreateCommand(OracleConnection connection, string sql)
        {
            OracleCommand command = new OracleCommand(sql, connection);
            command.BindByName = true;
            return command;
        }
    }
}
