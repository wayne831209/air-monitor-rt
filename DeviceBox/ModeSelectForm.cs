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
    public partial class ModeSelectForm : Form
    {
        private List<ScheduleMode> _modes = new List<ScheduleMode>();
        private ScheduleMode _selectedMode;
        private static readonly string ConfigFileName = "config.xml";

        // 顏色定義
        private readonly Color BackgroundColor = Color.FromArgb(30, 30, 30);
        private readonly Color CardBackgroundColor = Color.FromArgb(45, 45, 48);
        private readonly Color CardHoverColor = Color.FromArgb(55, 55, 60);
        private readonly Color CardSelectedColor = Color.FromArgb(0, 122, 204);
        private readonly Color AccentColor = Color.FromArgb(0, 122, 204);
        private readonly Color TextPrimaryColor = Color.White;
        private readonly Color TextSecondaryColor = Color.FromArgb(180, 180, 180);
        private readonly Color DangerColor = Color.FromArgb(255, 59, 48);
        private readonly Color SuccessColor = Color.FromArgb(52, 199, 89);
        private readonly Color WarningColor = Color.FromArgb(255, 140, 0);

        // 控制項
        private Panel panelModeList;
        private Button buttonAdd;
        private Button buttonConfirm;
        private Button buttonCancel;
        private Label labelTitle;

        /// <summary>
        /// 取得選擇的模式
        /// </summary>
        public ScheduleMode SelectedMode => _selectedMode;

        public ModeSelectForm()
        {
            InitializeComponent();
            LoadModes();
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ClientSize = new Size(500, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModeSelectForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "選擇排程模式";
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            // 標題
            labelTitle = new Label
            {
                Text = "選擇排程模式",
                Location = new Point(20, 15),
                Size = new Size(460, 40),
                Font = new Font("微軟正黑體", 18F, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                BackColor = Color.Transparent
            };
            this.Controls.Add(labelTitle);

            // 模式列表面板
            panelModeList = new Panel
            {
                Location = new Point(20, 65),
                Size = new Size(460, 340),
                BackColor = BackgroundColor,
                AutoScroll = true
            };
            this.Controls.Add(panelModeList);

            // 新增按鈕
            buttonAdd = new Button
            {
                Text = "＋ 新增模式",
                Location = new Point(20, 420),
                Size = new Size(140, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentColor,
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            buttonAdd.FlatAppearance.BorderSize = 0;
            buttonAdd.Click += ButtonAdd_Click;
            this.Controls.Add(buttonAdd);

            // 取消按鈕
            buttonCancel = new Button
            {
                Text = "取消",
                Location = new Point(280, 420),
                Size = new Size(95, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 12F),
                Cursor = Cursors.Hand
            };
            buttonCancel.FlatAppearance.BorderSize = 0;
            buttonCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(buttonCancel);

            // 確認按鈕
            buttonConfirm = new Button
            {
                Text = "確認",
                Location = new Point(385, 420),
                Size = new Size(95, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = SuccessColor,
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            buttonConfirm.FlatAppearance.BorderSize = 0;
            buttonConfirm.Click += ButtonConfirm_Click;
            this.Controls.Add(buttonConfirm);

            RefreshModeList();
        }

        private void LoadModes()
        {
            _modes.Clear();

            try
            {
                string configPath = Path.Combine(Application.StartupPath, ConfigFileName);
                if (!File.Exists(configPath)) return;

                XDocument doc = XDocument.Load(configPath);
                var modesElement = doc.Root?.Element("Modes");

                if (modesElement != null)
                {
                    foreach (var modeElement in modesElement.Elements("Mode"))
                    {
                        var mode = new ScheduleMode
                        {
                            Id = int.Parse(modeElement.Attribute("id")?.Value ?? "0"),
                            Name = modeElement.Attribute("name")?.Value ?? "",
                            Description = modeElement.Attribute("description")?.Value ?? "",
                            IsDefault = bool.Parse(modeElement.Attribute("isDefault")?.Value ?? "false")
                        };

                        // 載入該模式的排程
                        foreach (var scheduleElement in modeElement.Elements("Schedule"))
                        {
                            var schedule = new ModeScheduleItem
                            {
                                FactoryId = int.Parse(scheduleElement.Attribute("factoryId")?.Value ?? "0"),
                                FactoryName = scheduleElement.Attribute("factoryName")?.Value ?? "",
                                DeviceName = scheduleElement.Attribute("deviceName")?.Value ?? "",
                                MachineNo = int.Parse(scheduleElement.Attribute("machineNo")?.Value ?? "1"),
                                Enabled = bool.Parse(scheduleElement.Attribute("enabled")?.Value ?? "true"),
                                StartTime = TimeSpan.Parse(scheduleElement.Attribute("start")?.Value ?? "08:00"),
                                EndTime = TimeSpan.Parse(scheduleElement.Attribute("end")?.Value ?? "17:00")
                            };

                            // 載入星期
                            string daysStr = scheduleElement.Attribute("days")?.Value;
                            if (!string.IsNullOrEmpty(daysStr))
                            {
                                foreach (var dayNum in daysStr.Split(','))
                                {
                                    if (int.TryParse(dayNum.Trim(), out int d))
                                    {
                                        schedule.Days.Add((DayOfWeek)d);
                                    }
                                }
                            }

                            mode.Schedules.Add(schedule);
                        }

                        _modes.Add(mode);
                    }
                }

                // 如果沒有模式，新增預設模式
                if (_modes.Count == 0)
                {
                    _modes.Add(new ScheduleMode { Id = 1, Name = "模式一", Description = "一般模式", IsDefault = true });
                    _modes.Add(new ScheduleMode { Id = 2, Name = "高負荷", Description = "高負荷運轉模式", IsDefault = false });
                    SaveModes();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Load modes failed: " + ex.Message);
                _modes.Add(new ScheduleMode { Id = 1, Name = "模式一", Description = "一般模式", IsDefault = true });
                _modes.Add(new ScheduleMode { Id = 2, Name = "高負荷", Description = "高負荷運轉模式", IsDefault = false });
            }
        }

        private void RefreshModeList()
        {
            panelModeList.Controls.Clear();
            int yOffset = 5;

            foreach (var mode in _modes)
            {
                Panel card = CreateModeCard(mode, yOffset);
                panelModeList.Controls.Add(card);
                yOffset += 85;
            }
        }

        private Panel CreateModeCard(ScheduleMode mode, int yOffset)
        {
            bool isSelected = _selectedMode != null && _selectedMode.Id == mode.Id;

            Panel card = new Panel
            {
                Location = new Point(5, yOffset),
                Size = new Size(panelModeList.Width - 30, 75),
                BackColor = isSelected ? CardSelectedColor : CardBackgroundColor,
                Tag = mode,
                Cursor = Cursors.Hand
            };
            card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // 圓角效果
            card.Paint += (s, e) =>
            {
                using (GraphicsPath path = RoundedRect(card.ClientRectangle, 8))
                {
                    card.Region = new Region(path);
                }
            };

            // 模式名稱
            Label nameLabel = new Label
            {
                Text = mode.Name,
                Location = new Point(15, 10),
                Size = new Size(200, 25),
                Font = new Font("微軟正黑體", 14F, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                BackColor = Color.Transparent
            };
            card.Controls.Add(nameLabel);

            // 模式描述
            Label descLabel = new Label
            {
                Text = mode.Description,
                Location = new Point(15, 35),
                Size = new Size(200, 18),
                Font = new Font("微軟正黑體", 9F),
                ForeColor = TextSecondaryColor,
                BackColor = Color.Transparent
            };
            card.Controls.Add(descLabel);

            // 排程數量
            int scheduleCount = mode.Schedules.Count(s => s.Enabled);
            Label scheduleLabel = new Label
            {
                Text = scheduleCount > 0 ? $"已設定 {scheduleCount} 個排程" : "尚未設定排程",
                Location = new Point(15, 53),
                Size = new Size(150, 18),
                Font = new Font("微軟正黑體", 9F),
                ForeColor = scheduleCount > 0 ? SuccessColor : WarningColor,
                BackColor = Color.Transparent
            };
            card.Controls.Add(scheduleLabel);

            // 預設標籤
            if (mode.IsDefault)
            {
                Label defaultLabel = new Label
                {
                    Text = "預設",
                    Location = new Point(card.Width - 100, 10),
                    Size = new Size(40, 18),
                    Font = new Font("微軟正黑體", 9F),
                    ForeColor = SuccessColor,
                    BackColor = Color.Transparent
                };
                card.Controls.Add(defaultLabel);
            }

            // 編輯按鈕
            Button editButton = new Button
            {
                Text = "編輯",
                Location = new Point(card.Width - 55, 35),
                Size = new Size(45, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 9F),
                Cursor = Cursors.Hand,
                Tag = mode
            };
            editButton.FlatAppearance.BorderSize = 0;
            editButton.Click += EditButton_Click;
            card.Controls.Add(editButton);

            // 選擇事件
            Action selectAction = () =>
            {
                _selectedMode = mode;
                RefreshModeList();
            };

            card.Click += (s, e) => selectAction();
            nameLabel.Click += (s, e) => selectAction();
            descLabel.Click += (s, e) => selectAction();
            scheduleLabel.Click += (s, e) => selectAction();

            // 滑鼠效果
            card.MouseEnter += (s, e) =>
            {
                if (_selectedMode == null || _selectedMode.Id != mode.Id)
                    card.BackColor = CardHoverColor;
            };
            card.MouseLeave += (s, e) =>
            {
                if (_selectedMode == null || _selectedMode.Id != mode.Id)
                    card.BackColor = CardBackgroundColor;
            };

            return card;
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

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            using (var editForm = new ModeEditForm(null))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    var newMode = editForm.Mode;
                    newMode.Id = _modes.Count > 0 ? _modes.Max(m => m.Id) + 1 : 1;
                    _modes.Add(newMode);
                    SaveModes();
                    RefreshModeList();
                }
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            ScheduleMode mode = btn.Tag as ScheduleMode;

            using (var editForm = new ModeEditForm(mode))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    var editedMode = editForm.Mode;
                    int index = _modes.FindIndex(m => m.Id == mode.Id);
                    if (index >= 0)
                    {
                        editedMode.Id = mode.Id;
                        editedMode.Schedules = mode.Schedules; // 保留原有排程
                        _modes[index] = editedMode;
                        SaveModes();
                        RefreshModeList();
                    }
                }
            }
        }

        private void ButtonConfirm_Click(object sender, EventArgs e)
        {
            if (_selectedMode == null)
            {
                MessageBox.Show("請選擇一個模式", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 套用選擇的模式排程到設備
            ApplyModeSchedules(_selectedMode);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 將模式的排程套用到設備設定
        /// </summary>
        private void ApplyModeSchedules(ScheduleMode mode)
        {
            try
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

                // 套用模式的排程
                foreach (var schedule in mode.Schedules.Where(s => s.Enabled))
                {
                    var factoryElement = doc.Descendants("Factory")
                        .FirstOrDefault(f => int.Parse(f.Attribute("id")?.Value ?? "0") == schedule.FactoryId);

                    if (factoryElement != null)
                    {
                        var deviceElement = factoryElement.Descendants("Device")
                            .FirstOrDefault(d =>
                                d.Attribute("name")?.Value == schedule.DeviceName &&
                                int.Parse(d.Attribute("machineNo")?.Value ?? "0") == schedule.MachineNo);

                        if (deviceElement != null)
                        {
                            var scheduleElement = deviceElement.Element("Schedule");
                            if (scheduleElement == null)
                            {
                                scheduleElement = new XElement("Schedule");
                                deviceElement.Add(scheduleElement);
                            }

                            scheduleElement.SetAttributeValue("enabled", "true");

                            var rangeElement = new XElement("TimeRange");
                            rangeElement.SetAttributeValue("start", schedule.StartTime.ToString(@"hh\:mm"));
                            rangeElement.SetAttributeValue("end", schedule.EndTime.ToString(@"hh\:mm"));

                            if (schedule.Days.Count > 0 && schedule.Days.Count < 7)
                            {
                                string daysStr = string.Join(",", schedule.Days.Select(d => (int)d));
                                rangeElement.SetAttributeValue("days", daysStr);
                            }

                            scheduleElement.Add(rangeElement);
                        }
                    }
                }

                doc.Save(configPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"套用排程失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveModes()
        {
            try
            {
                string configPath = Path.Combine(Application.StartupPath, ConfigFileName);
                XDocument doc;

                if (File.Exists(configPath))
                {
                    doc = XDocument.Load(configPath);
                }
                else
                {
                    doc = new XDocument(new XElement("Setting"));
                }

                // 移除舊的 Modes 元素
                doc.Root.Element("Modes")?.Remove();

                // 建立新的 Modes 元素
                var modesElement = new XElement("Modes");
                foreach (var mode in _modes)
                {
                    var modeElement = new XElement("Mode");
                    modeElement.SetAttributeValue("id", mode.Id);
                    modeElement.SetAttributeValue("name", mode.Name);
                    modeElement.SetAttributeValue("description", mode.Description);
                    modeElement.SetAttributeValue("isDefault", mode.IsDefault.ToString().ToLower());

                    // 儲存該模式的排程
                    foreach (var schedule in mode.Schedules)
                    {
                        var scheduleElement = new XElement("Schedule");
                        scheduleElement.SetAttributeValue("factoryId", schedule.FactoryId);
                        scheduleElement.SetAttributeValue("factoryName", schedule.FactoryName);
                        scheduleElement.SetAttributeValue("deviceName", schedule.DeviceName);
                        scheduleElement.SetAttributeValue("machineNo", schedule.MachineNo);
                        scheduleElement.SetAttributeValue("enabled", schedule.Enabled.ToString().ToLower());
                        scheduleElement.SetAttributeValue("start", schedule.StartTime.ToString(@"hh\:mm"));
                        scheduleElement.SetAttributeValue("end", schedule.EndTime.ToString(@"hh\:mm"));

                        if (schedule.Days.Count > 0 && schedule.Days.Count < 7)
                        {
                            string daysStr = string.Join(",", schedule.Days.Select(d => (int)d));
                            scheduleElement.SetAttributeValue("days", daysStr);
                        }

                        modeElement.Add(scheduleElement);
                    }

                    modesElement.Add(modeElement);
                }

                doc.Root.Add(modesElement);
                doc.Save(configPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存模式失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 靜態方法：取得模式
        /// </summary>
        public static ScheduleMode GetModeById(int modeId)
        {
            try
            {
                string configPath = Path.Combine(Application.StartupPath, ConfigFileName);
                if (!File.Exists(configPath)) return null;

                XDocument doc = XDocument.Load(configPath);
                var modeElement = doc.Root?.Element("Modes")?.Elements("Mode")
                    .FirstOrDefault(m => int.Parse(m.Attribute("id")?.Value ?? "0") == modeId);

                if (modeElement != null)
                {
                    var mode = new ScheduleMode
                    {
                        Id = int.Parse(modeElement.Attribute("id")?.Value ?? "0"),
                        Name = modeElement.Attribute("name")?.Value ?? "",
                        Description = modeElement.Attribute("description")?.Value ?? "",
                        IsDefault = bool.Parse(modeElement.Attribute("isDefault")?.Value ?? "false")
                    };

                    foreach (var scheduleElement in modeElement.Elements("Schedule"))
                    {
                        var schedule = new ModeScheduleItem
                        {
                            FactoryId = int.Parse(scheduleElement.Attribute("factoryId")?.Value ?? "0"),
                            FactoryName = scheduleElement.Attribute("factoryName")?.Value ?? "",
                            DeviceName = scheduleElement.Attribute("deviceName")?.Value ?? "",
                            MachineNo = int.Parse(scheduleElement.Attribute("machineNo")?.Value ?? "1"),
                            Enabled = bool.Parse(scheduleElement.Attribute("enabled")?.Value ?? "true"),
                            StartTime = TimeSpan.Parse(scheduleElement.Attribute("start")?.Value ?? "08:00"),
                            EndTime = TimeSpan.Parse(scheduleElement.Attribute("end")?.Value ?? "17:00")
                        };

                        string daysStr = scheduleElement.Attribute("days")?.Value;
                        if (!string.IsNullOrEmpty(daysStr))
                        {
                            foreach (var dayNum in daysStr.Split(','))
                            {
                                if (int.TryParse(dayNum.Trim(), out int d))
                                    schedule.Days.Add((DayOfWeek)d);
                            }
                        }

                        mode.Schedules.Add(schedule);
                    }

                    return mode;
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// 靜態方法：儲存模式排程
        /// </summary>
        public static void SaveModeSchedules(ScheduleMode mode)
        {
            try
            {
                string configPath = Path.Combine(Application.StartupPath, ConfigFileName);
                XDocument doc = XDocument.Load(configPath);

                var modeElement = doc.Root?.Element("Modes")?.Elements("Mode")
                    .FirstOrDefault(m => int.Parse(m.Attribute("id")?.Value ?? "0") == mode.Id);

                if (modeElement != null)
                {
                    // 移除舊的排程
                    modeElement.Elements("Schedule").Remove();

                    // 新增新的排程
                    foreach (var schedule in mode.Schedules)
                    {
                        var scheduleElement = new XElement("Schedule");
                        scheduleElement.SetAttributeValue("factoryId", schedule.FactoryId);
                        scheduleElement.SetAttributeValue("factoryName", schedule.FactoryName);
                        scheduleElement.SetAttributeValue("deviceName", schedule.DeviceName);
                        scheduleElement.SetAttributeValue("machineNo", schedule.MachineNo);
                        scheduleElement.SetAttributeValue("enabled", schedule.Enabled.ToString().ToLower());
                        scheduleElement.SetAttributeValue("start", schedule.StartTime.ToString(@"hh\:mm"));
                        scheduleElement.SetAttributeValue("end", schedule.EndTime.ToString(@"hh\:mm"));

                        if (schedule.Days.Count > 0 && schedule.Days.Count < 7)
                        {
                            string daysStr = string.Join(",", schedule.Days.Select(d => (int)d));
                            scheduleElement.SetAttributeValue("days", daysStr);
                        }

                        modeElement.Add(scheduleElement);
                    }

                    doc.Save(configPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存排程失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// 排程模式
    /// </summary>
    public class ScheduleMode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public List<ModeScheduleItem> Schedules { get; set; } = new List<ModeScheduleItem>();
    }

    /// <summary>
    /// 模式排程項目
    /// </summary>
    public class ModeScheduleItem
    {
        public int FactoryId { get; set; }
        public string FactoryName { get; set; }
        public string DeviceName { get; set; }
        public int MachineNo { get; set; }
        public bool Enabled { get; set; } = true;
        public TimeSpan StartTime { get; set; } = TimeSpan.FromHours(8);
        public TimeSpan EndTime { get; set; } = TimeSpan.FromHours(17);
        public List<DayOfWeek> Days { get; set; } = new List<DayOfWeek>();

        public string GetTimeDisplayText()
        {
            return $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        }

        public string GetDaysDisplayText()
        {
            if (Days == null || Days.Count == 0 || Days.Count == 7)
                return "每天";

            if (Days.Count == 5 && !Days.Contains(DayOfWeek.Saturday) && !Days.Contains(DayOfWeek.Sunday))
                return "平日";

            if (Days.Count == 2 && Days.Contains(DayOfWeek.Saturday) && Days.Contains(DayOfWeek.Sunday))
                return "週末";

            string[] dayNames = { "日", "一", "二", "三", "四", "五", "六" };
            var sortedDays = Days.OrderBy(d => (int)d).Select(d => "週" + dayNames[(int)d]);
            return string.Join(", ", sortedDays);
        }
    }

    /// <summary>
    /// 模式編輯對話框
    /// </summary>
    public class ModeEditForm : Form
    {
        private TextBox textBoxName;
        private TextBox textBoxDescription;
        private CheckBox checkBoxDefault;
        private Button buttonSave;
        private Button buttonCancel;

        public ScheduleMode Mode { get; private set; }

        public ModeEditForm(ScheduleMode mode)
        {
            Mode = mode != null ? new ScheduleMode
            {
                Id = mode.Id,
                Name = mode.Name,
                Description = mode.Description,
                IsDefault = mode.IsDefault,
                Schedules = mode.Schedules
            } : new ScheduleMode();

            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ClientSize = new Size(400, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = Mode.Id > 0 ? "編輯模式" : "新增模式";

            // 名稱標籤
            Label labelName = new Label
            {
                Text = "模式名稱：",
                Location = new Point(20, 30),
                Size = new Size(100, 25),
                Font = new Font("微軟正黑體", 12F),
                ForeColor = Color.White
            };
            this.Controls.Add(labelName);

            // 名稱輸入框
            textBoxName = new TextBox
            {
                Location = new Point(130, 28),
                Size = new Size(250, 30),
                Font = new Font("微軟正黑體", 12F),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(textBoxName);

            // 描述標籤
            Label labelDesc = new Label
            {
                Text = "描述：",
                Location = new Point(20, 80),
                Size = new Size(100, 25),
                Font = new Font("微軟正黑體", 12F),
                ForeColor = Color.White
            };
            this.Controls.Add(labelDesc);

            // 描述輸入框
            textBoxDescription = new TextBox
            {
                Location = new Point(130, 78),
                Size = new Size(250, 30),
                Font = new Font("微軟正黑體", 12F),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(textBoxDescription);

            // 預設選項
            checkBoxDefault = new CheckBox
            {
                Text = "設為預設模式",
                Location = new Point(130, 130),
                Size = new Size(200, 25),
                Font = new Font("微軟正黑體", 11F),
                ForeColor = Color.White
            };
            this.Controls.Add(checkBoxDefault);

            // 取消按鈕
            buttonCancel = new Button
            {
                Text = "取消",
                Location = new Point(180, 185),
                Size = new Size(95, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 11F),
                Cursor = Cursors.Hand
            };
            buttonCancel.FlatAppearance.BorderSize = 0;
            buttonCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(buttonCancel);

            // 儲存按鈕
            buttonSave = new Button
            {
                Text = "儲存",
                Location = new Point(285, 185),
                Size = new Size(95, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 199, 89),
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            buttonSave.FlatAppearance.BorderSize = 0;
            buttonSave.Click += ButtonSave_Click;
            this.Controls.Add(buttonSave);
        }

        private void LoadData()
        {
            textBoxName.Text = Mode.Name;
            textBoxDescription.Text = Mode.Description;
            checkBoxDefault.Checked = Mode.IsDefault;
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show("請輸入模式名稱", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Mode.Name = textBoxName.Text.Trim();
            Mode.Description = textBoxDescription.Text.Trim();
            Mode.IsDefault = checkBoxDefault.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
