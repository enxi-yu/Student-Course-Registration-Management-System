using System.Collections.Generic;
using System.IO;
using System.Text;
using StudentCourse.Models;

namespace StudentCourse.Services
{
    public sealed class ExportService
    {
        public void ExportClassStudentsCsv(string path, IList<StudentListDto> students)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("学号,姓名,专业,年级,选课时间");

            foreach (StudentListDto student in students)
            {
                builder.AppendLine(
                    Csv(student.StudentNo) + "," +
                    Csv(student.StudentName) + "," +
                    Csv(student.Major) + "," +
                    Csv(student.Grade) + "," +
                    Csv(student.SelectTime));
            }

            File.WriteAllText(path, builder.ToString(), new UTF8Encoding(true));
        }

        private static string Csv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string escaped = value.Replace("\"", "\"\"");
            return "\"" + escaped + "\"";
        }
    }
}
