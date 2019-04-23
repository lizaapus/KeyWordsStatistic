using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KeyWordStatistic
{
   public class MysqlBaseOps
   {
        public string MysqlIP = "192.168.106.60";
        public string MysqlDatabase = "Test";
        public string MysqlUserName = "root";
        public string MysqlPassword = "cnkittod";
        public string MysqlTableName = "test";
        public bool IsCreatePrimaryKey = true;
        private string connectMysql = "";


        public MysqlBaseOps()
        {
            InitConnectMysql();
        }

        public void InitConnectMysql()
        {
              connectMysql = "server = " + MysqlIP
              + "; database =" + MysqlDatabase
              + "; user id = " + MysqlUserName
              + "; password = " + MysqlPassword
              + ";pooling=true;CharSet=utf8;port=3306;SslMode = none;";
        }

        
        /// <summary>
        /// KBase迁移数据至mysql
        /// </summary>
        /// <param name="Cols"></param>
        private bool CreatedMysqlTable(List<string> Cols)
        {
            #region 创建mysql数据库表
          
            var IsRegex = new Regex("^Is[A-Z]");
            using (var Conn = new MySqlConnection(connectMysql))
            {
                Conn.Open();
                if (Cols.Count == 0)
                {
                    Console.WriteLine("列名为空，非法");
                    Console.ReadLine();
                    return false;
                }
                string createStatement = "CREATE TABLE " + MysqlTableName + " (";
                int i = 0;
                foreach (string colloumn in Cols)
                {
                    var tempColloumn = colloumn.Replace("/", "");
                    tempColloumn = "`" + tempColloumn + "`";
                    if (i == 0)
                    {
                        if (IsCreatePrimaryKey == true)
                        {
                            createStatement += tempColloumn + " VarChar(255) not null primary key,";
                            i++;
                        }
                        else
                        {
                            createStatement += tempColloumn + " ,";
                            i++;
                        }
                    }
                    else if (IsRegex.Match(tempColloumn).Success)
                    {
                        createStatement += tempColloumn + " tinyint(4),";
                        i++;
                    }
                    else if (tempColloumn.Contains("Content")
                        || tempColloumn.Contains("content")
                        || tempColloumn.Contains("Html")
                        || tempColloumn.Contains("HTML")
                        || tempColloumn.Contains("正文")
                        || tempColloumn.Contains("SMARTS")
                        || tempColloumn.Contains("html")
                        || tempColloumn.Contains("概要")
                        || tempColloumn.Contains("摘要")
                        || tempColloumn.Contains("Xml")
                        || tempColloumn.Contains("xml")
                        || tempColloumn.Contains("XML")
                        )
                    {
                        createStatement += tempColloumn + " longtext,";
                        i++;
                    }
                    else
                    {
                        createStatement += " " + tempColloumn + " text,";
                        i++;
                    }
                }
                createStatement = createStatement.Remove(createStatement.Length - 1);
                createStatement += ")ENGINE=MyISAM DEFAULT CHARSET=utf8";
                using (MySqlCommand cmd = new MySqlCommand(createStatement, Conn))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();

                        Console.WriteLine("创建"+ MysqlTableName + "表成功！");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine( "创建Mysql-" + MysqlTableName + "表失败···\n");
                        return false;
                    }
                }
               
            }
            #endregion
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
                string searchSql = "select * from "+ MysqlTableName + " where keyWords='" + key + "'";
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
    }
}
