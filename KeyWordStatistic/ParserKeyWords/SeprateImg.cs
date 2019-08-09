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
    public class SeprateImg
    {
        string _ConnectString = "mongodb://192.168.106.56:27017";
        string sourceListDirPath = @"E:\2019\企业信息库\listImg\";
        string sourceProductsDirPath= @"E:\2019\企业信息库\productImg\";
        string netName = "w3618MedCom";
        string saveListDirPath = "";
        string saveProductDirPath = "";
        public SeprateImg()
        {
            var Filter = Builders<BsonDocument>.Filter;
            var Update = Builders<BsonDocument>.Update;

            saveListDirPath = @"E:\2019\企业信息库\图片\" + netName + @"\listImgs\";
            saveProductDirPath = @"E:\2019\企业信息库\图片\" + netName + @"\productsImgs\";

            var Client = new MongoClient(_ConnectString);
            var DataBase = Client.GetDatabase(netName);
            var DataCollection = DataBase.GetCollection<BsonDocument>("Data");
            var Curosr = DataCollection.Find(Filter.Eq("DataStatus",3)).ToCursor();
            int icount = 0;


            if (!Directory.Exists(saveListDirPath))
            {
                Directory.CreateDirectory(saveListDirPath);
            }

            if (!Directory.Exists(saveProductDirPath))
            {
                Directory.CreateDirectory(saveProductDirPath);
            }


            while (Curosr.MoveNext())
            {
                foreach (var Document in Curosr.Current)
                {
                    var id = Document["_id"].AsObjectId;
                    UpdateDefinition<BsonDocument> updater = null;

                    //var str = Document["主营产品图片"].AsString.Replace(netName + "", ";");
                    //var productsNewName = string.Empty;
                    //if (!string.IsNullOrEmpty(str))
                    //{
                    //    var productsImgNames = str.Split(';');
                    //    foreach (var item in productsImgNames)
                    //    {
                    //        CopyFileToDes(item, 1);
                    //        if (!string.IsNullOrEmpty(item))
                    //            productsNewName += netName + "_" + item + ";";
                    //    }
                    //}

                    var listfileName = Document["rowImg"].AsString;
                    if (!string.IsNullOrEmpty(listfileName))
                    {
                        CopyFileToDes(listfileName, 0);
                        listfileName = netName + "_" + listfileName;
                    }
                    if (listfileName == netName + "_")
                        listfileName = "";
                           

                    updater = Update.Set("rowImg", listfileName)
                        .Set("detailImg", listfileName)
                        .Set("主营产品图片", "");

                    DataCollection.UpdateOne(Filter.Eq("_id", id), updater);
                    icount++;
                    Console.CursorLeft = 0;
                    Console.WriteLine("修改第" + icount + "条");
                }
            }
        }

        private void CopyFileToDes(string fileName,int flag)
        {
            var pLocalFilePath = string.Empty;
            var pSaveFilePath = string.Empty;

            switch (flag)
            {
                case 0:
                     pLocalFilePath = sourceListDirPath + fileName;
                     pSaveFilePath = saveListDirPath + fileName;
                    break;
                case 1:
                    pLocalFilePath = sourceProductsDirPath  + fileName;
                    pSaveFilePath = saveProductDirPath + fileName;

                    break;
            }
            if (File.Exists(pLocalFilePath))//必须判断要复制的文件是否存在
            {
                File.Copy(pLocalFilePath, pSaveFilePath, true);//三个参数分别是源文件路径，存储路径，若存储路径有相同文件是否替换
            }


        }
    }
}
