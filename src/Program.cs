using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Windows.Forms;

namespace StudentCourse
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 程序启动时自动初始化数据库
            DatabaseHelper.EnsureDatabaseInitialized();

            Application.Run(new Form1());
        }
    }
}
