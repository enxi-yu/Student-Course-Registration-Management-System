using Oracle.ManagedDataAccess.Client;

namespace StudentCourse.Shared.Data;

public static class OracleConnections
{
    public static OracleConnection Create(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("OracleConnection connection string is not configured.");
        }

        return new OracleConnection(connectionString);
    }

    public static OracleConnection Open(string connectionString)
    {
        OracleConnection connection = Create(connectionString);
        connection.Open();
        return connection;
    }
}
