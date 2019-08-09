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
    public class UpdateMysqldb
    {
        string connectMysql = "server = 192.168.106.60"
              + "; database = enterprise_library"
              + "; user id = root"
              + "; password = cnkittod"
              + ";pooling=true;CharSet=utf8;port=3306;SslMode = none;Allow User Variables=True;";
        Dictionary<string, string> Dic = new Dictionary<string, string>();
        public UpdateMysqldb()
        {
            Dic = new ReadDic() { fileName = "managementStyle.txt" }.dic();
            using (var Conn = new MySqlConnection(connectMysql))
            {
                Conn.Open();
                string searchSql = "select * from company_new";
                var icount = 1;
                try
                {
                    using (var Reader = MySqlHelper.ExecuteReader(connectMysql, searchSql))
                    {
                        try
                        {
                            while (Reader.Read())
                            {
                                var id = Reader.GetValue(0).ToString();//获取id
                                var value = Reader.GetValue(4).ToString();
                                var temp = GetStyleValue(value);
                                //string updateSql = "update company_new set company_new.`经营方式规范`='" + temp + "'where company_new.`文件名`='" + id + "'";
                                string updateSql = "update company_new set `经营方式规范`='" + temp + "' where `文件名`='" + id + "'";
                                //MySqlHelper.ExecuteNonQuery(Conn, updateSql);
                                using (MySqlCommand cmd = new MySqlCommand(updateSql, Conn))
                                {
                                    try
                                    {
                                        cmd.CommandTimeout = 60000;
                                        cmd.ExecuteNonQuery();

                                        icount++;
                                        Console.CursorLeft = 0;
                                        Console.WriteLine("更新第" + icount + "条");
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
                catch (Exception e)
                {
                    return;
                }
            }
            Console.ReadKey();
        }

        public string GetStyleValue(string value)
        {
            string str = "";
            if (value == "")
                str = "其他";
            else
            {
                foreach (var item in Dic)
                {
                    if (value.Contains(item.Key))
                    {
                        if (!str.Contains(item.Value))
                            str += item.Value + ";";
                    }
                }
            }
            if (str == "")
                str = "其他";
            else
                str = str.Substring(0, str.Length - 1);
            return str;
        }
    }
}
