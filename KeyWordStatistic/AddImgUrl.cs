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
using System.Threading;

namespace KeyWordStatistic
{
    public class AddImgUrl
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        string netName = "ChinaNengyuanCom";
        public AddImgUrl()
        {
            Run();
        }

        private void Stop()
        {

        }

        private void test()
        {

        }

        private void Run()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;

            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase(netName);
            
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
                    Doc.LoadHtml(row);
                    var node = Doc.DocumentNode.SelectSingleNode("//table[4]//td[@class='fgray2']");
                    UpdateDefinition<BsonDocument> updater = null;
                    bool flag = false;
                    if (node != null)
                    {
                        flag = true;
                        var mainPro = node.InnerText.TrimHtmlDecode().Replace("主营：","");
                       updater = Update.Set("主营产品", mainPro);
                    }
                    else
                    {
                        updater = Update.Set("主营产品", "");
                    }
                    DataCollection.UpdateOne(Filter.Eq("_id", id), updater);
                    Console.CursorLeft = 0;
                    if (flag)
                        Console.WriteLine("修改第" + icount + "个");
                    else
                        Console.WriteLine("修改第" + icount + "个失败");
                    icount++;
                    //Thread.Sleep(1000);

                }
            }
            Console.Read();


            //var Doc = new Html.HtmlDocument();
            //int icount = 1;
            //while (Curosr.MoveNext())
            //{
            //    foreach (var Document in Curosr.Current)
            //    {
            //        var id = Document["_id"].AsObjectId;
            //        var row = Document["Row"].ToString();
            //        Doc.LoadHtml(row);
            //        var node = Doc.DocumentNode.SelectSingleNode("//img");
            //        UpdateDefinition<BsonDocument> updater = null;
            //        bool flag = false;
            //        if (node != null)
            //        {
            //            var imgUrl = node.GetAttributeValue("src", "");
            //            var fileNameIndex = imgUrl.LastIndexOf("/");
            //            var fileName = imgUrl.Substring(fileNameIndex + 1);
            //            if (fileName.Contains("."))
            //            {
            //                if (SavelImg(imgUrl, fileName))
            //                {
            //                    flag = true;
            //                    updater = Update.Set("列表图片", netName + @"\listImg\" + fileName).Set("详情图片", netName + @"\listImg\" + fileName);
            //                }
            //                else
            //                {
            //                    updater = Update.Set("列表图片", "").Set("详情图片", "");
            //                }
            //            }
            //        }
            //        else
            //        {
            //            updater = Update.Set("列表图片", "").Set("详情图片", "");
            //        }
            //        DataCollection.UpdateOne(Filter.Eq("_id", id), updater);
            //        Console.CursorLeft = 0;
            //        if(flag)
            //            Console.WriteLine("修改第" + icount + "个");
            //        else
            //            Console.WriteLine("修改第" + icount + "个失败");
            //        icount++;
            //        //Thread.Sleep(1000);

            //    }
            //}
        }
        public bool SavelImg(string url, string fileName)
        {
            try
            {
                string desPath = @"E:\2019\企业信息库\图片\"+ netName + @"\listImgs\" + fileName;
                WebClient mywebclient = new WebClient();
                mywebclient.Credentials = CredentialCache.DefaultCredentials; // 添加授权证书
                mywebclient.DownloadFile(url, desPath);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
                
                
                
                ////BizTouchevCom
                //try
                //{
                //    string desPath = @"E:\2019\企业信息库\图片\" + netName + @"\listImgs\" + fileName;
                //    WebClient mywebclient = new WebClient();
                //    mywebclient.Credentials = CredentialCache.DefaultCredentials; // 添加授权证书
                //    mywebclient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                //    mywebclient.Headers.Add("Host", "biz.touchev.com");
                //    mywebclient.Headers.Add("Cookie", "UM_distinctid=16bb1d9972eab2-0ec4ae521bb726-3e385b04-1fa400-16bb1d9972f9b8; PHPSESSID=upN0hwQw8FlkIm_Y7uegI45AB8qRVRDS7yq-YGrQ5o6mm6Hc_BSqQg7hNLQ6sr8x; Hm_lvt_6dba01603aa724759d9c4ea0dddd9b72=1562056956,1562816935; CNZZDATA1273105019=948668930-1562055616-%7C1562909757; Hm_lpvt_6dba01603aa724759d9c4ea0dddd9b72=1562914189");

                //    mywebclient.DownloadFile(url, desPath);
                //    return true;
                //}
                //catch(Exception e2)
                //{
                //    Console.WriteLine(e2);
                //    return false;
                //}
            }

        }

    }
}
