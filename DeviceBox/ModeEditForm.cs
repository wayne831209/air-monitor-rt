using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace DeviceBox
{
    public partial class ModeEditForm : Form
    {
        public ModeConfig Mode { get; private set; }

        // 顏色定義
        private readonly Color CardBackgroundColor = Color.FromArgb(45, 45, 48);
        private readonly Color CardHoverColor = Color.FromArgb(55, 55, 60);
        private readonly Color AccentColor = Color.FromArgb(0, 122, 204);
        private readonly Color EnabledColor = Color.FromArgb(52, 199, 89);
        private readonly Color DisabledColor = Color.FromArgb(100, 100, 100);
        private readonly Color TextPrimaryColor = Color.White;
        private readonly Color TextSecondaryColor = Color.FromArgb(180, 180, 180);
        private readonly Color DangerColor = Color.FromArgb(255, 59, 48);

        public ModeEditForm(ModeConfig mode)
        {
            Mode = mode.Clone();
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // 設定標題
            labelFormTitle.Text = Mode.Id == 0 ? "新增模式" : "編輯模式";
            this.Text = labelFormTitle.Text;

            // 載入基本資訊
            textBoxName.Text = Mode.Name ?? "";
            textBoxDescription.Text = Mode.Description ?? "";

            // 設定按鈕事件
            buttonAddSchedule.Click += AddScheduleButton_Click;
            buttonCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            buttonConfirm.Click += ConfirmButton_Click;

            RefreshScheduleList();
        }

        private void RefreshScheduleList()
        {
            panelScheduleList.Controls.Clear();
            int yOffset = 5;

            if (Mode.Schedules == null)
                Mode.Schedules = new List<ModeSchedule>();

            foreach (var schedule in Mode.Schedules)
            {
                Panel card = CreateScheduleCard(schedule, yOffset);
                panelScheduleList.Controls.Add(card);
                yOffset += 85;
            }

            if (Mode.Schedules.Count == 0)
            {
                Label emptyLabel = new Label
                {
                    Text = "尚無排程，點擊上方按鈕新增",
                    Location = new Point(10, 10),
                    Size = new Size(panelScheduleList.Width - 20, 30),
                    ForeColor = TextSecondaryColor,
                    Font = new Font("微軟正黑體", 10F),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                panelScheduleList.Controls.Add(emptyLabel);
            }
        }

        private Panel CreateScheduleCard(ModeSchedule schedule, int yOffset)
        {
            Panel card = new Panel
            {
                Location = new Point(5, yOffset),
                Size = new Size(panelScheduleList.Width - 30, 75),
                BackColor = CardBackgroundColor,
                Tag = schedule
            };

            // 圓角
            card.Paint += (s, e) =>
            {
                using (GraphicsPath path = RoundedRect(card.ClientRectangle, 8))
                {
                    card.Region = new Region(path);
                }
            };

            // 時間選擇器 - 開始
            Label startLabel = new Label
            {
                Text = "開始",
                Location = new Point(10, 12),
                Size = new Size(35, 20),
                ForeColor = TextSecondaryColor,
                Font = new Font("微軟正黑體", 9F)
            };
            card.Controls.Add(startLabel);

            DateTimePicker dtpStart = new DateTimePicker
            {
                Location = new Point(50, 8),
                Size = new Size(80, 25),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                Value = DateTime.Today.Add(schedule.StartTime),
                Font = new Font("微軟正黑體", 10F),
                Tag = schedule
            };
            dtpStart.ValueChanged += (s, e) => schedule.StartTime = dtpStart.Value.TimeOfDay;
            card.Controls.Add(dtpStart);

            // 時間選擇器 - 結束
            Label endLabel = new Label
            {
                Text = "結束",
                Location = new Point(140, 12),
                Size = new Size(35, 20),
                ForeColor = TextSecondaryColor,
                Font = new Font("微軟正黑體", 9F)
            };
            card.Controls.Add(endLabel);

            DateTimePicker dtpEnd = new DateTimePicker
            {
                Location = new Point(180, 8),
                Size = new Size(80, 25),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                Value = DateTime.Today.Add(schedule.EndTime),
                Font = new Font("微軟正黑體", 10F),
                Tag = schedule
            };
            dtpEnd.ValueChanged += (s, e) => schedule.EndTime = dtpEnd.Value.TimeOfDay;
            card.Controls.Add(dtpEnd);

            // 開關
            Panel toggleSwitch = CreateToggleSwitch(schedule);
            toggleSwitch.Location = new Point(card.Width - 70, 8);
            card.Controls.Add(toggleSwitch);

            // 星期選擇
            int dayX = 10;
            string[] dayNames = { "日", "一", "二", "三", "四", "五", "六" };
            for (int i = 0; i < 7; i++)
            {
                DayOfWeek day = (DayOfWeek)i;
                bool isSelected = schedule.Days != null && schedule.Days.Contains(day);

                Button dayBtn = new Button
                {
                    Text = dayNames[i],
                    Location = new Point(dayX, 40),
                    Size = new Size(32, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = isSelected ? AccentColor : Color.FromArgb(60, 60, 65),
                    ForeColor = Color.White,
                    Font = new Font("微軟正黑體", 8F, FontStyle.Bold),
                    Tag = new Tuple<ModeSchedule, DayOfWeek>(schedule, day),
                    Cursor = Cursors.Hand
                };
                dayBtn.FlatAppearance.BorderSize = 0;

                dayBtn.Click += (s, e) =>
                {
                    var btn = s as Button;
                    var tuple = btn.Tag as Tuple<ModeSchedule, DayOfWeek>;
                    var sch = tuple.Item1;
                    var d = tuple.Item2;

                    if (sch.Days == null) sch.Days = new List<DayOfWeek>();

                    if (sch.Days.Contains(d))
                    {
                        sch.Days.Remove(d);
                        btn.BackColor = Color.FromArgb(60, 60, 65);
                    }
                    else
                    {
                        sch.Days.Add(d);
                        btn.BackColor = AccentColor;
                    }
                };

                card.Controls.Add(dayBtn);
                dayX += 35;
            }

            // 刪除按鈕
            Button deleteBtn = new Button
            {
                Text = "×",
                Location = new Point(card.Width - 35, 42),
                Size = new Size(25, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = DangerColor,
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 10F, FontStyle.Bold),
                Tag = schedule,
                Cursor = Cursors.Hand
            };
            deleteBtn.FlatAppearance.BorderSize = 0;
            deleteBtn.Click += (s, e) =>
            {
                Mode.Schedules.Remove(schedule);
                RefreshScheduleList();
            };
            card.Controls.Add(deleteBtn);

            return card;
        }

        private Panel CreateToggleSwitch(ModeSchedule schedule)
        {
            Panel togglePanel = new Panel
            {
                Size = new Size(50, 26),
                BackColor = schedule.Enabled ? EnabledColor : DisabledColor,
                Cursor = Cursors.Hand
            };

            togglePanel.Paint += (s, e) =>
            {
                using (GraphicsPath path = RoundedRect(togglePanel.ClientRectangle, 13))
                {
                    togglePanel.Region = new Region(path);
                }
            };

            Panel knob = new Panel
            {
                Size = new Size(20, 20),
                Location = new Point(schedule.Enabled ? 27 : 3, 3),
                BackColor = Color.White
            };
            knob.Paint += (s, e) =>
            {
                using (GraphicsPath path = RoundedRect(knob.ClientRectangle, 10))
                {
                    knob.Region = new Region(path);
                }
            };
            togglePanel.Controls.Add(knob);

            Action toggle = () =>
            {
                schedule.Enabled = !schedule.Enabled;
                togglePanel.BackColor = schedule.Enabled ? EnabledColor : DisabledColor;
                knob.Location = new Point(schedule.Enabled ? 27 : 3, 3);
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

        private void AddScheduleButton_Click(object sender, EventArgs e)
        {
            var newSchedule = new ModeSchedule
            {
                Enabled = true,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(17),
                Days = new List<DayOfWeek>()
            };

            Mode.Schedules.Add(newSchedule);
            RefreshScheduleList();
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            // 驗證
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show("請輸入模式名稱", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxName.Focus();
                return;
            }

            // 更新模式資訊
            Mode.Name = textBoxName.Text.Trim();
            Mode.Description = textBoxDescription.Text.Trim();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
