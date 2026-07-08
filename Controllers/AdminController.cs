using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace StudentCourse.Controllers
{
    // 定义和前端交互的数据结构
    public class AdminCourseModel
    {
        public string c_name { get; set; } = string.Empty;
        public string c_credit { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        // 直接读取她们配好的标准连接字符串（从你的 appsettings.json 自动匹配）
        private readonly string _connectionString = "User Id=course;Password=Course2026;Data Source=47.116.104.63:1521/COURSEPDB;";

        // 1. 真实查询 Oracle 数据库
        [HttpGet("courses")]
        public IActionResult GetCourses()
        {
            var list = new List<AdminCourseModel>();
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();
                    // 注意：这里的表名（course）和字段名（c_name, c_credit）要和你们的 .sql 文件完全对应
                    string sql = "SELECT c_name, c_credit FROM course";
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new AdminCourseModel
                            {
                                c_name = reader["c_name"].ToString() ?? "",
                                c_credit = reader["c_credit"].ToString() ?? ""
                            });
                        }
                    }
                }
                return Ok(list);
            }
            catch (Exception ex)
            {
                // 如果数据库又连不上了，后端会返回 500 并告诉你具体原因（如网络超时、凭据错误）
                return StatusCode(500, $"数据库连接失败: {ex.Message}");
            }
        }

        // 2. 真实向 Oracle 数据库插入数据
        [HttpPost("courses")]
        public IActionResult AddCourse([FromBody] AdminCourseModel input)
        {
            if (input == null || string.IsNullOrEmpty(input.c_name))
            {
                return BadRequest("参数不能为空");
            }

            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();
                    // 使用参数化查询防止 SQL 注入
                    string sql = "INSERT INTO course (c_name, c_credit) VALUES (:name, :credit)";
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("name", input.c_name));
                        cmd.Parameters.Add(new OracleParameter("credit", input.c_credit));
                        cmd.ExecuteNonQuery();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"写入数据库失败: {ex.Message}");
            }
        }
    }
}