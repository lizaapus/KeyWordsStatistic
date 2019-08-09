using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using log4net;
using MongoDB.Bson;
using Html = HtmlAgilityPack;
using System.Linq;
using MongoDB.Bson.Serialization;
using System.Net.Http;
using System.Xml.Linq;
using JiebaNet.Analyser;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net;

namespace KeyWordStatistic
{
    public class AddcompanydetailMsg
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        protected HttpManager _HttpManager = new HttpManager();
        public AddcompanydetailMsg()
        {
            Run();
        }

        private void Run()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;

            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase("w3618MedCom");
            var DataCollection = DataBase.GetCollection<BsonDocument>("Data");
            var Curosr = DataCollection.Find(Filter.Empty).ToCursor();
            var Doc = new Html.HtmlDocument();
            int icount = 1;
            while (Curosr.MoveNext())
            {
                foreach (var Document in Curosr.Current)
                {
                    var id = Document["_id"].ToString();
                    var url = Document["Url"].ToString() ;//
                    var companyName = Document["Title"].ToString();
                    //解析该网站，获取该网站的所有产品信息并写入mysql中
                    ParseCompany(id, url, companyName);
                }
            }
        }
        
        private void ParseCompany(string id,string baseurl,string companyName)
        {
            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase("w3618MedCom");
            var _DataCollection = DataBase.GetCollection<BsonDocument>("Detail");
            int page = 1;
            int icount = 0;
            while(true){
                var url = string.Empty;
                if (page > 20)
                    break;
                if (baseurl.EndsWith(".com"))
                    url = baseurl + "/sub/products/p" + page + ".html";
                else
                    url = baseurl + "products/p" + page.ToString() + ".html";

                var Html = _HttpManager.Client.TrySend(GetRequest(url)).GetEncodedHtml(null);
                var Doc = new Html.HtmlDocument();
                List<DataEntity> dataList = new List<DataEntity>();
                Doc.LoadHtml(Html);
                if (Doc.DocumentNode.SelectNodes("//div[@class='prolist']//dl|//div[@class='index_pro_list']/ul//li|//div[@class='mainright fr']/ul//li") == null)
                    break;
                else
                {
                    var wModels = new List<WriteModel<BsonDocument>>();
                    Doc.DocumentNode.SelectNodes("//div[@class='prolist']//dl")?.ToList().ForEach(
                    tr =>
                    {
                        try
                        {
                            BsonDocument document = new BsonDocument();
                            document.Add("companyId", id);
                            document.Add("companyUrl", baseurl);
                            var node = tr.SelectSingleNode(".//a");
                            document.Add("产品名称", node.GetAttributeValue("title", ""));
                            var imgNode = tr.SelectSingleNode("./dt/a/img");
                            var imgUrl = imgNode.GetAttributeValue("src", "");
                            var fileNameIndex = imgUrl.LastIndexOf("/");
                            var fileName = imgUrl.Substring(fileNameIndex + 1);
                            if (fileName.Contains("."))
                            {
                                SavelImg(imgUrl, fileName);
                                document.Add("imgId", fileName);
                            }
                            else
                            {
                                document.Add("imgId", "");
                            }
                            document.Add("产品简介", "");
                            wModels.Add(new InsertOneModel<BsonDocument>(document));
                            icount++;
                        }
                        catch
                        {
                            Console.WriteLine("解析汇编页面失败");
                        }
                    });
                    Doc.DocumentNode.SelectNodes("//div[@class='index_pro_list']/ul//li")?.ToList().ForEach(
                    tr =>
                    {
                        try
                        {
                            BsonDocument document = new BsonDocument();
                            document.Add("companyId", id);
                            document.Add("companyUrl", baseurl);
                            var node = tr.SelectSingleNode(".//a");
                            document.Add("产品名称", node.GetAttributeValue("title",""));
                            var imgNode = tr.SelectSingleNode("./a/img");
                            var imgUrl = imgNode.GetAttributeValue("src", "");
                            var fileNameIndex = imgUrl.LastIndexOf("/");
                            var fileName = imgUrl.Substring(fileNameIndex + 1);
                            if (fileName.Contains("."))
                            {
                                SavelImg(imgUrl, fileName);
                                document.Add("imgId", fileName);
                            }
                            else
                            {
                                document.Add("imgId", "");
                            }
                            document.Add("产品简介", "");
                            wModels.Add(new InsertOneModel<BsonDocument>(document));
                            icount++;
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("解析汇编页面失败");
                        }
                    });
                   
                     Doc.DocumentNode.SelectNodes("//div[@class='mainright fr']/ul//li")?.ToList().ForEach(
                    tr =>
                    {
                        try
                        {
                            BsonDocument document = new BsonDocument();
                            document.Add("companyId", id);
                            document.Add("companyUrl", baseurl);
                            var node = tr.SelectSingleNode(".//a");
                            document.Add("产品名称", node.GetAttributeValue("title", ""));
                            var imgNode = tr.SelectSingleNode("./a/span/img");
                            var imgUrl = imgNode.GetAttributeValue("src", "");
                            var fileNameIndex = imgUrl.LastIndexOf("/");
                            var fileName = imgUrl.Substring(fileNameIndex + 1);
                            if (fileName.Contains("."))
                            {
                                SavelImg(imgUrl, fileName);
                                document.Add("imgId", fileName);
                            }
                            else
                            {
                                document.Add("imgId", "");
                            }
                            document.Add("产品简介","");
                            wModels.Add(new InsertOneModel<BsonDocument>(document));
                            icount++;
                        }
                        catch
                        {
                            Console.WriteLine("解析汇编页面失败");
                        }
                    });
                    if(wModels.Count>0)
                        _DataCollection.BulkWrite(wModels, new BulkWriteOptions { IsOrdered = false });
                    page++;
                }
                
            }

            Console.WriteLine(companyName + "   产品数：" + icount.ToString());
        }

        public HttpRequestMessage GetRequest(string Url)
        {
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.RequestUri = new Uri(Url);
            httpRequestMessage.Method = HttpMethod.Get;
            return httpRequestMessage;
        }

        public bool SavelImg(string url, string fileName)
        {
            try
            {
                string desPath = @"E:\2019\企业信息库\productImg\" + fileName;
                WebClient mywebclient = new WebClient();
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
