using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DeviceBox
{
    public partial class ScheduleSettingForm : Form
    {
        private Config _config;
        private List<ScheduleItem> _scheduleItems = new List<ScheduleItem>();
        private static readonly string ConfigFileName = "config.xml";
        private int _modeId;
        private string _modeName;
        private HashSet<int> _factoryIdFilter;

        // 顏色定義
        private readonly Color BackgroundColor = Color.FromArgb(30, 30, 30);
        private readonly Color CardBackgroundColor = Color.FromArgb(45, 45, 48);
        private readonly Color CardHoverColor = Color.FromArgb(55, 55, 60);
        private readonly Color AccentColor = Color.FromArgb(0, 122, 204);
        private readonly Color EnabledColor = Color.FromArgb(52, 199, 89);
        private readonly Color DisabledColor = Color.FromArgb(100, 100, 100);
        private readonly Color TextPrimaryColor = Color.White;
        private readonly Color TextSecondaryColor = Color.FromArgb(180, 180, 180);
        private readonly Color DangerColor = Color.FromArgb(255, 59, 48);

        public ScheduleSettingForm() : this(0, "", null)
        {
        }

        public ScheduleSettingForm(int modeId, string modeName, HashSet<int> factoryIdFilter = null)
        {
            _modeId = modeId;
            _modeName = modeName;
            _factoryIdFilter = factoryIdFilter;
            InitializeComponent();
            LoadConfiguration();
            SetupUI();
        }

        private void LoadConfiguration()
        {
            _config = new Config();
            _config.LoadConfig();
            LoadScheduleItems();
        }

        private void LoadScheduleItems()
        {
            _scheduleItems.Clear();

            // 如果有指定模式ID，從模式中載入排程
            if (_modeId > 0)
            {
                var mode = ModeSelectForm.GetModeById(_modeId);
                if (mode != null && mode.Schedules.Count > 0)
                {
                    foreach (var schedule in mode.Schedules)
                    {
                        _scheduleItems.Add(new ScheduleItem
                        {
                            FactoryId = schedule.FactoryId,
                            FactoryName = schedule.FactoryName,
                            DeviceName = schedule.DeviceName,
                            DeviceType = DeviceType.Compressor,
                            MachineNo = schedule.MachineNo,
                            Enabled = schedule.Enabled,
                            IsSpanMode = schedule.IsSpanMode,
                            StartDay = schedule.StartDay,
                            StartTime = schedule.StartTime,
                            EndDay = schedule.EndDay,
                            EndTime = schedule.EndTime,
                            RepeatDays = new List<DayOfWeek>(schedule.RepeatDays ?? new List<DayOfWeek>())
                        });
                    }
                    // 依工廠篩選
                    if (_factoryIdFilter != null)
                    {
                        _scheduleItems = _scheduleItems.Where(s => _factoryIdFilter.Contains(s.FactoryId)).ToList();
                    }
                    _scheduleItems = _scheduleItems.OrderBy(s => s.DeviceName).ToList();
                    return;
                }
            }

            // 沒有模式排程時，顯示空列表（讓使用者新增）
            // 不再從設備載入預設排程
        }

        private void SetupUI()
        {
            // 設定標題（包含模式名稱）
            if (!string.IsNullOrEmpty(_modeName))
            {
                labelTitle.Text = $"排程設定 - {_modeName}";
                this.Text = $"排程設定 - {_modeName}";
            }
            else
            {
                labelTitle.Text = "排程設定";
            }

            // 設定按鈕事件
            buttonAdd.Click += AddButton_Click;
            buttonSave.Click += SaveButton_Click;
            buttonViewWeekly.Click += ViewWeeklyButton_Click;

            RefreshScheduleList();
        }

        /// <summary>
        /// 週排程檢視按鈕點擊事件
        /// </summary>
        private void ViewWeeklyButton_Click(object sender, EventArgs e)
        {
            using (var weeklyView = new WeeklyScheduleViewForm(_scheduleItems, _modeName))
            {
                weeklyView.ShowDialog();
            }
        }

        private void RefreshScheduleList()
        {
            panelScheduleList.Controls.Clear();
            int yOffset = 5;

            foreach (var item in _scheduleItems)
            {
                Panel card = CreateScheduleCard(item, yOffset);
                panelScheduleList.Controls.Add(card);
                yOffset += 130;
            }
        }

        private Panel CreateScheduleCard(ScheduleItem item, int yOffset)
        {
            Panel card = new Panel
            {
                Location = new Point(5, yOffset),
                Size = new Size(panelScheduleList.Width - 30, 120),
                BackColor = CardBackgroundColor,
                Tag = item,
                Cursor = Cursors.Hand
            };
            card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // 圓角效果
            card.Paint += (s, e) =>
            {
                using (GraphicsPath path = RoundedRect(card.ClientRectangle, 10))
                {
                    card.Region = new Region(path);
                }
            };

            // 時間顯示
            Label timeLabel = new Label
            {
                Text = item.GetTimeDisplayText(),
                Location = new Point(15, 15),
                Size = new Size(card.Width - 80, 35),
                Font = new Font("微軟正黑體", 14F, FontStyle.Bold),
                ForeColor = item.Enabled ? TextPrimaryColor : DisabledColor,
                BackColor = Color.Transparent
            };
            card.Controls.Add(timeLabel);

            // 開關
            Panel toggleSwitch = CreateToggleSwitch(item);
            toggleSwitch.Location = new Point(card.Width - 70, 20);
            card.Controls.Add(toggleSwitch);

            // 設備名稱
            Label deviceLabel = new Label
            {
                Text = $"{item.FactoryName} - {item.DeviceName}",
                Location = new Point(15, 55),
                Size = new Size(card.Width - 140, 25),
                Font = new Font("微軟正黑體", 10F, FontStyle.Regular),
                ForeColor = TextSecondaryColor,
                BackColor = Color.Transparent
            };
            card.Controls.Add(deviceLabel);

            // 星期顯示
            string[] dayNamesZh = { "週日", "週一", "週二", "週三", "週四", "週五", "週六" };
            string daysText;
            if (item.IsSpanMode)
            {
                daysText = $"跨日: {dayNamesZh[(int)item.StartDay]} ~ {dayNamesZh[(int)item.EndDay]}";
            }
            else
            {
                if (item.RepeatDays == null || item.RepeatDays.Count == 0)
                    daysText = "重複: 每天";
                else
                    daysText = "重複: " + string.Join(", ", item.RepeatDays.OrderBy(d => (int)d).Select(d => dayNamesZh[(int)d]));
            }
            Label daysLabel = new Label
            {
                Text = daysText,
                Location = new Point(15, 85),
                Size = new Size(card.Width - 140, 25),
                Font = new Font("微軟正黑體", 9F, FontStyle.Regular),
                ForeColor = item.Enabled ? EnabledColor : DisabledColor,
                BackColor = Color.Transparent
            };
            card.Controls.Add(daysLabel);

            // 編輯按鈕
            Button editButton = new Button
            {
                Text = "編輯",
                Location = new Point(card.Width - 120, 75),
                Size = new Size(50, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentColor,
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 9F),
                Cursor = Cursors.Hand,
                Tag = item
            };
            editButton.FlatAppearance.BorderSize = 0;
            editButton.Click += EditButton_Click;
            card.Controls.Add(editButton);

            // 刪除按鈕
            Button deleteButton = new Button
            {
                Text = "刪除",
                Location = new Point(card.Width - 65, 75),
                Size = new Size(50, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = DangerColor,
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 9F),
                Cursor = Cursors.Hand,
                Tag = item
            };
            deleteButton.FlatAppearance.BorderSize = 0;
            deleteButton.Click += DeleteButton_Click;
            card.Controls.Add(deleteButton);

            // 滑鼠效果
            card.MouseEnter += (s, e) => card.BackColor = CardHoverColor;
            card.MouseLeave += (s, e) => card.BackColor = CardBackgroundColor;

            return card;
        }

        private Panel CreateToggleSwitch(ScheduleItem item)
        {
            Panel togglePanel = new Panel
            {
                Size = new Size(55, 30),
                BackColor = item.Enabled ? EnabledColor : DisabledColor,
                Tag = item,
                Cursor = Cursors.Hand
            };

            // 圓角
            togglePanel.Paint += (s, e) =>
            {
                using (GraphicsPath path = RoundedRect(togglePanel.ClientRectangle, 15))
                {
                    togglePanel.Region = new Region(path);
                }
            };

            // 滑塊
            Panel knob = new Panel
            {
                Size = new Size(24, 24),
                Location = new Point(item.Enabled ? 28 : 3, 3),
                BackColor = Color.White
            };
            knob.Paint += (s, e) =>
            {
                using (GraphicsPath path = RoundedRect(knob.ClientRectangle, 12))
                {
                    knob.Region = new Region(path);
                }
            };
            togglePanel.Controls.Add(knob);

            // 點擊切換
            Action toggle = () =>
            {
                item.Enabled = !item.Enabled;
                togglePanel.BackColor = item.Enabled ? EnabledColor : DisabledColor;
                knob.Location = new Point(item.Enabled ? 28 : 3, 3);
                RefreshScheduleList();
            };

            togglePanel.Click += (s, e) => toggle();
            knob.Click += (s, e) => toggle();

            return togglePanel;
        }

        private string GetDaysDisplayText(List<DayOfWeek> days)
        {
            if (days == null || days.Count == 0)
                return "每天";

            if (days.Count == 7)
                return "每天";

            if (days.Count == 5 && !days.Contains(DayOfWeek.Saturday) && !days.Contains(DayOfWeek.Sunday))
                return "平日 (週一至週五)";

            if (days.Count == 2 && days.Contains(DayOfWeek.Saturday) && days.Contains(DayOfWeek.Sunday))
                return "週末";

            string[] dayNames = { "日", "一", "二", "三", "四", "五", "六" };
            var sortedDays = days.OrderBy(d => (int)d).Select(d => "週" + dayNames[(int)d]);
            return string.Join(", ", sortedDays);
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

        private void AddButton_Click(object sender, EventArgs e)
        {
            using (var editForm = new ScheduleEditForm(_config, null, _factoryIdFilter))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    _scheduleItems.Add(editForm.ScheduleItem);
                    _scheduleItems = _scheduleItems.OrderBy(s => s.DeviceName).ToList();
                    RefreshScheduleList();
                }
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            ScheduleItem item = btn.Tag as ScheduleItem;

            using (var editForm = new ScheduleEditForm(_config, item, _factoryIdFilter))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    int index = _scheduleItems.IndexOf(item);
                    if (index >= 0)
                    {
                        _scheduleItems[index] = editForm.ScheduleItem;
                    }
                    _scheduleItems = _scheduleItems.OrderBy(s => s.DeviceName).ToList();
                    RefreshScheduleList();
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            ScheduleItem item = btn.Tag as ScheduleItem;

            var result = MessageBox.Show(
                $"確定要刪除 {item.FactoryName} - {item.DeviceName} 的排程嗎？",
                "確認刪除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                _scheduleItems.Remove(item);
                RefreshScheduleList();
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveToMode();
                MessageBox.Show("排程設定已儲存！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 儲存排程到模式
        /// </summary>
        private void SaveToMode()
        {
            if (_modeId <= 0)
            {
                MessageBox.Show("未指定模式，無法儲存", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var mode = ModeSelectForm.GetModeById(_modeId);
            if (mode == null)
            {
                MessageBox.Show("找不到指定的模式", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 保留其他廠域的排程，只更新目前篩選廠域的排程
            if (_factoryIdFilter != null)
            {
                mode.Schedules.RemoveAll(s => _factoryIdFilter.Contains(s.FactoryId));
            }
            else
            {
                mode.Schedules.Clear();
            }

            foreach (var item in _scheduleItems)
            {
                mode.Schedules.Add(new ModeScheduleItem
                {
                    FactoryId = item.FactoryId,
                    FactoryName = item.FactoryName,
                    DeviceName = item.DeviceName,
                    MachineNo = item.MachineNo,
                    Enabled = item.Enabled,
                    IsSpanMode = item.IsSpanMode,
                    StartDay = item.StartDay,
                    StartTime = item.StartTime,
                    EndDay = item.EndDay,
                    EndTime = item.EndTime,
                    RepeatDays = new List<DayOfWeek>(item.RepeatDays ?? new List<DayOfWeek>())
                });
            }

            // 儲存模式排程
            ModeSelectForm.SaveModeSchedules(mode);

            // 重新讀取完整模式（含所有廠域排程），再套用到設備設定
            var fullMode = ModeSelectForm.GetModeById(_modeId);
            if (fullMode != null)
            {
                ModeSelectForm.ApplyModeSchedulesToConfig(fullMode);
            }
        }

        /// <summary>
        /// 將排程套用到設備設定
        /// </summary>
        private void ApplySchedulesToDevices()
        {
            string configPath = Path.Combine(Application.StartupPath, ConfigFileName);
            XDocument doc = XDocument.Load(configPath);

            // 先清除所有設備的排程
            foreach (var deviceElement in doc.Descendants("Device"))
            {
                var scheduleElement = deviceElement.Element("Schedule");
                if (scheduleElement != null)
                {
                    scheduleElement.SetAttributeValue("enabled", "false");
                    scheduleElement.Elements("TimeRange").Remove();
                }
            }

            // 依設備分組套用排程
            var groupedItems = _scheduleItems.Where(i => i.Enabled)
                .GroupBy(i => new { i.FactoryId, i.DeviceName, i.MachineNo });

            foreach (var group in groupedItems)
            {
                var factoryElement = doc.Descendants("Factory")
                    .FirstOrDefault(f => int.Parse(f.Attribute("id")?.Value ?? "0") == group.Key.FactoryId);

                if (factoryElement != null)
                {
                    var deviceElement = factoryElement.Descendants("Device")
                        .FirstOrDefault(d =>
                            d.Attribute("name")?.Value == group.Key.DeviceName &&
                            int.Parse(d.Attribute("machineNo")?.Value ?? "0") == group.Key.MachineNo);

                    if (deviceElement != null)
                    {
                        var scheduleElement = deviceElement.Element("Schedule");
                        if (scheduleElement == null)
                        {
                            scheduleElement = new XElement("Schedule");
                            deviceElement.Add(scheduleElement);
                        }

                        scheduleElement.SetAttributeValue("enabled", "true");

                        foreach (var item in group)
                        {
                            var rangeElement = new XElement("TimeRange");
                            rangeElement.SetAttributeValue("startDay", item.StartDay.ToString());
                            rangeElement.SetAttributeValue("start", item.StartTime.ToString(@"hh\:mm"));
                            rangeElement.SetAttributeValue("endDay", item.EndDay.ToString());
                            rangeElement.SetAttributeValue("end", item.EndTime.ToString(@"hh\:mm"));

                            if (item.Days != null && item.Days.Count > 0 && item.Days.Count < 7)
                            {
                                string daysStr = string.Join(",", item.Days.Select(d => (int)d));
                                rangeElement.SetAttributeValue("days", daysStr);
                            }

                            scheduleElement.Add(rangeElement);
                        }
                    }
                }
            }

            doc.Save(configPath);
        }

        private void SaveToConfig()
        {
            string configPath = Path.Combine(Application.StartupPath, ConfigFileName);
            XDocument doc = XDocument.Load(configPath);

            // 依設備分組
            var groupedItems = _scheduleItems.GroupBy(i => new { i.FactoryId, i.DeviceType, i.MachineNo });

            foreach (var group in groupedItems)
            {
                var factoryElement = doc.Descendants("Factory")
                    .FirstOrDefault(f => int.Parse(f.Attribute("id")?.Value ?? "0") == group.Key.FactoryId);

                if (factoryElement != null)
                {
                    var deviceElement = factoryElement.Descendants("Device")
                        .FirstOrDefault(d =>
                            d.Attribute("type")?.Value == group.Key.DeviceType.ToString() &&
                            int.Parse(d.Attribute("machineNo")?.Value ?? "0") == group.Key.MachineNo);

                    if (deviceElement != null)
                    {
                        var scheduleElement = deviceElement.Element("Schedule");
                        if (scheduleElement == null)
                        {
                            scheduleElement = new XElement("Schedule");
                            deviceElement.Add(scheduleElement);
                        }

                        // 檢查是否有任何啟用的排程
                        bool anyEnabled = group.Any(i => i.Enabled);
                        scheduleElement.SetAttributeValue("enabled", anyEnabled.ToString().ToLower());

                        // 移除舊的 start/end 屬性（如果存在）
                        scheduleElement.Attribute("start")?.Remove();
                        scheduleElement.Attribute("end")?.Remove();
                        scheduleElement.Attribute("days")?.Remove();

                        // 移除舊的 TimeRange 子元素
                        scheduleElement.Elements("TimeRange").Remove();

                        // 新增每個排程項目為 TimeRange 子元素
                        foreach (var item in group.Where(i => i.Enabled))
                        {
                            var rangeElement = new XElement("TimeRange");
                            rangeElement.SetAttributeValue("startDay", item.StartDay.ToString());
                            rangeElement.SetAttributeValue("start", item.StartTime.ToString(@"hh\:mm"));
                            rangeElement.SetAttributeValue("endDay", item.EndDay.ToString());
                            rangeElement.SetAttributeValue("end", item.EndTime.ToString(@"hh\:mm"));

                            // 儲存 days
                            if (item.Days != null && item.Days.Count > 0 && item.Days.Count < 7)
                            {
                                string daysStr = string.Join(",", item.Days.Select(d => (int)d));
                                rangeElement.SetAttributeValue("days", daysStr);
                            }

                            scheduleElement.Add(rangeElement);
                        }
                    }
                }
            }

            doc.Save(configPath);
        }

    }

    /// <summary>
    /// 排程項目
    /// </summary>
    public class ScheduleItem
    {
        private static readonly string[] DayNamesZh = { "週日", "週一", "週二", "週三", "週四", "週五", "週六" };

        public int FactoryId { get; set; }
        public string FactoryName { get; set; }
        public string DeviceName { get; set; }
        public DeviceType DeviceType { get; set; }
        public int MachineNo { get; set; }
        public bool Enabled { get; set; }

        /// <summary>true=跨日連續模式, false=重複模式</summary>
        public bool IsSpanMode { get; set; } = true;

        // 跨日模式用
        public DayOfWeek StartDay { get; set; } = DayOfWeek.Monday;
        public DayOfWeek EndDay { get; set; } = DayOfWeek.Friday;

        // 重複模式用
        public List<DayOfWeek> RepeatDays { get; set; } = new List<DayOfWeek>();

        // 共用
        public TimeSpan StartTime { get; set; } = TimeSpan.FromHours(8);
        public TimeSpan EndTime { get; set; } = TimeSpan.FromHours(17);

        /// <summary>
        /// 向後相容：涵蓋的星期清單
        /// </summary>
        public List<DayOfWeek> Days
        {
            get
            {
                if (!IsSpanMode)
                    return RepeatDays != null && RepeatDays.Count > 0 ? new List<DayOfWeek>(RepeatDays) : new List<DayOfWeek>();

                var list = new List<DayOfWeek>();
                int s = (int)StartDay;
                int e = (int)EndDay;
                if (s <= e)
                {
                    for (int d = s; d <= e; d++)
                        list.Add((DayOfWeek)d);
                }
                else
                {
                    for (int d = s; d <= 6; d++)
                        list.Add((DayOfWeek)d);
                    for (int d = 0; d <= e; d++)
                        list.Add((DayOfWeek)d);
                }
                return list;
            }
            set { }
        }

        public string GetTimeDisplayText()
        {
            if (IsSpanMode)
                return $"{DayNamesZh[(int)StartDay]} {StartTime:hh\\:mm} - {DayNamesZh[(int)EndDay]} {EndTime:hh\\:mm}";

            string daysText = RepeatDays != null && RepeatDays.Count > 0
                ? string.Join(",", RepeatDays.OrderBy(d => (int)d).Select(d => DayNamesZh[(int)d]))
                : "每天";
            return $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm} ({daysText})";
        }
    }
}
