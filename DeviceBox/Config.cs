using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace DeviceBox
{
    #region Device Configuration Classes

    /// <summary>
    /// Device Type Enum
    /// </summary>
    public enum DeviceType
    {
        Compressor,      // Air Compressor
        Precooler,       // Precooler
        Dryer,           // Refrigerated Dryer
        Fan,             // Ventilation Fan
        PressureSensor,  // Pressure Sensor
        ReadyStatus      // Ready Status
    }

    /// <summary>
    /// Schedule Time Range - 單一時間段
    /// </summary>
    public class ScheduleTimeRange
    {
        public TimeSpan StartTime { get; set; } = TimeSpan.Zero;
        public TimeSpan EndTime { get; set; } = TimeSpan.Zero;
        public List<DayOfWeek> Days { get; set; } = new List<DayOfWeek>();

        public ScheduleTimeRange() { }

        public ScheduleTimeRange(TimeSpan start, TimeSpan end)
        {
            StartTime = start;
            EndTime = end;
        }

        public ScheduleTimeRange(TimeSpan start, TimeSpan end, List<DayOfWeek> days)
        {
            StartTime = start;
            EndTime = end;
            Days = days != null ? new List<DayOfWeek>(days) : new List<DayOfWeek>();
        }

        /// <summary>
        /// Check if specified time and day is within this range
        /// </summary>
        public bool IsInRange(TimeSpan time, DayOfWeek day)
        {
            // 不跨午夜的排程 (例如 08:00~17:00)
            if (StartTime <= EndTime)
            {
                if (Days.Count > 0 && !Days.Contains(day))
                    return false;
                return time >= StartTime && time <= EndTime;
            }
            else
            {
                // 跨午夜的排程 (例如 20:00~08:00)
                // 午夜前的部分 (20:00~23:59): 檢查當天是否在排程日
                if (time >= StartTime)
                {
                    if (Days.Count > 0 && !Days.Contains(day))
                        return false;
                    return true;
                }
                // 午夜後的部分 (00:00~08:00): 檢查前一天是否在排程日
                if (time <= EndTime)
                {
                    DayOfWeek previousDay = (day == DayOfWeek.Sunday) ? DayOfWeek.Saturday : (DayOfWeek)((int)day - 1);
                    if (Days.Count > 0 && !Days.Contains(previousDay))
                        return false;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Check if specified time is within this range (ignore day)
        /// </summary>
        public bool IsInRange(TimeSpan time)
        {
            if (StartTime <= EndTime)
                return time >= StartTime && time <= EndTime;
            else
                return time >= StartTime || time <= EndTime;
        }

        /// <summary>
        /// Get display text for this time range
        /// </summary>
        public string GetDisplayText()
        {
            return StartTime.ToString(@"hh\:mm") + "-" + EndTime.ToString(@"hh\:mm");
        }

        /// <summary>
        /// Clone this time range
        /// </summary>
        public ScheduleTimeRange Clone()
        {
            return new ScheduleTimeRange(StartTime, EndTime, Days);
        }
    }

    /// <summary>
    /// Schedule Configuration
    /// </summary>
    public class ScheduleConfig
    {
        public bool Enabled { get; set; } = false;
        public List<ScheduleTimeRange> TimeRanges { get; set; } = new List<ScheduleTimeRange>();
        public List<DayOfWeek> Days { get; set; } = new List<DayOfWeek>();

        // 舊屬性 - 為了向後相容，取得第一組時間
        public TimeSpan StartTime
        {
            get => TimeRanges.Count > 0 ? TimeRanges[0].StartTime : TimeSpan.Zero;
            set
            {
                if (TimeRanges.Count == 0)
                    TimeRanges.Add(new ScheduleTimeRange());
                TimeRanges[0].StartTime = value;
            }
        }

        public TimeSpan EndTime
        {
            get => TimeRanges.Count > 0 ? TimeRanges[0].EndTime : TimeSpan.Zero;
            set
            {
                if (TimeRanges.Count == 0)
                    TimeRanges.Add(new ScheduleTimeRange());
                TimeRanges[0].EndTime = value;
            }
        }

        /// <summary>
        /// Check if current time is within any schedule time range
        /// </summary>
        public bool IsInSchedule()
        {
            if (!Enabled) return false;
            if (TimeRanges.Count == 0) return false;

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;
            var currentDay = now.DayOfWeek;

            // Check if current time is in any time range
            foreach (var range in TimeRanges)
            {
                if (range.IsInRange(currentTime, currentDay))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get schedule display text
        /// </summary>
        public string GetDisplayText()
        {
            if (!Enabled) return "No Schedule";
            if (TimeRanges.Count == 0) return "No Time Range";

            var texts = TimeRanges.Select(r => r.GetDisplayText());
            return string.Join(", ", texts);
        }
    }

    /// <summary>
    /// I/O Configuration
    /// </summary>
    public class IOConfig
    {
        public int RunDI { get; set; } = -1;      // Run DI (Compressor)
        public int AlarmDI { get; set; } = -1;    // Alarm DI (Compressor)
        public int FaultDI { get; set; } = -1;    // Fault DI
        public int OnDI { get; set; } = -1;       // On DI (Other devices)
        public int OffDI { get; set; } = -1;      // Off DI (Other devices)
        public int ReadyDI { get; set; } = -1;    // Ready DI
        public int ControlDO { get; set; } = -1;  // Control DO (排程控制輸出)
        public string Adam { get; set; } = "4051"; // ADAM Module
    }

    /// <summary>
    /// Device Configuration
    /// </summary>
    public class DeviceConfig
    {
        public DeviceType Type { get; set; }
        public int MachineNo { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public IOConfig IO { get; set; }
        public ScheduleConfig Schedule { get; set; }

        public DeviceConfig()
        {
            IO = new IOConfig();
            Schedule = new ScheduleConfig();
        }
    }

    /// <summary>
    /// 警報上下限設定
    /// </summary>
    public class AlarmLimitsConfig
    {
        public double PressureUpperLimit { get; set; } = double.MaxValue;
        public double PressureLowerLimit { get; set; } = double.MinValue;
        public double TempUpperLimit { get; set; } = double.MaxValue;
        public double TempLowerLimit { get; set; } = double.MinValue;
    }

    /// <summary>
    /// Factory Configuration
    /// </summary>
    public class FactoryConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ModbusIp { get; set; }
        public string ModbusPort { get; set; }
        public List<DeviceConfig> Devices { get; set; }
        public AlarmLimitsConfig AlarmLimits { get; set; }

        public FactoryConfig()
        {
            Devices = new List<DeviceConfig>();
            AlarmLimits = new AlarmLimitsConfig();
        }

        public List<DeviceConfig> GetDevicesByType(DeviceType type)
        {
            return Devices.Where(d => d.Type == type && d.Enabled).ToList();
        }

        public DeviceConfig GetDevice(DeviceType type, int machineNo)
        {
            return Devices.FirstOrDefault(d => d.Type == type && d.MachineNo == machineNo && d.Enabled);
        }
    }

    #endregion

    public class Config
    {
        // Database Settings
        public string IP;
        public string DB;
        public string USER;
        public string Password;
        public string mysql_on;
        public string machinery_factory_demand_table1;
        public string machinery_factory_devicebox_table1;



        // Modbus Settings (Legacy)
        public List<string> ModBus_IP = new List<string>();
        public List<string> ModBus_Port = new List<string>();
        public List<string> ModBus_Name = new List<string>();
        public List<byte> ModBus_ID = new List<byte>();
        public List<int> Meter_Select = new List<int>();
        public List<int> Meter_contract = new List<int>();
        public List<string> Meter_factory = new List<string>();
        public List<int> Meter_Layer = new List<int>();
        public List<int> Meter_Mode = new List<int>();
        public List<int> SQL_Table = new List<int>();

        // Factory Settings
        public List<FactoryConfig> Factories = new List<FactoryConfig>();

        private static readonly string ConfigFileName = "config.xml";

        public Config()
        {
        }

        /// <summary>
        /// Load all configurations from config.xml
        /// </summary>
        public bool LoadConfig()
        {
            try
            {
                string configPath = Path.Combine(System.Windows.Forms.Application.StartupPath, ConfigFileName);

                if (!File.Exists(configPath))
                {
                    System.Diagnostics.Debug.WriteLine("Config file not found: " + configPath);
                    return false;
                }

                XDocument doc = XDocument.Load(configPath);
                var root = doc.Root;

                if (root == null)
                {
                    return false;
                }

                // Load Database Settings
                LoadDatabaseSettings(root.Element("Database"));

                // Load Factory Settings
                LoadFactorySettings(root.Element("Factories"));

                System.Diagnostics.Debug.WriteLine("Loaded " + Factories.Count + " factories");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Load config failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Load Database Settings
        /// </summary>
        private void LoadDatabaseSettings(XElement dbElement)
        {
            if (dbElement == null) return;

            mysql_on = dbElement.Element("mysql_on")?.Value ?? "0";
            IP = dbElement.Element("IP")?.Value ?? "";
            DB = dbElement.Element("DB")?.Value ?? "";
            USER = dbElement.Element("USER")?.Value ?? "";
            Password = dbElement.Element("Password")?.Value ?? "";
            machinery_factory_demand_table1 = dbElement.Element("machinery_factory_demand_table1")?.Value ?? "";
            machinery_factory_devicebox_table1 = dbElement.Element("machinery_factory_devicebox_table1")?.Value ?? "";
        }

        /// <summary>
        /// Load Factory Settings
        /// </summary>
        private void LoadFactorySettings(XElement factoriesElement)
        {
            if (factoriesElement == null) return;

            Factories.Clear();
            ModBus_IP.Clear();
            ModBus_Port.Clear();
            ModBus_Name.Clear();

            foreach (var factoryElement in factoriesElement.Elements("Factory"))
            {
                var factory = ParseFactory(factoryElement);
                if (factory != null)
                {
                    Factories.Add(factory);
                    ModBus_IP.Add(factory.ModbusIp);
                    ModBus_Port.Add(factory.ModbusPort);
                    ModBus_Name.Add(factory.Name);
                }
            }
        }

        /// <summary>
        /// Parse Factory Element
        /// </summary>
        private FactoryConfig ParseFactory(XElement element)
        {
            var factory = new FactoryConfig
            {
                Id = int.Parse(element.Attribute("id")?.Value ?? "0"),
                Name = element.Attribute("name")?.Value ?? "",
                ModbusIp = element.Attribute("modbusIp")?.Value ?? "",
                ModbusPort = element.Attribute("modbusPort")?.Value ?? "502"
            };

            var devicesElement = element.Element("Devices");
            if (devicesElement != null)
            {
                foreach (var deviceElement in devicesElement.Elements("Device"))
                {
                    var device = ParseDevice(deviceElement);
                    if (device != null)
                    {
                        factory.Devices.Add(device);
                        System.Diagnostics.Debug.WriteLine($"[Config] Factory={factory.Name}, Device={device.Name}, Type={device.Type}, MachineNo={device.MachineNo}, Enabled={device.Enabled}");
                    }
                }
            }

            // Parse AlarmLimits
            var alarmLimitsElement = element.Element("AlarmLimits");
            if (alarmLimitsElement != null)
            {
                factory.AlarmLimits = ParseAlarmLimits(alarmLimitsElement);
            }

            System.Diagnostics.Debug.WriteLine($"[Config] Factory={factory.Name} loaded {factory.Devices.Count} devices, Compressors={factory.Devices.Count(d => d.Type == DeviceType.Compressor)}");

            return factory;
        }

        /// <summary>
        /// Parse Device Element
        /// </summary>
        private DeviceConfig ParseDevice(XElement element)
        {
            var device = new DeviceConfig
            {
                Name = element.Attribute("name")?.Value ?? "",
                MachineNo = int.Parse(element.Attribute("machineNo")?.Value ?? "1"),
                Enabled = bool.Parse(element.Attribute("enabled")?.Value ?? "true")
            };

            string typeStr = element.Attribute("type")?.Value ?? "";
            if (Enum.TryParse(typeStr, out DeviceType type))
            {
                device.Type = type;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Unknown device type: " + typeStr);
                return null;
            }

            var ioElement = element.Element("IO");
            if (ioElement != null)
            {
                device.IO = ParseIO(ioElement);
            }

            var scheduleElement = element.Element("Schedule");
            if (scheduleElement != null)
            {
                device.Schedule = ParseSchedule(scheduleElement);
            }

            return device;
        }

        /// <summary>
        /// Parse IO Element
        /// </summary>
        private IOConfig ParseIO(XElement element)
        {
            var io = new IOConfig();

            if (element.Attribute("runDI") != null)
                io.RunDI = int.Parse(element.Attribute("runDI").Value);
            if (element.Attribute("alarmDI") != null)
                io.AlarmDI = int.Parse(element.Attribute("alarmDI").Value);
            if (element.Attribute("faultDI") != null)
                io.FaultDI = int.Parse(element.Attribute("faultDI").Value);
            if (element.Attribute("onDI") != null)
                io.OnDI = int.Parse(element.Attribute("onDI").Value);
            if (element.Attribute("offDI") != null)
                io.OffDI = int.Parse(element.Attribute("offDI").Value);
            if (element.Attribute("readyDI") != null)
                io.ReadyDI = int.Parse(element.Attribute("readyDI").Value);
            if (element.Attribute("controlDO") != null)
                io.ControlDO = int.Parse(element.Attribute("controlDO").Value);
            if (element.Attribute("adam") != null)
                io.Adam = element.Attribute("adam").Value;

            return io;
        }

        /// <summary>
        /// Parse Schedule Element
        /// </summary>
        private ScheduleConfig ParseSchedule(XElement element)
        {
            var schedule = new ScheduleConfig();

            if (element.Attribute("enabled") != null)
                schedule.Enabled = bool.Parse(element.Attribute("enabled").Value);

            // 檢查是否有 TimeRange 子元素（新格式）
            var timeRangeElements = element.Elements("TimeRange").ToList();
            if (timeRangeElements.Count > 0)
            {
                foreach (var rangeElement in timeRangeElements)
                {
                    var range = new ScheduleTimeRange();
                    if (rangeElement.Attribute("start") != null)
                        range.StartTime = TimeSpan.Parse(rangeElement.Attribute("start").Value);
                    if (rangeElement.Attribute("end") != null)
                        range.EndTime = TimeSpan.Parse(rangeElement.Attribute("end").Value);

                    // 解析每個 TimeRange 的 days 屬性
                    if (rangeElement.Attribute("days") != null)
                    {
                        string daysStr = rangeElement.Attribute("days").Value;
                        if (!string.IsNullOrEmpty(daysStr))
                        {
                            var dayNums = daysStr.Split(',');
                            foreach (var dayNum in dayNums)
                            {
                                if (int.TryParse(dayNum.Trim(), out int d))
                                {
                                    range.Days.Add((DayOfWeek)d);
                                }
                            }
                        }
                    }

                    schedule.TimeRanges.Add(range);
                }
            }
            else
            {
                // 舊格式相容：直接從 Schedule 元素讀取單一時間
                if (element.Attribute("start") != null && element.Attribute("end") != null)
                {
                    var range = new ScheduleTimeRange
                    {
                        StartTime = TimeSpan.Parse(element.Attribute("start").Value),
                        EndTime = TimeSpan.Parse(element.Attribute("end").Value)
                    };

                    // 解析舊格式的 days 屬性
                    if (element.Attribute("days") != null)
                    {
                        string daysStr = element.Attribute("days").Value;
                        if (!string.IsNullOrEmpty(daysStr))
                        {
                            var dayNums = daysStr.Split(',');
                            foreach (var dayNum in dayNums)
                            {
                                if (int.TryParse(dayNum.Trim(), out int d))
                                {
                                    range.Days.Add((DayOfWeek)d);
                                }
                            }
                        }
                    }

                    schedule.TimeRanges.Add(range);
                }
            }

            return schedule;
        }

        /// <summary>
        /// Parse AlarmLimits Element
        /// </summary>
        private AlarmLimitsConfig ParseAlarmLimits(XElement element)
        {
            var limits = new AlarmLimitsConfig();

            string pressureUpperStr = element.Attribute("pressureUpper")?.Value;
            if (!string.IsNullOrEmpty(pressureUpperStr))
            {
                double val;
                if (double.TryParse(pressureUpperStr, out val))
                    limits.PressureUpperLimit = val;
            }

            string pressureLowerStr = element.Attribute("pressureLower")?.Value;
            if (!string.IsNullOrEmpty(pressureLowerStr))
            {
                double val;
                if (double.TryParse(pressureLowerStr, out val))
                    limits.PressureLowerLimit = val;
            }

            string tempUpperStr = element.Attribute("tempUpper")?.Value;
            if (!string.IsNullOrEmpty(tempUpperStr))
            {
                double val;
                if (double.TryParse(tempUpperStr, out val))
                    limits.TempUpperLimit = val;
            }

            string tempLowerStr = element.Attribute("tempLower")?.Value;
            if (!string.IsNullOrEmpty(tempLowerStr))
            {
                double val;
                if (double.TryParse(tempLowerStr, out val))
                    limits.TempLowerLimit = val;
            }

            return limits;
        }

        /// <summary>
        /// 儲存指定工廠的警報上下限到 config.xml
        /// </summary>
        public bool SaveAlarmLimits(int factoryId, AlarmLimitsConfig limits)
        {
            try
            {
                string configPath = Path.Combine(System.Windows.Forms.Application.StartupPath, ConfigFileName);
                XDocument doc = XDocument.Load(configPath);
                var factoryElement = doc.Root.Element("Factories")?.Elements("Factory")
                    .FirstOrDefault(f => f.Attribute("id")?.Value == factoryId.ToString());

                if (factoryElement == null) return false;

                var alarmElement = factoryElement.Element("AlarmLimits");
                if (alarmElement == null)
                {
                    alarmElement = new XElement("AlarmLimits");
                    factoryElement.Add(alarmElement);
                }

                alarmElement.SetAttributeValue("pressureUpper", limits.PressureUpperLimit == double.MaxValue ? "" : limits.PressureUpperLimit.ToString());
                alarmElement.SetAttributeValue("pressureLower", limits.PressureLowerLimit == double.MinValue ? "" : limits.PressureLowerLimit.ToString());
                alarmElement.SetAttributeValue("tempUpper", limits.TempUpperLimit == double.MaxValue ? "" : limits.TempUpperLimit.ToString());
                alarmElement.SetAttributeValue("tempLower", limits.TempLowerLimit == double.MinValue ? "" : limits.TempLowerLimit.ToString());

                doc.Save(configPath);

                // 更新記憶體中的設定
                var factory = GetFactoryById(factoryId);
                if (factory != null)
                {
                    factory.AlarmLimits = limits;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Save alarm limits failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get Factory By Id
        /// </summary>
        public FactoryConfig GetFactoryById(int id)
        {
            return Factories.FirstOrDefault(f => f.Id == id);
        }

        /// <summary>
        /// Get Factory By Name
        /// </summary>
        public FactoryConfig GetFactoryByName(string name)
        {
            return Factories.FirstOrDefault(f => f.Name == name);
        }

        #region Legacy Support

        /// <summary>
        /// Legacy method for reading old config format
        /// </summary>
        public void readtask(string SelectNode)
        {
            try
            {
                if (File.Exists(Path.Combine(System.Windows.Forms.Application.StartupPath, "config.xml")))
                {
                    XmlDocument xDL = new XmlDocument();
                    xDL.Load(Path.Combine(System.Windows.Forms.Application.StartupPath, "config.xml"));
                    XmlNodeList NodeList = xDL.SelectNodes(SelectNode);

                    foreach (XmlNode item_File in NodeList)
                    {
                        XmlNodeList items = item_File.ChildNodes;
                        for (int i = 0; i < items.Count; i++)
                        {
                            XmlNodeList item = items[i].ChildNodes;
                            if (items[i].Name == "Database")
                            {
                                mysql_on = GetNodeValue(item, "mysql_on");
                                IP = GetNodeValue(item, "IP");
                                DB = GetNodeValue(item, "DB");
                                USER = GetNodeValue(item, "USER");
                                Password = GetNodeValue(item, "Password");
                                machinery_factory_demand_table1 = GetNodeValue(item, "machinery_factory_demand_table1");
                                machinery_factory_devicebox_table1 = GetNodeValue(item, "machinery_factory_devicebox_table1");
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private string GetNodeValue(XmlNodeList nodes, string nodeName)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.Name == nodeName)
                    return node.InnerText;
            }
            return "";
        }

        /// <summary>
        /// Legacy method - now calls LoadConfig
        /// </summary>
        public bool LoadDeviceConfig()
        {
            return LoadConfig();
        }

        #endregion
    }
}
