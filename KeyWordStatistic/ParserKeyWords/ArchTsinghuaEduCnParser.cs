using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Html = HtmlAgilityPack;

namespace KeyWordStatistic
{
    public class ArchTsinghuaEduCnParser
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        public ArchTsinghuaEduCnParser()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;
            var Dic = new Dictionary<string, int>();
            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase("TsinghuaEduCn");
            var DataCollection = DataBase.GetCollection<BsonDocument>("Scholar");
            var HtmlCollection = DataBase.GetCollection<BsonDocument>("Html");
            var Curosr = DataCollection.Find(Filter.Eq("学院", "建筑学院")).ToCursor();
            var Doc = new Html.HtmlDocument();
            while (Curosr.MoveNext())
            {
                foreach (var Document in Curosr.Current)
                {
                    var id = Document["_id"].AsObjectId;
                    var ContentHtml = HtmlCollection.Find(Filter.Eq("_id", id)).First()["Html"].AsString;
                    Doc.LoadHtml(ContentHtml);
                    var name = string.Empty;
                    var value = string.Empty;
                    var dic = new Dictionary<string, string>();
                    var nodes = Doc.DocumentNode.SelectNodes("//tr[3]/td/h2|//tr[3]/td/h3|//tr[3]/td//p/u");
                    if (nodes != null)
                    {
                        foreach (var p in nodes)
                        {
                            var text = p.InnerText.TrimHtmlDecode();
                            if (Dic.ContainsKey(text))
                                Dic[text]++;
                            else
                                Dic.Add(text, 1);
                        }
                    }
                }
            }

            string path = @"G:\img\ArchTsinghuaEduCnParser.txt";
            FileStream fs1 = new FileStream(path, FileMode.Create, FileAccess.Write);//搜索创建写入文件 
            StreamWriter sw = new StreamWriter(fs1);

            foreach (var kv in Dic)
            {
                sw.WriteLine(kv.Key + "\t" + kv.Value);
                //Console.WriteLine(kv.Key + "\t" + kv.Value);
            }

            sw.Close();
            fs1.Close();

            Console.ReadKey();
        }
    }
}
