using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Html = HtmlAgilityPack;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace KeyWordStatistic
{
    public class AddImgUrl
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";

        public AddImgUrl()
        {
            Run();

        }

        private void Run()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;

            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase("TsinghuaEduCn");
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

                    var node = Doc.DocumentNode.SelectSingleNode("//img");
                    var imgUrl = node.GetAttributeValue("src","");
                    //imgUrl = "http://www.otl.tsinghua.edu.cn" + imgUrl;

                    var fileNameIndex = imgUrl.LastIndexOf("/");
                    var fileName = imgUrl.Substring(fileNameIndex + 1);

                    UpdateDefinition<BsonDocument> updater = null;
                    updater = Update.Set("rowImg", fileName);

                    DataCollection.UpdateOne(Filter.Eq("_id", id), updater);
                    Console.CursorLeft = 0;
                    Console.WriteLine("修改第" + icount + "个");
                    icount++;

                }
            }
        }


    }
}
