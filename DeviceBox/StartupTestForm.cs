using System;
using System.Windows.Forms;

namespace DeviceBox
{
    /// <summary>
    /// 資料庫測試和初始化的啟動表單
    /// </summary>
    public partial class StartupTestForm : Form
    {
        private Button btnTestConnection;
        private Button btnInitDatabase;
        private Button btnCheckData;
        private Button btnMigrate;
        private Button btnDiagnostic;
        private Button btnStartApp;
        private TextBox txtLog;
        private Label lblTitle;

        public StartupTestForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "DeviceBox 資料庫測試工具";
            this.Size = new System.Drawing.Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title
            lblTitle = new Label
            {
                Text = "資料庫連線與資料測試",
                Font = new System.Drawing.Font("Microsoft YaHei UI", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(550, 30)
            };
            this.Controls.Add(lblTitle);

            // Test Connection Button
            btnTestConnection = new Button
            {
                Text = "1. 測試資料庫連線",
                Location = new System.Drawing.Point(20, 70),
                Size = new System.Drawing.Size(250, 40),
                Font = new System.Drawing.Font("Microsoft YaHei UI", 10)
            };
            btnTestConnection.Click += BtnTestConnection_Click;
            this.Controls.Add(btnTestConnection);

            // Initialize Database Button
            btnInitDatabase = new Button
            {
                Text = "2. 初始化資料表",
                Location = new System.Drawing.Point(20, 120),
                Size = new System.Drawing.Size(250, 40),
                Font = new System.Drawing.Font("Microsoft YaHei UI", 10)
            };
            btnInitDatabase.Click += BtnInitDatabase_Click;
            this.Controls.Add(btnInitDatabase);

            // Check Data Button
            btnCheckData = new Button
            {
                Text = "3. 檢查資料庫資料",
                Location = new System.Drawing.Point(20, 170),
                Size = new System.Drawing.Size(120, 40),
                Font = new System.Drawing.Font("Microsoft YaHei UI", 9)
            };
            btnCheckData.Click += BtnCheckData_Click;
            this.Controls.Add(btnCheckData);

            // Migrate Button
            btnMigrate = new Button
            {
                Text = "📥 匯入 XML 排程",
                Location = new System.Drawing.Point(150, 170),
                Size = new System.Drawing.Size(120, 40),
                Font = new System.Drawing.Font("Microsoft YaHei UI", 9, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.FromArgb(52, 199, 89),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnMigrate.Click += BtnMigrate_Click;
            this.Controls.Add(btnMigrate);

            // Diagnostic Button
            btnDiagnostic = new Button
            {
                Text = "🔧 進階診斷工具",
                Location = new System.Drawing.Point(290, 70),
                Size = new System.Drawing.Size(250, 130),
                Font = new System.Drawing.Font("Microsoft YaHei UI", 12, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.FromArgb(255, 140, 0),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDiagnostic.Click += BtnDiagnostic_Click;
            this.Controls.Add(btnDiagnostic);

            // Start App Button
            btnStartApp = new Button
            {
                Text = "✓ 啟動主程式",
                Location = new System.Drawing.Point(20, 220),
                Size = new System.Drawing.Size(250, 40),
                Font = new System.Drawing.Font("Microsoft YaHei UI", 11, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnStartApp.Click += BtnStartApp_Click;
            this.Controls.Add(btnStartApp);

            // Log TextBox
            txtLog = new TextBox
            {
                Location = new System.Drawing.Point(20, 280),
                Size = new System.Drawing.Size(550, 160),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new System.Drawing.Font("Consolas", 9),
                ReadOnly = true
            };
            this.Controls.Add(txtLog);
        }

        private void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            txtLog.ScrollToCaret();
        }

        private void BtnTestConnection_Click(object sender, EventArgs e)
        {
            try
            {
                Log("正在測試資料庫連線...");

                var config = new Config();
                if (!config.LoadConfig())
                {
                    Log("❌ 載入 config.xml 失敗");
                    return;
                }

                var scheduleDb = config.GetScheduleDatabase();
                if (scheduleDb == null)
                {
                    Log("❌ 無法取得 ScheduleDatabase 實例");
                    return;
                }

                if (scheduleDb.TestConnection())
                {
                    Log("✓ 資料庫連線成功！");
                    Log($"  伺服器: {config.IP}");
                    Log($"  資料庫: {config.DB}");
                }
                else
                {
                    Log("❌ 資料庫連線失敗");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 錯誤: {ex.Message}");
            }
        }

        private void BtnInitDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                Log("正在檢查資料表...");

                var config = new Config();
                if (!config.LoadConfig())
                {
                    Log("❌ 載入 config.xml 失敗");
                    return;
                }

                var scheduleDb = config.GetScheduleDatabase();
                if (scheduleDb == null)
                {
                    Log("❌ 無法取得 ScheduleDatabase 實例");
                    return;
                }

                if (scheduleDb.CheckTablesExist())
                {
                    Log("✓ 資料表已存在");
                }
                else
                {
                    Log("⚠ 資料表不存在，請手動執行 create_schedule_tables.sql");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 錯誤: {ex.Message}");
            }
        }

        private void BtnCheckData_Click(object sender, EventArgs e)
        {
            try
            {
                Log("正在檢查資料庫中的模式資料...");

                var config = new Config();
                if (!config.LoadConfig())
                {
                    Log("❌ 載入 config.xml 失敗");
                    return;
                }

                var scheduleDb = config.GetScheduleDatabase();
                if (scheduleDb == null)
                {
                    Log("❌ 無法取得 ScheduleDatabase 實例");
                    return;
                }

                var modes = scheduleDb.LoadModesFromDatabase();
                Log($"✓ 找到 {modes.Count} 個模式");

                foreach (var mode in modes)
                {
                    Log($"  - 模式 #{mode.Id}: {mode.Name}");
                    Log($"    說明: {mode.Description}");
                    Log($"    預設: {(mode.IsDefault ? "是" : "否")}");
                    Log($"    排程數量: {mode.Schedules.Count}");

                    foreach (var schedule in mode.Schedules)
                    {
                        Log($"      • {schedule.FactoryName} - {schedule.DeviceName}");
                        Log($"        時間: {schedule.StartDay} {schedule.StartTime:hh\\:mm} ~ {schedule.EndDay} {schedule.EndTime:hh\\:mm}");
                    }
                }

                if (modes.Count == 0)
                {
                    Log("⚠ 資料庫中沒有模式資料");
                    Log("  提示：啟動主程式後會自動建立預設模式");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 錯誤: {ex.Message}");
                Log($"   詳細: {ex.StackTrace}");
            }
        }

        private void BtnMigrate_Click(object sender, EventArgs e)
        {
            Log("========================================");
            Log("開始從 config.xml 匯入排程資料...");

            try
            {
                var config = new Config();
                if (!config.LoadConfig())
                {
                    Log("❌ 載入 config.xml 失敗");
                    return;
                }

                var scheduleDb = config.GetScheduleDatabase();
                if (scheduleDb == null)
                {
                    Log("❌ 無法取得 ScheduleDatabase 實例");
                    return;
                }

                // 詢問是否清空現有資料
                var existingModes = scheduleDb.LoadModesFromDatabase();
                if (existingModes.Count > 0)
                {
                    var result = MessageBox.Show(
                        $"資料庫中已有 {existingModes.Count} 個模式。\n\n" +
                        "匯入方式：\n" +
                        "• 是(Y) - 清空現有資料後重新匯入\n" +
                        "• 否(N) - 保留現有資料，只新增 XML 中的新模式\n" +
                        "• 取消 - 中止匯入",
                        "匯入選項",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Cancel)
                    {
                        Log("⚠ 使用者取消匯入");
                        return;
                    }

                    if (result == DialogResult.Yes)
                    {
                        Log("清空現有資料...");
                        // 這裡需要清空資料的邏輯
                        Log("⚠ 清空功能請使用診斷工具");
                    }
                    else
                    {
                        Log("保留現有資料，只新增新模式");
                    }
                }

                // 執行遷移
                Log("讀取 config.xml 中的模式...");
                var migrationTool = new MigrationTool(scheduleDb);
                bool success = migrationTool.MigrateModesFromXml();

                if (success)
                {
                    Log("✓ 匯入成功！");
                    Log("");
                    Log("查看匯入結果：");
                    BtnCheckData_Click(null, null);
                }
                else
                {
                    Log("❌ 匯入失敗，請查看詳細訊息");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 錯誤: {ex.Message}");
                Log($"   詳細: {ex.StackTrace}");
            }
        }

        private void BtnDiagnostic_Click(object sender, EventArgs e)
        {
            DatabaseDiagnosticForm.Show();
        }

        private void BtnStartApp_Click(object sender, EventArgs e)
        {
            Log("正在啟動主程式...");
            this.Hide();

            var mainForm = new MainForm();
            mainForm.FormClosed += (s, args) => this.Close();
            mainForm.Show();
        }
    }
}
