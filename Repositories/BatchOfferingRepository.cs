using Oracle.ManagedDataAccess.Client;
using StudentCourse.Infrastructure;
using StudentCourse.Models;

namespace StudentCourse.Repositories;

public sealed class BatchOfferingRepository
{
    public IList<BatchOfferingDto> Get(int batchId)
    {
        const string sql = @"
            SELECT tc.class_id, tc.class_name, c.course_name, s.semester,
                   NVL(u.real_name, '未分配') teacher_name,
                   CASE WHEN bc.class_id IS NULL THEN 0 ELSE 1 END selected,
                   (SELECT LISTAGG(x.major, ',') WITHIN GROUP (ORDER BY x.major)
                      FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.major IS NOT NULL) majors,
                   (SELECT LISTAGG(x.grade, ',') WITHIN GROUP (ORDER BY x.grade)
                      FROM batch_class_scope x WHERE x.batch_id=bc.batch_id AND x.class_id=bc.class_id AND x.grade IS NOT NULL) grades
              FROM teaching_class tc
              JOIN section s ON s.section_id=tc.section_id
              JOIN course c ON c.course_id=s.course_id
              LEFT JOIN teacher t ON t.teacher_no=tc.teacher_no
              LEFT JOIN ""user"" u ON u.user_id=t.user_id
              LEFT JOIN batch_class bc ON bc.class_id=tc.class_id AND bc.batch_id=:batchId AND bc.enabled=1
             ORDER BY s.semester DESC, c.course_name, tc.class_name";
        var rows = new List<BatchOfferingDto>();
        using OracleConnection connection = DbConnectionFactory.OpenConnection();
        using OracleCommand command = new(sql, connection) { BindByName = true };
        command.Parameters.Add("batchId", OracleDbType.Int32).Value = batchId;
        using OracleDataReader reader = command.ExecuteReader();
        while (reader.Read()) rows.Add(new BatchOfferingDto {
            ClassId=Convert.ToInt32(reader["class_id"]), ClassName=S(reader["class_name"]), CourseName=S(reader["course_name"]),
            Semester=S(reader["semester"]), TeacherName=S(reader["teacher_name"]), Selected=Convert.ToInt32(reader["selected"])==1,
            Majors=Split(S(reader["majors"])), Grades=Split(S(reader["grades"])) });
        return rows;
    }

    public void Save(int batchId, IList<BatchOfferingInput> offerings)
    {
        using OracleConnection connection = DbConnectionFactory.OpenConnection();
        using OracleTransaction tx = connection.BeginTransaction();
        try {
            using (OracleCommand delete = C(connection, "DELETE FROM batch_class WHERE batch_id=:batchId", tx)) {
                delete.Parameters.Add("batchId", OracleDbType.Int32).Value=batchId; delete.ExecuteNonQuery(); }
            foreach (BatchOfferingInput item in offerings.GroupBy(x=>x.ClassId).Select(x=>x.First())) {
                using (OracleCommand insert = C(connection, "INSERT INTO batch_class(batch_id,class_id,enabled,create_time) VALUES(:batchId,:classId,1,SYSDATE)", tx)) {
                    insert.Parameters.Add("batchId", OracleDbType.Int32).Value=batchId; insert.Parameters.Add("classId", OracleDbType.Int32).Value=item.ClassId; insert.ExecuteNonQuery(); }
                foreach (string major in Clean(item.Majors)) InsertScope(connection,tx,batchId,item.ClassId,major,null);
                foreach (string grade in Clean(item.Grades)) InsertScope(connection,tx,batchId,item.ClassId,null,grade);
            }
            tx.Commit();
        } catch { tx.Rollback(); throw; }
    }

    private static void InsertScope(OracleConnection c, OracleTransaction t, int batchId, int classId, string? major, string? grade) {
        using OracleCommand cmd=C(c,"INSERT INTO batch_class_scope(scope_id,batch_id,class_id,major,grade) VALUES(batch_class_scope_id_seq.NEXTVAL,:batchId,:classId,:major,:grade)",t);
        cmd.Parameters.Add("batchId",OracleDbType.Int32).Value=batchId; cmd.Parameters.Add("classId",OracleDbType.Int32).Value=classId;
        cmd.Parameters.Add("major",OracleDbType.Varchar2).Value=(object?)major??DBNull.Value; cmd.Parameters.Add("grade",OracleDbType.Varchar2).Value=(object?)grade??DBNull.Value; cmd.ExecuteNonQuery(); }
    private static OracleCommand C(OracleConnection c,string sql,OracleTransaction t)=>new(sql,c){BindByName=true,Transaction=t};
    private static string S(object v)=>v==DBNull.Value?"":Convert.ToString(v)??"";
    private static IList<string> Split(string v)=>string.IsNullOrWhiteSpace(v)?new List<string>():v.Split(',').Select(x=>x.Trim()).Where(x=>x.Length>0).Distinct().ToList();
    private static IEnumerable<string> Clean(IEnumerable<string>? xs)=>(xs??Array.Empty<string>()).SelectMany(x=>x.Split(',', '，')).Select(x=>x.Trim()).Where(x=>x.Length>0).Distinct();
}
