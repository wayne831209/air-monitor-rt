using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
//NuGet:MySQL.data-v8.0.11
namespace MySQL
{
    class MYSQL
    {
        public List<string> readdata;
        string MYSQL_IP;
        string MYSQL_DB;
        string MYSQL_user;
        string MYSQL_password;

        public MYSQL(string IP,string DB, string User, string Password)
        {
            MYSQL_IP = IP;
            MYSQL_DB = DB;
            MYSQL_user = User;
            MYSQL_password = Password;
        }

        //db.insertdata("INSERT INTO tpi_machinedata"+
        //    "(`Time`,`MachineName`,`NC_S_rpm`,`NC_Spindle_MotorTemp_degreesC`,`NC_Spindle_load_percent`,`MachineStatus`,`Spindle_X_g`,`Spindle_Y_g`,`Spindle_Z_g`)"+
        //    "VALUES('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','NXV1020A','" + listfocas[1] + "','" + listfocas[2] + "','" + listfocas[3] + "','" + listfocas[0] + "','" + X_rms + "','" + Y_rms + "','" + Z_rms + "')");
        public void insertdata(string Cmd)
        {
            string con_str = "server=" + MYSQL_IP + ";database=" + MYSQL_DB + ";uid=" + MYSQL_user + ";pwd=" + MYSQL_password;
            MySqlConnection dbcon = new MySqlConnection(con_str);
            dbcon.Open();
            MySqlCommand cmd;
            cmd = new MySqlCommand(Cmd, dbcon);
            //double val = (double)cmd.ExecuteNonQuery();
            cmd.ExecuteNonQuery();
            dbcon.Close();

            //return val;
        }
        //db.updatedata("UPDATE tpi_machinedata SET " +
        //        "`Time`='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
        //        "`MachineName`='NXV1020A', " +
        //        "`NC_S_rpm`='" + listfocas[1] + "', " +
        //        "`NC_Spindle_MotorTemp_degreesC`='" + listfocas[2] + "', " +
        //        "`NC_Spindle_load_percent`='" + listfocas[3] + "', " +
        //        "`MachineStatus`='" + listfocas[0] + "', " +
        //        "`Spindle_X_g`='" + X_rms + "', " +
        //        "`Spindle_Y_g`='" + Y_rms + "', " +
        //        "`Spindle_Z_g`='" + Z_rms + "' WHERE `Num`='1'");
        public void updatedata(string Cmd)
        {
            string con_str = "server=" + MYSQL_IP + ";database=" + MYSQL_DB + ";uid=" + MYSQL_user + ";pwd=" + MYSQL_password;
            MySqlConnection dbcon = new MySqlConnection(con_str);
            dbcon.Open();
            MySqlCommand cmd;
            cmd = new MySqlCommand(Cmd, dbcon);
            //double val = (double)cmd.ExecuteNonQuery();
            cmd.ExecuteNonQuery();
            dbcon.Close();
            
        }

        //energe.selectdata("SELECT MAX(Demand) FROM spindleservice.new_meterdemand where Meter_Name='" + Meter_id + "'&&year(time) = '" + DateTime.Now.Year.ToString() + "' &&month(time) = '" + DateTime.Now.Month.ToString() + "'");
        public void selectdata(string Cmd)
        {
            readdata = new List<string>();
            string con_str = "server=" + MYSQL_IP + ";database=" + MYSQL_DB + ";uid=" + MYSQL_user + ";pwd=" + MYSQL_password;
            MySqlConnection dbcon = new MySqlConnection(con_str);
            dbcon.Open();
            MySqlCommand cmd;
            cmd = new MySqlCommand(Cmd, dbcon);
            MySqlDataReader data = cmd.ExecuteReader();
            while (data.Read())
            {

                for (int i = 0; i < data.FieldCount; i++)
                {
                    readdata.Add(data[i].ToString());
                }
            }
            dbcon.Close();
        }
        public DataTable GetMyDataTable(string SqlString)
        {
            DataTable myDataTable = new DataTable();
            using (MySqlCommand isc = new MySqlCommand())
            {
                MySqlConnection icn = null;
                icn = MyOpenConn(MYSQL_IP, MYSQL_DB, MYSQL_user, MYSQL_password);
                MySqlDataAdapter da = new MySqlDataAdapter(isc);
                isc.Connection = icn;
                isc.CommandText = SqlString;
                //isc.CommandTimeout = 600;
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds);
                myDataTable = ds.Tables[0];
                if (icn.State == ConnectionState.Open) icn.Close();
                return myDataTable;
            }
            //MySqlConnection icn = null;
            //icn = MyOpenConn(Server, Database, dbuid, dbpwd);
            //MySqlCommand isc = new MySqlCommand();
            //MySqlDataAdapter da = new MySqlDataAdapter(isc);
            //isc.Connection = icn;
            //isc.CommandText = SqlString;
            ////isc.CommandTimeout = 600;
            //DataSet ds = new DataSet();
            //ds.Clear();
            //da.Fill(ds);
            //myDataTable = ds.Tables[0];
            //if (icn.State == ConnectionState.Open) icn.Close();
            //return myDataTable;
        }
        public static MySqlConnection MyOpenConn(string Server, string Database, string dbuid, string dbpwd)
        {
            string cnstr = string.Format("server={0};database={1};uid={2};pwd={3};Connect Timeout = 180; CharSet=utf8;Sslmode=none;", Server, Database, dbuid, dbpwd);
            MySqlConnection icn = new MySqlConnection();
            icn.ConnectionString = cnstr;
            if (icn.State == ConnectionState.Open) icn.Close();
            icn.Open();
            return icn;
        }
    }
}
