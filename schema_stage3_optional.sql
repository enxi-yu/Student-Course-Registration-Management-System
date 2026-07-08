-- 第三阶段可选结构增强脚本
-- 请在确认测试库后手动执行。程序不会自动执行本脚本。

DECLARE
  v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count
    FROM user_tab_columns
   WHERE table_name = 'COURSE_APPLICATION'
     AND column_name = 'COURSE_TYPE';

  IF v_count = 0 THEN
    EXECUTE IMMEDIATE 'ALTER TABLE course_application ADD course_type VARCHAR2(20)';
  END IF;

  SELECT COUNT(*) INTO v_count
    FROM user_tab_columns
   WHERE table_name = 'COURSE_APPLICATION'
     AND column_name = 'TARGET_MAJOR';

  IF v_count = 0 THEN
    EXECUTE IMMEDIATE 'ALTER TABLE course_application ADD target_major VARCHAR2(50)';
  END IF;

  SELECT COUNT(*) INTO v_count
    FROM user_tab_columns
   WHERE table_name = 'COURSE_APPLICATION'
     AND column_name = 'TARGET_GRADE';

  IF v_count = 0 THEN
    EXECUTE IMMEDIATE 'ALTER TABLE course_application ADD target_grade VARCHAR2(20)';
  END IF;

  SELECT COUNT(*) INTO v_count
    FROM user_tab_columns
   WHERE table_name = 'COURSE_APPLICATION'
     AND column_name = 'DESCRIPTION';

  IF v_count = 0 THEN
    EXECUTE IMMEDIATE 'ALTER TABLE course_application ADD description CLOB';
  END IF;

  SELECT COUNT(*) INTO v_count
    FROM user_tab_columns
   WHERE table_name = 'COURSE_APPLICATION'
     AND column_name = 'REVIEW_REMARK';

  IF v_count = 0 THEN
    EXECUTE IMMEDIATE 'ALTER TABLE course_application ADD review_remark VARCHAR2(255)';
  END IF;
END;
/

-- 建议唯一约束：避免同一学生在同一教学班出现多条成绩。
-- 如果已有重复数据，本语句会失败，需要先清理重复记录。
DECLARE
  v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count
    FROM user_constraints
   WHERE table_name = 'STUDENT_SCORE'
     AND constraint_name = 'UQ_STUDENT_SCORE';

  IF v_count = 0 THEN
    EXECUTE IMMEDIATE 'ALTER TABLE student_score ADD CONSTRAINT uq_student_score UNIQUE (student_no, class_id)';
  END IF;
END;
/
