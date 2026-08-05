using Oracle.ManagedDataAccess.Client;
using StudentCourse.Infrastructure;

namespace StudentCourse.Repositories;

public sealed class AccountRepository
{
    public string? GetPasswordHash(int userId)
    {
        const string sql = @"SELECT password FROM ""user"" WHERE user_id = :userId";
        using OracleConnection connection = DbConnectionFactory.OpenConnection();
        using OracleCommand command = new OracleCommand(sql, connection) { BindByName = true };
        command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
        object? value = command.ExecuteScalar();
        return value is null or DBNull ? null : Convert.ToString(value);
    }

    public bool UpdatePassword(int userId, string passwordHash)
    {
        const string sql = @"UPDATE ""user"" SET password = :password WHERE user_id = :userId";
        using OracleConnection connection = DbConnectionFactory.OpenConnection();
        using OracleCommand command = new OracleCommand(sql, connection) { BindByName = true };
        command.Parameters.Add("password", OracleDbType.Varchar2).Value = passwordHash;
        command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
        return command.ExecuteNonQuery() == 1;
    }
}
