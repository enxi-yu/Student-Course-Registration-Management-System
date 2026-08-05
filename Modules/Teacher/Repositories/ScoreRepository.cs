using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using StudentCourse.Infrastructure;
using StudentCourse.Models;

namespace StudentCourse.Repositories
{
    public sealed class ScoreRepository
    {
        public IList<ScoreDto> GetScoreSheet(string teacherNo, int classId)
        {
            const string sql = @"
                SELECT s.student_no,
                       u.real_name AS student_name,
                       cs.class_id,
                       ss.total_score,
                       ss.grade_level,
                       ss.gpa,
                       ss.credit_obtained,
                       ss.update_remark,
                       TO_CHAR(ss.update_time, 'YYYY-MM-DD HH24:MI:SS') AS update_time
                  FROM course_select cs
                  JOIN teaching_class tc ON tc.class_id = cs.class_id
                  JOIN section sec ON sec.section_id = tc.section_id
                  JOIN course c ON c.course_id = sec.course_id
                  JOIN student s ON s.student_no = cs.student_no
                  JOIN ""user"" u ON u.user_id = s.user_id
                  LEFT JOIN student_score ss ON ss.class_id = cs.class_id
                                           AND ss.student_no = cs.student_no
                 WHERE tc.teacher_no = :teacherNo
                   AND cs.class_id = :classId
                 ORDER BY s.student_no";

            List<ScoreDto> rows = new List<ScoreDto>();

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = teacherNo;
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(MapScore(reader));
                    }
                }
            }

            return rows;
        }

        public bool StudentSelectedClass(OracleConnection connection, OracleTransaction transaction, int classId, string studentNo)
        {
            const string sql = @"
                SELECT COUNT(*)
                  FROM course_select
                 WHERE class_id = :classId
                   AND student_no = :studentNo";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Transaction = transaction;
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public ScoreDto SaveScore(
            OracleConnection connection,
            OracleTransaction transaction,
            int classId,
            string studentNo,
            decimal totalScore,
            string gradeLevel,
            decimal gpa,
            decimal creditObtained,
            string updateRemark,
            bool requireRemarkForUpdate)
        {
            string scoreId = FindScoreId(connection, transaction, classId, studentNo);

            if (string.IsNullOrWhiteSpace(scoreId))
            {
                InsertScore(connection, transaction, classId, studentNo, totalScore, gradeLevel, gpa, creditObtained, updateRemark);
            }
            else
            {
                UpdateScore(connection, transaction, scoreId, totalScore, gradeLevel, gpa, creditObtained, updateRemark);
            }

            ScoreDto? saved = GetScore(connection, transaction, classId, studentNo);
            if (saved == null)
            {
                throw new InvalidOperationException("成绩保存失败，请刷新后重试");
            }

            return saved;
        }

        public ScoreDto? GetScore(OracleConnection connection, OracleTransaction transaction, int classId, string studentNo)
        {
            const string sql = @"
                SELECT s.student_no,
                       u.real_name AS student_name,
                       cs.class_id,
                       ss.total_score,
                       ss.grade_level,
                       ss.gpa,
                       ss.credit_obtained,
                       ss.update_remark,
                       TO_CHAR(ss.update_time, 'YYYY-MM-DD HH24:MI:SS') AS update_time
                  FROM course_select cs
                  JOIN student s ON s.student_no = cs.student_no
                  JOIN ""user"" u ON u.user_id = s.user_id
                  LEFT JOIN student_score ss ON ss.class_id = cs.class_id
                                           AND ss.student_no = cs.student_no
                 WHERE cs.class_id = :classId
                   AND cs.student_no = :studentNo";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Transaction = transaction;
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return MapScore(reader);
                }
            }
        }

        private static string FindScoreId(OracleConnection connection, OracleTransaction transaction, int classId, string studentNo)
        {
            const string sql = @"
                SELECT score_id
                  FROM student_score
                 WHERE class_id = :classId
                   AND student_no = :studentNo
                 FETCH FIRST 1 ROW ONLY";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Transaction = transaction;
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                object? result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? string.Empty : Convert.ToString(result) ?? string.Empty;
            }
        }

        private static void InsertScore(
            OracleConnection connection,
            OracleTransaction transaction,
            int classId,
            string studentNo,
            decimal totalScore,
            string gradeLevel,
            decimal gpa,
            decimal creditObtained,
            string updateRemark)
        {
            const string sql = @"
                INSERT INTO student_score (
                    score_id,
                    student_no,
                    class_id,
                    total_score,
                    grade_level,
                    gpa,
                    credit_obtained,
                    entry_time,
                    update_remark,
                    update_time
                ) VALUES (
                    :scoreId,
                    :studentNo,
                    :classId,
                    :totalScore,
                    :gradeLevel,
                    :gpa,
                    :creditObtained,
                    SYSDATE,
                    :updateRemark,
                    SYSDATE
                )";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Transaction = transaction;
                command.Parameters.Add("scoreId", OracleDbType.Varchar2).Value = Guid.NewGuid().ToString("N");
                AddScoreParameters(command, classId, studentNo, totalScore, gradeLevel, gpa, creditObtained, updateRemark);
                command.ExecuteNonQuery();
            }
        }

        private static void UpdateScore(
            OracleConnection connection,
            OracleTransaction transaction,
            string scoreId,
            decimal totalScore,
            string gradeLevel,
            decimal gpa,
            decimal creditObtained,
            string updateRemark)
        {
            const string sql = @"
                UPDATE student_score
                   SET total_score = :totalScore,
                       grade_level = :gradeLevel,
                       gpa = :gpa,
                       credit_obtained = :creditObtained,
                       update_remark = :updateRemark,
                       update_time = SYSDATE
                 WHERE score_id = :scoreId";

            using (OracleCommand command = TeachingClassRepository.CreateCommand(connection, sql))
            {
                command.Transaction = transaction;
                command.Parameters.Add("totalScore", OracleDbType.Decimal).Value = totalScore;
                command.Parameters.Add("gradeLevel", OracleDbType.Varchar2).Value = gradeLevel;
                command.Parameters.Add("gpa", OracleDbType.Decimal).Value = gpa;
                command.Parameters.Add("creditObtained", OracleDbType.Decimal).Value = creditObtained;
                command.Parameters.Add("updateRemark", OracleDbType.Varchar2).Value =
                    string.IsNullOrWhiteSpace(updateRemark) ? (object)DBNull.Value : updateRemark.Trim();
                command.Parameters.Add("scoreId", OracleDbType.Varchar2).Value = scoreId;
                command.ExecuteNonQuery();
            }
        }

        private static void AddScoreParameters(
            OracleCommand command,
            int classId,
            string studentNo,
            decimal totalScore,
            string gradeLevel,
            decimal gpa,
            decimal creditObtained,
            string updateRemark)
        {
            command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
            command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
            command.Parameters.Add("totalScore", OracleDbType.Decimal).Value = totalScore;
            command.Parameters.Add("gradeLevel", OracleDbType.Varchar2).Value = gradeLevel;
            command.Parameters.Add("gpa", OracleDbType.Decimal).Value = gpa;
            command.Parameters.Add("creditObtained", OracleDbType.Decimal).Value = creditObtained;
            command.Parameters.Add("updateRemark", OracleDbType.Varchar2).Value =
                string.IsNullOrWhiteSpace(updateRemark) ? (object)DBNull.Value : updateRemark.Trim();
        }

        private static ScoreDto MapScore(OracleDataReader reader)
        {
            return new ScoreDto
            {
                StudentNo = Convert.ToString(reader["student_no"]) ?? string.Empty,
                StudentName = Convert.ToString(reader["student_name"]) ?? string.Empty,
                ClassId = ToInt32(reader["class_id"]),
                TotalScore = ToNullableDecimal(reader["total_score"]),
                GradeLevel = Convert.ToString(reader["grade_level"]) ?? string.Empty,
                Gpa = ToNullableDecimal(reader["gpa"]),
                CreditObtained = ToDecimal(reader["credit_obtained"]),
                UpdateRemark = Convert.ToString(reader["update_remark"]) ?? string.Empty,
                UpdateTime = Convert.ToString(reader["update_time"]) ?? string.Empty
            };
        }

        private static int ToInt32(object value)
        {
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private static decimal ToDecimal(object value)
        {
            return value == null || value == DBNull.Value ? 0m : Convert.ToDecimal(value);
        }

        private static decimal? ToNullableDecimal(object value)
        {
            return value == null || value == DBNull.Value ? (decimal?)null : Convert.ToDecimal(value);
        }
    }
}
