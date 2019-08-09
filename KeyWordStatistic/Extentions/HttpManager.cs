using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace KeyWordStatistic
{
    public class HttpManager
    {
        HttpClientHandler _Handler = null;
        CookieContainer _Cookies = null;
        public HttpClient Client { get; } = null;

        public CookieContainer Cookies
        {
            get { return _Cookies; }
        }

        public HttpClientHandler Handler
        {
            get { return _Handler; }
        }

        public HttpManager()
        {
            _Cookies = new CookieContainer();
            _Handler = new HttpClientHandler()
            {
                CookieContainer = _Cookies,
                AllowAutoRedirect = true,
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };
            Client = new HttpClient(_Handler);
            Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36");
        }

        /// <summary>
        /// 设定一个与当前代理不同的IP
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="Port"></param>
        /// <returns></returns>
        public bool SetProxy(string Ip, int Port)
        {
            if (_Handler.Proxy is WebProxy currentHandlerProxy)
            {
                if (currentHandlerProxy.Address.Host == Ip)
                {
                    return false;
                }
                currentHandlerProxy.Address = new Uri($"http://{Ip}:{Port}");
            }
            else
            {
                _Handler.Proxy = new WebProxy(Ip, Port);
            }
            return true;
        }
        /// <summary>
        /// 所有Cookie都过期
        /// </summary>
        public void ExpireCookies()
        {
            List(_Cookies)
                .ForEach(
                    c => c.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1))
                );
        }

        /// <summary>
        /// 从CookieContainer变为List
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public List<Cookie> List(CookieContainer container)
        {
            var cookies = new List<Cookie>();

            var table = (System.Collections.Hashtable)container.GetType().InvokeMember("m_domainTable",
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
    }
}
