using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Html = HtmlAgilityPack;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization.Attributes;
using MySql.Data.MySqlClient;

namespace KeyWordStatistic
{
    public class TransforMongoToMysql
    {
        string _MongoDBConnectString = "mongodb://192.168.106.56:27017";
        string mongoDB = "Yqk39Net";
        string collect = "Enterprise";

        public string MysqlIP = "192.168.106.56";
        public string MysqlDatabase = "enterprise";
        public string MysqlUserName = "ClawMarks";
        public string MysqlPassword = "P@$$W0RD";
        public string MysqlTableName = "t_data";


     

        public async Task TransforMToMysql()
        {
            
            var Filter = Builders<BsonDocument>.Filter;
            var MongdbClient = new MongoClient(_MongoDBConnectString);

            //Console.Write("请输入要迁移的数据库名：");
            //mongoDB = Console.ReadLine();
            //Console.Write("请输入要迁移数据库下的collection名：");
            //collect = Console.ReadLine();

            var mDataBase = MongdbClient.GetDatabase(mongoDB);
            var DataCollection = mDataBase.GetCollection<BsonDocument>(collect);
            var Cursor = DataCollection.Find(Filter.Empty).ToCursor();

            var connectMysql = "server = " + MysqlIP
              + "; database =" + MysqlDatabase
              + "; user id = " + MysqlUserName
              + "; password = " + MysqlPassword
              + ";pooling=true;CharSet=utf8;port=3306;SslMode = none;AllowLoadLocalInfile=true;";


            Dictionary<string, string> matchDic = new Dictionary<string, string>();
            matchDic.Add("Id", "_id");
            matchDic.Add("企业全称", "企业名称");
            matchDic.Add("企业经济性质", "企业经济性质");
            matchDic.Add("注册资金", "注册资金");
            matchDic.Add("公司简介", "公司简介");
            matchDic.Add("经营方式", "经营方式");
            matchDic.Add("经营方式规范", "经营方式规范");
            matchDic.Add("所在地", "所在地");
            matchDic.Add("地域规范代码", "地域规范代码");
            matchDic.Add("应用领域规范代码", "应用领域规范代码");
            matchDic.Add("产品列表", "产品列表");
            matchDic.Add("地址", "地址");
            matchDic.Add("联系人", "联系人");
            matchDic.Add("电话", "电话");
            matchDic.Add("手机", "手机");
            matchDic.Add("邮箱", "邮箱");
            matchDic.Add("传真", "传真");
            matchDic.Add("主页", "主页");
            matchDic.Add("列表图片", "列表图片");
            matchDic.Add("详情图片", "详情图片");
            matchDic.Add("主营产品", "主营产品");
            matchDic.Add("产品名称", "产品");
            matchDic.Add("产品图片", "产品图片");
            matchDic.Add("URL", "Url");
            matchDic.Add("来源网站", "来源网站");
            matchDic.Add("createtime", "createtime");
            matchDic.Add("updatetime", "updatetime");
            var SetColumns = new HashSet<string>();
            SetColumns.Add("Id");
            SetColumns.Add("企业全称");
            SetColumns.Add("企业经济性质");
            SetColumns.Add("注册资金");
            SetColumns.Add("公司简介");
            SetColumns.Add("经营方式");
            SetColumns.Add("经营方式规范");
            SetColumns.Add("所在地");
            SetColumns.Add("地域规范代码");
            SetColumns.Add("应用领域规范代码");
            SetColumns.Add("产品列表");
            SetColumns.Add("地址");
            SetColumns.Add("联系人");
            SetColumns.Add("电话");
            SetColumns.Add("手机");
            SetColumns.Add("邮箱");
            SetColumns.Add("传真");
            SetColumns.Add("主页");
            SetColumns.Add("列表图片");
            SetColumns.Add("详情图片");
            SetColumns.Add("主营产品");
            SetColumns.Add("产品名称");
            SetColumns.Add("产品图片");
            SetColumns.Add("URL");
            SetColumns.Add("来源网站");
            SetColumns.Add("createtime");
            SetColumns.Add("updatetime");

            var FileNames = new List<string>();
            FileNames.Add("file0.txt");
            StreamWriter Writer = new StreamWriter(FileNames[0], false, Encoding.UTF8);
            int ncount = 0;
            //读数据
            while (await Cursor.MoveNextAsync())
            {
                foreach (var Document in Cursor.Current)
                {
                    Writer.Write("<%RS%>");
                    foreach (var col in SetColumns)
                    {
                        if(col == "createtime"||col== "updatetime")
                            Writer.Write($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}<%COL%>");
                        else if(col == "Id")
                        {
                            var bValue = Document.GetValue(matchDic[col]).AsObjectId.ToString();
                            Writer.Write($"{bValue}<%COL%>");
                        }else if(col == "产品名称")
                        {
                            var bValue = Document.GetValue(matchDic[col],"");
                            string pvalue = "";
                            if(bValue != "")
                            {
                                var pList = bValue.AsBsonArray;
                                foreach (var item in pList)
                                {
                                    pvalue += item["名称"].ToString() + ";";
                                }

                            }
                            Writer.Write($"{pvalue}<%COL%>");
                        }else if(col == "来源网站")
                        {
                            //var bValue = "3618医疗器械网";
                            //var bValue = "环球医疗器械网";
                            var bValue = "39健康网";
                            Writer.Write($"{bValue}<%COL%>");
                        }
                        else
                        {
                           
                            var bValue = string.Empty;
                            var bValueBson = Document.GetValue(matchDic[col], "");
                            if (!bValueBson.IsBsonNull)
                                bValue = RemoveInvalidateChar(bValueBson.ToString());
                            Writer.Write($"{bValue}<%COL%>");
                        }
                    }
                    Writer.Write("<%RE%>");
                    ncount++;
                    if (ncount % 1000 == 0)
                    {
                        Console.CursorLeft = 0;
                        Console.WriteLine( "已读取" + ncount + "条数据···");
                        string filename = "file" + ncount / 1000 + ".txt";
                        FileNames.Add(filename);
                        Writer.Close();
                        Writer = new StreamWriter(filename, false, Encoding.UTF8);
                    }
                }
            }
            Writer.Close();
            Console.WriteLine("共读取" + ncount.ToString() + "条数据");
            Console.WriteLine("开始写数据");
            using (var Conn = new MySqlConnection(connectMysql))
            {
                Conn.Open();
                ncount = 0;
                foreach (string filename in FileNames)
                {
                    try
                    {
                        var Bulk = new MySqlBulkLoader(Conn);
                        Bulk.TableName = "t_data";
                        Bulk.ConflictOption = MySqlBulkLoaderConflictOption.Ignore;
                        Bulk.Local = true;
                        Bulk.Timeout = 10 * 60 * 60 * 1000;
                        Bulk.CharacterSet = "utf8mb4";
                        Bulk.LinePrefix = "<%RS%>";
                        Bulk.LineTerminator = "<%RE%>";
                        Bulk.FieldTerminator = "<%COL%>";
                        Bulk.EscapeCharacter = '\b';
                        Bulk.FileName = filename;
                        ncount += Bulk.Load();
                        Console.CursorLeft = 0;
                        Console.Write("已写入" + ncount.ToString() + "条数据\n");    
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                Console.Write("共写入" + ncount.ToString() + "条数据\n");
            }
            Console.WriteLine("数据迁移完成");
            foreach (string filename in FileNames)
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            Console.ReadKey();
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
