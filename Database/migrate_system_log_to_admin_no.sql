-- 将系统日志的操作人标识从用户ID迁移为管理员工号（Oracle 19c）。
-- 执行前提：现有 system_log.user_id 均能关联 administrator.user_id。

ALTER TABLE system_log ADD (admin_no VARCHAR2(20));

UPDATE system_log l
   SET admin_no = (
       SELECT a.admin_no
         FROM administrator a
        WHERE a.user_id = l.user_id
   );

ALTER TABLE system_log MODIFY (admin_no NOT NULL);

ALTER TABLE system_log
  ADD CONSTRAINT fk_system_log_admin
  FOREIGN KEY (admin_no)
  REFERENCES administrator(admin_no);

ALTER TABLE system_log DROP COLUMN user_id;

COMMIT;
