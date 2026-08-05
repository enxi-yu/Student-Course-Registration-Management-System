-- Required before storing PBKDF2 password hashes.
ALTER TABLE "user" MODIFY ("PASSWORD" VARCHAR2(512))
