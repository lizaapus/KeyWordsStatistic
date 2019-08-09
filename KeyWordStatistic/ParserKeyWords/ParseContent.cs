using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Html = HtmlAgilityPack;

namespace KeyWordStatistic
{
    public class ParseContent
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        public ParseContent()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;
            var Dic = new Dictionary<string, int>();
            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase("ZhzxTongjiEduCn");
            var DataCollection = DataBase.GetCollection<BsonDocument>("Teacher");
            var HtmlCollection = DataBase.GetCollection<BsonDocument>("Html");
            var Curosr = DataCollection.Find(Filter.Empty).ToCursor();
            var Doc = new Html.HtmlDocument();
            while (Curosr.MoveNext())
            {
                foreach (var Document in Curosr.Current)
                {
                    UpdateDefinition<BsonDocument> updaterData = null;
                    //UpdateDefinition<BsonDocument> updaterHtml = null;

                    var id = Document["_id"].AsObjectId;
                    var ContentHtml = HtmlCollection.Find(Filter.Eq("_id", id)).First()["ContentHtml"].AsString;
                    Doc.LoadHtml(ContentHtml);
                    var value = Doc.DocumentNode.InnerText.TrimHtmlDecode();
                    string fileNames = string.Empty;
                    if(Doc.DocumentNode.SelectNodes("//img")!=null)
                    {
                        Doc.DocumentNode.SelectNodes("//img").ToList().ForEach(img =>
                        {
                            var src = img.GetAttributeValue("src", "");
                            var fileNameIndex = src.LastIndexOf("/");
                            var fileName = src.Substring(fileNameIndex + 1);
                            if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                            {
                                fileNames += fileName + ";";
                                //SavelImg(src, fileName);
                            }
                        });
                        updaterData = Update.Set("ContentImgs", fileNames);
                        DataCollection.UpdateOne(Filter.Eq("_id", id), updaterData);
                    }
                    //updaterHtml = Update.Set("Content", value);
                   // updaterData = Update.Set("Content", value);
                   
                    //HtmlCollection.UpdateOne(Filter.Eq("_id", id), updaterHtml);
                }
            }

     
        }
        public void SavelImg(string url, string fileName)
        {
            try
            {
                string desPath = @"G:\img\同济大学\ZhzxTongjiEduCn_TeacherContentImgs\" + fileName;
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
