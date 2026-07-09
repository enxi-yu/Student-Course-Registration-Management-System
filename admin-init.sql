CREATE TABLE course (
    course_id    INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    course_name  VARCHAR2(20)  NOT NULL,
    course_type  VARCHAR2(20)  NOT NULL,
    credit       DECIMAL(3,1)  NOT NULL CHECK (credit >= 0),
    total_hours  INTEGER       NOT NULL CHECK (total_hours >= 0),
    department   VARCHAR2(100),
    course_desc  TEXT
);