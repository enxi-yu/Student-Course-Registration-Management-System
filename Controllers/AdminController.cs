using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using StudentCourse.Infrastructure;
using StudentCourse.Models;
using System;
using System.Collections.Generic;

namespace StudentCourse.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        [HttpGet("courses")]
        public IActionResult GetCourses()
        {
            var list = new List<CourseDto>();
            try
            {
                using (var conn = DbConnectionFactory.OpenConnection())
                {
                    string sql = "SELECT course_id, course_name, course_type, credit, total_hours, department, course_desc FROM course";
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new CourseDto
                            {
                                CourseId = Convert.ToInt32(reader["course_id"]),
                                CourseName = reader["course_name"].ToString() ?? "",
                                CourseType = reader["course_type"].ToString() ?? "",
                                Credit = Convert.ToDecimal(reader["credit"]),
                                TotalHours = Convert.ToInt32(reader["total_hours"]),
                                Department = reader["department"]?.ToString() ?? "",
                                CourseDesc = reader["course_desc"]?.ToString() ?? ""
                            });
                        }
                    }
                }
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"数据库连接失败: {ex.Message}");
            }
        }

        [HttpGet("courses/{id}")]
        public IActionResult GetCourse(int id)
        {
            try
            {
                using (var conn = DbConnectionFactory.OpenConnection())
                {
                    string sql = "SELECT course_id, course_name, course_type, credit, total_hours, department, course_desc FROM course WHERE course_id = :id";
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("id", id));
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new CourseDto
                                {
                                    CourseId = Convert.ToInt32(reader["course_id"]),
                                    CourseName = reader["course_name"].ToString() ?? "",
                                    CourseType = reader["course_type"].ToString() ?? "",
                                    Credit = Convert.ToDecimal(reader["credit"]),
                                    TotalHours = Convert.ToInt32(reader["total_hours"]),
                                    Department = reader["department"]?.ToString() ?? "",
                                    CourseDesc = reader["course_desc"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
                return NotFound("课程不存在");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"数据库连接失败: {ex.Message}");
            }
        }

        [HttpPost("courses")]
        public IActionResult AddCourse([FromBody] CourseDto input)
        {
            if (input == null || string.IsNullOrEmpty(input.CourseName))
            {
                return BadRequest("课程名称不能为空");
            }

            if (string.IsNullOrEmpty(input.CourseType))
            {
                return BadRequest("课程类型不能为空");
            }

            try
            {
                using (var conn = DbConnectionFactory.OpenConnection())
                {
                    string maxIdSql = "SELECT NVL(MAX(course_id), 0) FROM course";
                    using (var maxCmd = new OracleCommand(maxIdSql, conn))
                    {
                        int newId = Convert.ToInt32(maxCmd.ExecuteScalar()) + 1;

                        string sql = "INSERT INTO course (course_id, course_name, course_type, credit, total_hours, department, course_desc) VALUES (:id, :name, :type, :credit, :hours, :dept, :cdesc)";
                        using (var cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(new OracleParameter("id", newId));
                            cmd.Parameters.Add(new OracleParameter("name", input.CourseName));
                            cmd.Parameters.Add(new OracleParameter("type", input.CourseType));
                            cmd.Parameters.Add(new OracleParameter("credit", input.Credit));
                            cmd.Parameters.Add(new OracleParameter("hours", input.TotalHours));
                            cmd.Parameters.Add(new OracleParameter("dept", string.IsNullOrEmpty(input.Department) ? DBNull.Value : (object)input.Department));
                            cmd.Parameters.Add(new OracleParameter("cdesc", string.IsNullOrEmpty(input.CourseDesc) ? DBNull.Value : (object)input.CourseDesc));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"写入数据库失败: {ex.Message}");
            }
        }

        [HttpPut("courses/{id}")]
        public IActionResult UpdateCourse(int id, [FromBody] CourseDto input)
        {
            if (input == null || string.IsNullOrEmpty(input.CourseName))
            {
                return BadRequest("课程名称不能为空");
            }

            if (string.IsNullOrEmpty(input.CourseType))
            {
                return BadRequest("课程类型不能为空");
            }

            try
            {
                using (var conn = DbConnectionFactory.OpenConnection())
                {
                    string sql = "UPDATE course SET course_name = :name, course_type = :type, credit = :credit, total_hours = :hours, department = :dept, course_desc = :cdesc WHERE course_id = :id";
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("name", input.CourseName));
                        cmd.Parameters.Add(new OracleParameter("type", input.CourseType));
                        cmd.Parameters.Add(new OracleParameter("credit", input.Credit));
                        cmd.Parameters.Add(new OracleParameter("hours", input.TotalHours));
                        cmd.Parameters.Add(new OracleParameter("dept", string.IsNullOrEmpty(input.Department) ? DBNull.Value : (object)input.Department));
                        cmd.Parameters.Add(new OracleParameter("cdesc", string.IsNullOrEmpty(input.CourseDesc) ? DBNull.Value : (object)input.CourseDesc));
                        cmd.Parameters.Add(new OracleParameter("id", id));
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            return NotFound("课程不存在");
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"更新数据库失败: {ex.Message}");
            }
        }

        [HttpDelete("courses/{id}")]
        public IActionResult DeleteCourse(int id)
        {
            try
            {
                using (var conn = DbConnectionFactory.OpenConnection())
                {
                    try
                    {
                        string checkSql = "SELECT COUNT(*) FROM section WHERE course_id = :id";
                        using (var checkCmd = new OracleCommand(checkSql, conn))
                        {
                            checkCmd.Parameters.Add(new OracleParameter("id", id));
                            int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                            if (count > 0)
                            {
                                return BadRequest($"无法删除该课程，存在 {count} 个教学班");
                            }
                        }
                    }
                    catch (OracleException ex) when (ex.Number == 942)
                    {
                    }

                    string sql = "DELETE FROM course WHERE course_id = :id";
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("id", id));
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            return NotFound("课程不存在");
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"删除失败: {ex.Message}");
            }
        }
    }
}