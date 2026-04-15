using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace DeviceBox
{
    /// <summary>
    /// 週排程檢視表單 - 顯示週一到週日的排程狀態
    /// </summary>
    public class WeeklyScheduleViewForm : Form
    {
        private List<ScheduleItem> _scheduleItems;
        private string _modeName;
        private Panel panelContent;
        private Panel panelLegend;

        // 顏色定義
        private readonly Color BackgroundColor = Color.FromArgb(30, 30, 30);
        private readonly Color HeaderColor = Color.FromArgb(0, 122, 204);
        private readonly Color CardBackgroundColor = Color.FromArgb(45, 45, 48);
        private readonly Color TextPrimaryColor = Color.White;
        private readonly Color TextSecondaryColor = Color.FromArgb(180, 180, 180);
        private readonly Color ScheduleActiveColor = Color.FromArgb(52, 199, 89);
        private readonly Color ScheduleInactiveColor = Color.FromArgb(80, 80, 80);
        private readonly Color TimeSlotColor = Color.FromArgb(0, 150, 136);
        private readonly Color GridLineColor = Color.FromArgb(60, 60, 60);

        // 時間軸設定
        private const int START_HOUR = 0;
        private const int END_HOUR = 24;
        private const int HOUR_WIDTH = 35;
        private const int ROW_HEIGHT = 50;
        private const int DEVICE_LABEL_WIDTH = 120;
        private const int DAY_LABEL_WIDTH = 50;

        public WeeklyScheduleViewForm(List<ScheduleItem> scheduleItems, string modeName)
        {
            _scheduleItems = scheduleItems;
            _modeName = modeName;
            InitializeComponent();
            if (!string.IsNullOrEmpty(_modeName))
            {
                this.Text = "週排程檢視 - " + _modeName;
            }
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "週排程檢視";
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            // 標題
            Panel panelTitle = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = HeaderColor
            };
            Label labelTitle = new Label
            {
                Text = $"週排程檢視 - {_modeName}",
                Dock = DockStyle.Fill,
                Font = new Font("微軟正黑體", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelTitle.Controls.Add(labelTitle);
            this.Controls.Add(panelTitle);

            // 圖例面板
            panelLegend = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = BackgroundColor,
                Padding = new Padding(20, 5, 20, 5)
            };
            CreateLegend();
            this.Controls.Add(panelLegend);
            panelLegend.BringToFront();

            // 內容面板
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackgroundColor,
                AutoScroll = true,
                Padding = new Padding(10)
            };
            this.Controls.Add(panelContent);
            panelContent.BringToFront();

            // 底部按鈕
            Panel panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = BackgroundColor,
                Padding = new Padding(15, 10, 15, 10)
            };
            Button buttonClose = new Button
            {
                Text = "關閉",
                Dock = DockStyle.Right,
                Width = 100,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                Font = new Font("微軟正黑體", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            buttonClose.FlatAppearance.BorderSize = 0;
            buttonClose.Click += (s, e) => this.Close();
            panelBottom.Controls.Add(buttonClose);
            this.Controls.Add(panelBottom);

            // 繪製週排程
            DrawWeeklySchedule();
        }

        private void CreateLegend()
        {
            FlowLayoutPanel flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            // 圖例項目
            AddLegendItem(flowPanel, ScheduleActiveColor, "排程時段");
            AddLegendItem(flowPanel, ScheduleInactiveColor, "非排程時段");

            panelLegend.Controls.Add(flowPanel);
        }

        private void AddLegendItem(FlowLayoutPanel panel, Color color, string text)
        {
            Panel colorBox = new Panel
            {
                Size = new Size(20, 20),
                BackColor = color,
                Margin = new Padding(10, 5, 5, 5)
            };
            Label label = new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = TextPrimaryColor,
                Font = new Font("微軟正黑體", 10F),
                Margin = new Padding(0, 7, 20, 5)
            };
            panel.Controls.Add(colorBox);
            panel.Controls.Add(label);
        }

        private void DrawWeeklySchedule()
        {
            panelContent.Controls.Clear();

            // 取得所有設備
            var devices = _scheduleItems
                .GroupBy(s => new { s.FactoryName, s.DeviceName })
                .Select(g => new { g.Key.FactoryName, g.Key.DeviceName, Schedules = g.ToList() })
                .ToList();

            if (devices.Count == 0)
            {
                Label noDataLabel = new Label
                {
                    Text = "尚無排程資料",
                    ForeColor = TextSecondaryColor,
                    Font = new Font("微軟正黑體", 14F),
                    AutoSize = true,
                    Location = new Point(panelContent.Width / 2 - 60, panelContent.Height / 2 - 20)
                };
                panelContent.Controls.Add(noDataLabel);
                return;
            }

            int yOffset = 10;

            foreach (var device in devices)
            {
                // 計算排程時數
                double weeklyTotalHours;
                Dictionary<DayOfWeek, double> dailyHours;
                CalculateWeeklyScheduledHours(device.Schedules, out weeklyTotalHours, out dailyHours);

                // 設備標題（含週排程總時數）
                string titleText = device.FactoryName + " - " + device.DeviceName
                    + "    排程總時數: " + weeklyTotalHours.ToString("F1") + " 小時";
                Label deviceLabel = new Label
                {
                    Text = titleText,
                    Location = new Point(10, yOffset),
                    Size = new Size(panelContent.Width - 40, 30),
                    Font = new Font("微軟正黑體", 12F, FontStyle.Bold),
                    ForeColor = TextPrimaryColor
                };
                panelContent.Controls.Add(deviceLabel);
                yOffset += 35;

                // 建立週排程表格
                Panel schedulePanel = CreateDeviceSchedulePanel(device.Schedules);
                schedulePanel.Location = new Point(10, yOffset);
                panelContent.Controls.Add(schedulePanel);
                yOffset += schedulePanel.Height + 5;

                // 每日排程時數摘要
                Panel summaryPanel = CreateDailySummaryPanel(dailyHours, weeklyTotalHours);
                summaryPanel.Location = new Point(10, yOffset);
                panelContent.Controls.Add(summaryPanel);
                yOffset += summaryPanel.Height + 20;
            }
        }

        private Panel CreateDeviceSchedulePanel(List<ScheduleItem> deviceSchedules)
        {
            int totalWidth = DAY_LABEL_WIDTH + (END_HOUR - START_HOUR) * HOUR_WIDTH + 20;
            int totalHeight = 30 + 7 * ROW_HEIGHT + 10;

            Panel panel = new Panel
            {
                Size = new Size(totalWidth, totalHeight),
                BackColor = CardBackgroundColor
            };

            // 繪製時間軸標題
            for (int hour = START_HOUR; hour <= END_HOUR; hour += 2)
            {
                Label hourLabel = new Label
                {
                    Text = $"{hour:00}",
                    Location = new Point(DAY_LABEL_WIDTH + (hour - START_HOUR) * HOUR_WIDTH - 10, 5),
                    Size = new Size(30, 20),
                    Font = new Font("微軟正黑體", 9F),
                    ForeColor = TextSecondaryColor,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                panel.Controls.Add(hourLabel);
            }

            string[] dayNames = { "週日", "週一", "週二", "週三", "週四", "週五", "週六" };
            int[] dayOrder = { 1, 2, 3, 4, 5, 6, 0 };

            for (int i = 0; i < 7; i++)
            {
                int dayIndex = dayOrder[i];
                DayOfWeek day = (DayOfWeek)dayIndex;
                int rowY = 30 + i * ROW_HEIGHT;

                Label dayLabel = new Label
                {
                    Text = dayNames[dayIndex],
                    Location = new Point(5, rowY + 15),
                    Size = new Size(DAY_LABEL_WIDTH - 10, 20),
                    Font = new Font("微軟正黑體", 10F),
                    ForeColor = TextPrimaryColor,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                panel.Controls.Add(dayLabel);

                Panel timelinePanel = new Panel
                {
                    Location = new Point(DAY_LABEL_WIDTH, rowY),
                    Size = new Size((END_HOUR - START_HOUR) * HOUR_WIDTH, ROW_HEIGHT - 5),
                    BackColor = ScheduleInactiveColor
                };

                timelinePanel.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(GridLineColor, 1))
                    {
                        for (int hour = START_HOUR; hour <= END_HOUR; hour++)
                        {
                            int x = (hour - START_HOUR) * HOUR_WIDTH;
                            e.Graphics.DrawLine(pen, x, 0, x, timelinePanel.Height);
                        }
                    }
                };

                // 繪製排程時段 - 使用連續週排程邏輯
                foreach (var schedule in deviceSchedules.Where(s => s.Enabled))
                {
                    double dayStartHour, dayEndHour;
                    if (GetDayActiveRange(schedule, day, out dayStartHour, out dayEndHour))
                    {
                        int startX = (int)((dayStartHour - START_HOUR) * HOUR_WIDTH);
                        int endX = (int)((dayEndHour - START_HOUR) * HOUR_WIDTH);
                        int width = Math.Max(endX - startX, 3);

                        Panel timeSlot = new Panel
                        {
                            Location = new Point(startX, 5),
                            Size = new Size(width, ROW_HEIGHT - 15),
                            BackColor = ScheduleActiveColor
                        };
                        AddTimeSlotTooltip(timeSlot, schedule);
                        timelinePanel.Controls.Add(timeSlot);
                    }
                }

                panel.Controls.Add(timelinePanel);
            }

            return panel;
        }

        /// <summary>
        /// 判斷某一天在排程中的運轉時段
        /// 支援跨日模式和重複模式
        /// </summary>
        private bool GetDayActiveRange(ScheduleItem schedule, DayOfWeek day, out double startHour, out double endHour)
        {
            startHour = 0;
            endHour = 0;

            if (!schedule.IsSpanMode)
            {
                // 重複模式：檢查該天是否在 RepeatDays 中
                bool isActive = schedule.RepeatDays == null || schedule.RepeatDays.Count == 0 ||
                                schedule.RepeatDays.Contains(day);
                if (!isActive) return false;

                startHour = schedule.StartTime.TotalHours;
                endHour = schedule.EndTime.TotalHours;
                if (endHour <= startHour) endHour = 24; // 跨午夜時在當天顯示到24
                return endHour > startHour;
            }

            // 跨日模式
            int sd = (int)schedule.StartDay;
            int ed = (int)schedule.EndDay;
            int d = (int)day;

            bool isDayInSpan;
            if (sd <= ed)
            {
                isDayInSpan = d >= sd && d <= ed;
            }
            else
            {
                isDayInSpan = d >= sd || d <= ed;
            }

            if (!isDayInSpan)
                return false;

            bool isStartDay = d == sd;
            bool isEndDay = d == ed;

            if (isStartDay && isEndDay)
            {
                // 同一天開始和結束
                if (sd <= ed)
                {
                    startHour = schedule.StartTime.TotalHours;
                    endHour = schedule.EndTime.TotalHours;
                }
                else
                {
                    // 跨週且同一天：例如 Wed 20:00 ~ Wed 08:00 (運轉6奩24小時+這天的部分)
                    startHour = 0;
                    endHour = 24;
                }
            }
            else if (isStartDay)
            {
                startHour = schedule.StartTime.TotalHours;
                endHour = 24;
            }
            else if (isEndDay)
            {
                startHour = 0;
                endHour = schedule.EndTime.TotalHours;
            }
            else
            {
                // 中間的天 → 全天24小時
                startHour = 0;
                endHour = 24;
            }

            return endHour > startHour;
        }

        private void AddTimeSlotTooltip(Panel panel, ScheduleItem schedule)
        {
            ToolTip tooltip = new ToolTip();
            string timeText = schedule.GetTimeDisplayText();
            tooltip.SetToolTip(panel, timeText);

            panel.MouseEnter += (s, e) => panel.BackColor = Color.FromArgb(100, 220, 120);
            panel.MouseLeave += (s, e) => panel.BackColor = ScheduleActiveColor;
        }

        /// <summary>
        /// 計算設備的每日排程時數及週排程總時數
        /// 使用連續週排程模型 (StartDay+StartTime → EndDay+EndTime)
        /// </summary>
        private void CalculateWeeklyScheduledHours(
            List<ScheduleItem> deviceSchedules,
            out double weeklyTotalHours,
            out Dictionary<DayOfWeek, double> dailyHours)
        {
            dailyHours = new Dictionary<DayOfWeek, double>();
            for (int d = 0; d < 7; d++)
                dailyHours[(DayOfWeek)d] = 0;

            weeklyTotalHours = 0;

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                bool[] minutes = new bool[1440];

                foreach (var schedule in deviceSchedules.Where(s => s.Enabled))
                {
                    double startHour, endHour;
                    if (GetDayActiveRange(schedule, day, out startHour, out endHour))
                    {
                        int startMin = (int)(startHour * 60);
                        int endMin = Math.Min((int)(endHour * 60), 1440);
                        for (int m = startMin; m < endMin; m++)
                            minutes[m] = true;
                    }
                }

                int totalMinutes = 0;
                for (int m = 0; m < 1440; m++)
                {
                    if (minutes[m]) totalMinutes++;
                }

                double hours = totalMinutes / 60.0;
                dailyHours[day] = hours;
                weeklyTotalHours += hours;
            }
        }

        /// <summary>
        /// 建立每日排程時數摘要面板
        /// </summary>
        private Panel CreateDailySummaryPanel(Dictionary<DayOfWeek, double> dailyHours, double weeklyTotalHours)
        {
            int totalWidth = DAY_LABEL_WIDTH + (END_HOUR - START_HOUR) * HOUR_WIDTH + 20;
            Panel panel = new Panel
            {
                Size = new Size(totalWidth, 30),
                BackColor = Color.FromArgb(38, 38, 42)
            };

            string[] dayNames = { "日", "一", "二", "三", "四", "五", "六" };
            int[] dayOrder = { 1, 2, 3, 4, 5, 6, 0 }; // 週一到週日

            int xOffset = 10;

            for (int i = 0; i < 7; i++)
            {
                DayOfWeek day = (DayOfWeek)dayOrder[i];
                double hours = dailyHours[day];
                string text = dayNames[dayOrder[i]] + ":" + hours.ToString("F1") + "h";

                Label lbl = new Label
                {
                    Text = text,
                    Location = new Point(xOffset, 5),
                    AutoSize = true,
                    Font = new Font("微軟正黑體", 9F),
                    ForeColor = hours > 0 ? ScheduleActiveColor : TextSecondaryColor,
                    BackColor = Color.Transparent
                };
                panel.Controls.Add(lbl);
                xOffset += 105;
            }

            // 週總計
            Label totalLabel = new Label
            {
                Text = "合計: " + weeklyTotalHours.ToString("F1") + " 小時",
                Location = new Point(xOffset + 10, 5),
                AutoSize = true,
                Font = new Font("微軟正黑體", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 200, 60),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(totalLabel);

            return panel;
        }
    }
}
