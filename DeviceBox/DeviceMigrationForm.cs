using System;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace DeviceBox
{
    /// <summary>
    /// 設備配置資料遷移表單
    /// 提供UI介面執行資料遷移和資料庫表建立
    /// </summary>
    public partial class DeviceMigrationForm : Form
    {
        private Config config;
        private TextBox txtLog;
        private Button btnCreateTables;
        private Button btnMigrate;
        private Button btnClose;
        private Label lblStatus;

        public DeviceMigrationForm()
        {
            InitializeComponent();
            config = new Config();
            config.LoadConfig();
        }

        private void InitializeComponent()
        {
            this.Text = "設備配置資料遷移工具";
            this.Size = new System.Drawing.Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Status Label
            lblStatus = new Label();
            lblStatus.Location = new System.Drawing.Point(20, 20);
            lblStatus.Size = new System.Drawing.Size(640, 30);
            lblStatus.Text = "此工具將把 config.xml 中的設備配置遷移到資料庫";
            lblStatus.Font = new System.Drawing.Font(lblStatus.Font.FontFamily, 10F, System.Drawing.FontStyle.Bold);
            this.Controls.Add(lblStatus);

            // Log TextBox
            txtLog = new TextBox();
            txtLog.Location = new System.Drawing.Point(20, 60);
            txtLog.Size = new System.Drawing.Size(640, 300);
            txtLog.Multiline = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.ReadOnly = true;
            txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.Controls.Add(txtLog);

            // Create Tables Button
            btnCreateTables = new Button();
            btnCreateTables.Location = new System.Drawing.Point(20, 380);
            btnCreateTables.Size = new System.Drawing.Size(200, 40);
            btnCreateTables.Text = "1. 建立資料庫表";
            btnCreateTables.Click += BtnCreateTables_Click;
            this.Controls.Add(btnCreateTables);

            // Migrate Button
            btnMigrate = new Button();
            btnMigrate.Location = new System.Drawing.Point(240, 380);
            btnMigrate.Size = new System.Drawing.Size(200, 40);
            btnMigrate.Text = "2. 執行資料遷移";
            btnMigrate.Click += BtnMigrate_Click;
            this.Controls.Add(btnMigrate);

            // Close Button
            btnClose = new Button();
            btnClose.Location = new System.Drawing.Point(460, 380);
            btnClose.Size = new System.Drawing.Size(200, 40);
            btnClose.Text = "關閉";
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void BtnCreateTables_Click(object sender, EventArgs e)
        {
            try
            {
                LogMessage("========================================");
                LogMessage("開始建立資料庫表...");
                LogMessage($"資料庫: {config.IP}/{config.DB}");

                string sqlFilePath = Path.Combine(Application.StartupPath, "Database", "create_device_tables.sql");

                if (!File.Exists(sqlFilePath))
                {
                    LogMessage($"錯誤: 找不到 SQL 檔案 {sqlFilePath}");
                    MessageBox.Show("找不到 create_device_tables.sql 檔案!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string sqlScript = File.ReadAllText(sqlFilePath);

                // 分割 SQL 語句 (以分號分隔,但跳過註解)
                string connectionString = $"server={config.IP};database={config.DB};uid={config.USER};pwd={config.Password};CharSet=utf8mb4;";

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    LogMessage("資料庫連線成功");

                    // 執行 SQL 腳本
                    var commands = SplitSqlScript(sqlScript);
                    int successCount = 0;

                    foreach (var cmdText in commands)
                    {
                        if (string.IsNullOrWhiteSpace(cmdText))
                            continue;

                        try
                        {
                            using (var cmd = new MySqlCommand(cmdText, connection))
                            {
                                cmd.ExecuteNonQuery();
                                successCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"警告: {ex.Message}");
                        }
                    }

                    LogMessage($"成功執行 {successCount} 個 SQL 指令");
                    LogMessage("資料庫表建立完成!");
                    LogMessage("========================================\n");

                    MessageBox.Show("資料庫表建立成功!", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"錯誤: {ex.Message}");
                MessageBox.Show($"建立資料庫表失敗:\n{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnMigrate_Click(object sender, EventArgs e)
        {
            try
            {
                LogMessage("========================================");
                LogMessage("開始資料遷移...");

                var migrationTool = new DeviceMigrationTool(config.IP, config.DB, config.USER, config.Password);
                var result = migrationTool.Migrate();

                LogMessage($"\n遷移結果:");
                LogMessage($"成功: {result.SuccessCount} 個工廠");
                LogMessage($"失敗: {result.FailureCount} 個工廠");

                foreach (var factory in result.Factories)
                {
                    if (factory.Success)
                    {
                        LogMessage($"✓ {factory.FactoryName} - {factory.DeviceCount} 個設備");
                    }
                    else
                    {
                        LogMessage($"✗ {factory.FactoryName} - {factory.ErrorMessage}");
                    }

                    foreach (var warning in factory.Warnings)
                    {
                        LogMessage($"  警告: {warning}");
                    }
                }

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    LogMessage($"\n錯誤: {result.ErrorMessage}");
                }

                LogMessage("\n" + result.Message);
                LogMessage("========================================\n");

                if (result.Success)
                {
                    MessageBox.Show(result.Message, "遷移完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"遷移失敗:\n{result.ErrorMessage}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"錯誤: {ex.Message}");
                MessageBox.Show($"資料遷移失敗:\n{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LogMessage(string message)
        {
            txtLog.AppendText(message + "\r\n");
            Application.DoEvents();
        }

        private string[] SplitSqlScript(string script)
        {
            // 簡單的 SQL 分割 (以分號+換行分隔)
            var commands = new System.Collections.Generic.List<string>();
            var lines = script.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var currentCommand = new System.Text.StringBuilder();

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                // 跳過註解
                if (trimmed.StartsWith("--") || trimmed.StartsWith("#") || string.IsNullOrWhiteSpace(trimmed))
                    continue;

                currentCommand.AppendLine(line);

                // 如果遇到分號,表示一個指令結束
                if (trimmed.EndsWith(";"))
                {
                    commands.Add(currentCommand.ToString());
                    currentCommand.Clear();
                }
            }

            // 加入最後一個指令 (如果沒有分號結尾)
            if (currentCommand.Length > 0)
            {
                commands.Add(currentCommand.ToString());
            }

            return commands.ToArray();
        }
    }
}
