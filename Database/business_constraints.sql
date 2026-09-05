-- Run after duplicate legacy data has been cleaned. Safe to rerun.
-- Enforces business uniqueness and adds indexes used by scheduling conflict checks.
DECLARE
  duplicate_count NUMBER;
  constraint_count NUMBER;

  PROCEDURE add_constraint_if_missing(constraint_name_in VARCHAR2, ddl_in VARCHAR2) IS
  BEGIN
    SELECT COUNT(*) INTO constraint_count
      FROM user_constraints
     WHERE constraint_name = UPPER(constraint_name_in);
    IF constraint_count = 0 THEN
      EXECUTE IMMEDIATE ddl_in;
    END IF;
  END;
BEGIN
  SELECT COUNT(*) INTO duplicate_count
    FROM (SELECT course_id, semester FROM section GROUP BY course_id, semester HAVING COUNT(*) > 1);
  IF duplicate_count > 0 THEN
    RAISE_APPLICATION_ERROR(-20001, 'SECTION contains duplicate course/semester rows; clean them before adding UQ_SECTION_COURSE_SEMESTER.');
  END IF;

  SELECT COUNT(*) INTO duplicate_count
    FROM (SELECT section_id, class_name FROM teaching_class GROUP BY section_id, class_name HAVING COUNT(*) > 1);
  IF duplicate_count > 0 THEN
    RAISE_APPLICATION_ERROR(-20002, 'TEACHING_CLASS contains duplicate names in one section; clean them before adding UQ_TEACHING_CLASS_NAME.');
  END IF;

  add_constraint_if_missing(
    'UQ_SECTION_COURSE_SEMESTER',
    'ALTER TABLE section ADD CONSTRAINT uq_section_course_semester UNIQUE (course_id, semester)');
  add_constraint_if_missing(
    'UQ_TEACHING_CLASS_NAME',
    'ALTER TABLE teaching_class ADD CONSTRAINT uq_teaching_class_name UNIQUE (section_id, class_name)');
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE INDEX ix_section_semester ON section (semester)';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE INDEX ix_teaching_class_teacher_section ON teaching_class (teacher_no, section_id)';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE INDEX ix_course_time_conflict ON course_time (weekday, start_period, end_period, classroom, class_id)';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

COMMIT;
