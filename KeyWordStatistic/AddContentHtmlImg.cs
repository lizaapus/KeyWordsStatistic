using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Html = HtmlAgilityPack;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using HTML = HtmlAgilityPack;
using System.Net;

namespace KeyWordStatistic
{
    public class AddContentHtmlImg
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";

        public AddContentHtmlImg()
        {
            Run();
        }

        private void Run()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;
            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase("ZhzxTongjiEduCn");
            var DataCollection = DataBase.GetCollection<BsonDocument>("Data");
            var htmlCollection = DataBase.GetCollection<BsonDocument>("Html");
            var cursor = htmlCollection.Find(Filter.Empty).ToCursor();
            var doc = new HTML.HtmlDocument();

            int icount = 0;
            while (cursor.MoveNext())
            {
                foreach (var document in cursor.Current)
                {
                    var html = document.GetValue("ContentHtml").AsString;
                    var id = document["_id"].AsObjectId;
                    doc.LoadHtml(html);
                    var imgNames = string.Empty;
                    if(doc.DocumentNode.SelectNodes("//img")!=null)
                    {
                        doc.DocumentNode.SelectNodes("//img").ToList().ForEach(img =>
                        {
                            var src = img.GetAttributeValue("src", "");
                            var imgNameIndex = src.LastIndexOf("/");
                            var imgName = src.Substring(imgNameIndex + 1);
                            if (imgName.Contains("."))
                            {
                                SavelImg(src, imgName);
                                imgNames += imgName + ";";
                            }
                        });

                        UpdateDefinition<BsonDocument> updater = null;
                        updater = Update.Set("contentImgs", imgNames);

                        DataCollection.UpdateOne(Filter.Eq("_id", id), updater);
                        Console.CursorLeft = 0;
                        Console.WriteLine("修改第" + icount + "个");
                        icount++;
                    }
                }
            }
        }

        public void SavelImg(string url, string fileName)
        {
            try
            {
                string desPath = @"G:\img\ZhzxTongjiEduCn_ContentImgs\" + fileName;
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
