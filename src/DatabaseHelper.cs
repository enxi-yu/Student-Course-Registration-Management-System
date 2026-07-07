using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Windows.Forms;

namespace StudentCourse
{
    public static class DatabaseHelper
    {
        // 读取连接字符串
        private static string _connStr = "User Id=course;Password=Course2026;Data Source=47.116.104.63:1521/COURSEPDB;";

        /// <summary>
        /// 检查数据库连接是否正常
        /// </summary>
        public static bool TestConnection()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 初始化数据库：删除所有表并重建
        /// </summary>
        /// <summary>
        /// 初始化数据库：如果表不存在则自动创建（保留已有数据）
        /// </summary>
        public static bool EnsureDatabaseInitialized()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(_connStr))
                {
                    conn.Open();

                    // ✅ 只在表不存在时才创建，表已存在则跳过（保留数据）
                    if (!TableExists(conn, "ROLE"))
                    {
                        CreateAllTables(conn);
                        MessageBox.Show("数据库初始化完成！所有表已创建。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    // else: 表已存在，跳过建表，保留所有数据

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"数据库初始化失败：{ex.Message}\n请检查网络连接或数据库配置。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 判断表是否存在
        /// </summary>
        private static bool TableExists(OracleConnection conn, string tableName)
        {
            string sql = "SELECT COUNT(*) FROM user_tables WHERE UPPER(table_name) = UPPER(:tableName)";
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.Parameters.Add(new OracleParameter("tableName", tableName));
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        
        /// <summary>
        /// 创建所有表
        /// </summary>
        private static void CreateAllTables(OracleConnection conn)
        {
       
            string[] createSqls = GetCreateTableSqls();
            foreach (string createSql in createSqls)
            {
                using (OracleCommand cmd = new OracleCommand(createSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 获取所有建表SQL（按依赖顺序排列）
        /// </summary>
        private static string[] GetCreateTableSqls()
        {
            return new string[]
            {
                // ========== 1. role表 ==========
                @"CREATE TABLE role (
                    role_id NUMBER(1) PRIMARY KEY,
                    role_name VARCHAR2(20) NOT NULL,
                    role_desc VARCHAR2(255)
                )",

                // ========== 2. permission表 ==========
                @"CREATE TABLE permission (
                    perm_id NUMBER PRIMARY KEY,
                    perm_code VARCHAR2(50) NOT NULL UNIQUE,
                    perm_name VARCHAR2(50) NOT NULL,
                    module VARCHAR2(50) NOT NULL
                )",

                // ========== 3. role_permission表 ==========
                @"CREATE TABLE role_permission (
                    role_id NUMBER(1) NOT NULL,
                    perm_id NUMBER NOT NULL,
                    PRIMARY KEY (role_id, perm_id),
                    FOREIGN KEY (role_id) REFERENCES role(role_id),
                    FOREIGN KEY (perm_id) REFERENCES permission(perm_id)
                )",

                // ========== 4. user表 ==========
                @"
                CREATE TABLE ""user"" (
                    user_id NUMBER PRIMARY KEY,
                    username VARCHAR2(20) NOT NULL UNIQUE,
                    password VARCHAR2(32) NOT NULL,
                    role_id NUMBER(1) NOT NULL,
                    real_name VARCHAR2(50) NOT NULL,
                    phone VARCHAR2(11),
                    email VARCHAR2(50),
                    status NUMBER(1) DEFAULT 1 NOT NULL,
                    last_login DATE,
                    create_time DATE DEFAULT SYSDATE NOT NULL,
                    FOREIGN KEY (role_id) REFERENCES role(role_id)
                )",

                // ========== 5. student表 ==========
                @"CREATE TABLE student (
                    user_id NUMBER PRIMARY KEY,
                    student_no VARCHAR2(20) NOT NULL UNIQUE,
                    major VARCHAR2(50) NOT NULL,
                    grade VARCHAR2(10) NOT NULL,
                    avg_gpa NUMBER(3,2) DEFAULT 0.00 CHECK (avg_gpa BETWEEN 0 AND 5),
                    credit_finished NUMBER(5,1) DEFAULT 0,
                    FOREIGN KEY (user_id) REFERENCES ""user""(user_id)
                )",

                // ========== 6. teacher表 ==========
                @"CREATE TABLE teacher (
                    user_id NUMBER PRIMARY KEY,
                    teacher_no VARCHAR2(20) NOT NULL UNIQUE,
                    title VARCHAR2(20) NOT NULL,
                    department VARCHAR2(50) NOT NULL,
                    FOREIGN KEY (user_id) REFERENCES ""user""(user_id)
                )",

                // ========== 7. administrator表 ==========
                @"CREATE TABLE administrator (
                    user_id NUMBER PRIMARY KEY,
                    admin_no VARCHAR2(20) NOT NULL UNIQUE,
                    admin_level NUMBER(1) NOT NULL,
                    managed_scope CLOB,
                    FOREIGN KEY (user_id) REFERENCES ""user""(user_id)
                )",

                // ========== 8. course表 ==========
                @"CREATE TABLE course (
                    course_id NUMBER PRIMARY KEY,
                    course_name VARCHAR2(20) NOT NULL,
                    course_type VARCHAR2(20) NOT NULL,
                    credit NUMBER(3,1) NOT NULL CHECK (credit >= 0),
                    total_hours NUMBER NOT NULL CHECK (total_hours >= 0),
                    department VARCHAR2(100),
                    course_desc CLOB
                )",

                // ========== 9. section表 ==========
                @"CREATE TABLE section (
                    section_id NUMBER PRIMARY KEY,
                    course_id NUMBER NOT NULL,
                    semester VARCHAR2(20) NOT NULL,
                    FOREIGN KEY (course_id) REFERENCES course(course_id)
                )",

                // ========== 10. teaching_class表 ==========
                @"CREATE TABLE teaching_class (
                    class_id NUMBER PRIMARY KEY,
                    class_name VARCHAR2(50) NOT NULL,
                    teacher_no VARCHAR2(20) NOT NULL,
                    capacity NUMBER NOT NULL,
                    selected_count NUMBER DEFAULT 0 NOT NULL,
                    section_id NUMBER NOT NULL,
                    FOREIGN KEY (teacher_no) REFERENCES teacher(teacher_no),
                    FOREIGN KEY (section_id) REFERENCES section(section_id)
                )",

                // ========== 11. course_time表 ==========
                @"CREATE TABLE course_time (
                    time_id NUMBER PRIMARY KEY,
                    class_id NUMBER NOT NULL,
                    weekday NUMBER(1) NOT NULL CHECK (weekday BETWEEN 1 AND 7),
                    start_period NUMBER(2) NOT NULL CHECK (start_period BETWEEN 1 AND 10),
                    end_period NUMBER(2) NOT NULL CHECK (end_period BETWEEN 1 AND 10),
                    week_range VARCHAR2(50) NOT NULL,
                    classroom VARCHAR2(50) NOT NULL,
                    FOREIGN KEY (class_id) REFERENCES teaching_class(class_id)
                )",

                // ========== 12. selection_batch表 ==========
                @"CREATE TABLE selection_batch (
                    batch_id NUMBER PRIMARY KEY,
                    batch_name VARCHAR2(50) NOT NULL,
                    start_time DATE NOT NULL,
                    end_time DATE NOT NULL,
                    status NUMBER(1) DEFAULT 0 NOT NULL CHECK (status IN (0, 1, 2))
                )",

                // ========== 13. course_select表 ==========
                @"CREATE TABLE course_select (
                    select_id NUMBER PRIMARY KEY,
                    class_id NUMBER NOT NULL,
                    batch_id NUMBER NOT NULL,
                    student_no VARCHAR2(20) NOT NULL,
                    FOREIGN KEY (class_id) REFERENCES teaching_class(class_id),
                    FOREIGN KEY (batch_id) REFERENCES selection_batch(batch_id),
                    FOREIGN KEY (student_no) REFERENCES student(student_no)
                )",

                // ========== 14. student_score表 ==========
                @"CREATE TABLE student_score (
                    score_id VARCHAR2(40) PRIMARY KEY,
                    student_no VARCHAR2(20) NOT NULL,
                    class_id NUMBER NOT NULL,
                    total_score NUMBER(5,1),
                    grade_level VARCHAR2(10),
                    gpa NUMBER(3,2),
                    credit_obtained NUMBER(3,1) DEFAULT 0 NOT NULL,
                    entry_time DATE DEFAULT SYSDATE,
                    update_remark VARCHAR2(200),
                    update_time DATE,
                    FOREIGN KEY (student_no) REFERENCES student(student_no),
                    FOREIGN KEY (class_id) REFERENCES teaching_class(class_id)
                )",

                // ========== 15. course_evaluation表 ==========
                @"CREATE TABLE course_evaluation (
                    eval_id VARCHAR2(40) PRIMARY KEY,
                    student_no VARCHAR2(20) NOT NULL,
                    class_id NUMBER NOT NULL,
                    d1_score NUMBER(1) NOT NULL,
                    d2_score NUMBER(1) NOT NULL,
                    d3_score NUMBER(1) NOT NULL,
                    d4_score NUMBER(1) NOT NULL,
                    eval_score NUMBER(3,1) NOT NULL,
                    eval_content CLOB,
                    eval_time DATE DEFAULT SYSDATE,
                    FOREIGN KEY (student_no) REFERENCES student(student_no),
                    FOREIGN KEY (class_id) REFERENCES teaching_class(class_id)
                )",

                // ========== 16. course_application表 ==========
                @"CREATE TABLE course_application (
                    apply_id VARCHAR2(40) PRIMARY KEY,
                    teacher_no VARCHAR2(20) NOT NULL,
                    course_name VARCHAR2(100) NOT NULL,
                    credit NUMBER(3,1) NOT NULL,
                    total_hours NUMBER NOT NULL,
                    teaching_plan CLOB,
                    textbook VARCHAR2(200),
                    apply_time DATE DEFAULT SYSDATE,
                    status VARCHAR2(20) DEFAULT '待审核' NOT NULL,
                    approve_time DATE,
                    approve_comment VARCHAR2(255),
                    FOREIGN KEY (teacher_no) REFERENCES teacher(teacher_no)
                )",

                // ========== 17. system_log表 ==========
                @"CREATE TABLE system_log (
                    log_id VARCHAR2(40) PRIMARY KEY,
                    user_id NUMBER NOT NULL,
                    operation_type VARCHAR2(30) NOT NULL,
                    operation_desc VARCHAR2(255),
                    target_id VARCHAR2(40),
                    ip_address VARCHAR2(45),
                    request_params CLOB,
                    result_status VARCHAR2(20) NOT NULL,
                    error_msg VARCHAR2(255),
                    log_time DATE DEFAULT SYSDATE,
                    FOREIGN KEY (user_id) REFERENCES ""user""(user_id)
                )"
            };
        }
    }
}