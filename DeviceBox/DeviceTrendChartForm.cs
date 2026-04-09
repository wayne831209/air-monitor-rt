using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MySQL;

namespace DeviceBox
{
    public partial class DeviceTrendChartForm : Form
    {
        private Config _config;
        private MYSQL _mysql;
        private List<string> _allDeviceNames = new List<string>();
        private string _preSelectedFactoryName;

        // 圖表配色
        private static readonly Color[] SeriesColors = new Color[]
        {
            Color.FromArgb(0, 180, 255),
            Color.FromArgb(255, 100, 100),
            Color.FromArgb(0, 200, 100),
            Color.FromArgb(255, 180, 0),
            Color.FromArgb(180, 100, 255),
            Color.FromArgb(255, 130, 60),
            Color.FromArgb(100, 220, 220),
            Color.FromArgb(220, 100, 180),
            Color.FromArgb(140, 200, 60),
            Color.FromArgb(100, 140, 255)
        };

        public DeviceTrendChartForm() : this(null)
        {
        }

        public DeviceTrendChartForm(string preSelectedFactoryName)
        {
            InitializeComponent();
            _preSelectedFactoryName = preSelectedFactoryName;
            try
            {
                LoadConfiguration();
                InitializeDatePickers();
                InitializeDeviceList();
                InitializeChartStyle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化失敗: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 載入設定
        /// </summary>
        private void LoadConfiguration()
        {
            _config = new Config();
            _config.LoadConfig();

            if (!string.IsNullOrEmpty(_config.IP) && !string.IsNullOrEmpty(_config.DB))
            {
                try
                {
                    _mysql = new MYSQL(_config.IP, _config.DB, _config.USER, _config.Password);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("MySQL 連線初始化失敗: " + ex.Message);
                    _mysql = null;
                }
            }
        }

        /// <summary>
        /// 初始化日期選擇器 - 預設為今天
        /// </summary>
        private void InitializeDatePickers()
        {
            dtpStart.Value = DateTime.Today;
            dtpEnd.Value = DateTime.Now;
        }

        /// <summary>
        /// 初始化設備清單 - 從 Config 取得所有設備名稱
        /// </summary>
        private void InitializeDeviceList()
        {
            _allDeviceNames.Clear();
            clbDevices.Items.Clear();

            if (_config == null || _config.Factories == null || _config.Factories.Count == 0)
                return;

            clbDevices.Items.Add("CO-29 - 裝配一廠", false);
            _allDeviceNames.Add("CO-29");
            foreach (var factory in _config.Factories)
            {
                if (factory == null) continue;
                var compressors = factory.GetDevicesByType(DeviceType.Compressor);
                if (compressors == null) continue;
                foreach (var compressor in compressors)
                {
                    if (compressor == null) continue;
                    string displayName = (compressor.Name ?? "") + " - " + (factory.Name ?? "");
                    _allDeviceNames.Add(compressor.Name ?? "");

                    // 若有指定預選廠域，僅勾選該廠域的設備；否則全選
                    bool isChecked = string.IsNullOrEmpty(_preSelectedFactoryName)
                        || factory.Name == _preSelectedFactoryName;
                    clbDevices.Items.Add(displayName, isChecked);
                }
            }
        }

        /// <summary>
        /// 初始化圖表樣式 - 三個圖表區域整合在同一個 Chart
        /// </summary>
        private void InitializeChartStyle()
        {
            var chart = chartCombined;
            chart.BackColor = Color.FromArgb(35, 35, 38);

            // 設定三個 ChartArea 的共用樣式
            SetupChartArea(chart, "ChartAreaPressure", "空壓壓力 (kgf/cm?)");
            SetupChartArea(chart, "ChartAreaTemp", "機房溫度 (°C)");
            SetupChartArea(chart, "ChartAreaDemand", "需量 (kW)");

            // 讓三個 ChartArea 垂直排列，共用 X 軸對齊
            var areaPressure = chart.ChartAreas["ChartAreaPressure"];
            var areaTemp = chart.ChartAreas["ChartAreaTemp"];
            var areaDemand = chart.ChartAreas["ChartAreaDemand"];

            areaPressure.Position = new ElementPosition(0, 3, 100, 30);
            areaTemp.Position = new ElementPosition(0, 33, 100, 30);
            areaDemand.Position = new ElementPosition(0, 63, 100, 37);

            // X 軸對齊
            areaTemp.AlignWithChartArea = "ChartAreaPressure";
            areaTemp.AlignmentOrientation = AreaAlignmentOrientations.Vertical;
            areaTemp.AlignmentStyle = AreaAlignmentStyles.PlotPosition;

            areaDemand.AlignWithChartArea = "ChartAreaPressure";
            areaDemand.AlignmentOrientation = AreaAlignmentOrientations.Vertical;
            areaDemand.AlignmentStyle = AreaAlignmentStyles.PlotPosition;

            // 只在最下方的 ChartArea 顯示 X 軸標籤
            areaPressure.AxisX.LabelStyle.Enabled = false;
            areaTemp.AxisX.LabelStyle.Enabled = false;
            areaDemand.AxisX.LabelStyle.Enabled = true;

            // 圖例 - 嵌入第一個 ChartArea 右上角，不佔用額外空間
            var legend = chart.Legends["LegendMain"];
            legend.BackColor = Color.FromArgb(180, 40, 40, 45);
            legend.ForeColor = Color.White;
            legend.Font = new Font("微軟正黑體", 9F);
            legend.IsDockedInsideChartArea = true;
            legend.DockedToChartArea = "ChartAreaPressure";
            legend.Docking = Docking.Top;
            legend.Alignment = StringAlignment.Far;

            // 標題
            chart.Titles.Clear();
            var chartTitle = new Title("設備壓力 / 溫度 / 需量曲線圖");
            chartTitle.Font = new Font("微軟正黑體", 14F, FontStyle.Bold);
            chartTitle.ForeColor = Color.White;
            chart.Titles.Add(chartTitle);

            // 滑鼠移動時顯示數值 (Tooltip)
            chart.MouseMove += ChartCombined_MouseMove;
        }

        /// <summary>
        /// 設定單一 ChartArea 的深色主題樣式
        /// </summary>
        private void SetupChartArea(Chart chart, string chartAreaName, string yAxisTitle)
        {
            var area = chart.ChartAreas[chartAreaName];
            area.BackColor = Color.FromArgb(35, 35, 38);
            area.AxisX.LabelStyle.ForeColor = Color.White;
            area.AxisX.LabelStyle.Format = "MM/dd HH:00";
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(60, 60, 65);
            area.AxisX.LineColor = Color.FromArgb(100, 100, 105);
            area.AxisX.LabelStyle.Font = new Font("微軟正黑體", 8F);
            area.AxisX.IsMarginVisible = false;
            area.AxisX.IntervalType = DateTimeIntervalType.Hours;
            area.AxisX.Interval = 1;
            area.AxisX.MajorGrid.IntervalType = DateTimeIntervalType.Hours;
            area.AxisX.MajorGrid.Interval = 1;

            area.AxisY.LabelStyle.ForeColor = Color.White;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(60, 60, 65);
            area.AxisY.LineColor = Color.FromArgb(100, 100, 105);
            area.AxisY.LabelStyle.Font = new Font("微軟正黑體", 9F);
            area.AxisY.Title = yAxisTitle;
            area.AxisY.TitleFont = new Font("微軟正黑體", 9F, FontStyle.Bold);

            area.AxisX.TitleForeColor = Color.White;
            area.AxisY.TitleForeColor = Color.White;

            // 游標設定（用於顯示十字線）
            area.CursorX.IsUserEnabled = true;
            area.CursorX.IsUserSelectionEnabled = false;
            area.CursorX.LineColor = Color.FromArgb(150, 255, 255, 255);
            area.CursorX.LineDashStyle = ChartDashStyle.Dash;
        }

        /// <summary>
        /// 滑鼠移動時在 ToolTip 顯示數值
        /// </summary>
        private void ChartCombined_MouseMove(object sender, MouseEventArgs e)
        {
            var chart = chartCombined;
            var hitResult = chart.HitTest(e.X, e.Y);

            if (hitResult.ChartElementType == ChartElementType.DataPoint && hitResult.Series != null)
            {
                var dp = hitResult.Series.Points[hitResult.PointIndex];
                DateTime xVal = DateTime.FromOADate(dp.XValue);
                string tipText = hitResult.Series.Name + "\n時間: " + xVal.ToString("yyyy-MM-dd HH:mm") + "\n數值: " + dp.YValues[0].ToString("F2");
                toolTip1.SetToolTip(chart, tipText);
            }
            else
            {
                toolTip1.SetToolTip(chart, "");
            }
        }

        /// <summary>
        /// 查詢按鈕
        /// </summary>
        private void btnQuery_Click(object sender, EventArgs e)
        {
            if (_mysql == null)
            {
                MessageBox.Show("資料庫未設定，請檢查 config.xml", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 取得選取的設備名稱
            var selectedDevices = GetSelectedDeviceNames();
            if (selectedDevices.Count == 0)
            {
                MessageBox.Show("請至少選擇一個設備", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DateTime startDate = dtpStart.Value;
            DateTime endDate = dtpEnd.Value;

            if (startDate >= endDate)
            {
                MessageBox.Show("開始時間必須早於結束時間", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                QueryAndDisplayData(selectedDevices, startDate, endDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show("查詢失敗: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 取得選取的設備名稱清單
        /// </summary>
        private List<string> GetSelectedDeviceNames()
        {
            var selected = new List<string>();
            for (int i = 0; i < clbDevices.Items.Count; i++)
            {
                if (clbDevices.GetItemChecked(i) && i < _allDeviceNames.Count)
                {
                    selected.Add(_allDeviceNames[i]);
                }
            }
            return selected;
        }

        /// <summary>
        /// 查詢資料庫並顯示圖表
        /// </summary>
        private void QueryAndDisplayData(List<string> deviceNames, DateTime startDate, DateTime endDate)
        {
            string deviceBoxTable = _config.machinery_factory_devicebox_table1;
            string demandTable = _config.machinery_factory_demand_table1;

            if (string.IsNullOrEmpty(deviceBoxTable))
            {
                MessageBox.Show("資料表名稱未設定", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 建立 IN 條件
            string nameCondition = string.Join(",", deviceNames.Select(n => "'" + n.Replace("'", "''") + "'"));

            // 查詢需量資料
            DataTable dtDemand = null;
            if (!string.IsNullOrEmpty(demandTable))
            {
                try
                {
                    string sqlDemand = "SELECT `Meter_Name`, `Time`, `Demand` FROM " + demandTable +
                                 " WHERE `Meter_Name` IN (" + nameCondition + ")" +
                                 " AND `Time` >= '" + startDate.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                                 " AND `Time` <= '" + endDate.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                                 " ORDER BY `Time` ASC";
                    dtDemand = _mysql.GetMyDataTable(sqlDemand);
                }
                catch
                {
                    // 需量表查詢失敗不影響其他資料顯示
                }
            }

            if (nameCondition == "'CO-29'")
            {
                nameCondition = "'CO-28'";
            }
            // 查詢壓力與溫度資料
            string sqlDeviceBox = "SELECT `Name`, `Time`, `CompressedAir`, `AmbientTempPV` FROM " + deviceBoxTable +
                         " WHERE `Name` IN (" + nameCondition + ")" +
                         " AND `Time` >= '" + startDate.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                         " AND `Time` <= '" + endDate.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                         " ORDER BY `Time` ASC";

            DataTable dtDeviceBox = _mysql.GetMyDataTable(sqlDeviceBox);


            bool hasDeviceBoxData = dtDeviceBox != null && dtDeviceBox.Rows.Count > 0;
            bool hasDemandData = dtDemand != null && dtDemand.Rows.Count > 0;

            if (!hasDeviceBoxData && !hasDemandData)
            {
                MessageBox.Show("查無資料", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                chartCombined.Series.Clear();
                return;
            }

            // 依設備名稱分組 - 壓力與溫度
            var groupedDeviceBox = new Dictionary<string, List<DeviceDataPoint>>();
            if (hasDeviceBoxData)
            {
                foreach (DataRow row in dtDeviceBox.Rows)
                {
                    string name = row["Name"].ToString();
                    DateTime time;
                    double pressure, temp;

                    if (!DateTime.TryParse(row["Time"].ToString(), out time))
                        continue;

                    double.TryParse(row["CompressedAir"].ToString(), out pressure);
                    double.TryParse(row["AmbientTempPV"].ToString(), out temp);

                    if (!groupedDeviceBox.ContainsKey(name))
                    {
                        groupedDeviceBox[name] = new List<DeviceDataPoint>();
                    }

                    groupedDeviceBox[name].Add(new DeviceDataPoint { Time = time, Pressure = pressure, Temperature = temp });
                }
            }

            // 依設備名稱分組 - 需量
            var groupedDemand = new Dictionary<string, List<DemandDataPoint>>();
            if (hasDemandData)
            {
                foreach (DataRow row in dtDemand.Rows)
                {
                    string name = row["Meter_Name"].ToString();
                    DateTime time;
                    double demand;

                    if (!DateTime.TryParse(row["Time"].ToString(), out time))
                        continue;

                    double.TryParse(row["Demand"].ToString(), out demand);

                    if (!groupedDemand.ContainsKey(name))
                    {
                        groupedDemand[name] = new List<DemandDataPoint>();
                    }

                    groupedDemand[name].Add(new DemandDataPoint { Time = time, Demand = demand });
                }
            }

            // 繪製圖表
            DrawCharts(groupedDeviceBox, groupedDemand);
        }

        /// <summary>
        /// 繪製壓力、溫度、需量圖表（整合在同一個 Chart 的三個 ChartArea）
        /// </summary>
        private void DrawCharts(Dictionary<string, List<DeviceDataPoint>> groupedDeviceBox, Dictionary<string, List<DemandDataPoint>> groupedDemand)
        {
            chartCombined.Series.Clear();

            // 清除舊的限制線
            foreach (var area in chartCombined.ChartAreas)
            {
                area.AxisY.StripLines.Clear();
            }

            // 建立設備名稱與顏色的對應表，確保同一設備在所有圖表中顏色一致
            var deviceColorMap = new Dictionary<string, Color>();
            int colorIndex = 0;

            // 先從壓力/溫度資料收集設備名稱
            foreach (var deviceName in groupedDeviceBox.Keys)
            {
                if (!deviceColorMap.ContainsKey(deviceName))
                {
                    deviceColorMap[deviceName] = SeriesColors[colorIndex % SeriesColors.Length];
                    colorIndex++;
                }
            }

            // 再從需量資料收集設備名稱（若有新設備才分配新顏色）
            foreach (var deviceName in groupedDemand.Keys)
            {
                if (!deviceColorMap.ContainsKey(deviceName))
                {
                    deviceColorMap[deviceName] = SeriesColors[colorIndex % SeriesColors.Length];
                    colorIndex++;
                }
            }

            // 圖例只顯示設備名稱（每個設備一筆），用隱藏的 Series 代表
            foreach (var kvp in deviceColorMap)
            {
                var legendSeries = new Series(kvp.Key);
                legendSeries.ChartType = SeriesChartType.Line;
                legendSeries.Color = kvp.Value;
                legendSeries.BorderWidth = 2;
                legendSeries.ChartArea = "ChartAreaPressure";
                legendSeries.Legend = "LegendMain";
                legendSeries.IsVisibleInLegend = true;
                chartCombined.Series.Add(legendSeries);
            }

            // 繪製壓力與溫度曲線（實線，不顯示在圖例中）
            foreach (var kvp in groupedDeviceBox)
            {
                string deviceName = kvp.Key;
                var dataPoints = kvp.Value;
                Color color = deviceColorMap[deviceName];

                // 壓力曲線
                var pressureSeries = new Series(deviceName + " 壓力");
                pressureSeries.ChartType = SeriesChartType.Line;
                pressureSeries.Color = color;
                pressureSeries.BorderWidth = 2;
                pressureSeries.ChartArea = "ChartAreaPressure";
                pressureSeries.Legend = "LegendMain";
                pressureSeries.IsVisibleInLegend = false;

                foreach (var dp in dataPoints)
                {
                    pressureSeries.Points.AddXY(dp.Time, dp.Pressure);
                }

                chartCombined.Series.Add(pressureSeries);

                // 溫度曲線（實線）
                var tempSeries = new Series(deviceName + " 溫度");
                tempSeries.ChartType = SeriesChartType.Line;
                tempSeries.Color = color;
                tempSeries.BorderWidth = 2;
                tempSeries.ChartArea = "ChartAreaTemp";
                tempSeries.Legend = "LegendMain";
                tempSeries.IsVisibleInLegend = false;

                foreach (var dp in dataPoints)
                {
                    tempSeries.Points.AddXY(dp.Time, dp.Temperature);
                }

                chartCombined.Series.Add(tempSeries);
            }

            // 繪製需量曲線（實線，使用相同設備對應的顏色，不顯示在圖例中）
            foreach (var kvp in groupedDemand)
            {
                string deviceName = kvp.Key;
                var dataPoints = kvp.Value;
                Color color = deviceColorMap[deviceName];

                var demandSeries = new Series(deviceName + " 需量");
                demandSeries.ChartType = SeriesChartType.Line;
                demandSeries.Color = color;
                demandSeries.BorderWidth = 2;
                demandSeries.ChartArea = "ChartAreaDemand";
                demandSeries.Legend = "LegendMain";
                demandSeries.IsVisibleInLegend = false;

                foreach (var dp in dataPoints)
                {
                    demandSeries.Points.AddXY(dp.Time, dp.Demand);
                }

                chartCombined.Series.Add(demandSeries);
            }

            // 設定 X 軸格式 - 固定以小時為單位
            foreach (var area in chartCombined.ChartAreas)
            {
                area.AxisX.LabelStyle.Format = "MM/dd HH:00";
                area.AxisX.IntervalType = DateTimeIntervalType.Hours;
                area.AxisX.Interval = 1;
            }

            // 根據實際資料重新計算每個 ChartArea 的 Y 軸範圍
            foreach (var area in chartCombined.ChartAreas)
            {
                var seriesInArea = chartCombined.Series.Cast<Series>()
                    .Where(s => s.ChartArea == area.Name && s.Points.Count > 0)
                    .ToList();

                if (seriesInArea.Count == 0)
                {
                    area.AxisY.Minimum = double.NaN;
                    area.AxisY.Maximum = double.NaN;
                    area.AxisY.Interval = 0;
                    continue;
                }

                double dataMin = seriesInArea.Min(s => s.Points.Min(p => p.YValues[0]));
                double dataMax = seriesInArea.Max(s => s.Points.Max(p => p.YValues[0]));

                // 空壓壓力：Y軸固定顯示 0~10，間隔為 1
                if (area.Name == "ChartAreaPressure")
                {
                    area.AxisY.Minimum = 0;
                    area.AxisY.Maximum = 10;
                    area.AxisY.Interval = 1;
                    continue;
                }

                double range = dataMax - dataMin;
                if (range < 0.01)
                {
                    // 資料幾乎無變化時，以中心值上下擴展
                    double center = (dataMax + dataMin) / 2.0;
                    dataMin = center - 1;
                    dataMax = center + 1;
                    range = 2;
                }

                // 上下各留 10% 的邊距
                double margin = range * 0.1;
                double yMinCalc = dataMin - margin;
                double yMaxCalc = dataMax + margin;

                // 計算合適的刻度間距（目標約 4~6 格）
                double rawInterval = (yMaxCalc - yMinCalc) / 5.0;
                double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawInterval)));
                double normalized = rawInterval / magnitude;
                double niceInterval;
                if (normalized <= 1.0)
                    niceInterval = 1.0 * magnitude;
                else if (normalized <= 2.0)
                    niceInterval = 2.0 * magnitude;
                else if (normalized <= 5.0)
                    niceInterval = 5.0 * magnitude;
                else
                    niceInterval = 10.0 * magnitude;

                // 將最小/最大值對齊到刻度
                yMinCalc = Math.Floor(yMinCalc / niceInterval) * niceInterval;
                yMaxCalc = Math.Ceiling(yMaxCalc / niceInterval) * niceInterval;

                area.AxisY.Minimum = yMinCalc;
                area.AxisY.Maximum = yMaxCalc;
                area.AxisY.Interval = niceInterval;
            }

            // 繪製警報上下限線
            var selectedDevices = GetSelectedDeviceNames();
            DrawAlarmLimitLines(selectedDevices);
        }

        /// <summary>
        /// 繪製警報上下限線到圖表上
        /// 從選取設備對應的工廠取得 AlarmLimits，在壓力與溫度 ChartArea 上繪製水平參考線
        /// </summary>
        private void DrawAlarmLimitLines(List<string> selectedDeviceNames)
        {
            var chart = chartCombined;
            var areaPressure = chart.ChartAreas["ChartAreaPressure"];
            var areaTemp = chart.ChartAreas["ChartAreaTemp"];

            // 收集所有選取設備對應工廠的 AlarmLimits（去重複）
            var processedFactoryIds = new HashSet<int>();
            var allPressureLimits = new List<KeyValuePair<string, AlarmLimitsConfig>>();

            foreach (var factory in _config.Factories)
            {
                if (factory == null) continue;
                var compressors = factory.GetDevicesByType(DeviceType.Compressor);
                bool hasSelected = compressors.Any(c => selectedDeviceNames.Contains(c.Name));
                if (!hasSelected) continue;
                if (processedFactoryIds.Contains(factory.Id)) continue;
                processedFactoryIds.Add(factory.Id);

                allPressureLimits.Add(new KeyValuePair<string, AlarmLimitsConfig>(factory.Name, factory.AlarmLimits));
            }

            // 顏色設定 - 上下限一律使用紅色虛線
            Color limitColor = Color.Red;

            // 追蹤所有限制線的值，用於調整 Y 軸範圍
            var pressureLimitValues = new List<double>();
            var tempLimitValues = new List<double>();

            foreach (var kvp in allPressureLimits)
            {
                string factoryName = kvp.Key;
                var limits = kvp.Value;
                if (limits == null) continue;

                // 壓力上限線（紅色虛線）
                if (limits.PressureUpperLimit != double.MaxValue)
                {
                    var strip = new StripLine();
                    strip.IntervalOffset = limits.PressureUpperLimit;
                    strip.StripWidth = 0;
                    strip.BorderColor = limitColor;
                    strip.BorderWidth = 2;
                    strip.BorderDashStyle = ChartDashStyle.Dash;
                    strip.Text = factoryName + " 上限: " + limits.PressureUpperLimit.ToString("F2");
                    strip.ForeColor = limitColor;
                    strip.Font = new Font("微軟正黑體", 8F);
                    strip.TextAlignment = StringAlignment.Near;
                    areaPressure.AxisY.StripLines.Add(strip);
                    pressureLimitValues.Add(limits.PressureUpperLimit);
                }

                // 壓力下限線（紅色虛線）
                if (limits.PressureLowerLimit != double.MinValue)
                {
                    var strip = new StripLine();
                    strip.IntervalOffset = limits.PressureLowerLimit;
                    strip.StripWidth = 0;
                    strip.BorderColor = limitColor;
                    strip.BorderWidth = 2;
                    strip.BorderDashStyle = ChartDashStyle.Dash;
                    strip.Text = factoryName + " 下限: " + limits.PressureLowerLimit.ToString("F2");
                    strip.ForeColor = limitColor;
                    strip.Font = new Font("微軟正黑體", 8F);
                    strip.TextAlignment = StringAlignment.Near;
                    areaPressure.AxisY.StripLines.Add(strip);
                    pressureLimitValues.Add(limits.PressureLowerLimit);
                }

                // 溫度上限線（紅色虛線）
                if (limits.TempUpperLimit != double.MaxValue)
                {
                    var strip = new StripLine();
                    strip.IntervalOffset = limits.TempUpperLimit;
                    strip.StripWidth = 0;
                    strip.BorderColor = limitColor;
                    strip.BorderWidth = 2;
                    strip.BorderDashStyle = ChartDashStyle.Dash;
                    strip.Text = factoryName + " 上限: " + limits.TempUpperLimit.ToString("F1");
                    strip.ForeColor = limitColor;
                    strip.Font = new Font("微軟正黑體", 8F);
                    strip.TextAlignment = StringAlignment.Near;
                    areaTemp.AxisY.StripLines.Add(strip);
                    tempLimitValues.Add(limits.TempUpperLimit);
                }

                // 溫度下限線（紅色虛線）
                if (limits.TempLowerLimit != double.MinValue)
                {
                    var strip = new StripLine();
                    strip.IntervalOffset = limits.TempLowerLimit;
                    strip.StripWidth = 0;
                    strip.BorderColor = limitColor;
                    strip.BorderWidth = 2;
                    strip.BorderDashStyle = ChartDashStyle.Dash;
                    strip.Text = factoryName + " 下限: " + limits.TempLowerLimit.ToString("F1");
                    strip.ForeColor = limitColor;
                    strip.Font = new Font("微軟正黑體", 8F);
                    strip.TextAlignment = StringAlignment.Near;
                    areaTemp.AxisY.StripLines.Add(strip);
                    tempLimitValues.Add(limits.TempLowerLimit);
                }
            }

            // 調整 Y 軸範圍以確保限制線可見
            AdjustAxisRangeForLimits(areaPressure, pressureLimitValues);
            AdjustAxisRangeForLimits(areaTemp, tempLimitValues);
        }

        /// <summary>
        /// 調整 ChartArea 的 Y 軸範圍以確保限制線可見
        /// </summary>
        private void AdjustAxisRangeForLimits(ChartArea area, List<double> limitValues)
        {
            if (limitValues.Count == 0) return;
            if (double.IsNaN(area.AxisY.Minimum) || double.IsNaN(area.AxisY.Maximum)) return;

            double currentMin = area.AxisY.Minimum;
            double currentMax = area.AxisY.Maximum;
            double interval = area.AxisY.Interval;
            if (interval <= 0) return;

            double newMin = currentMin;
            double newMax = currentMax;

            foreach (var val in limitValues)
            {
                if (val < newMin) newMin = val - interval;
                if (val > newMax) newMax = val + interval;
            }

            // 對齊到刻度
            newMin = Math.Floor(newMin / interval) * interval;
            newMax = Math.Ceiling(newMax / interval) * interval;

            area.AxisY.Minimum = newMin;
            area.AxisY.Maximum = newMax;
        }

        /// <summary>
        /// 全選按鈕
        /// </summary>
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbDevices.Items.Count; i++)
            {
                clbDevices.SetItemChecked(i, true);
            }
        }

        /// <summary>
        /// 取消全選按鈕
        /// </summary>
        private void btnDeselectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbDevices.Items.Count; i++)
            {
                clbDevices.SetItemChecked(i, false);
            }
        }

        /// <summary>
        /// 設備資料點結構（壓力與溫度）
        /// </summary>
        private class DeviceDataPoint
        {
            public DateTime Time { get; set; }
            public double Pressure { get; set; }
            public double Temperature { get; set; }
        }

        /// <summary>
        /// 需量資料點結構
        /// </summary>
        private class DemandDataPoint
        {
            public DateTime Time { get; set; }
            public double Demand { get; set; }
        }
    }
}
