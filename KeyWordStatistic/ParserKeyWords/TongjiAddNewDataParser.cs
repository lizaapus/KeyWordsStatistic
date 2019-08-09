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
using System.Net;

namespace KeyWordStatistic
{
    public class TongjiAddNewDataParser
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";

        public TongjiAddNewDataParser()
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
                            if(name == "成果附件")
                            {
                                var nodes = div.SelectNodes(".//a");
                                foreach(var node in nodes)
                                {
                                    var src = node.GetAttributeValue("href", "");
                                    var fileNameIndex = src.LastIndexOf("/");
                                    var fileName = src.Substring(fileNameIndex + 1);
                                    if (fileName.Contains("."))
                                        SaveFile(src, fileName);
                                }
                            }
                            
                        }
                    }
                }
            }
        }
        public void SaveFile(string url, string fileName)
        {
            try
            {
                string desPath = @"G:\img\ZhzxTongjiEduCn_ContentFiles\" + fileName;
                WebClient mywebclient = new WebClient();
                mywebclient.DownloadFile(url, desPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();

            }

        }
    }
}
