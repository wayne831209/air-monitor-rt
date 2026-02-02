using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


/// <summary>
/// path:   儲存路徑
/// item:   標題
/// msg:    內容
/// name:   檔名
/// recover:是否覆寫(0:否 1:是)
/// </summary>
namespace FILE
{
    class file
    {
        public void save_txt(string path,string[] item,string[] msg,string name,int recover)
        {
            string str = "";
            string val = "";
            string filepath = path + DateTime.Now.ToString("yyyyMMdd_") + name + ".txt";
            for (int i = 0; i < item.Count(); i++)
            {
                str += item[i] + "\t";
                val += msg[i] + "\t";
            }
            if (File.Exists(filepath))
            {
                // Add some text to the file.
                try
                {
                    StreamWriter txt;
                    if (recover == 0)
                        txt = new StreamWriter(filepath, true);
                    else
                        txt = new StreamWriter(filepath, false);
                    txt.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t" + val);
                    txt.Close();

                    //FileInfo fileinfo = new FileInfo(filepath);
                    //if(fileinfo.Length > 512000)
                    //{
                    //    string newfilepath = path + DateTime.Now.ToString("yyyyMMddHHmm") + name + ".txt";
                    //    File.Move(filepath, newfilepath);
                    //}

                }
                catch { }
            }
            else
            {
                StreamWriter txt;
                if (recover == 0)
                    txt = new StreamWriter(filepath, true);
                else
                    txt = new StreamWriter(filepath, false);

                txt.WriteLine("Time" + "\t" + str);
                txt.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t" + val);

                txt.Close();
            }
        }
        public void rename(string path,string name)
        {
            string filepath = path + DateTime.Now.ToString("yyyyMMdd") + name + ".txt";
            string newfilepath = path + DateTime.Now.ToString("yyyyMMddHHmm") + name + ".txt";
            if (!File.Exists(newfilepath))
            {
                File.Move(filepath, newfilepath);
            }
            else
            {

            }
        }
    }
}
