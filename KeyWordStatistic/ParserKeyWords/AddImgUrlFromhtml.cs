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
    public class AddImgUrlFromhtml
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        string netName = "ChinaNengyuanCom";
        public AddImgUrlFromhtml()
        {
            Run();
        }

        private void Run()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;

            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase(netName);
            var HtmlCollection = DataBase.GetCollection<BsonDocument>("Html");
            var DataCollection = DataBase.GetCollection<BsonDocument>("Data");
            var Curosr = DataCollection.Find(Filter.Empty).ToCursor();
           

            var Doc = new Html.HtmlDocument();
            int icount = 1;
            while (Curosr.MoveNext())
            {
                foreach (var Document in Curosr.Current)
                {
                    var id = Document["_id"].AsObjectId;
                    var htmlDocList = HtmlCollection.Find(Filter.Eq("_id", id));
                    if(htmlDocList.CountDocuments() !=0)
                    {
                        var htmlDoc = htmlDocList.First();
                        var row = htmlDoc["Html"].ToString();
                        Doc.LoadHtml(row);
                        var node = Doc.DocumentNode.SelectSingleNode("//img[@style='margin:0px 0 0 5px;']/@src");
                        if (node == null)
                            node = Doc.DocumentNode.SelectSingleNode("//td[@class='logo']//img/@src");
                        if (node == null)
                            node = Doc.DocumentNode.SelectSingleNode("//h3//img/@src");
                        if (node == null)
                            node = Doc.DocumentNode.SelectSingleNode("//li[@class='imglogo']//img/@src");
                        if (node == null)
                            node = Doc.DocumentNode.SelectSingleNode("//div[@class='left']//img[1]/@src");
                   

                        UpdateDefinition<BsonDocument> updater = null;
                        if (node != null)
                        {
                            var imgUrl = node.GetAttributeValue("src", "");
                            var fileNameIndex = imgUrl.LastIndexOf("/");
                            var fileName = imgUrl.Substring(fileNameIndex + 1);
                            if (fileName.Contains("."))
                            {
                                if (SavelImg(imgUrl, fileName))
                                    updater = Update.Set("列表图片", netName + @"\listImg\" + fileName).Set("详情图片", netName + @"\listImg\" + fileName);
                                else
                                    updater = Update.Set("列表图片", "").Set("详情图片", "");
                            }
                            else
                            {
                                updater = Update.Set("列表图片", "").Set("详情图片", "");
                            }
                        }
                        else
                        {
                            updater = Update.Set("列表图片", "").Set("详情图片", "");
                        }
                        DataCollection.UpdateOne(Filter.Eq("_id", id), updater);
                        Console.CursorLeft = 0;
                        Console.WriteLine("修改第" + icount + "个");
                        icount++;
                    }
                }
            }
        }
        public bool SavelImg(string url, string fileName)
        {
            try
            {
                string desPath = @"E:\2019\企业信息库\图片\"+netName+@"\listImgs\"+fileName;
                WebClient mywebclient = new WebClient();
                mywebclient.DownloadFile(url, desPath);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //Console.ReadLine();
                return false;

            }

        }

    }
}
