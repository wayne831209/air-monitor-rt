using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeviceBox
{
    public partial class AlarmLimitSettingForm : Form
    {
        private readonly string factoryName;
        private readonly int factoryId;
        private readonly AlarmLimitsConfig currentLimits;

        /// <summary>
        /// 設定完成後的警報上下限
        /// </summary>
        public AlarmLimitsConfig ResultLimits { get; private set; }

        /// <summary>
        /// 設定類型: "Pressure" 或 "Temp"
        /// </summary>
        private readonly string settingType;

        public AlarmLimitSettingForm(int factoryId, string factoryName, AlarmLimitsConfig currentLimits, string settingType)
        {
            InitializeComponent();
            this.factoryId = factoryId;
            this.factoryName = factoryName;
            this.currentLimits = currentLimits;
            this.settingType = settingType;
            this.ResultLimits = new AlarmLimitsConfig
            {
                PressureUpperLimit = currentLimits.PressureUpperLimit,
                PressureLowerLimit = currentLimits.PressureLowerLimit,
                TempUpperLimit = currentLimits.TempUpperLimit,
                TempLowerLimit = currentLimits.TempLowerLimit
            };

            LoadCurrentValues();
        }

        private void LoadCurrentValues()
        {
            lblFactoryName.Text = factoryName;

            if (settingType == "Pressure")
            {
                this.Text = factoryName + " - 空壓上下限設定";
                lblTitle.Text = "空壓上下限設定 (kg/cm?)";
                txtUpperLimit.Text = currentLimits.PressureUpperLimit == double.MaxValue ? "" : currentLimits.PressureUpperLimit.ToString();
                txtLowerLimit.Text = currentLimits.PressureLowerLimit == double.MinValue ? "" : currentLimits.PressureLowerLimit.ToString();
            }
            else
            {
                this.Text = factoryName + " - 溫度上下限設定";
                lblTitle.Text = "溫度上下限設定 (°C)";
                txtUpperLimit.Text = currentLimits.TempUpperLimit == double.MaxValue ? "" : currentLimits.TempUpperLimit.ToString();
                txtLowerLimit.Text = currentLimits.TempLowerLimit == double.MinValue ? "" : currentLimits.TempLowerLimit.ToString();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            double upperLimit = double.MaxValue;
            double lowerLimit = double.MinValue;

            if (!string.IsNullOrWhiteSpace(txtUpperLimit.Text))
            {
                if (!double.TryParse(txtUpperLimit.Text, out upperLimit))
                {
                    MessageBox.Show("上限值格式不正確", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(txtLowerLimit.Text))
            {
                if (!double.TryParse(txtLowerLimit.Text, out lowerLimit))
                {
                    MessageBox.Show("下限值格式不正確", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (upperLimit != double.MaxValue && lowerLimit != double.MinValue && upperLimit <= lowerLimit)
            {
                MessageBox.Show("上限值必須大於下限值", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (settingType == "Pressure")
            {
                ResultLimits.PressureUpperLimit = upperLimit;
                ResultLimits.PressureLowerLimit = lowerLimit;
            }
            else
            {
                ResultLimits.TempUpperLimit = upperLimit;
                ResultLimits.TempLowerLimit = lowerLimit;
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
            txtUpperLimit.Text = "";
            txtLowerLimit.Text = "";
        }
    }
}
