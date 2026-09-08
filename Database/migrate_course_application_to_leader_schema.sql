-- 开课申请表迁移到组长新版字段。
-- 新字段：apply_id, teacher_no, course_name, course_type, credit, textbook,
--        course_summary, apply_time, status, approve_time, approve_comment
-- 说明：脚本会迁移旧字段中的数据，然后删除不再使用的旧字段。

SET DEFINE OFF;

DECLARE
    FUNCTION column_exists(p_column VARCHAR2) RETURN BOOLEAN IS
        v_count NUMBER;
    BEGIN
        SELECT COUNT(*)
          INTO v_count
          FROM user_tab_columns
         WHERE table_name = 'COURSE_APPLICATION'
           AND column_name = UPPER(p_column);

        RETURN v_count > 0;
    END;

    PROCEDURE add_column_if_missing(p_column VARCHAR2, p_sql VARCHAR2) IS
    BEGIN
        IF NOT column_exists(p_column) THEN
            EXECUTE IMMEDIATE p_sql;
        END IF;
    END;

    PROCEDURE drop_column_if_exists(p_column VARCHAR2) IS
    BEGIN
        IF column_exists(p_column) THEN
            EXECUTE IMMEDIATE 'ALTER TABLE course_application DROP COLUMN ' || p_column;
        END IF;
    END;

    PROCEDURE drop_trigger_if_exists(p_trigger VARCHAR2) IS
        v_count NUMBER;
    BEGIN
        SELECT COUNT(*)
          INTO v_count
          FROM user_triggers
         WHERE trigger_name = UPPER(p_trigger);

        IF v_count > 0 THEN
            EXECUTE IMMEDIATE 'DROP TRIGGER ' || p_trigger;
        END IF;
    END;
BEGIN
    add_column_if_missing('course_type', 'ALTER TABLE course_application ADD (course_type VARCHAR2(20))');
    add_column_if_missing('textbook', 'ALTER TABLE course_application ADD (textbook VARCHAR2(200))');
    add_column_if_missing('course_summary', 'ALTER TABLE course_application ADD (course_summary CLOB)');
    add_column_if_missing('approve_comment', 'ALTER TABLE course_application ADD (approve_comment VARCHAR2(255))');

    IF column_exists('target_grade') THEN
        EXECUTE IMMEDIATE q'[
            UPDATE course_application
               SET textbook = target_grade
             WHERE textbook IS NULL
               AND target_grade IS NOT NULL
        ]';
    END IF;

    IF column_exists('description') THEN
        EXECUTE IMMEDIATE q'[
            UPDATE course_application
               SET course_summary = DBMS_LOB.SUBSTR(description, 4000, 1)
             WHERE course_summary IS NULL
               AND description IS NOT NULL
        ]';
    END IF;

    IF column_exists('teaching_plan') THEN
        EXECUTE IMMEDIATE q'[
            UPDATE course_application
               SET course_summary = DBMS_LOB.SUBSTR(teaching_plan, 4000, 1)
             WHERE course_summary IS NULL
               AND teaching_plan IS NOT NULL
        ]';
    END IF;

    IF column_exists('review_remark') THEN
        EXECUTE IMMEDIATE q'[
            UPDATE course_application
               SET approve_comment = review_remark
             WHERE approve_comment IS NULL
               AND review_remark IS NOT NULL
        ]';
    END IF;

    drop_trigger_if_exists('trg_course_app_summary');

    drop_column_if_exists('total_hours');
    drop_column_if_exists('department');
    drop_column_if_exists('teaching_plan');
    drop_column_if_exists('description');
    drop_column_if_exists('target_major');
    drop_column_if_exists('target_grade');
    drop_column_if_exists('review_remark');
END;
/

COMMIT;
