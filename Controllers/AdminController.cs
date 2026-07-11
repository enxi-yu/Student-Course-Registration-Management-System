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
        /// <summary>
        /// 获取所有课程列表
        /// </summary>
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

        /// <summary>
        /// 根据课程ID获取课程详情
        /// </summary>
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


        ///<summary>
        /// 插入课程
        /// <summary>
        private void InsertCourse(OracleConnection conn,
                                string courseName,string courseType,
                                decimal credit, int totalHours,
                                string department,string courseDesc)
        {
            string maxIdSql = "SELECT NVL(MAX(course_id), 0) FROM course";
            using (var maxCmd = new OracleCommand(maxIdSql, conn))
            {
                int newId = Convert.ToInt32(maxCmd.ExecuteScalar()) + 1;

                string sql = "INSERT INTO course (course_id, course_name, course_type, credit, total_hours, department, course_desc) VALUES (:id, :name, :type, :credit, :hours, :dept, :cdesc)";
                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.Parameters.Add(new OracleParameter("id", newId));
                    cmd.Parameters.Add(new OracleParameter("name", courseName));
                    cmd.Parameters.Add(new OracleParameter("type", courseType));
                    cmd.Parameters.Add(new OracleParameter("credit", credit));
                    cmd.Parameters.Add(new OracleParameter("hours", totalHours));
                    cmd.Parameters.Add(new OracleParameter("dept", string.IsNullOrEmpty(department) ? DBNull.Value : (object)department));
                    cmd.Parameters.Add(new OracleParameter("cdesc", string.IsNullOrEmpty(courseDesc) ? DBNull.Value : (object)courseDesc));
                    cmd.ExecuteNonQuery();
                }
            }
        }



        /// <summary>
        /// 发布新课程
        /// </summary>
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
                    InsertCourse(conn, input.CourseName, input.CourseType,
                        input.Credit, input.TotalHours, input.Department, input.CourseDesc);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"写入数据库失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新课程信息
        /// </summary>
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

        /// <summary>
        /// 删除课程（需先检查是否有section关联）
        /// </summary>
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
                                return BadRequest($"无法删除该课程，存在 {count} 次开课");
                            }
                        }
                    }
                    catch (OracleException ex) when (ex.Number == 942)
                    {
                        // section表不存在，无需检查
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

        /// <summary>
        /// 获取所有开课申请列表
        /// </summary>
        [HttpGet("applications")]
        public IActionResult GetApplications()
        {
            var list = new List<CourseApplicationDto>();
            try{
                using (var conn = DbConnectionFactory.OpenConnection()){
                    string sql = @"SELECT apply_id, teacher_no, course_name, credit, total_hours, textbook, 
                                       course_summary, course_type, department, apply_time, status, 
                                       approve_time, approve_comment 
                                FROM course_application";
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader()){
                        while (reader.Read()) {
                            list.Add(new CourseApplicationDto{
                                ApplyId = reader["apply_id"].ToString() ?? "",
                                TeacherNo = reader["teacher_no"].ToString() ?? "",
                                CourseName = reader["course_name"].ToString() ?? "",
                                Credit = Convert.ToDecimal(reader["credit"]),
                                TotalHours = Convert.ToInt32(reader["total_hours"]),
                                Textbook = reader["textbook"]?.ToString() ?? "",
                                CourseSummary = reader["course_summary"]?.ToString() ?? "",
                                CourseType = reader["course_type"].ToString() ?? "",
                                Department = reader["department"].ToString() ?? "",
                                ApplyTime = reader["apply_time"]?.ToString() ?? "",
                                Status = reader["status"].ToString() ?? "",
                                ApproveTime = reader["approve_time"]?.ToString() ?? "",
                                ApproveComment = reader["approve_comment"]?.ToString() ?? ""
                            });
                        }
                    }
                }
                return Ok(list);
            }
            catch (OracleException ex) when (ex.Number == 942){
                return Ok(list);
            }
            catch (Exception ex) {
                return StatusCode(500, $"数据库连接失败: {ex.Message}");
            }
        }

        ///<summary>
        /// 查询申请详情的私有方法
        /// <summary>
        private CourseApplicationDto? GetApplicationById(OracleConnection conn,string applyId){
            string sql = @"SELECT apply_id, teacher_no, course_name, credit, total_hours, textbook, 
                                       course_summary, course_type, department, apply_time, status, 
                                       approve_time, approve_comment 
                                FROM course_application
                                WHERE apply_id = :applyID";
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("applyID", applyId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new CourseApplicationDto
                                {
                                ApplyId = reader["apply_id"].ToString() ?? "",
                                TeacherNo = reader["teacher_no"].ToString() ?? "",
                                CourseName = reader["course_name"].ToString() ?? "",
                                Credit = Convert.ToDecimal(reader["credit"]),
                                TotalHours = Convert.ToInt32(reader["total_hours"]),
                                Textbook = reader["textbook"]?.ToString() ?? "",
                                CourseSummary = reader["course_summary"]?.ToString() ?? "",
                                CourseType = reader["course_type"].ToString() ?? "",
                                Department = reader["department"].ToString() ?? "",
                                ApplyTime = reader["apply_time"]?.ToString() ?? "",
                                Status = reader["status"].ToString() ?? "",
                                ApproveTime = reader["approve_time"]?.ToString() ?? "",
                                ApproveComment = reader["approve_comment"]?.ToString() ?? ""
                                };
                            }
                        }
                    }
            return null;
        }

        /// <summary>
        /// 根据申请ID获取申请详情
        /// </summary>
        [HttpGet("applications/{applyId}")]
        public IActionResult GetApplication(string applyId)
        {
            try
            {
                using (var conn = DbConnectionFactory.OpenConnection())
                {
                    var application=GetApplicationById(conn,applyId);
                    if(application!=null){
                        return Ok(application);
                    }   
                }
                return NotFound("申请不存在");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 待审核->通过/驳回->已开课（当section表中存在对应记录时）
        /// </summary>
        [HttpPut("applications/{applyId}/approve")]
        public IActionResult ApproveApplication(string applyId, [FromBody] ApprovalRequest request)
        {
            // 验证请求参数
            if (request == null){
                return BadRequest("请求参数不能为空");
            }
            if (string.IsNullOrEmpty(request.Status)){
                return BadRequest("审批状态不能为空");
            }
            if (request.Status != "通过" && request.Status != "驳回"){
                return BadRequest("审批状态只能为'通过'或'驳回'");
            }
            else{
                //更新 course_application 表
                using (var conn = DbConnectionFactory.OpenConnection())
                {
                    string sql=@"UPDATE course_application
                                SET status=:status, approve_time=SYSDATE,approve_comment=:approve_comment
                                WHERE apply_id=:applyID";
                    using (var cmd = new OracleCommand(sql, conn)){
                        cmd.Parameters.Add(new OracleParameter("status", request.Status));
                        cmd.Parameters.Add(new OracleParameter("approve_comment", request.Comment));
                        cmd.Parameters.Add(new OracleParameter("applyID",applyId));
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0) {
                            return NotFound("申请不存在");
                        }
                    }
                    //审核通过时，同步把课程加入到course表
                    if (request.Status == "通过"){
                        var application=GetApplicationById(conn,applyId);
                        if(application!=null){
                            InsertCourse(conn,application.CourseName,application.CourseType,
                                        application.Credit,application.TotalHours,application.Department,application.CourseSummary);
                        }   
                    }
                }
            }
            return Ok();
        }
    }

    /// <summary>
    /// 审批请求 DTO
    /// </summary>
    public class ApprovalRequest
    {
        //审批状态
        public string Status { get; set; } = string.Empty;
        //审批意见
        public string Comment { get; set; } = string.Empty;
    }
}