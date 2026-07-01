using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceBox
{
    internal static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 檢查命令列參數
            if (args.Length > 0 && args[0].ToLower() == "--migrate")
            {
                Application.Run(new DeviceMigrationForm());
                return;
            }
            if (args.Length > 0 && (args[0].ToLower() == "--diagnostic" || args[0].ToLower() == "--diag"))
            {
                Application.Run(new DiagnosticForm());
                return;
            }

            // 啟動測試表單（可以選擇測試或直接啟動主程式）
            //Application.Run(new StartupTestForm());

            Application.Run(new MainForm());

        }
    }
}
