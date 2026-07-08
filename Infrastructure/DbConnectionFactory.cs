using System;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace StudentCourse.Infrastructure
{
    public static class DbConnectionFactory
    {
        public static string ConnectionString
        {
            get
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["OracleConnection"];
                if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                {
                    throw new InvalidOperationException("未在 App.config 中找到 OracleConnection 连接字符串。");
                }

                return settings.ConnectionString;
            }
        }

        public static OracleConnection CreateConnection()
        {
            return new OracleConnection(ConnectionString);
        }

        public static OracleConnection OpenConnection()
        {
            OracleConnection connection = CreateConnection();
            connection.Open();
            return connection;
        }

        public static DbConnectionTestResult TestConnection()
        {
            try
            {
                using (OracleConnection connection = OpenConnection())
                using (OracleCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT SYS_CONTEXT('USERENV', 'SERVER_HOST') AS server_host, SYS_CONTEXT('USERENV', 'CURRENT_USER') AS current_user FROM DUAL";

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        return new DbConnectionTestResult
                        {
                            Success = true,
                            Message = "Oracle 连接成功",
                            ServerHost = Convert.ToString(reader["server_host"]),
                            CurrentUser = Convert.ToString(reader["current_user"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new DbConnectionTestResult
                {
                    Success = false,
                    Error = ex.ToString()
                };
            }
        }
    }

    public sealed class DbConnectionTestResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string ServerHost { get; set; }

        public string CurrentUser { get; set; }

        public string Error { get; set; }
    }
}
