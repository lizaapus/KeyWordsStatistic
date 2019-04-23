using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using HTML = HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace KeyWordStatistic
{
    public class SaveHtmlImg
    {
        string ConnectDB = "mongodb://192.168.106.56:27017";
        string DBName = "TsinghuaEduCn";
        public void Run()
        {
            MongoClient _client = new MongoClient(ConnectDB);
            var dataBase = _client.GetDatabase(DBName);
            var htmlCollection = dataBase.GetCollection<BsonDocument>("Data");
            var Filter = Builders<BsonDocument>.Filter;
            var cursor = htmlCollection.Find(Filter.Empty).ToCursor();
            var doc = new HTML.HtmlDocument();
            int icount = 0;
            while (cursor.MoveNext())
            {
                foreach (var document in cursor.Current)
                {
                    var html = document.GetValue("Row").AsString;
                    doc.LoadHtml(html);
                    doc.DocumentNode.SelectNodes("//img").ToList().ForEach(img =>
                    {
                        var src = img.GetAttributeValue("src", "");
                        var fileNameIndex = src.LastIndexOf("/");
                        var fileName = src.Substring(fileNameIndex + 1);
                        if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                        {
                            src = "http://www.otl.tsinghua.edu.cn" + src;
                            SavelImg(src, fileName);
                            Console.CursorLeft = 0;
                            Console.Write($"保存照片数:{(icount + 1).ToString("D8")}");
                            icount++;
                        }
                    });

                }
            }
            Console.ReadKey();
        }

        public void SavelImg(string url, string fileName)
        {
            try
            {
                string desPath = @"G:\img\" + fileName;
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
