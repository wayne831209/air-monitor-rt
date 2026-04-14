using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace DeviceBox
{
    public partial class ScheduleEditForm : Form
    {
        private Config _config;
        private HashSet<int> _factoryIdFilter;
        public ScheduleItem ScheduleItem { get; private set; }

        // 控制項
        private CheckBox[] _dayCheckBoxes;
        private Panel _togglePanel;
        private Panel _toggleKnob;

        // 顏色
        private readonly Color BackgroundColor = Color.FromArgb(30, 30, 30);
        private readonly Color CardBackgroundColor = Color.FromArgb(45, 45, 48);
        private readonly Color AccentColor = Color.FromArgb(0, 122, 204);
        private readonly Color EnabledColor = Color.FromArgb(52, 199, 89);
        private readonly Color DisabledColor = Color.FromArgb(100, 100, 100);
        private readonly Color TextPrimaryColor = Color.White;
        private readonly Color TextSecondaryColor = Color.FromArgb(180, 180, 180);

        public ScheduleEditForm(Config config, ScheduleItem existingItem, HashSet<int> factoryIdFilter = null)
        {
            _config = config;
            _factoryIdFilter = factoryIdFilter;
            ScheduleItem = existingItem != null ? CloneScheduleItem(existingItem) : new ScheduleItem
            {
                Enabled = true,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(17),
                Days = new List<DayOfWeek>()
            };

            InitializeComponent();
            SetupUI();

            if (existingItem != null)
            {
                LoadExistingData();
            }
        }

        private ScheduleItem CloneScheduleItem(ScheduleItem item)
        {
            return new ScheduleItem
            {
                FactoryId = item.FactoryId,
                FactoryName = item.FactoryName,
                DeviceName = item.DeviceName,
                DeviceType = item.DeviceType,
                MachineNo = item.MachineNo,
                Enabled = item.Enabled,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                Days = new List<DayOfWeek>(item.Days ?? new List<DayOfWeek>())
            };
        }

        private void SetupUI()
        {
            // 設定標題
            labelFormTitle.Text = ScheduleItem.DeviceName == null ? "新增排程" : "編輯排程";
            this.Text = labelFormTitle.Text;

            // 載入工廠下拉選單（依篩選條件）
            foreach (var factory in _config.Factories)
            {
                if (_factoryIdFilter != null && !_factoryIdFilter.Contains(factory.Id))
                    continue;
                comboBoxFactory.Items.Add(new ComboBoxItem { Text = factory.Name, Value = factory });
            }
            comboBoxFactory.SelectedIndexChanged += ComboBoxFactory_SelectedIndexChanged;

            // 設定時間選擇器
            dateTimePickerStart.Value = DateTime.Today.Add(ScheduleItem.StartTime);
            dateTimePickerEnd.Value = DateTime.Today.Add(ScheduleItem.EndTime);
            dateTimePickerStart.ValueChanged += (s, e) => UpdateDurationLabel();
            dateTimePickerEnd.ValueChanged += (s, e) => UpdateDurationLabel();

            // 設定全天 CheckBox
            checkBox24Hours.CheckedChanged += (s, e) =>
            {
                bool is24 = checkBox24Hours.Checked;
                dateTimePickerStart.Enabled = !is24;
                dateTimePickerEnd.Enabled = !is24;
                if (is24)
                {
                    dateTimePickerStart.Value = DateTime.Today;
                    dateTimePickerEnd.Value = DateTime.Today.AddHours(23).AddMinutes(59);
                }
                UpdateDurationLabel();
            };

            // 設定開關
            SetupToggleSwitch();

            // 設定星期選擇
            SetupDayButtons();

            // 設定按鈕事件
            buttonCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            buttonConfirm.Click += ConfirmButton_Click;

            UpdateDurationLabel();
        }

        private void UpdateDurationLabel()
        {
            TimeSpan start = dateTimePickerStart.Value.TimeOfDay;
            TimeSpan end = dateTimePickerEnd.Value.TimeOfDay;
            TimeSpan duration = end >= start ? end - start : TimeSpan.FromHours(24) - start + end;
            labelDuration.Text = $"運轉 {duration.TotalHours:F1} 小時";
        }

        private void SetupToggleSwitch()
        {
            _togglePanel = new Panel
            {
                Size = new Size(55, 30),
                Location = new Point(panelEnable.Width - 75, 25),
                BackColor = ScheduleItem.Enabled ? EnabledColor : DisabledColor,
                Cursor = Cursors.Hand
            };
            _togglePanel.Paint += (s, e) =>
            {
                using (GraphicsPath path = RoundedRect(_togglePanel.ClientRectangle, 15))
                {
                    _togglePanel.Region = new Region(path);
                }
            };

            _toggleKnob = new Panel
            {
                Size = new Size(24, 24),
                Location = new Point(ScheduleItem.Enabled ? 28 : 3, 3),
                BackColor = Color.White
            };
            _toggleKnob.Paint += (s, e) =>
            {
                using (GraphicsPath path = RoundedRect(_toggleKnob.ClientRectangle, 12))
                {
                    _toggleKnob.Region = new Region(path);
                }
            };
            _togglePanel.Controls.Add(_toggleKnob);

            Action toggle = () =>
            {
                ScheduleItem.Enabled = !ScheduleItem.Enabled;
                _togglePanel.BackColor = ScheduleItem.Enabled ? EnabledColor : DisabledColor;
                _toggleKnob.Location = new Point(ScheduleItem.Enabled ? 28 : 3, 3);
            };

            _togglePanel.Click += (s, e) => toggle();
            _toggleKnob.Click += (s, e) => toggle();

            panelEnable.Controls.Add(_togglePanel);
        }

        private void SetupDayButtons()
        {
            string[] dayNames = { "日", "一", "二", "三", "四", "五", "六" };
            _dayCheckBoxes = new CheckBox[7];

            int xOffset = 15;
            for (int i = 0; i < 7; i++)
            {
                DayOfWeek day = (DayOfWeek)i;
                bool isSelected = ScheduleItem.Days != null && ScheduleItem.Days.Contains(day);

                Panel dayPanel = new Panel
                {
                    Location = new Point(xOffset, 30),
                    Size = new Size(45, 45),
                    BackColor = isSelected ? AccentColor : CardBackgroundColor,
                    Cursor = Cursors.Hand,
                    Tag = day
                };

                dayPanel.Paint += (s, e) =>
                {
                    using (GraphicsPath path = RoundedRect(dayPanel.ClientRectangle, 8))
                    {
                        dayPanel.Region = new Region(path);
                    }
                };

                Label dayLabel = new Label
                {
                    Text = dayNames[i],
                    Dock = DockStyle.Fill,
                    Font = new Font("微軟正黑體", 12F, FontStyle.Bold),
                    ForeColor = TextPrimaryColor,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                dayPanel.Controls.Add(dayLabel);

                CheckBox cb = new CheckBox
                {
                    Visible = false,
                    Checked = isSelected,
                    Tag = day
                };
                _dayCheckBoxes[i] = cb;
                panelDays.Controls.Add(cb);

                Panel currentPanel = dayPanel;
                dayPanel.Click += (s, e) =>
                {
                    cb.Checked = !cb.Checked;
                    currentPanel.BackColor = cb.Checked ? AccentColor : CardBackgroundColor;
                };

                dayLabel.Click += (s, e) =>
                {
                    cb.Checked = !cb.Checked;
                    currentPanel.BackColor = cb.Checked ? AccentColor : CardBackgroundColor;
                };

                panelDays.Controls.Add(dayPanel);
                xOffset += 50;
            }
        }

        private void ComboBoxFactory_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxDevice.Items.Clear();

            if (comboBoxFactory.SelectedItem is ComboBoxItem item && item.Value is FactoryConfig factory)
            {
                var compressors = factory.GetDevicesByType(DeviceType.Compressor);
                foreach (var compressor in compressors)
                {
                    comboBoxDevice.Items.Add(new ComboBoxItem
                    {
                        Text = compressor.Name,
                        Value = compressor
                    });
                }

                if (comboBoxDevice.Items.Count > 0)
                {
                    comboBoxDevice.SelectedIndex = 0;
                }
            }
        }

        private void LoadExistingData()
        {
            // 選擇對應的工廠
            for (int i = 0; i < comboBoxFactory.Items.Count; i++)
            {
                if (comboBoxFactory.Items[i] is ComboBoxItem item && item.Value is FactoryConfig factory)
                {
                    if (factory.Id == ScheduleItem.FactoryId)
                    {
                        comboBoxFactory.SelectedIndex = i;
                        break;
                    }
                }
            }

            // 選擇對應的設備
            for (int i = 0; i < comboBoxDevice.Items.Count; i++)
            {
                if (comboBoxDevice.Items[i] is ComboBoxItem item && item.Value is DeviceConfig device)
                {
                    if (device.Name == ScheduleItem.DeviceName && device.MachineNo == ScheduleItem.MachineNo)
                    {
                        comboBoxDevice.SelectedIndex = i;
                        break;
                    }
                }
            }

            // 設定時間
            dateTimePickerStart.Value = DateTime.Today.Add(ScheduleItem.StartTime);
            dateTimePickerEnd.Value = DateTime.Today.Add(ScheduleItem.EndTime);

            // 如果開始為 00:00、結束為 23:59 則自動勾選全天
            if (ScheduleItem.StartTime == TimeSpan.Zero && ScheduleItem.EndTime == new TimeSpan(23, 59, 0))
            {
                checkBox24Hours.Checked = true;
            }
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            // 驗證
            if (comboBoxFactory.SelectedItem == null)
            {
                MessageBox.Show("請選擇工廠", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBoxDevice.SelectedItem == null)
            {
                MessageBox.Show("請選擇設備", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 更新資料
            var factoryItem = comboBoxFactory.SelectedItem as ComboBoxItem;
            var deviceItem = comboBoxDevice.SelectedItem as ComboBoxItem;
            var factory = factoryItem.Value as FactoryConfig;
            var device = deviceItem.Value as DeviceConfig;

            ScheduleItem.FactoryId = factory.Id;
            ScheduleItem.FactoryName = factory.Name;
            ScheduleItem.DeviceName = device.Name;
            ScheduleItem.DeviceType = device.Type;
            ScheduleItem.MachineNo = device.MachineNo;
            ScheduleItem.StartTime = dateTimePickerStart.Value.TimeOfDay;
            ScheduleItem.EndTime = dateTimePickerEnd.Value.TimeOfDay;

            // 收集選擇的星期
            ScheduleItem.Days = new List<DayOfWeek>();
            foreach (var cb in _dayCheckBoxes)
            {
                if (cb.Checked)
                {
                    ScheduleItem.Days.Add((DayOfWeek)cb.Tag);
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

    }

    /// <summary>
    /// ComboBox 項目
    /// </summary>
    public class ComboBoxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
