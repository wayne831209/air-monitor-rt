using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DeviceBox
{
    public partial class AlarmLimitSettingForm : Form
    {
        /// <summary>
        /// 設定類型: "Pressure" 或 "Temp"
        /// </summary>
        private readonly string settingType;

        /// <summary>
        /// 所有工廠設定
        /// </summary>
        private readonly List<FactoryConfig> factories;

        /// <summary>
        /// 資料庫存取
        /// </summary>
        private readonly DeviceDatabase database;

        /// <summary>
        /// 設定完成後的結果 (factoryId → AlarmLimitsConfig)
        /// </summary>
        public Dictionary<int, AlarmLimitsConfig> ResultLimitsMap { get; private set; }

        public AlarmLimitSettingForm(List<FactoryConfig> factories, string settingType, DeviceDatabase database)
        {
            InitializeComponent();
            this.factories = factories;
            this.settingType = settingType;
            this.database = database;
            this.ResultLimitsMap = new Dictionary<int, AlarmLimitsConfig>();

            LoadAllFactories();
            LoadNotificationSettings();
        }

        private void LoadAllFactories()
        {
            if (settingType == "Pressure")
            {
                this.Text = "全部設備 - 空壓上下限設定";
                lblTitle.Text = "空壓上下限設定 (kg/cm²)";
            }
            else
            {
                this.Text = "全部設備 - 溫度上下限設定";
                lblTitle.Text = "溫度上下限設定 (°C)";
            }
            lblFactoryName.Text = "全部設備上下限設定";

            dgvLimits.Rows.Clear();
            foreach (var factory in factories)
            {
                string upper, lower;
                if (settingType == "Pressure")
                {
                    upper = factory.AlarmLimits.PressureUpperLimit == double.MaxValue ? "" : factory.AlarmLimits.PressureUpperLimit.ToString();
                    lower = factory.AlarmLimits.PressureLowerLimit == double.MinValue ? "" : factory.AlarmLimits.PressureLowerLimit.ToString();
                }
                else
                {
                    upper = factory.AlarmLimits.TempUpperLimit == double.MaxValue ? "" : factory.AlarmLimits.TempUpperLimit.ToString();
                    lower = factory.AlarmLimits.TempLowerLimit == double.MinValue ? "" : factory.AlarmLimits.TempLowerLimit.ToString();
                }

                int rowIndex = dgvLimits.Rows.Add(factory.Name, upper, lower);
                dgvLimits.Rows[rowIndex].Tag = factory.Id;
            }
        }

        /// <summary>
        /// 從資料庫載入推播設定到 UI
        /// </summary>
        private void LoadNotificationSettings()
        {
            try
            {
                if (database == null)
                    return;

                var settings = database.LoadNotificationSettings();

                // 載入推播開關
                if (settings.ContainsKey("teams_enabled"))
                {
                    chkTeamsEnabled.Checked = bool.Parse(settings["teams_enabled"]);
                }

                // 載入推播人員清單
                if (settings.ContainsKey("teams_email"))
                {
                    string emailsString = settings["teams_email"];
                    if (!string.IsNullOrWhiteSpace(emailsString))
                    {
                        char[] separators = new[] { ',', ';' };
                        var emails = emailsString.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var email in emails)
                        {
                            string trimmedEmail = email.Trim();
                            if (!string.IsNullOrWhiteSpace(trimmedEmail))
                            {
                                lstEmails.Items.Add(trimmedEmail);
                            }
                        }
                    }
                }

                // 載入推播間隔時間
                if (settings.ContainsKey("notification_cooldown_minutes"))
                {
                    if (int.TryParse(settings["notification_cooldown_minutes"], out int cooldown))
                    {
                        nudCooldown.Value = Math.Max(1, Math.Min(60, cooldown));
                    }
                }

                // 載入超限延遲時間
                if (settings.ContainsKey("alarm_delay_minutes"))
                {
                    if (int.TryParse(settings["alarm_delay_minutes"], out int delay))
                    {
                        nudDelay.Value = Math.Max(0, Math.Min(60, delay));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"載入推播設定失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            ResultLimitsMap.Clear();

            foreach (DataGridViewRow row in dgvLimits.Rows)
            {
                if (row.Tag == null) continue;
                int factoryId = (int)row.Tag;
                string factoryName = row.Cells[0].Value?.ToString() ?? "";

                string upperText = row.Cells[1].Value?.ToString() ?? "";
                string lowerText = row.Cells[2].Value?.ToString() ?? "";

                double upperLimit = double.MaxValue;
                double lowerLimit = double.MinValue;

                if (!string.IsNullOrWhiteSpace(upperText))
                {
                    if (!double.TryParse(upperText, out upperLimit))
                    {
                        MessageBox.Show($"「{factoryName}」的上限值格式不正確", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(lowerText))
                {
                    if (!double.TryParse(lowerText, out lowerLimit))
                    {
                        MessageBox.Show($"「{factoryName}」的下限值格式不正確", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (upperLimit != double.MaxValue && lowerLimit != double.MinValue && upperLimit <= lowerLimit)
                {
                    MessageBox.Show($"「{factoryName}」的上限值必須大於下限值", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var factory = factories.FirstOrDefault(f => f.Id == factoryId);
                var limits = new AlarmLimitsConfig
                {
                    PressureUpperLimit = factory != null ? factory.AlarmLimits.PressureUpperLimit : double.MaxValue,
                    PressureLowerLimit = factory != null ? factory.AlarmLimits.PressureLowerLimit : double.MinValue,
                    TempUpperLimit = factory != null ? factory.AlarmLimits.TempUpperLimit : double.MaxValue,
                    TempLowerLimit = factory != null ? factory.AlarmLimits.TempLowerLimit : double.MinValue
                };

                if (settingType == "Pressure")
                {
                    limits.PressureUpperLimit = upperLimit;
                    limits.PressureLowerLimit = lowerLimit;
                }
                else
                {
                    limits.TempUpperLimit = upperLimit;
                    limits.TempLowerLimit = lowerLimit;
                }

                ResultLimitsMap[factoryId] = limits;
            }

            // 儲存推播設定到資料庫
            try
            {
                if (database != null)
                {
                    // 儲存推播開關
                    database.SaveNotificationSetting("teams_enabled", chkTeamsEnabled.Checked.ToString().ToLower(), "推播開關");

                    // 儲存推播人員清單
                    var emailList = new List<string>();
                    foreach (var item in lstEmails.Items)
                    {
                        emailList.Add(item.ToString());
                    }
                    string emailsString = string.Join(",", emailList);
                    database.SaveNotificationSetting("teams_email", emailsString, "推播人員清單（逗號分隔）");

                    // 儲存推播間隔時間
                    database.SaveNotificationSetting("notification_cooldown_minutes", nudCooldown.Value.ToString(), "推播間隔時間（分鐘）");

                    // 儲存超限延遲時間
                    database.SaveNotificationSetting("alarm_delay_minutes", nudDelay.Value.ToString(), "超限延遲推播時間（分鐘）");

                    System.Diagnostics.Debug.WriteLine("[AlarmLimitSettingForm] 推播設定已儲存到資料庫");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存推播設定失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvLimits.Rows)
            {
                row.Cells[1].Value = "";
                row.Cells[2].Value = "";
            }
        }

        /// <summary>
        /// 新增推播人員
        /// </summary>
        private void btnAddEmail_Click(object sender, EventArgs e)
        {
            string email = SimpleInputDialog.ShowDialog(
                "請輸入 Email 地址:", 
                "新增推播人員");

            if (!string.IsNullOrWhiteSpace(email))
            {
                email = email.Trim();

                // 簡單的 Email 格式驗證
                if (!email.Contains("@") || !email.Contains("."))
                {
                    MessageBox.Show("Email 格式不正確", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 檢查是否已存在
                foreach (var item in lstEmails.Items)
                {
                    if (item.ToString().Equals(email, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("此 Email 已存在於清單中", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                lstEmails.Items.Add(email);
            }
        }

        /// <summary>
        /// 刪除推播人員
        /// </summary>
        private void btnRemoveEmail_Click(object sender, EventArgs e)
        {
            if (lstEmails.SelectedIndex >= 0)
            {
                lstEmails.Items.RemoveAt(lstEmails.SelectedIndex);
            }
            else
            {
                MessageBox.Show("請先選擇要刪除的人員", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
