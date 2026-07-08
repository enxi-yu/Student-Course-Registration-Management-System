-- 教师端第二阶段测试数据
-- 说明：
-- 1. 本脚本不会被程序自动执行，请在确认测试库后手动执行。
-- 2. 当前项目建表 SQL 中用户表名为 "user"，因此所有引用都带双引号。
-- 3. 使用 MERGE 避免重复插入，不删除、不覆盖已有业务数据。

MERGE INTO role r
USING (SELECT 0 AS role_id, '学生' AS role_name, '学生角色' AS role_desc FROM dual) src
ON (r.role_id = src.role_id)
WHEN NOT MATCHED THEN
  INSERT (role_id, role_name, role_desc)
  VALUES (src.role_id, src.role_name, src.role_desc);

MERGE INTO role r
USING (SELECT 1 AS role_id, '教师' AS role_name, '教师角色' AS role_desc FROM dual) src
ON (r.role_id = src.role_id)
WHEN NOT MATCHED THEN
  INSERT (role_id, role_name, role_desc)
  VALUES (src.role_id, src.role_name, src.role_desc);

MERGE INTO "user" u
USING (
  SELECT 9001 AS user_id,
         'teacher_demo' AS username,
         '123456' AS password,
         1 AS role_id,
         '张老师' AS real_name,
         '13800000001' AS phone,
         'teacher_demo@example.com' AS email
    FROM dual
) src
ON (u.user_id = src.user_id)
WHEN NOT MATCHED THEN
  INSERT (user_id, username, password, role_id, real_name, phone, email, status, create_time)
  VALUES (src.user_id, src.username, src.password, src.role_id, src.real_name, src.phone, src.email, 1, SYSDATE);

MERGE INTO teacher t
USING (
  SELECT 9001 AS user_id,
         'T001' AS teacher_no,
         '讲师' AS title,
         '计算机科学与技术学院' AS department
    FROM dual
) src
ON (t.teacher_no = src.teacher_no)
WHEN NOT MATCHED THEN
  INSERT (user_id, teacher_no, title, department)
  VALUES (src.user_id, src.teacher_no, src.title, src.department);

MERGE INTO "user" u
USING (
  SELECT 9101 AS user_id,
         'student_demo_1' AS username,
         '123456' AS password,
         0 AS role_id,
         '李明' AS real_name,
         '13800001001' AS phone,
         'student_demo_1@example.com' AS email
    FROM dual
) src
ON (u.user_id = src.user_id)
WHEN NOT MATCHED THEN
  INSERT (user_id, username, password, role_id, real_name, phone, email, status, create_time)
  VALUES (src.user_id, src.username, src.password, src.role_id, src.real_name, src.phone, src.email, 1, SYSDATE);

MERGE INTO "user" u
USING (
  SELECT 9102 AS user_id,
         'student_demo_2' AS username,
         '123456' AS password,
         0 AS role_id,
         '王雨' AS real_name,
         '13800001002' AS phone,
         'student_demo_2@example.com' AS email
    FROM dual
) src
ON (u.user_id = src.user_id)
WHEN NOT MATCHED THEN
  INSERT (user_id, username, password, role_id, real_name, phone, email, status, create_time)
  VALUES (src.user_id, src.username, src.password, src.role_id, src.real_name, src.phone, src.email, 1, SYSDATE);

MERGE INTO student s
USING (
  SELECT 9101 AS user_id,
         'S001' AS student_no,
         '计算机科学与技术' AS major,
         '2024' AS grade
    FROM dual
) src
ON (s.student_no = src.student_no)
WHEN NOT MATCHED THEN
  INSERT (user_id, student_no, major, grade, avg_gpa, credit_finished)
  VALUES (src.user_id, src.student_no, src.major, src.grade, 0, 0);

MERGE INTO student s
USING (
  SELECT 9102 AS user_id,
         'S002' AS student_no,
         '软件工程' AS major,
         '2024' AS grade
    FROM dual
) src
ON (s.student_no = src.student_no)
WHEN NOT MATCHED THEN
  INSERT (user_id, student_no, major, grade, avg_gpa, credit_finished)
  VALUES (src.user_id, src.student_no, src.major, src.grade, 0, 0);

MERGE INTO course c
USING (
  SELECT 1001 AS course_id,
         '数据库原理' AS course_name,
         '专业必修' AS course_type,
         3.0 AS credit,
         48 AS total_hours,
         '计算机科学与技术学院' AS department,
         '数据库系统基本理论与应用实践' AS course_desc
    FROM dual
) src
ON (c.course_id = src.course_id)
WHEN NOT MATCHED THEN
  INSERT (course_id, course_name, course_type, credit, total_hours, department, course_desc)
  VALUES (src.course_id, src.course_name, src.course_type, src.credit, src.total_hours, src.department, src.course_desc);

MERGE INTO course c
USING (
  SELECT 1002 AS course_id,
         '数据结构' AS course_name,
         '专业必修' AS course_type,
         4.0 AS credit,
         64 AS total_hours,
         '计算机科学与技术学院' AS department,
         '线性表、树、图与常用算法' AS course_desc
    FROM dual
) src
ON (c.course_id = src.course_id)
WHEN NOT MATCHED THEN
  INSERT (course_id, course_name, course_type, credit, total_hours, department, course_desc)
  VALUES (src.course_id, src.course_name, src.course_type, src.credit, src.total_hours, src.department, src.course_desc);

MERGE INTO section s
USING (SELECT 2001 AS section_id, 1001 AS course_id, '2025-2026-2' AS semester FROM dual) src
ON (s.section_id = src.section_id)
WHEN NOT MATCHED THEN
  INSERT (section_id, course_id, semester)
  VALUES (src.section_id, src.course_id, src.semester);

MERGE INTO section s
USING (SELECT 2002 AS section_id, 1002 AS course_id, '2025-2026-2' AS semester FROM dual) src
ON (s.section_id = src.section_id)
WHEN NOT MATCHED THEN
  INSERT (section_id, course_id, semester)
  VALUES (src.section_id, src.course_id, src.semester);

MERGE INTO teaching_class tc
USING (
  SELECT 3001 AS class_id,
         '数据库原理一班' AS class_name,
         'T001' AS teacher_no,
         60 AS capacity,
         2 AS selected_count,
         2001 AS section_id
    FROM dual
) src
ON (tc.class_id = src.class_id)
WHEN NOT MATCHED THEN
  INSERT (class_id, class_name, teacher_no, capacity, selected_count, section_id)
  VALUES (src.class_id, src.class_name, src.teacher_no, src.capacity, src.selected_count, src.section_id);

MERGE INTO teaching_class tc
USING (
  SELECT 3002 AS class_id,
         '数据结构一班' AS class_name,
         'T001' AS teacher_no,
         50 AS capacity,
         1 AS selected_count,
         2002 AS section_id
    FROM dual
) src
ON (tc.class_id = src.class_id)
WHEN NOT MATCHED THEN
  INSERT (class_id, class_name, teacher_no, capacity, selected_count, section_id)
  VALUES (src.class_id, src.class_name, src.teacher_no, src.capacity, src.selected_count, src.section_id);

MERGE INTO selection_batch b
USING (
  SELECT 4001 AS batch_id,
         '2025-2026-2 第一轮选课' AS batch_name,
         SYSDATE - 30 AS start_time,
         SYSDATE + 30 AS end_time,
         1 AS status
    FROM dual
) src
ON (b.batch_id = src.batch_id)
WHEN NOT MATCHED THEN
  INSERT (batch_id, batch_name, start_time, end_time, status)
  VALUES (src.batch_id, src.batch_name, src.start_time, src.end_time, src.status);

MERGE INTO course_select cs
USING (SELECT 5001 AS select_id, 3001 AS class_id, 4001 AS batch_id, 'S001' AS student_no FROM dual) src
ON (cs.select_id = src.select_id)
WHEN NOT MATCHED THEN
  INSERT (select_id, class_id, batch_id, student_no)
  VALUES (src.select_id, src.class_id, src.batch_id, src.student_no);

MERGE INTO course_select cs
USING (SELECT 5002 AS select_id, 3001 AS class_id, 4001 AS batch_id, 'S002' AS student_no FROM dual) src
ON (cs.select_id = src.select_id)
WHEN NOT MATCHED THEN
  INSERT (select_id, class_id, batch_id, student_no)
  VALUES (src.select_id, src.class_id, src.batch_id, src.student_no);

MERGE INTO course_select cs
USING (SELECT 5003 AS select_id, 3002 AS class_id, 4001 AS batch_id, 'S001' AS student_no FROM dual) src
ON (cs.select_id = src.select_id)
WHEN NOT MATCHED THEN
  INSERT (select_id, class_id, batch_id, student_no)
  VALUES (src.select_id, src.class_id, src.batch_id, src.student_no);

COMMIT;
