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
using Newtonsoft.Json;
using log4net;
using MongoDB.Bson.Serialization;
using System.Net.Http;
using System.Xml.Linq;
using JiebaNet.Analyser;
using Newtonsoft.Json.Linq;

namespace KeyWordStatistic
{
    public class AddProductsImgFromHtml
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        string netName = "ChinaNengyuanCom";
        protected HttpManager _HttpManager = new HttpManager();
        public AddProductsImgFromHtml()
        {
            Run();
        }

        public bool SavelImg(string url, string fileName)
        {
            try
            {
                string desPath = @"E:\2019\企业信息库\图片\"+netName+@"\productsImgs\" + fileName;
                WebClient mywebclient = new WebClient();
               
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
                //    string desPath = @"E:\2019\企业信息库\图片\" + netName + @"\productsImgs\" + fileName;
                //    WebClient mywebclient = new WebClient();
                //    mywebclient.Credentials = CredentialCache.DefaultCredentials; // 添加授权证书
                //    mywebclient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                //    mywebclient.Headers.Add("Host", "biz.touchev.com");
                //    mywebclient.Headers.Add("Cookie", "UM_distinctid=16bb1d9972eab2-0ec4ae521bb726-3e385b04-1fa400-16bb1d9972f9b8; PHPSESSID=upN0hwQw8FlkIm_Y7uegI45AB8qRVRDS7yq-YGrQ5o6mm6Hc_BSqQg7hNLQ6sr8x; Hm_lvt_6dba01603aa724759d9c4ea0dddd9b72=1562056956,1562816935; CNZZDATA1273105019=948668930-1562055616-%7C1562909757; Hm_lpvt_6dba01603aa724759d9c4ea0dddd9b72=1562914189");
                //    mywebclient.DownloadFile(url, desPath);
                //    return true;
                //}catch(Exception e2)
                //{
                //    Console.WriteLine(e2);
                //    return false;
                //}
            }
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
                    var url = Document["Url"].AsString;

                    var productsNames = "";
                    List<productImg> productsImgs = new List<productImg>();
                    var htmlDocList = HtmlCollection.Find(Filter.Eq("_id", id));
                    UpdateDefinition<BsonDocument> updater = null;
                    var description = string.Empty;

                    //////ChinaNengyuanCom
                    //if (htmlDocList.CountDocuments() != 0)
                    //{
                    //    var htmlDoc = htmlDocList.First();
                    //    var html = htmlDoc["Html"].ToString();
                    //    Doc.LoadHtml(html);
                    //    if (Doc.DocumentNode.SelectNodes("//table[@class='bord top10']//tr/td[2]") != null)
                    //    {
                    //        Regex regex = new Regex(@"主营产品：(.*?)</td>");
                    //        Match match = regex.Match(html);
                    //        if (match.Success)
                    //        {
                    //            description = match.Groups[1].Value;
                    //        }
                    //        Doc.DocumentNode.SelectNodes("//table[@class='list']//div//a")?.ToList().ForEach(node =>
                    //        {
                    //            var imgNode = node.SelectSingleNode(".//img");
                    //            var src = imgNode.GetAttributeValue("src", "");

                    //            var fileNameIndex = src.LastIndexOf("/");
                    //            var fileName = src.Substring(fileNameIndex + 1);
                    //            var productName = node.GetAttributeValue("title", "");
                    //            if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                    //            {
                    //                productsNames += productName + ";";
                    //                if (SavelImg(src, fileName))
                    //                {
                    //                    productImg item = new productImg()
                    //                    {
                    //                        name = productName,
                    //                        path = netName + @"\productsImgs\" + fileName
                    //                    };
                    //                    productsImgs.Add(item);
                    //                }
                    //            }
                    //        });
                    //    }
                    //   else if (Doc.DocumentNode.SelectNodes("//table[@style='margin-top:10px']//td") != null)
                    //    {

                    //        Regex regex = new Regex(@"主营产品：(.*?)<br");
                    //        Match match = regex.Match(html);
                    //        if (match.Success)
                    //        {
                    //            description = match.Groups[1].Value;
                    //        }
                    //        Doc.DocumentNode.SelectNodes("//td[@width='139']//table//td//a")?.ToList().ForEach(node =>
                    //        {
                    //            if (node.SelectSingleNode(".//img") != null)
                    //            {
                    //                var imgNode = node.SelectSingleNode(".//img");
                    //                var src = imgNode.GetAttributeValue("src", "");
                    //                var fileNameIndex = src.LastIndexOf("/");
                    //                var fileName = src.Substring(fileNameIndex + 1);
                    //                var productName = node.GetAttributeValue("title", "");
                    //                if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                    //                {
                    //                    productsNames += productName + ";";
                    //                    if (SavelImg(src, fileName))
                    //                    {
                    //                        productImg item = new productImg()
                    //                        {
                    //                            name = productName,
                    //                            path = netName + @"\productsImgs\" + fileName
                    //                        };
                    //                        productsImgs.Add(item);
                    //                    }
                    //                }
                    //            }

                    //        });
                    //    }

                    //   else if (Doc.DocumentNode.SelectNodes("//table[@width='92%']//td[@width='160']//a") != null)
                    //    {
                    //        Regex regex = new Regex(@"主营产品：(.*?)<br");
                    //        Match match = regex.Match(html);
                    //        if (match.Success)
                    //        {
                    //            description = match.Groups[1].Value;
                    //        }
                    //        Doc.DocumentNode.SelectNodes("//td[@width='139']//table//td//a")?.ToList().ForEach(node =>
                    //        {
                    //            var productName = node.GetAttributeValue("title", "");
                    //            productsNames += productName + ";";
                    //        });
                    //    }
                    //    else
                    //    {

                    //    }
                    //    updater = Update.Set("产品列表", productsNames)
                    //       .Set("产品名称", productsNames)
                    //       .Set("主营产品", description)
                    //       .Set("产品图片", JsonConvert.SerializeObject(productsImgs));
                    //    DataCollection.UpdateOne(Filter.Eq("_id", id), updater);

                    //    Console.CursorLeft = 0;
                    //    Console.Write($"更新第:{(icount + 1).ToString("D8")}条");
                    //    icount++;
                    //}



                    ////////BizTouchevCom
                    //var Doc = new Html.HtmlDocument();
                    //int icount = 1;
                    //while (Curosr.MoveNext())
                    //{
                    //    foreach (var Document in Curosr.Current)
                    //    {
                    //        var id = Document["_id"].AsObjectId;
                    //        var url = Document["Url"].AsString;

                    //        var productsNames = "";
                    //        List<productImg>productsImgs  = new List<productImg>();
                    //        var htmlDocList = HtmlCollection.Find(Filter.Eq("_id", id));
                    //        UpdateDefinition<BsonDocument> updater = null;
                    //        var discription = string.Empty;

                    //        //BizTouchevCom
                    //        if (htmlDocList.CountDocuments() != 0)
                    //        {
                    //            var htmlDoc = htmlDocList.First();
                    //            var row = htmlDoc["Html"].ToString();
                    //            Doc.LoadHtml(row);
                    //            if (Doc.DocumentNode.SelectNodes("//h2/a/text()") != null)
                    //            {
                    //                //discription = Doc.DocumentNode.SelectSingleNode("//li[@class='table0']//table//tr[2]//td[2]")?.InnerText;
                    //                Doc.DocumentNode.SelectNodes("//ul[@class='new_cp']/li//img/@alt")?.ToList().ForEach(node =>
                    //                {
                    //                    var src = node.GetAttributeValue("src", "");
                    //                    if (!src.Contains("http:"))
                    //                        src = "http:" + src;
                    //                    var fileNameIndex = src.LastIndexOf("/");
                    //                    var fileName = src.Substring(fileNameIndex + 1);
                    //                    var productName = node.GetAttributeValue("alt", "");
                    //                    if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                    //                    {
                    //                        productsNames += productName + ";";
                    //                        if (SavelImg(src, fileName))
                    //                        { 
                    //                            productImg item = new productImg()
                    //                            {
                    //                                name = productName,
                    //                                path = netName + @"\productsImgs\" + fileName
                    //                            };
                    //                            productsImgs.Add(item);
                    //                        }
                    //                    }
                    //                });
                    //            }
                    //            else if (Doc.DocumentNode.SelectNodes("//div[@class='chpzhsh-img']//img") != null)
                    //            {
                    //                Doc.DocumentNode.SelectNodes("//div[@class='chpzhsh-img']//img").ToList().ForEach(node =>
                    //                {
                    //                    var src = node.GetAttributeValue("src", "");
                    //                    if (!src.Contains("http:"))
                    //                        src = "http:" + src;
                    //                    var fileNameIndex = src.LastIndexOf("/");
                    //                    var fileName = src.Substring(fileNameIndex + 1);
                    //                    var productName = node.GetAttributeValue("alt", "");
                    //                    if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                    //                    {
                    //                        productsNames += productName + ";";
                    //                        if (SavelImg(src, fileName))
                    //                        {
                    //                            productImg item = new productImg()
                    //                            {
                    //                                name = productName,
                    //                                path = netName + @"\productsImgs\" + fileName
                    //                            };
                    //                            productsImgs.Add(item);
                    //                        }
                    //                    }
                    //                });
                    //            }
                    //            else if (Doc.DocumentNode.SelectNodes("//td[@class='company_name']//h1/text()") != null)
                    //            {

                    //                var Html = _HttpManager.Client.TrySend(GetRequest(url+ "-prolist")).GetEncodedHtml(null);
                    //                var productDoc = new Html.HtmlDocument();
                    //                productDoc.LoadHtml(Html);
                    //                productDoc.DocumentNode.SelectNodes("//div[@class='com_proimg']//img")?.ToList().ForEach(node =>
                    //                {
                    //                        var src = node.GetAttributeValue("src", "");
                    //                        src = "http://biz.touchev.com/" + src;
                    //                        var fileNameIndex = src.LastIndexOf("/");
                    //                        var fileName = src.Substring(fileNameIndex + 1);
                    //                        var productName = node.GetAttributeValue("alt", "");
                    //                        if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                    //                        {
                    //                            productsNames += productName + ";";
                    //                            if (SavelImg(src, fileName))
                    //                            {
                    //                                productImg item = new productImg()
                    //                                {
                    //                                    name = productName,
                    //                                    path = netName + @"\productsImgs\" + fileName
                    //                                };
                    //                                productsImgs.Add(item);
                    //                            }
                    //                        }
                    //                });

                    //            }
                    //            else if (Doc.DocumentNode.SelectNodes("//ul[@class='gysxx']//h3/text()") != null)
                    //            {
                    //                var Html = _HttpManager.Client.TrySend(GetRequest(url + "-prolist")).GetEncodedHtml(null);
                    //                var productDoc = new Html.HtmlDocument();

                    //                productDoc.LoadHtml(Html);
                    //                productDoc.DocumentNode.SelectNodes("//div[@class='blank20']/li//img")?.ToList().ForEach(node =>
                    //                {
                    //                    var src = node.GetAttributeValue("src", "");
                    //                    src = "http://biz.touchev.com/" + src;
                    //                    var fileNameIndex = src.LastIndexOf("/");
                    //                    var fileName = src.Substring(fileNameIndex + 1);
                    //                    var productName = node.GetAttributeValue("alt", "");
                    //                    if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                    //                    {
                    //                        productsNames += productName + ";";
                    //                        if (SavelImg(src, fileName))
                    //                        {
                    //                            productImg item = new productImg()
                    //                            {
                    //                                name = productName,
                    //                                path = netName + @"\productsImgs\" + fileName
                    //                            };
                    //                            productsImgs.Add(item);
                    //                        }
                    //                    }
                    //                });
                    //            }
                    //            else
                    //            {

                    //            }
                    //        }
                    //        updater = Update.Set("产品列表", productsNames)
                    //          .Set("产品名称", productsNames)
                    //          .Set("主营产品", discription)
                    //          .Set("产品图片", JsonConvert.SerializeObject(productsImgs));
                    //        DataCollection.UpdateOne(Filter.Eq("_id", id), updater);

                    //        Console.CursorLeft = 0;
                    //        Console.Write($"更新第:{(icount + 1).ToString("D8")}条");
                    //        icount++;



                    //////B2b168Com
                    //if (htmlDocList.CountDocuments() != 0)
                    //{
                    //    var htmlDoc = htmlDocList.First();
                    //    var row = htmlDoc["Html"].ToString();
                    //    Doc.LoadHtml(row);

                    //    if (Doc.DocumentNode.SelectNodes("//li[@class='li01']//img") != null)
                    //    {
                    //        discription = Doc.DocumentNode.SelectSingleNode("//li[@class='table0']//table//tr[2]//td[2]")?.InnerText;
                    //        Doc.DocumentNode.SelectNodes("//li[@class='li01']//img").ToList().ForEach(node =>
                    //        {
                    //            var src = node.GetAttributeValue("src", "");
                    //            if (!src.Contains("http:"))
                    //                src = "http:" + src;
                    //            var fileNameIndex = src.LastIndexOf("/");
                    //            var fileName = src.Substring(fileNameIndex + 1);
                    //            var productName = node.GetAttributeValue("alt", "");
                    //            if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                    //            {
                    //                if (SavelImg(src, fileName))
                    //                {
                    //                    productsNames += productName + ";";
                    //                    productImg item = new productImg()
                    //                    {
                    //                        name = productName,
                    //                        path = netName + @"\productsImgs\" + fileName
                    //                    };
                    //                    productsImgs.Add(item);
                    //                }
                    //            }
                    //        });
                    //    }
                    //    else if( Doc.DocumentNode.SelectNodes("//div[@class='s01']//span[@class='span02']") != null)
                    //    {

                    //    }else if (Doc.DocumentNode.SelectNodes("//li[@class='li02']") != null)
                    //    {

                    //    }else if (Doc.DocumentNode.SelectNodes("//span[@class='span01']") != null)
                    //    {

                    //    }
                    //    else
                    //    {

                    //    }
                    //}
                    //updater = Update.Set("产品列表", productsNames)
                    //  .Set("产品名称", productsNames)
                    //  .Set("主营产品", discription)
                    //  .Set("产品图片", JsonConvert.SerializeObject(productsImgs));
                    //DataCollection.UpdateOne(Filter.Eq("_id", id), updater);

                    //Console.CursorLeft = 0;
                    //Console.Write($"更新第:{(icount + 1).ToString("D8")}条");
                    //icount++;

                    ////* w3618MedCom
                    //if (htmlDocList.CountDocuments() != 0)
                    //{
                    //    var htmlDoc = htmlDocList.First();
                    //    var row = htmlDoc["Html"].ToString();
                    //    Doc.LoadHtml(row);
                    //    discription = Doc.DocumentNode.SelectSingleNode("//div[@class='qy_logo']//p").InnerText.Replace("主营产品：", "");
                    //    if (Doc.DocumentNode.SelectNodes("//div[@class='index_pro_list']//img") !=null)
                    //    {

                    //        Doc.DocumentNode.SelectNodes("//div[@class='index_pro_list']//img").ToList().ForEach(node =>
                    //        {
                    //            var src = node.GetAttributeValue("src", "");
                    //            var fileNameIndex = src.LastIndexOf("/");
                    //            var fileName = src.Substring(fileNameIndex + 1);
                    //            var productName = node.GetAttributeValue("alt", "");
                    //            if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                    //            {
                    //                if (SavelImg(src, fileName))
                    //                {
                    //                    productsNames  += productName + ";";
                    //                    productImg item = new productImg()
                    //                    {
                    //                        name = productName,
                    //                        path = @"w3618MedCom\productsImgs\" + fileName
                    //                    };
                    //                    productsImgs.Add(item);
                    //                }
                    //            }
                    //        });
                    //    }
                    //    else if(Doc.DocumentNode.SelectNodes("//ul[@class='prolist']//li//img") !=null)
                    //    {

                    //        Doc.DocumentNode.SelectNodes("//ul[@class='prolist']//li//img").ToList().ForEach(node =>
                    //        {
                    //            var src = node.GetAttributeValue("src", "");
                    //            var fileNameIndex = src.LastIndexOf("/");
                    //            var fileName = src.Substring(fileNameIndex + 1);
                    //            var productName = node.GetAttributeValue("alt", "");
                    //            if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                    //            {
                    //                if (SavelImg(src, fileName))
                    //                {
                    //                    productsNames += productName + ";";
                    //                    productImg item = new productImg()
                    //                    {
                    //                        name = productName,
                    //                        path = @"w3618MedCom\productsImgs\" + fileName
                    //                    };
                    //                    productsImgs.Add(item);
                    //                }
                    //            }
                    //        });
                    //    }
                    //    else if(Doc.DocumentNode.SelectNodes("//div[@class='index_product']//li/a[1]/img") != null)
                    //    {

                    //        Doc.DocumentNode.SelectNodes("//div[@class='index_product']//li/a[1]/img").ToList().ForEach(node =>
                    //        {
                    //            var src = node.GetAttributeValue("src", "");
                    //            var fileNameIndex = src.LastIndexOf("/");
                    //            var fileName = src.Substring(fileNameIndex + 1);
                    //            var productName = node.GetAttributeValue("alt", "");
                    //            if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                    //            {
                    //                if (SavelImg(src, fileName))
                    //                {
                    //                    productsNames += productName + ";";
                    //                    productImg item = new productImg()
                    //                    {
                    //                        name = productName,
                    //                        path = @"w3618MedCom\productsImgs\" + fileName
                    //                    };
                    //                    productsImgs.Add(item);
                    //                }
                    //            }
                    //        });
                    //    }
                    //    else if (Doc.DocumentNode.SelectNodes("//div[@class='prolist']//dl") != null)
                    //    {
                    //        Doc.DocumentNode.SelectNodes("//div[@class='prolist']//dl").ToList().ForEach(node =>
                    //        {
                    //            var imgNode = node.SelectSingleNode(".//img");
                    //            var src = imgNode.GetAttributeValue("src", "");
                    //            var fileNameIndex = src.LastIndexOf("/");
                    //            var fileName = src.Substring(fileNameIndex + 1);

                    //            var anode = node.SelectSingleNode(".//dt[1]//a");
                    //            var productName = anode.GetAttributeValue("title", "");
                    //            if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                    //            {
                    //                if (SavelImg(src, fileName))
                    //                {
                    //                    productsNames += productName + ";";
                    //                    productImg item = new productImg()
                    //                    {
                    //                        name = productName,
                    //                        path = @"w3618MedCom\productsImgs\" + fileName
                    //                    };
                    //                    productsImgs.Add(item);
                    //                }
                    //            }
                    //        });
                    //    }
                    //    else if(Doc.DocumentNode.SelectNodes("//div[@class='caselist']//dl/dt[1]//img") != null)
                    //    {
                    //        Doc.DocumentNode.SelectNodes("//div[@class='caselist']//dl/dt[1]//img").ToList().ForEach(node =>
                    //        {
                    //            var src = node.GetAttributeValue("src", "");
                    //            var fileNameIndex = src.LastIndexOf("/");
                    //            var fileName = src.Substring(fileNameIndex + 1);
                    //            var productName = node.GetAttributeValue("alt", "");
                    //            if (!string.IsNullOrEmpty(src) && fileName.Contains("."))
                    //            {
                    //                if (SavelImg(src, fileName))
                    //                {
                    //                    productsNames += productName + ";";
                    //                    productImg item = new productImg()
                    //                    {
                    //                        name = productName,
                    //                        path = @"w3618MedCom\productsImgs\" + fileName
                    //                    };
                    //                    productsImgs.Add(item);
                    //                }
                    //            }
                    //        });
                    //    }
                    //    else
                    //    {

                    //    }
                    //}

                    //    updater = Update.Set("产品列表", productsNames)
                    //    .Set("产品名称",productsNames)
                    //    .Set("主营产品", discription)
                    //    .Set("产品图片", JsonConvert.SerializeObject(productsImgs));
                    //DataCollection.UpdateOne(Filter.Eq("_id", id), updater);

                    //Console.CursorLeft = 0;
                    //Console.Write($"更新第:{(icount + 1).ToString("D8")}条");
                    //icount++;
                }
            }
        }

        public HttpRequestMessage GetRequest(string Url)
        {

            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.RequestUri = new Uri(Url);
            httpRequestMessage.Method = HttpMethod.Get;

            //httpRequestMessage.Method = HttpMethod.Post;
            //httpRequestMessage.Content = Utility.MakePostConet(new List<(string Name, string Value)>
            //    {
            //        (Name:"categoryId",""),
            //        (Name:"year",Value:$"{0}"),
            //        (Name:"bar",Value:"true"),
            //        (Name:"words",Value:"40"),
            //        (Name:"pageNo",Value:$"{Page}"),
            //        (Name:"pageSize",Value:"15"),
            //    });
            return httpRequestMessage;
        }

    }

    public class productImg
    {
        public string name { get; set; }
        public string path { get; set; }
    }
}
