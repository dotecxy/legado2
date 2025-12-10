using Legado.Core.Data.Entities;
using Legado.Core.Helps.Http;
using Legado.Core.Models; // 假设 Book, BaseSource 等模型在此命名空间
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Utils; // 假设 NetworkUtils, CookieStore 等在此
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Legado.Core.Utils.NetworkUtils;
using static Legado.Core.Constants.AppPattern;
using Legado.Core.Helps;

namespace Legado.Core.Models.AnalyzeRules
{
    /// <summary>
    /// 搜索URL规则解析
    /// 对应 Kotlin: AnalyzeUrl.kt
    /// </summary>
    public partial class AnalyzeUrl : JsExtensions
    {


        // ================= 属性字段 =================
        public string RuleUrl { get; private set; }
        public string Url { get; private set; }
        public string Type { get; private set; }
        public Dictionary<string, string> HeaderMap { get; private set; } = new Dictionary<string, string>();
        public long? ServerID { get; private set; }

        public CookieStore CookieStore { get; private set; } = new CookieStore();
        public CacheManager CacheManager { get => CacheManager.Instance; }

        private string mUrl;
        private string key;
        private int? page;
        private string speakText;
        private int? speakSpeed;
        private string baseUrl;
        private BookSource source;
        private object ruleData; // Book or other data
        private BookChapter chapter;

        // HTTP 请求相关
        private string body;
        private string urlNoQuery;
        private string encodedForm;
        private string encodedQuery;
        private string charset;
        private string method = "GET";
        private string proxy;
        private int retry = 0;
        private bool useWebView = true;
        private string webJs;
        private bool enabledCookieJar;
        private string domain;
        private long webViewDelayTime = 0;

        // JS 引擎封装 (假设使用了之前提供的 Jint 封装类)
        private AnalyzeRule _jsContext;

        // ================= 构造函数 =================
        public AnalyzeUrl(
            string mUrl,
            string key = null,
            int? page = null,
            string speakText = null,
            int? speakSpeed = null,
            string baseUrl = "",
            BookSource source = null,
            object ruleData = null,
            BookChapter chapter = null,
            Dictionary<string, string> headerMapF = null) : base("cache_path")
        {
            this.mUrl = mUrl;
            this.key = key;
            this.page = page;
            this.speakText = speakText;
            this.speakSpeed = speakSpeed;
            this.baseUrl = baseUrl;
            this.source = source;
            this.ruleData = ruleData;
            this.chapter = chapter;
            this.enabledCookieJar = source?.EnabledCookieJar ?? false;

            // 处理 BaseUrl
            var urlMatcher = PARAM_PATTERN.Match(this.baseUrl);
            if (urlMatcher.Success)
            {
                this.baseUrl = this.baseUrl.Substring(0, urlMatcher.Index);
            }

            // 处理 Headers
            if (headerMapF != null)
            {
                foreach (var kvp in headerMapF) HeaderMap[kvp.Key] = kvp.Value;
            }
            else
            {
                // TODO: 实现 source.getHeaderMap
                var sourceHeaders = source?.HeaderMap;
                if (sourceHeaders != null)
                {
                    foreach (var kvp in sourceHeaders)
                    {
                        if (kvp.Key == "proxy") proxy = kvp.Value;
                        else HeaderMap[kvp.Key] = kvp.Value;
                    }
                }
            }

            InitUrl();
            this.domain = GetSubDomain(source?.BookSourceUrl ?? Url);
        }

        /// <summary>
        /// 初始化处理 URL
        /// </summary>
        private void InitUrl()
        {
            RuleUrl = mUrl;
            AnalyzeJs();       // 执行 <js>
            ReplaceKeyPageJs(); // 替换 {{js}} 和 <page>
            AnalyzeUrlInternal(); // 解析 URL 参数
        }

        // ================= 核心逻辑 =================

        /// <summary>
        /// 执行 @js, <js></js>
        /// </summary>
        private void AnalyzeJs()
        {
            int start = 0;
            var matches = JS_PATTERN.Matches(RuleUrl);
            string result = RuleUrl;

            foreach (Match match in matches)
            {
                if (match.Index > start)
                {
                    string prefix = RuleUrl.Substring(start, match.Index - start).Trim();
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        result = result.Replace("@result", prefix);
                    }
                }

                string jsCode = match.Groups[2].Success ? match.Groups[2].Value : match.Groups[3].Value;
                // JS 执行时，result 作为上下文传入
                object jsEval = EvalJS(jsCode, result);
                result = jsEval?.ToString() ?? "";

                start = match.Index + match.Length;
            }

            if (RuleUrl.Length > start)
            {
                string suffix = RuleUrl.Substring(start).Trim();
                if (!string.IsNullOrEmpty(suffix))
                {
                    result = result.Replace("@result", suffix);
                }
            }
            RuleUrl = result;
        }

        /// <summary>
        /// 替换关键字,页数,JS ({{...}})
        /// </summary>
        private void ReplaceKeyPageJs()
        {
            // 1. 替换 {{js}}
            if (RuleUrl.Contains("{{") && RuleUrl.Contains("}}"))
            {
                // 使用正则替换 {{...}} 内容
                RuleUrl = Regex.Replace(RuleUrl, @"\{\{([\w\W]*?)\}\}", m =>
                {
                    string innerJs = m.Groups[1].Value;
                    object eval = EvalJS(innerJs);

                    if (eval is double d && d % 1.0 == 0.0) return d.ToString("0"); // 整数格式化
                    return eval?.ToString() ?? "";
                });
            }

            // 2. 替换分页 <1,2,3>
            if (page.HasValue)
            {
                RuleUrl = PAGE_PATTERN.Replace(RuleUrl, m =>
                {
                    var pages = m.Groups[1].Value.Split(',');
                    int idx = page.Value - 1;
                    if (idx < 0) idx = 0;

                    if (idx < pages.Length)
                        return pages[idx].Trim();
                    else
                        return pages.Last().Trim();
                });
            }
        }


        /// <summary>
        /// 解析 URL 及 JSON 选项
        /// </summary>
        private void AnalyzeUrlInternal()
        {
            // 分离 URL 和 JSON 选项
            Match match = PARAM_PATTERN.Match(RuleUrl);
            string urlNoOption = match.Success ? RuleUrl.Substring(0, match.Index) : RuleUrl;

            // 处理绝对路径 
            Url = NetworkUtils.GetAbsoluteUrl(baseUrl, urlNoOption);

            // 更新 baseUrl
            string newBase = GetBaseUrl(Url);
            if (!string.IsNullOrEmpty(newBase)) baseUrl = newBase;

            // 解析 JSON 选项
            if (urlNoOption.Length != RuleUrl.Length)
            {
                string jsonStr = RuleUrl.Substring(match.Index + match.Length - 1); // 包含 {
                // 去掉开头的逗号空白等
                int braceIndex = jsonStr.IndexOf('{');
                if (braceIndex > -1) jsonStr = jsonStr.Substring(braceIndex);

                try
                {
                    var option = JsonConvert.DeserializeObject<UrlOption>(jsonStr);
                    if (option != null)
                    {
                        if (!string.IsNullOrEmpty(option.Method)) method = option.Method.ToUpper();

                        if (option.Headers != null)
                        {
                            var headers = JObject.FromObject(option.Headers);
                            foreach (var prop in headers)
                            {
                                HeaderMap[prop.Key] = prop.Value.ToString();
                            }
                        }

                        body = option.GetBody();
                        Type = option.Type;
                        charset = option.Charset;
                        retry = option.Retry ?? 0;
                        useWebView = option.UseWebView();
                        webJs = option.WebJs;
                        ServerID = option.ServerID;
                        webViewDelayTime = option.WebViewDelayTime ?? 0;

                        // 执行 UrlOption 中的 js 
                        if (!string.IsNullOrEmpty(option.Js))
                        {
                            var jsRes = EvalJS(option.Js, Url);
                            if (jsRes != null) Url = jsRes.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"URL Option JSON Parse Error: {ex.Message}");
                }
            }

            urlNoQuery = Url;

            if (method == "GET")
            {
                int pos = Url.IndexOf('?');
                if (pos != -1)
                {
                    AnalyzeQuery(Url.Substring(pos + 1));
                    urlNoQuery = Url.Substring(0, pos);
                }
            }
            else if (method == "POST" && body != null)
            {
                // 简单判断是否为 JSON 或 XML
                bool isJson = body.Trim().StartsWith("{") || body.Trim().StartsWith("[");
                bool isXml = body.Trim().StartsWith("<");
                if (!isJson && !isXml && !HeaderMap.ContainsKey("Content-Type"))
                {
                    AnalyzeFields(body);
                }
            }
        }

        // ================= 参数编码处理 =================

        private void AnalyzeFields(string fieldsTxt)
        {
            encodedForm = EncodeParams(fieldsTxt, charset, false);
        }

        private void AnalyzeQuery(string query)
        {
            encodedQuery = EncodeParams(query, charset, true);
        }

        private string EncodeParams(string paramsStr, string charsetName, bool isQuery)
        {
            Encoding encoding = Encoding.UTF8;
            if (!string.IsNullOrEmpty(charsetName))
            {
                if (charsetName == "escape") encoding = null; // 特殊标记
                else
                {
                    try { encoding = Encoding.GetEncoding(charsetName); }
                    catch { encoding = Encoding.UTF8; }
                }
            }

            // 如果已经是 URL 编码过的，直接返回 (仅针对Query)
            if (isQuery && encoding != null && IsEncoded(paramsStr)) return paramsStr;

            StringBuilder sb = new StringBuilder();
            int len = paramsStr.Length;
            int pos = 0;

            while (pos < len)
            {
                if (sb.Length > 0) sb.Append("&");

                int ampOffset = paramsStr.IndexOf('&', pos);
                if (ampOffset == -1) ampOffset = len;

                int eqOffset = paramsStr.IndexOf('=', pos);
                string key, value;

                if (eqOffset == -1 || eqOffset > ampOffset)
                {
                    key = paramsStr.Substring(pos, ampOffset - pos);
                    value = null;
                }
                else
                {
                    key = paramsStr.Substring(pos, eqOffset - pos);
                    value = paramsStr.Substring(eqOffset + 1, ampOffset - eqOffset - 1);
                }

                AppendEncoded(sb, key, encoding);
                if (value != null)
                {
                    sb.Append("=");
                    AppendEncoded(sb, value, encoding);
                }

                pos = ampOffset + 1;
            }

            return sb.ToString();
        }

        private void AppendEncoded(StringBuilder sb, string value, Encoding encoding)
        {
            if (encoding == null)
            {
                sb.Append(Uri.EscapeDataString(value)); // 模拟 escape
            }
            else
            {
                // C# Uri.EscapeDataString 总是 UTF8。
                // 如果需要 GBK 等编码，需要手动转 byte 再转 %XX
                if (encoding == Encoding.UTF8)
                {
                    sb.Append(Uri.EscapeDataString(value));
                }
                else
                {
                    byte[] bytes = encoding.GetBytes(value);
                    foreach (byte b in bytes)
                    {
                        // 简单处理 RFC3986 Unreserved
                        if ((b >= 'A' && b <= 'Z') || (b >= 'a' && b <= 'z') || (b >= '0' && b <= '9') || b == '-' || b == '_' || b == '.' || b == '~')
                            sb.Append((char)b);
                        else
                            sb.Append('%').Append(b.ToString("X2"));
                    }
                }
            }
        }

        private bool IsEncoded(string text)
        {
            // 简单判断是否包含 %XX
            return Regex.IsMatch(text, "%[0-9A-Fa-f]{2}");
        }

        public void put(string key, string value)
        {
            if (ruleData is IRuleData data)
            {
                data.putBigVariable(key, value);
            }
        }

        /// <summary>
        /// 获取存储的变量值
        /// </summary>
        public string get(string key)
        {
            if (ruleData is IRuleData data)
            {
                return data.getBigVariable(key);
            }
            return null;
        }

        // ================= 网络请求 =================

        /// <summary>
        /// 异步获取字符串响应（对应 Kotlin 的 getStrResponseAwait）
        /// </summary>
        public async Task<StrResponse> GetStrResponseAwait(string jsStr = null, string sourceRegex = null, bool useWebView = true)
        {
            if (Type != null)
            {
                byte[] bytes = await GetByteArrayAwait();
                // 假设 HexUtil.encodeHexStr
                return new StrResponse(Url, BitConverter.ToString(bytes).Replace("-", "").ToLower());
            }

            // Rate Limit
            // await ConcurrentRateLimiter.AcquireAsync(source);

            SetCookie();

            if (this.useWebView && useWebView)
            {
                var body = await webViewAsync("", urlNoQuery, jsStr);
                return new StrResponse(Url, body);
            }

            using (var client = CreateHttpClient())
            {
                // 构建请求
                var request = new HttpRequestMessage(new HttpMethod(method), urlNoQuery);

                // Headers
                foreach (var kv in HeaderMap) request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);

                // Body / Query
                if (method == "GET" && !string.IsNullOrEmpty(encodedQuery))
                {
                    request.RequestUri = new Uri(urlNoQuery + "?" + encodedQuery);
                }
                else if (method == "POST")
                {
                    if (!string.IsNullOrEmpty(encodedForm))
                    {
                        request.Content = new StringContent(encodedForm, Encoding.UTF8, "application/x-www-form-urlencoded");
                    }
                    else if (!string.IsNullOrEmpty(body))
                    {
                        var contentType = HeaderMap.ContainsKey("Content-Type") ? HeaderMap["Content-Type"] : "application/json";
                        request.Content = new StringContent(body, Encoding.UTF8, contentType);
                    }
                }

                // 发送
                var response = await client.SendAsync(request);
                byte[] respBytes = await response.Content.ReadAsByteArrayAsync();

                // 处理编码 (这里简化，应从 Content-Type 获取 charset)
                string respStr = Encoding.UTF8.GetString(respBytes);

                // XML 处理逻辑
                // if (isXml...)

                return new StrResponse(Url, respStr);
            }
        }

        /// <summary>
        /// 同步获取字符串响应（对应 Kotlin 的 getStrResponse）
        /// </summary>
        public StrResponse GetStrResponse(string jsStr = null, string sourceRegex = null, bool useWebView = true)
        {
            return GetStrResponseAwait(jsStr, sourceRegex, useWebView).GetAwaiter().GetResult();
        }

        public async Task<byte[]> GetByteArrayAwait()
        {
            // Data URI 处理
            if (urlNoQuery.StartsWith("data:"))
            {
                var match = DATA_URI_REGEX.Match(urlNoQuery);
                if (match.Success)
                {
                    return Convert.FromBase64String(match.Groups[1].Value);
                }
            }

            SetCookie();

            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync(Url); // 简化版，未处理 POST
                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        /// <summary>
        /// 同步获取字节数组（对应 Kotlin 的 getByteArray）
        /// </summary>
        public byte[] GetByteArray()
        {
            return GetByteArrayAwait().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 异步获取 HttpResponseMessage（对应 Kotlin 的 getResponseAwait）
        /// </summary>
        public async Task<HttpResponseMessage> GetResponseAwait()
        {
            SetCookie();

            using (var client = CreateHttpClient())
            {
                var request = new HttpRequestMessage(new HttpMethod(method), urlNoQuery);

                // Headers
                foreach (var kv in HeaderMap)
                    request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);

                // Body / Query
                if (method == "GET" && !string.IsNullOrEmpty(encodedQuery))
                {
                    request.RequestUri = new Uri(urlNoQuery + "?" + encodedQuery);
                }
                else if (method == "POST")
                {
                    if (!string.IsNullOrEmpty(encodedForm))
                    {
                        request.Content = new StringContent(encodedForm, Encoding.UTF8, "application/x-www-form-urlencoded");
                    }
                    else if (!string.IsNullOrEmpty(body))
                    {
                        var contentType = HeaderMap.ContainsKey("Content-Type") ? HeaderMap["Content-Type"] : "application/json";
                        request.Content = new StringContent(body, Encoding.UTF8, contentType);
                    }
                }

                return await client.SendAsync(request);
            }
        }

        /// <summary>
        /// 同步获取 HttpResponseMessage（对应 Kotlin 的 getResponse）
        /// </summary>
        public HttpResponseMessage GetResponse()
        {
            return GetResponseAwait().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 异步获取输入流（对应 Kotlin 的 getInputStreamAwait）
        /// </summary>
        public async Task<System.IO.Stream> GetInputStreamAwait()
        {
            // Data URI 处理
            if (urlNoQuery.StartsWith("data:"))
            {
                var match = DATA_URI_REGEX.Match(urlNoQuery);
                if (match.Success)
                {
                    var bytes = Convert.FromBase64String(match.Groups[1].Value);
                    return new System.IO.MemoryStream(bytes);
                }
            }

            var response = await GetResponseAwait();
            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// 同步获取输入流（对应 Kotlin 的 getInputStream）
        /// </summary>
        public System.IO.Stream GetInputStream()
        {
            return GetInputStreamAwait().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 是否为 POST 请求（对应 Kotlin 的 isPost）
        /// </summary>
        public bool IsPost()
        {
            return method == "POST";
        }

        /// <summary>
        /// 获取 User-Agent（对应 Kotlin 的 getUserAgent）
        /// </summary>
        public string GetUserAgent()
        {
            if (HeaderMap.ContainsKey("User-Agent"))
                return HeaderMap["User-Agent"];
            // 返回默认 UA
            return "Mozilla/5.0";
        }

        // ================= 辅助方法 ================

        private HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            if (!string.IsNullOrEmpty(proxy))
            {
                // proxy 格式通常是 http://ip:port
                handler.Proxy = new WebProxy(proxy);
                handler.UseProxy = true;
            }
            // 自动管理 Cookies 可以在这里开启，但在 Legado 中是手动管理的
            handler.UseCookies = false;

            var client = new HttpClient(handler);
            if (retry > 0)
            {
                // C# HttpClient 默认没有重试，需要 Polly 等库，或者手动循环
            }
            return client;
        }

        private void SetCookie()
        {
            string cookie = CookieStore.GetCookie(domain);
            if (!string.IsNullOrEmpty(cookie))
            {
                // Merge Logic needed
                if (HeaderMap.ContainsKey("Cookie"))
                {
                    HeaderMap["Cookie"] += ";" + cookie;
                }
                else
                {
                    HeaderMap["Cookie"] = cookie;
                }
            }
        }


        /// <summary>
        /// 执行 JS（对应 Kotlin 的 evalJS）
        /// </summary>
        public object EvalJS(string jsStr, object result = null)
        {
            // 调用父类 JsExtensions 的 evalJS 方法
            using JsEvaluator evaluator = new JsEvaluator();
            Dictionary<string, object> bindings = new Dictionary<string, object>();
            bindings.Add("java", this);
            bindings.Add("baseUrl", this.baseUrl);
            bindings.Add("cookie", this.CookieStore);
            bindings.Add("cache", CacheManager);
            bindings.Add("page", page);
            bindings.Add("key", key);
            bindings.Add("speakText", speakText);
            bindings.Add("speakSpeed", speakSpeed);
            bindings.Add("book", this.ruleData as Book);
            bindings.Add("source", this.source);
            bindings.Add("result", result);
            evaluator.Bindings = bindings;
            return evaluator.EvalJs(jsStr, result);
        }

    }

    /// <summary>
    /// URL 选项实体
    /// </summary>
    public class UrlOption
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("charset")]
        public string Charset { get; set; }

        [JsonProperty("headers")]
        public object Headers { get; set; } // 可以是 Map 或 String

        [JsonProperty("body")]
        public object Body { get; set; }

        [JsonProperty("retry")]
        public int? Retry { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("webView")]
        public object WebView { get; set; }

        [JsonProperty("webJs")]
        public string WebJs { get; set; }

        [JsonProperty("js")]
        public string Js { get; set; }

        [JsonProperty("serverID")]
        public long? ServerID { get; set; }

        [JsonProperty("webViewDelayTime")]
        public long? WebViewDelayTime { get; set; }

        public bool UseWebView()
        {
            if (WebView == null) return false;
            string s = WebView.ToString().ToLower();
            return s != "" && s != "false";
        }

        public string GetBody()
        {
            if (Body == null) return null;
            if (Body is string s) return s;
            return JsonConvert.SerializeObject(Body);
        }
    }

}