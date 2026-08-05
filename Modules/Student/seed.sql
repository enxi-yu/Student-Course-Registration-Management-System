BEGIN
  FOR c IN (SELECT table_name, constraint_name FROM user_constraints WHERE constraint_type = 'R')
  LOOP
    EXECUTE IMMEDIATE 'ALTER TABLE "' || c.table_name || '" DISABLE CONSTRAINT "' || c.constraint_name || '"';
  END LOOP;
END;
/
DELETE FROM course_select;
DELETE FROM student_score;
DELETE FROM course_evaluation;
DELETE FROM system_log;
DELETE FROM course_time;
DELETE FROM course_application;
DELETE FROM teaching_class;
DELETE FROM section;
DELETE FROM selection_batch;
DELETE FROM student;
DELETE FROM teacher;
DELETE FROM administrator;
DELETE FROM role_permission;
DELETE FROM "user";
DELETE FROM course;
DELETE FROM permission;
DELETE FROM role;
COMMIT;
INSERT INTO role (role_id, role_name) VALUES (0, '学生');
INSERT INTO role (role_id, role_name) VALUES (1, '教师');
INSERT INTO "user" (user_id, username, password, role_id, real_name) VALUES (9001, 'T001', 'T001', 1, '张教授');
INSERT INTO teacher (user_id, teacher_no, title, department) VALUES (9001, 'T001', '教授', '计算机学院');
INSERT INTO "user" (user_id, username, password, role_id, real_name) VALUES (9101, 'S2024001', 'S2024001', 0, '测试账号');
INSERT INTO student (user_id, student_no, major, grade) VALUES (9101, 'S2024001', '计算机科学与技术', '2024');
INSERT INTO course (course_id, course_name, course_type, credit, total_hours) VALUES (101, '数据库系统概论', '必修', 4, 64);
INSERT INTO course (course_id, course_name, course_type, credit, total_hours) VALUES (102, '数据结构与算法', '必修', 4, 64);
INSERT INTO course (course_id, course_name, course_type, credit, total_hours) VALUES (103, '操作系统', '必修', 3.5, 56);
INSERT INTO course (course_id, course_name, course_type, credit, total_hours) VALUES (104, '计算机网络', '必修', 3, 48);
INSERT INTO course (course_id, course_name, course_type, credit, total_hours) VALUES (105, '软件工程', '选修', 2, 32);
INSERT INTO section (section_id, course_id, semester) VALUES (201, 101, '2026-spring');
INSERT INTO section (section_id, course_id, semester) VALUES (202, 102, '2026-spring');
INSERT INTO section (section_id, course_id, semester) VALUES (203, 103, '2026-spring');
INSERT INTO section (section_id, course_id, semester) VALUES (204, 104, '2026-spring');
INSERT INTO section (section_id, course_id, semester) VALUES (205, 105, '2026-spring');
INSERT INTO teaching_class (class_id, class_name, teacher_no, capacity, selected_count, section_id) VALUES (301, '数据库-01班', 'T001', 60, 0, 201);
INSERT INTO teaching_class (class_id, class_name, teacher_no, capacity, selected_count, section_id) VALUES (302, '数据结构-01班', 'T001', 60, 0, 202);
INSERT INTO teaching_class (class_id, class_name, teacher_no, capacity, selected_count, section_id) VALUES (303, '操作系统-01班', 'T001', 50, 0, 203);
INSERT INTO teaching_class (class_id, class_name, teacher_no, capacity, selected_count, section_id) VALUES (304, '计算机网络-01班', 'T001', 50, 50, 204);
INSERT INTO teaching_class (class_id, class_name, teacher_no, capacity, selected_count, section_id) VALUES (305, '软件工程-01班', 'T001', 40, 0, 205);
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (401, 301, 1, 1, 2, '1-16', '致远楼201');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (402, 301, 3, 3, 4, '1-16', '致远楼201');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (403, 302, 2, 1, 2, '1-16', '明德楼305');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (404, 303, 1, 3, 4, '1-16', '致远楼301');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (405, 304, 3, 1, 2, '1-16', '知行楼102');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (406, 305, 2, 3, 4, '1-16', '思源楼203');
COMMIT;
BEGIN
  FOR c IN (SELECT table_name, constraint_name FROM user_constraints WHERE constraint_type = 'R')
  LOOP
    EXECUTE IMMEDIATE 'ALTER TABLE "' || c.table_name || '" ENABLE CONSTRAINT "' || c.constraint_name || '"';
  END LOOP;
END;
/
