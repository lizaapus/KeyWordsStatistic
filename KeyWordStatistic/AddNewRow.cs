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
    public class AddNewRow
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";

        public AddNewRow()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;

            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase("TsinghuaEduCn");
            var DataCollection = DataBase.GetCollection<BsonDocument>("Data");
            var HtmlCollection = DataBase.GetCollection<BsonDocument>("Html");
            var Curosr = DataCollection.Find(Filter.Eq("Section", "首页 > 成果汇粹 > 推广成果")).ToCursor();
            var Doc = new Html.HtmlDocument();

            var MatchDic = new ReadDic().dic();
            while (Curosr.MoveNext())
            {
                foreach (var Document in Curosr.Current)
                {
                    var id = Document["_id"].AsObjectId;
                    var url = Document["Url"];
                    if (url == "http://www.otl.tsinghua.edu.cn/info/cghc_tgcg/1608")
                    {
                        var ContentHtml = HtmlCollection.Find(Filter.Eq("_id", id)).First()["ContentHtml"].AsString;
                        Doc.LoadHtml(ContentHtml);
                        var name = string.Empty;
                        var value = string.Empty;
                        var dic = new Dictionary<string, string>();
                        foreach (var p in Doc.DocumentNode.SelectNodes("//article/p"))
                        {
                            var text = p.InnerText.TrimHtmlDecode();
                            if (MatchDic.ContainsKey(text))
                            {
                                if (!string.IsNullOrEmpty(name))
                                {
                                    if (name == "成果简介")
                                    {
                                        dic.Add("成果简介html", value);
                                        break;
                                    }
                                    value = string.Empty;
                                }
                                if (MatchDic.ContainsKey(text))
                                    name = MatchDic[text];
                                else
                                    name = "";

                                value = string.Empty;
                            }
                            else
                            {
                                p.Attributes.RemoveAll();
                                if (p.SelectNodes(".//span") != null)
                                    foreach (var span in p.SelectNodes(".//span"))
                                        span.Attributes.RemoveAll();

                                var imgNode = p.SelectSingleNode(".//img");
                                if (imgNode != null)
                                {
                                    if (imgNode.Attributes["style"] != null)
                                        imgNode.Attributes["style"].Remove();
                                    string src = imgNode.GetAttributeValue("src", "");
                                    string imgName = src.Substring(src.LastIndexOf('/'));

                                    if (imgName.Contains("."))
                                    {
                                        src = "/cast-web/upload/detail" + imgName;
                                        imgNode.SetAttributeValue("src", src);
                                    }
                                }
                                value += p.OuterHtml;
                            }
                        }

                        if (dic.Count > 0)
                        {
                            UpdateDefinition<BsonDocument> updater = null;
                            foreach (var kv in dic)
                                if (updater == null)
                                    updater = Update.Set(kv.Key, kv.Value);
                                else
                                    updater = updater.Set(kv.Key, kv.Value);
                            DataCollection.UpdateOne(Filter.Eq("_id", id), updater);
                        }
                    }

                }
            }

        }
    }
}
