-- 课程总学时统一由教学班排课自动计算：周数 × 每周节数。
-- week_range 支持“1-16周”和“第8周”等包含一个或两个数字的格式。
CREATE OR REPLACE TRIGGER trg_sync_course_total_hours
AFTER INSERT OR UPDATE OR DELETE ON course_time
BEGIN
    UPDATE course c
       SET c.total_hours = NVL((
           SELECT MAX(SUM(
               (ct.end_period - ct.start_period + 1)
               * (
                   NVL(
                       TO_NUMBER(REGEXP_SUBSTR(ct.week_range, '[0-9]+', 1, 2)),
                       TO_NUMBER(REGEXP_SUBSTR(ct.week_range, '[0-9]+', 1, 1))
                   )
                   - TO_NUMBER(REGEXP_SUBSTR(ct.week_range, '[0-9]+', 1, 1))
                   + 1
               ))
           )
             FROM section s
             JOIN teaching_class tc ON tc.section_id = s.section_id
             JOIN course_time ct ON ct.class_id = tc.class_id
            WHERE s.course_id = c.course_id
            GROUP BY tc.class_id
       ), 0);
END;
/

-- 立即按照现有课表重算全部课程，清除原有手工学时。
UPDATE course c
   SET c.total_hours = NVL((
       SELECT MAX(SUM(
           (ct.end_period - ct.start_period + 1)
           * (
               NVL(
                   TO_NUMBER(REGEXP_SUBSTR(ct.week_range, '[0-9]+', 1, 2)),
                   TO_NUMBER(REGEXP_SUBSTR(ct.week_range, '[0-9]+', 1, 1))
               )
               - TO_NUMBER(REGEXP_SUBSTR(ct.week_range, '[0-9]+', 1, 1))
               + 1
           ))
       )
         FROM section s
         JOIN teaching_class tc ON tc.section_id = s.section_id
         JOIN course_time ct ON ct.class_id = tc.class_id
        WHERE s.course_id = c.course_id
        GROUP BY tc.class_id
   ), 0);

COMMIT;
