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
    public class AlertMongodbValue
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        public AlertMongodbValue()
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
                    UpdateDefinition<BsonDocument> updater = null;
                    var id = Document["_id"].AsObjectId;
                    var fileName = Document["rowImg"].ToString();
                    if (!string.IsNullOrEmpty(fileName))
                        fileName = fileName.Replace("w3618MedCom_", @"w3618MedCom\listImgs\");

                    updater = Update.Set("rowImg", fileName).Set("detailImg", fileName);

                    DataCollection.UpdateOne(Filter.Eq("_id", id), updater);
                    Console.CursorLeft = 0;
                    
                    Console.WriteLine("修改第" + icount + "个");
                    
                    icount++;
                    //Thread.Sleep(1000);
                }
            }
        }

    }
}
