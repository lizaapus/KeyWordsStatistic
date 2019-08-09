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

namespace KeyWordStatistic
{
    public class MongodbCollumnStatistic
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        string netName = "NsfcGovCn";
        public MongodbCollumnStatistic()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase(netName);
            var DataCollection = DataBase.GetCollection<BsonDocument>("Temp");
            var Curosr = DataCollection.Find(Filter.Empty).ToCursor();
            var SetColumns = new HashSet<string>();
            while (Curosr.MoveNext())
            {
                foreach (var Document in Curosr.Current)
                {
                    foreach (var ele in Document.Elements)
                    {
                        if (!SetColumns.Contains(ele.Name))
                        {
                            SetColumns.Add(ele.Name);
                        }
                    }
                }
            }

            string path = @"G:\img\prase2.txt";
            FileStream fs1 = new FileStream(path, FileMode.Create, FileAccess.Write);//搜索创建写入文件 
            StreamWriter sw = new StreamWriter(fs1);

            foreach (var kv in SetColumns)
            {
                sw.WriteLine(kv + "\t");
                Console.WriteLine(kv);
            }

            sw.Close();
            fs1.Close();
        }

    }


    
}
