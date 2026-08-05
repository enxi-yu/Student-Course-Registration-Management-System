using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using StudentCourse.Infrastructure;
using StudentCourse.Models;

namespace StudentCourse.Repositories
{
    public sealed class StudentListRepository
    {
        public IList<StudentListDto> GetClassStudents(string teacherNo, int classId)
        {
            const string sql = @"
                SELECT s.student_no,
                       u.real_name AS student_name,
                       s.major,
                       s.grade
                  FROM course_select cs
                  JOIN teaching_class tc ON tc.class_id = cs.class_id
                  JOIN student s ON s.student_no = cs.student_no
                  JOIN ""user"" u ON u.user_id = s.user_id
                 WHERE tc.teacher_no = :teacherNo
                   AND cs.class_id = :classId
                 ORDER BY s.student_no";

            List<StudentListDto> students = new List<StudentListDto>();

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add(new StudentListDto
                        {
                            StudentNo = Convert.ToString(reader["student_no"]),
                            StudentName = Convert.ToString(reader["student_name"]),
                            Major = Convert.ToString(reader["major"]),
                            Grade = Convert.ToString(reader["grade"]),
                            SelectTime = string.Empty
                        });
                    }
                }
            }

            return students;
        }
    }
}
