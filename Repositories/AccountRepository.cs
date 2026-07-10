using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using StudentCourse.Infrastructure;

namespace StudentCourse.Repositories
{
    public sealed class AccountRepository
    {
        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            const string sql = @"
                UPDATE ""user""
                   SET password = :newPassword
                 WHERE user_id = :userId
                   AND password = :oldPassword";

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                command.BindByName = true;
                command.Parameters.Add("newPassword", OracleDbType.Varchar2).Value = newPassword;
                command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                command.Parameters.Add("oldPassword", OracleDbType.Varchar2).Value = oldPassword;

                return command.ExecuteNonQuery() > 0;
            }
        }
    }
}
