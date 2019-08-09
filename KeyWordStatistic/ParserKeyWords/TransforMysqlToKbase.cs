using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyWordStatistic
{
    public class TransforMysqlToKbase
    {
        private Dictionary<string, string> FieldsDic { get; set; } = new Dictionary<string, string>();
        private Dictionary<string, string> FieldsMappers { get; set; } = new Dictionary<string, string>();
        /// </summary>
        private HashSet<string> XmlFields { get; set; } = new HashSet<string>();
        private List<string> SortOrders { get; set; } = new List<string>();
        public string FileName { get; set; }

        public DateTime RunDataTime { get; } = DateTime.Now;

        // 基本配置
        private string _BaseDir;
        private string _MysqlConnStr;
        private string _MysqlCmd;

        // 固定值字段
        string FieldsMapper = string.Empty;
        string XFs = string.Empty;

        public string MysqlIP = "192.168.106.56";
        public string MysqlDatabase = "enterprise";
        public string MysqlUserName = "ClawMarks";
        public string MysqlPassword = "P@$$W0RD";
        public string MysqlTableName = "t_data";
        public TransforMysqlToKbase()
        {
            //_MysqlConnStr = "Server=192.168.106.56;Database=enterprise;Uid=root;Pwd=cnkittod;AllowZeroDateTime=True;default command timeout=120";
            _MysqlConnStr = "server = " + MysqlIP
             + "; database =" + MysqlDatabase
             + "; user id = " + MysqlUserName
             + "; password = " + MysqlPassword
             + ";pooling=true;CharSet=utf8;port=3306;SslMode = none;AllowLoadLocalInfile=true;allow zero datetime=true;";
            _MysqlCmd = "SELECT t_data.Id,t_data.`企业全称`,t_data.`公司简介`,t_data.`经营方式`,t_data.`经营方式规范`,t_data.`所在地`,t_data.`地域规范代码`,t_area.`Name` `地域规范`,t_field.`Name` `应用领域`,t_data.`应用领域规范代码`,t_field.`Name` `应用领域规范`,t_data.`地址`,t_data.`联系人`,t_data.`电话`,t_data.`手机`,t_data.`邮箱`,t_data.`传真`,t_data.`列表图片`,t_data.`详情图片`,t_data.`主营产品`,t_data.`产品名称`,t_data.`产品图片`,t_data.URL,t_data.`企业经济性质`,t_data.`注册资金`,t_data.`主页`,t_data.`来源网站` from t_data LEFT OUTER JOIN t_area on t_area.`Code` = t_data.`地域规范代码` LEFT OUTER JOIN t_field ON t_data.`应用领域规范代码` = t_field.`Code`; ";
            _BaseDir = $"D:/Data/RecFile";
            FileName = MysqlTableName + RunDataTime.ToString("yyyyMMddHHmmss") + ".txt";
            string fileName = "KBaseDictionary.json";
            StreamReader sr = File.OpenText(fileName);
            string json = sr.ReadToEnd();
            FieldsMapperList result = JsonConvert.DeserializeObject<FieldsMapperList>(json);
            Dictionary<string, string> FieldsMappers = new Dictionary<string, string>();
            foreach(var item in result.ItemsList)
            {
                FieldsMappers.Add(item.KbaseField,item.MysqlField);
            }
            Directory.CreateDirectory(_BaseDir);
            using (var Writer = new StreamWriter(System.IO.Path.Combine(_BaseDir, FileName), false, Encoding.Default))
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
                        Writer.WriteLine("<REC>");
                        foreach (var item in FieldsMappers)
                        {
                            var i = DataFields.ToList().IndexOf(item.Value);
                            if (i < 0)
                                continue;
                            var v = Reader.IsDBNull(i) ? "" : Reader.GetString(i);
                            Writer.WriteLine($"<{item.Key}>={v}");
                        }
                    }
                }
                Console.WriteLine("生成REC文件成功！");
            }
        }

    }

    public class FieldsMapperList
    {
        public List<FieldsMapper> ItemsList = new List<FieldsMapper>();
    }

    public class FieldsMapper
    {
        public string KbaseField;
        public string MysqlField;
    }
}
