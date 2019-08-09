﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Html = HtmlAgilityPack;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;

namespace KeyWordStatistic
{ 
    public class tongjCGJJHtml
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        public tongjCGJJHtml()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;

            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase("ZhzxTongjiEduCn");
            var DataCollection = DataBase.GetCollection<BsonDocument>("Data");
            var HtmlCollection = DataBase.GetCollection<BsonDocument>("Html");
            var Curosr = DataCollection.Find(Filter.Empty).ToCursor();
            var Doc = new Html.HtmlDocument();

            var MatchDic = new ReadDic().dic();
            while (Curosr.MoveNext())
            {
                foreach (var Document in Curosr.Current)
                {
                    var id = Document["_id"].AsObjectId;
                    var url = Document["Url"];

                    var ContentHtml = HtmlCollection.Find(Filter.Eq("_id", id)).First()["ContentHtml"].AsString;
                    Doc.LoadHtml(ContentHtml);
                    var dic = new Dictionary<string, string>();
                    foreach (var div in Doc.DocumentNode.SelectNodes("//div[@class='cg_info']/div"))
                    {
                        var span = div.SelectSingleNode(".//span[@class='effect']");
                        if (span != null)
                        {
                            var name = span.InnerText.TrimHtmlDecode();
                            if (name == "成果简介")
                            {
                                if (div.Attributes["style"] != null)
                                    div.Attributes["style"].Remove();
                                if (div.Attributes["class"] != null)
                                    div.Attributes["class"].Remove();
                                foreach (var child in div.ChildNodes)
                                    RemoveStyle(child);
                                var value = div.OuterHtml;
                                dic.Add("成果简介html", value);
                            }
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

        private void RemoveStyle(Html.HtmlNode node)
        {
            if (node.Attributes["style"] != null)
                node.Attributes["style"].Remove();
            if(node.Attributes["class"]!=null)
                node.Attributes["class"].Remove();
            foreach (var child in node.ChildNodes)
                RemoveStyle(child);
        }
    }
}
