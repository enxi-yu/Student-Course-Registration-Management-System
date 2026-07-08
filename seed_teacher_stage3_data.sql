-- 教师端第三阶段测试数据
-- 请手动执行；不删除、不覆盖已有业务数据。
-- 当前项目用户表为 "user"，因此必须带双引号。

MERGE INTO role r
USING (SELECT 0 AS role_id, '学生' AS role_name, '学生角色' AS role_desc FROM dual) src
ON (r.role_id = src.role_id)
WHEN NOT MATCHED THEN
  INSERT (role_id, role_name, role_desc) VALUES (src.role_id, src.role_name, src.role_desc);

MERGE INTO role r
USING (SELECT 1 AS role_id, '教师' AS role_name, '教师角色' AS role_desc FROM dual) src
ON (r.role_id = src.role_id)
WHEN NOT MATCHED THEN
  INSERT (role_id, role_name, role_desc) VALUES (src.role_id, src.role_name, src.role_desc);

MERGE INTO "user" u
USING (
  SELECT 9001 AS user_id, 'teacher_demo' AS username, '123456' AS password, 1 AS role_id,
         '张老师' AS real_name, '13800000001' AS phone, 'teacher_demo@example.com' AS email
    FROM dual
) src
ON (u.user_id = src.user_id)
WHEN NOT MATCHED THEN
  INSERT (user_id, username, password, role_id, real_name, phone, email, status, create_time)
  VALUES (src.user_id, src.username, src.password, src.role_id, src.real_name, src.phone, src.email, 1, SYSDATE);

MERGE INTO teacher t
USING (
  SELECT 9001 AS user_id, 'T001' AS teacher_no, '讲师' AS title, '计算机科学与技术学院' AS department
    FROM dual
) src
ON (t.teacher_no = src.teacher_no)
WHEN NOT MATCHED THEN
  INSERT (user_id, teacher_no, title, department)
  VALUES (src.user_id, src.teacher_no, src.title, src.department);

MERGE INTO course c
USING (
  SELECT 1001 AS course_id, '数据库原理' AS course_name, '专业必修' AS course_type,
         3.0 AS credit, 48 AS total_hours, '计算机科学与技术学院' AS department,
         '数据库系统基本理论与应用实践' AS course_desc
    FROM dual
) src
ON (c.course_id = src.course_id)
WHEN NOT MATCHED THEN
  INSERT (course_id, course_name, course_type, credit, total_hours, department, course_desc)
  VALUES (src.course_id, src.course_name, src.course_type, src.credit, src.total_hours, src.department, src.course_desc);

MERGE INTO course c
USING (
  SELECT 1002 AS course_id, '数据结构' AS course_name, '专业必修' AS course_type,
         4.0 AS credit, 64 AS total_hours, '计算机科学与技术学院' AS department,
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
  INSERT (section_id, course_id, semester) VALUES (src.section_id, src.course_id, src.semester);

MERGE INTO section s
USING (SELECT 2002 AS section_id, 1002 AS course_id, '2025-2026-2' AS semester FROM dual) src
ON (s.section_id = src.section_id)
WHEN NOT MATCHED THEN
  INSERT (section_id, course_id, semester) VALUES (src.section_id, src.course_id, src.semester);

MERGE INTO teaching_class tc
USING (
  SELECT 3001 AS class_id, '数据库原理一班' AS class_name, 'T001' AS teacher_no,
         60 AS capacity, 3 AS selected_count, 2001 AS section_id
    FROM dual
) src
ON (tc.class_id = src.class_id)
WHEN NOT MATCHED THEN
  INSERT (class_id, class_name, teacher_no, capacity, selected_count, section_id)
  VALUES (src.class_id, src.class_name, src.teacher_no, src.capacity, src.selected_count, src.section_id);

MERGE INTO teaching_class tc
USING (
  SELECT 3002 AS class_id, '数据结构一班' AS class_name, 'T001' AS teacher_no,
         50 AS capacity, 2 AS selected_count, 2002 AS section_id
    FROM dual
) src
ON (tc.class_id = src.class_id)
WHEN NOT MATCHED THEN
  INSERT (class_id, class_name, teacher_no, capacity, selected_count, section_id)
  VALUES (src.class_id, src.class_name, src.teacher_no, src.capacity, src.selected_count, src.section_id);

MERGE INTO selection_batch b
USING (
  SELECT 4001 AS batch_id, '2025-2026-2 第一轮选课' AS batch_name,
         SYSDATE - 30 AS start_time, SYSDATE + 30 AS end_time, 1 AS status
    FROM dual
) src
ON (b.batch_id = src.batch_id)
WHEN NOT MATCHED THEN
  INSERT (batch_id, batch_name, start_time, end_time, status)
  VALUES (src.batch_id, src.batch_name, src.start_time, src.end_time, src.status);

MERGE INTO "user" u
USING (SELECT 9201 AS user_id, 'stage3_student_1' AS username, '123456' AS password, 0 AS role_id, '张三' AS real_name FROM dual) src
ON (u.user_id = src.user_id)
WHEN NOT MATCHED THEN
  INSERT (user_id, username, password, role_id, real_name, status, create_time)
  VALUES (src.user_id, src.username, src.password, src.role_id, src.real_name, 1, SYSDATE);

MERGE INTO "user" u
USING (SELECT 9202 AS user_id, 'stage3_student_2' AS username, '123456' AS password, 0 AS role_id, '李四' AS real_name FROM dual) src
ON (u.user_id = src.user_id)
WHEN NOT MATCHED THEN
  INSERT (user_id, username, password, role_id, real_name, status, create_time)
  VALUES (src.user_id, src.username, src.password, src.role_id, src.real_name, 1, SYSDATE);

MERGE INTO "user" u
USING (SELECT 9203 AS user_id, 'stage3_student_3' AS username, '123456' AS password, 0 AS role_id, '王五' AS real_name FROM dual) src
ON (u.user_id = src.user_id)
WHEN NOT MATCHED THEN
  INSERT (user_id, username, password, role_id, real_name, status, create_time)
  VALUES (src.user_id, src.username, src.password, src.role_id, src.real_name, 1, SYSDATE);

MERGE INTO student s
USING (SELECT 9201 AS user_id, '2452501' AS student_no, '计算机科学与技术' AS major, '2024级' AS grade FROM dual) src
ON (s.student_no = src.student_no)
WHEN NOT MATCHED THEN
  INSERT (user_id, student_no, major, grade, avg_gpa, credit_finished)
  VALUES (src.user_id, src.student_no, src.major, src.grade, 0, 0);

MERGE INTO student s
USING (SELECT 9202 AS user_id, '2452502' AS student_no, '软件工程' AS major, '2024级' AS grade FROM dual) src
ON (s.student_no = src.student_no)
WHEN NOT MATCHED THEN
  INSERT (user_id, student_no, major, grade, avg_gpa, credit_finished)
  VALUES (src.user_id, src.student_no, src.major, src.grade, 0, 0);

MERGE INTO student s
USING (SELECT 9203 AS user_id, '2452503' AS student_no, '人工智能' AS major, '2024级' AS grade FROM dual) src
ON (s.student_no = src.student_no)
WHEN NOT MATCHED THEN
  INSERT (user_id, student_no, major, grade, avg_gpa, credit_finished)
  VALUES (src.user_id, src.student_no, src.major, src.grade, 0, 0);

MERGE INTO course_select cs
USING (SELECT 5301 AS select_id, 3001 AS class_id, 4001 AS batch_id, '2452501' AS student_no FROM dual) src
ON (cs.select_id = src.select_id)
WHEN NOT MATCHED THEN
  INSERT (select_id, class_id, batch_id, student_no)
  VALUES (src.select_id, src.class_id, src.batch_id, src.student_no);

MERGE INTO course_select cs
USING (SELECT 5302 AS select_id, 3001 AS class_id, 4001 AS batch_id, '2452502' AS student_no FROM dual) src
ON (cs.select_id = src.select_id)
WHEN NOT MATCHED THEN
  INSERT (select_id, class_id, batch_id, student_no)
  VALUES (src.select_id, src.class_id, src.batch_id, src.student_no);

MERGE INTO course_select cs
USING (SELECT 5303 AS select_id, 3002 AS class_id, 4001 AS batch_id, '2452503' AS student_no FROM dual) src
ON (cs.select_id = src.select_id)
WHEN NOT MATCHED THEN
  INSERT (select_id, class_id, batch_id, student_no)
  VALUES (src.select_id, src.class_id, src.batch_id, src.student_no);

MERGE INTO student_score ss
USING (
  SELECT 'STAGE3_SCORE_2452501_3001' AS score_id,
         '2452501' AS student_no,
         3001 AS class_id,
         88.5 AS total_score,
         'B' AS grade_level,
         3.0 AS gpa,
         3.0 AS credit_obtained,
         '初始测试成绩' AS update_remark
    FROM dual
) src
ON (ss.score_id = src.score_id)
WHEN NOT MATCHED THEN
  INSERT (score_id, student_no, class_id, total_score, grade_level, gpa, credit_obtained, entry_time, update_remark, update_time)
  VALUES (src.score_id, src.student_no, src.class_id, src.total_score, src.grade_level, src.gpa, src.credit_obtained, SYSDATE, src.update_remark, SYSDATE);

COMMIT;
