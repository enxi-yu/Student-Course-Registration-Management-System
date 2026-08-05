using Oracle.ManagedDataAccess.Client;

namespace StudentCourse.Infrastructure
{
    public static class DbConnectionFactory
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string ConnectionString
        {
            get
            {
                string? connectionString = _configuration?.GetConnectionString("OracleConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("未在 appsettings.json 中找到 OracleConnection 连接字符串。");
                }

                return connectionString;
            }
        }

        public static OracleConnection CreateConnection()
        {
            return StudentCourse.Shared.Data.OracleConnections.Create(ConnectionString);
        }

        public static OracleConnection OpenConnection()
        {
            return StudentCourse.Shared.Data.OracleConnections.Open(ConnectionString);
        }

        public static DbConnectionTestResult TestConnection()
        {
            try
            {
                using OracleConnection connection = OpenConnection();
                using OracleCommand command = connection.CreateCommand();
                command.CommandText = "SELECT SYS_CONTEXT('USERENV', 'SERVER_HOST') AS server_host, SYS_CONTEXT('USERENV', 'CURRENT_USER') AS current_user FROM DUAL";

                using OracleDataReader reader = command.ExecuteReader();
                reader.Read();
                return new DbConnectionTestResult
                {
                    Success = true,
                    Message = "Oracle 连接成功",
                    ServerHost = Convert.ToString(reader["server_host"]),
                    CurrentUser = Convert.ToString(reader["current_user"])
                };
            }
            catch (Exception ex)
            {
                return new DbConnectionTestResult
                {
                    Success = false,
                    Error = "数据库连接失败"
                };
            }
        }
    }

    public sealed class DbConnectionTestResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ServerHost { get; set; }
        public string? CurrentUser { get; set; }
        public string? Error { get; set; }
    }
}
