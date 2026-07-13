-- Optional seed data for testing teacher class student list and score entry.
-- Review IDs before running in a shared database. This script never deletes data.

MERGE INTO role r
USING (SELECT 1 role_id, 'teacher' role_name, '教师' role_desc FROM dual) src
   ON (r.role_name = src.role_name)
 WHEN NOT MATCHED THEN
      INSERT (role_id, role_name, role_desc)
      VALUES (src.role_id, src.role_name, src.role_desc);

MERGE INTO role r
USING (SELECT 0 role_id, 'student' role_name, '学生' role_desc FROM dual) src
   ON (r.role_name = src.role_name)
 WHEN NOT MATCHED THEN
      INSERT (role_id, role_name, role_desc)
      VALUES (src.role_id, src.role_name, src.role_desc);

MERGE INTO "user" u
USING (
    SELECT 900101 user_id,
           'teacher_score_demo' username,
           '123456' password,
           NVL((SELECT role_id FROM role WHERE role_name = 'teacher' FETCH FIRST 1 ROW ONLY), 1) role_id,
           '测试教师' real_name,
           '13800000001' phone,
           'teacher_score_demo@example.com' email,
           1 status
      FROM dual
) src
   ON (u.username = src.username)
 WHEN NOT MATCHED THEN
      INSERT (user_id, username, password, role_id, real_name, phone, email, status, create_time)
      VALUES (src.user_id, src.username, src.password, src.role_id, src.real_name, src.phone, src.email, src.status, SYSDATE);

MERGE INTO teacher t
USING (SELECT 900101 user_id, 'T_SCORE_01' teacher_no, '讲师' title, '计算机学院' department FROM dual) src
   ON (t.teacher_no = src.teacher_no)
 WHEN NOT MATCHED THEN
      INSERT (user_id, teacher_no, title, department)
      VALUES (src.user_id, src.teacher_no, src.title, src.department);

MERGE INTO course c
USING (
    SELECT 900101 course_id,
           '数据库测试课' course_name,
           '专业课' course_type,
           3.0 credit,
           48 total_hours,
           '计算机学院' department,
           '教师端选课名单和成绩录入测试课程' course_desc
      FROM dual
) src
   ON (c.course_id = src.course_id)
 WHEN NOT MATCHED THEN
      INSERT (course_id, course_name, course_type, credit, total_hours, department, course_desc)
      VALUES (src.course_id, src.course_name, src.course_type, src.credit, src.total_hours, src.department, src.course_desc);

MERGE INTO section s
USING (SELECT 900101 section_id, 900101 course_id, '2026-2027-1' semester FROM dual) src
   ON (s.section_id = src.section_id)
 WHEN NOT MATCHED THEN
      INSERT (section_id, course_id, semester)
      VALUES (src.section_id, src.course_id, src.semester);

MERGE INTO teaching_class tc
USING (
    SELECT 900101 class_id,
           '数据库测试课-01班' class_name,
           'T_SCORE_01' teacher_no,
           60 capacity,
           2 selected_count,
           900101 section_id
      FROM dual
) src
   ON (tc.class_id = src.class_id)
 WHEN NOT MATCHED THEN
      INSERT (class_id, class_name, teacher_no, capacity, selected_count, section_id)
      VALUES (src.class_id, src.class_name, src.teacher_no, src.capacity, src.selected_count, src.section_id);

MERGE INTO selection_batch b
USING (
    SELECT 900101 batch_id,
           '测试选课批次' batch_name,
           TO_DATE('2026-07-01 08:00:00', 'YYYY-MM-DD HH24:MI:SS') start_time,
           TO_DATE('2026-08-01 18:00:00', 'YYYY-MM-DD HH24:MI:SS') end_time,
           1 status
      FROM dual
) src
   ON (b.batch_id = src.batch_id)
 WHEN NOT MATCHED THEN
      INSERT (batch_id, batch_name, start_time, end_time, status)
      VALUES (src.batch_id, src.batch_name, src.start_time, src.end_time, src.status);

MERGE INTO "user" u
USING (
    SELECT 900201 user_id,
           'student_score_demo1' username,
           '123456' password,
           NVL((SELECT role_id FROM role WHERE role_name = 'student' FETCH FIRST 1 ROW ONLY), 0) role_id,
           '学生一' real_name,
           '13800000011' phone,
           'student_score_demo1@example.com' email,
           1 status
      FROM dual
) src
   ON (u.username = src.username)
 WHEN NOT MATCHED THEN
      INSERT (user_id, username, password, role_id, real_name, phone, email, status, create_time)
      VALUES (src.user_id, src.username, src.password, src.role_id, src.real_name, src.phone, src.email, src.status, SYSDATE);

MERGE INTO "user" u
USING (
    SELECT 900202 user_id,
           'student_score_demo2' username,
           '123456' password,
           NVL((SELECT role_id FROM role WHERE role_name = 'student' FETCH FIRST 1 ROW ONLY), 0) role_id,
           '学生二' real_name,
           '13800000012' phone,
           'student_score_demo2@example.com' email,
           1 status
      FROM dual
) src
   ON (u.username = src.username)
 WHEN NOT MATCHED THEN
      INSERT (user_id, username, password, role_id, real_name, phone, email, status, create_time)
      VALUES (src.user_id, src.username, src.password, src.role_id, src.real_name, src.phone, src.email, src.status, SYSDATE);

MERGE INTO student s
USING (SELECT 900201 user_id, 'S_SCORE_01' student_no, '软件工程' major, '2026' grade FROM dual) src
   ON (s.student_no = src.student_no)
 WHEN NOT MATCHED THEN
      INSERT (user_id, student_no, major, grade, avg_gpa, credit_finished)
      VALUES (src.user_id, src.student_no, src.major, src.grade, 0, 0);

MERGE INTO student s
USING (SELECT 900202 user_id, 'S_SCORE_02' student_no, '计算机科学与技术' major, '2026' grade FROM dual) src
   ON (s.student_no = src.student_no)
 WHEN NOT MATCHED THEN
      INSERT (user_id, student_no, major, grade, avg_gpa, credit_finished)
      VALUES (src.user_id, src.student_no, src.major, src.grade, 0, 0);

MERGE INTO course_select cs
USING (SELECT 900101 select_id, 900101 class_id, 900101 batch_id, 'S_SCORE_01' student_no FROM dual) src
   ON (cs.class_id = src.class_id AND cs.student_no = src.student_no)
 WHEN NOT MATCHED THEN
      INSERT (select_id, class_id, batch_id, student_no)
      VALUES (src.select_id, src.class_id, src.batch_id, src.student_no);

MERGE INTO course_select cs
USING (SELECT 900102 select_id, 900101 class_id, 900101 batch_id, 'S_SCORE_02' student_no FROM dual) src
   ON (cs.class_id = src.class_id AND cs.student_no = src.student_no)
 WHEN NOT MATCHED THEN
      INSERT (select_id, class_id, batch_id, student_no)
      VALUES (src.select_id, src.class_id, src.batch_id, src.student_no);

MERGE INTO student_score ss
USING (
    SELECT 'seed_score_s_score_01_900101' score_id,
           'S_SCORE_01' student_no,
           900101 class_id,
           88.0 total_score,
           'B' grade_level,
           3.0 gpa,
           3.0 credit_obtained,
           '测试初始成绩' update_remark
      FROM dual
) src
   ON (ss.student_no = src.student_no AND ss.class_id = src.class_id)
 WHEN NOT MATCHED THEN
      INSERT (score_id, student_no, class_id, total_score, grade_level, gpa, credit_obtained, entry_time, update_remark, update_time)
      VALUES (src.score_id, src.student_no, src.class_id, src.total_score, src.grade_level, src.gpa, src.credit_obtained, SYSDATE, src.update_remark, SYSDATE);

COMMIT;

-- Suggested constraint if student_score currently allows duplicate score rows.
-- Run separately only after checking existing duplicate data:
-- ALTER TABLE student_score ADD CONSTRAINT uq_student_score UNIQUE (student_no, class_id);
