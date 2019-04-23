using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyWordStatistic
{
    public class UpdateMongoDb
    {
        public FilterDefinitionBuilder<BsonDocument> Filter { get { return Builders<BsonDocument>.Filter; } }
        public UpdateDefinitionBuilder<BsonDocument> Update
        {
            get { return Builders<BsonDocument>.Update; }
        }
        public ProjectionDefinitionBuilder<BsonDocument> Projection { get; private set; }
        public SortDefinitionBuilder<BsonDocument> Sort { get; private set; }
        protected IMongoDatabase DataBase = null;
        protected IMongoCollection<BsonDocument> _DataColletion = null;
        protected IMongoCollection<BsonDocument> _htmlColletion = null;

        string connetMongoDB = "mongodb://192.168.106.56:27017?slaveOk=true";
        string dbName = "TsinghuaEduCn";
        List<UpdateStr> updateStrList = new List<UpdateStr>();
        public UpdateMongoDb()
        {
            var Client = new MongoClient(connetMongoDB).WithReadPreference(ReadPreference.SecondaryPreferred);
            DataBase = Client.GetDatabase(dbName);
            _DataColletion = DataBase.GetCollection<BsonDocument>("Data");
            _htmlColletion = DataBase.GetCollection<BsonDocument>("Html");
            //PraseContentAsync();
            BatcRunAsync();

        }

        public async Task PraseContentAsync()
        {
           // IAsyncCursor<BsonDocument> Cursor;
            var   Cursor = _DataColletion.Find(Filter.Eq("Section", "首页 > 成果汇粹 > 推广成果")).ToList();

           
            int icount = 0;
           
                foreach (var Document in Cursor)
                {
                    icount++;
                    Console.WriteLine("检索到第" + icount + " 篇 ");
                    try
                    {
                        //string str = Document.GetValue("_id").AsObjectId.ToString();
                        BsonDocument doc = _htmlColletion.Find(Filter.Eq("_id", Document["_id"])).FirstOrDefault();
                        string content = doc.GetValue("Content").ToString();
                        PraseContent(content, Document.GetValue("_id").AsObjectId, updateStrList);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.ReadLine();
                    }

                }
            foreach (var strC in updateStrList)
            {
                UpdateDefinition<BsonDocument> Updater;
                Updater= Update.Set("成果简介", strC.str1)
                    .Set("应用前景", strC.str2)
                    .Set("知识产权", strC.str3)
                    .Set("团队介绍", strC.str4)
                    .Set("合作方式", strC.str5)
                    .Set("联系方式", strC.str6)
                    .Set("成果编号", strC.str7);
                try
                {
                    _DataColletion.UpdateOne(Filter.Eq("_id", strC._id), Updater);
                }catch(Exception e)
                {
                    Console.WriteLine(e);
                    Console.ReadLine();
                }
                
            }
        }

        private void PraseContent(string content, ObjectId _id, List<UpdateStr> updateStrList)
        {
            try
            {
                UpdateStr strC = new UpdateStr();
                strC._id = _id;
                content = content.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(" ", "");
                int index01 = content.IndexOf("01.");
                if (index01 == -1)
                    index01 = content.IndexOf("01．");
                int index02 = content.IndexOf("02.");
                if (index02 == -1)
                    index02 = content.IndexOf("02．");
                int index03 = content.IndexOf("03.");
                if (index03 == -1)
                    index03 = content.IndexOf("03．");
                int index04 = content.IndexOf("04.");
                if (index04 == -1)
                    index04 = content.IndexOf("04．");
                int index05 = content.IndexOf("05.");
                if (index05 == -1)
                    index05 = content.IndexOf("05．");
                int index06 = content.IndexOf("06.");
                if(index06==-1)
                    index06 = content.IndexOf("06．"); 
                int index07 = content.IndexOf("成果编号：");
                if (index07 == -1)
                    index07 = content.IndexOf("成果编号:");

                if (index01 == 0 || index01 == -1)
                {
                    Console.WriteLine(_id);
                   
                    return;
                }


                strC.str1 = (index02 - index01) > 0 ? content.Substring(index01 + 3, index02 - index01 - 3) : string.Empty;
                strC.str2 = (index03 - index02) > 0 ? content.Substring(index02 + 3, index03 - index02 - 3) : string.Empty;
                strC.str3 = (index04 - index03) > 0 ? content.Substring(index03 + 3, index04 - index03 - 3) : string.Empty;
                strC.str4 = (index05 - index04) > 0 ? content.Substring(index04 + 3, index05 - index04 - 3) : string.Empty;
                strC.str5 = (index06 - index05) > 0 ? content.Substring(index05 + 3, index06 - index05 - 3) : string.Empty;
                strC.str6 = (index07 - index06) > 0 ? content.Substring(index06 + 3, index07 - index06 - 3) : string.Empty;
                strC.str7 = index07 > 0 ? content.Substring(index07 + 5, content.Length - index07 - 5) : string.Empty;

                if (string.IsNullOrEmpty(strC.str1)
                   || string.IsNullOrEmpty(strC.str2)
                   || string.IsNullOrEmpty(strC.str3)
                   || string.IsNullOrEmpty(strC.str4)
                   || string.IsNullOrEmpty(strC.str5)
                   || string.IsNullOrEmpty(strC.str6))
                {
                    Console.WriteLine(_id);

                    //Console.ReadLine();
                    return;
                }

                updateStrList.Add(strC);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }



        public void BatcRunAsync()
        {

            List<DataEntity> dataList = new GetHtml().GetPageList();
            UpdateDefinition<BsonDocument> Updater;
            int icount = 0;
            foreach (var data in dataList)
            {
                Updater = Update.Set("成果领域", data.field);

                _DataColletion.UpdateOne(Filter.Eq("Title", data.title), Updater);
                Console.WriteLine(icount + ":" + data.title);
                icount++;
            }
            Console.ReadLine();
        }

    }

    public class UpdateStr
    {
        public ObjectId _id;
        public string str1 { get; set; }
        public string str2 { get; set; }
        public string str3 { get; set; }
        public string str4 { get; set; }
        public string str5 { get; set; }
        public string str6 { get; set; }
        public string str7 { get; set; }

    }

}
