using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using StudentCourse.Infrastructure;
using StudentCourse.Models;

namespace StudentCourse.Repositories
{
    public sealed class AdminRepository
    {
        /// <summary>
        /// 获取所有课程列表
        /// </summary>
        public IList<CourseDto> GetCourses()
        {
            var list = new List<CourseDto>();
            
            using (var conn = DbConnectionFactory.OpenConnection())
            {
                string sql = "SELECT course_id, course_name, course_type, credit, total_hours, department, course_desc FROM course";
                using (var cmd = new OracleCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new CourseDto
                        {
                            CourseId = Convert.ToInt32(reader["course_id"]),
                            CourseName = reader["course_name"].ToString() ?? "",
                            CourseType = reader["course_type"].ToString() ?? "",
                            Credit = Convert.ToDecimal(reader["credit"]),
                            TotalHours = Convert.ToInt32(reader["total_hours"]),
                            Department = reader["department"]?.ToString() ?? "",
                            CourseDesc = reader["course_desc"]?.ToString() ?? ""
                        });
                    }
                }
            }
            
            return list;
        }

        /// <summary>
        /// 根据课程ID获取课程详情
        /// </summary>
        public CourseDto? GetCourseById(int courseId)
        {
            using (var conn = DbConnectionFactory.OpenConnection())
            {
                string sql = "SELECT course_id, course_name, course_type, credit, total_hours, department, course_desc FROM course WHERE course_id = :courseId";
                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.Parameters.Add(new OracleParameter("courseId", courseId));
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new CourseDto
                            {
                                CourseId = Convert.ToInt32(reader["course_id"]),
                                CourseName = reader["course_name"].ToString() ?? "",
                                CourseType = reader["course_type"].ToString() ?? "",
                                Credit = Convert.ToDecimal(reader["credit"]),
                                TotalHours = Convert.ToInt32(reader["total_hours"]),
                                Department = reader["department"]?.ToString() ?? "",
                                CourseDesc = reader["course_desc"]?.ToString() ?? ""
                            };
                        }
                    }
                }
            }
            
            return null;
        }


        ///<summary>
        /// 插入课程
        /// </summary>
        public CourseDto InsertCourse(CourseDto input)
        {
            int newId;
            
            using (var conn = DbConnectionFactory.OpenConnection())
            {
                string maxIdSql = "SELECT NVL(MAX(course_id), 0) FROM course";
                using (var maxCmd = new OracleCommand(maxIdSql, conn))
                {
                    newId = Convert.ToInt32(maxCmd.ExecuteScalar()) + 1;
                }

                string sql = "INSERT INTO course (course_id, course_name, course_type, credit, total_hours, department, course_desc) VALUES (:id, :name, :type, :credit, :hours, :dept, :cdesc)";
                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.Parameters.Add(new OracleParameter("id", newId));
                    cmd.Parameters.Add(new OracleParameter("name", input.CourseName));
                    cmd.Parameters.Add(new OracleParameter("type", input.CourseType));
                    cmd.Parameters.Add(new OracleParameter("credit", input.Credit));
                    cmd.Parameters.Add(new OracleParameter("hours", input.TotalHours));
                    cmd.Parameters.Add(new OracleParameter("dept", string.IsNullOrEmpty(input.Department) ? DBNull.Value : (object)input.Department));
                    cmd.Parameters.Add(new OracleParameter("cdesc", string.IsNullOrEmpty(input.CourseDesc) ? DBNull.Value : (object)input.CourseDesc));
                    cmd.ExecuteNonQuery();
                }
            }
            
            return GetCourseById(newId);
        }

        /// <summary>
        /// 更新课程信息
        /// </summary>
        public CourseDto UpdateCourse(int id, CourseDto input)
        {
            using (var conn = DbConnectionFactory.OpenConnection())
            {
                string sql = "UPDATE course SET course_name = :name, course_type = :type, credit = :credit, total_hours = :hours, department = :dept, course_desc = :cdesc WHERE course_id = :id";
                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.Parameters.Add(new OracleParameter("name", input.CourseName));
                    cmd.Parameters.Add(new OracleParameter("type", input.CourseType));
                    cmd.Parameters.Add(new OracleParameter("credit", input.Credit));
                    cmd.Parameters.Add(new OracleParameter("hours", input.TotalHours));
                    cmd.Parameters.Add(new OracleParameter("dept", string.IsNullOrEmpty(input.Department) ? DBNull.Value : (object)input.Department));
                    cmd.Parameters.Add(new OracleParameter("cdesc", string.IsNullOrEmpty(input.CourseDesc) ? DBNull.Value : (object)input.CourseDesc));
                    cmd.Parameters.Add(new OracleParameter("id", id));
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("课程不存在");
                    }
                }
            }
            return GetCourseById(id);
        }

        /// <summary>
        /// 删除课程（需先检查是否有section关联）
        /// </summary>
        public void DeleteCourse(int id)
        {
          using (var conn = DbConnectionFactory.OpenConnection())
          {
              try
              {
                  string checkSql = "SELECT COUNT(*) FROM section WHERE course_id = :id";
                  using (var checkCmd = new OracleCommand(checkSql, conn))
                  {
                      checkCmd.Parameters.Add(new OracleParameter("id", id));
                      int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                      if (count > 0)
                      {
                          throw new InvalidOperationException($"无法删除该课程，存在 {count} 次开课");
                      }
                  }
              }
              catch (OracleException ex) when (ex.Number == 942)
              {
                  // section表不存在，无需检查
              }
              string sql = "DELETE FROM course WHERE course_id = :id";
              using (var cmd = new OracleCommand(sql, conn))
              {
                  cmd.Parameters.Add(new OracleParameter("id", id));
                  int rowsAffected = cmd.ExecuteNonQuery();
                  if (rowsAffected == 0)
                  {
                      throw new InvalidOperationException("课程不存在");
                  }
              }
            }
        }

        /// <summary>
        /// 获取所有开课申请列表
        /// </summary>
        public IList<CourseApplicationDto> GetApplications()
        {
            List<CourseApplicationDto> list = new List<CourseApplicationDto>();
            try{
                using (var conn = DbConnectionFactory.OpenConnection()){
                    string sql = @"SELECT apply_id, teacher_no, course_name, credit, total_hours, textbook, 
                                       course_summary, course_type, department, apply_time, status, 
                                       approve_time, approve_comment 
                                FROM course_application";
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader()){
                        while (reader.Read()) {
                            list.Add(new CourseApplicationDto{
                                ApplyId = reader["apply_id"].ToString() ?? "",
                                TeacherNo = reader["teacher_no"].ToString() ?? "",
                                CourseName = reader["course_name"].ToString() ?? "",
                                Credit = Convert.ToDecimal(reader["credit"]),
                                TotalHours = Convert.ToInt32(reader["total_hours"]),
                                Textbook = reader["textbook"]?.ToString() ?? "",
                                CourseSummary = reader["course_summary"]?.ToString() ?? "",
                                CourseType = reader["course_type"].ToString() ?? "",
                                Department = reader["department"].ToString() ?? "",
                                ApplyTime = reader["apply_time"]?.ToString() ?? "",
                                Status = reader["status"].ToString() ?? "",
                                ApproveTime = reader["approve_time"]?.ToString() ?? "",
                                ApproveComment = reader["approve_comment"]?.ToString() ?? ""
                            });
                        }
                    }
                }
                return list;
            }
            catch (OracleException ex) when (ex.Number == 942){
                return list;
            }
        }

        ///<summary>
        /// 根据申请ID获取申请详情
        /// </summary>
        public CourseApplicationDto? GetApplicationById(string applyId)
        {
            using (var conn = DbConnectionFactory.OpenConnection())
            {
                string sql = @"SELECT apply_id, teacher_no, course_name, credit, total_hours, textbook, 
                                       course_summary, course_type, department, apply_time, status, 
                                       approve_time, approve_comment 
                                FROM course_application
                                WHERE apply_id = :applyID";
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("applyID", applyId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new CourseApplicationDto
                                {
                                ApplyId = reader["apply_id"].ToString() ?? "",
                                TeacherNo = reader["teacher_no"].ToString() ?? "",
                                CourseName = reader["course_name"].ToString() ?? "",
                                Credit = Convert.ToDecimal(reader["credit"]),
                                TotalHours = Convert.ToInt32(reader["total_hours"]),
                                Textbook = reader["textbook"]?.ToString() ?? "",
                                CourseSummary = reader["course_summary"]?.ToString() ?? "",
                                CourseType = reader["course_type"].ToString() ?? "",
                                Department = reader["department"].ToString() ?? "",
                                ApplyTime = reader["apply_time"]?.ToString() ?? "",
                                Status = reader["status"].ToString() ?? "",
                                ApproveTime = reader["approve_time"]?.ToString() ?? "",
                                ApproveComment = reader["approve_comment"]?.ToString() ?? ""
                            };
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 审批开课申请（通过/驳回）
        /// </summary>
        public CourseApplicationDto ApproveApplication(string applyId, string status, string comment)
        {
            //更新 course_application 表
            using (var conn = DbConnectionFactory.OpenConnection())
            {
                string sql=@"UPDATE course_application
                            SET status=:status, approve_time=SYSDATE,approve_comment=:approve_comment
                            WHERE apply_id=:applyID";
                using (var cmd = new OracleCommand(sql, conn)){
                    cmd.Parameters.Add(new OracleParameter("status", status));
                    cmd.Parameters.Add(new OracleParameter("approve_comment", comment));
                    cmd.Parameters.Add(new OracleParameter("applyID",applyId));
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0) {
                        throw new InvalidOperationException("申请不存在");
                    }
                }
                //审核通过时，同步把课程加入到course表
                CourseApplicationDto application=GetApplicationById(applyId);
                if (status == "通过"){
                    if(application!=null){
                        InsertCourse(new CourseDto{
                            CourseName=application.CourseName,CourseType=application.CourseType,
                            Credit=application.Credit,TotalHours=application.TotalHours,Department=application.Department,CourseDesc=application.CourseSummary
                    });
                    }   
                }
            }
            return GetApplicationById(applyId);
        }

        public AdminCredentialDto? GetAdminCredential(string username)
        {
            const string sql = @"
                SELECT u.user_id,
                       u.username,
                       u.password,
                       u.real_name,
                       u.status,
                       a.admin_no,
                       a.admin_level,
                       a.managed_scope
                  FROM ""user"" u
                  JOIN administrator a ON a.user_id = u.user_id
                 WHERE u.username = :username";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("username", OracleDbType.Varchar2).Value = username;

            using OracleDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapAdminCredential(reader) : null;
        }

        public void EnsureDefaultAdmin(string passwordHash)
        {
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleTransaction transaction = connection.BeginTransaction();

            const string roleSql = @"
                MERGE INTO role r
                USING (SELECT 2 AS role_id, 'admin' AS role_name, '管理员' AS role_desc FROM dual) src
                   ON (r.role_id = src.role_id)
                 WHEN NOT MATCHED THEN
                   INSERT (role_id, role_name, role_desc)
                   VALUES (src.role_id, src.role_name, src.role_desc)";

            using (OracleCommand roleCommand = CreateCommand(connection, roleSql, transaction))
            {
                roleCommand.ExecuteNonQuery();
            }

            int userId = FindUserId(connection, transaction, "admin");
            if (userId == 0)
            {
                userId = GetNextIntId(connection, transaction, @"""user""", "user_id");

                const string insertUserSql = @"
                    INSERT INTO ""user"" (
                        user_id,
                        username,
                        password,
                        role_id,
                        real_name,
                        status,
                        create_time
                    ) VALUES (
                        :userId,
                        'admin',
                        :password,
                        2,
                        '系统管理员',
                        1,
                        SYSDATE
                    )";

                using OracleCommand insertUserCommand = CreateCommand(connection, insertUserSql, transaction);
                insertUserCommand.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                insertUserCommand.Parameters.Add("password", OracleDbType.Varchar2).Value = passwordHash;
                insertUserCommand.ExecuteNonQuery();
            }
            else
            {
                const string updateUserSql = @"
                    UPDATE ""user""
                       SET password = :password,
                           role_id = 2,
                           real_name = CASE WHEN real_name IS NULL THEN '系统管理员' ELSE real_name END,
                           status = 1
                     WHERE user_id = :userId";

                using OracleCommand updateUserCommand = CreateCommand(connection, updateUserSql, transaction);
                updateUserCommand.Parameters.Add("password", OracleDbType.Varchar2).Value = passwordHash;
                updateUserCommand.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                updateUserCommand.ExecuteNonQuery();
            }

            const string adminSql = @"
                MERGE INTO administrator a
                USING (
                    SELECT :userId AS user_id,
                           'ADM001' AS admin_no,
                           0 AS admin_level,
                           '{""scope"":""all""}' AS managed_scope
                      FROM dual
                ) src
                   ON (a.user_id = src.user_id)
                 WHEN MATCHED THEN
                   UPDATE SET a.admin_no = src.admin_no,
                              a.admin_level = src.admin_level,
                              a.managed_scope = src.managed_scope
                 WHEN NOT MATCHED THEN
                   INSERT (user_id, admin_no, admin_level, managed_scope)
                   VALUES (src.user_id, src.admin_no, src.admin_level, src.managed_scope)";

            using (OracleCommand adminCommand = CreateCommand(connection, adminSql, transaction))
            {
                adminCommand.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                adminCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public AdminCurrentDto? GetCurrentAdmin(int userId)
        {
            const string sql = @"
                SELECT u.user_id,
                       u.username,
                       u.real_name,
                       a.admin_no,
                       a.admin_level,
                       a.managed_scope
                  FROM ""user"" u
                  JOIN administrator a ON a.user_id = u.user_id
                 WHERE u.user_id = :userId";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

            using OracleDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapAdminCurrent(reader) : null;
        }

        public void UpdateLastLogin(int userId)
        {
            const string sql = @"UPDATE ""user"" SET last_login = SYSDATE WHERE user_id = :userId";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
            command.ExecuteNonQuery();
        }

        public IList<AdminPermissionDto> GetPermissions(int roleId)
        {
            const string sql = @"
                SELECT p.perm_id,
                       p.perm_code,
                       p.perm_name,
                       p.module
                  FROM role_permission rp
                  JOIN permission p ON p.perm_id = rp.perm_id
                 WHERE rp.role_id = :roleId
                 ORDER BY p.module, p.perm_code";

            List<AdminPermissionDto> rows = new List<AdminPermissionDto>();

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("roleId", OracleDbType.Int32).Value = roleId;

            using OracleDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new AdminPermissionDto
                {
                    PermissionId = ToInt32(reader["perm_id"]),
                    PermissionCode = Convert.ToString(reader["perm_code"]) ?? string.Empty,
                    PermissionName = Convert.ToString(reader["perm_name"]) ?? string.Empty,
                    Module = Convert.ToString(reader["module"]) ?? string.Empty
                });
            }

            return rows;
        }

        public IList<AdminStudentDto> GetStudents(string? keyword)
        {
            string sql = @"
                SELECT u.user_id,
                       u.username,
                       u.real_name,
                       u.phone,
                       u.email,
                       u.status,
                       TO_CHAR(u.last_login, 'YYYY-MM-DD HH24:MI:SS') AS last_login,
                       TO_CHAR(u.create_time, 'YYYY-MM-DD HH24:MI:SS') AS create_time,
                       s.student_no,
                       s.major,
                       s.grade,
                       s.avg_gpa,
                       s.credit_finished
                  FROM student s
                  JOIN ""user"" u ON u.user_id = s.user_id";

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += @"
                 WHERE UPPER(s.student_no) LIKE :keyword
                    OR UPPER(u.username) LIKE :keyword
                    OR UPPER(u.real_name) LIKE :keyword
                    OR UPPER(s.major) LIKE :keyword";
            }

            sql += " ORDER BY s.student_no FETCH FIRST 200 ROWS ONLY";

            List<AdminStudentDto> rows = new List<AdminStudentDto>();

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                command.Parameters.Add("keyword", OracleDbType.Varchar2).Value = "%" + keyword.Trim().ToUpperInvariant() + "%";
            }

            using OracleDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(MapStudent(reader));
            }

            return rows;
        }

        public AdminStudentDto? GetStudentByUserId(int userId)
        {
            const string sql = @"
                SELECT u.user_id,
                       u.username,
                       u.real_name,
                       u.phone,
                       u.email,
                       u.status,
                       TO_CHAR(u.last_login, 'YYYY-MM-DD HH24:MI:SS') AS last_login,
                       TO_CHAR(u.create_time, 'YYYY-MM-DD HH24:MI:SS') AS create_time,
                       s.student_no,
                       s.major,
                       s.grade,
                       s.avg_gpa,
                       s.credit_finished
                  FROM student s
                  JOIN ""user"" u ON u.user_id = s.user_id
                 WHERE u.user_id = :userId";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

            using OracleDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapStudent(reader) : null;
        }

        public AdminStudentDto InsertStudent(AdminUserInput input, string passwordHash)
        {
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleTransaction transaction = connection.BeginTransaction();

            int userId = InsertUser(connection, transaction, input, passwordHash, 0);

            const string sql = @"
                INSERT INTO student (
                    user_id,
                    student_no,
                    major,
                    grade,
                    avg_gpa,
                    credit_finished
                ) VALUES (
                    :userId,
                    :studentNo,
                    :major,
                    :grade,
                    :avgGpa,
                    :creditFinished
                )";

            using (OracleCommand command = CreateCommand(connection, sql, transaction))
            {
                command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = input.StudentNo.Trim();
                command.Parameters.Add("major", OracleDbType.Varchar2).Value = input.Major.Trim();
                command.Parameters.Add("grade", OracleDbType.Varchar2).Value = input.Grade.Trim();
                command.Parameters.Add("avgGpa", OracleDbType.Decimal).Value = input.AvgGpa;
                command.Parameters.Add("creditFinished", OracleDbType.Decimal).Value = input.CreditFinished;
                command.ExecuteNonQuery();
            }

            transaction.Commit();
            return GetStudentByUserId(userId)!;
        }

        public AdminStudentDto UpdateStudent(int userId, AdminUserInput input)
        {
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleTransaction transaction = connection.BeginTransaction();

            UpdateUser(connection, transaction, userId, input);

            const string sql = @"
                UPDATE student
                   SET student_no = :studentNo,
                       major = :major,
                       grade = :grade,
                       avg_gpa = :avgGpa,
                       credit_finished = :creditFinished
                 WHERE user_id = :userId";

            using (OracleCommand command = CreateCommand(connection, sql, transaction))
            {
                command.Parameters.Add("studentNo", OracleDbType.Varchar2).Value = input.StudentNo.Trim();
                command.Parameters.Add("major", OracleDbType.Varchar2).Value = input.Major.Trim();
                command.Parameters.Add("grade", OracleDbType.Varchar2).Value = input.Grade.Trim();
                command.Parameters.Add("avgGpa", OracleDbType.Decimal).Value = input.AvgGpa;
                command.Parameters.Add("creditFinished", OracleDbType.Decimal).Value = input.CreditFinished;
                command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                if (command.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException("学生不存在");
                }
            }

            transaction.Commit();
            return GetStudentByUserId(userId)!;
        }

        public IList<AdminTeacherDto> GetTeachers(string? keyword)
        {
            string sql = @"
                SELECT u.user_id,
                       u.username,
                       u.real_name,
                       u.phone,
                       u.email,
                       u.status,
                       TO_CHAR(u.last_login, 'YYYY-MM-DD HH24:MI:SS') AS last_login,
                       TO_CHAR(u.create_time, 'YYYY-MM-DD HH24:MI:SS') AS create_time,
                       t.teacher_no,
                       t.title,
                       t.department
                  FROM teacher t
                  JOIN ""user"" u ON u.user_id = t.user_id";

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += @"
                 WHERE UPPER(t.teacher_no) LIKE :keyword
                    OR UPPER(u.username) LIKE :keyword
                    OR UPPER(u.real_name) LIKE :keyword
                    OR UPPER(t.department) LIKE :keyword";
            }

            sql += " ORDER BY t.teacher_no FETCH FIRST 200 ROWS ONLY";

            List<AdminTeacherDto> rows = new List<AdminTeacherDto>();

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                command.Parameters.Add("keyword", OracleDbType.Varchar2).Value = "%" + keyword.Trim().ToUpperInvariant() + "%";
            }

            using OracleDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(MapTeacher(reader));
            }

            return rows;
        }

        public AdminTeacherDto? GetTeacherByUserId(int userId)
        {
            const string sql = @"
                SELECT u.user_id,
                       u.username,
                       u.real_name,
                       u.phone,
                       u.email,
                       u.status,
                       TO_CHAR(u.last_login, 'YYYY-MM-DD HH24:MI:SS') AS last_login,
                       TO_CHAR(u.create_time, 'YYYY-MM-DD HH24:MI:SS') AS create_time,
                       t.teacher_no,
                       t.title,
                       t.department
                  FROM teacher t
                  JOIN ""user"" u ON u.user_id = t.user_id
                 WHERE u.user_id = :userId";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

            using OracleDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapTeacher(reader) : null;
        }

        public AdminTeacherDto InsertTeacher(AdminUserInput input, string passwordHash)
        {
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleTransaction transaction = connection.BeginTransaction();

            int userId = InsertUser(connection, transaction, input, passwordHash, 1);

            const string sql = @"
                INSERT INTO teacher (
                    user_id,
                    teacher_no,
                    title,
                    department
                ) VALUES (
                    :userId,
                    :teacherNo,
                    :title,
                    :department
                )";

            using (OracleCommand command = CreateCommand(connection, sql, transaction))
            {
                command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = input.TeacherNo.Trim();
                command.Parameters.Add("title", OracleDbType.Varchar2).Value = input.Title.Trim();
                command.Parameters.Add("department", OracleDbType.Varchar2).Value = input.Department.Trim();
                command.ExecuteNonQuery();
            }

            transaction.Commit();
            return GetTeacherByUserId(userId)!;
        }

        public AdminTeacherDto UpdateTeacher(int userId, AdminUserInput input)
        {
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleTransaction transaction = connection.BeginTransaction();

            UpdateUser(connection, transaction, userId, input);

            const string sql = @"
                UPDATE teacher
                   SET teacher_no = :teacherNo,
                       title = :title,
                       department = :department
                 WHERE user_id = :userId";

            using (OracleCommand command = CreateCommand(connection, sql, transaction))
            {
                command.Parameters.Add("teacherNo", OracleDbType.Varchar2).Value = input.TeacherNo.Trim();
                command.Parameters.Add("title", OracleDbType.Varchar2).Value = input.Title.Trim();
                command.Parameters.Add("department", OracleDbType.Varchar2).Value = input.Department.Trim();
                command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                if (command.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException("教师不存在");
                }
            }

            transaction.Commit();
            return GetTeacherByUserId(userId)!;
        }

        public void SetUserStatus(int userId, int status)
        {
            const string sql = @"UPDATE ""user"" SET status = :status WHERE user_id = :userId";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("status", OracleDbType.Int32).Value = status;
            command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("用户不存在");
            }
        }

        public void ResetPassword(int userId, string passwordHash)
        {
            const string sql = @"UPDATE ""user"" SET password = :password WHERE user_id = :userId";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("password", OracleDbType.Varchar2).Value = passwordHash;
            command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("用户不存在");
            }
        }

        public IList<SelectionBatchDto> GetBatches()
        {
            const string sql = @"
                SELECT batch_id,
                       batch_name,
                       TO_CHAR(start_time, 'YYYY-MM-DD HH24:MI') AS start_time,
                       TO_CHAR(end_time, 'YYYY-MM-DD HH24:MI') AS end_time,
                       status
                  FROM selection_batch
                 ORDER BY start_time DESC, batch_id DESC";

            List<SelectionBatchDto> rows = new List<SelectionBatchDto>();

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            using OracleDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(MapBatch(reader));
            }

            return rows;
        }

        public SelectionBatchDto? GetBatchById(int batchId)
        {
            const string sql = @"
                SELECT batch_id,
                       batch_name,
                       TO_CHAR(start_time, 'YYYY-MM-DD HH24:MI') AS start_time,
                       TO_CHAR(end_time, 'YYYY-MM-DD HH24:MI') AS end_time,
                       status
                  FROM selection_batch
                 WHERE batch_id = :batchId";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;

            using OracleDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapBatch(reader) : null;
        }

        public SelectionBatchDto InsertBatch(SelectionBatchInput input, int status)
        {
            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            int batchId = GetNextIntId(connection, null, "selection_batch", "batch_id");

            const string sql = @"
                INSERT INTO selection_batch (
                    batch_id,
                    batch_name,
                    start_time,
                    end_time,
                    status
                ) VALUES (
                    :batchId,
                    :batchName,
                    :startTime,
                    :endTime,
                    :status
                )";

            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;
            AddBatchParameters(command, input, status);
            command.ExecuteNonQuery();

            return GetBatchById(batchId)!;
        }

        public SelectionBatchDto UpdateBatch(int batchId, SelectionBatchInput input, int status)
        {
            const string sql = @"
                UPDATE selection_batch
                   SET batch_name = :batchName,
                       start_time = :startTime,
                       end_time = :endTime,
                       status = :status
                 WHERE batch_id = :batchId";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            AddBatchParameters(command, input, status);
            command.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;

            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("选课批次不存在");
            }

            return GetBatchById(batchId)!;
        }

        public IList<AdminClassDto> GetClasses(string? keyword)
        {
            string sql = @"
                SELECT tc.class_id,
                       tc.class_name,
                       c.course_id,
                       c.course_name,
                       s.semester,
                       tc.teacher_no,
                       u.real_name AS teacher_name,
                       tc.capacity,
                       tc.selected_count
                  FROM teaching_class tc
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  LEFT JOIN teacher t ON t.teacher_no = tc.teacher_no
                  LEFT JOIN ""user"" u ON u.user_id = t.user_id";

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += @"
                 WHERE UPPER(tc.class_name) LIKE :keyword
                    OR UPPER(c.course_name) LIKE :keyword
                    OR UPPER(tc.teacher_no) LIKE :keyword
                    OR UPPER(s.semester) LIKE :keyword";
            }

            sql += " ORDER BY s.semester DESC, c.course_name, tc.class_name FETCH FIRST 200 ROWS ONLY";

            List<AdminClassDto> rows = new List<AdminClassDto>();

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                command.Parameters.Add("keyword", OracleDbType.Varchar2).Value = "%" + keyword.Trim().ToUpperInvariant() + "%";
            }

            using OracleDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(MapClass(reader));
            }

            return rows;
        }

        public AdminClassDto? GetClassById(int classId)
        {
            const string sql = @"
                SELECT tc.class_id,
                       tc.class_name,
                       c.course_id,
                       c.course_name,
                       s.semester,
                       tc.teacher_no,
                       u.real_name AS teacher_name,
                       tc.capacity,
                       tc.selected_count
                  FROM teaching_class tc
                  JOIN section s ON s.section_id = tc.section_id
                  JOIN course c ON c.course_id = s.course_id
                  LEFT JOIN teacher t ON t.teacher_no = tc.teacher_no
                  LEFT JOIN ""user"" u ON u.user_id = t.user_id
                 WHERE tc.class_id = :classId";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;

            using OracleDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapClass(reader) : null;
        }

        public AdminClassDto UpdateClassCapacity(int classId, int capacity)
        {
            const string sql = @"UPDATE teaching_class SET capacity = :capacity WHERE class_id = :classId";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("capacity", OracleDbType.Int32).Value = capacity;
            command.Parameters.Add("classId", OracleDbType.Int32).Value = classId;
            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("教学班不存在");
            }

            return GetClassById(classId)!;
        }

        public void InsertSystemLog(
            int userId,
            string operationType,
            string operationDesc,
            string targetId,
            string ipAddress,
            string requestParams,
            string resultStatus,
            string errorMessage)
        {
            const string sql = @"
                INSERT INTO system_log (
                    log_id,
                    user_id,
                    operation_type,
                    operation_desc,
                    target_id,
                    ip_address,
                    request_params,
                    result_status,
                    error_msg,
                    log_time
                ) VALUES (
                    :logId,
                    :userId,
                    :operationType,
                    :operationDesc,
                    :targetId,
                    :ipAddress,
                    :requestParams,
                    :resultStatus,
                    :errorMessage,
                    SYSDATE
                )";

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);
            command.Parameters.Add("logId", OracleDbType.Varchar2).Value = Guid.NewGuid().ToString("N");
            command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
            command.Parameters.Add("operationType", OracleDbType.Varchar2).Value = operationType;
            command.Parameters.Add("operationDesc", OracleDbType.Varchar2).Value = DbValue(operationDesc);
            command.Parameters.Add("targetId", OracleDbType.Varchar2).Value = DbValue(targetId);
            command.Parameters.Add("ipAddress", OracleDbType.Varchar2).Value = DbValue(ipAddress);
            command.Parameters.Add("requestParams", OracleDbType.Clob).Value = DbValue(requestParams);
            command.Parameters.Add("resultStatus", OracleDbType.Varchar2).Value = resultStatus;
            command.Parameters.Add("errorMessage", OracleDbType.Varchar2).Value = DbValue(errorMessage);
            command.ExecuteNonQuery();
        }

        public IList<SystemLogDto> GetSystemLogs(string? keyword, string? operationType, DateTime? startTime, DateTime? endTime)
        {
            string sql = @"
                SELECT l.log_id,
                       l.user_id,
                       u.username,
                       l.operation_type,
                       l.operation_desc,
                       l.target_id,
                       l.ip_address,
                       l.request_params,
                       l.result_status,
                       l.error_msg,
                       TO_CHAR(l.log_time, 'YYYY-MM-DD HH24:MI:SS') AS log_time
                  FROM system_log l
                  LEFT JOIN ""user"" u ON u.user_id = l.user_id
                 WHERE 1 = 1";

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += @"
                   AND (UPPER(u.username) LIKE :keyword
                    OR UPPER(l.operation_desc) LIKE :keyword
                    OR UPPER(l.target_id) LIKE :keyword)";
            }

            if (!string.IsNullOrWhiteSpace(operationType))
            {
                sql += " AND l.operation_type = :operationType";
            }

            if (startTime.HasValue)
            {
                sql += " AND l.log_time >= :startTime";
            }

            if (endTime.HasValue)
            {
                sql += " AND l.log_time <= :endTime";
            }

            sql += " ORDER BY l.log_time DESC FETCH FIRST 300 ROWS ONLY";

            List<SystemLogDto> rows = new List<SystemLogDto>();

            using OracleConnection connection = DbConnectionFactory.OpenConnection();
            using OracleCommand command = CreateCommand(connection, sql);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                command.Parameters.Add("keyword", OracleDbType.Varchar2).Value = "%" + keyword.Trim().ToUpperInvariant() + "%";
            }

            if (!string.IsNullOrWhiteSpace(operationType))
            {
                command.Parameters.Add("operationType", OracleDbType.Varchar2).Value = operationType.Trim();
            }

            if (startTime.HasValue)
            {
                command.Parameters.Add("startTime", OracleDbType.TimeStamp).Value = startTime.Value;
            }

            if (endTime.HasValue)
            {
                command.Parameters.Add("endTime", OracleDbType.TimeStamp).Value = endTime.Value;
            }

            using OracleDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new SystemLogDto
                {
                    LogId = Convert.ToString(reader["log_id"]) ?? string.Empty,
                    UserId = ToInt32(reader["user_id"]),
                    Username = Convert.ToString(reader["username"]) ?? string.Empty,
                    OperationType = Convert.ToString(reader["operation_type"]) ?? string.Empty,
                    OperationDesc = Convert.ToString(reader["operation_desc"]) ?? string.Empty,
                    TargetId = Convert.ToString(reader["target_id"]) ?? string.Empty,
                    IpAddress = Convert.ToString(reader["ip_address"]) ?? string.Empty,
                    RequestParams = ReadText(reader["request_params"]),
                    ResultStatus = Convert.ToString(reader["result_status"]) ?? string.Empty,
                    ErrorMessage = Convert.ToString(reader["error_msg"]) ?? string.Empty,
                    LogTime = Convert.ToString(reader["log_time"]) ?? string.Empty
                });
            }

            return rows;
        }

        private static int InsertUser(OracleConnection connection, OracleTransaction transaction, AdminUserInput input, string passwordHash, int roleId)
        {
            int userId = GetNextIntId(connection, transaction, @"""user""", "user_id");

            const string sql = @"
                INSERT INTO ""user"" (
                    user_id,
                    username,
                    password,
                    role_id,
                    real_name,
                    phone,
                    email,
                    status,
                    create_time
                ) VALUES (
                    :userId,
                    :username,
                    :password,
                    :roleId,
                    :realName,
                    :phone,
                    :email,
                    :status,
                    SYSDATE
                )";

            using OracleCommand command = CreateCommand(connection, sql, transaction);
            command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
            command.Parameters.Add("username", OracleDbType.Varchar2).Value = input.Username.Trim();
            command.Parameters.Add("password", OracleDbType.Varchar2).Value = passwordHash;
            command.Parameters.Add("roleId", OracleDbType.Int32).Value = roleId;
            command.Parameters.Add("realName", OracleDbType.Varchar2).Value = input.RealName.Trim();
            command.Parameters.Add("phone", OracleDbType.Varchar2).Value = DbValue(input.Phone);
            command.Parameters.Add("email", OracleDbType.Varchar2).Value = DbValue(input.Email);
            command.Parameters.Add("status", OracleDbType.Int32).Value = input.Status == 0 ? 0 : 1;
            command.ExecuteNonQuery();

            return userId;
        }

        private static void UpdateUser(OracleConnection connection, OracleTransaction transaction, int userId, AdminUserInput input)
        {
            const string sql = @"
                UPDATE ""user""
                   SET username = :username,
                       real_name = :realName,
                       phone = :phone,
                       email = :email,
                       status = :status
                 WHERE user_id = :userId";

            using OracleCommand command = CreateCommand(connection, sql, transaction);
            command.Parameters.Add("username", OracleDbType.Varchar2).Value = input.Username.Trim();
            command.Parameters.Add("realName", OracleDbType.Varchar2).Value = input.RealName.Trim();
            command.Parameters.Add("phone", OracleDbType.Varchar2).Value = DbValue(input.Phone);
            command.Parameters.Add("email", OracleDbType.Varchar2).Value = DbValue(input.Email);
            command.Parameters.Add("status", OracleDbType.Int32).Value = input.Status == 0 ? 0 : 1;
            command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("用户不存在");
            }
        }

        private static int GetNextIntId(OracleConnection connection, OracleTransaction? transaction, string tableName, string idColumn)
        {
            string sql = $"SELECT NVL(MAX({idColumn}), 0) + 1 FROM {tableName}";
            using OracleCommand command = CreateCommand(connection, sql, transaction);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        private static int FindUserId(OracleConnection connection, OracleTransaction transaction, string username)
        {
            const string sql = @"SELECT user_id FROM ""user"" WHERE username = :username";
            using OracleCommand command = CreateCommand(connection, sql, transaction);
            command.Parameters.Add("username", OracleDbType.Varchar2).Value = username;

            object result = command.ExecuteScalar();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        private static void AddBatchParameters(OracleCommand command, SelectionBatchInput input, int status)
        {
            command.Parameters.Add("batchName", OracleDbType.Varchar2).Value = input.BatchName.Trim();
            command.Parameters.Add("startTime", OracleDbType.TimeStamp).Value = input.StartTime;
            command.Parameters.Add("endTime", OracleDbType.TimeStamp).Value = input.EndTime;
            command.Parameters.Add("status", OracleDbType.Int32).Value = status;
        }

        private static OracleCommand CreateCommand(OracleConnection connection, string sql, OracleTransaction? transaction = null)
        {
            OracleCommand command = new OracleCommand(sql, connection);
            command.BindByName = true;
            command.Transaction = transaction;
            return command;
        }

        private static object DbValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
        }

        private static AdminCredentialDto MapAdminCredential(OracleDataReader reader)
        {
            return new AdminCredentialDto
            {
                UserId = ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]) ?? string.Empty,
                PasswordHash = Convert.ToString(reader["password"]) ?? string.Empty,
                RealName = Convert.ToString(reader["real_name"]) ?? string.Empty,
                Status = ToInt32(reader["status"]),
                AdminNo = Convert.ToString(reader["admin_no"]) ?? string.Empty,
                AdminLevel = ToInt32(reader["admin_level"]),
                ManagedScope = ReadText(reader["managed_scope"])
            };
        }

        private static AdminCurrentDto MapAdminCurrent(OracleDataReader reader)
        {
            return new AdminCurrentDto
            {
                UserId = ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]) ?? string.Empty,
                RealName = Convert.ToString(reader["real_name"]) ?? string.Empty,
                AdminNo = Convert.ToString(reader["admin_no"]) ?? string.Empty,
                AdminLevel = ToInt32(reader["admin_level"]),
                ManagedScope = ReadText(reader["managed_scope"])
            };
        }

        private static AdminStudentDto MapStudent(OracleDataReader reader)
        {
            return new AdminStudentDto
            {
                UserId = ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]) ?? string.Empty,
                RealName = Convert.ToString(reader["real_name"]) ?? string.Empty,
                Phone = Convert.ToString(reader["phone"]) ?? string.Empty,
                Email = Convert.ToString(reader["email"]) ?? string.Empty,
                Status = ToInt32(reader["status"]),
                LastLogin = Convert.ToString(reader["last_login"]) ?? string.Empty,
                CreateTime = Convert.ToString(reader["create_time"]) ?? string.Empty,
                StudentNo = Convert.ToString(reader["student_no"]) ?? string.Empty,
                Major = Convert.ToString(reader["major"]) ?? string.Empty,
                Grade = Convert.ToString(reader["grade"]) ?? string.Empty,
                AvgGpa = ToDecimal(reader["avg_gpa"]),
                CreditFinished = ToDecimal(reader["credit_finished"])
            };
        }

        private static AdminTeacherDto MapTeacher(OracleDataReader reader)
        {
            return new AdminTeacherDto
            {
                UserId = ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]) ?? string.Empty,
                RealName = Convert.ToString(reader["real_name"]) ?? string.Empty,
                Phone = Convert.ToString(reader["phone"]) ?? string.Empty,
                Email = Convert.ToString(reader["email"]) ?? string.Empty,
                Status = ToInt32(reader["status"]),
                LastLogin = Convert.ToString(reader["last_login"]) ?? string.Empty,
                CreateTime = Convert.ToString(reader["create_time"]) ?? string.Empty,
                TeacherNo = Convert.ToString(reader["teacher_no"]) ?? string.Empty,
                Title = Convert.ToString(reader["title"]) ?? string.Empty,
                Department = Convert.ToString(reader["department"]) ?? string.Empty
            };
        }

        private static SelectionBatchDto MapBatch(OracleDataReader reader)
        {
            int status = ToInt32(reader["status"]);
            return new SelectionBatchDto
            {
                BatchId = ToInt32(reader["batch_id"]),
                BatchName = Convert.ToString(reader["batch_name"]) ?? string.Empty,
                StartTime = Convert.ToString(reader["start_time"]) ?? string.Empty,
                EndTime = Convert.ToString(reader["end_time"]) ?? string.Empty,
                Status = status,
                StatusText = BatchStatusText(status)
            };
        }

        private static AdminClassDto MapClass(OracleDataReader reader)
        {
            return new AdminClassDto
            {
                ClassId = ToInt32(reader["class_id"]),
                ClassName = Convert.ToString(reader["class_name"]) ?? string.Empty,
                CourseId = ToInt32(reader["course_id"]),
                CourseName = Convert.ToString(reader["course_name"]) ?? string.Empty,
                Semester = Convert.ToString(reader["semester"]) ?? string.Empty,
                TeacherNo = Convert.ToString(reader["teacher_no"]) ?? string.Empty,
                TeacherName = Convert.ToString(reader["teacher_name"]) ?? string.Empty,
                Capacity = ToInt32(reader["capacity"]),
                SelectedCount = ToInt32(reader["selected_count"])
            };
        }

        private static string BatchStatusText(int status)
        {
            return status switch
            {
                0 => "未开始",
                1 => "进行中",
                2 => "已结束",
                _ => "未知"
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

        private static string ReadText(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            OracleClob? clob = value as OracleClob;
            return clob == null ? Convert.ToString(value) ?? string.Empty : clob.Value;
        }
    }
}
