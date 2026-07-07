using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace StudentCourse
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string connStr = "User Id=course;Password=Course2026;Data Source=47.116.104.63:1521/COURSEPDB;";

            try
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // 查询服务器主机名和当前用户名
                    string sql = "SELECT SYS_CONTEXT('USERENV', 'SERVER_HOST') AS 主机名, SYS_CONTEXT('USERENV', 'CURRENT_USER') AS 用户名 FROM DUAL";

                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        string result = $"✅ 连接成功！\n\n主机名：{reader["主机名"]}\n用户名：{reader["用户名"]}\n\n请截图发到群里！";
                        MessageBox.Show(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ 连接失败：\n" + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string connStr = "User Id=course;Password=Course2026;Data Source=47.116.104.63:1521/COURSEPDB;";
            string sql = "SELECT table_name FROM user_tables ORDER BY table_name";

            try
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        string result = "===== 当前数据库中的表 =====\n\n";
                        int count = 0;
                        while (reader.Read())
                        {
                            count++;
                            result += $"{count}. {reader["table_name"]}\n";
                        }
                        result += $"\n共 {count} 张表";
                        MessageBox.Show(result, "查表结果");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("查询失败：" + ex.Message);
            }
        }
    }
}
