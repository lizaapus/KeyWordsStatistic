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

namespace KeyWordStatistic
{
    public class TransforMongoToMongo
    {

        string _ConnectString = "mongodb://192.168.106.56:27017";
        string db = "NsfcGovCn";//新

        public void addData()
        {
           
            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase(db);
            var NewDataCollection = DataBase.GetCollection<BsonDocument>("BOIC");
            var wModels = new List<WriteModel<BsonDocument>>();
            //wModels.Add(new InsertOneModel<BsonDocument>(new BOICItem().ToBsonDocument()));
            var Data1 = new BOICItem()
            {
                Url = "http://bic.nsfc.gov.cn/Show.aspx?AI=116",
                序号="1",
                科学部编号= "1171101041",
                项目名称= "非厄米光子晶体中光场调控理论与实验研究",
                中方申请人= "董建文",
                中方申请人单位= "中山大学",
                外方申请人= "陈子亭",
                外方申请人单位= "香港科技大学"
            };
            var Data2 = new BOICItem()
            {
                Url = "http://bic.nsfc.gov.cn/Show.aspx?AI=119",
                序号 = "1",
                科学部编号 = "1171101077",
                项目名称 = "一些关键核反应的理论研究及其在火星辐射环境研究中的应用",
                中方申请人 = "任中洲",
                中方申请人单位 = "南京大学",
                外方申请人 = "张小平",
                外方申请人单位 = "澳门科技大学"
            };
            var Data3 = new BOICItem()
            {
                Url = "http://bic.nsfc.gov.cn/Show.aspx?AI=253",
                序号 = "1",
                科学部编号 = "4161101113",
                项目名称 = "地球磁场的时空结构与地球空间环境的长期变化",
                中方申请人 = "魏勇",
                中方申请人单位 = "中国科学院地质与地球物理研究所",
                外方申请人 = "张可可",
                外方申请人单位 = "澳门科技大学"
            };
            var Data4 = new BOICItem()
            {
                Url = "http://bic.nsfc.gov.cn/Show.aspx?AI=258",
                序号 = "1",
                科学部编号 = "1161101012",
                项目名称 = "求解非均匀介质中麦克斯韦方程组及相应反问题的非协调有限元方法及其收敛性分析",
                中方申请人 = "段火元",
                中方申请人单位 = "武汉大学",
                外方申请人 = "邹军",
                外方申请人单位 = "香港中文大学"
            };
            var Data5 = new BOICItem()
            {
                Url = "http://bic.nsfc.gov.cn/Show.aspx?AI=398",
                序号 = "1",
                科学部编号 = "1151101091",
                项目名称 = "旋转热对流中涡旋结构的几何特性、涡动力学及小尺度统计规律的实验研究",
                中方申请人 = "钟锦强",
                中方申请人单位 = "同济大学",
                外方申请人 = "夏克青",
                外方申请人单位 = "香港中文大学"
            };
            var Data6 = new BOICItem()
            {
                Url = "http://bic.nsfc.gov.cn/Show.aspx?AI=511",
                序号 = "1",
                科学部编号 = "1141101053",
                项目名称 = "稀疏优化算法与理论",
                中方申请人 = "袁亚湘",
                中方申请人单位 = "中国科学院数学与系统科学研",
                外方申请人 = "陈小君",
                外方申请人单位 = "香港理工大学"
            };
            wModels.Add(new InsertOneModel<BsonDocument>(Data1.ToBsonDocument()));
            wModels.Add(new InsertOneModel<BsonDocument>(Data2.ToBsonDocument()));
            wModels.Add(new InsertOneModel<BsonDocument>(Data3.ToBsonDocument()));
            wModels.Add(new InsertOneModel<BsonDocument>(Data4.ToBsonDocument()));
            wModels.Add(new InsertOneModel<BsonDocument>(Data5.ToBsonDocument()));
            wModels.Add(new InsertOneModel<BsonDocument>(Data6.ToBsonDocument()));

            NewDataCollection.BulkWrite(wModels, new BulkWriteOptions { IsOrdered = false });
               
            wModels.Clear();
           




        }
        public TransforMongoToMongo()
        {
            //addData();
            var Filter = Builders<BsonDocument>.Filter;
            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase(db);
            var DataCollection1 = DataBase.GetCollection<BsonDocument>("Temp");
            var NewDataCollection = DataBase.GetCollection<BsonDocument>("BOIC");
            var Curosr = DataCollection1.Find(Filter.Empty).ToCursor();

            var MatchDic = new ReadDic() { fileName = "国际合作局字段对应.txt", path = @"E:\2019\科研项目\", }.dic();

            var wModels = new List<WriteModel<BsonDocument>>();
            int icount = 1;
            while (Curosr.MoveNext())
            {
                foreach (var doc in Curosr.Current)
                {
                    var Data = new BOICItem();
                    foreach (var ele in doc.Elements)
                    {
                        switch (MatchDic[ele.Name])
                        {
                            case "Url":
                                Data.Url += ele.Value;
                                break;
                            case "序号":
                                Data.序号 += ele.Value;
                                break;
                            case "编号":
                                Data.编号 += ele.Value;
                                break;
                            case "项目编号":
                                Data.项目编号 += ele.Value;
                                break;
                            case "港方编号":
                                Data.港方编号 += ele.Value;
                                break;
                            case "科学部编号":
                                Data.科学部编号 += ele.Value;
                                break;
                            case "科学部受理号":
                                Data.科学部受理号 += ele.Value;
                                break;
                            case "内地编号":
                                Data.内地编号 += ele.Value;
                                break;
                            case "项目名称":
                                Data.项目名称 += ele.Value;
                                break;
                            case "中方申请人":
                                Data.中方申请人 += ele.Value;
                                break;
                            case "中方申请人单位":
                                Data.中方申请人单位 += ele.Value;
                                break;
                            case "中方申请人及单位":
                                Data.中方申请人及单位 += ele.Value;
                                break;
                            case "外方申请人":
                                Data.外方申请人 += ele.Value;
                                break;
                            case "外方申请人单位":
                                Data.外方申请人单位 += ele.Value;
                                break;
                            case "外方申请人及单位":
                                Data.外方申请人及单位 += ele.Value;
                                break;
                            case "中外申请单位":
                                Data.中外申请单位 += ele.Value;
                                break;
                        }
                    }
                    wModels.Add(new InsertOneModel<BsonDocument>(Data.ToBsonDocument()));
                    if (icount % 100 == 0)
                    {
                        var Sucessed = wModels.Count;
                        try
                        {
                            NewDataCollection.BulkWrite(wModels, new BulkWriteOptions { IsOrdered = false });
                        }
                        catch (MongoBulkWriteException ex)
                        {
                            Sucessed = Sucessed - ex.WriteErrors.Count;
                        }
                        catch (Exception ex)
                        {
                            Sucessed = 0;
                        }
                        Console.WriteLine("成功写入" + $" {Sucessed}:{icount}条数据\n");
                        wModels.Clear();
                    }
                    icount++;
                }
            }

            var Sucessed2 = wModels.Count;
            try
            {
                NewDataCollection.BulkWrite(wModels, new BulkWriteOptions { IsOrdered = false });
            }
            catch (MongoBulkWriteException ex)
            {
                Sucessed2 = Sucessed2 - ex.WriteErrors.Count;
            }
            catch (Exception ex)
            {
                Sucessed2 = 0;
            }
            Console.WriteLine("成功写入" + $" {Sucessed2}:{wModels.Count()}条数据\n");
            wModels.Clear();
            Console.ReadKey();
        }
       
    }

    public class BOICItem
    {
        public string Url { get; set; } = string.Empty;
        public string 序号 { get; set; } = string.Empty;
        public string 编号 { get;set; } = string.Empty;
        public string 项目编号 { get; set; } = string.Empty;
        public string 港方编号 { get; set; } = string.Empty;
        public string 科学部编号 { get; set; } = string.Empty;
        public string 科学部受理号 { get; set; } = string.Empty;
        public string 内地编号 { get; set; } = string.Empty;
        public string 项目名称 { get; set; } = string.Empty;
        public string 中方申请人 { get; set; } = string.Empty;
        public string 中方申请人单位 { get; set; } = string.Empty;
        public string 中方申请人及单位 { get; set; } = string.Empty;
        public string 外方申请人 { get; set; } = string.Empty;
        public string 外方申请人单位 { get; set; } = string.Empty;
        public string 外方申请人及单位 { get; set; } = string.Empty;
        public string 中外申请单位 { get; set; } = string.Empty;
        public string 中外申请人 { get; set; } = string.Empty;
        public string 资助额度 { get; set; } = string.Empty;
        public string 外方资助经费 { get; set; } = string.Empty;
        public string 项目执行日期 { get; set; } = string.Empty;
        public string 学科代码 { get; set; } = string.Empty;
        public string 学科分类 { get; set; } = string.Empty;
        public string 职称 { get; set; } = string.Empty;
        public string 申请类别 { get; set; } = string.Empty;
        public string 类别 { get; set; } = string.Empty;
        public string 国籍 { get; set; } = string.Empty;
    }
}
