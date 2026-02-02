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
        // 工業風格配色常數
        private static readonly Color StatusRunning = Color.FromArgb(0, 200, 0);      // 綠色 - 運轉/啟動
        private static readonly Color StatusStopped = Color.FromArgb(128, 128, 128);  // 灰色 - 停止/關機
        private static readonly Color StatusAlarm = Color.FromArgb(255, 140, 0);      // 橘色 - 警報
        private static readonly Color StatusFault = Color.FromArgb(220, 50, 50);      // 紅色 - 故障
        private static readonly Color StatusReady = Color.FromArgb(0, 180, 255);      // 藍色 - 備妥
        private static readonly Color StatusNotReady = Color.FromArgb(100, 100, 100); // 深灰 - 未備妥
        private static readonly Color StatusDisabled = Color.FromArgb(60, 60, 60);    // 深灰 - 未啟用

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
        /// 初始化 Modbus 連線
        /// </summary>
        private void InitializeModbus()
        {
            modbusList = new List<ModBus_List>();

            try
            {
                // 從設備配置檔建立 Modbus 連線
                foreach (var factory in config.Factories)
                {
                    modbusList.Add(new ModBus_List(factory.ModbusIp, factory.ModbusPort, factory.Name));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Modbus初始化失敗: " + ex.Message);
            }
        }

        /// <summary>
        /// 初始化場域標題
        /// </summary>
        private void InitializeFactoryHeaders()
        {
            // 場域標題 Labels
            Label[] factoryHeaders = { label6, label7, label8, label9, label10 };

            for (int i = 0; i < factoryHeaders.Length; i++)
            {
                if (i < config.Factories.Count)
                {
                    factoryHeaders[i].Text = config.Factories[i].Name;
                }
                else
                {
                    factoryHeaders[i].Text = "--";
                }
            }
        }

        private void InitializeTimer()
        {
            updateTimer = new Timer();
            updateTimer.Interval = 1000; // 每秒更新一次
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (modbusList == null || modbusList.Count == 0)
                return;

            try
            {
                // 更新各廠區狀態
                for (int factoryIndex = 0; factoryIndex < Math.Min(modbusList.Count, 5); factoryIndex++)
                {
                    var modbus = modbusList[factoryIndex];
                    if (modbus.address_val == null)
                        continue;

                    var factoryConfig = config.Factories[factoryIndex];

                    // 根據配置解析設備狀態
                    var compressor1Status = GetCompressorStatusByConfig(modbus, factoryConfig, 1);
                    var compressor2Status = GetCompressorStatusByConfig(modbus, factoryConfig, 2);
                    var compressor3Status = GetCompressorStatusByConfig(modbus, factoryConfig, 3);
                    var precoolerStatus = GetDeviceStatusByConfig(modbus, factoryConfig, DeviceType.Precooler);
                    var dryerStatus = GetDeviceStatusByConfig(modbus, factoryConfig, DeviceType.Dryer);
                    var fanStatus = GetDeviceStatusByConfig(modbus, factoryConfig, DeviceType.Fan);
                    var pressure = GetPressureValue(modbus);

                    // 更新對應的Label
                    UpdateFactoryLabels(factoryIndex, compressor1Status, compressor2Status, compressor3Status,
                        precoolerStatus, dryerStatus, fanStatus, pressure);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("更新狀態失敗: " + ex.Message);
            }
        }

        /// <summary>
        /// 根據配置取得空壓機狀態
        /// </summary>
        private DeviceStatus GetCompressorStatusByConfig(ModBus_List modbus, FactoryConfig factory, int machineNo)
        {
            var device = factory.GetDevice(DeviceType.Compressor, machineNo);
            
            if (device == null || !device.Enabled)
            {
                return new DeviceStatus($"{machineNo}號機 --", StatusDisabled);
            }

            bool isRunning = GetDIValue(modbus, device.IO.RunDI);
            bool isAlarm = GetDIValue(modbus, device.IO.AlarmDI);
            bool isFault = GetDIValue(modbus, device.IO.FaultDI);

            string machineName = $"{machineNo}號機";

            if (isFault) return new DeviceStatus($"{machineName} 故障", StatusFault);
            if (isAlarm) return new DeviceStatus($"{machineName} 警報", StatusAlarm);
            if (isRunning) return new DeviceStatus($"{machineName} 運轉", StatusRunning);
            return new DeviceStatus($"{machineName} 停止", StatusStopped);
        }

        /// <summary>
        /// 根據配置取得一般設備狀態 (預冷散熱器、冷凍式乾燥機、機房風扇)
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

            // 故障判斷: OFF + Fault DI 同時為1
            if (isOff && isFault) return new DeviceStatus("故障", StatusFault);
            if (isOn) return new DeviceStatus("啟動", StatusRunning);
            if (isOff) return new DeviceStatus("停止", StatusStopped);
            return new DeviceStatus("--", StatusStopped);
        }

        /// <summary>
        /// 根據 DI 編號取得對應的值
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
        /// 取得空壓壓力值
        /// </summary>
        private string GetPressureValue(ModBus_List modbus)
        {
            try
            {
                double pressureValue = Convert.ToDouble(modbus.address_val.Address_Air_Sensor_Pressure_Value);
                double decimalPlaces = Convert.ToDouble(modbus.address_val.Address_Air_Sensor_Decimal);
                double pressure = pressureValue / Math.Pow(10, decimalPlaces);
                return pressure.ToString("F2") + " kgf/cm²";
            }
            catch
            {
                return "-- kgf/cm²";
            }
        }

        /// <summary>
        /// 更新對應廠區的Label顯示
        /// </summary>
        private void UpdateFactoryLabels(int factoryIndex, DeviceStatus compressor1, DeviceStatus compressor2, 
            DeviceStatus compressor3, DeviceStatus precooler, DeviceStatus dryer, DeviceStatus fan, string pressure)
        {
            // 1號機狀態 (第一行)
            Label[] compressor1Labels = { label20, label19, label18, label17, label16 };
            // 2號機狀態 (第二行)
            Label[] compressor2Labels = { label25, label24, label23, label22, label21 };
            // 3號機狀態 (第三行)
            Label[] compressor3Labels = { label30, label29, label28, label27, label26 };
            // 預冷散熱器
            Label[] precoolerLabels = { label35, label34, label33, label32, label31 };
            // 冷凍式乾燥機
            Label[] dryerLabels = { label40, label39, label38, label37, label36 };
            // 機房風扇
            Label[] fanLabels = { label45, label44, label43, label42, label41 };
            // 空壓壓力
            Label[] pressureLabels = { label50, label49, label48, label47, label46 };

            if (factoryIndex < compressor1Labels.Length)
            {
                UpdateLabel(compressor1Labels[factoryIndex], compressor1.Text, compressor1.Color);
                UpdateLabel(compressor2Labels[factoryIndex], compressor2.Text, compressor2.Color);
                UpdateLabel(compressor3Labels[factoryIndex], compressor3.Text, compressor3.Color);
                UpdateLabel(precoolerLabels[factoryIndex], precooler.Text, precooler.Color);
                UpdateLabel(dryerLabels[factoryIndex], dryer.Text, dryer.Color);
                UpdateLabel(fanLabels[factoryIndex], fan.Text, fan.Color);
                UpdateLabel(pressureLabels[factoryIndex], pressure, StatusRunning);
            }
        }

        /// <summary>
        /// 更新Label的文字和顏色
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            updateTimer?.Stop();
            updateTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }

    /// <summary>
    /// 設備狀態結構
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
}
