using AngleSharp;
using AngleSharp.Html.Parser;
using Jint;
using Legado.Core.Helps;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Legado.Core.Helps
{
    /// <summary>
    /// 对应 Legado 的 JsExtensions.kt
    /// 提供了网络请求、文件操作、编码转换等功能供 JS 调用
    /// </summary>
    public partial class JsExtensions : JsEncodeUtils // 继承自上一个问题的加密工具类
    {
        private readonly string _cachePath;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _angleSharpConfig;

        // 模拟 Source 对象，用于获取 Header 等信息
        private readonly object _sourceContext;

        public JsExtensions(string cachePath, object sourceContext = null)
        {
            _cachePath = cachePath;
            if (!Directory.Exists(_cachePath)) Directory.CreateDirectory(_cachePath);

            _sourceContext = sourceContext;

            // 初始化 HttpClient (实际使用建议注入单例)
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                AllowAutoRedirect = true // Legado 默认有时是 false，视情况调整
            };
            _httpClient = new HttpClient(handler);

            // 注册编码支持 (GBK等)
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // AngleSharp 配置
            _angleSharpConfig = Configuration.Default.WithDefaultLoader();
        }

        #region 网络请求 (Ajax/Connect)

        /// <summary>
        /// 访问网络, 返回 String
        /// </summary>
        public string ajax(object url)
        {
            string targetUrl = url is object[] list ? list.FirstOrDefault()?.ToString() : url.ToString();
            if (string.IsNullOrEmpty(targetUrl)) return null;

            return AnalyzeUrl(targetUrl, null).Result.Body;
        }

        /// <summary>
        /// 访问网络, 返回 Response 对象 (模拟 Legado 的 StrResponse)
        /// </summary>
        public StrResponse connect(string url, string header = null)
        {
            Dictionary<string, string> headers = null;
            if (!string.IsNullOrEmpty(header))
            {
                try { headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(header); } catch { }
            }

            var result = AnalyzeUrl(url, headers).Result;
            return result;
        }

        /// <summary>
        /// 这里的 AnalyzeUrl 模拟 Legado 的复杂网络处理逻辑
        /// </summary>
        private async Task<StrResponse> AnalyzeUrl(string url, Dictionary<string, string> customHeaders)
        {
            try
            {
                // 处理 Legado 特有的 URL 格式，如 "http://xyz.com,{'method':'POST'}"
                string method = "GET";
                string finalUrl = url;
                string body = null;
                string charset = null;

                // 简单的 URL 参数解析 (这里简化处理，完整版需要正则匹配 Legado 的 url 规则)
                if (url.Contains(",{"))
                {
                    var parts = url.Split(new[] { ",{" }, 2, StringSplitOptions.None);
                    finalUrl = parts[0];
                    string jsonStr = "{" + parts[1];
                    try
                    {
                        var json = JObject.Parse(jsonStr);
                        if (json["method"] != null) method = json["method"].ToString().ToUpper();
                        if (json["body"] != null) body = json["body"].ToString();
                        if (json["charset"] != null) charset = json["charset"].ToString();
                        // 更多 headers 处理...
                    }
                    catch { }
                }

                var request = new HttpRequestMessage(new HttpMethod(method), finalUrl);

                // 添加 Headers
                // 1. 默认 User-Agent
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                // 2. 传入的 Headers
                if (customHeaders != null)
                {
                    foreach (var kv in customHeaders)
                    {
                        request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                    }
                }

                // 3. POST Body
                if (method == "POST" && body != null)
                {
                    request.Content = new StringContent(body, Encoding.UTF8); // 默认 UTF8，具体视情况
                }

                // 发送请求
                var response = await _httpClient.SendAsync(request);
                var bytes = await response.Content.ReadAsByteArrayAsync();

                // 自动检测编码或使用指定编码
                Encoding encoding = Encoding.UTF8;
                if (!string.IsNullOrEmpty(charset))
                {
                    try { encoding = Encoding.GetEncoding(charset); } catch { }
                }
                else
                {
                    // 简易探测编码：检查 Content-Type 或 HTML meta
                    // 这里为了简化，默认 UTF-8，实际项目建议引入 CharsetDetector
                }

                string html = encoding.GetString(bytes);
                return new StrResponse(finalUrl, html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ajax Error] {ex.Message}");
                return new StrResponse(url, ex.Message); // Legado 出错通常返回 stacktrace
            }
        }
         

        // 模拟 Jsoup.connect 的 GET/POST/HEAD
        public object get(string url, object headers) => JsoupConnect(url, headers, "GET");
        public object post(string url, string body, object headers) => JsoupConnect(url, headers, "POST", body);

        private object JsoupConnect(string url, object headers, string method, string body = null)
        {
            // 这里返回一个模拟 Jsoup Response 的对象供 JS 调用
            // 由于 Jint 可以直接操作 C# 对象，我们返回一个包装好的 Result
            var headerDict = headers as Dictionary<string, string>;
            // ... 执行同步请求 (因为 Legado JS 规则通常是同步等待)
            var response = AnalyzeUrl(url, headerDict).GetAwaiter().GetResult();

            // 返回一个匿名对象或特定类，模拟 Jsoup Response 的常用方法
            return new
            {
                body = new Func<string>(() => response.Body),
                text = new Func<string>(() => response.Body),
                url = new Func<string>(() => response.Url),
                statusCode = 200 // 简化
            };
        }

        #endregion

        //#region WebView (Stub / Not Supported)

        //public string webView(string html, string url, string js)
        //{
        //    Console.WriteLine("[Warn] webView methods are not fully supported in headless C#.");
        //    // 如果必须支持，此处需要调用 PuppeteerSharp
        //    return "";
        //}

        //#endregion

        #region 文件操作 (File Operations)

        /// <summary>
        /// 获取缓存路径下的文件内容
        /// </summary>
        public string readTxtFile(string path)
        {
            string fullPath = GetSafePath(path);
            if (File.Exists(fullPath))
            {
                return File.ReadAllText(fullPath);
            }
            return "";
        }

        /// <summary>
        /// 下载文件到缓存
        /// </summary>
        public string downloadFile(string url)
        {
            try
            {
                var bytes = _httpClient.GetByteArrayAsync(url).Result;
                string md5 = Md5Encode16(url);
                // 简单猜测后缀
                string ext = ".tmp";
                if (url.Contains(".jpg")) ext = ".jpg";
                else if (url.Contains(".png")) ext = ".png";
                else if (url.Contains(".txt")) ext = ".txt";

                string fileName = md5 + ext;
                string fullPath = Path.Combine(_cachePath, fileName);

                File.WriteAllBytes(fullPath, bytes);
                return fileName; // 返回相对路径
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "";
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        public bool deleteFile(string path)
        {
            string fullPath = GetSafePath(path);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }

        // 安全路径检查，防止读取系统文件
        private string GetSafePath(string path)
        {
            string combined = Path.Combine(_cachePath, path.TrimStart('/', '\\'));
            string fullPath = Path.GetFullPath(combined);
            if (!fullPath.StartsWith(Path.GetFullPath(_cachePath)))
            {
                throw new System.Security.SecurityException("Illegal path access.");
            }
            return fullPath;
        }

        #endregion

        #region 压缩解压 (Archives via SharpCompress)

        public string unzipFile(string zipPath) => UnArchive(zipPath);
        public string un7zFile(string zipPath) => UnArchive(zipPath);
        public string unrarFile(string zipPath) => UnArchive(zipPath);

        private string UnArchive(string relPath)
        {
            if (string.IsNullOrEmpty(relPath)) return "";
            string fullPath = GetSafePath(relPath);
            if (!File.Exists(fullPath)) return "";

            string folderName = Md5Encode16(Path.GetFileName(fullPath));
            string targetDir = Path.Combine(_cachePath, "temp", folderName);

            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            try
            {
                using (var archive = ArchiveFactory.Open(fullPath))
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(targetDir, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
                // 返回相对路径
                return Path.Combine("temp", folderName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unzip failed: " + ex.Message);
                return "";
            }
        }

        public string getTxtInFolder(string path)
        {
            string fullPath = GetSafePath(path);
            if (!Directory.Exists(fullPath)) return "";

            StringBuilder sb = new StringBuilder();
            var files = Directory.GetFiles(fullPath);
            foreach (var f in files)
            {
                // 简单处理，假设都是 UTF8，实际需检测编码
                sb.Append(File.ReadAllText(f)).Append("\n");
            }
            // 模仿 Legado 删除文件夹
            Directory.Delete(fullPath, true);
            return sb.ToString().TrimEnd('\n');
        }

        #endregion

        #region 工具方法 (Utils)

        public string encodeURI(string str) => WebUtility.UrlEncode(str);

        public string htmlFormat(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            // 使用 AngleSharp 解析并格式化
            // 简单实现：Legado 的 HtmlFormatter 主要做的是把 <br> 换行标准化，去广告标签等
            // 这里用 AngleSharp 做一个简单的 parser
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(html);
            return doc.Body.TextContent; // 这是一个非常粗暴的转文本，如果需要保留格式需自定义遍历
        }

        public string timeFormat(long time)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(time).ToString("yyyy/MM/dd HH:mm");
        }

        // 转换繁简 (需引入其他库，这里仅占位)
        public string t2s(string text) => text;
        public string s2t(string text) => text;

        public void log(object msg)
        {
            Console.WriteLine($"[JS Log] {msg}");
        }

        public void toast(object msg)
        {
            Console.WriteLine($"[Toast] {msg}");
        }

        // Base64 扩展
        public string base64Decode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            byte[] bytes = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(bytes);
        }

        public string base64Encode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(bytes);
        }

        #endregion
    }
}