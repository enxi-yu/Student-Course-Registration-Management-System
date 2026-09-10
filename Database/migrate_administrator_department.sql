-- 管理员学院化兼容迁移：新增 department，同时保留旧版 managed_scope。
-- 本脚本不会修改课程、教师、学生、选课或批次数据，可重复执行。

DECLARE
    column_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO column_count
      FROM user_tab_columns
     WHERE table_name = 'ADMINISTRATOR'
       AND column_name = 'DEPARTMENT';

    IF column_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE administrator ADD (department VARCHAR2(100))';
    END IF;
END;
/

-- 不自动给普通管理员填写虚构学院，也不删除 managed_scope。
-- department 为空时，新版系统会阻止普通管理员越权操作并提示先配置学院。

SELECT a.admin_no,
       u.real_name,
       a.admin_level,
       CASE
           WHEN a.admin_level = 0 THEN '全校'
           ELSE NVL(a.department, '未分配')
       END AS department
  FROM administrator a
  JOIN "user" u ON u.user_id = a.user_id
 ORDER BY a.admin_level, a.admin_no;
