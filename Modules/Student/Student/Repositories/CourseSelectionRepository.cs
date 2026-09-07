using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using StudentCourse.Infrastructure;
using StudentCourse.Student.Models;

namespace StudentCourse.Student.Repositories
{
    public sealed class CourseSelectionRepository
    {
        public List<StudentSelectionBatchDto> GetSelectionBatches(string studentNo)
        {
            const string sql = @"
                SELECT b.batch_id, b.batch_name,
                       TO_CHAR(b.start_time,'YYYY-MM-DD HH24:MI') start_time,
                       TO_CHAR(b.end_time,'YYYY-MM-DD HH24:MI') end_time,
                       CASE WHEN SYSDATE < b.start_time THEN 0 WHEN SYSDATE > b.end_time THEN 2 ELSE 1 END actual_status,
                       COUNT(DISTINCT bc.class_id) course_count
                  FROM selection_batch b
                  JOIN batch_class bc ON bc.batch_id=b.batch_id AND bc.enabled=1
                  JOIN student st ON st.student_no=:studentNo
                 WHERE b.end_time >= SYSDATE
                   AND (NOT EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.major IS NOT NULL)
                        OR EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.major=st.major))
                   AND (NOT EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.grade IS NOT NULL)
                        OR EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.grade=st.grade))
                 GROUP BY b.batch_id,b.batch_name,b.start_time,b.end_time
                 ORDER BY b.start_time DESC";
            var rows=new List<StudentSelectionBatchDto>();
            using OracleConnection connection=DbConnectionFactory.OpenConnection(); using OracleCommand command=CreateCommand(connection,sql);
            command.Parameters.Add("studentNo",OracleDbType.Varchar2).Value=studentNo; using OracleDataReader reader=command.ExecuteReader();
            while(reader.Read()) { int status=StudentProfileRepository.SafeGetInt(reader["actual_status"]); rows.Add(new StudentSelectionBatchDto {
                BatchId=StudentProfileRepository.SafeGetInt(reader["batch_id"]),BatchName=StudentProfileRepository.SafeGetString(reader["batch_name"]),
                StartTime=StudentProfileRepository.SafeGetString(reader["start_time"]),EndTime=StudentProfileRepository.SafeGetString(reader["end_time"]),Status=status,
                StatusText=status==0?"未开始":status==1?"进行中":"已结束",CourseCount=StudentProfileRepository.SafeGetInt(reader["course_count"]) }); }
            return rows;
        }

        public List<CourseSelectionDto> GetAvailableCourses(string studentNo, int batchId)
        {
            var courses = new List<CourseSelectionDto>();

            const string sql = @"
                SELECT tc.class_id,
                       c.course_id,
                       c.course_name,
                       tc.class_name,
                       c.course_type,
                       u.real_name AS teacher_name,
                       s.semester,
                       c.credit,
                       tc.capacity,
                       tc.selected_count,
                       CASE WHEN cs2.student_no IS NOT NULL THEN 1 ELSE 0 END AS is_selected,
                       (SELECT LISTAGG(ct2.weekday || '-' || ct2.start_period || '-' || ct2.end_period, '; ')
                              WITHIN GROUP (ORDER BY ct2.weekday)
                          FROM course_time ct2 WHERE ct2.class_id = tc.class_id) AS schedule_summary
                  FROM teaching_class tc
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  JOIN teacher t ON t.teacher_no = tc.teacher_no
                  JOIN ""user"" u ON u.user_id = t.user_id
                  LEFT JOIN course_select cs2 ON cs2.class_id = tc.class_id AND cs2.student_no = :studentNo
                  JOIN batch_class bc ON bc.class_id=tc.class_id AND bc.batch_id=:batchId AND bc.enabled=1
                  JOIN student st ON st.student_no=:studentNo
                 WHERE (NOT EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.major IS NOT NULL)
                        OR EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.major=st.major))
                   AND (NOT EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.grade IS NOT NULL)
                        OR EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.grade=st.grade))
                 ORDER BY c.course_name, tc.class_name";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                const string batchSql = "SELECT COUNT(*) FROM selection_batch WHERE batch_id=:batchId AND start_time<=SYSDATE AND end_time>=SYSDATE";
                using (OracleCommand batchCommand = CreateCommand(connection, batchSql))
                {
                    batchCommand.Parameters.Add("batchId",OracleDbType.Int32).Value=batchId;
                    if (Convert.ToInt32(batchCommand.ExecuteScalar()) == 0) return courses;
                }

                using OracleCommand command = CreateCommand(connection, sql);
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                command.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new CourseSelectionDto
                        {
                            ClassId = StudentProfileRepository.SafeGetInt(reader["class_id"]),
                            CourseId = StudentProfileRepository.SafeGetInt(reader["course_id"]),
                            CourseName = StudentProfileRepository.SafeGetString(reader["course_name"]),
                            ClassName = StudentProfileRepository.SafeGetString(reader["class_name"]),
                            CourseType = StudentProfileRepository.SafeGetString(reader["course_type"]),
                            TeacherName = StudentProfileRepository.SafeGetString(reader["teacher_name"]),
                            Semester = StudentProfileRepository.SafeGetString(reader["semester"]),
                            Credit = StudentProfileRepository.SafeGetDecimal(reader["credit"]),
                            Capacity = StudentProfileRepository.SafeGetInt(reader["capacity"]),
                            SelectedCount = StudentProfileRepository.SafeGetInt(reader["selected_count"]),
                            IsSelected = StudentProfileRepository.SafeGetInt(reader["is_selected"]) == 1,
                            ScheduleSummary = StudentProfileRepository.SafeGetString(reader["schedule_summary"])
                        });
                    }
                }
            }

            return courses;
        }

        public CourseDetailDto? GetCourseDetail(string studentNo, int classId)
        {
            CourseDetailDto? dto = null;
            const string sql = @"
                SELECT tc.class_id,
                       c.course_id,
                       c.course_name,
                       tc.class_name,
                       c.course_type,
                       c.credit,
                       c.total_hours,
                       u.real_name AS teacher_name,
                       c.department,
                       tc.capacity,
                       tc.selected_count,
                       c.course_desc
                  FROM teaching_class tc
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  JOIN teacher t ON t.teacher_no = tc.teacher_no
                  JOIN ""user"" u ON u.user_id = t.user_id
                 WHERE tc.class_id = :classId";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                using (OracleCommand command = CreateCommand(connection, sql))
                {
                    command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return null;

                        dto = new CourseDetailDto
                        {
                            ClassId = StudentProfileRepository.SafeGetInt(reader["class_id"]),
                            CourseId = StudentProfileRepository.SafeGetInt(reader["course_id"]),
                            CourseName = StudentProfileRepository.SafeGetString(reader["course_name"]),
                            ClassName = StudentProfileRepository.SafeGetString(reader["class_name"]),
                            CourseType = StudentProfileRepository.SafeGetString(reader["course_type"]),
                            Credit = StudentProfileRepository.SafeGetDecimal(reader["credit"]),
                            TotalHours = StudentProfileRepository.SafeGetInt(reader["total_hours"]),
                            TeacherName = StudentProfileRepository.SafeGetString(reader["teacher_name"]),
                            Department = StudentProfileRepository.SafeGetString(reader["department"]),
                            Capacity = StudentProfileRepository.SafeGetInt(reader["capacity"]),
                            SelectedCount = StudentProfileRepository.SafeGetInt(reader["selected_count"]),
                            Description = StudentProfileRepository.ReadClob(reader["course_desc"]),
                            Schedule = GetClassSchedule(classId)
                        };
                    }
                }

                dto.IsSelected = false;
                dto.CanDrop = false;

                const string selSql = @"
                    SELECT cs.batch_id
                      FROM course_select cs
                     WHERE cs.student_no = :studentNo AND cs.class_id = :classId";
                using (OracleCommand cmd = CreateCommand(connection, selSql))
                {
                    cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                    cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    object batchValue = cmd.ExecuteScalar();
                    if (batchValue != null && batchValue != DBNull.Value)
                    {
                        dto.IsSelected = true;
                        int batchId = Convert.ToInt32(batchValue);
                        const string batchSql = @"
                            SELECT COUNT(*) FROM selection_batch
                             WHERE batch_id = :batchId AND start_time <= SYSDATE AND end_time >= SYSDATE";
                        using (OracleCommand bcmd = CreateCommand(connection, batchSql))
                        {
                            bcmd.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;
                            if (Convert.ToInt32(bcmd.ExecuteScalar()) > 0) dto.CanDrop = true;
                        }
                    }
                }
            }

            return dto;
        }

        public SelectionResultDto SelectCourse(string studentNo, int classId, int batchId)
        {
            var result = new SelectionResultDto { Success = false };

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleTransaction tx = connection.BeginTransaction())
            {
                try
                {
                    const string releaseSql=@"SELECT COUNT(*) FROM selection_batch b JOIN batch_class bc ON bc.batch_id=b.batch_id AND bc.class_id=:classId AND bc.enabled=1 JOIN student st ON st.student_no=:studentNo WHERE b.batch_id=:batchId AND b.start_time<=SYSDATE AND b.end_time>=SYSDATE AND (NOT EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.major IS NOT NULL) OR EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.major=st.major)) AND (NOT EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.grade IS NOT NULL) OR EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.grade=st.grade))";
                    using OracleCommand release=CreateCommand(connection,releaseSql,tx); release.Parameters.Add("classId",OracleDbType.Int32).Value=classId; release.Parameters.Add("studentNo",OracleDbType.Varchar2).Value=studentNo; release.Parameters.Add("batchId",OracleDbType.Int32).Value=batchId;
                    if (Convert.ToInt32(release.ExecuteScalar())==0)
                    {
                        result.Message = "该课程未在此批次向你的专业和年级开放。";
                        return result;
                    }
                    // 检查重复选课
                    const string checkSql = @"
                        SELECT COUNT(*) FROM course_select
                         WHERE student_no = :studentNo AND class_id = :classId";

                    using (OracleCommand cmd = CreateCommand(connection, checkSql, tx))
                    {
                        cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                        cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                        {
                            result.Message = "已选择该课程，不能重复选课。";
                            return result;
                        }
                    }

                    // 检查容量（FOR UPDATE 防并发超选）
                    const string capSql = @"
                        SELECT capacity, selected_count FROM teaching_class
                         WHERE class_id = :classId FOR UPDATE";

                    using (OracleCommand cmd = CreateCommand(connection, capSql, tx))
                    {
                        cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int capacity = StudentProfileRepository.SafeGetInt(reader["capacity"]);
                                int selected = StudentProfileRepository.SafeGetInt(reader["selected_count"]);
                                if (selected >= capacity)
                                {
                                    result.Message = "该课程已满，无法选课。";
                                    return result;
                                }
                            }
                            else
                            {
                                result.Message = "教学班不存在。";
                                return result;
                            }
                        }
                    }

                    // 检查时间冲突
                    result.ConflictCourses = CheckTimeConflict(connection, studentNo, classId, tx);
                    if (result.ConflictCourses != null && result.ConflictCourses.Count > 0)
                    {
                        result.Message = "选课失败：与已选课程存在时间冲突。";
                        return result;
                    }

                    // 执行选课
                    const string insertSql = @"
                        INSERT INTO course_select (select_id, class_id, batch_id, student_no)
                        VALUES (course_select_id_seq.NEXTVAL, :classId, :batchId, :studentNo)";

                    using (OracleCommand cmd = CreateCommand(connection, insertSql, tx))
                    {
                        cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                        cmd.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;
                        cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                        cmd.ExecuteNonQuery();
                    }

                    UpdateSelectedCount(connection, classId, tx);

                    tx.Commit();
                    result.Success = true;
                    result.Message = "选课成功！";
                    return result;
                }
                catch
                {
                    result.Message = "选课失败，请重试。";
                    return result;
                }
            }
        }

        public SelectionResultDto DropCourse(string studentNo, int classId)
        {
            var result = new SelectionResultDto { Success = false };

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleTransaction tx = connection.BeginTransaction())
            {
                // 1. 查找真实选课记录并锁定，避免并发重复退课
                const string findSql = @"
                    SELECT cs.batch_id
                      FROM course_select cs
                     WHERE cs.student_no = :studentNo AND cs.class_id = :classId
                    FOR UPDATE";

                int batchId;
                using (OracleCommand cmd = CreateCommand(connection, findSql, tx))
                {
                    cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                    cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    object batchValue = cmd.ExecuteScalar();
                    if (batchValue == null || batchValue == DBNull.Value)
                    {
                        result.Message = "未找到该选课记录，无法退课。";
                        return result;
                    }
                    batchId = Convert.ToInt32(batchValue);
                }

                // 2. 校验该选课记录所属批次是否仍处于退课时间
                const string batchSql = @"
                    SELECT COUNT(*) FROM selection_batch
                     WHERE batch_id = :batchId AND start_time <= SYSDATE AND end_time >= SYSDATE";
                using (OracleCommand cmd = CreateCommand(connection, batchSql, tx))
                {
                    cmd.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        result.Message = "当前不在该课程所属选课批次的退课时间内。";
                        return result;
                    }
                }

                // 3. 删除选课记录
                const string deleteSql = @"
                    DELETE FROM course_select
                     WHERE student_no = :studentNo AND class_id = :classId";
                using (OracleCommand cmd = CreateCommand(connection, deleteSql, tx))
                {
                    cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                    cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    cmd.ExecuteNonQuery();
                }

                // 4. 更新人数
                UpdateSelectedCount(connection, classId, tx);

                tx.Commit();

                result.Success = true;
                result.Message = "退课成功。";
                return result;
            }
        }

        public SelectionResultDto SaveCourseSelection(string studentNo, int batchId, IList<int> desiredClassIds)
        {
            var result = new SelectionResultDto { Success = false };
            var desired = (desiredClassIds ?? new List<int>()).Distinct().ToList();

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleTransaction tx = connection.BeginTransaction())
            {
                try
                {
                    // 1. 校验批次开放
                    if (!IsBatchOpen(connection, tx, batchId))
                    {
                        result.Message = "当前选课批次不在开放时间内。";
                        return result;
                    }

                    // 2. 校验每个 classId 都属于该批次且符合专业/年级范围
                    foreach (var classId in desired)
                    {
                        if (!ClassInBatchScope(connection, tx, studentNo, batchId, classId))
                        {
                            result.Message = "提交的课程不属于当前选课批次或不符合选课范围。";
                            return result;
                        }
                    }

                    // 3. 读取当前批次真实选课，后端自行计算 diff
                    var currentBatchClassIds = GetBatchClassIds(connection, tx, studentNo, batchId);
                    var toDrop = currentBatchClassIds.Where(id => !desired.Contains(id)).ToList();
                    var toSelect = desired.Where(id => !currentBatchClassIds.Contains(id)).ToList();

                    // 4. 保存完成后学生最终全部课程（其他批次保留 + 本批次 desired）
                    var otherBatchClassIds = GetOtherBatchClassIds(connection, tx, studentNo, batchId);
                    var finalClassIds = otherBatchClassIds.Concat(desired).Distinct().ToList();

                    // 5. 同一门课程只能选择一个教学班
                    if (finalClassIds.Count > 0 && HasDuplicateCourse(connection, tx, finalClassIds))
                    {
                        result.Message = "同一门课程只能选择一个教学班。";
                        return result;
                    }

                    // 6. 最终课表时间冲突（仅同 semester 比较）
                    var conflictNames = FindTimeConflicts(connection, tx, finalClassIds);
                    if (conflictNames.Count > 0)
                    {
                        result.ConflictCourses = conflictNames;
                        result.Message = "选课失败：与已选课程存在时间冲突。";
                        return result;
                    }

                    // 7. 容量校验（仅新增教学班，锁定行）
                    foreach (var classId in toSelect)
                    {
                        if (!HasCapacity(connection, tx, classId))
                        {
                            result.Message = "该课程已满，无法选课。";
                            return result;
                        }
                    }

                    // 8. 删除退课
                    DeleteSelections(connection, tx, studentNo, batchId, toDrop);

                    // 9. 插入选课
                    InsertSelections(connection, tx, studentNo, batchId, toSelect);

                    // 10. 更新受影响教学班人数
                    foreach (var classId in toDrop.Concat(toSelect).Distinct())
                    {
                        UpdateSelectedCount(connection, classId, tx);
                    }

                    tx.Commit();
                    result.Success = true;
                    result.Message = "保存成功！";
                    return result;
                }
                catch
                {
                    result.Message = "保存失败，请重试。";
                    return result;
                }
            }
        }

        private static bool IsBatchOpen(OracleConnection connection, OracleTransaction tx, int batchId)
        {
            const string sql = "SELECT COUNT(*) FROM selection_batch WHERE batch_id=:batchId AND start_time<=SYSDATE AND end_time>=SYSDATE";
            using (OracleCommand cmd = CreateCommand(connection, sql, tx))
            {
                cmd.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static bool ClassInBatchScope(OracleConnection connection, OracleTransaction tx, string studentNo, int batchId, int classId)
        {
            const string sql = @"
                SELECT COUNT(*)
                  FROM batch_class bc
                  JOIN student st ON st.student_no = :studentNo
                 WHERE bc.batch_id = :batchId AND bc.class_id = :classId AND bc.enabled = 1
                   AND (NOT EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.major IS NOT NULL)
                        OR EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.major=st.major))
                   AND (NOT EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.grade IS NOT NULL)
                        OR EXISTS (SELECT 1 FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.grade=st.grade))";
            using (OracleCommand cmd = CreateCommand(connection, sql, tx))
            {
                cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                cmd.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;
                cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static List<int> GetBatchClassIds(OracleConnection connection, OracleTransaction tx, string studentNo, int batchId)
        {
            var ids = new List<int>();
            const string sql = "SELECT class_id FROM course_select WHERE student_no=:studentNo AND batch_id=:batchId";
            using (OracleCommand cmd = CreateCommand(connection, sql, tx))
            {
                cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                cmd.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) ids.Add(StudentProfileRepository.SafeGetInt(reader["class_id"]));
                }
            }
            return ids;
        }

        private static List<int> GetOtherBatchClassIds(OracleConnection connection, OracleTransaction tx, string studentNo, int batchId)
        {
            var ids = new List<int>();
            const string sql = "SELECT class_id FROM course_select WHERE student_no=:studentNo AND batch_id<>:batchId";
            using (OracleCommand cmd = CreateCommand(connection, sql, tx))
            {
                cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                cmd.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) ids.Add(StudentProfileRepository.SafeGetInt(reader["class_id"]));
                }
            }
            return ids;
        }

        private static bool HasDuplicateCourse(OracleConnection connection, OracleTransaction tx, IList<int> classIds)
        {
            const string sql = @"
                SELECT COUNT(*) FROM (
                    SELECT c.course_id
                      FROM teaching_class tc
                      JOIN section s ON s.section_id = tc.section_id
                      JOIN course c ON c.course_id = s.course_id
                     WHERE tc.class_id IN {IN}
                     GROUP BY c.course_id
                    HAVING COUNT(DISTINCT tc.class_id) > 1
                )";
            using (OracleCommand cmd = CreateCommand(connection, "", tx))
            {
                string inClause = BuildInClause(cmd, "c", classIds);
                cmd.CommandText = sql.Replace("{IN}", inClause);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static List<string> FindTimeConflicts(OracleConnection connection, OracleTransaction tx, IList<int> classIds)
        {
            var conflicts = new List<string>();
            if (classIds.Count == 0) return conflicts;

            const string sql = @"
                SELECT tc.class_id, s.semester, c.course_name, ct.weekday, ct.start_period, ct.end_period, ct.week_range
                  FROM course_time ct
                  JOIN teaching_class tc ON tc.class_id = ct.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                 WHERE ct.class_id IN {IN}
                 ORDER BY tc.class_id, ct.weekday, ct.start_period";

            var slots = new List<ScheduleItemDto>();
            using (OracleCommand cmd = CreateCommand(connection, "", tx))
            {
                string inClause = BuildInClause(cmd, "f", classIds);
                cmd.CommandText = sql.Replace("{IN}", inClause);
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        slots.Add(new ScheduleItemDto
                        {
                            ClassId = StudentProfileRepository.SafeGetInt(reader["class_id"]),
                            Semester = StudentProfileRepository.SafeGetString(reader["semester"]),
                            CourseName = StudentProfileRepository.SafeGetString(reader["course_name"]),
                            Weekday = StudentProfileRepository.SafeGetInt(reader["weekday"]),
                            StartPeriod = StudentProfileRepository.SafeGetInt(reader["start_period"]),
                            EndPeriod = StudentProfileRepository.SafeGetInt(reader["end_period"]),
                            WeekRange = StudentProfileRepository.SafeGetString(reader["week_range"])
                        });
                    }
                }
            }

            for (int i = 0; i < slots.Count; i++)
            {
                for (int j = i + 1; j < slots.Count; j++)
                {
                    var a = slots[i];
                    var b = slots[j];
                    if (a.ClassId == b.ClassId) continue;
                    if (!string.Equals(a.Semester, b.Semester, StringComparison.OrdinalIgnoreCase)) continue;
                    if (a.Weekday != b.Weekday) continue;
                    if (a.StartPeriod > b.EndPeriod || b.StartPeriod > a.EndPeriod) continue;
                    if (!WeeksOverlap(a.WeekRange, b.WeekRange)) continue;
                    if (!conflicts.Contains(b.CourseName)) conflicts.Add(b.CourseName);
                }
            }

            return conflicts;
        }

        private static bool HasCapacity(OracleConnection connection, OracleTransaction tx, int classId)
        {
            const string sql = @"
                SELECT capacity, (SELECT COUNT(*) FROM course_select WHERE class_id = :classId) AS selected
                  FROM teaching_class
                 WHERE class_id = :classId
                 FOR UPDATE";
            using (OracleCommand cmd = CreateCommand(connection, sql, tx))
            {
                cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return false;
                    int capacity = StudentProfileRepository.SafeGetInt(reader["capacity"]);
                    int selected = StudentProfileRepository.SafeGetInt(reader["selected"]);
                    return selected < capacity;
                }
            }
        }

        private static void DeleteSelections(OracleConnection connection, OracleTransaction tx, string studentNo, int batchId, IList<int> classIds)
        {
            foreach (var classId in classIds)
            {
                const string sql = "DELETE FROM course_select WHERE student_no=:studentNo AND batch_id=:batchId AND class_id=:classId";
                using (OracleCommand cmd = CreateCommand(connection, sql, tx))
                {
                    cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                    cmd.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;
                    cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void InsertSelections(OracleConnection connection, OracleTransaction tx, string studentNo, int batchId, IList<int> classIds)
        {
            foreach (var classId in classIds)
            {
                const string sql = @"
                    INSERT INTO course_select (select_id, class_id, batch_id, student_no)
                    VALUES (course_select_id_seq.NEXTVAL, :classId, :batchId, :studentNo)";
                using (OracleCommand cmd = CreateCommand(connection, sql, tx))
                {
                    cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    cmd.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;
                    cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static string BuildInClause(OracleCommand cmd, string prefix, IList<int> ids)
        {
            var names = new List<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                string name = prefix + i;
                cmd.Parameters.Add(name, OracleDbType.Int32).Value = ids[i];
                names.Add(":" + name);
            }
            return "(" + string.Join(",", names) + ")";
        }

        private static bool WeeksOverlap(string a, string b)
        {
            var na = ExtractWeekRange(a);
            var nb = ExtractWeekRange(b);
            int aMin = Math.Min(na[0], na[1]);
            int aMax = Math.Max(na[0], na[1]);
            int bMin = Math.Min(nb[0], nb[1]);
            int bMax = Math.Max(nb[0], nb[1]);
            return aMin <= bMax && bMin <= aMax;
        }

        private static int[] ExtractWeekRange(string range)
        {
            var numbers = System.Text.RegularExpressions.Regex.Matches(range ?? "", "\\d+");
            if (numbers.Count >= 2)
            {
                return new[] { int.Parse(numbers[0].Value), int.Parse(numbers[1].Value) };
            }
            if (numbers.Count == 1)
            {
                int value = int.Parse(numbers[0].Value);
                return new[] { value, value };
            }
            return new[] { 1, 99 };
        }

        public List<ScheduleItemDto> GetWeeklySchedule(string studentNo, string semester)
        {
            var items = new List<ScheduleItemDto>();

            const string sql = @"
                SELECT tc.class_id,
                       c.course_name,
                       tc.class_name,
                       s.semester,
                       c.credit,
                       c.total_hours,
                       u.real_name AS teacher_name,
                       ct.classroom,
                       ct.weekday,
                       ct.start_period,
                       ct.end_period,
                       ct.week_range
                  FROM course_select cs
                  JOIN teaching_class tc ON tc.class_id = cs.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  JOIN course_time ct ON ct.class_id = tc.class_id
                  JOIN teacher t ON t.teacher_no = tc.teacher_no
                  JOIN ""user"" u ON u.user_id = t.user_id
                 WHERE cs.student_no = :studentNo
                   AND (:semester IS NULL OR s.semester = :semester)
                 ORDER BY ct.weekday, ct.start_period";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                command.Parameters.Add("semester", OracleDbType.Varchar2).Value =
                    string.IsNullOrWhiteSpace(semester) ? (object)DBNull.Value : semester.Trim();

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new ScheduleItemDto
                        {
                            ClassId = StudentProfileRepository.SafeGetInt(reader["class_id"]),
                            CourseName = StudentProfileRepository.SafeGetString(reader["course_name"]),
                            ClassName = StudentProfileRepository.SafeGetString(reader["class_name"]),
                            Semester = StudentProfileRepository.SafeGetString(reader["semester"]),
                            Credit = StudentProfileRepository.SafeGetDecimal(reader["credit"]),
                            TotalHours = StudentProfileRepository.SafeGetInt(reader["total_hours"]),
                            TeacherName = StudentProfileRepository.SafeGetString(reader["teacher_name"]),
                            Classroom = StudentProfileRepository.SafeGetString(reader["classroom"]),
                            Weekday = StudentProfileRepository.SafeGetInt(reader["weekday"]),
                            StartPeriod = StudentProfileRepository.SafeGetInt(reader["start_period"]),
                            EndPeriod = StudentProfileRepository.SafeGetInt(reader["end_period"]),
                            WeekRange = StudentProfileRepository.SafeGetString(reader["week_range"])
                        });
                    }
                }
            }

            return items;
        }

        private List<ScheduleItemDto> GetClassSchedule(int classId)
        {
            var items = new List<ScheduleItemDto>();

            const string sql = @"
                SELECT ct.class_id,
                       c.course_name,
                       tc.class_name,
                       u.real_name AS teacher_name,
                       ct.classroom,
                       ct.weekday,
                       ct.start_period,
                       ct.end_period,
                       ct.week_range
                  FROM course_time ct
                  JOIN teaching_class tc ON tc.class_id = ct.class_id
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  JOIN teacher t ON t.teacher_no = tc.teacher_no
                  JOIN ""user"" u ON u.user_id = t.user_id
                 WHERE ct.class_id = :classId
                 ORDER BY ct.weekday, ct.start_period";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new ScheduleItemDto
                        {
                            ClassId = StudentProfileRepository.SafeGetInt(reader["class_id"]),
                            CourseName = StudentProfileRepository.SafeGetString(reader["course_name"]),
                            ClassName = StudentProfileRepository.SafeGetString(reader["class_name"]),
                            TeacherName = StudentProfileRepository.SafeGetString(reader["teacher_name"]),
                            Classroom = StudentProfileRepository.SafeGetString(reader["classroom"]),
                            Weekday = StudentProfileRepository.SafeGetInt(reader["weekday"]),
                            StartPeriod = StudentProfileRepository.SafeGetInt(reader["start_period"]),
                            EndPeriod = StudentProfileRepository.SafeGetInt(reader["end_period"]),
                            WeekRange = StudentProfileRepository.SafeGetString(reader["week_range"])
                        });
                    }
                }
            }

            return items;
        }

        private List<string> CheckTimeConflict(OracleConnection connection, string studentNo, int classId, OracleTransaction transaction)
        {
            var conflicts = new List<string>();

            const string sql = @"
                SELECT DISTINCT c2.course_name
                  FROM course_time ct1
                  JOIN teaching_class tc1 ON tc1.class_id = ct1.class_id
                  JOIN section s1 ON s1.section_id = tc1.section_id
                  JOIN course_time ct2 ON ct2.class_id <> ct1.class_id
                    AND ct2.weekday = ct1.weekday
                    AND ct2.start_period <= ct1.end_period
                    AND ct2.end_period >= ct1.start_period
                    AND TO_NUMBER(REGEXP_SUBSTR(ct2.week_range, '[0-9]+', 1, 1)) <= NVL(TO_NUMBER(REGEXP_SUBSTR(ct1.week_range, '[0-9]+', 1, 2)), TO_NUMBER(REGEXP_SUBSTR(ct1.week_range, '[0-9]+', 1, 1)))
                    AND NVL(TO_NUMBER(REGEXP_SUBSTR(ct2.week_range, '[0-9]+', 1, 2)), TO_NUMBER(REGEXP_SUBSTR(ct2.week_range, '[0-9]+', 1, 1))) >= TO_NUMBER(REGEXP_SUBSTR(ct1.week_range, '[0-9]+', 1, 1))
                  JOIN course_select cs ON cs.class_id = ct2.class_id AND cs.student_no = :studentNo
                  JOIN teaching_class tc2 ON tc2.class_id = ct2.class_id
                  JOIN section s2 ON s2.section_id = tc2.section_id AND s2.semester = s1.semester
                  JOIN course c2 ON c2.course_id = s2.course_id
                 WHERE ct1.class_id = :classId
                 ORDER BY c2.course_name";

            using (OracleCommand cmd = CreateCommand(connection, sql, transaction))
            {
                cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        conflicts.Add(StudentProfileRepository.SafeGetString(reader["course_name"]));
                    }
                }
            }

            return conflicts;
        }

        private void UpdateSelectedCount(OracleConnection connection, int classId, OracleTransaction transaction)
        {
            const string updateSql = @"
                UPDATE teaching_class
                   SET selected_count = (SELECT COUNT(*) FROM course_select WHERE class_id = :classId)
                 WHERE class_id = :classId";

            using (OracleCommand cmd = CreateCommand(connection, updateSql, transaction))
            {
                cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                cmd.ExecuteNonQuery();
            }
        }

        private static int? GetActiveBatchId(OracleConnection connection, OracleTransaction transaction)
        {
            const string sql = @"SELECT batch_id FROM selection_batch WHERE status = 1 AND start_time <= SYSDATE AND end_time >= SYSDATE ORDER BY start_time DESC FETCH FIRST 1 ROW ONLY";
            using OracleCommand command = CreateCommand(connection, sql, transaction);
            object value = command.ExecuteScalar();
            return value == null || value == DBNull.Value ? null : Convert.ToInt32(value);
        }

        private static OracleCommand CreateCommand(OracleConnection connection, string sql, OracleTransaction? transaction = null)
        {
            OracleCommand command = new OracleCommand(sql, connection);
            command.BindByName = true;
            command.Transaction = transaction;
            return command;
        }
    }
}
