MERGE INTO role r USING (SELECT 0 id, 'student' n, '学生' d FROM dual) s ON (r.role_id=s.id) WHEN NOT MATCHED THEN INSERT (role_id,role_name,role_desc) VALUES(s.id,s.n,s.d);
MERGE INTO role r USING (SELECT 1 id, 'teacher' n, '教师' d FROM dual) s ON (r.role_id=s.id) WHEN NOT MATCHED THEN INSERT (role_id,role_name,role_desc) VALUES(s.id,s.n,s.d);
MERGE INTO role r USING (SELECT 2 id, 'admin' n, '管理员' d FROM dual) s ON (r.role_id=s.id) WHEN NOT MATCHED THEN INSERT (role_id,role_name,role_desc) VALUES(s.id,s.n,s.d);
DECLARE
  PROCEDURE add_user(p_username VARCHAR2,p_role NUMBER,p_name VARCHAR2,p_no VARCHAR2,p_major VARCHAR2 DEFAULT NULL,p_grade VARCHAR2 DEFAULT NULL,p_title VARCHAR2 DEFAULT NULL,p_department VARCHAR2 DEFAULT NULL,p_level NUMBER DEFAULT NULL,p_scope VARCHAR2 DEFAULT NULL) IS
    v_id NUMBER;
  BEGIN
    SELECT user_id INTO v_id FROM "user" WHERE username=p_username;
  EXCEPTION WHEN NO_DATA_FOUND THEN
    BEGIN
      SELECT user_id_seq.NEXTVAL INTO v_id FROM dual;
      INSERT INTO "user" (user_id,username,password,role_id,real_name,phone,email,status,create_time)
      SELECT v_id,p_username,password,p_role,p_name,'1380000000' || SUBSTR(p_no,-1),LOWER(p_username)||'@test.edu.cn',1,SYSDATE FROM "user" WHERE username='T2026999';
      IF p_role=0 THEN INSERT INTO student (user_id,student_no,major,grade,avg_gpa,credit_finished) VALUES(v_id,p_no,p_major,p_grade,0,0); END IF;
      IF p_role=1 THEN INSERT INTO teacher (user_id,teacher_no,title,department) VALUES(v_id,p_no,p_title,p_department); END IF;
      IF p_role=2 THEN INSERT INTO administrator (user_id,admin_no,admin_level,managed_scope) VALUES(v_id,p_no,p_level,p_scope); END IF;
    END;
  END;
BEGIN
  add_user('S2026001',0,'张三','2026001','软件工程','2026'); add_user('S2026002',0,'李四','2026002','计算机科学与技术','2026');
  add_user('T2026001',1,'王老师','2026001',NULL,NULL,'讲师','计算机学院'); add_user('T2026002',1,'刘老师','2026002',NULL,NULL,'副教授','软件学院');
  add_user('A2026001',2,'测试管理员一','A2026001',NULL,NULL,NULL,NULL,0,'{"scope":"all"}'); add_user('A2026002',2,'测试管理员二','A2026002',NULL,NULL,NULL,NULL,1,'{"scope":"teaching"}');
END;
/
