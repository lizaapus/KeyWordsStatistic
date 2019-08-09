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

namespace KeyWordStatistic
{
    public class MergeTwoMongodb
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        string db1 = "OutputNsfcGovCn";//新
        string db2 = "NpdNsfcGovCn";//旧
        public MergeTwoMongodb()
        {
            var Filter1 = Builders<BsonDocument>.Filter;
            var Client1 = new MongoClient(_ConnectString);
            var DataBase1 = Client1.GetDatabase(db1);
            var DataCollection1 = DataBase1.GetCollection<BsonDocument>("Project");
            var NewDataCollection = DataBase1.GetCollection<BsonDocument>("MergeProject");
            var Curosr1 = DataCollection1.Find(Filter1.Empty).ToCursor();

            var Filter2 = Builders<BsonDocument>.Filter;
            var Client2 = new MongoClient(_ConnectString);
            var DataBase2 = Client2.GetDatabase(db2);
            var DataCollection2 = DataBase2.GetCollection<BsonDocument>("Project");

            Dictionary<string, string> applyDic = GetApplyDic();




            var wModels = new List<WriteModel<BsonDocument>>();

            int icount = 0;
            while (Curosr1.MoveNext())
            {
                foreach (var newDoc in Curosr1.Current)
                {
                    var numb = newDoc.GetValue("批准号").AsString;
                    BsonDocument oldDoc = new BsonDocument();
                    bool isExist = false;
                    try
                    {
                        oldDoc = DataCollection2.Find(Filter2.Eq("批准号", numb)).First();
                        isExist = true;
                    }
                    catch (Exception e){}
                    if (isExist)
                    {
                        var applycode = string.IsNullOrEmpty(oldDoc.GetValue("申请代码", string.Empty).AsString) ? newDoc.GetValue("申请代码", string.Empty).AsString : oldDoc.GetValue("申请代码", string.Empty).AsString;
                        var applyName = string.Empty;
                        if (applyDic.ContainsKey(applycode))
                            applyName = applyDic[applycode];

                        var time = string.IsNullOrEmpty(oldDoc.GetValue("研究期限", string.Empty).AsString) ? newDoc.GetValue("研究期限", string.Empty).AsString : oldDoc.GetValue("研究期限", string.Empty).AsString;
                        DateTime startTime = new DateTime();
                        DateTime endTime = new DateTime();
                        if (!string.IsNullOrEmpty(time))
                        {
                            var timeList = time.Split('到');
                            if (timeList.Count() >= 2)
                            {
                                if (!DateTime.TryParse(timeList[0], out startTime))
                                    startTime = DateTime.MinValue;
                                if (!DateTime.TryParse(timeList[1], out endTime))
                                    endTime = DateTime.MinValue;
                            }
                        }
                        var Data = new NewDataEntity
                        {
                            Url = newDoc.GetValue("Url").AsString,
                            批准号 = newDoc.GetValue("批准号").AsString,
                            项目名称 = string.IsNullOrEmpty(oldDoc.GetValue("项目名称", string.Empty).AsString) ? newDoc.GetValue("项目名称", string.Empty).AsString : oldDoc.GetValue("项目名称", string.Empty).AsString,
                            项目类别 = string.IsNullOrEmpty(oldDoc.GetValue("项目类别", string.Empty).AsString) ? newDoc.GetValue("项目类别", string.Empty).AsString : oldDoc.GetValue("项目类别", string.Empty).AsString,
                            资助类别代码 = string.IsNullOrEmpty(oldDoc.GetValue("资助类别代码", string.Empty).AsString) ? newDoc.GetValue("资助类别代码", string.Empty).AsString : oldDoc.GetValue("资助类别代码", string.Empty).AsString,
                            申请代码 = applycode,
                            申请名称 = applyName,
                            项目负责人 = string.IsNullOrEmpty(oldDoc.GetValue("项目负责人", string.Empty).AsString) ? newDoc.GetValue("项目负责人", string.Empty).AsString : oldDoc.GetValue("项目负责人", string.Empty).AsString,
                            负责人职称 = string.IsNullOrEmpty(oldDoc.GetValue("负责人职称", string.Empty).AsString) ? newDoc.GetValue("负责人职称", string.Empty).AsString : oldDoc.GetValue("负责人职称", string.Empty).AsString,
                            依托单位 = string.IsNullOrEmpty(oldDoc.GetValue("依托单位", string.Empty).AsString) ? newDoc.GetValue("依托单位", string.Empty).AsString : oldDoc.GetValue("依托单位", string.Empty).AsString,
                            资助经费 = string.IsNullOrEmpty(oldDoc.GetValue("资助经费", string.Empty).AsString) ? newDoc.GetValue("资助经费", string.Empty).AsString : oldDoc.GetValue("资助经费", string.Empty).AsString,
                            批准年度 = string.IsNullOrEmpty(oldDoc.GetValue("批准年度", string.Empty).AsString) ? newDoc.GetValue("批准年度", string.Empty).AsString : oldDoc.GetValue("批准年度", string.Empty).AsString,
                            关键词 = string.IsNullOrEmpty(oldDoc.GetValue("关键词", string.Empty).AsString) ? newDoc.GetValue("关键词", string.Empty).AsString : oldDoc.GetValue("关键词", string.Empty).AsString,
                            是否结题 = string.IsNullOrEmpty(oldDoc.GetValue("是否结题", string.Empty).AsString) ? newDoc.GetValue("是否结题", string.Empty).AsString : oldDoc.GetValue("是否结题", string.Empty).AsString,
                            研究期限开始 = startTime,
                            研究期限结束 = endTime,
                            支持经费 = string.IsNullOrEmpty(oldDoc.GetValue("项目负责人", string.Empty).AsString) ? newDoc.GetValue("项目负责人", string.Empty).AsString : oldDoc.GetValue("项目负责人", string.Empty).AsString,
                            项目摘要中文 = string.IsNullOrEmpty(oldDoc.GetValue("项目摘要中文", string.Empty).AsString) ? newDoc.GetValue("项目摘要中文", string.Empty).AsString : oldDoc.GetValue("项目摘要中文", string.Empty).AsString,
                            项目摘要英文 = string.IsNullOrEmpty(oldDoc.GetValue("项目摘要英文", string.Empty).AsString) ? newDoc.GetValue("项目摘要英文", string.Empty).AsString : oldDoc.GetValue("项目摘要英文", string.Empty).AsString,
                            结题摘要 = string.IsNullOrEmpty(oldDoc.GetValue("结题摘要", string.Empty).AsString) ? newDoc.GetValue("结题摘要", string.Empty).AsString : oldDoc.GetValue("结题摘要", string.Empty).AsString
                        };



                        wModels.Add(new InsertOneModel<BsonDocument>(Data.ToBsonDocument()));

                    }
                    else
                    {
                        var applycode = newDoc.GetValue("申请代码", string.Empty).AsString ;
                        var applyName = string.Empty;
                        if (applyDic.ContainsKey(applycode))
                            applyName = applyDic[applycode];

                        var time =newDoc.GetValue("研究期限", string.Empty).AsString;
                        DateTime startTime = new DateTime();
                        DateTime endTime = new DateTime();
                        if (!string.IsNullOrEmpty(time))
                        {
                            var timeList = time.Split('到');
                            if (timeList.Count() >= 2)
                            {
                                if (!DateTime.TryParse(timeList[0], out startTime))
                                    startTime = DateTime.MinValue;
                                if (!DateTime.TryParse(timeList[1], out endTime))
                                    endTime = DateTime.MinValue;
                            }
                        }
                        var Data = new NewDataEntity
                        {
                            Url = newDoc.GetValue("Url").AsString,
                            批准号 = newDoc.GetValue("批准号").AsString,
                            项目名称 = newDoc.GetValue("项目名称", string.Empty).AsString,
                            项目类别 = newDoc.GetValue("项目类别", string.Empty).AsString,
                            资助类别代码 =newDoc.GetValue("资助类别代码", string.Empty).AsString,
                            申请代码 = applycode,
                            申请名称 = applyName,
                            项目负责人 = newDoc.GetValue("项目负责人", string.Empty).AsString,
                            负责人职称 = newDoc.GetValue("负责人职称", string.Empty).AsString ,
                            依托单位 = newDoc.GetValue("依托单位", string.Empty).AsString,
                            资助经费 =newDoc.GetValue("资助经费", string.Empty).AsString,
                            批准年度 =newDoc.GetValue("批准年度", string.Empty).AsString,
                            关键词 = newDoc.GetValue("关键词", string.Empty).AsString,
                            是否结题 = newDoc.GetValue("是否结题", string.Empty).AsString,
                            研究期限开始 = startTime,
                            研究期限结束 = endTime,
                            支持经费 = newDoc.GetValue("项目负责人", string.Empty).AsString,
                            项目摘要中文 = newDoc.GetValue("项目摘要中文", string.Empty).AsString,
                            项目摘要英文 = newDoc.GetValue("项目摘要英文", string.Empty).AsString,
                            结题摘要 =  newDoc.GetValue("结题摘要", string.Empty).AsString
                        };



                        wModels.Add(new InsertOneModel<BsonDocument>(Data.ToBsonDocument()));
                    }

                    icount++;
                    if (icount % 100 == 0)
                    {
                        NewDataCollection.BulkWrite(wModels, new BulkWriteOptions { IsOrdered = false });
                        Console.WriteLine("已写入MongoDB-：" + $" {icount}条数据\n");
                        wModels.Clear();
                    }



                }
            }


        }
        public Dictionary<string, string> GetApplyDic()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            string path = @"C:\Users\Myn\Desktop\applyCode.json";
            string json = File.ReadAllText(path);
            Applyroot result = JsonConvert.DeserializeObject<Applyroot>(json);
            foreach(var item in result.data)
            {
                dic.Add(item.code, item.name);
            }
            return dic;
        }
    }
    public class NewDataEntity
    {
        public string Url { get; set; }
        public string 批准号 { get; set; }
        public string 项目名称 { get; set; }
        public string 项目类别 { get; set; }
        public string 资助类别代码 { get; set; }
        public string 申请代码 { get; set; }
        public string 申请名称 { get; set; }
        public string 项目负责人 { get; set; }
        public string 负责人职称 { get; set; }
        public string 依托单位 { get; set; }
        public string 资助经费 { get; set; }
        public string 批准年度 { get; set; }
        public string 关键词 { get; set; }
        public string 是否结题 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 研究期限开始 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 研究期限结束 { get; set; }

        public string 支持经费 { get; set; }
        public string 项目摘要中文 { get; set; }
        public string 项目摘要英文 { get; set; }
        public string 结题摘要 { get; set; }
    }


    public class DataItem
    {
        /// <summary>
        /// 数理科学部
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string code { get; set; }
    }

    public class Applyroot
    {
        /// <summary>
        /// 
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<DataItem> data { get; set; }
    }
}
