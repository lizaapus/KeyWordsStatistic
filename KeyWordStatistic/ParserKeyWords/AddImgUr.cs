using System;
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
using Newtonsoft.Json;
using MongoDB.Bson.Serialization.Attributes;

namespace KeyWordStatistic
{
    
    class AddImgUr
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        string db = "YlqxQgyyzsNet";//新
        public AddImgUr()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase(db);
            var DataCollection1 = DataBase.GetCollection<BsonDocument>("Enterprise");

            var NewDataCollection = DataBase.GetCollection<BsonDocument>("Images");
            var Curosr = DataCollection1.Find(Filter.Empty).ToCursor();
            var wModels = new List<WriteModel<BsonDocument>>();
            int icount = 0;
            var Sucessed = 0;
            while (Curosr.MoveNext())
            {
                foreach (var doc in Curosr.Current)
                {
                    var RowImgUrl = doc.GetValue("RowImgUrl", "").ToString();
                    if(!string.IsNullOrEmpty(RowImgUrl))
                    {
                        var Data = new ImgItem();
                        Data.Url = FormAturl(RowImgUrl, doc["Url"].ToString() );
                        Data.DataStatus = 0;
                        wModels.Add(new InsertOneModel<BsonDocument>(Data.ToBsonDocument()));
                        icount++;
                    }

                    var DetailImg = doc.GetValue("DetailImg", "").ToString();
                    if (!string.IsNullOrEmpty(DetailImg))
                    {
                        var Data = new ImgItem();
                        Data.Url = FormAturl(DetailImg, doc["Url"].ToString() ); 
                        Data.DataStatus = 0;
                        wModels.Add(new InsertOneModel<BsonDocument>(Data.ToBsonDocument()));
                        icount++;
                    }

                    var bValue = doc.GetValue("产品", "");
                    if (bValue != "")
                    {
                        var pList = bValue.AsBsonArray;
                        foreach (var item in pList)
                        {
                            var Data = new ImgItem();
                            Data.Url = FormAturl(item["ImgUrl"].ToString(), doc["Url"].ToString() ); 
                            Data.DataStatus = 0;
                            wModels.Add(new InsertOneModel<BsonDocument>(Data.ToBsonDocument()));
                            icount++;
                        }
                    }
                    if (icount % 100 == 0)
                    {
                        Sucessed += wModels.Count;
                        try
                        {
                            NewDataCollection.BulkWrite(wModels, new BulkWriteOptions { IsOrdered = false });
                        }
                        catch (MongoBulkWriteException ex)
                        {
                            Sucessed = Sucessed - ex.WriteErrors.Count;
                        }
                        catch (Exception ex)
                        {
                            Sucessed = 0;
                        }
                        Console.WriteLine("成功写入" + $" {Sucessed}:{icount}条数据\n");
                        wModels.Clear();
                    }
                }
            }
            Sucessed += wModels.Count;
            try
            {
                NewDataCollection.BulkWrite(wModels, new BulkWriteOptions { IsOrdered = false });
            }
            catch (MongoBulkWriteException ex)
            {
                Sucessed = Sucessed - ex.WriteErrors.Count;
            }
            catch (Exception ex)
            {
                Sucessed = 0;
            }

            Console.WriteLine("成功写入" + $" {Sucessed}:{icount}条数据\n");
            wModels.Clear();
            Console.ReadKey();
        }
        public String FormAturl(String urlX, string objurl)
        {
            Uri baseUri = new Uri(objurl); // http://www.enet.com.cn/enews/inforcenter/designmore.jsp
            Uri absoluteUri = new Uri(baseUri, urlX);//相对绝对路径都在这里转 这里的urlx ="../test.html"
            return absoluteUri.ToString();//   http://www.enet.com.cn/enews/test.html   
        }
    }
    public class ImgItem
    {
        public string Url { get; set; } = string.Empty;
        public int DataStatus { get; set; } = 0;
    }
}


//BsonValue DetailImgUrl;
//if (doc.TryGetValue("DetailImg", out DetailImgUrl))
//{
//    if (!string.IsNullOrEmpty(DetailImgUrl.ToString()))
//    {
//        var Data = new ImgItem();
//        Data.Url = DetailImgUrl.ToString();
//        Data.DataStatus = 0;
//        wModels.Add(new InsertOneModel<BsonDocument>(Data.ToBsonDocument()));
//    }
//}