using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceBox
{
    public partial class TrendChart : Form
    {
        private Config _config;
        private Timer _currentTimeTimer;
        private List<GanttRowInfo> _ganttRows = new List<GanttRowInfo>();
        private HashSet<int> _factoryIdFilter = null;  // null = show all

        // 設計常數
        private const int ROW_HEIGHT = 45;

        // 顏色定義
        private readonly Color ChartBgColor = Color.FromArgb(35, 35, 38);
        private readonly Color GridLineColor = Color.FromArgb(60, 60, 65);
        private readonly Color RunningColor = Color.FromArgb(0, 180, 80);
        private readonly Color StoppedColor = Color.FromArgb(80, 80, 85);
        private readonly Color CurrentTimeLineColor = Color.FromArgb(255, 80, 80);
        private readonly Color TextColor = Color.FromArgb(240, 240, 240);
        private readonly Color FactoryHeaderColor = Color.FromArgb(0, 100, 180);
        private readonly Color DeviceLabelBgColor = Color.FromArgb(50, 50, 55);

        public TrendChart()
        {
            InitializeComponent();
            LoadConfiguration();
            InitializeGanttChart();
            StartCurrentTimeTimer();
        }

        /// <summary>
        /// 建構子：可傳入要顯示的工廠 ID 集合，null 表示顯示全部
        /// </summary>
        public TrendChart(HashSet<int> factoryIdFilter) : this()
        {
            _factoryIdFilter = factoryIdFilter;
            BuildGanttData();
            PopulateDeviceLabels();
            panelGanttChart?.Invalidate();
        }

        private void LoadConfiguration()
        {
            _config = new Config();
            _config.LoadConfig();
        }

        private void InitializeGanttChart()
        {
            // 設定繪圖事件
            this.panelTimeAxis.Paint += PanelTimeAxis_Paint;
            this.panelGanttChart.Paint += PanelGanttChart_Paint;

            // 建立資料
            BuildGanttData();

            // 填充設備標籤
            PopulateDeviceLabels();
        }

        private void BuildGanttData()
        {
            _ganttRows.Clear();

            foreach (var factory in _config.Factories)
            {
                // 如果有設定篩選條件，只顯示符合的工廠
                if (_factoryIdFilter != null && !_factoryIdFilter.Contains(factory.Id))
                    continue;

                // 取得該工廠所有有排程的壓縮機
                var compressorsWithSchedule = factory.Devices
                    .Where(d => d.Type == DeviceType.Compressor && d.Enabled && d.Schedule != null && d.Schedule.Enabled)
                    .ToList();

                if (compressorsWithSchedule.Count == 0)
                    continue;

                // 工廠標題行
                _ganttRows.Add(new GanttRowInfo
                {
                    IsFactoryHeader = true,
                    FactoryName = factory.Name,
                    Device = null
                });

                // 設備行
                foreach (var device in compressorsWithSchedule)
                {
                    _ganttRows.Add(new GanttRowInfo
                    {
                        IsFactoryHeader = false,
                        FactoryName = factory.Name,
                        Device = device
                    });
                }
            }
        }

        private void PopulateDeviceLabels()
        {
            panelDeviceLabels.Controls.Clear();
            int yOffset = 0;

            foreach (var row in _ganttRows)
            {
                Label label;
                if (row.IsFactoryHeader)
                {
                    label = new Label
                    {
                        Text = row.FactoryName,
                        Location = new Point(0, yOffset),
                        Size = new Size(panelDeviceLabels.Width, ROW_HEIGHT),
                        Font = new Font("微軟正黑體", 11F, FontStyle.Bold),
                        ForeColor = Color.White,
                        BackColor = FactoryHeaderColor,
                        TextAlign = ContentAlignment.MiddleCenter,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                }
                else
                {
                    label = new Label
                    {
                        Text = $"  {row.Device.Name}",
                        Location = new Point(0, yOffset),
                        Size = new Size(panelDeviceLabels.Width, ROW_HEIGHT),
                        Font = new Font("微軟正黑體", 10F, FontStyle.Regular),
                        ForeColor = TextColor,
                        BackColor = DeviceLabelBgColor,
                        TextAlign = ContentAlignment.MiddleLeft,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                }

                panelDeviceLabels.Controls.Add(label);
                yOffset += ROW_HEIGHT;
            }
        }

        private void PanelTimeAxis_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int width = panelTimeAxis.Width;
            int height = panelTimeAxis.Height;
            float hourWidth = width / 24f;

            using (Font font = new Font("微軟正黑體", 8F, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.White))
            using (Pen linePen = new Pen(Color.FromArgb(100, 255, 255, 255), 1))
            {
                for (int hour = 0; hour <= 24; hour++)
                {
                    float x = hour * hourWidth;

                    // 時間標籤（每2小時顯示）
                    if (hour % 2 == 0 && hour < 24)
                    {
                        string timeText = $"{hour:D2}:00";
                        SizeF textSize = g.MeasureString(timeText, font);
                        g.DrawString(timeText, font, textBrush, x + 2, (height - textSize.Height) / 2);
                    }

                    // 分隔線
                    if (hour > 0 && hour < 24)
                    {
                        g.DrawLine(linePen, x, 0, x, height);
                    }
                }
            }
        }

        private void PanelGanttChart_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int chartWidth = panelGanttChart.Width;
            int chartHeight = panelGanttChart.Height;
            float hourWidth = chartWidth / 24f;

            // 繪製網格線
            using (Pen gridPen = new Pen(GridLineColor, 1))
            {
                for (int hour = 0; hour <= 24; hour++)
                {
                    float x = hour * hourWidth;
                    g.DrawLine(gridPen, x, 0, x, chartHeight);
                }
            }

            // 繪製每一行
            int yOffset = 0;
            foreach (var row in _ganttRows)
            {
                if (row.IsFactoryHeader)
                {
                    // 工廠標題行背景
                    using (Brush bgBrush = new SolidBrush(Color.FromArgb(40, 40, 45)))
                    {
                        g.FillRectangle(bgBrush, 0, yOffset, chartWidth, ROW_HEIGHT);
                    }
                }
                else
                {
                    // 設備排程條
                    DrawScheduleBar(g, row.Device, yOffset, chartWidth, hourWidth);
                }

                // 行分隔線
                using (Pen gridPen = new Pen(GridLineColor, 1))
                {
                    g.DrawLine(gridPen, 0, yOffset + ROW_HEIGHT, chartWidth, yOffset + ROW_HEIGHT);
                }

                yOffset += ROW_HEIGHT;
            }

            // 繪製當前時間線
            DrawCurrentTimeLine(g, chartHeight, hourWidth);
        }

        private void DrawScheduleBar(Graphics g, DeviceConfig device, int y, int chartWidth, float hourWidth)
        {
            // 背景（停止狀態）
            using (Brush stoppedBrush = new SolidBrush(StoppedColor))
            {
                g.FillRectangle(stoppedBrush, 2, y + 8, chartWidth - 4, ROW_HEIGHT - 16);
            }

            if (device.Schedule != null && device.Schedule.Enabled && device.Schedule.TimeRanges != null)
            {
                DayOfWeek today = DateTime.Now.DayOfWeek;

                using (Brush runBrush = new SolidBrush(RunningColor))
                {
                    foreach (var range in device.Schedule.TimeRanges)
                    {
                        double dayStartHour, dayEndHour;
                        if (GetTodayActiveRange(range, today, out dayStartHour, out dayEndHour))
                        {
                            float startX = (float)(dayStartHour * hourWidth);
                            float endX = (float)(dayEndHour * hourWidth);
                            float barWidth = endX - startX;

                            if (barWidth > 0)
                            {
                                g.FillRectangle(runBrush, startX + 2, y + 8, barWidth - 2, ROW_HEIGHT - 16);

                                // 時間標籤
                                TimeSpan displayStart = TimeSpan.FromHours(dayStartHour);
                                TimeSpan displayEnd = TimeSpan.FromHours(Math.Min(dayEndHour, 23.983)); // 23:59
                                DrawTimeLabel(g, displayStart, displayEnd, startX, y, barWidth);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判斷今天在某個 ScheduleTimeRange 中的實際運轉時段
        /// 支援跨日模式（StartDay/EndDay）和重複模式（Days 清單）
        /// 
        /// 跨日模式範例：StartDay=Monday 08:00, EndDay=Wednesday 17:00
        ///   Monday    → 08:00~24:00
        ///   Tuesday   → 00:00~24:00（全天）
        ///   Wednesday → 00:00~17:00
        ///   其他天    → 不運轉
        /// 
        /// 重複模式範例：Days=[Monday,Tuesday], StartTime=08:00, EndTime=17:00
        ///   Monday    → 08:00~17:00
        ///   Tuesday   → 08:00~17:00
        ///   其他天    → 不運轉
        /// </summary>
        private bool GetTodayActiveRange(ScheduleTimeRange range, DayOfWeek today, out double startHour, out double endHour)
        {
            startHour = 0;
            endHour = 0;

            // 重複模式：用 IsSpanMode 判斷，而非 Days.Count
            if (!range.IsSpanMode)
            {
                if (range.Days != null && range.Days.Count > 0 && !range.Days.Contains(today))
                    return false;

                startHour = range.StartTime.TotalHours;
                endHour = range.EndTime.TotalHours;

                // 處理跨午夜（例如 22:00~06:00）：當天只顯示到24點
                if (endHour <= startHour)
                    endHour = 24;

                return endHour > startHour;
            }

            // 跨日模式：根據 StartDay/EndDay 判斷今天的運轉區間
            int sd = (int)range.StartDay;
            int ed = (int)range.EndDay;
            int d = (int)today;

            // 判斷今天是否在跨日範圍內
            bool isDayInSpan;
            if (sd <= ed)
                isDayInSpan = d >= sd && d <= ed;
            else
                isDayInSpan = d >= sd || d <= ed;

            if (!isDayInSpan)
                return false;

            bool isStartDay = d == sd;
            bool isEndDay = d == ed;

            if (isStartDay && isEndDay)
            {
                // 同一天開始和結束
                if (sd <= ed)
                {
                    startHour = range.StartTime.TotalHours;
                    endHour = range.EndTime.TotalHours;
                }
                else
                {
                    // 跨週且同一天（例如 Wed 20:00 ~ Wed 08:00，運轉6天24小時+這天的部分）
                    startHour = 0;
                    endHour = 24;
                }
            }
            else if (isStartDay)
            {
                // 開始日：從 StartTime 到 24:00
                startHour = range.StartTime.TotalHours;
                endHour = 24;
            }
            else if (isEndDay)
            {
                // 結束日：從 00:00 到 EndTime
                startHour = 0;
                endHour = range.EndTime.TotalHours;
            }
            else
            {
                // 中間日：全天 00:00~24:00
                startHour = 0;
                endHour = 24;
            }

            return endHour > startHour;
        }

        private void DrawTimeLabel(Graphics g, TimeSpan startTime, TimeSpan endTime, float x, int y, float width)
        {
            string timeText = $"{startTime:hh\\:mm} - {endTime:hh\\:mm}";

            using (Font font = new Font("微軟正黑體", 8F, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.White))
            {
                SizeF textSize = g.MeasureString(timeText, font);

                if (width > textSize.Width + 10)
                {
                    float textX = x + (width - textSize.Width) / 2;
                    float textY = y + (ROW_HEIGHT - textSize.Height) / 2;
                    g.DrawString(timeText, font, textBrush, textX, textY);
                }
            }
        }

        private void DrawCurrentTimeLine(Graphics g, int chartHeight, float hourWidth)
        {
            DateTime now = DateTime.Now;
            float currentX = (float)(now.TimeOfDay.TotalHours * hourWidth);

            using (Pen timePen = new Pen(CurrentTimeLineColor, 2))
            {
                g.DrawLine(timePen, currentX, 0, currentX, chartHeight);
            }

            // 當前時間標籤
            string currentTimeText = now.ToString("HH:mm");
            using (Font font = new Font("微軟正黑體", 8F, FontStyle.Bold))
            using (Brush bgBrush = new SolidBrush(CurrentTimeLineColor))
            using (Brush textBrush = new SolidBrush(Color.White))
            {
                SizeF textSize = g.MeasureString(currentTimeText, font);
                float labelX = currentX - textSize.Width / 2;
                float labelY = 2;

                g.FillRectangle(bgBrush, labelX - 2, labelY, textSize.Width + 4, textSize.Height);
                g.DrawString(currentTimeText, font, textBrush, labelX, labelY);
            }
        }

        private void StartCurrentTimeTimer()
        {
            _currentTimeTimer = new Timer
            {
                Interval = 60000 // 每分鐘更新
            };
            _currentTimeTimer.Tick += (s, e) => panelGanttChart?.Invalidate();
            _currentTimeTimer.Start();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            panelGanttChart?.Invalidate();
            panelTimeAxis?.Invalidate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _currentTimeTimer?.Stop();
            _currentTimeTimer?.Dispose();
        }

        /// <summary>
        /// 甘特圖行資訊
        /// </summary>
        private class GanttRowInfo
        {
            public bool IsFactoryHeader { get; set; }
            public string FactoryName { get; set; }
            public DeviceConfig Device { get; set; }
        }
    }
}
