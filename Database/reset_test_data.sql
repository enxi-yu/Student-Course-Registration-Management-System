-- 当前系统结构的完整测试数据。执行后会清空全部业务数据并重新初始化。
-- 所有测试账号密码：Demo123

DELETE FROM course_evaluation;
DELETE FROM student_score;
DELETE FROM course_select;
DELETE FROM batch_class_scope;
DELETE FROM batch_class;
DELETE FROM course_time;
DELETE FROM teaching_class;
DELETE FROM section;
DELETE FROM course_application;
DELETE FROM selection_batch;
DELETE FROM system_log;
DELETE FROM role_permission;
DELETE FROM permission;
DELETE FROM administrator;
DELETE FROM student;
DELETE FROM teacher;
DELETE FROM course;
DELETE FROM "user";
DELETE FROM role;

INSERT INTO role VALUES (0, 'student', '学生');
INSERT INTO role VALUES (1, 'teacher', '教师');
INSERT INTO role VALUES (2, 'admin', '管理员');

INSERT INTO permission VALUES (9201, 'admin:login', '管理员登录', 'backend');
INSERT INTO permission VALUES (9202, 'course:manage', '课程管理', 'backend');
INSERT INTO permission VALUES (9203, 'application:review', '开课申请审批', 'backend');
INSERT INTO permission VALUES (9204, 'student:manage', '学生信息管理', 'backend');
INSERT INTO permission VALUES (9205, 'teacher:manage', '教师信息管理', 'backend');
INSERT INTO permission VALUES (9206, 'batch:manage', '选课批次管理', 'backend');
INSERT INTO permission VALUES (9207, 'capacity:adjust', '选课容量调整', 'backend');
INSERT INTO permission VALUES (9208, 'log:view', '系统日志查看', 'backend');
INSERT INTO permission VALUES (9209, 'permission:view', '权限查看', 'backend');
INSERT INTO permission VALUES (9210, 'evaluation:view', '评价查询', 'backend');
INSERT INTO role_permission SELECT 2, perm_id FROM permission;

INSERT INTO "user" VALUES (9101, 'S_DEMO_01', '2278a0f743d90e23b2fb4d009e6af10a', 0, '张明', '13800000101', 's_demo_01@test.edu.cn', 1, NULL, SYSDATE-100);
INSERT INTO "user" VALUES (9102, 'S_DEMO_02', '2278a0f743d90e23b2fb4d009e6af10a', 0, '李华', '13800000102', 's_demo_02@test.edu.cn', 1, NULL, SYSDATE-100);
INSERT INTO "user" VALUES (9103, 'S_DEMO_03', '2278a0f743d90e23b2fb4d009e6af10a', 0, '陈晨', '13800000103', 's_demo_03@test.edu.cn', 1, NULL, SYSDATE-100);
INSERT INTO "user" VALUES (9201, 'T_DEMO_01', '2278a0f743d90e23b2fb4d009e6af10a', 1, '王老师', '13900000201', 't_demo_01@test.edu.cn', 1, NULL, SYSDATE-200);
INSERT INTO "user" VALUES (9202, 'T_DEMO_02', '2278a0f743d90e23b2fb4d009e6af10a', 1, '李老师', '13900000202', 't_demo_02@test.edu.cn', 1, NULL, SYSDATE-200);
INSERT INTO "user" VALUES (9203, 'T_DEMO_03', '2278a0f743d90e23b2fb4d009e6af10a', 1, '陈老师', '13900000203', 't_demo_03@test.edu.cn', 1, NULL, SYSDATE-200);
INSERT INTO "user" VALUES (9301, 'A_DEMO_01', '2278a0f743d90e23b2fb4d009e6af10a', 2, '超级管理员', '13700000301', 'a_demo_01@test.edu.cn', 1, NULL, SYSDATE-300);
INSERT INTO "user" VALUES (9302, 'A_DEMO_02', '2278a0f743d90e23b2fb4d009e6af10a', 2, '教务管理员', '13700000302', 'a_demo_02@test.edu.cn', 1, NULL, SYSDATE-300);

INSERT INTO student VALUES (9101, 'S_DEMO_01', '软件工程', '2026', 3.60, 7);
INSERT INTO student VALUES (9102, 'S_DEMO_02', '计算机科学与技术', '2026', 3.00, 3);
INSERT INTO student VALUES (9103, 'S_DEMO_03', '软件工程', '2025', 4.00, 4);
INSERT INTO teacher VALUES (9201, 'T_DEMO_01', '副教授', '软件学院');
INSERT INTO teacher VALUES (9202, 'T_DEMO_02', '讲师', '计算机学院');
INSERT INTO teacher VALUES (9203, 'T_DEMO_03', '教授', '人工智能学院');
INSERT INTO administrator VALUES (9301, 'A_DEMO_01', 0, '{"scope":"all"}');
INSERT INTO administrator VALUES (9302, 'A_DEMO_02', 1, '{"scope":"teaching"}');

INSERT INTO course VALUES (8001, '数据库原理', '必修', 3, 32, '软件学院', '数据库模型、SQL 与事务管理');
INSERT INTO course VALUES (8002, '操作系统', '必修', 4, 32, '计算机学院', '进程、内存、文件与设备管理');
INSERT INTO course VALUES (8003, 'Web应用开发', '选修', 2, 32, '软件学院', 'Web 前后端应用开发基础');
INSERT INTO course VALUES (8004, '人工智能导论', '选修', 2, 32, '人工智能学院', '人工智能基础方法与应用');
INSERT INTO course VALUES (8005, '计算机网络', '必修', 3, 32, '计算机学院', '网络体系结构与协议');
INSERT INTO course VALUES (8006, '软件测试技术', '选修', 2, 32, '软件学院', '测试设计与自动化测试');
INSERT INTO course VALUES (8007, '数据结构', '必修', 4, 32, '计算机学院', '线性结构、树和图');
INSERT INTO course VALUES (8008, '程序设计基础', '必修', 3, 32, '软件学院', '程序设计基本方法');

INSERT INTO section VALUES (8101, 8001, '2026-2027-1');
INSERT INTO section VALUES (8102, 8002, '2026-2027-1');
INSERT INTO section VALUES (8103, 8003, '2026-2027-1');
INSERT INTO section VALUES (8104, 8004, '2026-2027-1');
INSERT INTO section VALUES (8105, 8005, '2026-2027-1');
INSERT INTO section VALUES (8106, 8006, '2026-2027-1');
INSERT INTO section VALUES (8107, 8007, '2025-2026-2');
INSERT INTO section VALUES (8108, 8008, '2025-2026-2');

INSERT INTO teaching_class VALUES (8201, '数据库原理01班', 'T_DEMO_01', 30, 2, 8101);
INSERT INTO teaching_class VALUES (8202, '操作系统01班', 'T_DEMO_01', 30, 1, 8102);
INSERT INTO teaching_class VALUES (8203, 'Web应用开发01班', 'T_DEMO_02', 2, 1, 8103);
INSERT INTO teaching_class VALUES (8204, '人工智能导论01班', 'T_DEMO_03', 20, 0, 8104);
INSERT INTO teaching_class VALUES (8205, '计算机网络01班', 'T_DEMO_02', 30, 1, 8105);
INSERT INTO teaching_class VALUES (8206, '软件测试技术01班', 'T_DEMO_02', 20, 0, 8106);
INSERT INTO teaching_class VALUES (8207, '数据结构01班', 'T_DEMO_01', 30, 2, 8107);
INSERT INTO teaching_class VALUES (8208, '程序设计基础01班', 'T_DEMO_03', 30, 1, 8108);

INSERT INTO course_time VALUES (8301, 8201, 1, 1, 2, '1-16周', '教一-101');
INSERT INTO course_time VALUES (8302, 8202, 2, 3, 4, '1-16周', '教一-202');
INSERT INTO course_time VALUES (8303, 8203, 5, 5, 6, '1-16周', '实验楼-301');
INSERT INTO course_time VALUES (8304, 8204, 3, 7, 8, '1-16周', '教二-205');
INSERT INTO course_time VALUES (8305, 8205, 4, 3, 4, '1-16周', '教一-303');
INSERT INTO course_time VALUES (8306, 8206, 2, 7, 8, '1-16周', '实验楼-302');
INSERT INTO course_time VALUES (8307, 8207, 1, 3, 4, '1-16周', '教一-201');
INSERT INTO course_time VALUES (8308, 8208, 4, 1, 2, '1-16周', '实验楼-201');

INSERT INTO selection_batch VALUES (8501, '第一轮选课', SYSDATE-2, SYSDATE+5, 1);
INSERT INTO selection_batch VALUES (8502, '第二轮补选', SYSDATE+7, SYSDATE+14, 0);
INSERT INTO selection_batch VALUES (8503, '历史选课批次', SYSDATE-180, SYSDATE-170, 2);

INSERT INTO batch_class VALUES (8501, 8201, 1, SYSDATE-2);
INSERT INTO batch_class VALUES (8501, 8202, 1, SYSDATE-2);
INSERT INTO batch_class VALUES (8501, 8203, 1, SYSDATE-2);
INSERT INTO batch_class VALUES (8501, 8204, 1, SYSDATE-2);
INSERT INTO batch_class VALUES (8501, 8205, 1, SYSDATE-2);
INSERT INTO batch_class VALUES (8501, 8206, 1, SYSDATE-2);
INSERT INTO batch_class VALUES (8502, 8201, 1, SYSDATE);
INSERT INTO batch_class VALUES (8502, 8203, 1, SYSDATE);
INSERT INTO batch_class VALUES (8502, 8206, 1, SYSDATE);
INSERT INTO batch_class VALUES (8503, 8207, 1, SYSDATE-180);
INSERT INTO batch_class VALUES (8503, 8208, 1, SYSDATE-180);

INSERT INTO batch_class_scope VALUES (8601, 8501, 8201, '软件工程', '2026');
INSERT INTO batch_class_scope VALUES (8602, 8501, 8202, '软件工程', '2026');
INSERT INTO batch_class_scope VALUES (8603, 8501, 8203, '软件工程', '2026');
INSERT INTO batch_class_scope VALUES (8604, 8501, 8204, '计算机科学与技术', '2026');
INSERT INTO batch_class_scope VALUES (8605, 8501, 8205, '计算机科学与技术', '2026');
INSERT INTO batch_class_scope VALUES (8606, 8502, 8201, '软件工程', NULL);
INSERT INTO batch_class_scope VALUES (8607, 8502, 8203, NULL, '2026');

INSERT INTO course_select (select_id, class_id, batch_id, student_no) VALUES (8701, 8201, 8501, 'S_DEMO_01');
INSERT INTO course_select (select_id, class_id, batch_id, student_no) VALUES (8702, 8202, 8501, 'S_DEMO_01');
INSERT INTO course_select (select_id, class_id, batch_id, student_no) VALUES (8703, 8203, 8501, 'S_DEMO_01');
INSERT INTO course_select (select_id, class_id, batch_id, student_no) VALUES (8704, 8201, 8501, 'S_DEMO_02');
INSERT INTO course_select (select_id, class_id, batch_id, student_no) VALUES (8705, 8205, 8501, 'S_DEMO_02');
INSERT INTO course_select (select_id, class_id, batch_id, student_no) VALUES (8706, 8207, 8503, 'S_DEMO_01');
INSERT INTO course_select (select_id, class_id, batch_id, student_no) VALUES (8707, 8208, 8503, 'S_DEMO_01');
INSERT INTO course_select (select_id, class_id, batch_id, student_no) VALUES (8708, 8207, 8503, 'S_DEMO_03');

INSERT INTO student_score VALUES ('SCORE_DEMO_01', 'S_DEMO_01', 8207, 88, 'B+', 3.7, 4, SYSDATE-120, '首次录入', SYSDATE-120);
INSERT INTO student_score VALUES ('SCORE_DEMO_02', 'S_DEMO_01', 8208, 82, 'B', 3.0, 3, SYSDATE-120, '首次录入', SYSDATE-120);
INSERT INTO student_score VALUES ('SCORE_DEMO_03', 'S_DEMO_03', 8207, 92, 'A', 4.0, 4, SYSDATE-120, '首次录入', SYSDATE-120);

INSERT INTO course_evaluation VALUES ('EVAL_DEMO_01', 'S_DEMO_01', 8207, 5, 4, 5, 4, 4.5, '讲解清晰，案例可以再多一些。', SYSDATE-110);
INSERT INTO course_evaluation VALUES ('EVAL_DEMO_02', 'S_DEMO_03', 8207, 4, 5, 4, 5, 4.5, '课堂互动充分，重点突出。', SYSDATE-109);
INSERT INTO course_evaluation VALUES ('EVAL_DEMO_03', 'S_DEMO_01', 8208, 4, 4, 4, 4, 4.0, '整体学习效果良好。', SYSDATE-108);

INSERT INTO course_application (apply_id,teacher_no,course_name,course_type,credit,total_hours,department,course_summary,apply_time,status) VALUES ('APPLY_DEMO_PENDING','T_DEMO_01','软件工程实践','选修',2,32,'软件学院','包含需求、设计、开发与测试',SYSDATE-1,'待审核');
INSERT INTO course_application (apply_id,teacher_no,course_name,course_type,credit,total_hours,department,course_summary,apply_time,status,approve_time,approve_comment) VALUES ('APPLY_DEMO_APPROVED','T_DEMO_02','云计算基础','选修',2,32,'计算机学院','云平台基础与实践',SYSDATE-10,'通过',SYSDATE-9,'申请内容完整');
INSERT INTO course_application (apply_id,teacher_no,course_name,course_type,credit,total_hours,department,course_summary,apply_time,status,approve_time,approve_comment) VALUES ('APPLY_DEMO_REJECTED','T_DEMO_03','智能系统专题','选修',2,24,'人工智能学院','专题内容待完善',SYSDATE-8,'驳回',SYSDATE-7,'请补充教学计划');

INSERT INTO system_log VALUES ('LOG_DEMO_01',9301,'登录','管理员登录系统','A_DEMO_01','127.0.0.1',NULL,'成功',NULL,SYSDATE-1);
INSERT INTO system_log VALUES ('LOG_DEMO_02',9301,'修改容量','将 Web应用开发01班容量调整为2','8203','127.0.0.1','{"capacity":2,"reason":"测试满班状态"}','成功',NULL,SYSDATE-0.5);
INSERT INTO system_log VALUES ('LOG_DEMO_03',9302,'审批','驳回智能系统专题开课申请','APPLY_DEMO_REJECTED','127.0.0.1',NULL,'成功',NULL,SYSDATE-0.25);

COMMIT;
