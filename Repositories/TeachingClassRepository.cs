using System;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using StudentCourse.Infrastructure;

namespace StudentCourse.Repositories
{
    public sealed class TeachingClassRepository
    {
        public bool TeacherOwnsClass(string teacherNo, int classId)
        {
            const string sql = @"
                SELECT COUNT(*)
                  FROM teaching_class
                 WHERE teacher_no = :teacherNo
                   AND class_id = :classId";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public string GetClassName(string teacherNo, int classId)
        {
            const string sql = @"
                SELECT class_name
                  FROM teaching_class
                 WHERE teacher_no = :teacherNo
                   AND class_id = :classId";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? string.Empty : Convert.ToString(result);
            }
        }

        public decimal GetCourseCredit(string teacherNo, int classId)
        {
            const string sql = @"
                SELECT c.credit
                  FROM teaching_class tc
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                 WHERE tc.teacher_no = :teacherNo
                   AND tc.class_id = :classId";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
            }
        }

        internal static OracleCommand CreateCommand(OracleConnection connection, string sql)
        {
            OracleCommand command = new OracleCommand(sql, connection);
            command.BindByName = true;
            return command;
        }
    }
}
