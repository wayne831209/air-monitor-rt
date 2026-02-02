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
    class Config
    {
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

        public Config()
        {

        }
        
        public void readtask(string SelectNode)//讀取xml檔至任務清單
        {
            try
            {
                if (File.Exists(Path.Combine(System.Windows.Forms.Application.StartupPath, "config.xml")))
                {
                    //利用XMLDocument讀取;
                    XmlDocument xDL = new XmlDocument();
                    xDL.Load(Path.Combine(System.Windows.Forms.Application.StartupPath, "config.xml")); //Load XML檔
                                                                                                         //讀取單一Node，利用SingleNode方法取得
                    XmlNodeList NodeList = xDL.SelectNodes(SelectNode);

                    foreach (XmlNode item_File in NodeList)
                    {
                        XmlNodeList items = item_File.ChildNodes;
                        for (int i = 0; i < items.Count; i++)
                        {
                            XmlNodeList item = items[i].ChildNodes;
                            if (i == 0)
                            {
                                mysql_on = item[0].InnerText;
                                IP = item[1].InnerText;
                                DB = item[2].InnerText;
                                USER = item[3].InnerText;
                                Password = item[4].InnerText;
                                machinery_factory_demand_table1 = item[5].InnerText;
                                machinery_factory_hour_table1 = item[6].InnerText;
                                machinery_factory_day_table1 = item[7].InnerText;
                                machinery_factory_month_table1 = item[8].InnerText;
                                machinery_factory_realtime_table1 = item[9].InnerText;
                            }
                            else if (i == 1)
                            {
                                for (int j = 0; j < item.Count; j++)
                                {
                                    XmlNodeList device = item[j].ChildNodes;
                                    ModBus_IP.Add(device[0].InnerText);                     //擷取器IP
                                    ModBus_Port.Add(device[1].InnerText);                   //RTU/TCP Port
                                    ModBus_Name.Add(device[2].InnerText);                   //電表名稱
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
