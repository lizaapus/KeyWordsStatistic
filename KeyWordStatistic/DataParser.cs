using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Html = HtmlAgilityPack;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace KeyWordStatistic
{
    public class DataParser
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";

        public void Parese()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;

            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase("TsinghuaEduCn");
            var DataCollection = DataBase.GetCollection<BsonDocument>("Data");
            var HtmlCollection = DataBase.GetCollection<BsonDocument>("Html");
            var Curosr = DataCollection.Find(Filter.Eq("Section", "首页 > 成果汇粹 > 推广成果")).ToCursor();
            var Doc = new Html.HtmlDocument();
            var Columns = new List<string>
            {
                "成果简介",
                "应用前景",
                "知识产权",
                "团队介绍",
                "合作方式",
                "联系方式-电话",
                "联系方式-邮箱",
                "成果编号",
            };
            while (Curosr.MoveNext())
            {
                foreach (var Document in Curosr.Current)
                {
                    var id = Document["_id"].AsObjectId;
                    var ContentHtml = HtmlCollection.Find(Filter.Eq("_id", id)).First()["ContentHtml"].AsString;
                    Doc.LoadHtml(ContentHtml);
                    var name = string.Empty;
                    var value = string.Empty;
                    var dic = new Dictionary<string, string>();
                    foreach (var p in Doc.DocumentNode.SelectNodes("//article/p"))
                    {
                        var text = p.InnerText.TrimHtmlDecode();
                        var mc = Regex.Match(text, "^\\d+\\s*[\\.．]\\s*(.+)");
                        if (mc.Success || text.StartsWith("成果编号"))
                        {
                            if (!string.IsNullOrEmpty(name))
                            {
                                if (name == "联系方式")
                                {
                                    dic.Add(name, value);
                                    var lxmc = Regex.Match(value, @"电话\s*[:：](.+)邮箱\s*[:：](.+)",RegexOptions.Singleline);
                                    if (lxmc.Success)
                                    {
                                        dic.Add("联系方式-电话", lxmc.Groups[1].Value);
                                        dic.Add("联系方式-邮箱", lxmc.Groups[2].Value);
                                    }
                                }
                                else if (name == "成果编号")
                                {
                                    var cnmc = Regex.Match(value, "\\d+");
                                    if (cnmc.Success)
                                    {
                                        value = cnmc.Value;
                                    }
                                    else
                                    {
                                        value = string.Empty;
                                    }
                                    dic.Add(name, value);
                                }
                                else
                                {
                                    dic.Add(name, value);
                                }
                                value = string.Empty;
                            }
                            name =text;
                            if (name.Contains("成果简介"))
                                name = "成果简介";
                            else if (name.Contains("应用前景"))
                                name = "应用前景";
                            else if (name.Contains("知识产权"))
                                name = "知识产权";
                            else if (name.Contains("团队介绍"))
                                name = "团队介绍";
                            else if (name.Contains("合作方式"))
                                name = "合作方式";
                            else if (name.Contains("联系"))
                                name = "联系方式";
                            else if (name.Contains("成果编号"))
                            {
                                name = "成果编号";
                                var cnmc = Regex.Match(text, "\\d+");
                                if (cnmc.Success)
                                {
                                    value = cnmc.Value;
                                }
                                else
                                {
                                    value = string.Empty;
                                }
                                dic.Add(name, value);
                            }
                            else
                                name = "";

                            value = string.Empty;
                        }
                        else
                            value+= text + "\r\n";
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
