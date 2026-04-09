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
        /// 設定完成後的結果 (factoryId → AlarmLimitsConfig)
        /// </summary>
        public Dictionary<int, AlarmLimitsConfig> ResultLimitsMap { get; private set; }

        public AlarmLimitSettingForm(List<FactoryConfig> factories, string settingType)
        {
            InitializeComponent();
            this.factories = factories;
            this.settingType = settingType;
            this.ResultLimitsMap = new Dictionary<int, AlarmLimitsConfig>();

            LoadAllFactories();
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
    }
}
