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
    public partial class ModeSettingForm : Form
    {
        private Config _config;
        private List<ModeConfig> _modes = new List<ModeConfig>();
        private static readonly string ConfigFileName = "config.xml";

        // 顏色定義
        private readonly Color BackgroundColor = Color.FromArgb(30, 30, 30);
        private readonly Color CardBackgroundColor = Color.FromArgb(45, 45, 48);
        private readonly Color CardHoverColor = Color.FromArgb(55, 55, 60);
        private readonly Color AccentColor = Color.FromArgb(0, 122, 204);
        private readonly Color EnabledColor = Color.FromArgb(52, 199, 89);
        private readonly Color DisabledColor = Color.FromArgb(100, 100, 100);
        private readonly Color ActiveModeColor = Color.FromArgb(255, 180, 0);
        private readonly Color TextPrimaryColor = Color.White;
        private readonly Color TextSecondaryColor = Color.FromArgb(180, 180, 180);
        private readonly Color DangerColor = Color.FromArgb(255, 59, 48);

        public ModeSettingForm()
        {
            InitializeComponent();
            LoadConfiguration();
            SetupUI();
        }

        private void LoadConfiguration()
        {
            _config = new Config();
            _config.LoadConfig();
            LoadModes();
        }

        private void LoadModes()
        {
            _modes.Clear();

            // 從 config 載入模式
            if (_config.Modes != null)
            {
                foreach (var mode in _config.Modes)
                {
                    _modes.Add(mode.Clone());
                }
            }

            // 如果沒有任何模式，建立預設模式
            if (_modes.Count == 0)
            {
                _modes.Add(new ModeConfig
                {
                    Id = 1,
                    Name = "高負荷",
                    Description = "高負荷運轉模式",
                    Enabled = true,
                    Schedules = new List<ModeSchedule>
                    {
                        new ModeSchedule
                        {
                            Enabled = true,
                            StartTime = TimeSpan.FromHours(8),
                            EndTime = TimeSpan.FromHours(17),
                            Days = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }
                        }
                    }
                });

                _modes.Add(new ModeConfig
                {
                    Id = 2,
                    Name = "保養",
                    Description = "設備保養模式",
                    Enabled = false,
                    Schedules = new List<ModeSchedule>()
                });
            }
        }

        private void SetupUI()
        {
            // 設定按鈕事件
            buttonAdd.Click += AddButton_Click;
            buttonSave.Click += SaveButton_Click;

            UpdateCurrentModeLabel();
            RefreshModeList();
        }

        private void UpdateCurrentModeLabel()
        {
            var activeMode = _modes.FirstOrDefault(m => m.Enabled && m.IsInSchedule());
            if (activeMode != null)
            {
                labelCurrentMode.Text = $"目前模式：{activeMode.Name}";
                labelCurrentMode.ForeColor = ActiveModeColor;
            }
            else
            {
                var enabledMode = _modes.FirstOrDefault(m => m.Enabled);
                if (enabledMode != null)
                {
                    labelCurrentMode.Text = $"目前模式：{enabledMode.Name} (非排程時間)";
                    labelCurrentMode.ForeColor = Color.FromArgb(200, 230, 255);
                }
                else
                {
                    labelCurrentMode.Text = "目前模式：無";
                    labelCurrentMode.ForeColor = Color.FromArgb(200, 230, 255);
                }
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
                yOffset += 140;
            }

            UpdateCurrentModeLabel();
        }

        private Panel CreateModeCard(ModeConfig mode, int yOffset)
        {
            bool isActive = mode.Enabled && mode.IsInSchedule();

            Panel card = new Panel
            {
                Location = new Point(5, yOffset),
                Size = new Size(panelModeList.Width - 30, 130),
                BackColor = CardBackgroundColor,
                Tag = mode,
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

                // 如果是當前模式，畫邊框
                if (isActive)
                {
                    using (Pen pen = new Pen(ActiveModeColor, 3))
                    {
                        e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 3, card.Height - 3);
                    }
                }
            };

            // 模式名稱
            Label nameLabel = new Label
            {
                Text = mode.Name,
                Location = new Point(15, 15),
                Size = new Size(200, 30),
                Font = new Font("微軟正黑體", 16F, FontStyle.Bold),
                ForeColor = isActive ? ActiveModeColor : (mode.Enabled ? TextPrimaryColor : DisabledColor),
                BackColor = Color.Transparent
            };
            card.Controls.Add(nameLabel);

            // 狀態標籤
            if (isActive)
            {
                Label activeLabel = new Label
                {
                    Text = "● 運轉中",
                    Location = new Point(220, 20),
                    Size = new Size(80, 20),
                    Font = new Font("微軟正黑體", 9F, FontStyle.Bold),
                    ForeColor = ActiveModeColor,
                    BackColor = Color.Transparent
                };
                card.Controls.Add(activeLabel);
            }

            // 開關
            Panel toggleSwitch = CreateToggleSwitch(mode);
            toggleSwitch.Location = new Point(card.Width - 70, 20);
            card.Controls.Add(toggleSwitch);

            // 描述
            Label descLabel = new Label
            {
                Text = mode.Description ?? "",
                Location = new Point(15, 50),
                Size = new Size(card.Width - 140, 25),
                Font = new Font("微軟正黑體", 9F, FontStyle.Regular),
                ForeColor = TextSecondaryColor,
                BackColor = Color.Transparent
            };
            card.Controls.Add(descLabel);

            // 排程資訊
            string scheduleText = GetScheduleDisplayText(mode);
            Label scheduleLabel = new Label
            {
                Text = scheduleText,
                Location = new Point(15, 75),
                Size = new Size(card.Width - 140, 20),
                Font = new Font("微軟正黑體", 9F, FontStyle.Regular),
                ForeColor = mode.Enabled ? EnabledColor : DisabledColor,
                BackColor = Color.Transparent
            };
            card.Controls.Add(scheduleLabel);

            // 編輯按鈕
            Button editButton = new Button
            {
                Text = "編輯",
                Location = new Point(card.Width - 120, 90),
                Size = new Size(50, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentColor,
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 9F),
                Cursor = Cursors.Hand,
                Tag = mode
            };
            editButton.FlatAppearance.BorderSize = 0;
            editButton.Click += EditButton_Click;
            card.Controls.Add(editButton);

            // 刪除按鈕
            Button deleteButton = new Button
            {
                Text = "刪除",
                Location = new Point(card.Width - 65, 90),
                Size = new Size(50, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = DangerColor,
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 9F),
                Cursor = Cursors.Hand,
                Tag = mode
            };
            deleteButton.FlatAppearance.BorderSize = 0;
            deleteButton.Click += DeleteButton_Click;
            card.Controls.Add(deleteButton);

            // 滑鼠效果
            card.MouseEnter += (s, e) => card.BackColor = CardHoverColor;
            card.MouseLeave += (s, e) => card.BackColor = CardBackgroundColor;

            return card;
        }

        private string GetScheduleDisplayText(ModeConfig mode)
        {
            if (mode.Schedules == null || mode.Schedules.Count == 0)
                return "無排程";

            var enabledSchedules = mode.Schedules.Where(s => s.Enabled).ToList();
            if (enabledSchedules.Count == 0)
                return "無排程";

            var texts = enabledSchedules.Select(s => s.GetDisplayText() + " " + s.GetDaysDisplayText());
            return string.Join(" | ", texts);
        }

        private Panel CreateToggleSwitch(ModeConfig mode)
        {
            Panel togglePanel = new Panel
            {
                Size = new Size(55, 30),
                BackColor = mode.Enabled ? EnabledColor : DisabledColor,
                Tag = mode,
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
                Location = new Point(mode.Enabled ? 28 : 3, 3),
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
                mode.Enabled = !mode.Enabled;
                togglePanel.BackColor = mode.Enabled ? EnabledColor : DisabledColor;
                knob.Location = new Point(mode.Enabled ? 28 : 3, 3);
                RefreshModeList();
            };

            togglePanel.Click += (s, e) => toggle();
            knob.Click += (s, e) => toggle();

            return togglePanel;
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
            // 產生新 ID
            int newId = _modes.Count > 0 ? _modes.Max(m => m.Id) + 1 : 1;

            var newMode = new ModeConfig
            {
                Id = newId,
                Name = "新模式",
                Description = "",
                Enabled = false,
                Schedules = new List<ModeSchedule>()
            };

            using (var editForm = new ModeEditForm(newMode))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    _modes.Add(editForm.Mode);
                    RefreshModeList();
                }
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            ModeConfig mode = btn.Tag as ModeConfig;

            using (var editForm = new ModeEditForm(mode.Clone()))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    int index = _modes.FindIndex(m => m.Id == mode.Id);
                    if (index >= 0)
                    {
                        _modes[index] = editForm.Mode;
                    }
                    RefreshModeList();
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            ModeConfig mode = btn.Tag as ModeConfig;

            var result = MessageBox.Show(
                $"確定要刪除模式「{mode.Name}」嗎？",
                "確認刪除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                _modes.RemoveAll(m => m.Id == mode.Id);
                RefreshModeList();
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveToConfig();
                MessageBox.Show("模式設定已儲存！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveToConfig()
        {
            string configPath = Path.Combine(Application.StartupPath, ConfigFileName);
            XDocument doc = XDocument.Load(configPath);

            // 找到或建立 Modes 節點
            var modesElement = doc.Root.Element("Modes");
            if (modesElement == null)
            {
                modesElement = new XElement("Modes");
                doc.Root.Add(modesElement);
            }
            else
            {
                // 清除舊的模式
                modesElement.RemoveAll();
            }

            // 儲存每個模式
            foreach (var mode in _modes)
            {
                var modeElement = new XElement("Mode");
                modeElement.SetAttributeValue("id", mode.Id);
                modeElement.SetAttributeValue("name", mode.Name);
                modeElement.SetAttributeValue("enabled", mode.Enabled.ToString().ToLower());

                if (!string.IsNullOrEmpty(mode.Description))
                {
                    modeElement.SetAttributeValue("description", mode.Description);
                }

                // 儲存排程
                if (mode.Schedules != null && mode.Schedules.Count > 0)
                {
                    var schedulesElement = new XElement("Schedules");

                    foreach (var schedule in mode.Schedules)
                    {
                        var scheduleElement = new XElement("Schedule");
                        scheduleElement.SetAttributeValue("enabled", schedule.Enabled.ToString().ToLower());
                        scheduleElement.SetAttributeValue("start", schedule.StartTime.ToString(@"hh\:mm"));
                        scheduleElement.SetAttributeValue("end", schedule.EndTime.ToString(@"hh\:mm"));

                        if (schedule.Days != null && schedule.Days.Count > 0 && schedule.Days.Count < 7)
                        {
                            string daysStr = string.Join(",", schedule.Days.Select(d => (int)d));
                            scheduleElement.SetAttributeValue("days", daysStr);
                        }

                        schedulesElement.Add(scheduleElement);
                    }

                    modeElement.Add(schedulesElement);
                }

                modesElement.Add(modeElement);
            }

            doc.Save(configPath);
        }
    }
}
