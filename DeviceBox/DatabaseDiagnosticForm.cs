using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace DeviceBox
{
    /// <summary>
    /// 資料庫診斷工具 - 檢查資料表狀態和資料
    /// </summary>
    public partial class DatabaseDiagnosticForm : Form
    {
        private TextBox txtLog;
        private Button btnCheckTables;
        private Button btnCheckModes;
        private Button btnCheckSchedules;
        private Button btnTestInsert;
        private Button btnClearAll;
        private Config _config;
        private ScheduleDatabase _scheduleDb;

        public DatabaseDiagnosticForm()
        {
            InitializeComponent();
            LoadConfig();
        }

        private void InitializeComponent()
        {
            this.Text = "資料庫診斷工具";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Log TextBox
            txtLog = new TextBox
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(740, 400),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new System.Drawing.Font("Consolas", 9),
                ReadOnly = true
            };
            this.Controls.Add(txtLog);

            // Buttons
            btnCheckTables = new Button
            {
                Text = "檢查資料表結構",
                Location = new System.Drawing.Point(20, 440),
                Size = new System.Drawing.Size(140, 40)
            };
            btnCheckTables.Click += BtnCheckTables_Click;
            this.Controls.Add(btnCheckTables);

            btnCheckModes = new Button
            {
                Text = "查看 Modes 資料",
                Location = new System.Drawing.Point(170, 440),
                Size = new System.Drawing.Size(140, 40)
            };
            btnCheckModes.Click += BtnCheckModes_Click;
            this.Controls.Add(btnCheckModes);

            btnCheckSchedules = new Button
            {
                Text = "查看 Schedules 資料",
                Location = new System.Drawing.Point(320, 440),
                Size = new System.Drawing.Size(140, 40)
            };
            btnCheckSchedules.Click += BtnCheckSchedules_Click;
            this.Controls.Add(btnCheckSchedules);

            btnTestInsert = new Button
            {
                Text = "測試插入資料",
                Location = new System.Drawing.Point(470, 440),
                Size = new System.Drawing.Size(140, 40),
                BackColor = System.Drawing.Color.Green,
                ForeColor = System.Drawing.Color.White
            };
            btnTestInsert.Click += BtnTestInsert_Click;
            this.Controls.Add(btnTestInsert);

            btnClearAll = new Button
            {
                Text = "清空所有資料",
                Location = new System.Drawing.Point(620, 440),
                Size = new System.Drawing.Size(140, 40),
                BackColor = System.Drawing.Color.Red,
                ForeColor = System.Drawing.Color.White
            };
            btnClearAll.Click += BtnClearAll_Click;
            this.Controls.Add(btnClearAll);
        }

        private void LoadConfig()
        {
            try
            {
                _config = new Config();
                if (_config.LoadConfig())
                {
                    _scheduleDb = _config.GetScheduleDatabase();
                    Log($"✓ 設定載入成功");
                    Log($"  資料庫: {_config.IP}/{_config.DB}");
                }
                else
                {
                    Log("❌ 設定載入失敗");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 載入設定錯誤: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            txtLog.ScrollToCaret();
            Application.DoEvents();
        }

        private void BtnCheckTables_Click(object sender, EventArgs e)
        {
            Log("========================================");
            Log("檢查資料表結構...");

            try
            {
                using (var conn = new MySqlConnection($"server={_config.IP};database={_config.DB};uid={_config.USER};pwd={_config.Password};"))
                {
                    conn.Open();
                    Log("✓ 資料庫連線成功");

                    // Check schedule_modes
                    string sql = "SHOW TABLES LIKE 'schedule_modes'";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            Log("✓ 資料表 schedule_modes 存在");

                            // Show structure
                            sql = "DESCRIBE schedule_modes";
                            using (var cmd2 = new MySqlCommand(sql, conn))
                            using (var reader = cmd2.ExecuteReader())
                            {
                                Log("  欄位:");
                                while (reader.Read())
                                {
                                    Log($"    - {reader.GetString(0)} ({reader.GetString(1)})");
                                }
                            }
                        }
                        else
                        {
                            Log("❌ 資料表 schedule_modes 不存在");
                        }
                    }

                    // Check schedule_items
                    sql = "SHOW TABLES LIKE 'schedule_items'";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        Log(result != null ? "✓ 資料表 schedule_items 存在" : "❌ 資料表 schedule_items 不存在");
                    }

                    // Check mode_schedule_mapping
                    sql = "SHOW TABLES LIKE 'mode_schedule_mapping'";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        Log(result != null ? "✓ 資料表 mode_schedule_mapping 存在" : "❌ 資料表 mode_schedule_mapping 不存在");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 錯誤: {ex.Message}");
            }
        }

        private void BtnCheckModes_Click(object sender, EventArgs e)
        {
            Log("========================================");
            Log("查詢 schedule_modes 資料...");

            try
            {
                using (var conn = new MySqlConnection($"server={_config.IP};database={_config.DB};uid={_config.USER};pwd={_config.Password};"))
                {
                    conn.Open();

                    string sql = "SELECT id, name, description, is_default, enabled FROM schedule_modes ORDER BY id";
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            count++;
                            Log($"模式 #{reader.GetInt32(0)}:");
                            Log($"  名稱: {reader.GetString(1)}");
                            Log($"  說明: {(reader.IsDBNull(2) ? "" : reader.GetString(2))}");
                            Log($"  預設: {reader.GetBoolean(3)}");
                            Log($"  啟用: {reader.GetBoolean(4)}");
                            Log("");
                        }

                        if (count == 0)
                        {
                            Log("⚠ 資料表是空的，沒有任何模式資料");
                        }
                        else
                        {
                            Log($"✓ 共找到 {count} 筆模式資料");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 錯誤: {ex.Message}");
                Log($"   詳細: {ex.StackTrace}");
            }
        }

        private void BtnCheckSchedules_Click(object sender, EventArgs e)
        {
            Log("========================================");
            Log("查詢 schedule_items 資料...");

            try
            {
                using (var conn = new MySqlConnection($"server={_config.IP};database={_config.DB};uid={_config.USER};pwd={_config.Password};"))
                {
                    conn.Open();

                    string sql = "SELECT id, factory_name, device_name, start_time, end_time FROM schedule_items ORDER BY id";
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            count++;
                            Log($"排程 #{reader.GetInt32(0)}: {reader.GetString(1)} - {reader.GetString(2)}");
                            Log($"  時間: {reader.GetTimeSpan(3)} ~ {reader.GetTimeSpan(4)}");
                        }

                        if (count == 0)
                        {
                            Log("⚠ 資料表是空的，沒有任何排程資料");
                        }
                        else
                        {
                            Log($"✓ 共找到 {count} 筆排程資料");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 錯誤: {ex.Message}");
            }
        }

        private void BtnTestInsert_Click(object sender, EventArgs e)
        {
            Log("========================================");
            Log("測試插入資料...");

            try
            {
                var testMode = new ScheduleMode
                {
                    Id = 0,  // 讓資料庫自動產生
                    Name = "測試模式 " + DateTime.Now.ToString("HHmmss"),
                    Description = "自動產生的測試模式",
                    IsDefault = false
                };

                Log($"嘗試儲存模式: {testMode.Name}");

                bool success = _scheduleDb.SaveMode(testMode);

                if (success)
                {
                    Log($"✓ 儲存成功！新模式 ID: {testMode.Id}");
                    BtnCheckModes_Click(null, null);
                }
                else
                {
                    Log("❌ 儲存失敗");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 錯誤: {ex.Message}");
                Log($"   詳細: {ex.StackTrace}");
            }
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "確定要清空所有模式和排程資料嗎？\n此操作無法復原！",
                "警告",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                Log("========================================");
                Log("清空所有資料...");

                try
                {
                    using (var conn = new MySqlConnection($"server={_config.IP};database={_config.DB};uid={_config.USER};pwd={_config.Password};"))
                    {
                        conn.Open();

                        // Delete in correct order due to foreign keys
                        string[] sqls = new string[]
                        {
                            "DELETE FROM mode_schedule_mapping",
                            "DELETE FROM schedule_items",
                            "DELETE FROM schedule_modes"
                        };

                        foreach (var sql in sqls)
                        {
                            using (var cmd = new MySqlCommand(sql, conn))
                            {
                                int rows = cmd.ExecuteNonQuery();
                                Log($"✓ 執行: {sql} (影響 {rows} 筆)");
                            }
                        }

                        Log("✓ 所有資料已清空");
                    }
                }
                catch (Exception ex)
                {
                    Log($"❌ 錯誤: {ex.Message}");
                }
            }
        }

        public static void Show()
        {
            var form = new DatabaseDiagnosticForm();
            form.ShowDialog();
        }
    }
}
