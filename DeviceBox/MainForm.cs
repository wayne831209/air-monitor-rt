using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        // View Mode
        private enum ViewMode { OtherFactories, CastingFactory }
        private ViewMode currentViewMode = ViewMode.OtherFactories;
        private int[] currentDisplayIndices = { 0, 1, 2, 3, 4 };  // Factory indices to display
        private const int CASTING_FACTORY_ID = 6;  // Casting Factory ID in config

        private Timer updateTimer;
        private List<ModBus_List> modbusList;
        private Config config;

        public MainForm()
        {
            InitializeComponent();
            InitializeConfig();
            InitializeModbus();
            InitializeTimer();
            InitializeFactoryHeaders();
            InitializeCompressorNames();
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

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (modbusList == null || modbusList.Count == 0)
                return;

            try
            {
                UpdateAllFactories();
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
            Label[] scheduleLabels = { schedule_col1, schedule_col2, schedule_col3, schedule_col4, schedule_col5 };
            Label[] statusLabels = { status_col1, status_col2, status_col3, status_col4, status_col5 };
            Label[] precoolerLabels = { precooler_col1, precooler_col2, precooler_col3, precooler_col4, precooler_col5 };
            Label[] dryerLabels = { dryer_col1, dryer_col2, dryer_col3, dryer_col4, dryer_col5 };
            Label[] fanLabels = { fan_col1, fan_col2, fan_col3, fan_col4, fan_col5 };
            Label[] pressureLabels = { pressure_col1, pressure_col2, pressure_col3, pressure_col4, pressure_col5 };
            Label[] tempLabels = { temp_col1, temp_col2, temp_col3, temp_col4, temp_col5 };

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
                    UpdateLabel(pressureLabels[colIndex], pressure, StatusRunning);
                    UpdateLabel(tempLabels[colIndex], temp, StatusRunning);
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
                        UpdateLabel(pressureLabels[colIndex], pressure, StatusRunning);
                        UpdateLabel(tempLabels[colIndex], temp, StatusRunning);
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
                return temperature.ToString("F2");
            }
            catch
            {
                return "--";
            }
        }

        /// <summary>
        /// Update Factory Labels
        /// </summary>
        private void UpdateFactoryLabels(int factoryIndex, FactoryConfig factory, 
            List<CompressorStatus> compressorStatuses, DeviceStatus precooler, 
            DeviceStatus dryer, DeviceStatus fan, string pressure,string temp)
        {
            // Row3: Status Labels
            Label[] statusLabels = { status_col1, status_col2, status_col3, status_col4, status_col5 };
            // Precooler
            Label[] precoolerLabels = { precooler_col1, precooler_col2, precooler_col3, precooler_col4, precooler_col5 };
            // Dryer
            Label[] dryerLabels = { dryer_col1, dryer_col2, dryer_col3, dryer_col4, dryer_col5 };
            // Fan
            Label[] fanLabels = { fan_col1, fan_col2, fan_col3, fan_col4, fan_col5 };
            // Pressure
            Label[] pressureLabels = { pressure_col1, pressure_col2, pressure_col3, pressure_col4, pressure_col5 };
            //Temp
            Label[] tempLabels = { temp_col1, temp_col2, temp_col3, temp_col4, temp_col5 };

            if (factoryIndex < statusLabels.Length)
            {
                // Build status string for Row3
                if (compressorStatuses.Count > 0)
                {
                    string statusText = BuildCompressorStatusString(factory, compressorStatuses);
                    Color statusColor = GetOverallStatusColor(compressorStatuses);
                    UpdateLabel(statusLabels[factoryIndex], statusText, statusColor);
                }
                else
                {
                    UpdateLabel(statusLabels[factoryIndex], "--", StatusDisabled);
                }

                // Other devices
                UpdateLabel(precoolerLabels[factoryIndex], precooler.Text, precooler.Color);
                UpdateLabel(dryerLabels[factoryIndex], dryer.Text, dryer.Color);
                UpdateLabel(fanLabels[factoryIndex], fan.Text, fan.Color);
                UpdateLabel(pressureLabels[factoryIndex], pressure, StatusRunning);
                UpdateLabel(tempLabels[factoryIndex], temp, StatusRunning);
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
                    }
                }
            }
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
