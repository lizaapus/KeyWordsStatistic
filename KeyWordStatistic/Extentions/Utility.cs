using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace KeyWordStatistic
{
    /// <summary>
    /// 工具方法类
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// 制作Post内容
        /// </summary>
        /// <returns></returns>
        public static HttpContent MakePostConet(List<(string Name, string Value)> PostDatas, bool IsUrlEncoded = true, string ContentType = "application/x-www-form-urlencoded", Encoding Encode = null)
        {
            if (Encode == null)
                Encode = Encoding.UTF8;
            var PostText = string.Empty;
            if (PostDatas == null || PostDatas.Count == 0)
            {

            }
            else
            {
                if (IsUrlEncoded)
                    PostText = PostDatas.Select(kv => kv.Name + "=" + HttpUtility.UrlEncode(kv.Value, Encode)).AggregateString("&");
                else
                    PostText = PostDatas.Select(kv => kv.Name + "=" + kv.Value).AggregateString("&");
            }
            var buffer = Encode.GetBytes(PostText);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
            return byteContent;
        }
        /// <summary>
        /// 判断Href连接是否是一个可采集链接
        /// </summary>
        /// <param name="Href"></param>
        /// <returns></returns>
        public static bool IsLinkHref(string Href)
        {
            return !(Href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
                || Href.StartsWith("vbscript:", StringComparison.OrdinalIgnoreCase)
                || Href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                || Href.StartsWith("#")
                || Href.StartsWith("thunder:", StringComparison.OrdinalIgnoreCase)
                || Href.StartsWith("file:", StringComparison.OrdinalIgnoreCase));
        }
        /// <summary>
        /// 判断链接是否是下载文件链接
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static bool IsFileLink(string Url)
        {
            try
            {
                if (new Uri(Url).IsFile)
                    return true;
            }
            catch { }
            var wUrl = Url.ToLower();
            if (wUrl.EndsWith(".pdf"))
                return true;
            if (wUrl.EndsWith(".zip"))
                return true;
            if (wUrl.EndsWith(".rar"))
                return true;
            if (wUrl.EndsWith(".doc"))
                return true;
            if (wUrl.EndsWith(".docx"))
                return true;
            if (wUrl.EndsWith(".xls"))
                return true;
            if (wUrl.EndsWith(".xlsx"))
                return true;
            if (wUrl.EndsWith(".ppt"))
                return true;
            if (wUrl.EndsWith(".pptx"))
                return true;
            if (wUrl.EndsWith(".wps"))
                return true;
            if (wUrl.EndsWith(".rtf"))
                return true;
            if (wUrl.EndsWith(".jpg"))
                return true;
            if (wUrl.EndsWith(".exe"))
                return true;
            if (wUrl.EndsWith(".pps"))
                return true;
            if (wUrl.EndsWith(".flv"))
                return true;
            if (wUrl.EndsWith(".msw"))
                return true;
            if (wUrl.EndsWith(".apk"))
                return true;
            if (wUrl.EndsWith(".mp4"))
                return true;
            if (wUrl.EndsWith(".avi"))
                return true;
            if (wUrl.EndsWith(".ppsx"))
                return true;
            if (wUrl.EndsWith(".mpg"))
                return true;
            if (wUrl.EndsWith(".wmv"))
                return true;
            if (wUrl.EndsWith(".tif"))
                return true;

            return false;
        }
        /// <summary>
        /// 将响应内容转换为可反序列化的Json对象的String
        /// </summary>
        /// <param name="Html"></param>
        /// <returns></returns>
        public static string ConvertToJson(string Html)
        {
            var Json = Html;
            var mc = Regex.Match(Html, @"(\{.+})", RegexOptions.Singleline);
            if (mc.Success)
            {
                Json = mc.Groups[1].Value;
            }
            //压缩中间 ":"
            Json = Regex.Replace(Json, @"""\s*:\s*""", @""":""");
            //压缩结尾 ","
            Json = Regex.Replace(Json, @""",\s*""", @""",""");
            Json = Regex.Replace(Json, @"\s*{\s*""", @"{""");
            Json = Regex.Replace(Json, @"""\s*}\s*", @"""}");
            var macthes = Regex.Matches(Json, @"\s*""([^""]+?)""\s*:\s*""(((?!"",""|""},{).)+)""", RegexOptions.Singleline);
            var start = 0;
            var Items = new List<(string Text, bool IsValue)>();
            foreach (Match match in macthes)
            {
                var length = match.Groups[2].Index - start;
                if (length == 0)
                {
                }
                Items.Add((Json.Substring(start, length), false));
                length = match.Groups[2].Length;
                if (length == 0)
                {
                }
                Items.Add((Json.Substring(match.Groups[2].Index, length), true));
                start = (match.Groups[2].Index + match.Groups[2].Length);
            }
            Items.Add((Json.Substring(start), false));
            Json = string.Empty;
            foreach (var item in Items)
            {
                if (!item.IsValue)
                    Json += item.Text;
                else
                {
                    var temp = item.Text;
                    temp = Regex.Replace(temp, "\r", "\\r");
                    temp = Regex.Replace(temp, "\n", "\\n");
                    temp = Regex.Replace(temp, @"(?<!\\)""", "\\\"");
                    Json += temp;
                }
            }

            return Json;
        }

        /// <summary>
        /// 结果转换为页数
        /// </summary>
        /// <param name="Count"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public static int RecCountToPageNum(int Count, int PageSize)
        {
            return Count % PageSize == 0 ? Count / PageSize : (Count / PageSize + 1);
        }
    }
}