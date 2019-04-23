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

namespace KeyWordStatistic
{
    public class GetHtml
    {
        protected HttpManager _HttpManager = new HttpManager();
        public bool IsGet = true;

        public List<DataEntity> GetPageList()
        {
            var Html = _HttpManager.Client.TrySend(GetRequest("http://www.otl.tsinghua.edu.cn/info/cghc_tgcg/1732")).GetEncodedHtml(null);
            var Doc = new Html.HtmlDocument();
            List< DataEntity > dataList = new List<DataEntity>();
            Doc.LoadHtml(Html);
            int icount = 0;
            Doc.DocumentNode.SelectNodes("//table//tr")?.ToList().ForEach(
            tr =>
            {
                try
                {
                    icount++;
                    if(!(icount==1||icount==2))
                    {
                        var Entity = new DataEntity();
                        Entity.Numb = tr.SelectSingleNode("./td[1]").InnerText.TrimHtmlDecode();
                        Entity.title = tr.SelectSingleNode("./td[2]").InnerText.TrimHtmlDecode();
                        Entity.publishTime = tr.SelectSingleNode("./td[3]").InnerText.TrimHtmlDecode();
                        Entity.field = tr.SelectSingleNode("./td[4]").InnerText.TrimHtmlDecode();
                        dataList.Add(Entity);
                    }
                   
                }
                catch {
                    Console.WriteLine("解析汇编页面失败");
                }
            });
            return dataList;
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

    public class DataEntity
    {
        public string Numb { get; set; }
        public string title { get; set; }
        public string publishTime { get; set; }
        public string field { get; set; }
    }

}
