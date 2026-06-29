using System;
using System.Windows.Forms;

namespace DeviceBox
{
    /// <summary>
    /// 資料庫管理測試表單
    /// 用於開發階段測試資料庫初始化和資料遷移
    /// </summary>
    public partial class DatabaseTestForm : Form
    {
        public DatabaseTestForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // 表單設定
            this.ClientSize = new System.Drawing.Size(500, 400);
            this.Name = "DatabaseTestForm";
            this.Text = "資料庫管理工具";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);

            // 標題
            Label lblTitle = new Label
            {
                Text = "排程資料庫管理工具",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(460, 40),
                Font = new System.Drawing.Font("微軟正黑體", 16F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.White
            };
            this.Controls.Add(lblTitle);

            // 說明
            Label lblDescription = new Label
            {
                Text = "此工具用於初始化資料庫表結構和遷移現有的排程資料",
                Location = new System.Drawing.Point(20, 70),
                Size = new System.Drawing.Size(460, 40),
                Font = new System.Drawing.Font("微軟正黑體", 10F),
                ForeColor = System.Drawing.Color.FromArgb(180, 180, 180)
            };
            this.Controls.Add(lblDescription);

            // 按鈕1：建立資料庫表
            Button btnCreateTables = new Button
            {
                Text = "1. 建立資料庫表結構",
                Location = new System.Drawing.Point(50, 130),
                Size = new System.Drawing.Size(400, 50),
                Font = new System.Drawing.Font("微軟正黑體", 12F),
                BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCreateTables.FlatAppearance.BorderSize = 0;
            btnCreateTables.Click += (s, e) => DatabaseInitializer.ShowInitializeDialog();
            this.Controls.Add(btnCreateTables);

            // 按鈕2：檢查資料庫狀態
            Button btnCheckDatabase = new Button
            {
                Text = "2. 檢查資料庫狀態",
                Location = new System.Drawing.Point(50, 190),
                Size = new System.Drawing.Size(400, 50),
                Font = new System.Drawing.Font("微軟正黑體", 12F),
                BackColor = System.Drawing.Color.FromArgb(0, 150, 136),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCheckDatabase.FlatAppearance.BorderSize = 0;
            btnCheckDatabase.Click += BtnCheckDatabase_Click;
            this.Controls.Add(btnCheckDatabase);

            // 按鈕3：資料遷移
            Button btnMigrate = new Button
            {
                Text = "3. 從 config.xml 遷移資料到資料庫",
                Location = new System.Drawing.Point(50, 250),
                Size = new System.Drawing.Size(400, 50),
                Font = new System.Drawing.Font("微軟正黑體", 12F),
                BackColor = System.Drawing.Color.FromArgb(255, 140, 0),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnMigrate.FlatAppearance.BorderSize = 0;
            btnMigrate.Click += (s, e) => MigrationTool.ShowMigrationDialog();
            this.Controls.Add(btnMigrate);

            // 關閉按鈕
            Button btnClose = new Button
            {
                Text = "關閉",
                Location = new System.Drawing.Point(200, 330),
                Size = new System.Drawing.Size(100, 40),
                Font = new System.Drawing.Font("微軟正黑體", 10F),
                BackColor = System.Drawing.Color.FromArgb(80, 80, 80),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);

            this.ResumeLayout(false);
        }

        private void BtnCheckDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                var config = new Config();
                if (!config.LoadConfig())
                {
                    MessageBox.Show("無法載入 config.xml", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var initializer = new DatabaseInitializer(config.IP, config.DB, config.USER, config.Password);
                var scheduleDb = new ScheduleDatabase(config.IP, config.DB, config.USER, config.Password);

                string status = "📊 資料庫狀態報告\n\n";
                status += $"伺服器：{config.IP}\n";
                status += $"資料庫：{config.DB}\n";
                status += $"使用者：{config.USER}\n\n";

                // 測試連線
                if (scheduleDb.TestConnection())
                {
                    status += "✓ 資料庫連線成功\n\n";

                    // 檢查表
                    if (initializer.CheckTablesExist())
                    {
                        status += "✓ 資料庫表結構完整\n\n";

                        // 載入模式統計
                        var modes = scheduleDb.LoadModesFromDatabase();
                        status += $"模式數量：{modes.Count}\n";

                        int totalSchedules = 0;
                        foreach (var mode in modes)
                        {
                            status += $"  - {mode.Name}: {mode.Schedules.Count} 個排程";
                            if (mode.IsDefault)
                                status += " (預設)";
                            status += "\n";
                            totalSchedules += mode.Schedules.Count;
                        }

                        status += $"\n總排程數：{totalSchedules}\n";
                    }
                    else
                    {
                        status += "✗ 資料庫表結構不完整\n";
                        status += "  請先執行「建立資料庫表結構」\n";
                    }
                }
                else
                {
                    status += "✗ 資料庫連線失敗\n";
                    status += "  請檢查連線設定\n";
                }

                MessageBox.Show(status, "資料庫狀態", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"檢查資料庫時發生錯誤：\n{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 顯示資料庫管理工具視窗
        /// </summary>
        public static void Show()
        {
            var form = new DatabaseTestForm();
            form.ShowDialog();
        }
    }
}
