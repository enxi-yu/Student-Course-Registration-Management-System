using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using StudentCourse.Infrastructure;
using StudentCourse.Student.Models;

namespace StudentCourse.Student.Repositories
{
    public sealed class CourseSelectionRepository
    {
        public List<CourseSelectionDto> GetAvailableCourses(string studentNo, string semester)
        {
            var courses = new List<CourseSelectionDto>();

            const string sql = @"
                SELECT tc.class_id,
                       c.course_name,
                       tc.class_name,
                       c.course_type,
                       u.real_name AS teacher_name,
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
                 WHERE s.semester = :semester
                 ORDER BY c.course_name, tc.class_name";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                command.Parameters.Add("semester", OracleDbType.Varchar2).Value = semester;

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new CourseSelectionDto
                        {
                            ClassId = StudentProfileRepository.SafeGetInt(reader["class_id"]),
                            CourseName = StudentProfileRepository.SafeGetString(reader["course_name"]),
                            ClassName = StudentProfileRepository.SafeGetString(reader["class_name"]),
                            CourseType = StudentProfileRepository.SafeGetString(reader["course_type"]),
                            TeacherName = StudentProfileRepository.SafeGetString(reader["teacher_name"]),
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

        public CourseDetailDto? GetCourseDetail(int classId)
        {
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
            using (OracleCommand command = CreateCommand(connection, sql))
            {
                command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return null;

                    return new CourseDetailDto
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
        }

        public SelectionResultDto SelectCourse(string studentNo, int classId, int batchId)
        {
            var result = new SelectionResultDto { Success = false };

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                connection.Open();

                // 检查重复选课
                const string checkSql = @"
                    SELECT COUNT(*) FROM course_select
                     WHERE student_no = :studentNo AND class_id = :classId";

                using (OracleCommand cmd = CreateCommand(connection, checkSql))
                {
                    cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                    cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                    {
                        result.Message = "已选择该课程，不能重复选课。";
                        return result;
                    }
                }

                // 检查容量
                const string capSql = @"
                    SELECT capacity, selected_count FROM teaching_class WHERE class_id = :classId";

                using (OracleCommand cmd = CreateCommand(connection, capSql))
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
                result.ConflictCourses = CheckTimeConflict(connection, studentNo, classId);
                if (result.ConflictCourses != null && result.ConflictCourses.Count > 0)
                {
                    result.Message = "选课失败：与已选课程存在时间冲突。";
                    return result;
                }

                // 执行选课
                const string insertSql = @"
                    INSERT INTO course_select (select_id, class_id, batch_id, student_no)
                    VALUES ((SELECT NVL(MAX(select_id), 0) + 1 FROM course_select), :classId, :batchId, :studentNo)";

                using (OracleCommand cmd = CreateCommand(connection, insertSql))
                {
                    cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    cmd.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;
                    cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                    cmd.ExecuteNonQuery();
                }

                UpdateSelectedCount(connection, classId);

                result.Success = true;
                result.Message = "选课成功！";
                return result;
            }
        }

        public SelectionResultDto DropCourse(string studentNo, int classId)
        {
            var result = new SelectionResultDto { Success = false };

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            {
                connection.Open();

                const string deleteSql = @"
                    DELETE FROM course_select
                     WHERE student_no = :studentNo AND class_id = :classId";

                using (OracleCommand cmd = CreateCommand(connection, deleteSql))
                {
                    cmd.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = studentNo;
                    cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                    int affected = cmd.ExecuteNonQuery();

                    if (affected == 0)
                    {
                        result.Message = "未找到该选课记录，无法退课。";
                        return result;
                    }
                }

                UpdateSelectedCount(connection, classId);

                result.Success = true;
                result.Message = "退课成功。";
                return result;
            }
        }

        public List<ScheduleItemDto> GetWeeklySchedule(string studentNo, string semester)
        {
            var items = new List<ScheduleItemDto>();

            const string sql = @"
                SELECT tc.class_id,
                       c.course_name,
                       tc.class_name,
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

        private List<string> CheckTimeConflict(OracleConnection connection, string studentNo, int classId)
        {
            var conflicts = new List<string>();

            const string sql = @"
                SELECT DISTINCT c2.course_name
                  FROM course_time ct1
                  JOIN course_time ct2 ON ct2.class_id <> ct1.class_id
                    AND ct2.weekday = ct1.weekday
                    AND ct2.start_period <= ct1.end_period
                    AND ct2.end_period >= ct1.start_period
                  JOIN course_select cs ON cs.class_id = ct2.class_id AND cs.student_no = :studentNo
                  JOIN teaching_class tc2 ON tc2.class_id = ct2.class_id
                  JOIN section s2 ON s2.section_id = tc2.section_id
                  JOIN course c2 ON c2.course_id = s2.course_id
                 WHERE ct1.class_id = :classId
                 ORDER BY c2.course_name";

            using (OracleCommand cmd = CreateCommand(connection, sql))
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

        private void UpdateSelectedCount(OracleConnection connection, int classId)
        {
            const string updateSql = @"
                UPDATE teaching_class
                   SET selected_count = (SELECT COUNT(*) FROM course_select WHERE class_id = :classId)
                 WHERE class_id = :classId";

            using (OracleCommand cmd = CreateCommand(connection, updateSql))
            {
                cmd.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
                cmd.ExecuteNonQuery();
            }
        }

        private static OracleCommand CreateCommand(OracleConnection connection, string sql)
        {
            OracleCommand command = new OracleCommand(sql, connection);
            command.BindByName = true;
            return command;
        }
    }
}
