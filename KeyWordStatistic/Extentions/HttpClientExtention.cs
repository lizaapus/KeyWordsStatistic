using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Thrinax.Chardet;
using Thrinax.Enums;

namespace KeyWordStatistic
{
    /// <summary>
    /// HttpClient的扩展方法
    /// </summary>
    public static class HttpClientExtention
    {
        public static byte[] GetByteArray(this HttpClient Client, string Url)
        {
            var bytes = Client.GetByteArrayAsync(Url).ConfigureAwait(false).GetAwaiter().GetResult();
            return bytes;
        }
        public static byte[] TryGetByteArray(this HttpClient Client, string Url, int TryTimes = 3, Action ExceptionAction = null)
        {
            byte[] Result = null;
            try
            {
                var Response = Client.TrySend(new HttpRequestMessage
                {
                    RequestUri = new Uri(Url),
                    Method = HttpMethod.Get,
                }, TryTimes: TryTimes, ExceptionAction: ExceptionAction);
                Result = Response.Content.ReadAsByteArrayAsync().Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Result;
        }

        /// <summary>
        /// 简单Get方法请求返回Html
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Url"></param>
        /// <param name="Encode"></param>
        /// <returns></returns>
        public static string GetString(this HttpClient Client, string Url, Encoding Encode = null)
        {
            return Client.Send(new HttpRequestMessage
            {
                RequestUri = new Uri(Url),
                Method = HttpMethod.Get,
            }).GetEncodedHtml(Encode);
        }

        public static string TryGetString(this HttpClient Client, string Url, Encoding Encode = null, int TryTimes = 3, Action ExceptionAction = null)
        {
            var Result = string.Empty;
            try
            {
                Result = Client.TrySend(new HttpRequestMessage
                {
                    RequestUri = new Uri(Url),
                    Method = HttpMethod.Get,
                }, TryTimes: TryTimes, ExceptionAction: ExceptionAction).GetEncodedHtml(Encode);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Result;
        }
        /// <summary>
        /// 响应自动编码
        /// </summary>
        /// <param name="Response"></param>
        /// <param name="Encode"></param>
        /// <returns></returns>
        public static string GetEncodedHtml(this HttpResponseMessage Response, Encoding Encode = null)
        {
            var bytes = Response.Content.ReadAsByteArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var encode = Encode;
            if (encode == null)
            {

                //Http响应头中拿编码
                string Ncharset = Response.Content.Headers.ContentType.CharSet ?? string.Empty;
                //Html头中找编码
                string Hcharset = "";
                string CharsetReg = @"(meta.*?charset=""?(?<Charset>[^\s""'>;]+)""?)|(xml.*?encoding=""?(?<Charset>[^\s"">;]+)""?)";
                var txt = Encoding.ASCII.GetString(bytes);
                var cache = string.Empty;
                var mc = Regex.Match(txt, @".+?<\/head>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (mc.Success)
                    cache = mc.Value;
                Match match = Regex.Match(cache, CharsetReg, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (match.Success)
                    Hcharset = match.Groups["Charset"].Value;

                if (!string.IsNullOrEmpty(Ncharset) && Ncharset.ToUpper() == Hcharset.ToUpper())
                {
                    try
                    {
                        encode = Encoding.GetEncoding(Hcharset);
                    }
                    catch { }

                }
                //正文识别出来的编码
                string Rcharset = "";
                if (encode == null)
                {
                    Rcharset = NChardetHelper.RecogCharset(bytes, language: Language.CHINESE, maxLength: -1);
                    if (!string.IsNullOrEmpty(Rcharset) &&
                        (Rcharset.ToUpper() == Hcharset.ToUpper() || Rcharset.ToUpper() == Ncharset.ToUpper()))
                        try
                        {
                            encode = Encoding.GetEncoding(Rcharset);
                        }
                        catch { }
                }

                //使用单一方式识别出的编码，网页自动识别 > 解析ContentType > 解析Html编码声明
                if (encode == null && !string.IsNullOrEmpty(Rcharset))
                {
                    try
                    {
                        encode = Encoding.GetEncoding(Rcharset);
                    }
                    catch { }
                }
                if (encode == null && !string.IsNullOrEmpty(Ncharset))
                {
                    try
                    {
                        encode = Encoding.GetEncoding(Ncharset);
                    }
                    catch { }
                }
                if (encode == null && !string.IsNullOrEmpty(Hcharset))
                {
                    try
                    {
                        encode = Encoding.GetEncoding(Hcharset);
                    }
                    catch { }
                }

                if (encode == null)
                    encode = Encoding.Default;
            }
            return encode.GetString(bytes);
        }
        /// <summary>
        /// Send的同步方法,会验证得到的响应代码是否正确
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Request"></param>
        /// <returns></returns>
        public static HttpResponseMessage Send(this HttpClient Client, HttpRequestMessage Request)
        {
            var Response = Client.SendAsync(Request).ConfigureAwait(false).GetAwaiter().GetResult();
            //抛出错误响应异常
            Response.EnsureSuccessStatusCode();
            return Response;
        }
        /// <summary>
        /// 尝试Send
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Request"></param>
        /// <param name="TryTimes"></param>
        /// <returns></returns>
        public static HttpResponseMessage TrySend(this HttpClient Client, HttpRequestMessage Request, int TryTimes = 3, Action ExceptionAction = null)
        {
            if (TryTimes < 1)
                TryTimes = 1;
            else if (TryTimes > 20)
                TryTimes = 20;
            HttpResponseMessage Response = null;
            for (int i = 1; i <= TryTimes; i++)
            {
                try
                {
                    Response = Client.Send(Request.Clone());
                    break;
                }
                catch (Exception ex)
                {
                    if (i == TryTimes)
                    {
                        throw ex;
                    }
                    if (ExceptionAction != null)
                        ExceptionAction();
                    continue;
                }
            }
            return Response;
        }
        /// <summary>
        /// 克隆请求Request
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        static HttpRequestMessage Clone(this HttpRequestMessage Request)
        {
            HttpRequestMessage clone = new HttpRequestMessage(Request.Method, Request.RequestUri);
            clone.Content = Request.Content;
            clone.Version = Request.Version;
            foreach (KeyValuePair<string, object> prop in Request.Properties)
            {
                clone.Properties.Add(prop);
            }
            foreach (KeyValuePair<string, IEnumerable<string>> header in Request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return clone;
        }

        /// <summary>
        /// 从CookieContainer变为List
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static List<Cookie> List(this CookieContainer container)
        {
            var cookies = new List<Cookie>();

            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable",
                BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
                null, container, new object[] { });

            foreach (var key in table.Keys)
            {
                var domain = key as string;

                if (domain == null)
                    continue;

                if (domain.StartsWith("."))
                    domain = domain.Substring(1);

                var httpAddress = string.Format("http://{0}/", domain);
                var httpsAddress = string.Format("https://{0}/", domain);

                if (Uri.TryCreate(httpAddress, UriKind.RelativeOrAbsolute, out var httpUri))
                {
                    foreach (Cookie cookie in container.GetCookies(httpUri))
                    {
                        cookies.Add(cookie);
                    }
                }
                if (Uri.TryCreate(httpsAddress, UriKind.RelativeOrAbsolute, out var httpsUri))
                {
                    foreach (Cookie cookie in container.GetCookies(httpsUri))
                    {
                        cookies.Add(cookie);
                    }
                }
            }

            return cookies;
        }
        /// <summary>
        /// 页面文字Html解码并且Trim
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TrimHtmlDecode(this string str)
        {
            return HttpUtility.HtmlDecode(str).Trim();
        }
        /// <summary>
        /// Url编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlEncode(this string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        /// <summary>
        /// 相对地址变绝对地址，将地址进行连接
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="Href"></param>
        /// <returns></returns>
        public static string UrlConcat(this string Url, string Href)
        {
            try
            {
                return new Uri(new Uri(Url), Href).ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// 字符串数组连接方法
        /// </summary>
        /// <param name="List"></param>
        /// <param name="Separator"></param>
        /// <returns></returns>
        public static string AggregateString(this IEnumerable<string> List, string Separator = "")
        {
            if (List == null)
                throw new ArgumentNullException();
            var Count = List.Count();
            if (Count == 0)
                return string.Empty;
            if (Count == 1)
                return List.First();
            return List.Aggregate((s1, s2) => (s1 + Separator + s2));
        }

        /// <summary>
        /// 如果是空如何替换
        /// </summary>
        /// <param name="value"></param>
        /// <param name="alternative"></param>
        /// <returns></returns>
        public static string IsNullOrEmptyThen(this string value, string alternative)
        {
            return string.IsNullOrEmpty(value) ? alternative : value;
        }

        public static int ConvertToInt(this string text, int Default = 0)
        {
            if (int.TryParse(text, out int res))
                return res;
            return Default;
        }
    }
}