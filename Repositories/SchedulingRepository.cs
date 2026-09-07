using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using StudentCourse.Infrastructure;
using StudentCourse.Models;

namespace StudentCourse.Repositories
{
    public sealed class SchedulingRepository
    {
        public SchedulingLookupDto<CourseOptionDto> SearchCourses(string? keyword, int page, int pageSize)
        {
            SchedulingLookupDto<CourseOptionDto> result = new SchedulingLookupDto<CourseOptionDto> { Page = page, PageSize = pageSize };
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            const string sql = @"SELECT course_id, course_name, department FROM (
                    SELECT c.course_id, c.course_name, c.department,
                           ROW_NUMBER() OVER (ORDER BY c.course_name, c.course_id) AS rn
                      FROM course c
                     WHERE :keyword IS NULL
                        OR UPPER(c.course_name) LIKE '%' || UPPER(:keyword) || '%'
                        OR TO_CHAR(c.course_id) LIKE '%' || :keyword || '%'
                        OR UPPER(NVL(c.department, '')) LIKE '%' || UPPER(:keyword) || '%'
                ) WHERE rn > :offset AND rn <= :upperBound ORDER BY rn";
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("keyword", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(keyword) ? DBNull.Value : keyword.Trim();
                command.Parameters.Add("offset", OracleDbType.Int32).Value = (page - 1) * pageSize;
                command.Parameters.Add("upperBound", OracleDbType.Int32).Value = page * pageSize + 1;
                using OracleDataReader reader = command.ExecuteReader();
                while (reader.Read()) result.Items.Add(new CourseOptionDto
                {
                    CourseId = Convert.ToInt32(reader["course_id"]),
                    CourseName = Convert.ToString(reader["course_name"]) ?? "",
                    Department = Convert.ToString(reader["department"]) ?? ""
                });
            }
            result.HasMore = result.Items.Count > pageSize;
            if (result.HasMore) result.Items.RemoveAt(result.Items.Count - 1);
            return result;
        }

        public SchedulingLookupDto<TeacherOptionDto> SearchTeachers(string? keyword, int page, int pageSize)
        {
            SchedulingLookupDto<TeacherOptionDto> result = new SchedulingLookupDto<TeacherOptionDto> { Page = page, PageSize = pageSize };
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            const string sql = @"SELECT teacher_no, real_name, department, title FROM (
                    SELECT t.teacher_no, u.real_name, t.department, t.title,
                           ROW_NUMBER() OVER (ORDER BY u.real_name, t.teacher_no) AS rn
                      FROM teacher t JOIN ""user"" u ON u.user_id = t.user_id
                     WHERE u.status = 1 AND (:keyword IS NULL
                        OR UPPER(u.real_name) LIKE '%' || UPPER(:keyword) || '%'
                        OR UPPER(t.teacher_no) LIKE '%' || UPPER(:keyword) || '%'
                        OR UPPER(NVL(t.department, '')) LIKE '%' || UPPER(:keyword) || '%'
                        OR UPPER(NVL(t.title, '')) LIKE '%' || UPPER(:keyword) || '%')
                ) WHERE rn > :offset AND rn <= :upperBound ORDER BY rn";
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("keyword", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(keyword) ? DBNull.Value : keyword.Trim();
                command.Parameters.Add("offset", OracleDbType.Int32).Value = (page - 1) * pageSize;
                command.Parameters.Add("upperBound", OracleDbType.Int32).Value = page * pageSize + 1;
                using OracleDataReader reader = command.ExecuteReader();
                while (reader.Read()) result.Items.Add(new TeacherOptionDto
                {
                    TeacherNo = Convert.ToString(reader["teacher_no"]) ?? "",
                    TeacherName = Convert.ToString(reader["real_name"]) ?? "",
                    Department = Convert.ToString(reader["department"]) ?? "",
                    Title = Convert.ToString(reader["title"]) ?? ""
                });
            }
            result.HasMore = result.Items.Count > pageSize;
            if (result.HasMore) result.Items.RemoveAt(result.Items.Count - 1);
            return result;
        }

        public IList<ScheduleRowDto> GetSchedules(string? semester)
        {
            const string sql = @"
                SELECT tc.class_id, tc.class_name, c.course_id, c.course_name, s.semester,
                       tc.teacher_no, u.real_name AS teacher_name, tc.capacity, tc.selected_count,
                       c.total_hours,
                       LISTAGG('周' || CASE ct.weekday WHEN 1 THEN '一' WHEN 2 THEN '二' WHEN 3 THEN '三' WHEN 4 THEN '四' WHEN 5 THEN '五' WHEN 6 THEN '六' ELSE '日' END
                           || ' ' || ct.start_period || '-' || ct.end_period || '节 ' || ct.week_range || ' ' || ct.classroom, '；')
                           WITHIN GROUP (ORDER BY ct.weekday, ct.start_period) AS schedule_text
                  FROM teaching_class tc
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  JOIN teacher t ON t.teacher_no = tc.teacher_no
                  JOIN ""user"" u ON u.user_id = t.user_id
                  LEFT JOIN course_time ct ON ct.class_id = tc.class_id
                 WHERE (:semester IS NULL OR s.semester = :semester)
                 GROUP BY tc.class_id, tc.class_name, c.course_id, c.course_name, s.semester,
                          tc.teacher_no, u.real_name, tc.capacity, tc.selected_count, c.total_hours
                 ORDER BY s.semester DESC, c.course_name, tc.class_name";

            List<ScheduleRowDto> rows = new List<ScheduleRowDto>();
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("semester", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(semester) ? DBNull.Value : semester.Trim();
            using OracleDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new ScheduleRowDto
                {
                    ClassId = Convert.ToInt32(reader["class_id"]), ClassName = Convert.ToString(reader["class_name"]) ?? "",
                    CourseId = Convert.ToInt32(reader["course_id"]), CourseName = Convert.ToString(reader["course_name"]) ?? "",
                    Semester = Convert.ToString(reader["semester"]) ?? "", TeacherNo = Convert.ToString(reader["teacher_no"]) ?? "",
                    TeacherName = Convert.ToString(reader["teacher_name"]) ?? "", Capacity = Convert.ToInt32(reader["capacity"]),
                    SelectedCount = Convert.ToInt32(reader["selected_count"]), TotalHours = Convert.ToInt32(reader["total_hours"]),
                    ScheduleText = reader["schedule_text"] == DBNull.Value ? "尚未安排时间" : Convert.ToString(reader["schedule_text"]) ?? ""
                });
            }
            return rows;
        }

        public ScheduleDetailDto GetSchedule(int classId)
        {
            const string classSql = @"
                SELECT tc.class_id, s.course_id, c.course_name, c.department AS course_department,
                       tc.teacher_no, u.real_name AS teacher_name, t.department AS teacher_department,
                       t.title AS teacher_title, s.semester, tc.class_name, tc.capacity, tc.selected_count
                  FROM teaching_class tc
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  JOIN teacher t ON t.teacher_no = tc.teacher_no
                  JOIN ""user"" u ON u.user_id = t.user_id
                 WHERE tc.class_id = :classId";
            const string timesSql = @"
                SELECT weekday, start_period, end_period,
                       TO_NUMBER(REGEXP_SUBSTR(week_range, '[0-9]+', 1, 1)) AS start_week,
                       NVL(TO_NUMBER(REGEXP_SUBSTR(week_range, '[0-9]+', 1, 2)),
                           TO_NUMBER(REGEXP_SUBSTR(week_range, '[0-9]+', 1, 1))) AS end_week,
                       classroom
                  FROM course_time
                 WHERE class_id = :classId
                 ORDER BY weekday, start_period, time_id";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            ScheduleDetailDto? result = null;
            using (OracleCommand command = CreateCommand(connection, classSql))
            {
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                using OracleDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    result = new ScheduleDetailDto
                    {
                        ClassId = Convert.ToInt32(reader["class_id"]),
                        CourseId = Convert.ToInt32(reader["course_id"]),
                        CourseName = Convert.ToString(reader["course_name"]) ?? "",
                        CourseDepartment = Convert.ToString(reader["course_department"]) ?? "",
                        TeacherNo = Convert.ToString(reader["teacher_no"]) ?? "",
                        TeacherName = Convert.ToString(reader["teacher_name"]) ?? "",
                        TeacherDepartment = Convert.ToString(reader["teacher_department"]) ?? "",
                        TeacherTitle = Convert.ToString(reader["teacher_title"]) ?? "",
                        Semester = Convert.ToString(reader["semester"]) ?? "",
                        ClassName = Convert.ToString(reader["class_name"]) ?? "",
                        Capacity = Convert.ToInt32(reader["capacity"]),
                        SelectedCount = Convert.ToInt32(reader["selected_count"])
                    };
                }
            }
            if (result == null) throw new InvalidOperationException("排课记录不存在");

            using (OracleCommand command = CreateCommand(connection, timesSql))
            {
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                using OracleDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Times.Add(new ScheduleTimeInput
                    {
                        Weekday = Convert.ToInt32(reader["weekday"]),
                        StartPeriod = Convert.ToInt32(reader["start_period"]),
                        EndPeriod = Convert.ToInt32(reader["end_period"]),
                        StartWeek = Convert.ToInt32(reader["start_week"]),
                        EndWeek = Convert.ToInt32(reader["end_week"]),
                        Classroom = Convert.ToString(reader["classroom"]) ?? ""
                    });
                }
            }
            return result;
        }

        public ScheduleRowDto Create(SchedulingInput input)
        {
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleTransaction transaction = connection.BeginTransaction();
            try
            {
                int sectionId = FindOrCreateSection(connection, transaction, input.CourseId, input.Semester);
                int classId = NextSequenceValue(connection, transaction, "teaching_class_id_seq");
                EnsureClassNameUnique(connection, transaction, sectionId, input.ClassName);
                const string classSql = "INSERT INTO teaching_class (class_id, class_name, teacher_no, capacity, selected_count, section_id) VALUES (:classId, :className, :teacherNo, :capacity, 0, :sectionId)";
                using (OracleCommand command = CreateCommand(connection, classSql, transaction))
                {
                    command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    command.Parameters.Add("className", OracleDbType.Varchar2).Value = input.ClassName.Trim();
                    command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = input.TeacherNo;
                    command.Parameters.Add("capacity", OracleDbType.Int32).Value = input.Capacity;
                    command.Parameters.Add("sectionId", OracleDbType.Int32).Value = sectionId;
                    command.ExecuteNonQuery();
                }

                foreach (ScheduleTimeInput time in input.Times)
                {
                    const string conflictSql = @"SELECT COUNT(*) FROM course_time ct JOIN teaching_class tc ON tc.class_id = ct.class_id JOIN section s ON s.section_id = tc.section_id WHERE s.semester = :semester AND ct.weekday = :weekday AND ct.start_period <= :endPeriod AND ct.end_period >= :startPeriod AND TO_NUMBER(REGEXP_SUBSTR(ct.week_range, '[0-9]+', 1, 1)) <= :endWeek AND NVL(TO_NUMBER(REGEXP_SUBSTR(ct.week_range, '[0-9]+', 1, 2)), TO_NUMBER(REGEXP_SUBSTR(ct.week_range, '[0-9]+', 1, 1))) >= :startWeek AND (tc.teacher_no = :teacherNo OR ct.classroom = :classroom)";
                    using (OracleCommand conflict = CreateCommand(connection, conflictSql, transaction))
                    {
                        conflict.Parameters.Add("semester", OracleDbType.Varchar2).Value = input.Semester;
                        conflict.Parameters.Add("weekday", OracleDbType.Int32).Value = time.Weekday;
                        conflict.Parameters.Add("endPeriod", OracleDbType.Int32).Value = time.EndPeriod;
                        conflict.Parameters.Add("startPeriod", OracleDbType.Int32).Value = time.StartPeriod;
                        conflict.Parameters.Add("endWeek", OracleDbType.Int32).Value = time.EndWeek;
                        conflict.Parameters.Add("startWeek", OracleDbType.Int32).Value = time.StartWeek;
                        conflict.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = input.TeacherNo;
                        conflict.Parameters.Add("classroom", OracleDbType.Varchar2).Value = time.Classroom;
                        if (Convert.ToInt32(conflict.ExecuteScalar()) > 0) throw new InvalidOperationException("任课教师或教室在所选时间已有课程");
                    }
                    const string timeSql = "INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (:timeId, :classId, :weekday, :startPeriod, :endPeriod, :weekRange, :classroom)";
                    using OracleCommand command = CreateCommand(connection, timeSql, transaction);
                    command.Parameters.Add("timeId", OracleDbType.Int32).Value = NextSequenceValue(connection, transaction, "course_time_id_seq");
                    command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    command.Parameters.Add("weekday", OracleDbType.Int32).Value = time.Weekday;
                    command.Parameters.Add("startPeriod", OracleDbType.Int32).Value = time.StartPeriod;
                    command.Parameters.Add("endPeriod", OracleDbType.Int32).Value = time.EndPeriod;
                    command.Parameters.Add("weekRange", OracleDbType.Varchar2).Value = $"{time.StartWeek}-{time.EndWeek}周";
                    command.Parameters.Add("classroom", OracleDbType.Varchar2).Value = time.Classroom;
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
                return GetSchedules(input.Semester).Single(row => row.ClassId == classId);
            }
            catch { transaction.Rollback(); throw; }
        }

        public ScheduleRowDto Update(int classId, SchedulingInput input)
        {
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleTransaction transaction = connection.BeginTransaction();
            try
            {
                int selectedCount;
                using (OracleCommand check = CreateCommand(connection, "SELECT selected_count FROM teaching_class WHERE class_id = :classId FOR UPDATE", transaction))
                {
                    check.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    object? value = check.ExecuteScalar();
                    if (value == null || value == DBNull.Value) throw new InvalidOperationException("排课记录不存在");
                    selectedCount = Convert.ToInt32(value);
                }
                if (input.Capacity < selectedCount) throw new InvalidOperationException($"课程容量不能小于当前已选人数 {selectedCount}");

                int sectionId = FindOrCreateSection(connection, transaction, input.CourseId, input.Semester);
                EnsureClassNameUnique(connection, transaction, sectionId, input.ClassName, classId);
                CheckConflicts(connection, transaction, input, classId);

                using (OracleCommand times = CreateCommand(connection, "DELETE FROM course_time WHERE class_id = :classId", transaction))
                {
                    times.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    times.ExecuteNonQuery();
                }
                const string updateSql = @"UPDATE teaching_class
                                               SET class_name = :className, teacher_no = :teacherNo,
                                                   capacity = :capacity, section_id = :sectionId
                                             WHERE class_id = :classId";
                using (OracleCommand command = CreateCommand(connection, updateSql, transaction))
                {
                    command.Parameters.Add("className", OracleDbType.Varchar2).Value = input.ClassName.Trim();
                    command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = input.TeacherNo.Trim();
                    command.Parameters.Add("capacity", OracleDbType.Int32).Value = input.Capacity;
                    command.Parameters.Add("sectionId", OracleDbType.Int32).Value = sectionId;
                    command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    command.ExecuteNonQuery();
                }
                InsertTimes(connection, transaction, classId, input.Times);
                transaction.Commit();
                return GetSchedules(input.Semester).Single(row => row.ClassId == classId);
            }
            catch { transaction.Rollback(); throw; }
        }

        public void Delete(int classId)
        {
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleTransaction transaction = connection.BeginTransaction();
            try
            {
                int sectionId;
                using (OracleCommand section = CreateCommand(connection, "SELECT section_id FROM teaching_class WHERE class_id = :classId FOR UPDATE", transaction))
                {
                    section.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    object? value = section.ExecuteScalar();
                    if (value == null || value == DBNull.Value) throw new InvalidOperationException("排课记录不存在");
                    sectionId = Convert.ToInt32(value);
                }
                using (OracleCommand check = CreateCommand(connection, "SELECT COUNT(*) FROM course_select WHERE class_id = :classId", transaction))
                {
                    check.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    if (Convert.ToInt32(check.ExecuteScalar()) > 0) throw new InvalidOperationException("该教学班已有学生选课，不能删除排课");
                }
                using (OracleCommand times = CreateCommand(connection, "DELETE FROM course_time WHERE class_id = :classId", transaction)) { times.Parameters.Add("classId", OracleDbType.Int32).Value = classId; times.ExecuteNonQuery(); }
                using (OracleCommand classes = CreateCommand(connection, "DELETE FROM teaching_class WHERE class_id = :classId", transaction)) { classes.Parameters.Add("classId", OracleDbType.Int32).Value = classId; if (classes.ExecuteNonQuery() == 0) throw new InvalidOperationException("排课记录不存在"); }
                using (OracleCommand section = CreateCommand(connection, "DELETE FROM section WHERE section_id = :sectionId AND NOT EXISTS (SELECT 1 FROM teaching_class WHERE section_id = :sectionId)", transaction))
                {
                    section.Parameters.Add("sectionId", OracleDbType.Int32).Value = sectionId;
                    section.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch { transaction.Rollback(); throw; }
        }

        private static int FindOrCreateSection(OracleConnection connection, OracleTransaction transaction, int courseId, string semester)
        {
            using (OracleCommand find = CreateCommand(connection, "SELECT section_id FROM section WHERE course_id = :courseId AND semester = :semester FETCH FIRST 1 ROW ONLY", transaction))
            {
                find.Parameters.Add("courseId", OracleDbType.Int32).Value = courseId;
                find.Parameters.Add("semester", OracleDbType.Varchar2).Value = semester;
                object? value = find.ExecuteScalar();
                if (value != null && value != DBNull.Value) return Convert.ToInt32(value);
            }
            int sectionId = NextSequenceValue(connection, transaction, "section_id_seq");
            using OracleCommand insert = CreateCommand(connection, "INSERT INTO section (section_id, course_id, semester) VALUES (:sectionId, :courseId, :semester)", transaction);
            insert.Parameters.Add("sectionId", OracleDbType.Int32).Value = sectionId;
            insert.Parameters.Add("courseId", OracleDbType.Int32).Value = courseId;
            insert.Parameters.Add("semester", OracleDbType.Varchar2).Value = semester;
            insert.ExecuteNonQuery();
            return sectionId;
        }

        private static int NextSequenceValue(OracleConnection connection, OracleTransaction transaction, string sequenceName)
        {
            using OracleCommand command = CreateCommand(connection, $"SELECT {sequenceName}.NEXTVAL FROM dual", transaction);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        private static void EnsureClassNameUnique(OracleConnection connection, OracleTransaction transaction, int sectionId, string? className, int? excludedClassId = null)
        {
            string value = (className ?? string.Empty).Trim();
            string sql = @"SELECT COUNT(*) FROM teaching_class WHERE section_id = :sectionId AND TRIM(class_name) = :className";
            if (excludedClassId.HasValue) 
                sql += " AND class_id <> :excludedClassId";
            using (OracleCommand command = CreateCommand(connection, sql, transaction))
            {
                command.Parameters.Add("sectionId", OracleDbType.Int32).Value = sectionId;
                command.Parameters.Add("className", OracleDbType.Varchar2).Value = value;
                if (excludedClassId.HasValue) 
                    command.Parameters.Add("excludedClassId", OracleDbType.Int32).Value = excludedClassId.Value;
                if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                {
                    throw new InvalidOperationException($"同课程同学期已存在教学班“{value}”，请更换教学班名称");
                }
            }
        }

        private static void CheckConflicts(OracleConnection connection, OracleTransaction transaction, SchedulingInput input, int excludedClassId)
        {
            const string sql = @"SELECT COUNT(*) FROM course_time ct
                JOIN teaching_class tc ON tc.class_id = ct.class_id
                JOIN section s ON s.section_id = tc.section_id
                WHERE s.semester = :semester AND ct.class_id <> :excludedClassId
                  AND ct.weekday = :weekday AND ct.start_period <= :endPeriod AND ct.end_period >= :startPeriod
                  AND TO_NUMBER(REGEXP_SUBSTR(ct.week_range, '[0-9]+', 1, 1)) <= :endWeek
                  AND NVL(TO_NUMBER(REGEXP_SUBSTR(ct.week_range, '[0-9]+', 1, 2)), TO_NUMBER(REGEXP_SUBSTR(ct.week_range, '[0-9]+', 1, 1))) >= :startWeek
                  AND (tc.teacher_no = :teacherNo OR ct.classroom = :classroom)";
            foreach (ScheduleTimeInput time in input.Times)
            {
                using OracleCommand command = CreateCommand(connection, sql, transaction);
                command.Parameters.Add("semester", OracleDbType.Varchar2).Value = input.Semester;
                command.Parameters.Add("excludedClassId", OracleDbType.Int32).Value = excludedClassId;
                command.Parameters.Add("weekday", OracleDbType.Int32).Value = time.Weekday;
                command.Parameters.Add("endPeriod", OracleDbType.Int32).Value = time.EndPeriod;
                command.Parameters.Add("startPeriod", OracleDbType.Int32).Value = time.StartPeriod;
                command.Parameters.Add("endWeek", OracleDbType.Int32).Value = time.EndWeek;
                command.Parameters.Add("startWeek", OracleDbType.Int32).Value = time.StartWeek;
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = input.TeacherNo;
                command.Parameters.Add("classroom", OracleDbType.Varchar2).Value = time.Classroom;
                if (Convert.ToInt32(command.ExecuteScalar()) > 0) throw new InvalidOperationException("任课教师或教室在所选时间已有课程");
            }
        }

        private static void InsertTimes(OracleConnection connection, OracleTransaction transaction, int classId, IList<ScheduleTimeInput> times)
        {
            const string sql = "INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (:timeId, :classId, :weekday, :startPeriod, :endPeriod, :weekRange, :classroom)";
            foreach (ScheduleTimeInput time in times)
            {
                using OracleCommand command = CreateCommand(connection, sql, transaction);
                command.Parameters.Add("timeId", OracleDbType.Int32).Value = NextSequenceValue(connection, transaction, "course_time_id_seq");
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                command.Parameters.Add("weekday", OracleDbType.Int32).Value = time.Weekday;
                command.Parameters.Add("startPeriod", OracleDbType.Int32).Value = time.StartPeriod;
                command.Parameters.Add("endPeriod", OracleDbType.Int32).Value = time.EndPeriod;
                command.Parameters.Add("weekRange", OracleDbType.Varchar2).Value = $"{time.StartWeek}-{time.EndWeek}周";
                command.Parameters.Add("classroom", OracleDbType.Varchar2).Value = time.Classroom.Trim();
                command.ExecuteNonQuery();
            }
        }

        private static OracleCommand CreateCommand(OracleConnection connection, string sql, OracleTransaction? transaction = null)
        {
            OracleCommand command = connection.CreateCommand(); command.BindByName = true; command.CommandText = sql; command.Transaction = transaction; return command;
        }
    }
}
