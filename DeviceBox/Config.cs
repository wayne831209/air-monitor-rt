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
    /// Schedule Configuration
    /// </summary>
    public class ScheduleConfig
    {
        public bool Enabled { get; set; } = false;
        public TimeSpan StartTime { get; set; } = TimeSpan.Zero;
        public TimeSpan EndTime { get; set; } = TimeSpan.Zero;
        public List<DayOfWeek> Days { get; set; } = new List<DayOfWeek>();

        /// <summary>
        /// Check if current time is within schedule
        /// </summary>
        public bool IsInSchedule()
        {
            if (!Enabled) return false;

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;
            var currentDay = now.DayOfWeek;

            // Check if today is a scheduled day
            if (Days.Count > 0 && !Days.Contains(currentDay))
                return false;

            // Check time range
            if (StartTime <= EndTime)
                return currentTime >= StartTime && currentTime <= EndTime;
            else
                return currentTime >= StartTime || currentTime <= EndTime;
        }

        /// <summary>
        /// Get schedule display text
        /// </summary>
        public string GetDisplayText()
        {
            if (!Enabled) return "No Schedule";
            return StartTime.ToString(@"hh\:mm") + "-" + EndTime.ToString(@"hh\:mm");
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
    /// Factory Configuration
    /// </summary>
    public class FactoryConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ModbusIp { get; set; }
        public string ModbusPort { get; set; }
        public List<DeviceConfig> Devices { get; set; }

        public FactoryConfig()
        {
            Devices = new List<DeviceConfig>();
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

    class Config
    {
        // Database Settings
        public string IP;
        public string DB;
        public string USER;
        public string Password;
        public string mysql_on;
        public string machinery_factory_demand_table1;
        public string machinery_factory_hour_table1;
        public string machinery_factory_day_table1;
        public string machinery_factory_month_table1;
        public string machinery_factory_realtime_table1;

        public string machinery_factory_demand_table2;
        public string machinery_factory_hour_table2;
        public string machinery_factory_day_table2;
        public string machinery_factory_month_table2;
        public string machinery_factory_realtime_table2;

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
            machinery_factory_hour_table1 = dbElement.Element("machinery_factory_hour_table1")?.Value ?? "";
            machinery_factory_day_table1 = dbElement.Element("machinery_factory_day_table1")?.Value ?? "";
            machinery_factory_month_table1 = dbElement.Element("machinery_factory_month_table1")?.Value ?? "";
            machinery_factory_realtime_table1 = dbElement.Element("machinery_factory_realtime_table1")?.Value ?? "";
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
                    }
                }
            }

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
            if (element.Attribute("start") != null)
                schedule.StartTime = TimeSpan.Parse(element.Attribute("start").Value);
            if (element.Attribute("end") != null)
                schedule.EndTime = TimeSpan.Parse(element.Attribute("end").Value);

            return schedule;
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
                                machinery_factory_hour_table1 = GetNodeValue(item, "machinery_factory_hour_table1");
                                machinery_factory_day_table1 = GetNodeValue(item, "machinery_factory_day_table1");
                                machinery_factory_month_table1 = GetNodeValue(item, "machinery_factory_month_table1");
                                machinery_factory_realtime_table1 = GetNodeValue(item, "machinery_factory_realtime_table1");
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
