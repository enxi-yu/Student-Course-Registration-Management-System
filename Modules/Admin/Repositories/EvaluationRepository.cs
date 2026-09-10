using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using StudentCourse.Infrastructure;
using StudentCourse.Models;

namespace StudentCourse.Repositories
{
    public sealed class EvaluationRepository
    {
        public IList<AdminEvaluationDto> Search(string? keyword, string? semester, string? department = null)
        {
            const string sql = @"SELECT ce.student_no, tc.class_id, tc.class_name, c.course_name,
                NVL(u.real_name, tc.teacher_no) teacher_name, s.semester, ce.d1_score, ce.d2_score,
                ce.d3_score, ce.d4_score, ce.eval_score, ce.eval_content,
                TO_CHAR(ce.eval_time,'YYYY-MM-DD HH24:MI') evaluation_time
                FROM course_evaluation ce JOIN teaching_class tc ON tc.class_id=ce.class_id
                JOIN section s ON s.section_id=tc.section_id JOIN course c ON c.course_id=s.course_id
                LEFT JOIN teacher t ON t.teacher_no=tc.teacher_no LEFT JOIN ""user"" u ON u.user_id=t.user_id
                WHERE (:keyword IS NULL OR UPPER(c.course_name) LIKE UPPER(:keywordLike) OR UPPER(tc.class_name) LIKE UPPER(:keywordLike) OR UPPER(NVL(u.real_name,tc.teacher_no)) LIKE UPPER(:keywordLike))
                AND (:semester IS NULL OR s.semester=:semester)
                AND (:department IS NULL OR UPPER(TRIM(c.department)) = UPPER(TRIM(:department)))
                ORDER BY ce.eval_time DESC";
            var rows = new List<AdminEvaluationDto>(); using OracleConnection connection=DbConnectionFactory.OpenConnection(); using OracleCommand command=new(sql,connection){BindByName=true};
            string key=(keyword??"").Trim(); command.Parameters.Add("keyword",OracleDbType.Varchar2).Value=key.Length==0?(object)DBNull.Value:key; command.Parameters.Add("keywordLike",OracleDbType.Varchar2).Value=$"%{key}%"; command.Parameters.Add("semester",OracleDbType.Varchar2).Value=string.IsNullOrWhiteSpace(semester)?(object)DBNull.Value:semester.Trim(); command.Parameters.Add("department",OracleDbType.Varchar2).Value=string.IsNullOrWhiteSpace(department)?(object)DBNull.Value:department.Trim();
            using OracleDataReader reader=command.ExecuteReader(); while(reader.Read()) rows.Add(new AdminEvaluationDto { StudentNo=S(reader["student_no"]),ClassId=I(reader["class_id"]),ClassName=S(reader["class_name"]),CourseName=S(reader["course_name"]),TeacherName=S(reader["teacher_name"]),Semester=S(reader["semester"]),D1Score=I(reader["d1_score"]),D2Score=I(reader["d2_score"]),D3Score=I(reader["d3_score"]),D4Score=I(reader["d4_score"]),EvalScore=D(reader["eval_score"]),Comment=S(reader["eval_content"]),EvaluationTime=S(reader["evaluation_time"]) }); return rows;
        }
        private static string S(object value) { if(value==DBNull.Value)return ""; if(value is OracleClob c)return c.Value; return Convert.ToString(value)??""; }
        private static int I(object value)=>value==DBNull.Value?0:Convert.ToInt32(value);
        private static decimal D(object value)=>value==DBNull.Value?0:Convert.ToDecimal(value);
    }
}
