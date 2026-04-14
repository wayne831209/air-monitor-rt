using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySQL;

namespace DeviceBox
{
    public partial class MainForm : Form
    {
        // Color Constants
        private static readonly Color StatusRunning = Color.FromArgb(0, 200, 0);      // Green - Running
        private static readonly Color StatusStopped = Color.FromArgb(128, 128, 128);  // Gray - Stopped
        private static readonly Color StatusAlarm = Color.FromArgb(255, 140, 0);      // Orange - Alarm
        private static readonly Color StatusFault = Color.FromArgb(220, 50, 50);      // Red - Fault
        private static readonly Color StatusReady = Color.FromArgb(0, 180, 255);      // Blue - Ready
        private static readonly Color StatusNotReady = Color.FromArgb(100, 100, 100); // Dark Gray - Not Ready
        private static readonly Color StatusDisabled = Color.FromArgb(60, 60, 60);    // Dark Gray - Disabled
        private static readonly Color TextNormal = Color.White;                        // Normal Text
        private static readonly Color ScheduleActive = Color.FromArgb(0, 150, 0);     // Green - Schedule Active
        private static readonly Color ScheduleInactive = Color.FromArgb(150, 0, 0);   // Red - No Schedule
        private static readonly Color StatusOverLimit = Color.FromArgb(255, 50, 50);   // Red - Over Limit

        // View Mode
        private enum ViewMode { OtherFactories, CastingFactory }
        private ViewMode currentViewMode = ViewMode.OtherFactories;
        private int[] currentDisplayIndices = { 0, 1, 2, 3, 4 };  // Factory indices to display
        private const int CASTING_FACTORY_ID = 6;  // Casting Factory ID in config

        private Timer updateTimer;
        private List<ModBus_List> modbusList;
        private Config config;
        private ScheduleMode currentMode;  // 當前選擇的模式
        private bool isManualMode = false;    // 是否為手動模式

        // 記錄每個設備上次寫入的 DO 狀態，避免重複寫入
        // Key: "FactoryId_MachineNo", Value: last written DO value (1=on, 0=off)
        private Dictionary<string, ushort> lastDOStates = new Dictionary<string, ushort>();

        // 手動模式下，記錄每個設備的手動 DO 狀態
        // Key: "FactoryId_MachineNo", Value: manual DO value (1=on, 0=off)
        private Dictionary<string, ushort> manualDOStates = new Dictionary<string, ushort>();

        public MainForm()
        {
            InitializeComponent();
            InitializeConfig();
            InitializeModbus();
            InitializeTimer();
            InitializeFactoryHeaders();
            InitializeCompressorNames();
            InitializeDefaultMode();
        }

        /// <summary>
        /// Initialize Config
        /// </summary>
        private void InitializeConfig()
        {
            config = new Config();
            if (!config.LoadConfig())
            {
                MessageBox.Show("Failed to load config.xml", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Initialize Modbus Connections
        /// </summary>
        private void InitializeModbus()
        {
            modbusList = new List<ModBus_List>();

            try
            {
                foreach (var factory in config.Factories)
                {
                    modbusList.Add(new ModBus_List(factory.ModbusIp, factory.ModbusPort, factory.Name));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Modbus init failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Initialize Factory Headers
        /// </summary>
        private void InitializeFactoryHeaders()
        {
            // Set initial display indices (first 5 factories excluding casting factory)
            var otherFactories = config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).Take(5).ToList();
            for (int i = 0; i < 5; i++)
            {
                if (i < otherFactories.Count)
                    currentDisplayIndices[i] = config.Factories.IndexOf(otherFactories[i]);
                else
                    currentDisplayIndices[i] = -1;
            }
            
            RefreshFactoryDisplay();
        }

        /// <summary>
        /// Initialize Compressor Names (Row1)
        /// </summary>
        private void InitializeCompressorNames()
        {
            // Row1: Device Names
            Label[] deviceNameLabels = { device_col1, device_col2, device_col3, device_col4, device_col5 };
            // Row2: Schedule
            Label[] scheduleLabels = { schedule_col1, schedule_col2, schedule_col3, schedule_col4, schedule_col5 };

            for (int i = 0; i < Math.Min(config.Factories.Count, 5); i++)
            {
                var factory = config.Factories[i];
                var compressors = factory.GetDevicesByType(DeviceType.Compressor);

                if (compressors.Count > 0)
                {
                    // Build device name string
                    string deviceNames = BuildCompressorNameString(factory, compressors);
                    UpdateLabel(deviceNameLabels[i], deviceNames, TextNormal);

                    // Update Schedule (Row2)
                    UpdateScheduleLabel(scheduleLabels[i], compressors);
                }
                else
                {
                    UpdateLabel(deviceNameLabels[i], "--", StatusDisabled);
                    UpdateLabelWithBackground(scheduleLabels[i], "No Schedule", TextNormal, ScheduleInactive);
                }
            }
        }

        /// <summary>
        /// Build Compressor Name String
        /// Format: "FactoryName:CO-38" or "CO-38\nCO-37" for multiple
        /// </summary>
        private string BuildCompressorNameString(FactoryConfig factory, List<DeviceConfig> compressors)
        {
            if (compressors.Count == 1)
            {
                // Single compressor: just show name
                return compressors[0].Name;
            }
            else
            {
                // Multiple compressors: show factory:name format
                var names = compressors
                    .OrderBy(c => c.MachineNo)
                    .Select(c => factory.Name + ":" + c.Name);
                return string.Join("\n", names);
            }
        }

        private void InitializeTimer()
        {
            updateTimer = new Timer();
            updateTimer.Interval = 1000;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        /// <summary>
        /// Initialize Default Mode - 啟動時載入預設模式並自動套用排程
        /// </summary>
        private void InitializeDefaultMode()
        {
            try
            {
                // 從 config 載入預設模式（IsDefault=true）
                var defaultMode = ModeSelectForm.GetDefaultMode();
                if (defaultMode != null)
                {
                    currentMode = defaultMode;
                    label3.Text = defaultMode.Name;
                    if (!string.IsNullOrEmpty(defaultMode.Description))
                    {
                        label4.Text = defaultMode.Description;
                    }

                    // 自動套用預設模式的排程到設備
                    ModeSelectForm.ApplyModeSchedulesToConfig(defaultMode);

                    // 重新載入設定以反映套用的排程
                    config.LoadConfig();
                    RefreshFactoryDisplay();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Load default mode failed: " + ex.Message);
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (modbusList == null || modbusList.Count == 0)
                return;

            try
            {
                UpdateAllFactories();
                ExecuteScheduleControl();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Update failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Update all factories based on current view mode
        /// </summary>
        private void UpdateAllFactories()
        {
            Label[] deviceNameLabels = { device_col1, device_col2, device_col3, device_col4, device_col5 };
            Label[] scheduleLabels = { schedule_col1, schedule_col2, schedule_col3, schedule_col4, schedule_col5 };
            Label[] statusLabels = { status_col1, status_col2, status_col3, status_col4, status_col5 };
            Label[] precoolerLabels = { precooler_col1, precooler_col2, precooler_col3, precooler_col4, precooler_col5 };
            Label[] dryerLabels = { dryer_col1, dryer_col2, dryer_col3, dryer_col4, dryer_col5 };
            Label[] fanLabels = { fan_col1, fan_col2, fan_col3, fan_col4, fan_col5 };
            Label[] pressureLabels = { pressure_col1, pressure_col2, pressure_col3, pressure_col4, pressure_col5 };
        Label[] tempLabels = { temp_col1, temp_col2, temp_col3, temp_col4, temp_col5 };
            Label[] powerLabels = { power_col1, power_col2, power_col3, power_col4, power_col5 };

            if (currentViewMode == ViewMode.CastingFactory)
            {
                // Casting Factory Mode - Each compressor in separate column
                var castingFactory = config.Factories.FirstOrDefault(f => f.Id == CASTING_FACTORY_ID);
                if (castingFactory == null) return;

                int modbusIndex = config.Factories.IndexOf(castingFactory);
                if (modbusIndex >= modbusList.Count) return;

                var modbus = modbusList[modbusIndex];
                if (modbus.address_val == null) return;

                var compressors = castingFactory.GetDevicesByType(DeviceType.Compressor).OrderBy(c => c.MachineNo).ToList();

                // Update device name label color based on ConnectState
                for (int colIndex = 0; colIndex < Math.Min(compressors.Count, 5); colIndex++)
                {
                    UpdateLabel(deviceNameLabels[colIndex], deviceNameLabels[colIndex].Text, modbus.ConnectState ? Color.Green : Color.Red);
                }

                // Common devices (shared across all columns)
                var precoolerStatus = GetDeviceStatusByConfig(modbus, castingFactory, DeviceType.Precooler);
                var dryerStatus = GetDeviceStatusByConfig(modbus, castingFactory, DeviceType.Dryer);
                var fanStatus = GetDeviceStatusByConfig(modbus, castingFactory, DeviceType.Fan);
                var pressure = GetPressureValue(modbus);
                var temp = GetTempValue(modbus);

                // Update each compressor in separate column
                for (int colIndex = 0; colIndex < Math.Min(compressors.Count, 5); colIndex++)
                {
                    var compressor = compressors[colIndex];
                    
                    // Get individual compressor status
                    bool isRunning = GetDIValue(modbus, compressor.IO.RunDI);
                    bool isAlarm = GetDIValue(modbus, compressor.IO.AlarmDI);
                    bool isFault = GetDIValue(modbus, compressor.IO.FaultDI);

                    DeviceStatus status;
                    if (isFault)
                        status = new DeviceStatus("故障", StatusFault);
                    else if (isAlarm)
                        status = new DeviceStatus("警報", StatusAlarm);
                    else if (isRunning)
                        status = new DeviceStatus("運轉", StatusRunning);
                    else
                        status = new DeviceStatus("停止", StatusStopped);

                    UpdateLabel(statusLabels[colIndex], status.Text, status.Color);
                    UpdateScheduleLabel(scheduleLabels[colIndex], new List<DeviceConfig> { compressor });

                    // Common devices - show same values in all compressor columns
                    UpdateLabel(precoolerLabels[colIndex], precoolerStatus.Text, precoolerStatus.Color);
                    UpdateLabel(dryerLabels[colIndex], dryerStatus.Text, dryerStatus.Color);
                    UpdateLabel(fanLabels[colIndex], fanStatus.Text, fanStatus.Color);
                    UpdatePressureLabelWithLimitCheck(pressureLabels[colIndex], pressure, castingFactory.AlarmLimits);
                    UpdateTempLabelWithLimitCheck(tempLabels[colIndex], temp, castingFactory.AlarmLimits);

                    // Power value from DB
                    string powerValue = GetPowerValueFromDB(compressor.Name);
                    UpdateLabel(powerLabels[colIndex], powerValue, StatusRunning);
                }
            }
            else
            {
                // Other Factories Mode
                var otherFactories = config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).Take(5).ToList();

                for (int colIndex = 0; colIndex < 5; colIndex++)
                {
                    if (colIndex < otherFactories.Count)
                    {
                        var factory = otherFactories[colIndex];
                        int modbusIndex = config.Factories.IndexOf(factory);

                        if (modbusIndex >= modbusList.Count) continue;

                        var modbus = modbusList[modbusIndex];

                        // Update device name label color based on ConnectState
                        UpdateLabel(deviceNameLabels[colIndex], deviceNameLabels[colIndex].Text, modbus.ConnectState ? Color.Green : Color.Red);

                        if (modbus.address_val == null) continue;

                        var compressors = factory.GetDevicesByType(DeviceType.Compressor);
                        var compressorStatuses = GetCompressorStatuses(modbus, factory);
                        var precoolerStatus = GetDeviceStatusByConfig(modbus, factory, DeviceType.Precooler);
                        var dryerStatus = GetDeviceStatusByConfig(modbus, factory, DeviceType.Dryer);
                        var fanStatus = GetDeviceStatusByConfig(modbus, factory, DeviceType.Fan);
                        var pressure = GetPressureValue(modbus);
                        var temp = GetTempValue(modbus);

                        // Update compressor status
                        if (compressorStatuses.Count > 0)
                        {
                            string statusText = BuildCompressorStatusString(factory, compressorStatuses);
                            Color statusColor = GetOverallStatusColor(compressorStatuses);
                            UpdateLabel(statusLabels[colIndex], statusText, statusColor);
                        }
                        else
                        {
                            UpdateLabel(statusLabels[colIndex], "--", StatusDisabled);
                        }

                        UpdateScheduleLabel(scheduleLabels[colIndex], compressors);
                        UpdateLabel(precoolerLabels[colIndex], precoolerStatus.Text, precoolerStatus.Color);
                        UpdateLabel(dryerLabels[colIndex], dryerStatus.Text, dryerStatus.Color);
                        UpdateLabel(fanLabels[colIndex], fanStatus.Text, fanStatus.Color);
                        UpdatePressureLabelWithLimitCheck(pressureLabels[colIndex], pressure, factory.AlarmLimits);
                        UpdateTempLabelWithLimitCheck(tempLabels[colIndex], temp, factory.AlarmLimits);

                        // Power value from DB - build combined power text for all compressors
                        if (compressors.Count == 1)
                        {
                            string powerValue = GetPowerValueFromDB(compressors[0].Name);
                            UpdateLabel(powerLabels[colIndex], powerValue, StatusRunning);
                        }
                        else if (compressors.Count > 1)
                        {
                            var powerTexts = compressors
                                .OrderBy(c => c.MachineNo)
                                .Select(c => c.Name + ":" + GetPowerValueFromDB(c.Name));
                            UpdateLabel(powerLabels[colIndex], string.Join("\n", powerTexts), StatusRunning);
                        }
                        else
                        {
                            UpdateLabel(powerLabels[colIndex], "--", StatusDisabled);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all compressor statuses for a factory
        /// </summary>
        private List<CompressorStatus> GetCompressorStatuses(ModBus_List modbus, FactoryConfig factory)
        {
            var statuses = new List<CompressorStatus>();
            var compressors = factory.GetDevicesByType(DeviceType.Compressor);

            System.Diagnostics.Debug.WriteLine($"[{factory.Name}] Found {compressors.Count} compressors");

            foreach (var compressor in compressors.OrderBy(c => c.MachineNo))
            {
                bool isRunning = GetDIValue(modbus, compressor.IO.RunDI);
                bool isAlarm = GetDIValue(modbus, compressor.IO.AlarmDI);
                bool isFault = GetDIValue(modbus, compressor.IO.FaultDI);

                System.Diagnostics.Debug.WriteLine($"  [{compressor.Name}] MachineNo={compressor.MachineNo}, RunDI={compressor.IO.RunDI}, isRunning={isRunning}");

                DeviceStatus status;
                if (isFault)
                    status = new DeviceStatus("故障", StatusFault);
                else if (isAlarm)
                    status = new DeviceStatus("警報", StatusAlarm);
                else if (isRunning)
                    status = new DeviceStatus("運轉", StatusRunning);
                else
                    status = new DeviceStatus("停止", StatusStopped);

                statuses.Add(new CompressorStatus
                {
                    Name = compressor.Name,
                    MachineNo = compressor.MachineNo,
                    Status = status
                });
            }

            return statuses;
        }

        /// <summary>
        /// Get device status by config
        /// </summary>
        private DeviceStatus GetDeviceStatusByConfig(ModBus_List modbus, FactoryConfig factory, DeviceType deviceType)
        {
            var device = factory.GetDevice(deviceType, 1);
            
            if (device == null || !device.Enabled)
            {
                return new DeviceStatus("--", StatusDisabled);
            }

            bool isOn = GetDIValue(modbus, device.IO.OnDI);
            bool isOff = GetDIValue(modbus, device.IO.OffDI);
            bool isFault = GetDIValue(modbus, device.IO.FaultDI);

            if (isOff && isFault) return new DeviceStatus("故障", StatusFault);
            if (isOn) return new DeviceStatus("啟動", StatusRunning);
            if (isOff) return new DeviceStatus("停止" +
                "", StatusStopped);
            return new DeviceStatus("--", StatusStopped);
        }

        /// <summary>
        /// Get DI Value by number
        /// </summary>
        private bool GetDIValue(ModBus_List modbus, int diNumber)
        {
            if (diNumber < 0) return false;

            switch (diNumber)
            {
                case 0: return modbus.address_val.Address_4051_DI_0 == "1";
                case 1: return modbus.address_val.Address_4051_DI_1 == "1";
                case 2: return modbus.address_val.Address_4051_DI_2 == "1";
                case 3: return modbus.address_val.Address_4051_DI_3 == "1";
                case 4: return modbus.address_val.Address_4051_DI_4 == "1";
                case 5: return modbus.address_val.Address_4051_DI_5 == "1";
                case 6: return modbus.address_val.Address_4051_DI_6 == "1";
                case 7: return modbus.address_val.Address_4051_DI_7 == "1";
                case 8: return modbus.address_val.Address_4051_DI_8 == "1";
                case 9: return modbus.address_val.Address_4051_DI_9 == "1";
                case 10: return modbus.address_val.Address_4051_DI_10 == "1";
                case 11: return modbus.address_val.Address_4051_DI_11 == "1";
                case 12: return modbus.address_val.Address_4051_DI_12 == "1";
                case 13: return modbus.address_val.Address_4051_DI_13 == "1";
                case 14: return modbus.address_val.Address_4051_DI_14 == "1";
                case 15: return modbus.address_val.Address_4051_DI_15 == "1";
                default: return false;
            }
        }

        /// <summary>
        /// Get Pressure Value
        /// </summary>
        private string GetPressureValue(ModBus_List modbus)
        {
            try
            {
                double pressureValue = Convert.ToDouble(modbus.address_val.Address_Air_Sensor_Pressure_Value);
                double decimalPlaces = Convert.ToDouble(modbus.address_val.Address_Air_Sensor_Decimal);
                double pressure = pressureValue / Math.Pow(10, decimalPlaces);
                return pressure.ToString("F2");
            }
            catch
            {
                return "--";
            }
        }

        /// <summary>
        /// Get Temp Value
        /// </summary>
        private string GetTempValue(ModBus_List modbus)
        {
            try
            {
                double tempValue = Convert.ToDouble(modbus.address_val.Address_E5CC_1_PV);
                double temperature = tempValue;
                if (temperature == 0)
                    return "--";
                return temperature.ToString();
            }
            catch
            {
                return "--";
            }
        }

        /// <summary>
        /// 從資料庫讀取設備的功率值 (P 欄位)
        /// </summary>
        /// <param name="deviceName">設備名稱</param>
        /// <returns>功率值字串，讀取失敗時回傳 "--"</returns>
        private string GetPowerValueFromDB(string deviceName)
        {
            try
            {
                if (string.IsNullOrEmpty(config.IP) || string.IsNullOrEmpty(config.DB) || string.IsNullOrEmpty(config.machinery_factory_realtime_table1))
                    return "--";

                var db = new MYSQL(config.IP, config.DB, config.USER, config.Password);
                db.selectdata("SELECT `P` FROM `" + config.machinery_factory_realtime_table1 + "` WHERE `Meter_Name`='" + deviceName + "' ORDER BY `Time` DESC LIMIT 1");

                if (db.readdata != null && db.readdata.Count > 0 && !string.IsNullOrEmpty(db.readdata[0]))
                {
                    return db.readdata[0];
                }
                return "--";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetPowerValueFromDB] {deviceName} failed: {ex.Message}");
                return "--";
            }
        }


        /// <summary>
        /// Build Compressor Status String for Row3
        /// </summary>
        private string BuildCompressorStatusString(FactoryConfig factory, List<CompressorStatus> statuses)
        {
            if (statuses.Count == 1)
            {
                return statuses[0].Status.Text;
            }
            else
            {
                // Multiple compressors: show each status
                var statusStrings = statuses.Select(s => s.Name + ":" + s.Status.Text);
                return string.Join("\n", statusStrings);
            }
        }

        /// <summary>
        /// Get Overall Status Color (priority: Fault > Alarm > Running > Stopped)
        /// </summary>
        private Color GetOverallStatusColor(List<CompressorStatus> statuses)
        {
            if (statuses.Any(s => s.Status.Color == StatusFault))
                return StatusFault;
            if (statuses.Any(s => s.Status.Color == StatusAlarm))
                return StatusAlarm;
            if (statuses.Any(s => s.Status.Color == StatusRunning))
                return StatusRunning;
            return StatusStopped;
        }

        /// <summary>
        /// Update Label
        /// </summary>
        private void UpdateLabel(Label label, string text, Color foreColor)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() =>
                {
                    label.Text = text;
                    label.ForeColor = foreColor;
                }));
            }
            else
            {
                label.Text = text;
                label.ForeColor = foreColor;
            }
        }

        /// <summary>
        /// Update Schedule Label with background color
        /// </summary>
        private void UpdateScheduleLabel(Label label, List<DeviceConfig> compressors)
        {
            // 手動模式下顯示「手動模式」
            if (isManualMode)
            {
                UpdateLabelWithBackground(label, "手動模式", TextNormal, Color.FromArgb(0, 122, 204));
                return;
            }

            bool hasActiveSchedule = compressors.Any(c => c.Schedule.Enabled && c.Schedule.IsInSchedule());
            bool hasAnySchedule = compressors.Any(c => c.Schedule.Enabled);

            if (hasAnySchedule)
            {
                var scheduleTexts = compressors
                    .Where(c => c.Schedule.Enabled)
                    .Select(c => c.Schedule.GetDisplayText());
                string scheduleText = string.Join("\n", scheduleTexts.Distinct());

                if (hasActiveSchedule)
                {
                    UpdateLabelWithBackground(label, "時間排程", TextNormal, ScheduleActive);
                }
                else
                {
                    UpdateLabelWithBackground(label, "時間排程", TextNormal, ScheduleInactive);
                }
            }
            else
            {
                UpdateLabelWithBackground(label, "時間排程", TextNormal, ScheduleInactive);
            }
        }

        /// <summary>
        /// Update Label with text, foreground and background color
        /// </summary>
        private void UpdateLabelWithBackground(Label label, string text, Color foreColor, Color backColor)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() =>
                {
                    label.Text = text;
                    label.ForeColor = foreColor;
                    label.BackColor = backColor;
                }));
            }
            else
            {
                label.Text = text;
                label.ForeColor = foreColor;
                label.BackColor = backColor;
            }
        }

        /// <summary>
        /// 排程控制 - 根據排程時間自動控制 DO 輸出
        /// 當目前時間在排程內 → DO=1 (啟動)
        /// 當目前時間不在排程內 → DO=0 (停止)
        /// 手動模式下不執行自動排程控制
        /// </summary>
        private void ExecuteScheduleControl()
        {
            // 手動模式下不執行自動排程控制
            if (isManualMode) return;

            for (int i = 0; i < config.Factories.Count; i++)
            {
                var factory = config.Factories[i];
                if (i >= modbusList.Count) continue;

                var modbus = modbusList[i];
                if (!modbus.ConnectState || modbus.address_val == null) continue;

                var compressors = factory.GetDevicesByType(DeviceType.Compressor);
                foreach (var compressor in compressors)
                {
                    // 只處理有設定 controlDO 且有啟用排程的設備
                    if (compressor.IO.ControlDO < 0 || !compressor.Schedule.Enabled)
                        continue;

                    bool isInSchedule = compressor.Schedule.IsInSchedule();
                    ushort targetValue = isInSchedule ? (ushort)1 : (ushort)0;

                    // 用 FactoryId_MachineNo 作為 key 來追蹤狀態
                    string key = factory.Id + "_" + compressor.MachineNo;

                    // 只在狀態變化時才寫入，避免每秒重複寫入
                    ushort lastValue;
                    if (!lastDOStates.TryGetValue(key, out lastValue) || lastValue != targetValue)
                    {
                        bool success = modbus.WriteDO(compressor.IO.ControlDO, targetValue);
                        if (success)
                        {
                            lastDOStates[key] = targetValue;
                            System.Diagnostics.Debug.WriteLine(
                                $"[排程控制] {factory.Name} {compressor.Name} (MachineNo={compressor.MachineNo}) " +
                                $"DO_{compressor.IO.ControlDO} = {targetValue} ({(isInSchedule ? "啟動" : "停止")})");
                        }
                    }
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            updateTimer?.Stop();
            updateTimer?.Dispose();
            base.OnFormClosing(e);
        }

        private void Factory_Click(object sender, EventArgs e)
        {
            if (currentViewMode == ViewMode.OtherFactories)
            {
                // Switch to Casting Factory view
                currentViewMode = ViewMode.CastingFactory;
                Factory.Text = "鑄造廠域_空壓系統即時狀態";
                
                // Find casting factory index
                var castingFactory = config.Factories.FirstOrDefault(f => f.Id == CASTING_FACTORY_ID);
                if (castingFactory != null)
                {
                    int castingIndex = config.Factories.IndexOf(castingFactory);
                    // Display casting factory in all 5 columns (for 3 compressors)
                    currentDisplayIndices = new int[] { castingIndex, -1, -1, -1, -1 };
                }
            }
            else
            {
                // Switch to Other Factories view
                currentViewMode = ViewMode.OtherFactories;
                Factory.Text = "其它廠域_空壓系統即時狀態";
                
                // Display first 5 factories (excluding casting factory)
                var otherFactories = config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).Take(5).ToList();
                currentDisplayIndices = new int[5];
                for (int i = 0; i < 5; i++)
                {
                    if (i < otherFactories.Count)
                        currentDisplayIndices[i] = config.Factories.IndexOf(otherFactories[i]);
                    else
                        currentDisplayIndices[i] = -1;
                }
            }
            
            // Refresh display
            RefreshFactoryDisplay();
        }

        /// <summary>
        /// Refresh all factory displays based on current view mode
        /// </summary>
        private void RefreshFactoryDisplay()
        {
            Label[] factoryHeaders = { factory_col1, factory_col2, factory_col3, factory_col4, factory_col5 };
            Label[] deviceNameLabels = { device_col1, device_col2, device_col3, device_col4, device_col5 };
            Label[] scheduleLabels = { schedule_col1, schedule_col2, schedule_col3, schedule_col4, schedule_col5 };
            Label[] statusLabels = { status_col1, status_col2, status_col3, status_col4, status_col5 };
            Label[] precoolerLabels = { precooler_col1, precooler_col2, precooler_col3, precooler_col4, precooler_col5 };
            Label[] dryerLabels = { dryer_col1, dryer_col2, dryer_col3, dryer_col4, dryer_col5 };
            Label[] fanLabels = { fan_col1, fan_col2, fan_col3, fan_col4, fan_col5 };
            Label[] pressureLabels = { pressure_col1, pressure_col2, pressure_col3, pressure_col4, pressure_col5 };
            Label[] tempLabels = { temp_col1, temp_col2, temp_col3, temp_col4, temp_col5 };
            Label[] powerLabels = { power_col1, power_col2, power_col3, power_col4, power_col5 };

            if (currentViewMode == ViewMode.CastingFactory)
            {
                // Casting Factory Mode - Each compressor in separate column
                var castingFactory = config.Factories.FirstOrDefault(f => f.Id == CASTING_FACTORY_ID);
                if (castingFactory != null)
                {
                    var compressors = castingFactory.GetDevicesByType(DeviceType.Compressor).OrderBy(c => c.MachineNo).ToList();
                    
                    for (int i = 0; i < 5; i++)
                    {
                        if (i < compressors.Count)
                        {
                            var compressor = compressors[i];
                            // Header: Factory name
                            UpdateLabel(factoryHeaders[i], castingFactory.Name, TextNormal);
                            // Device name: Compressor name
                            UpdateLabel(deviceNameLabels[i], compressor.Name, TextNormal);
                            // Schedule
                            UpdateScheduleLabel(scheduleLabels[i], new List<DeviceConfig> { compressor });
                            // Status will be updated in timer
                            UpdateLabel(statusLabels[i], "--", StatusDisabled);
                            // Power will be updated in timer
                            UpdateLabel(powerLabels[i], "--", StatusDisabled);
                            // Common devices will be updated in timer (show in all columns)
                        }
                        else
                        {
                            // Hide unused columns
                            UpdateLabel(factoryHeaders[i], "--", StatusDisabled);
                            UpdateLabel(deviceNameLabels[i], "--", StatusDisabled);
                            UpdateLabelWithBackground(scheduleLabels[i], "--", TextNormal, StatusDisabled);
                            UpdateLabel(statusLabels[i], "--", StatusDisabled);
                            UpdateLabel(precoolerLabels[i], "--", StatusDisabled);
                            UpdateLabel(dryerLabels[i], "--", StatusDisabled);
                            UpdateLabel(fanLabels[i], "--", StatusDisabled);
                            UpdateLabel(pressureLabels[i], "--", StatusDisabled);
                            UpdateLabel(tempLabels[i], "--", StatusDisabled);
                            UpdateLabel(powerLabels[i], "--", StatusDisabled);
                        }
                    }
                }
            }
            else
            {
                // Other Factories Mode
                var otherFactories = config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).Take(5).ToList();
                
                for (int i = 0; i < 5; i++)
                {
                    if (i < otherFactories.Count)
                    {
                        var factory = otherFactories[i];
                        var compressors = factory.GetDevicesByType(DeviceType.Compressor);
                        
                        UpdateLabel(factoryHeaders[i], factory.Name, TextNormal);
                        
                        if (compressors.Count > 0)
                        {
                            string deviceNames = BuildCompressorNameString(factory, compressors);
                            UpdateLabel(deviceNameLabels[i], deviceNames, TextNormal);
                            UpdateScheduleLabel(scheduleLabels[i], compressors);
                        }
                        else
                        {
                            UpdateLabel(deviceNameLabels[i], "--", StatusDisabled);
                            UpdateLabelWithBackground(scheduleLabels[i], "No Schedule", TextNormal, ScheduleInactive);
                        }
                    }
                    else
                    {
                        UpdateLabel(factoryHeaders[i], "--", StatusDisabled);
                        UpdateLabel(deviceNameLabels[i], "--", StatusDisabled);
                        UpdateLabelWithBackground(scheduleLabels[i], "--", TextNormal, StatusDisabled);
                        UpdateLabel(statusLabels[i], "--", StatusDisabled);
                        UpdateLabel(precoolerLabels[i], "--", StatusDisabled);
                        UpdateLabel(dryerLabels[i], "--", StatusDisabled);
                        UpdateLabel(fanLabels[i], "--", StatusDisabled);
                        UpdateLabel(pressureLabels[i], "--", StatusDisabled);
                        UpdateLabel(tempLabels[i], "--", StatusDisabled);
                        UpdateLabel(powerLabels[i], "--", StatusDisabled);
                    }
                }
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            // 取得當前模式名稱，傳入排程設定表單
            string modeName = currentMode != null ? currentMode.Name : label3.Text;
            int modeId = currentMode != null ? currentMode.Id : 0;
            
            // 依目前廠域篩選工廠清單
            var factories = GetCurrentViewFactories();
            var factoryIds = new HashSet<int>(factories.Select(f => f.Id));

            ScheduleSettingForm scheduleForm = new ScheduleSettingForm(modeId, modeName, factoryIds);
            scheduleForm.ShowDialog();

            // 重新載入設定
            if (scheduleForm.DialogResult == DialogResult.OK)
            {
                config.LoadConfig();
                RefreshFactoryDisplay();
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            using (var modeSelectForm = new ModeSelectForm())
            {
                if (modeSelectForm.ShowDialog() == DialogResult.OK && modeSelectForm.SelectedMode != null)
                {
                    var selectedMode = modeSelectForm.SelectedMode;
                    
                    // 儲存當前選擇的模式
                    currentMode = selectedMode;
                    
                    // 判斷是否為手動模式（模式名稱包含「手動」）
                    bool wasManual = isManualMode;
                    isManualMode = selectedMode.Name.Contains("手動");
                    
                    // 切換到手動模式時，清除手動 DO 狀態
                    if (isManualMode && !wasManual)
                    {
                        manualDOStates.Clear();
                        lastDOStates.Clear();
                    }
                    
                    // 更新 label3 顯示模式名稱
                    label3.Text = selectedMode.Name;
                    
                    // 更新 label4 顯示模式描述（如果有的話）
                    if (!string.IsNullOrEmpty(selectedMode.Description))
                    {
                        label4.Text = selectedMode.Description;
                    }
                    
                    // 重新載入設定（因為模式切換時已套用排程到設備）
                    config.LoadConfig();
                    RefreshFactoryDisplay();
                    UpdateStatusLabelCursors();
                }
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            TrendChart trendChart = new TrendChart();
            trendChart.Show();
        }

        /// <summary>
        /// device_col1 ~ device_col5 點選事件 - 開啟壓力/溫度曲線圖
        /// </summary>
        private void DeviceCol_Click(object sender, EventArgs e)
        {
            Label clickedLabel = sender as Label;
            if (clickedLabel == null) return;

            // 判斷點選的是哪一欄
            Label[] deviceLabels = { device_col1, device_col2, device_col3, device_col4, device_col5 };
            Label[] factoryLabels = { factory_col1, factory_col2, factory_col3, factory_col4, factory_col5 };

            string factoryName = null;
            for (int i = 0; i < deviceLabels.Length; i++)
            {
                if (clickedLabel == deviceLabels[i])
                {
                    factoryName = factoryLabels[i].Text;
                    break;
                }
            }

            DeviceTrendChartForm chartForm = new DeviceTrendChartForm(factoryName);
            chartForm.Show();
        }

        /// <summary>
        /// pressure_col1 ~ pressure_col5 點選事件 - 設定空壓上下限（依目前廠域）
        /// </summary>
        private void PressureCol_Click(object sender, EventArgs e)
        {
            var factories = GetCurrentViewFactories();
            using (var form = new AlarmLimitSettingForm(factories, "Pressure"))
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (var kvp in form.ResultLimitsMap)
                    {
                        config.SaveAlarmLimits(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        /// <summary>
        /// temp_col1 ~ temp_col5 點選事件 - 設定溫度上下限（依目前廠域）
        /// </summary>
        private void TempCol_Click(object sender, EventArgs e)
        {
            var factories = GetCurrentViewFactories();
            using (var form = new AlarmLimitSettingForm(factories, "Temp"))
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (var kvp in form.ResultLimitsMap)
                    {
                        config.SaveAlarmLimits(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 根據目前的 ViewMode 取得對應的工廠清單
        /// </summary>
        private List<FactoryConfig> GetCurrentViewFactories()
        {
            if (currentViewMode == ViewMode.CastingFactory)
            {
                return config.Factories.Where(f => f.Id == CASTING_FACTORY_ID).ToList();
            }
            else
            {
                return config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).ToList();
            }
        }

        /// <summary>
        /// 根據目前顯示欄位索引取得對應的 FactoryConfig
        /// </summary>
        private FactoryConfig GetFactoryByColumnIndex(int colIndex)
        {
            if (currentViewMode == ViewMode.CastingFactory)
            {
                return config.Factories.FirstOrDefault(f => f.Id == CASTING_FACTORY_ID);
            }
            else
            {
                var otherFactories = config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).Take(5).ToList();
                if (colIndex < otherFactories.Count)
                    return otherFactories[colIndex];
                return null;
            }
        }

        /// <summary>
        /// 更新空壓 Label 並檢查是否超過上下限，超過則變色並呼叫推播函式
        /// </summary>
        private void UpdatePressureLabelWithLimitCheck(Label label, string valueText, AlarmLimitsConfig limits)
        {
            double value;
            if (double.TryParse(valueText, out value))
            {
                bool overLimit = (limits.PressureUpperLimit != double.MaxValue && value > limits.PressureUpperLimit)
                              || (limits.PressureLowerLimit != double.MinValue && value < limits.PressureLowerLimit);
                if (overLimit)
                {
                    UpdateLabel(label, valueText, StatusOverLimit);
                    // 呼叫推播通知
                    OnPressureOverLimit(label.FindForm()?.Text, valueText, limits);
                }
                else
                {
                    UpdateLabel(label, valueText, StatusRunning);
                }
            }
            else
            {
                UpdateLabel(label, valueText, StatusRunning);
            }
        }

        /// <summary>
        /// 更新溫度 Label 並檢查是否超過上下限，超過則變色並呼叫推播函式
        /// </summary>
        private void UpdateTempLabelWithLimitCheck(Label label, string valueText, AlarmLimitsConfig limits)
        {
            double value;
            if (double.TryParse(valueText, out value))
            {
                bool overLimit = (limits.TempUpperLimit != double.MaxValue && value > limits.TempUpperLimit)
                              || (limits.TempLowerLimit != double.MinValue && value < limits.TempLowerLimit);
                if (overLimit)
                {
                    UpdateLabel(label, valueText, StatusOverLimit);
                    // 呼叫推播通知
                    OnTempOverLimit(label.FindForm()?.Text, valueText, limits);
                }
                else
                {
                    UpdateLabel(label, valueText, StatusRunning);
                }
            }
            else
            {
                UpdateLabel(label, valueText, StatusRunning);
            }
        }

        /// <summary>
        /// 【預留】空壓數值超過上下限時的推播通知函式
        /// TODO: 實作推播通知邏輯（例如 Line Notify、Email、簡訊等）
        /// </summary>
        /// <param name="source">來源資訊</param>
        /// <param name="currentValue">目前空壓數值</param>
        /// <param name="limits">設定的上下限</param>
        private void OnPressureOverLimit(string source, string currentValue, AlarmLimitsConfig limits)
        {
            // TODO: 在此實作空壓超限推播通知
            // 例如：發送 Line Notify、Email、簡訊通知相關人員
            // System.Diagnostics.Debug.WriteLine($"[推播] 空壓超限! 來源={source}, 數值={currentValue}, 上限={limits.PressureUpperLimit}, 下限={limits.PressureLowerLimit}");
        }

        /// <summary>
        /// 【預留】溫度數值超過上下限時的推播通知函式
        /// TODO: 實作推播通知邏輯（例如 Line Notify、Email、簡訊等）
        /// </summary>
        /// <param name="source">來源資訊</param>
        /// <param name="currentValue">目前溫度數值</param>
        /// <param name="limits">設定的上下限</param>
        private void OnTempOverLimit(string source, string currentValue, AlarmLimitsConfig limits)
        {
            // TODO: 在此實作溫度超限推播通知
            // 例如：發送 Line Notify、Email、簡訊通知相關人員
            // System.Diagnostics.Debug.WriteLine($"[推播] 溫度超限! 來源={source}, 數值={currentValue}, 上限={limits.TempUpperLimit}, 下限={limits.TempLowerLimit}");
        }

        /// <summary>
        /// 更新 status_col 的游標樣式（手動模式下顯示手型游標）
        /// </summary>
        private void UpdateStatusLabelCursors()
        {
            Label[] statusLabels = { status_col1, status_col2, status_col3, status_col4, status_col5 };
            foreach (var label in statusLabels)
            {
                label.Cursor = isManualMode ? Cursors.Hand : Cursors.Default;
            }
        }

        /// <summary>
        /// status_col1 ~ status_col5 點選事件 - 手動模式下切換空壓機啟動/停止
        /// </summary>
        private void StatusCol_Click(object sender, EventArgs e)
        {
            if (!isManualMode) return;

            Label clickedLabel = sender as Label;
            if (clickedLabel == null) return;

            Label[] statusLabels = { status_col1, status_col2, status_col3, status_col4, status_col5 };
            int colIndex = -1;
            for (int i = 0; i < statusLabels.Length; i++)
            {
                if (clickedLabel == statusLabels[i])
                {
                    colIndex = i;
                    break;
                }
            }
            if (colIndex < 0) return;

            if (currentViewMode == ViewMode.CastingFactory)
            {
                // 鑄造廠模式：每欄一台壓縮機
                var castingFactory = config.Factories.FirstOrDefault(f => f.Id == CASTING_FACTORY_ID);
                if (castingFactory == null) return;

                int modbusIndex = config.Factories.IndexOf(castingFactory);
                if (modbusIndex >= modbusList.Count) return;

                var modbus = modbusList[modbusIndex];
                var compressors = castingFactory.GetDevicesByType(DeviceType.Compressor).OrderBy(c => c.MachineNo).ToList();
                if (colIndex >= compressors.Count) return;

                var compressor = compressors[colIndex];
                ToggleCompressorManual(castingFactory, compressor, modbus);
            }
            else
            {
                // 其它廠域模式：每欄一個工廠
                var otherFactories = config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).Take(5).ToList();
                if (colIndex >= otherFactories.Count) return;

                var factory = otherFactories[colIndex];
                int modbusIndex = config.Factories.IndexOf(factory);
                if (modbusIndex >= modbusList.Count) return;

                var modbus = modbusList[modbusIndex];
                var compressors = factory.GetDevicesByType(DeviceType.Compressor);

                if (compressors.Count == 1)
                {
                    ToggleCompressorManual(factory, compressors[0], modbus);
                }
                else if (compressors.Count > 1)
                {
                    // 多台壓縮機時，彈出選擇視窗
                    using (var selectForm = new ManualCompressorSelectForm(factory, compressors, manualDOStates))
                    {
                        if (selectForm.ShowDialog() == DialogResult.OK && selectForm.SelectedCompressor != null)
                        {
                            ToggleCompressorManual(factory, selectForm.SelectedCompressor, modbus);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 手動切換壓縮機的啟動/停止
        /// </summary>
        private void ToggleCompressorManual(FactoryConfig factory, DeviceConfig compressor, ModBus_List modbus)
        {
            if (compressor.IO.ControlDO < 0)
            {
                MessageBox.Show($"{compressor.Name} 未設定控制 DO，無法手動控制", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!modbus.ConnectState)
            {
                MessageBox.Show($"{factory.Name} 通訊中斷，無法控制", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string key = factory.Id + "_" + compressor.MachineNo;

            // 取得目前手動狀態，預設為停止(0)
            ushort currentState;
            if (!manualDOStates.TryGetValue(key, out currentState))
                currentState = 0;

            // 切換狀態
            ushort newState = currentState == 1 ? (ushort)0 : (ushort)1;
            string actionText = newState == 1 ? "啟動" : "停止";

            var result = MessageBox.Show(
                $"確定要{actionText} {factory.Name} - {compressor.Name} 嗎？",
                "手動控制確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                bool success = modbus.WriteDO(compressor.IO.ControlDO, newState);
                if (success)
                {
                    manualDOStates[key] = newState;
                    lastDOStates[key] = newState;
                    System.Diagnostics.Debug.WriteLine(
                        $"[手動控制] {factory.Name} {compressor.Name} DO_{compressor.IO.ControlDO} = {newState} ({actionText})");
                }
                else
                {
                    MessageBox.Show($"控制指令發送失敗", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

    }

    /// <summary>
    /// Device Status Structure
    /// </summary>
    public struct DeviceStatus
    {
        public string Text { get; }
        public Color Color { get; }

        public DeviceStatus(string text, Color color)
        {
            Text = text;
            Color = color;
        }
    }

    /// <summary>
    /// Compressor Status Structure
    /// </summary>
    public struct CompressorStatus
    {
        public string Name { get; set; }
        public int MachineNo { get; set; }
        public DeviceStatus Status { get; set; }
    }
}
