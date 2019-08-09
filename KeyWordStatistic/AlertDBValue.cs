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
    public class AlertDBValue
    {
        //mysql连接语句
        string connectMysql = "server = 192.168.106.60"
               + "; database = enterprise_library"
               + "; user id = root"
               + "; password = cnkittod"
               + ";pooling=true;CharSet=utf8;port=3306;SslMode = none;";
        public AlertDBValue()
        {
            Run();
        }

        private void Run()
        {
            using (var Conn = new MySqlConnection(connectMysql))
            {
                Conn.Open();
                string searchSql = "select * from temp_youzui";
                using (var Reader = MySqlHelper.ExecuteReader(connectMysql, searchSql))
                {
                    try
                    {
                        while (Reader.Read())
                        {
                            string updateSql = "update temp_youzui set 应用领域 = '医药健康' where 应用领域 = ''";
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
            }
        }
    }
}
