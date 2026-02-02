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

        private Timer updateTimer;
        private List<ModBus_List> modbusList;
        private Config config;

        public MainForm()
        {
            InitializeComponent();
            InitializeModbus();
            InitializeTimer();
        }

        private void InitializeModbus()
        {
            config = new Config();
            modbusList = new List<ModBus_List>();

            try
            {
                config.readtask("Setting");
                for (int i = 0; i < config.ModBus_IP.Count; i++)
                {
                    modbusList.Add(new ModBus_List(config.ModBus_IP[i], config.ModBus_Port[i], config.ModBus_Name[i]));
                }
            }
            catch (Exception ex)
            {
                // 設定檔讀取失敗時的處理
                System.Diagnostics.Debug.WriteLine("Modbus初始化失敗: " + ex.Message);
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
                // 更新各廠區狀態 (假設最多5個廠區對應 label16-50)
                for (int factoryIndex = 0; factoryIndex < Math.Min(modbusList.Count, 5); factoryIndex++)
                {
                    var modbus = modbusList[factoryIndex];
                    if (modbus.address_val == null)
                        continue;

                    // 解析設備狀態
                    var compressorStatus = GetCompressorStatus(modbus);
                    var precoolerStatus = GetPrecoolerStatus(modbus);
                    var dryerStatus = GetDryerStatus(modbus);
                    var fanStatus = GetFanStatus(modbus);
                    var readyStatus = GetReadyStatus(modbus);
                    var pressure = GetPressureValue(modbus);

                    // 更新對應的Label (根據廠區索引)
                    UpdateFactoryLabels(factoryIndex, compressorStatus, precoolerStatus, dryerStatus, fanStatus, pressure);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("更新狀態失敗: " + ex.Message);
            }
        }

        /// <summary>
        /// 取得空壓機狀態
        /// DI7=運轉, DI8=警報, DI9=故障, 均無=停止
        /// </summary>
        private DeviceStatus GetCompressorStatus(ModBus_List modbus)
        {
            bool di7 = modbus.address_val.Address_4051_DI_7 == "1"; // 運轉
            bool di8 = modbus.address_val.Address_4051_DI_8 == "1"; // 警報
            bool di9 = modbus.address_val.Address_4051_DI_9 == "1"; // 故障

            if (di9) return new DeviceStatus("故障", StatusFault);
            if (di8) return new DeviceStatus("警報", StatusAlarm);
            if (di7) return new DeviceStatus("運轉", StatusRunning);
            return new DeviceStatus("停止", StatusStopped);
        }

        /// <summary>
        /// 取得預冷散熱器狀態
        /// DI1=ON, DI2=OFF, DI2+DI0=故障
        /// </summary>
        private DeviceStatus GetPrecoolerStatus(ModBus_List modbus)
        {
            bool di0 = modbus.address_val.Address_4051_DI_0 == "1";
            bool di1 = modbus.address_val.Address_4051_DI_1 == "1"; // ON
            bool di2 = modbus.address_val.Address_4051_DI_2 == "1"; // OFF

            if (di2 && di0) return new DeviceStatus("故障", StatusFault);
            if (di1) return new DeviceStatus("啟動", StatusRunning);
            if (di2) return new DeviceStatus("停止", StatusStopped);
            return new DeviceStatus("--", StatusStopped);
        }

        /// <summary>
        /// 取得冷凍式乾燥機狀態
        /// DI3=ON, DI4=OFF, DI4+DI0=故障
        /// </summary>
        private DeviceStatus GetDryerStatus(ModBus_List modbus)
        {
            bool di0 = modbus.address_val.Address_4051_DI_0 == "1";
            bool di3 = modbus.address_val.Address_4051_DI_3 == "1"; // ON
            bool di4 = modbus.address_val.Address_4051_DI_4 == "1"; // OFF

            if (di4 && di0) return new DeviceStatus("故障", StatusFault);
            if (di3) return new DeviceStatus("啟動", StatusRunning);
            if (di4) return new DeviceStatus("停止", StatusStopped);
            return new DeviceStatus("--", StatusStopped);
        }

        /// <summary>
        /// 取得機房抽風機狀態
        /// DI5=ON, DI6=OFF, DI6+DI0=故障
        /// </summary>
        private DeviceStatus GetFanStatus(ModBus_List modbus)
        {
            bool di0 = modbus.address_val.Address_4051_DI_0 == "1";
            bool di5 = modbus.address_val.Address_4051_DI_5 == "1"; // ON
            bool di6 = modbus.address_val.Address_4051_DI_6 == "1"; // OFF

            if (di6 && di0) return new DeviceStatus("故障", StatusFault);
            if (di5) return new DeviceStatus("啟動", StatusRunning);
            if (di6) return new DeviceStatus("停止", StatusStopped);
            return new DeviceStatus("--", StatusStopped);
        }

        /// <summary>
        /// 取得設備備妥狀態 (ADAM-4050)
        /// DI2=備妥, 無DI2=未備妥
        /// </summary>
        private DeviceStatus GetReadyStatus(ModBus_List modbus)
        {
            bool di2 = modbus.address_val.Address_4050_DI_2 == "1";

            if (di2) return new DeviceStatus("備妥", StatusReady);
            return new DeviceStatus("未備妥", StatusNotReady);
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
        private void UpdateFactoryLabels(int factoryIndex, DeviceStatus compressor, DeviceStatus precooler, 
            DeviceStatus dryer, DeviceStatus fan, string pressure)
        {
            // 根據廠區索引找到對應的Label
            // 空壓設備: label16-20 (第一行), label26-30 (第二行), label31-35 (第三行)
            // 預冷散熱器: label31-35
            // 冷凍式乾燥機: label36-40
            // 機房風扇: label41-45
            // 空壓壓力: label46-50

            Label[] compressorLabels = { label20, label19, label18, label17, label16 };
            Label[] compressor2Labels = { label30, label29, label28, label27, label26 };
            Label[] precoolerLabels = { label35, label34, label33, label32, label31 };
            Label[] dryerLabels = { label40, label39, label38, label37, label36 };
            Label[] fanLabels = { label45, label44, label43, label42, label41 };
            Label[] pressureLabels = { label50, label49, label48, label47, label46 };

            if (factoryIndex < compressorLabels.Length)
            {
                // 更新空壓設備狀態 (第一行)
                UpdateLabel(compressorLabels[factoryIndex], compressor.Text, compressor.Color);
                // 更新空壓設備狀態 (第二行 - 可顯示額外資訊如備妥狀態)
                var readyStatus = GetReadyStatus(modbusList[factoryIndex]);
                UpdateLabel(compressor2Labels[factoryIndex], readyStatus.Text, readyStatus.Color);
                // 更新預冷散熱器
                UpdateLabel(precoolerLabels[factoryIndex], precooler.Text, precooler.Color);
                // 更新冷凍式乾燥機
                UpdateLabel(dryerLabels[factoryIndex], dryer.Text, dryer.Color);
                // 更新機房風扇
                UpdateLabel(fanLabels[factoryIndex], fan.Text, fan.Color);
                // 更新空壓壓力
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
