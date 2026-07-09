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
            return new OracleConnection(ConnectionString);
        }

        public static OracleConnection OpenConnection()
        {
            List<string> errors = new List<string>();

            foreach (string connectionString in GetConnectionStringCandidates())
            {
                OracleConnection connection = new OracleConnection(connectionString);
                try
                {
                    connection.Open();
                    return connection;
                }
                catch (Exception ex)
                {
                    connection.Dispose();
                    errors.Add(ex.Message);
                }
            }

            throw new InvalidOperationException("Oracle 连接失败。已尝试 TNS 描述符和 EZCONNECT 格式。" + string.Join(" | ", errors));
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
                    Message = "Oracle 连接失败，请检查服务名、账号密码或数据库监听状态。",
                    Error = ex.Message
                };
            }
        }

        private static IEnumerable<string> GetConnectionStringCandidates()
        {
            yield return ConnectionString;
            yield return "User Id=course;Password=Course2026;Data Source=47.116.104.63:1521/COURSEPDB;Connection Timeout=15;";
            yield return "User Id=course;Password=Course2026;Data Source=//47.116.104.63:1521/COURSEPDB;Connection Timeout=15;";
            yield return "User Id=course;Password=Course2026;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=47.116.104.63)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=COURSEPDB)));Connection Timeout=15;";
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
