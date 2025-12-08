//using Legado.Core.Data.Entities;
//using Legado.Core.Helps;
//using Legado.Core.Models.AnalyzeRules;
//using Legado.Models.AnalyzeRules;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace Legado.Core.Models
//{
//    /// <summary>
//    /// 网络书籍处理引擎 (对应 WebBook.kt)
//    /// </summary>
//    public class WebBook
//    {
//        private readonly BookSource _source;
//        static readonly string webViewTrue = JsonConvert.SerializeObject(new { webView = true });

//        public WebBook(BookSource source)
//        {
//            _source = source;
//        }

//        /// <summary>
//        /// 执行搜索
//        /// </summary>
//        /// <param name="key">搜索关键字</param>
//        /// <param name="page">页码 (从1开始)</param>
//        public async Task<List<SearchBook>> SearchBookAsync(string key, int page = 1)
//        {
//            if (string.IsNullOrEmpty(_source.SearchUrl) || _source.RuleSearch == null)
//            {
//                return new List<SearchBook>();
//            }

//            // 1. 处理 URL 变量替换 ({{key}}, {{page}})
//            //string targetPath = ruleItem.Url.Replace("{{page}}", page.ToString());

//            string url = AnalyzeUrlParams(_source.SearchUrl, key, page);

//            // 2. 发起网络请求
//            // 搜索规则的 baseUrl 通常就是 SearchUrl 处理后的结果，或者是 SourceUrl
//            var analyzeUrl = new AnalyzeUrl(url, baseUrl: _source.BookSourceUrl, source: _source);
//            var response = await analyzeUrl.GetStrResponseAsync();

//            if (response.IsError)
//            {
//                //Debug.Log($"Search Error: {response.Body}");
//                return new List<SearchBook>();
//            }

//            // 3. 解析结果
//            return AnalyzeSearchBook(response.Body, _source.RuleSearch, isSearch: true, baseUrl: analyzeUrl.Url);
//        }

//        /// <summary>
//        /// 通用书籍列表解析逻辑 (对应 WebBook.kt 中的 analyzeSearchBook)
//        /// </summary>
//        private List<SearchBook> AnalyzeSearchBook(string content, RuleSearch ruleJson, bool isSearch, string baseUrl)
//        {
//            var books = new List<SearchBook>();

//            // 1. 解析规则 JSON 字符串
//            if (ruleJson == null)
//            {
//                return books;
//            }

//            // 2. 提取列表集合 (bookList)
//            string bookListRule = ruleJson.BookList;
//            if (string.IsNullOrEmpty(bookListRule))
//            {
//                // 如果没有列表规则，可能整个 Body 就是一个对象，或者无法解析
//                // 此时可以尝试直接解析字段，但通常 bookList 是必须的
//                // 这里做兼容处理：假设 content 本身就是列表(JSON)或根节点(HTML)
//                // 实际 Legado 逻辑会更复杂，这里简化
//            }

//            // 准备解析器
//            // SearchBook 只是临时对象，不需要绑定具体的 Book 实体
//            var ruleAnalyzer = new AnalyzeRule();

//            // 执行列表提取
//            // 判断规则类型：JSONPath ($开头) 还是 CSS/Jsoup
//            List<object> collections;

//            // 这里为了简单，复用了之前的静态方法。实际应该集成到 AnalyzeRule 中
//            if (bookListRule != null && (bookListRule.StartsWith("$") || bookListRule.StartsWith("@json:")))
//            {
//                collections = AnalyzeByJsonPath.Analyze(content, bookListRule);
//            }
//            else if (bookListRule != null && bookListRule.StartsWith("<js>"))
//            {
//                collections = AnalyzeByJsonPath.Analyze(content, bookListRule);
//            }
//            else
//            {
//                collections = AnalyzeByAngleSharp.Analyze(content, bookListRule);
//            }

//            if (collections == null || collections.Count == 0) return books;

//            // 3. 遍历列表提取字段
//            foreach (var item in collections)
//            {
//                var searchBook = new SearchBook();
//                searchBook.Origin = _source.BookSourceUrl;


//                searchBook.Name = AnalyzeByAngleSharp.Analyze(item, ruleJson.Name)?.FirstOrDefault()?.ToString();
//                searchBook.Author = AnalyzeByAngleSharp.Analyze(item, ruleJson.Author)?.FirstOrDefault()?.ToString();
//                searchBook.Kind = AnalyzeByAngleSharp.Analyze(item, ruleJson.Kind)?.FirstOrDefault()?.ToString();
//                searchBook.Intro = AnalyzeByAngleSharp.Analyze(item, ruleJson.Intro)?.FirstOrDefault()?.ToString();
//                searchBook.CoverUrl = AnalyzeByAngleSharp.Analyze(item, ruleJson.CoverUrl)?.FirstOrDefault()?.ToString();
//                searchBook.BookUrl = AnalyzeByAngleSharp.Analyze(item, ruleJson.BookUrl)?.FirstOrDefault()?.ToString();
//                searchBook.LatestChapterTitle = AnalyzeByAngleSharp.Analyze(item, ruleJson.LastChapter)?.FirstOrDefault()?.ToString();  // Legado 规则里字段名叫 lastChapter



//                // 简单的 HTML 标签清理 (如书名里包含 <b>key</b>)
//                searchBook.Name = CleanHtml(searchBook.Name);
//                searchBook.Author = CleanHtml(searchBook.Author);

//                books.Add(searchBook);
//            }

//            return books;
//        }

//        /// <summary>
//        /// URL 参数替换 (对应 AnalyzeUrl.kt 中的处理，但在 WebBook 层先处理变量)
//        /// </summary>
//        private string AnalyzeUrlParams(string urlRule, string key, int page)
//        {

//            if (string.IsNullOrEmpty(urlRule)) return "";


//            UrlRuleParser parser = new UrlRuleParser();
//            var uri = parser.ParseUrl(urlRule, _source.BookSourceUrl, key, page);
//            return uri + "," + webViewTrue;


//            // 1. 处理 Legado 的 <js> 标签 (如果在 URL 里)
//            // C# 中简单处理：先执行 JS 替换，再做变量替换
//            // 完整实现需要 Jint 介入，这里简化为只替换变量

//            string url = urlRule;

//            // 2. 替换关键字
//            if (url.Contains("{{key}}"))
//            {
//                // 注意 URL 编码
//                // Legado 默认编码取决于 Source 的 charset，这里暂用 UTF-8
//                string encodedKey = System.Net.WebUtility.UrlEncode(key);
//                url = url.Replace("{{key}}", encodedKey);
//            }

//            // 3. 替换页码
//            if (url.Contains("{{page}}"))
//            {
//                url = url.Replace("{{page}}", page.ToString());
//            }

//            // 4. 处理 searchPage - 1 这种简单的计算 (Legado 支持 {{page-1}})
//            // 简单正则处理 {{page-1}}
//            url = Regex.Replace(url, @"\{\{page\s*-\s*1\}\}", (match) => (page - 1).ToString());

//            return url;
//        }

//        private string CompleteUrl(string baseUrl, string relativeUrl)
//        {
//            if (string.IsNullOrEmpty(relativeUrl)) return "";
//            if (relativeUrl.StartsWith("http")) return relativeUrl;
//            try
//            {
//                return Flurl.Url.Combine(baseUrl, relativeUrl);
//            }
//            catch
//            {
//                return relativeUrl;
//            }
//        }

//        private string CleanHtml(string html)
//        {
//            if (string.IsNullOrEmpty(html)) return "";
//            // 简单去除 HTML 标签
//            return Regex.Replace(html, "<.*?>", "").Trim();
//        }

//        /// <summary>
//        /// 1. 获取目录列表
//        /// </summary>
//        public async Task<List<BookChapter>> GetChapterList(Book book)
//        {
//            var chapterList = new List<BookChapter>();
//            string tocUrl = string.IsNullOrEmpty(book.TocUrl) ? book.BookUrl : book.TocUrl;

//            // 1. 网络请求
//            var analyzerUrl = new AnalyzeUrl(tocUrl, baseUrl: _source.BookSourceUrl, source: _source);
//            var response = await analyzerUrl.GetStrResponseAsync();
//            if (response.IsError) throw new Exception(response.Body);

//            string html = response.Body;

//            // 2. 准备规则解析器
//            var ruleAnalyzer = new AnalyzeRule(book);

//            // 3. 提取目录列表对象 (List of Elements / JArray)
//            // 规则: ruleToc.chapterList (例如 "div.list a" 或 "$.data.list[*]")
//            // 这里我们需要 AnalyzeRule 能返回原始对象而不是 String，所以 AnalyzeRule 需要稍微扩展或直接调用底层
//            // 为了简化，假设 AnalyzeRule.Analyze 返回 List<string> 不够用，我们直接调用底层逻辑：

//            List<object> collections;

//            // 简化的逻辑：根据 ruleToc 的首字符判断是用 CSS 还是 JSON 提取列表集合
//            string listRule = _source.RuleToc.ChapterList; // 需实现解析 JSON 规则对象的方法

//            if (string.IsNullOrEmpty(listRule))
//            {
//                // 如果没有列表规则，可能整个 body 就是
//                collections = new List<object> { html };
//            }
//            else if (listRule.StartsWith("$"))
//            {
//                collections = AnalyzeByJsonPath.Analyze(html, listRule);
//            }
//            else
//            {
//                // HTML
//                collections = AnalyzeByAngleSharp.Analyze(html, listRule);
//            }

//            if (collections == null) return chapterList;

//            // 4. 遍历集合提取章节信息
//            int index = 0;
//            string nameRule = _source.RuleToc.ChapterName;
//            string urlRule = _source.RuleToc.ChapterList;

//            foreach (var item in collections)
//            {
//                // 提取标题
//                var names = ruleAnalyzer.Analyze(item, nameRule);
//                string title = names.FirstOrDefault() ?? "";

//                // 提取URL
//                var urls = ruleAnalyzer.Analyze(item, urlRule);
//                string url = urls.FirstOrDefault() ?? "";

//                // 处理相对路径
//                if (!url.StartsWith("http"))
//                {
//                    url = Flurl.Url.Combine(_source.BookSourceUrl, url);
//                }

//                chapterList.Add(new BookChapter
//                {
//                    Index = index++,
//                    Title = title,
//                    Url = url,
//                    BookUrl = book.BookUrl
//                });
//            }

//            // TODO: 处理下一页 (ruleToc.nextTocUrl) 的递归逻辑

//            return chapterList;
//        }

//        /// <summary>
//        /// 2. 获取正文内容
//        /// </summary>
//        public async Task<string> GetContent(BookChapter chapter)
//        {
//            // 1. 网络请求
//            var analyzerUrl = new AnalyzeUrl(chapter.Url, baseUrl: _source.BookSourceUrl, source: _source);
//            var response = await analyzerUrl.GetStrResponseAsync();

//            // 2. 解析规则
//            var ruleAnalyzer = new AnalyzeRule(chapter: chapter);

//            // 提取正文规则
//            string contentRule = GetRule(_source.RuleContent, "content");

//            // 解析内容
//            var results = ruleAnalyzer.Analyze(response.Body, contentRule);
//            string content = string.Join("\n", results);

//            // 3. 执行净化替换 (ruleContent.replaceRegex)
//            string replaceRule = GetRule(_source.RuleContent, "replaceRegex");
//            if (!string.IsNullOrEmpty(replaceRule))
//            {
//                // Legado 的 replaceRegex 也是 ## 分割的
//                string[] replaces = replaceRule.Split(new[] { "##" }, StringSplitOptions.RemoveEmptyEntries);
//                foreach (var r in replaces)
//                {
//                    // 简单正则清理
//                    try { content = Regex.Replace(content, r, ""); } catch { }
//                }
//            }

//            // 4. 处理分页内容 (nextContentUrl) - 此处省略递归

//            return content;
//        }

//        // 辅助：从 Legado 的规则 JSON/XML 结构中提取特定字段
//        // 因为 BookSource 中的 RuleToc 实际上是一个 JSON 字符串，如 {"chapterList":"div.a", "chapterName":"text"}
//        private string GetRule(string ruleJson, string key)
//        {
//            if (string.IsNullOrEmpty(ruleJson)) return "";
//            try
//            {
//                // 如果规则本身不是 JSON (旧版 Legado 可能是纯字符串)，则直接返回
//                if (!ruleJson.Trim().StartsWith("{")) return ruleJson;

//                var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(ruleJson);
//                return jsonObj[key]?.ToString();
//            }
//            catch
//            {
//                return ""; // 解析失败或不存在
//            }
//        }

//        private string GetRule(RuleContent ruleContent, string key)
//        {
//            if (ruleContent == null) return "";
//            try
//            {
//                return "";
//            }
//            catch
//            {
//                return ""; // 解析失败或不存在
//            }
//        }
//    }
//}