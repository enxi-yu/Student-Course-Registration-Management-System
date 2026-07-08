-- ===================================================
-- 学生选课管理系统 - 管理员模块数据库初始化脚本
-- 作用：创建管理员端专属的课程管理表和选课批次管理表
-- ===================================================

-- 创建管理员专属课程表
CREATE TABLE ADMIN_COURSE (
    course_name VARCHAR2(100) NOT NULL, -- 课程名称
    credit      NUMBER NOT NULL         -- 学分
);

-- 创建管理员专属选课批次表
CREATE TABLE ADMIN_BATCH (
    start_time  VARCHAR2(50) NOT NULL,  -- 选课开始时间
    end_time    VARCHAR2(50) NOT NULL   -- 选课结束时间
);

