using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TPI;

namespace KeyWordStatistic
{
    public class KeyWordStatisticC
    {
        string connectMysql = "server = 192.168.106.60"
               + "; database = Test"
               + "; user id = root"
               + "; password = cnkittod"
               + ";pooling=true;CharSet=utf8;port=3306;SslMode = none;";
        public void Start()
        {
            Thread thread = new Thread(new ThreadStart(NewThread));
            thread.Name = "NewThread";
            thread.Start();
        }

        public void NewThread()
        {
            string KBaseIP = "192.168.107.99";
            int KbasePort = 4567;
            string KBaseUser = "DBOWN";
            string KBasePwd = string.Empty;
            List<string> kbs = GetKbaseDBs();
            CreatedMysqlTable();
            Client client = new Client();
            if (client.Connect(KBaseIP, KbasePort, KBaseUser, KBasePwd))
            {
                List<string> Cols = new List<string>();
                StringBuilder DataLine = new StringBuilder();
                foreach (var KBaseSql in kbs)
                {
                    Console.WriteLine("检索表" + KBaseSql);
                    RecordSet cursor = client.OpenRecordSet(KBaseSql);
                    if (cursor == null)
                    {
                        Console.WriteLine("KBase sql语句错误 或 KBase数据库无数据！");
                        continue;
                    }

                    int RowCnt = cursor.GetCount();
                    Console.WriteLine("数据库总条数：" + RowCnt);
                    for (int i = 0; i < RowCnt; i++)
                    {
                        string bValue = cursor.GetValue("主题");
                        DealValue(bValue);
                        Console.CursorLeft = 0;
                        Console.Write($"处理条数:{i.ToString("D8")}");
                        cursor.MoveNext();
                    }
                    cursor.Dispose();
                    Console.WriteLine("检索完成");
                }
                Console.WriteLine("全部检索完成");
            }
        }

        private bool CreatedMysqlTable()
        {
            #region 创建mysql数据库表

            var IsRegex = new Regex("^Is[A-Z]");
            using (var Conn = new MySqlConnection(connectMysql))
            {
                Conn.Open();

                string createStatement = "CREATE TABLE keyWordsCnts (`keyWords` VarChar(255) not null primary key , `cnts` int )ENGINE=MyISAM DEFAULT CHARSET=utf8";
                using (MySqlCommand cmd = new MySqlCommand(createStatement, Conn))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return false;
                    }
                }
                #endregion
            }
        }

        private void DealValue(string bValue)
        {
            if (string.IsNullOrEmpty(bValue))
                return;
            bValue.Split(';').ToList().ForEach(item =>
            {
                item = RemoveInvalidateChar(item);
                int index = item.IndexOf(':');
                if (index < 0)
                    index = item.IndexOf(" ");
                if (index > 0)
                {
                    item = item.Substring(0, index);
                    if (item != "篇长")
                        UpdateMysql(item);
                }

            });

        }

        private string RemoveInvalidateChar(string str)
        {
            StringBuilder info = new StringBuilder();
            foreach (char c in str)
            {
                int s = (int)c;
                if (((s >= 0) && (s <= 8)) || ((s >= 11) && (s <= 12)) || ((s >= 14) && (s <= 32)))
                {
                    info.Append(" ");
                }
                else
                    info.Append(c);
            }
            return info.ToString();
        }

        private void UpdateMysql(string key)
        {

            int count = 0;
            using (var Conn = new MySqlConnection(connectMysql))
            {
                Conn.Open();
                key = key.Replace("'", "").Replace("/", "").Replace("\"", "");
                string searchSql = "select * from keyWordsCnts where keyWords='" + key + "'";
                bool isExist = false;
                try
                {
                    using (var Reader = MySqlHelper.ExecuteReader(connectMysql, searchSql))
                    {
                        try
                        {
                            while (Reader.Read())
                            {
                                isExist = true;
                                count = Convert.ToInt32(Reader.GetValue(1).ToString());
                                count++;
                                string updateSql = "update keyWordsCnts set cnts=" + count + " where keyWords = '" + key + "'";

                                using (MySqlCommand cmd = new MySqlCommand(updateSql, Conn))
                                {
                                    try
                                    {
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                    if (!isExist)
                    {
                        string sqlstr = "insert into keyWordsCnts values('" + key + "',1)";
                        using (MySqlCommand cmd = new MySqlCommand(sqlstr, Conn))
                        {
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    return;
                }
            }

        }

        private List<string> GetKbaseDBs()
        {
            List<string> kbs = new List<string>() {
                "SELECT * FROM GMRB GO",
                "SELECT * FROM JFJB GO",
                "SELECT * FROM JJRB GO",
                "SELECT * FROM QIUSHI GO",
                "SELECT * FROM RMRB GO",
                "SELECT * FROM XXSB GO",
            };
            return kbs;
        }
    }
    public class KBaseConfig
    {
        public string KBaseIP;
        public string KbasePort;
        public string KBaseUser;
        public string KBasePwd;
        public string KBaseSql;
        public KBaseConfig()
        {
            KBaseIP = "192.168.107.99";
            KbasePort = "4567";
            KBaseUser = "DBOWN";
            KBasePwd = string.Empty;
            KBaseSql = string.Empty;
        }
    }
}
