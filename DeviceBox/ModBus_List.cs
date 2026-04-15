using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Modbus.Device;
using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel;
using System.IO.Ports;

namespace DeviceBox
{
    public class ModBus_List:Address
    {
        [DllImport("WININET", CharSet = CharSet.Auto)]
        static extern bool InternetGetConnectedState(ref InternetConnectionState lpdwFlags, int dwReserved);
        BackgroundWorker backgroundWorker_ECU;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        ModbusIpMaster master_tcp;
        ModbusSerialMaster master_rtu;
        public Address address_val;
        byte slaveAddress;
        int meter_select, contract, mode;

        public bool ConnectState = false;
        public volatile bool DataReady = false;
        public string[] Address_Val = new string[36];
        public string[] Address_Str = new string[36];
        public string name,ip,port;
        public string timer_bill,timer_season;
        public bool trigger_demand = true;
        public bool trigger_hour = true;
        public bool trigger_day = true;
        public bool trigger_month = true;
        public int sql_table;
        public string factory = ""; //廠別名稱

        public int season = 0;
        public string bill_status = ""; 
        double Esum = 0, Qsum = 0;


        public ModBus_List(string IP, string Port, string name)
        {
            address_val = new Address();
            this.name = name;
            this.ip = IP;
            this.port = Port;

            Address_Str[0] = this.name + "_" + address_val.Address_4051_DI_0;
            Address_Str[1] = this.name + "_" + address_val.Address_4051_DI_1;
            Address_Str[2] = this.name + "_" + address_val.Address_4051_DI_2;
            Address_Str[3] = this.name + "_" + address_val.Address_4051_DI_3;
            Address_Str[4] = this.name + "_" + address_val.Address_4051_DI_4;
            Address_Str[5] = this.name + "_" + address_val.Address_4051_DI_5;
            Address_Str[4] = this.name + "_" + address_val.Address_4051_DI_6;
            Address_Str[7] = this.name + "_" + address_val.Address_4051_DI_7;
            Address_Str[8] = this.name + "_" + address_val.Address_4051_DI_8;
            Address_Str[9] = this.name + "_" + address_val.Address_4051_DI_9;
            Address_Str[10] = this.name + "_" + address_val.Address_4051_DI_10;
            Address_Str[11] = this.name + "_" + address_val.Address_4051_DI_11;
            Address_Str[12] = this.name + "_" + address_val.Address_4051_DI_12;
            Address_Str[13] = this.name + "_" + address_val.Address_4051_DI_13;
            Address_Str[14] = this.name + "_" + address_val.Address_4051_DI_14;
            Address_Str[15] = this.name + "_" + address_val.Address_4051_DI_15;
            Address_Str[16] = this.name + "_" + address_val.Address_4050_DI_0;
            Address_Str[17] = this.name + "_" + address_val.Address_4050_DI_1;
            Address_Str[18] = this.name + "_" + address_val.Address_4050_DI_2;
            Address_Str[19] = this.name + "_" + address_val.Address_4050_DI_3;
            Address_Str[20] = this.name + "_" + address_val.Address_4050_DI_4;
            Address_Str[21] = this.name + "_" + address_val.Address_4050_DI_5;
            Address_Str[22] = this.name + "_" + address_val.Address_4050_DI_6;
            Address_Str[23] = this.name + "_" + address_val.Address_4050_DI_7;
            Address_Str[24] = this.name + "_" + address_val.Address_4050_DO_0;
            Address_Str[25] = this.name + "_" + address_val.Address_4050_DO_1;
            Address_Str[26] = this.name + "_" + address_val.Address_4050_DO_2;
            Address_Str[27] = this.name + "_" + address_val.Address_4050_DO_3;
            Address_Str[28] = this.name + "_" + address_val.Address_4050_DO_4;
            Address_Str[29] = this.name + "_" + address_val.Address_4050_DO_5;
            Address_Str[30] = this.name + "_" + address_val.Address_4050_DO_6;
            Address_Str[31] = this.name + "_" + address_val.Address_4050_DO_7;
            Address_Str[32] = this.name + "_" + address_val.Address_Air_Sensor_Pressure_Value;
            Address_Str[33] = this.name + "_" + address_val.Address_Air_Sensor_Decimal;
            Address_Str[34] = this.name + "_" + address_val.Address_E5CC_1_PV;
            Address_Str[35] = this.name + "_" + address_val.Address_E5CC_1_SV;


            this.backgroundWorker_ECU = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker_ECU.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork_ECU);
            TCP_Connect(IP);
            backgroundWorker_ECU.RunWorkerAsync();
        }

        private async void backgroundWorker_DoWork_ECU(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    ushort[] holding_register = null;
                    if (ConnectState)
                    {
                        holding_register = master_tcp.ReadHoldingRegisters(1, 0, 50);

                        address_val.Address_4051_DI_0 = holding_register[3].ToString();
                        address_val.Address_4051_DI_1 = holding_register[4].ToString();
                        address_val.Address_4051_DI_2 = holding_register[5].ToString();
                        address_val.Address_4051_DI_3 = holding_register[6].ToString();
                        address_val.Address_4051_DI_4 = holding_register[7].ToString();
                        address_val.Address_4051_DI_5 = holding_register[8].ToString();
                        address_val.Address_4051_DI_6 = holding_register[9].ToString();
                        address_val.Address_4051_DI_7 = holding_register[10].ToString();
                        address_val.Address_4051_DI_8 = holding_register[11].ToString();
                        address_val.Address_4051_DI_9 = holding_register[12].ToString();
                        address_val.Address_4051_DI_10 = holding_register[13].ToString();
                        address_val.Address_4051_DI_11 = holding_register[14].ToString();
                        address_val.Address_4051_DI_12 = holding_register[15].ToString();
                        address_val.Address_4051_DI_13 = holding_register[16].ToString();
                        address_val.Address_4051_DI_14 = holding_register[17].ToString();
                        address_val.Address_4051_DI_15 = holding_register[18].ToString();
                        address_val.Address_4050_DI_0 = holding_register[22].ToString();
                        address_val.Address_4050_DI_1 = holding_register[23].ToString();
                        address_val.Address_4050_DI_2 = holding_register[24].ToString();
                        address_val.Address_4050_DI_3 = holding_register[25].ToString();
                        address_val.Address_4050_DI_4 = holding_register[26].ToString();
                        address_val.Address_4050_DI_5 = holding_register[27].ToString();
                        address_val.Address_4050_DI_6 = holding_register[28].ToString();
                        address_val.Address_4050_DI_7 = holding_register[29].ToString();
                        address_val.Address_4050_DO_0 = holding_register[30].ToString();
                        address_val.Address_4050_DO_1 = holding_register[31].ToString();
                        address_val.Address_4050_DO_2 = holding_register[32].ToString();
                        address_val.Address_4050_DO_3 = holding_register[33].ToString();
                        address_val.Address_4050_DO_4 = holding_register[34].ToString();
                        address_val.Address_4050_DO_5 = holding_register[35].ToString();
                        address_val.Address_4050_DO_6 = holding_register[36].ToString();
                        address_val.Address_4050_DO_7 = holding_register[37].ToString();
                        address_val.Address_E5CC_1_PV = holding_register[48].ToString();
                        address_val.Address_E5CC_1_SV = holding_register[49].ToString();


                        holding_register = master_tcp.ReadHoldingRegisters(1, 2000, 10);
                        address_val.Address_Air_Sensor_Pressure_Value = holding_register[4].ToString();
                        address_val.Address_Air_Sensor_Decimal = holding_register[6].ToString();

                        Address_Val[0] = address_val.Address_4051_DI_0;
                        Address_Val[1] = address_val.Address_4051_DI_1;
                        Address_Val[2] = address_val.Address_4051_DI_2;
                        Address_Val[3] = address_val.Address_4051_DI_3;
                        Address_Val[4] = address_val.Address_4051_DI_4;
                        Address_Val[5] = address_val.Address_4051_DI_5;
                        Address_Val[6] = address_val.Address_4051_DI_6;
                        Address_Val[7] = address_val.Address_4051_DI_7;
                        Address_Val[8] = address_val.Address_4051_DI_8;
                        Address_Val[9] = address_val.Address_4051_DI_9;
                        Address_Val[10] = address_val.Address_4051_DI_10;
                        Address_Val[11] = address_val.Address_4051_DI_11;
                        Address_Val[12] = address_val.Address_4051_DI_12;
                        Address_Val[13] = address_val.Address_4051_DI_13;
                        Address_Val[14] = address_val.Address_4051_DI_14;
                        Address_Val[15] = address_val.Address_4051_DI_15;
                        Address_Val[16] = address_val.Address_4050_DI_0;
                        Address_Val[17] = address_val.Address_4050_DI_1;
                        Address_Val[18] = address_val.Address_4050_DI_2;
                        Address_Val[19] = address_val.Address_4050_DI_3;
                        Address_Val[20] = address_val.Address_4050_DI_4;
                        Address_Val[21] = address_val.Address_4050_DI_5;
                        Address_Val[22] = address_val.Address_4050_DI_6;
                        Address_Val[23] = address_val.Address_4050_DI_7;
                        Address_Val[24] = address_val.Address_4050_DO_0;
                        Address_Val[25] = address_val.Address_4050_DO_1;
                        Address_Val[26] = address_val.Address_4050_DO_2;
                        Address_Val[27] = address_val.Address_4050_DO_3;
                        Address_Val[28] = address_val.Address_4050_DO_4;
                        Address_Val[29] = address_val.Address_4050_DO_5;
                        Address_Val[30] = address_val.Address_4050_DO_6;
                        Address_Val[31] = address_val.Address_4050_DO_7;
                        Address_Val[32] = address_val.Address_Air_Sensor_Pressure_Value;
                        Address_Val[33] = address_val.Address_Air_Sensor_Decimal;
                        Address_Val[34] = address_val.Address_E5CC_1_PV;
                        Address_Val[35] = address_val.Address_E5CC_1_SV;

                        DataReady = true;
                        ConnectState = true;
                    }
                    else
                    {
                        ConnectState = false;
                        DataReady = false;
                        for (int i = 0; i < Address_Val.Count(); i++)
                        {
                            Address_Val[i] = "0";
                        }
                        TCP_Connect(this.ip);
                    }
                }
                catch
                {
                    ConnectState = false;
                    DataReady = false;
                    for (int i = 0; i < Address_Val.Count(); i++)
                    {
                        Address_Val[i] = "0";
                    }
                    //address_val.Address_KVAH_TOTAL = "0";
                    TCP_Connect(this.ip);
                }

                await Task.Delay(1000);
            }
        }

        public void RTU_Connect(string Port)
        {
            SerialPort port = new SerialPort();
            port.PortName = Port;
            port.BaudRate = 9600;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.Parity = Parity.None;
            if(!port.IsOpen)
                port.Open();
            master_rtu = ModbusSerialMaster.CreateRtu(port);
            master_rtu.Transport.ReadTimeout = 300;
        }
        /// <summary>
        /// 寫入 DO 值 (控制輸出)
        /// doNumber: DO 通道編號 (0~7 對應 Address_4050_DO_0 ~ DO_7)
        /// value: 1=啟動, 0=停止
        /// </summary>
        public bool WriteDO(int doNumber, ushort value)
        {
            if (!ConnectState || master_tcp == null) return false;
            if (doNumber < 0 || doNumber > 7) return false;

            try
            {
                // DO_0 對應 holding register 30, DO_1=31, DO_2=32 ...
                ushort registerAddress = (ushort)(30 + doNumber);
                //master_tcp.WriteSingleRegister(1, registerAddress, value);
                System.Diagnostics.Debug.WriteLine($"[{name}] WriteDO: DO_{doNumber} (register {registerAddress}) = {value}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{name}] WriteDO failed: {ex.Message}");
                return false;
            }
        }

        public bool TCP_Connect(string IP)
        {
            if (CheckInternet())
            {
                try
                {
                    TcpClient tcpClient = new TcpClient();
                    IAsyncResult asyncResult = tcpClient.BeginConnect(IP, 502, null, null);
                    asyncResult.AsyncWaitHandle.WaitOne(3000, true); //wait for 3 sec
                    if (!asyncResult.IsCompleted)
                    {
                        ConnectState = false;
                        tcpClient.Close();
                        return false;
                    }
                    master_tcp = ModbusIpMaster.CreateIp(tcpClient);
                    master_tcp.Transport.Retries = 5;   //don't have to do retries
                    master_tcp.Transport.ReadTimeout = 1500;
                    ConnectState = true;

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }
        enum InternetConnectionState : int
        {
            INTERNET_CONNECTION_MODEM = 0x1,
            INTERNET_CONNECTION_LAN = 0x2,
            INTERNET_CONNECTION_PROXY = 0x4,
            INTERNET_RAS_INSTALLED = 0x10,
            INTERNET_CONNECTION_OFFLINE = 0x20,
            INTERNET_CONNECTION_CONFIGURED = 0x40
        }
        private bool CheckInternet()
        {
            //http://msdn.microsoft.com/en-us/library/windows/desktop/aa384702(v=vs.85).aspx
            InternetConnectionState flag = InternetConnectionState.INTERNET_CONNECTION_LAN;
            return InternetGetConnectedState(ref flag, 0);
        }
    }
}
