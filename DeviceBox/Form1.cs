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
    public partial class Form1 : Form
    {
        public static List<ModBus_List> modbus_List = new List<ModBus_List>();
        Config config = new Config();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            config.readtask("Setting");
            for (int i = 0; i < config.ModBus_IP.Count; i++)
            {
                modbus_List.Add(new ModBus_List(config.ModBus_IP[i], config.ModBus_Port[i], config.ModBus_Name[i]));
                comboBox1.Items.Add(config.ModBus_Name[i]);
            }
            comboBox1.SelectedIndex = 0;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(backgroundWorker1.IsBusy==false)
            {
                backgroundWorker1.RunWorkerAsync();
            }   
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                txtDI4051_0.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_0.ToString();
                txtDI4051_1.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_1.ToString();
                txtDI4051_2.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_2.ToString();
                txtDI4051_3.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_3.ToString();
                txtDI4051_4.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_4.ToString();
                txtDI4051_5.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_5.ToString();
                txtDI4051_6.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_6.ToString();
                txtDI4051_7.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_7.ToString();
                txtDI4051_8.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_8.ToString();
                txtDI4051_9.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_9.ToString();
                txtDI4051_10.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_10.ToString();
                txtDI4051_11.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_11.ToString();
                txtDI4051_12.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_12.ToString();
                txtDI4051_13.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_13.ToString();
                txtDI4051_14.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_14.ToString();
                txtDI4051_15.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4051_DI_15.ToString();

                txtDI4050_0.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DI_0.ToString();
                txtDI4050_1.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DI_1.ToString();
                txtDI4050_2.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DI_2.ToString();
                txtDI4050_3.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DI_3.ToString();
                txtDI4050_4.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DI_4.ToString();
                txtDI4050_5.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DI_5.ToString();
                txtDI4050_6.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DI_6.ToString();
                txtDI4050_7.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DI_7.ToString();
                txtDO4050_0.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DO_0.ToString();
                txtDO4050_1.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DO_1.ToString();
                txtDO4050_2.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DO_2.ToString();
                txtDO4050_3.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DO_3.ToString();
                txtDO4050_4.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DO_4.ToString();
                txtDO4050_5.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DO_5.ToString();
                txtDO4050_6.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DO_6.ToString();
                txtDO4050_7.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_4050_DO_7.ToString();

                AirPresure.Text = (Convert.ToDouble(modbus_List[comboBox1.SelectedIndex].address_val.Address_Air_Sensor_Pressure_Value.ToString()) / Math.Pow(10, Convert.ToDouble(modbus_List[comboBox1.SelectedIndex].address_val.Address_Air_Sensor_Decimal.ToString()))).ToString("F4");
                TempPV.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_E5CC_1_PV.ToString();
                TempSV.Text = modbus_List[comboBox1.SelectedIndex].address_val.Address_E5CC_1_SV.ToString();
            }catch
            {

            }
        }
    }
}
