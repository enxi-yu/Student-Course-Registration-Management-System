-- Run once against the Oracle schema before deploying the sequence-based code.
-- This script creates missing sequences and advances existing ones past current table IDs.
DECLARE
  next_value NUMBER;

  PROCEDURE ensure_sequence(name_in VARCHAR2, minimum_next_value NUMBER) IS
    current_value NUMBER;
    increment_value NUMBER;
  BEGIN
    BEGIN
      EXECUTE IMMEDIATE 'CREATE SEQUENCE ' || name_in || ' START WITH ' || minimum_next_value || ' INCREMENT BY 1 NOCACHE';
    EXCEPTION
      WHEN OTHERS THEN
        IF SQLCODE != -955 THEN
          RAISE;
        END IF;
    END;

    -- NEXTVAL also works for a newly created sequence. One skipped value is harmless.
    EXECUTE IMMEDIATE 'SELECT ' || name_in || '.NEXTVAL FROM dual' INTO current_value;
    IF current_value < minimum_next_value THEN
      increment_value := minimum_next_value - current_value;
      EXECUTE IMMEDIATE 'ALTER SEQUENCE ' || name_in || ' INCREMENT BY ' || increment_value;
      EXECUTE IMMEDIATE 'SELECT ' || name_in || '.NEXTVAL FROM dual' INTO current_value;
      EXECUTE IMMEDIATE 'ALTER SEQUENCE ' || name_in || ' INCREMENT BY 1';
    END IF;
  END;

BEGIN
  EXECUTE IMMEDIATE 'SELECT NVL(MAX(user_id), 0) + 1 FROM "user"' INTO next_value;
  ensure_sequence('USER_ID_SEQ', next_value);

  EXECUTE IMMEDIATE 'SELECT NVL(MAX(course_id), 0) + 1 FROM course' INTO next_value;
  ensure_sequence('COURSE_ID_SEQ', next_value);

  EXECUTE IMMEDIATE 'SELECT NVL(MAX(select_id), 0) + 1 FROM course_select' INTO next_value;
  ensure_sequence('COURSE_SELECT_ID_SEQ', next_value);

  EXECUTE IMMEDIATE 'SELECT NVL(MAX(batch_id), 0) + 1 FROM selection_batch' INTO next_value;
  ensure_sequence('SELECTION_BATCH_ID_SEQ', next_value);

  EXECUTE IMMEDIATE 'SELECT NVL(MAX(section_id), 0) + 1 FROM section' INTO next_value;
  ensure_sequence('SECTION_ID_SEQ', next_value);

  EXECUTE IMMEDIATE 'SELECT NVL(MAX(class_id), 0) + 1 FROM teaching_class' INTO next_value;
  ensure_sequence('TEACHING_CLASS_ID_SEQ', next_value);

  EXECUTE IMMEDIATE 'SELECT NVL(MAX(time_id), 0) + 1 FROM course_time' INTO next_value;
  ensure_sequence('COURSE_TIME_ID_SEQ', next_value);
END;
/
