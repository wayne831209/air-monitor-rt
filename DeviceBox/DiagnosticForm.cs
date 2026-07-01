using System;
using System.Windows.Forms;
using System.Linq;

namespace DeviceBox
{
    /// <summary>
    /// 資料載入診斷工具 - 檢查程式是否正確從資料庫載入資料
    /// </summary>
    public partial class DiagnosticForm : Form
    {
        private TextBox txtLog;
        private Button btnTestDatabase;
        private Button btnTestConfig;
        private Button btnClose;
        private Label lblTitle;

        public DiagnosticForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "資料載入診斷工具";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Title
            lblTitle = new Label();
            lblTitle.Location = new System.Drawing.Point(20, 20);
            lblTitle.Size = new System.Drawing.Size(740, 30);
            lblTitle.Text = "檢查程式是否正確從資料庫載入設備配置";
            lblTitle.Font = new System.Drawing.Font(lblTitle.Font.FontFamily, 12F, System.Drawing.FontStyle.Bold);
            this.Controls.Add(lblTitle);

            // Log TextBox
            txtLog = new TextBox();
            txtLog.Location = new System.Drawing.Point(20, 60);
            txtLog.Size = new System.Drawing.Size(740, 420);
            txtLog.Multiline = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.ReadOnly = true;
            txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.Controls.Add(txtLog);

            // Test Database Button
            btnTestDatabase = new Button();
            btnTestDatabase.Location = new System.Drawing.Point(20, 500);
            btnTestDatabase.Size = new System.Drawing.Size(230, 40);
            btnTestDatabase.Text = "1. 測試資料庫連線";
            btnTestDatabase.Click += BtnTestDatabase_Click;
            this.Controls.Add(btnTestDatabase);

            // Test Config Button
            btnTestConfig = new Button();
            btnTestConfig.Location = new System.Drawing.Point(270, 500);
            btnTestConfig.Size = new System.Drawing.Size(230, 40);
            btnTestConfig.Text = "2. 測試載入配置";
            btnTestConfig.Click += BtnTestConfig_Click;
            this.Controls.Add(btnTestConfig);

            // Close Button
            btnClose = new Button();
            btnClose.Location = new System.Drawing.Point(520, 500);
            btnClose.Size = new System.Drawing.Size(240, 40);
            btnClose.Text = "關閉";
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void BtnTestDatabase_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
            Log("========================================");
            Log("測試資料庫連線與資料");
            Log("========================================\n");

            try
            {
                // 載入 config.xml 取得資料庫設定
                var config = new Config();
                if (!config.LoadConfig())
                {
                    Log("❌ 錯誤: 無法載入 config.xml");
                    return;
                }

                Log($"✅ 已載入 config.xml");
                Log($"   資料庫位址: {config.IP}");
                Log($"   資料庫名稱: {config.DB}");
                Log($"   使用者: {config.USER}\n");

                // 建立資料庫連線
                var deviceDb = new DeviceDatabase(config.IP, config.DB, config.USER, config.Password);

                // 測試連線
                Log("測試資料庫連線...");
                if (!deviceDb.TestConnection())
                {
                    Log("❌ 錯誤: 資料庫連線失敗!");
                    Log("   請檢查:");
                    Log("   1. MySQL 伺服器是否啟動");
                    Log("   2. IP 位址是否正確");
                    Log("   3. 帳號密碼是否正確");
                    Log("   4. 防火牆是否阻擋連線");
                    return;
                }

                Log("✅ 資料庫連線成功!\n");

                // 查詢工廠資料
                Log("查詢工廠資料...");
                var factories = deviceDb.LoadFactories();
                Log($"✅ 找到 {factories.Count} 個工廠:");

                if (factories.Count == 0)
                {
                    Log("❌ 警告: 資料庫中沒有工廠資料!");
                    Log("   請先執行資料遷移工具 (DeviceBox.exe --migrate)");
                    return;
                }

                foreach (var factory in factories)
                {
                    Log($"   - [{factory.Id}] {factory.Name} ({factory.ModbusIp}:{factory.ModbusPort})");
                }
                Log("");

                // 查詢設備資料
                Log("查詢設備資料...");
                int totalDevices = 0;
                foreach (var factory in factories)
                {
                    var devices = deviceDb.LoadDevices(factory.Id);
                    totalDevices += devices.Count;
                    Log($"   {factory.Name}: {devices.Count} 個設備");

                    foreach (var device in devices)
                    {
                        Log($"      - {device.Type} #{device.MachineNo}: {device.Name}");
                    }
                }
                Log($"✅ 總共 {totalDevices} 個設備\n");

                // 查詢警報上下限
                Log("查詢警報上下限...");
                int limitsCount = 0;
                foreach (var factory in factories)
                {
                    var limits = deviceDb.LoadAlarmLimits(factory.Id);
                    if (limits.PressureUpperLimit != double.MaxValue || limits.PressureLowerLimit != double.MinValue)
                    {
                        limitsCount++;
                        Log($"   {factory.Name}:");
                        if (limits.PressureUpperLimit != double.MaxValue)
                            Log($"      壓力上限: {limits.PressureUpperLimit}");
                        if (limits.PressureLowerLimit != double.MinValue)
                            Log($"      壓力下限: {limits.PressureLowerLimit}");
                    }
                }
                Log($"✅ {limitsCount} 個工廠有上下限設定\n");

                Log("========================================");
                Log("✅ 資料庫測試完成 - 一切正常!");
                Log("========================================");

                MessageBox.Show($"資料庫測試成功!\n\n工廠: {factories.Count} 個\n設備: {totalDevices} 個\n上下限: {limitsCount} 組", 
                    "測試成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log($"\n❌ 錯誤: {ex.Message}");
                Log($"   詳細資訊: {ex.StackTrace}");
                MessageBox.Show($"測試失敗:\n{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTestConfig_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
            Log("========================================");
            Log("測試 Config 類別載入配置");
            Log("========================================\n");

            try
            {
                var config = new Config();

                Log("載入 config.xml...");
                if (!config.LoadConfig())
                {
                    Log("❌ 錯誤: Config.LoadConfig() 返回 false");
                    return;
                }

                Log("✅ Config.LoadConfig() 成功\n");

                // 檢查資料庫設定
                Log("資料庫設定:");
                Log($"   IP: {config.IP}");
                Log($"   DB: {config.DB}");
                Log($"   USER: {config.USER}");
                Log($"   mysql_on: {config.mysql_on}\n");

                // 檢查工廠資料
                Log($"載入的工廠數量: {config.Factories.Count}");

                if (config.Factories.Count == 0)
                {
                    Log("❌ 警告: Config.Factories 是空的!");
                    Log("   可能原因:");
                    Log("   1. 資料庫中沒有資料 (請執行遷移工具)");
                    Log("   2. 資料庫連線失敗 (請檢查 config.xml 的資料庫設定)");
                    Log("   3. LoadFactoriesFromDatabase() 發生錯誤\n");

                    Log("建議:");
                    Log("   1. 先執行「1. 測試資料庫連線」確認資料庫有資料");
                    Log("   2. 檢查 Visual Studio 的「輸出」視窗查看 Debug 訊息");
                    return;
                }

                Log("✅ 成功載入工廠資料:\n");

                foreach (var factory in config.Factories)
                {
                    Log($"工廠 [{factory.Id}]: {factory.Name}");
                    Log($"   Modbus: {factory.ModbusIp}:{factory.ModbusPort}");
                    Log($"   設備數量: {factory.Devices.Count}");

                    if (factory.Devices.Count > 0)
                    {
                        var compressors = factory.Devices.Count(d => d.Type == DeviceType.Compressor);
                        var others = factory.Devices.Count - compressors;
                        Log($"      空壓機: {compressors} 個");
                        Log($"      其他設備: {others} 個");
                    }

                    if (factory.AlarmLimits.PressureUpperLimit != double.MaxValue)
                    {
                        Log($"   壓力上限: {factory.AlarmLimits.PressureUpperLimit}");
                    }
                    Log("");
                }

                Log("========================================");
                Log("✅ Config 載入測試完成 - 一切正常!");
                Log("========================================");

                MessageBox.Show($"Config 載入成功!\n\n工廠: {config.Factories.Count} 個\n設備: {config.Factories.Sum(f => f.Devices.Count)} 個", 
                    "測試成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log($"\n❌ 錯誤: {ex.Message}");
                Log($"   詳細資訊: {ex.StackTrace}");
                MessageBox.Show($"測試失敗:\n{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Log(string message)
        {
            txtLog.AppendText(message + "\r\n");
            Application.DoEvents();
        }
    }
}
