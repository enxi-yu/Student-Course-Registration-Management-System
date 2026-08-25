using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using StudentCourse.Infrastructure;
using StudentCourse.Models;

namespace StudentCourse.Repositories
{
    /// <summary>
    /// 管理员代学生选课数据访问，严格校验重复/容量/时间冲突
    /// 学期默认：2025-2026-2
    /// </summary>
    public sealed class AdminSelectionRepository
    {
        private const string DefaultSemester = "2025-2026-2";

        // 可选教学班列表
        public IList<AdminSelectionClassDto> GetSelectableClasses(string? semester, string? keyword)
        {
            string sem = string.IsNullOrWhiteSpace(semester) ? DefaultSemester : semester.Trim();
            //基础查询，LISTAGG合并时间段
            string sql = @"
                SELECT tc.class_id, c.course_id, c.course_name, c.course_type, c.credit,
                       s.semester, tc.teacher_no, u.real_name AS teacher_name,
                       tc.capacity, tc.selected_count,
                       (SELECT LISTAGG(ct.weekday || '-' || ct.start_period || '-' || ct.end_period, '; ')
                               WITHIN GROUP (ORDER BY ct.weekday)
                          FROM course_time ct WHERE ct.class_id = tc.class_id) AS schedule_summary
                  FROM teaching_class tc
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  LEFT JOIN teacher t ON t.teacher_no = tc.teacher_no
                  LEFT JOIN ""user"" u ON u.user_id = t.user_id
                 WHERE s.semester = :semester";
            //拼接附加条件
            if (!string.IsNullOrWhiteSpace(keyword))
                sql += " AND (c.course_name || ' ' || tc.teacher_no || ' ' || NVL(u.real_name, ' ')) LIKE :keyword";
            sql += " ORDER BY c.course_name, tc.class_id FETCH FIRST 200 ROWS ONLY";

            List<AdminSelectionClassDto> rows = new List<AdminSelectionClassDto>();

            using OracleConnection conn = DbConnectionFactory.OpenConnection();
            using OracleCommand cmd = CreateCommand(conn, sql);
            cmd.Parameters.Add("semester", OracleDbType.Varchar2).Value = sem;
            if (!string.IsNullOrWhiteSpace(keyword))
                cmd.Parameters.Add("keyword", OracleDbType.Varchar2).Value = "%" + keyword.Trim() + "%";

            using OracleDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) rows.Add(new AdminSelectionClassDto
            {
                ClassId = reader["class_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["class_id"]),
                CourseId = reader["course_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["course_id"]),
                CourseName = reader["course_name"]?.ToString() ?? "",
                CourseType = reader["course_type"]?.ToString() ?? "",
                Credit = reader["credit"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["credit"]),
                Semester = reader["semester"]?.ToString() ?? "",
                TeacherNo = reader["teacher_no"]?.ToString() ?? "",
                TeacherName = reader["teacher_name"]?.ToString() ?? "",
                Capacity = reader["capacity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["capacity"]),
                SelectedCount = reader["selected_count"] == DBNull.Value ? 0 : Convert.ToInt32(reader["selected_count"]),
                ScheduleSummary = reader["schedule_summary"]?.ToString() ?? ""
            });
            return rows;
        }

        // 某学生已选课程
        public IList<AdminEnrollmentDto> GetStudentEnrollments(string studentNo, string? semester)
        {
            string sem = string.IsNullOrWhiteSpace(semester) ? DefaultSemester : semester.Trim();

            const string sql = @"
                SELECT cs.select_id, tc.class_id, c.course_name, c.course_type, c.credit,
                       s.semester, u.real_name AS teacher_name, cs.batch_id, b.batch_name,
                       (SELECT LISTAGG(ct.weekday || '-' || ct.start_period || '-' || ct.end_period, '; ')
                               WITHIN GROUP (ORDER BY ct.weekday)
                          FROM course_time ct WHERE ct.class_id = tc.class_id) AS schedule_summary
                  FROM course_select cs
                  JOIN teaching_class tc ON tc.class_id = cs.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  LEFT JOIN teacher t ON t.teacher_no = tc.teacher_no
                  LEFT JOIN ""user"" u ON u.user_id = t.user_id
                  LEFT JOIN selection_batch b ON b.batch_id = cs.batch_id
                 WHERE cs.student_no = :studentNo AND s.semester = :semester
                 ORDER BY schedule_summary";

            List<AdminEnrollmentDto> rows = new List<AdminEnrollmentDto>();

            using OracleConnection conn = DbConnectionFactory.OpenConnection();
            using OracleCommand cmd = CreateCommand(conn, sql);
            cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
            cmd.Parameters.Add("semester", OracleDbType.Varchar2).Value = sem;
            using OracleDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                object batchValue = reader["batch_id"];
                rows.Add(new AdminEnrollmentDto
                {
                    SelectId = Convert.ToInt32(reader["select_id"]),
                    ClassId = reader["class_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["class_id"]),
                    CourseName = reader["course_name"]?.ToString() ?? "",
                    CourseType = reader["course_type"]?.ToString() ?? "",
                    Credit = reader["credit"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["credit"]),
                    Semester = reader["semester"]?.ToString() ?? "",
                    TeacherName = reader["teacher_name"]?.ToString() ?? "",
                    BatchId = batchValue == DBNull.Value ? null : Convert.ToInt32(batchValue),
                    BatchName = reader["batch_name"]?.ToString() ?? "",
                    ScheduleSummary = reader["schedule_summary"]?.ToString() ?? ""
                });
            }
            return rows;
        }

        // 代选课
        // force=false：容量严格校验，满了返回 RequireCapacityConfirm=true 供前端二次确认
        // force=true：容量已满仍超员插入，并把容量 +1 更新
        public AdminSelectionResultDto SelectForStudent(string studentNo, int classId, bool force)
        {
            AdminSelectionResultDto result = new AdminSelectionResultDto { Success = false };

            using OracleConnection conn = DbConnectionFactory.OpenConnection();
            using OracleTransaction tx = conn.BeginTransaction();
            try
            {
                // 重复选课
                const string checkSql = "SELECT COUNT(*) FROM course_select WHERE student_no = :studentNo AND class_id = :classId";
                using (OracleCommand cmd = CreateCommand(conn, checkSql, tx))
                {
                    AddParams(cmd, ("studentNo", OracleDbType.Varchar2, studentNo), ("classId", OracleDbType.Int32, classId));
                    int dup = Convert.ToInt32(cmd.ExecuteScalar());
                    if (dup > 0) { result.Message = "该学生已选择此课程，不能重复选课。"; return result; }
                }

                // 时间冲突检测
                result.ConflictCourses = CheckTimeConflict(conn, studentNo, classId, tx);
                if (result.ConflictCourses.Count > 0) { result.Message = "选课失败：与已选课程存在时间冲突。"; return result; }

                // 容量检测 + 超员扩容（force=true 时）
                int selected, capacity;
                const string capSql = "SELECT capacity, selected_count FROM teaching_class WHERE class_id = :classId";
                using (OracleCommand cmd = CreateCommand(conn, capSql, tx))
                {
                    cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    using OracleDataReader reader = cmd.ExecuteReader();
                    if (!reader.Read()) { result.Message = "教学班不存在。"; return result; }
                    selected = reader["selected_count"] == DBNull.Value ? 0 : Convert.ToInt32(reader["selected_count"]);
                    capacity = reader["capacity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["capacity"]);
                }

                if (selected >= capacity)
                {
                    if (!force)
                    {
                        // 返回二次确认标记 + 当前人数，让前端弹"容量已满，是否扩班+1后继续"
                        result.RequireCapacityConfirm = true;
                        result.CurrentSelected = selected;
                        result.CurrentCapacity = capacity;
                        result.Message = $"该教学班已满（{selected}/{capacity}）。是否确认超员扩班后继续？";
                        return result;
                    }
                    // force=true：容量 +1 以便超员放入
                    const string expandSql = "UPDATE teaching_class SET capacity = capacity + 1 WHERE class_id = :classId";
                    using (OracleCommand cmd = CreateCommand(conn, expandSql, tx))
                    {
                        cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                        cmd.ExecuteNonQuery();
                    }
                    capacity += 1;
                }

                // 取当前开放批次，可空
                int? batchId = GetActiveBatchId(conn, tx);

                // 写入选课记录
                const string insertSql = @"INSERT INTO course_select (select_id, class_id, batch_id, student_no)
                    VALUES (course_select_id_seq.NEXTVAL, :classId, :batchId, :studentNo)";
                using (OracleCommand cmd = CreateCommand(conn, insertSql, tx))
                {
                    cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    cmd.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId.HasValue ? (object)batchId.Value : DBNull.Value;
                    cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                    cmd.ExecuteNonQuery();
                }

                // 回填教学班已选人数
                UpdateSelectedCount(conn, classId, tx);

                tx.Commit();
                result.Success = true;
                if (force)
                    result.Message = batchId.HasValue
                        ? "代选课成功（已超员扩容 +1），已记入选课批次。"
                        : "代选课成功（已超员扩容 +1），当前无开放选课批次，batch_id 置空。";
                else
                    result.Message = batchId.HasValue
                        ? "代选课成功，已记入选课批次。"
                        : "代选课成功，当前无开放选课批次，batch_id 置空。";
                return result;
            }
            catch
            {
                tx.Rollback();
                result.Message = "代选课失败，请稍后重试。";
                return result;
            }
        }

        // 代退课
        public AdminSelectionResultDto DropForStudent(string studentNo, int classId)
        {
            AdminSelectionResultDto result = new AdminSelectionResultDto { Success = false };

            using OracleConnection conn = DbConnectionFactory.OpenConnection();
            using OracleTransaction tx = conn.BeginTransaction();
            try
            {
                const string deleteSql = "DELETE FROM course_select WHERE student_no = :studentNo AND class_id = :classId";
                using (OracleCommand cmd = CreateCommand(conn, deleteSql, tx))
                {
                    AddParams(cmd, ("studentNo", OracleDbType.Varchar2, studentNo), ("classId", OracleDbType.Int32, classId));
                    if (cmd.ExecuteNonQuery() == 0) { result.Message = "未找到该学生的选课记录，无法退课。"; return result; }
                }

                UpdateSelectedCount(conn, classId, tx);
                tx.Commit();
                result.Success = true;
                result.Message = "代退课成功。";
                return result;
            }
            catch
            {
                tx.Rollback();
                result.Message = "代退课失败，请稍后重试。";
                return result;
            }
        }

        private List<string> CheckTimeConflict(OracleConnection conn, string studentNo, int classId, OracleTransaction tx)
        {
            List<string> conflicts = new List<string>();
            const string sql = @"
                SELECT DISTINCT c2.course_name
                  FROM course_time ct1
                  JOIN course_time ct2
                    AND ct2.weekday = ct1.weekday AND ct2.start_period <= ct1.end_period
                    AND ct2.end_period >= ct1.start_period
                  JOIN course_select cs ON cs.class_id = ct2.class_id AND cs.student_no = :studentNo
                  JOIN teaching_class tc2 ON tc2.class_id = ct2.class_id
                  JOIN section s2 ON s2.section_id = tc2.section_id
                  JOIN course c2 ON c2.course_id = s2.course_id
                 WHERE ct1.class_id = :classId
                 ORDER BY c2.course_name";

            using OracleCommand cmd = CreateCommand(conn, sql, tx);
            AddParams(cmd, ("studentNo", OracleDbType.Varchar2, studentNo), ("classId", OracleDbType.Int32, classId));
            using OracleDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                conflicts.Add(reader["course_name"]?.ToString() ?? "");
            }
            return conflicts;
        }

        // 用 course_select 实际计数回填 teaching_class.selected_count，保证人数准确
        private static void UpdateSelectedCount(OracleConnection conn, int classId, OracleTransaction tx)
        {
            const string sql = @"UPDATE teaching_class
                   SET selected_count = (SELECT COUNT(*) FROM course_select WHERE class_id = :classId)
                 WHERE class_id = :classId";
            using OracleCommand cmd = CreateCommand(conn, sql, tx);
            cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
            cmd.ExecuteNonQuery();
        }

        // 取当前开放批次：status=1 且当前时间在 [start_time, end_time] 之间，无则返回 null
        private static int? GetActiveBatchId(OracleConnection conn, OracleTransaction tx)
        {
            const string sql = @"SELECT batch_id FROM selection_batch
                    WHERE status = 1 AND start_time <= SYSDATE AND end_time >= SYSDATE";
            using OracleCommand cmd = CreateCommand(conn, sql, tx);
            object value = cmd.ExecuteScalar();
            return value == DBNull.Value ? null : Convert.ToInt32(value);
        }

        private static OracleCommand CreateCommand(OracleConnection conn, string sql, OracleTransaction? tx = null)
        {
            OracleCommand command = new OracleCommand(sql, conn);
            command.BindByName = true;  // 按名字绑定
            command.Transaction = tx;
            return command;
        }

        // 批量加入命名参数
        private static void AddParams(OracleCommand cmd, params (string Name, OracleDbType Type, object Value)[] args)
        {
            foreach (var a in args) cmd.Parameters.Add(a.Name, a.Type).Value = a.Value ?? DBNull.Value;
        }
    }
}
