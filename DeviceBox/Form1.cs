using MySQL;
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
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace DeviceBox
{
    public partial class Form1 : Form
    {
        private List<ModBus_List> modbusList;
        private Config config;
        bool Trigger = true;
        MYSQL mysql;

        public Form1()
        {
            InitializeComponent();
            InitializeConfig();
            InitializeModbus();
            mysql = new MYSQL(config.IP, config.DB, config.USER, config.Password);

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

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (var factory in config.Factories)
            {
                var compressors = factory.GetDevicesByType(DeviceType.Compressor);
                foreach (var compressor in compressors)
                {
                    comboBox1.Items.Add(compressor.Name);
                }
            }
            comboBox1.SelectedIndex = 0;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(backgroundWorker1.IsBusy==false)
            {
                int timerInterval;
                if (!int.TryParse(TimerSet.Text, out timerInterval) || timerInterval <= 0)
                    return;
                backgroundWorker1.RunWorkerAsync(timerInterval);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int timerInterval = (int)e.Argument;

            if (DateTime.Now.Minute % timerInterval == 0 && Trigger == true)
            {
                string Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string cmd = "";
                int itemIndex = 0;
                for (int factoryIdx = 0; factoryIdx < config.Factories.Count; factoryIdx++)
                {
                    if (!modbusList[factoryIdx].DataReady)
                        continue;

                    var factory = config.Factories[factoryIdx];
                    var compressors = factory.GetDevicesByType(DeviceType.Compressor);
                    foreach (var compressor in compressors)
                    {
                        if (itemIndex > 0)
                            cmd += ",";

                        double pressureValue, decimalValue;
                        if (!double.TryParse(modbusList[factoryIdx].address_val.Address_Air_Sensor_Pressure_Value, out pressureValue))
                            pressureValue = 0;
                        if (!double.TryParse(modbusList[factoryIdx].address_val.Address_Air_Sensor_Decimal, out decimalValue))
                            decimalValue = 1;

                        string airPressure = (pressureValue / Math.Pow(10, decimalValue)).ToString("F4");
                        string tempPV = modbusList[factoryIdx].address_val.Address_E5CC_1_PV.ToString();
                        string tempSV = modbusList[factoryIdx].address_val.Address_E5CC_1_SV.ToString();

                        cmd += "('" + compressor.Name + "','" + Time + "','" + airPressure + "','" + tempPV + "','" + tempSV + "')";
                        itemIndex++;
                    }
                }
                if (!string.IsNullOrEmpty(cmd))
                {
                    mysql.insertdata("INSERT INTO " + config.machinery_factory_devicebox_table1 +
                                         "(`Name`,`Time`,`CompressedAir`,`AmbientTempPV`,`AmbientTempSV`)" +
                                         "VALUES" + cmd + "");
                }
                Trigger = false;
            }
            else if (DateTime.Now.Minute % timerInterval == 1)
            {
                Trigger = true;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                txtDI4051_0.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_0.ToString();
                txtDI4051_1.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_1.ToString();
                txtDI4051_2.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_2.ToString();
                txtDI4051_3.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_3.ToString();
                txtDI4051_4.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_4.ToString();
                txtDI4051_5.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_5.ToString();
                txtDI4051_6.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_6.ToString();
                txtDI4051_7.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_7.ToString();
                txtDI4051_8.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_8.ToString();
                txtDI4051_9.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_9.ToString();
                txtDI4051_10.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_10.ToString();
                txtDI4051_11.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_11.ToString();
                txtDI4051_12.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_12.ToString();
                txtDI4051_13.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_13.ToString();
                txtDI4051_14.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_14.ToString();
                txtDI4051_15.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4051_DI_15.ToString();

                txtDI4050_0.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DI_0.ToString();
                txtDI4050_1.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DI_1.ToString();
                txtDI4050_2.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DI_2.ToString();
                txtDI4050_3.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DI_3.ToString();
                txtDI4050_4.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DI_4.ToString();
                txtDI4050_5.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DI_5.ToString();
                txtDI4050_6.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DI_6.ToString();
                txtDI4050_7.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DI_7.ToString();
                txtDO4050_0.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DO_0.ToString();
                txtDO4050_1.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DO_1.ToString();
                txtDO4050_2.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DO_2.ToString();
                txtDO4050_3.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DO_3.ToString();
                txtDO4050_4.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DO_4.ToString();
                txtDO4050_5.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DO_5.ToString();
                txtDO4050_6.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DO_6.ToString();
                txtDO4050_7.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_4050_DO_7.ToString();

                double pressureVal, decimalVal;
                if (double.TryParse(modbusList[comboBox1.SelectedIndex].address_val.Address_Air_Sensor_Pressure_Value, out pressureVal)
                    && double.TryParse(modbusList[comboBox1.SelectedIndex].address_val.Address_Air_Sensor_Decimal, out decimalVal))
                {
                    AirPresure.Text = (pressureVal / Math.Pow(10, decimalVal)).ToString("F4");
                }
                else
                {
                    AirPresure.Text = "0.0000";
                }
                TempPV.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_E5CC_1_PV.ToString();
                TempSV.Text = modbusList[comboBox1.SelectedIndex].address_val.Address_E5CC_1_SV.ToString();
            }catch
            {

            }
        }
    }
}
