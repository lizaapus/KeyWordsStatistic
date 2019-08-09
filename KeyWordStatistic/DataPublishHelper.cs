using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using MySql.Data.MySqlClient;
using Dapper;
using System.IO;
using System.Xml.Linq;
using TPI;

namespace CnkiCrawler.Tools
{
    /// <summary>
    ///  自定义数据源，将数据导入到kbase统一合表中
    /// </summary>
    public class DataPublishHelper
    {
        private const string SERVER_CONN_STR = "Server=192.168.106.56;Database=cnkicrawler;Uid=ClawMarks;Pwd=P@$$W0RD;default command timeout=7200;";

        /// <summary>
        /// 字段翻译字典<key,value>=<cn,en>
        /// </summary>
        private Dictionary<string, string> FieldsDic { get; set; } = new Dictionary<string, string>();

        private Dictionary<string, string> FieldsMappers { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// Xml字段字典
        /// </summary>
        private HashSet<string> XmlFields { get; set; } = new HashSet<string>();
        /// <summary>
        /// KBase刷排序的
        /// </summary>
        private List<string> SortOrders { get; set; } = new List<string>();
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        public DateTime RunDataTime { get; } = DateTime.Now;

        // 基本配置
        private string _BaseDir;
        private string _MysqlConnStr;
        private string _MysqlCmd;
        private string _ShareDir;
        private string _ShareAbsDir;
        private bool _IsUploadToKBase;

        // 固定值字段
        private string _SourceType;
        private string _IsAttachment;
        private string _IsRedirect;
        private string _TableName;
        private string _FFD;
        private string _Collector;


        public DataPublishHelper()
        {
            try
            {
                _BaseDir = ConfigurationManager.AppSettings["BaseDir"].ToString();
                _MysqlConnStr = ConfigurationManager.AppSettings["MysqlConnStr"].ToString();
                _MysqlCmd = ConfigurationManager.AppSettings["MysqlCmd"].ToString();
                _ShareDir = ConfigurationManager.AppSettings["ShareDir"].ToString();
                _ShareAbsDir = ConfigurationManager.AppSettings["ShareAbsDir"].ToString();
                _IsUploadToKBase = Convert.ToBoolean(ConfigurationManager.AppSettings["IsUploadToKBase"].ToString());
                _SourceType = ConfigurationManager.AppSettings["SourceType"]?.ToString();
                _IsAttachment = ConfigurationManager.AppSettings["IsAttachment"].ToString();
                _IsRedirect = ConfigurationManager.AppSettings["IsRedirect"].ToString();
                _TableName = ConfigurationManager.AppSettings["TableName"].ToString();
                _FFD = ConfigurationManager.AppSettings["FFD"].ToString();
                _Collector = ConfigurationManager.AppSettings["Collector"].ToString();

                var FieldsMapper = ConfigurationManager.AppSettings["FieldsMapper"].ToString();
                var XFs = ConfigurationManager.AppSettings["XmlFields"].ToString();

                foreach (var item in FieldsMapper.Split('|'))
                {
                    var kvs = item.Split(':');
                    var key = kvs[0];
                    var value = kvs[1];
                    if (string.IsNullOrEmpty(key) || FieldsMappers.Keys.Contains(key))
                        continue;
                    FieldsMappers.Add(key, value ?? "");
                }


                foreach (var item in XFs.Split('|'))
                {
                    XmlFields.Add(item);
                }

                // Init Translation Dictionary
                using (var Reader = MySqlHelper.ExecuteReader(SERVER_CONN_STR, "SELECT Cn,En FROM `t_translation`;"))
                {
                    while (Reader.Read())
                    {
                        var cn = Reader.GetString(0);
                        var en = Reader.GetString(1);
                        FieldsDic.Add(cn, en);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }

        public void Run()
        {
            Console.WriteLine();
            Console.WriteLine($"运行时间:{RunDataTime.ToString()}");
            Console.WriteLine("MakeRecFile...");
            MakeRecFile();

            if (_IsUploadToKBase)
            {
                Console.WriteLine();
                Console.WriteLine("Upload Data...");
                LoadToKbase();
            }

        }

        public void MakeRecFile()
        {
            FileName = _TableName + RunDataTime.ToString("yyyyMMddHHmmss") + ".txt";
            try
            {
                Directory.CreateDirectory(_BaseDir);
                using (var Writer = new StreamWriter(Path.Combine(_BaseDir, FileName), false, Encoding.Default))
                {
                    HashSet<string> DataFields = new HashSet<string>();
                    using (var Reader = MySqlHelper.ExecuteReader(_MysqlConnStr, _MysqlCmd))
                    {
                        for (int i = 0; i < Reader.FieldCount; i++)
                        {
                            DataFields.Add(Reader.GetName(i));
                        }
                        var Cnt = 0;
                        while (Reader.Read())
                        {
                            Cnt++;
                            Console.CursorLeft = 0;
                            Console.Write(Cnt);
                            int year = 0;
                            try
                            {
                                var XmlNode = new XElement("Root");
                                Writer.WriteLine("<REC>");
                                // 写KBase基本字段
                                foreach (var item in FieldsMappers)
                                {
                                    var i = DataFields.AsList().IndexOf(item.Value);
                                    if (i < 0)
                                        continue;

                                    var v = Reader.IsDBNull(i) ? "" : Reader.GetString(i);
                                    Writer.WriteLine($"<{item.Key}>={v}");
                                    if (XmlFields.Contains(item.Key))
                                    {
                                        XmlNode.Add(new XElement(FieldsDic.Keys.Contains(item.Key) ? FieldsDic.FirstOrDefault(_ => _.Key.Equals(item.Key)).Value : item.Key, v.RemoveInvalidChars()));
                                    }

                                    if (item.Key.Equals("年"))
                                        Int32.TryParse(v, out year);
                                }

                                // 判断是否存在“年”，如果不存在，取"发布日期（kbase）中的年"
                                if (year == 0)
                                {
                                    var PublishDate = FieldsMappers.FirstOrDefault(_ => _.Key.Equals("发布日期"));
                                    if (PublishDate.Key != null && DataFields.AsList().IndexOf(PublishDate.Value) >= 0)
                                    {
                                        DateTime dt;
                                        var pd = Reader.IsDBNull(DataFields.AsList().IndexOf(PublishDate.Value)) ? "" : Reader.GetString(DataFields.AsList().IndexOf(PublishDate.Value));
                                        if (DateTime.TryParse(pd, out dt))
                                            year = dt.Year;
                                    }
                                }

                                if (!FieldsMappers.Keys.Contains("年"))
                                {
                                    Writer.WriteLine($"<年>={(year == 0 ? "" : year.ToString())}");
                                    XmlNode.Add(new XElement(FieldsDic.FirstOrDefault(_ => _.Key.Equals("年")).Value, year == 0 ? "" : year.ToString()));
                                }
                                if (!FieldsMappers.Keys.Contains("资源类型"))
                                {
                                    Writer.WriteLine($"<资源类型>={_SourceType}");
                                    XmlNode.Add(new XElement(FieldsDic.FirstOrDefault(_ => _.Key.Equals("资源类型")).Value, _SourceType));
                                }

                                if (!FieldsMappers.Keys.Contains("是否有附件"))
                                {
                                    Writer.WriteLine($"<是否有附件>={_IsAttachment}");
                                    XmlNode.Add(new XElement(FieldsDic.FirstOrDefault(_ => _.Key.Equals("是否有附件")).Value, _IsAttachment));
                                }

                                if (!FieldsMappers.Keys.Contains("是否跳转"))
                                    Writer.WriteLine($"<是否跳转>={_IsRedirect}");

                                if (!FieldsMappers.Keys.Contains("TableName"))
                                {
                                    Writer.WriteLine($"<TableName>={_TableName}");
                                }

                                if (!FieldsMappers.Keys.Contains("FFD"))
                                    Writer.WriteLine($"<FFD>={_FFD}");
                                if (!FieldsMappers.Keys.Contains("采集人"))
                                {
                                    Writer.WriteLine($"<采集人>={_Collector}");
                                    XmlNode.Add(new XElement(FieldsDic.FirstOrDefault(_ => _.Key.Equals("采集人")).Value, _Collector));
                                }
                                if (!FieldsMappers.Keys.Contains("表名"))
                                {
                                    var tname = _TableName + (year > 2012 ? year.ToString() : (year == 0 ? RunDataTime.Year.ToString() : "2012"));
                                    Writer.WriteLine($"<表名>={tname}");
                                    XmlNode.Add(new XElement(FieldsDic.FirstOrDefault(_ => _.Key.Equals("表名")).Value, tname));
                                }

                                if (!FieldsMappers.Keys.Contains("更新日期"))
                                {
                                    Writer.WriteLine($"<更新日期>={RunDataTime.ToString("yyyy-MM-dd")}");
                                    XmlNode.Add(new XElement(FieldsDic.FirstOrDefault(_ => _.Key.Equals("更新日期")).Value, RunDataTime.ToString("yyyy-MM-dd")));
                                }

                                XmlFields.Except(FieldsMappers.Keys).AsList().ForEach(_ =>
                                {
                                    var i = DataFields.AsList().IndexOf(_);
                                    if (i != -1)
                                    {
                                        var t = FieldsDic.FirstOrDefault(fd => fd.Key.Equals(_));
                                        if (t.Key == null)
                                        {
                                            Console.WriteLine();
                                            Console.WriteLine($"字典中未找到‘{_}’的翻译...");
                                            _IsUploadToKBase = false;
                                            Reader.Close();
                                        }
                                        XmlNode.Add(new XElement(t.Value, Reader.IsDBNull(i) ? "" : Reader.GetString(i).RemoveInvalidChars()));
                                    }

                                });
                                Writer.WriteLine($"<XML字段>={XmlNode.ToString()}");
                            }
                            catch (Exception e)
                            {
                                //using (var EW = new StreamWriter(Path.Combine(_BaseDir, $"ErrorLog_{RunDataTime.ToString("yyyyMMddHHmmss")}.txt"), true, Encoding.UTF8))
                                //{
                                //    EW.WriteLine();
                                //    EW.WriteLine("UrlID:" + Reader.GetString(0));
                                //    EW.WriteLine(e.Message);
                                //}
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("生成REC文件失败！");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                _IsUploadToKBase = false;
            }
        }

        public void LoadToKbase()
        {
            //  读取kbase配置
            var RemoteIP = ConfigurationManager.AppSettings["RemoteIP"].ToString();
            var KBasePort = Convert.ToInt32(ConfigurationManager.AppSettings["KBasePort"].ToString());
            var KBaseUser = ConfigurationManager.AppSettings["KBaseUser"].ToString();
            var KBasePwd = ConfigurationManager.AppSettings["KBasePwd"].ToString();

            string RemoteDir = Path.Combine(_ShareDir, _TableName);
            Directory.CreateDirectory(RemoteDir);

            if (File.Exists(Path.Combine(RemoteDir, FileName)))
                File.Delete(Path.Combine(RemoteDir, FileName));
            File.Copy(Path.Combine(_BaseDir, FileName), Path.Combine(RemoteDir, FileName));

            HashSet<string> IndexCols = new HashSet<string>(FieldsMappers.Keys.AsEnumerable());
            IndexCols.Add("年");
            IndexCols.Add("资源类型");
            IndexCols.Add("是否有附件");
            IndexCols.Add("是否跳转");
            IndexCols.Add("TableName");
            IndexCols.Add("FFD");
            IndexCols.Add("采集人");
            IndexCols.Add("表名");
            IndexCols.Add("更新日期");
            IndexCols.Add("XML字段");

            using (var Reader = MySqlHelper.ExecuteReader(SERVER_CONN_STR, "SELECT * FROM `t_kbase_order`;"))
            {
                while (Reader.Read())
                {
                    SortOrders.Add(Reader.GetString(0));
                }
            }

            using (var client = new Client())
            {
                if (client.Connect(RemoteIP, KBasePort, KBaseUser, KBasePwd))
                {
                    RecordSet rs = client.OpenRecordSet($"SELECT 文件名 FROM {_TableName}");
                    int cnt = rs.GetCount();
                    rs.Close();

                    int i = 0;
                    //导入数据(数据在kbase服务器)
                    i = client.ExecMgrSQL($"bulkload table {_TableName} '{Path.Combine(_ShareAbsDir, _TableName, Path.GetFileName(FileName))}'");
                    Console.WriteLine("Indexing...");
                    //索引插入数据
                    if (cnt > 0)
                    {
                        i = client.ExecMgrSQL($"INDEX {_TableName} ON {IndexCols.AsEnumerable().Aggregate((c1, c2) => c1 + "," + c2)} FROM {cnt + 1}");
                    }
                    else
                    {
                        i = client.ExecMgrSQL($"INDEX {_TableName} ON {IndexCols.AsEnumerable().Aggregate((c1, c2) => c1 + "," + c2)}");
                    }

                    Console.WriteLine("Ordering...");
                    // 获取排序字段语句
                    SortOrders.ForEach(_ =>
                    {
                        client.ExecMgrSQL(_.Replace("{TABLENAME}", _TableName));
                        // 每次执行排序后，需要执行以下语句
                        client.ExecMgrSQL($"dbum refresh sortfile of table {_TableName}");
                    });
                }
            }
        }
    }
}
