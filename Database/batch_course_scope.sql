-- 选课批次发布课程及适用范围（Oracle 19c）
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE batch_class (
    batch_id NUMBER NOT NULL,
    class_id NUMBER NOT NULL,
    enabled NUMBER(1) DEFAULT 1 NOT NULL,
    create_time DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT pk_batch_class PRIMARY KEY (batch_id, class_id),
    CONSTRAINT fk_bc_batch FOREIGN KEY (batch_id) REFERENCES selection_batch(batch_id) ON DELETE CASCADE,
    CONSTRAINT fk_bc_class FOREIGN KEY (class_id) REFERENCES teaching_class(class_id) ON DELETE CASCADE,
    CONSTRAINT ck_bc_enabled CHECK (enabled IN (0,1))
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE batch_class_scope (
    scope_id NUMBER NOT NULL,
    batch_id NUMBER NOT NULL,
    class_id NUMBER NOT NULL,
    major VARCHAR2(100),
    grade VARCHAR2(20),
    CONSTRAINT pk_batch_class_scope PRIMARY KEY (scope_id),
    CONSTRAINT fk_bcs_batch_class FOREIGN KEY (batch_id, class_id)
      REFERENCES batch_class(batch_id, class_id) ON DELETE CASCADE,
    CONSTRAINT ck_bcs_value CHECK (major IS NOT NULL OR grade IS NOT NULL)
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE SEQUENCE batch_class_scope_id_seq START WITH 1 INCREMENT BY 1 NOCACHE';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN EXECUTE IMMEDIATE 'CREATE INDEX idx_bc_class ON batch_class(class_id)';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;
/
BEGIN EXECUTE IMMEDIATE 'CREATE INDEX idx_bcs_match ON batch_class_scope(batch_id, class_id, major, grade)';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;
/

-- 兼容现有 Demo：已有批次默认发布同时间范围内涉及学期的全部教学班。
MERGE INTO batch_class target
USING (SELECT b.batch_id, tc.class_id
         FROM selection_batch b CROSS JOIN teaching_class tc) source
ON (target.batch_id = source.batch_id AND target.class_id = source.class_id)
WHEN NOT MATCHED THEN INSERT (batch_id, class_id, enabled, create_time)
VALUES (source.batch_id, source.class_id, 1, SYSDATE);

COMMIT;
