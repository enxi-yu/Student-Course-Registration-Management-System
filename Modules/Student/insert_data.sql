-- 角色
INSERT INTO role (role_id, role_name, role_desc) VALUES (0, '学生', '学生用户');
INSERT INTO role (role_id, role_name, role_desc) VALUES (1, '教师', '教师用户');

-- 教师
INSERT INTO "user" (user_id, username, password, role_id, real_name) VALUES (9001, 'T001', 'T001', 1, '张教授');
INSERT INTO teacher (user_id, teacher_no, title, department) VALUES (9001, 'T001', '教授', '计算机科学与技术学院');
INSERT INTO "user" (user_id, username, password, role_id, real_name) VALUES (9002, 'T002', 'T002', 1, '李副教授');
INSERT INTO teacher (user_id, teacher_no, title, department) VALUES (9002, 'T002', '副教授', '计算机科学与技术学院');
INSERT INTO "user" (user_id, username, password, role_id, real_name) VALUES (9003, 'T003', 'T003', 1, '王讲师');
INSERT INTO teacher (user_id, teacher_no, title, department) VALUES (9003, 'T003', '讲师', '数学与统计学院');

-- 学生
INSERT INTO "user" (user_id, username, password, role_id, real_name) VALUES (9101, 'S2024001', 'S2024001', 0, '学生测试账号');
INSERT INTO student (user_id, student_no, major, grade) VALUES (9101, 'S2024001', '计算机科学与技术', '2024');
INSERT INTO "user" (user_id, username, password, role_id, real_name) VALUES (9102, 'S2024002', 'S2024002', 0, '小明');
INSERT INTO student (user_id, student_no, major, grade) VALUES (9102, 'S2024002', '软件工程', '2024');
INSERT INTO "user" (user_id, username, password, role_id, real_name) VALUES (9103, 'S2024003', 'S2024003', 0, '小红');
INSERT INTO student (user_id, student_no, major, grade) VALUES (9103, 'S2024003', '计算机科学与技术', '2024');

-- 课程
INSERT INTO course (course_id, course_name, course_type, credit, total_hours, department) VALUES (101, '数据库系统概论', '必修', 4.0, 64, '计算机科学与技术学院');
INSERT INTO course (course_id, course_name, course_type, credit, total_hours, department) VALUES (102, '数据结构与算法', '必修', 4.0, 64, '计算机科学与技术学院');
INSERT INTO course (course_id, course_name, course_type, credit, total_hours, department) VALUES (103, '操作系统', '必修', 3.5, 56, '计算机科学与技术学院');
INSERT INTO course (course_id, course_name, course_type, credit, total_hours, department) VALUES (104, '计算机网络', '必修', 3.0, 48, '计算机科学与技术学院');
INSERT INTO course (course_id, course_name, course_type, credit, total_hours, department) VALUES (105, '软件工程', '选修', 2.0, 32, '计算机科学与技术学院');
INSERT INTO course (course_id, course_name, course_type, credit, total_hours, department) VALUES (106, '高等数学A', '必修', 5.0, 80, '数学与统计学院');

-- 学期开课
INSERT INTO section (section_id, course_id, semester) VALUES (201, 101, '2025-2026-2');
INSERT INTO section (section_id, course_id, semester) VALUES (202, 102, '2025-2026-2');
INSERT INTO section (section_id, course_id, semester) VALUES (203, 103, '2025-2026-2');
INSERT INTO section (section_id, course_id, semester) VALUES (204, 104, '2025-2026-2');
INSERT INTO section (section_id, course_id, semester) VALUES (205, 105, '2025-2026-2');
INSERT INTO section (section_id, course_id, semester) VALUES (206, 106, '2025-2026-2');

-- 教学班
INSERT INTO teaching_class (class_id, class_name, teacher_no, capacity, selected_count, section_id) VALUES (301, '数据库-01班', 'T001', 60, 5, 201);
INSERT INTO teaching_class (class_id, class_name, teacher_no, capacity, selected_count, section_id) VALUES (302, '数据结构-01班', 'T001', 60, 10, 202);
INSERT INTO teaching_class (class_id, class_name, teacher_no, capacity, selected_count, section_id) VALUES (303, '操作系统-01班', 'T002', 50, 3, 203);
INSERT INTO teaching_class (class_id, class_name, teacher_no, capacity, selected_count, section_id) VALUES (304, '计算机网络-01班', 'T002', 50, 50, 204);
INSERT INTO teaching_class (class_id, class_name, teacher_no, capacity, selected_count, section_id) VALUES (305, '软件工程-01班', 'T003', 40, 8, 205);
INSERT INTO teaching_class (class_id, class_name, teacher_no, capacity, selected_count, section_id) VALUES (306, '高数A-01班', 'T003', 80, 20, 206);

-- 上课时间
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (401, 301, 1, 1, 2, '1-16', '致远楼201');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (402, 301, 3, 3, 4, '1-16', '致远楼201');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (403, 302, 2, 1, 2, '1-16', '明德楼305');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (404, 302, 4, 5, 6, '1-16', '明德楼305');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (405, 303, 1, 3, 4, '1-16', '致远楼301');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (406, 303, 5, 1, 2, '1-16', '致远楼301');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (407, 304, 3, 1, 2, '1-16', '知行楼102');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (408, 304, 5, 3, 4, '1-16', '知行楼102');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (409, 305, 2, 3, 4, '1-16', '思源楼203');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (410, 306, 4, 1, 2, '1-16', '博学楼401');
INSERT INTO course_time (time_id, class_id, weekday, start_period, end_period, week_range, classroom) VALUES (411, 306, 5, 5, 6, '1-16', '博学楼401');

-- 选课批次
INSERT INTO selection_batch (batch_id, batch_name, start_time, end_time, status) VALUES (1, '2025-2026-2 选课批次', TO_DATE('2026-02-20', 'YYYY-MM-DD'), TO_DATE('2026-03-05', 'YYYY-MM-DD'), 0);

-- S2024001 已选课程
INSERT INTO course_select (select_id, class_id, batch_id, student_no) VALUES (501, 302, 1, 'S2024001');
INSERT INTO course_select (select_id, class_id, batch_id, student_no) VALUES (502, 303, 1, 'S2024001');
SELECT * FROM section WHERE section_id = 206;
SELECT * FROM teacher WHERE teacher_no = 'T003';
COMMIT;

