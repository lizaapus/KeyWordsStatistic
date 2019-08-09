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


namespace KeyWordStatistic
{
    public class enterprise_parse
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        public enterprise_parse()
        {
            var MatchStyleDic = new ReadDic() { fileName = "managementStyle.txt" }.dic();
            var matchAreaDic = new ReadDicFromMongodb().dic();
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;

            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase("Yqk39Net");
            var DataCollection = DataBase.GetCollection<BsonDocument>("Enterprise");
            var Curosr = DataCollection.Find(Filter.Eq("地域规范", "")).ToCursor();
            var Doc = new Html.HtmlDocument();

            int icount = 0;
            while (Curosr.MoveNext())
            {
                foreach (var Document in Curosr.Current)
                {
                    var id = Document["_id"].AsObjectId;
                    var dic = new Dictionary<string, string>();
                    var locationBson = Document.GetValue("所在地","");
                    var location = string.Empty;
                    if (!locationBson.IsBsonNull)
                        location = locationBson.AsString;
                    bool flag = false;
                    if (!string.IsNullOrEmpty(location))
                    {
                        foreach (var item in matchAreaDic[0])
                        {
                            if (location.Contains(item.Key))
                            {
                                flag = true;
                                dic.Add("地域规范代码", item.Value); 
                                dic.Add("地域规范", matchAreaDic[1][item.Value]);
                                break;
                            }
                        }
                        if (!flag)
                        {
                            foreach (var item in matchAreaDic[2])
                            {

                                if (location.Substring(0, 2) == (item.Key).Substring(0, 2))
                                {
                                    flag = true;
                                    dic.Add("地域规范代码", item.Value);
                                    dic.Add("地域规范", matchAreaDic[1][item.Value]);
                                    break;
                                }
                            }
                        }
                    }
                    if(flag==false)
                    {
                        var enterpriseName = Document.GetValue("地址","").AsString;
                        if (enterpriseName.Length > 0)
                        {
                            foreach (var item in matchAreaDic[0])
                            {
                                if (enterpriseName.Contains(item.Key))
                                {
                                    flag = true;
                                    dic.Add("地域规范代码", item.Value);
                                    dic.Add("地域规范", matchAreaDic[1][item.Value]);
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                foreach (var item in matchAreaDic[2])
                                {

                                    if (enterpriseName.Substring(0, 2) == (item.Key).Substring(0, 2))
                                    {
                                        flag = true;
                                        dic.Add("地域规范代码", item.Value);
                                        dic.Add("地域规范", matchAreaDic[1][item.Value]);
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    dic.Add("地域规范代码", "");
                                    dic.Add("地域规范", "");
                                }

                            }
                        }
                         
                    }
                    var style = Document.GetValue("经营方式","").AsString; ;

                    if (!string.IsNullOrEmpty(style))
                    {
                        var str = "";
                        foreach (var item in MatchStyleDic)
                        {
                            if (style.Contains(item.Key))
                            {
                                if (!str.Contains(item.Value))
                                    str += item.Value + ";";
                            }
                        }
                        if (str == "")
                            str = "其他";
                        else
                            str = str.Substring(0, str.Length - 1);
                        dic.Add("经营方式规范", str);
                    }
                    else
                    {
                        dic.Add("经营方式规范", "其他");
                    }

                    dic.Add("应用领域", "医药健康");
                    dic.Add("应用领域规范代码", "03");
                    dic.Add("应用领域规范", "医药健康");

                    UpdateDefinition<BsonDocument> updater = null;
                    foreach (var kv in dic)
                        if (updater == null)
                            updater = Update.Set(kv.Key, kv.Value);
                        else
                            updater = updater.Set(kv.Key, kv.Value);
                    DataCollection.UpdateOne(Filter.Eq("_id", id), updater);
                    icount++;
                    Console.CursorLeft=0;
                    Console.WriteLine("修改第" + icount + "条");
                }
            }

        }
       

    }

    public class ReadDicFromMongodb
    {
        public string ConnectString = "mongodb://192.168.106.56:27017";
        public List<Dictionary<string, string>> dic()
        {
            List<Dictionary<string, string> > dicList = new List<Dictionary<string, string>>();
            Dictionary<string, string> dic1 = new Dictionary<string, string>();
            Dictionary<string, string> dic2 = new Dictionary<string, string>();
            Dictionary<string, string> dic3 = new Dictionary<string, string>();
            //Dictionary<string, string> dic3 = new Dictionary<string, string>();
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;

            var Client = new MongoClient(ConnectString);
            var DataBase = Client.GetDatabase("Test");
            var DataCollection = DataBase.GetCollection<BsonDocument>("area_data");
            var Data2Collection = DataBase.GetCollection<BsonDocument>("applydomin_data");
            var Curosr = DataCollection.Find(Filter.Eq("地区级别", "2")).ToCursor();
            //var curosr = Data2Collection.Find(Filter.Eq("层级"，"1")).ToCursor();
            while (Curosr.MoveNext())
            {
                foreach (var Document in Curosr.Current)
                {
                    var value  = Document["地区编码"].AsString;
                    
                    var key = Document["地区名"].AsString.Replace("省","").Replace("自治区","").Replace("特别行政区","").Replace("市","");

                    if (key == "广西壮族")
                        key = "广西";
                    if (key == "宁夏回族")
                        key = "宁夏";
                    if (key == "新疆维吾尔")
                        key = "新疆";
                    
                    dic1.Add(key, value);
                    dic2.Add(value,Document["地区名"].AsString);
                }
            }
            var Curosr2 = DataCollection.Find(Filter.Eq("地区级别", "3")).ToCursor();
            while (Curosr2.MoveNext())
            {
                foreach (var Document in Curosr2.Current)
                {
                    var value = Document["地区父节点"].AsString;

                    var key = Document["地区名"].AsString;
                    //if (key.Length > 2)
                    //{
                    //    key.Replace("市", "")
                    //        .Replace("州", "")
                    //        .Replace("盟","")
                    //        .Replace("群岛","")
                    //        .Replace("林区","")
                    //        .Replace("新区","")
                    //        .Replace("地区", "")
                    //        .Replace("区", "")
                    //        .Replace("县", "");
                    //}
                    //if (key == "恩施土家族苗族自治")
                    //    key = "恩施";
                    //if (key == "湘西土家族苗族自治")
                    //    key = "湘西";
                    //if (key == "延边朝鲜族自治")
                    //    key = "延边";
                    //if (key == "神农架林")
                    //    key = "神农架";
                    //if (key == "临夏回族自治")
                    //    key = "临夏回族自治州";
                    //if(key== "黄南藏族自治")
                    //    key= "黄南";

                    dic3.Add(key, value);
                }
            }
            dicList.Add(dic1);
            dicList.Add(dic2);
            dicList.Add(dic3);
            return dicList;
        }
    }
}
