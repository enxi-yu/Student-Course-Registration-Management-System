using Oracle.ManagedDataAccess.Client;

namespace StudentCourse.Shared.Data;

public static class OracleConnections
{
    private static string Normalize(string connectionString)
    {
        var builder = new OracleConnectionStringBuilder(connectionString)
        {
            ValidateConnection = true,
            ConnectionTimeout = 5
        };
        return builder.ConnectionString;
    }

    public static OracleConnection Create(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("OracleConnection connection string is not configured.");
        }

        return new OracleConnection(Normalize(connectionString));
    }

    public static OracleConnection Open(string connectionString)
    {
        OracleConnection connection = Create(connectionString);
        try
        {
            connection.Open();
            return connection;
        }
        catch (OracleException)
        {
            try { OracleConnection.ClearPool(connection); } catch { }
            connection.Dispose();

            // 连接池中的旧连接失效时，清理后仅重试一次。
            connection = Create(connectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }
    }
}
