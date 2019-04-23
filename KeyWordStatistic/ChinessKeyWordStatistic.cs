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
    public class ChinessKeyWordStatistic
    {
        string connectMysql = "server = 192.168.106.60"
               + "; database = Test"
               + "; user id = root"
               + "; password = cnkittod"
               + ";pooling=true;CharSet=utf8;port=3306;SslMode = none;";

        string mysqltableName = "ChinesekeyWordsCnts";

        string KBaseIP = "192.168.106.18";
        int KbasePort = 4567;
        string KBaseUser = "DBOWN";
        string KBasePwd = string.Empty;
        public void Start()
        {
            Thread thread = new Thread(new ThreadStart(NewThread));
            thread.Name = "NewThread";
            thread.Start();
        }

        public void NewThread()
        {
            //CreatedMysqlTable();
            Client client = new Client();
            var Dic = new Dictionary<string, int>();
            if (client.Connect(KBaseIP, KbasePort, KBaseUser, KBasePwd))
            {
                List<string> Cols = new List<string>();
                StringBuilder DataLine = new StringBuilder();
               
                string KBaseSql = "select 中文关键词,* from CJFD where (主题%='习近平新时代中国特色社会主义思想' or 题名%='习近平新时代中国特色社会主义思想')";
                RecordSet cursor = client.OpenRecordSet(KBaseSql);
                if (cursor == null)
                {
                    Console.WriteLine("KBase sql语句错误 或 KBase数据库无数据！");
                    Console.ReadLine();
                    return;
                }

                int RowCnt = cursor.GetCount();
                Console.WriteLine("查询总条数：" + RowCnt);

                int ColumnCnt = cursor.GetFieldCount();
                for (int i = 0; i < RowCnt; i++)
                {
                    var keys = cursor.GetValue(0);
                    if (!string.IsNullOrWhiteSpace(keys))
                    {
                        var arr = Regex.Split(keys, ";;");
                        foreach (var k in arr)
                        {
                            if (string.IsNullOrWhiteSpace(k))
                                continue;
                            if (Dic.ContainsKey(k))
                                Dic[k]++;
                            else
                                Dic.Add(k, 1);

                        }
                    }
                    Console.CursorLeft = 0;
                    Console.Write($"处理条数:{(i+1).ToString("D8")}");
                    cursor.MoveNext();
                }

                cursor.Dispose();
                Console.WriteLine();
                Console.WriteLine("检索完成");
            }
            var sql = "INSERT IGNORE INTO t_ck VALUE(?k,?cnt)";
            var j = 0;
            foreach (var kv in Dic)
            {
                j += MySqlHelper.ExecuteNonQuery(connectMysql, sql, new MySqlParameter("k", kv.Key), new MySqlParameter("cnt", kv.Value));
                Console.CursorLeft = 0;
                Console.Write($"添加条数:{j.ToString("D8")}");
            }
            Console.WriteLine();
            Console.ReadLine();

        }

        private bool CreatedMysqlTable()
        {
            #region 创建mysql数据库表
            using (var Conn = new MySqlConnection(connectMysql))
            {
                Conn.Open();

                string createStatement = "CREATE TABLE "+ mysqltableName + " (`keyWords` VarChar(255) not null primary key , `cnts` int )ENGINE=MyISAM DEFAULT CHARSET=utf8";
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
                if (!string.IsNullOrEmpty(item))
                {
                        if (item != "篇长")
                            UpdateMysql(item);
                }
            });

        }

        private void UpdateMysql(string key)
        {

            int count = 0;
            using (var Conn = new MySqlConnection(connectMysql))
            {
                Conn.Open();
                key = key.Replace("'", "").Replace("/", "").Replace("\"", "");
                string searchSql = "select * from " + mysqltableName + " where keyWords='" + key + "'";
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
                                string updateSql = "update " + mysqltableName + " set cnts=" + count + " where keyWords = '" + key + "'";

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
                        string sqlstr = "insert into " + mysqltableName + " values('" + key + "',1)";
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

        
    }
}
