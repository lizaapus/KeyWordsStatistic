using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Html = HtmlAgilityPack;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.Net;

namespace KeyWordStatistic
{
    public class Enterprise_addListImg
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";

        public Enterprise_addListImg()
        {
            Run();
        }

        private void Run()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;

            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase("QichecailiaoCom");
            var DataCollection = DataBase.GetCollection<BsonDocument>("Data");
            var Curosr = DataCollection.Find(Filter.Empty).ToCursor();
            var Doc = new Html.HtmlDocument();
            int icount = 1;
            while (Curosr.MoveNext())
            {
                foreach (var Document in Curosr.Current)
                {
                    var id = Document["_id"].AsObjectId;
                    var row = Document["Row"].ToString();
                    var products = Document["主营行业"].ToString()
                        .Replace("\n",";"); 
                    Doc.LoadHtml(row);
                    var node = Doc.DocumentNode.SelectSingleNode("//img");
                    var imgUrl = node.GetAttributeValue("src", "");
                    var fileNameIndex = imgUrl.LastIndexOf("/");
                    var fileName = imgUrl.Substring(fileNameIndex + 1);
                    if (fileName.Contains("."))
                    {
                        if (!SavelImg(imgUrl, fileName))
                        {
                            fileName = "";
                        }
                    }
                    UpdateDefinition<BsonDocument> updater = null;
                    updater = Update.Set("主营行业", products)
                        .Set("rowImg", fileName)
                        .Set("detailImg",fileName);
                    DataCollection.UpdateOne(Filter.Eq("_id", id), updater);
                    Console.CursorLeft = 0;
                    Console.WriteLine("修改第" + icount + "个");
                    icount++;
                }
            }
        }
        public bool SavelImg(string url, string fileName)
        {
            string desPath = @"E:\2019\企业信息库\listImg\" + fileName;
            WebClient mywebclient = new WebClient();
            try
            {
                mywebclient.DownloadFile(url, desPath);
                return true;
            }
            catch (Exception e)
            {
                
                
                    Console.WriteLine(e);
                    return false;
                
                
            }

        }

    }
}
