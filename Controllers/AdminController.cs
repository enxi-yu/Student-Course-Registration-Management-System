using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace StudentCourse.Controllers
{
    // 精准声明一个类，属性必须是公开的 (public) 才能被系统自动注入
    public class AdminCourseModel
    {
        public string c_name { get; set; } = string.Empty;
        public string c_credit { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        // 内存数据库，并给两个初始值防止页面线框空掉
        private static readonly List<AdminCourseModel> MockCourses = new List<AdminCourseModel>
        {
            new AdminCourseModel { c_name = "高级计算机系统架构", c_credit = "4" },
            new AdminCourseModel { c_name = "编译原理与自动化验证", c_credit = "3" }
        };

        // 查课程：GET /api/admin/courses
        [HttpGet("courses")]
        public IActionResult GetCourses()
        {
            return Ok(MockCourses);
        }

        // 发课程：POST /api/admin/courses
        [HttpPost("courses")]
        public IActionResult AddCourse([FromBody] AdminCourseModel input)
        {
            if (input == null || string.IsNullOrEmpty(input.c_name))
            {
                return BadRequest();
            }

            MockCourses.Add(input);
            return Ok(); // 返回 200 OK 成功状态
        }
    }
}