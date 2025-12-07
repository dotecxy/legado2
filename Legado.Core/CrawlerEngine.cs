using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Legado.Core
{
    /// <summary>
    /// 爬虫引擎
    /// </summary>
    public class CrawlerEngine
    {
        private readonly HttpClient _httpClient;
        private readonly IBrowsingContext _browsingContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CrawlerEngine()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            var config = Configuration.Default.WithDefaultLoader();
            _browsingContext = BrowsingContext.New(config);
        }

        /// <summary>
        /// 加载书源列表
        /// </summary>
        /// <param name="jsonContent">书源JSON内容</param>
        /// <returns>书源列表</returns>
        public List<BookSource> LoadBookSources(string jsonContent)
        {
            return JsonConvert.DeserializeObject<List<BookSource>>(jsonContent);
        }

        /// <summary>
        /// 搜索书籍
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="keyword">关键词</param>
        /// <returns>搜索结果</returns>
        public async Task<List<BookSearchResult>> SearchBooksAsync(BookSource bookSource, string keyword)
        {
            // 简单解析包含JavaScript的URL规则
            var processedUrl = ProcessUrlRule(bookSource, bookSource.searchUrl, keyword);
            var htmlContent = await FetchContentAsync(processedUrl);
            return ParseSearchResults(bookSource, htmlContent);
        }

        /// <summary>
        /// 处理URL规则
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="urlRule">URL规则</param>
        /// <param name="keyword">关键词</param>
        /// <returns>处理后的URL</returns>
        private string ProcessUrlRule(BookSource bookSource, string urlRule, string keyword)
        {
            if (string.IsNullOrEmpty(urlRule))
                return string.Empty;

            // 替换关键词
            var processedUrl = urlRule.Replace("{{key}}", Uri.EscapeDataString(keyword));

            // 简单处理JavaScript表达式
            if (processedUrl.StartsWith("@js:"))
            {
                // 提取JavaScript代码
                var jsCode = processedUrl.Substring(4);
                
                // 解析URL赋值表达式
                if (jsCode.Contains("url=") && jsCode.Contains(";result=url;"))
                {
                    // 提取URL赋值部分
                    var urlAssignment = jsCode.Split(new[] { ";result=url;" }, StringSplitOptions.None)[0];
                    var urlPart = urlAssignment.Substring(urlAssignment.IndexOf("url=") + 4);
                    
                    // 替换baseUrl
                    if (urlPart.Contains("baseUrl"))
                    {
                        // 处理baseUrl拼接
                        if (urlPart.Contains("baseUrl+'"))
                        {
                            urlPart = urlPart.Replace("baseUrl+'", bookSource.bookSourceUrl);
                        }
                        else
                        {
                            urlPart = urlPart.Replace("baseUrl", bookSource.bookSourceUrl);
                        }
                    }
                    
                    // 替换key
                    if (urlPart.Contains("key"))
                    {
                        // 处理key拼接
                        if (urlPart.Contains("key+'"))
                        {
                            urlPart = urlPart.Replace("key+'", Uri.EscapeDataString(keyword));
                        }
                        else if (urlPart.Contains("+'+key"))
                        {
                            urlPart = urlPart.Replace("+'+key", Uri.EscapeDataString(keyword));
                        }
                        else
                        {
                            urlPart = urlPart.Replace("key", Uri.EscapeDataString(keyword));
                        }
                    }
                    
                    // 移除所有的引号和加号
                    urlPart = urlPart.Replace("'", "").Replace("+", "");
                    
                    processedUrl = urlPart;
                }
            }
            // 处理相对URL
            else if (!processedUrl.StartsWith("http://") && !processedUrl.StartsWith("https://"))
            {
                // 确保书源基础URL以/结尾
                var baseUrl = bookSource.bookSourceUrl;
                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }
                
                // 移除相对URL开头的/（如果有）
                if (processedUrl.StartsWith("/"))
                {
                    processedUrl = processedUrl.Substring(1);
                }
                
                processedUrl = baseUrl + processedUrl;
            }

            return processedUrl;
        }

        /// <summary>
        /// 获取书籍信息
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="bookUrl">书籍URL</param>
        /// <returns>书籍信息</returns>
        public async Task<BookInfo> GetBookInfoAsync(BookSource bookSource, string bookUrl)
        {
            var htmlContent = await FetchContentAsync(bookUrl);
            return ParseBookInfo(bookSource, htmlContent, bookUrl);
        }

        /// <summary>
        /// 获取目录列表
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="tocUrl">目录URL</param>
        /// <returns>目录列表</returns>
        public async Task<List<ChapterInfo>> GetChapterListAsync(BookSource bookSource, string tocUrl)
        {
            var htmlContent = await FetchContentAsync(tocUrl);
            return ParseChapterList(bookSource, htmlContent);
        }

        /// <summary>
        /// 获取章节内容
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="chapterUrl">章节URL</param>
        /// <returns>章节内容</returns>
        public async Task<ChapterContent> GetChapterContentAsync(BookSource bookSource, string chapterUrl)
        {
            var htmlContent = await FetchContentAsync(chapterUrl);
            return ParseChapterContent(bookSource, htmlContent);
        }

        /// <summary>
        /// 发送HTTP请求获取内容
        /// </summary>
        /// <param name="url">请求URL</param>
        /// <returns>HTML内容</returns>
        private async Task<string> FetchContentAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch content from {url}: {ex.Message}");
            }
        }

        /// <summary>
        /// 解析搜索结果
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="htmlContent">HTML内容</param>
        /// <returns>搜索结果列表</returns>
        private List<BookSearchResult> ParseSearchResults(BookSource bookSource, string htmlContent)
        {
            var results = new List<BookSearchResult>();
            var document = _browsingContext.OpenAsync(req => req.Content(htmlContent)).Result;

            var bookListSelector = bookSource.ruleSearch.bookList;
            var bookItems = document.QuerySelectorAll(bookListSelector);

            foreach (var item in bookItems)
            {
                var result = new BookSearchResult
                {
                    Name = ExtractContent(item, bookSource.ruleSearch.name),
                    Author = ExtractContent(item, bookSource.ruleSearch.author),
                    Kind = ExtractContent(item, bookSource.ruleSearch.kind),
                    LastChapter = ExtractContent(item, bookSource.ruleSearch.lastChapter),
                    BookUrl = ResolveUrl(bookSource.bookSourceUrl, ExtractContent(item, bookSource.ruleSearch.bookUrl)),
                    WordCount = ExtractContent(item, bookSource.ruleSearch.wordCount)
                };

                if (!string.IsNullOrEmpty(result.Name))
                {
                    results.Add(result);
                }
            }

            return results;
        }

        /// <summary>
        /// 解析书籍信息
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="htmlContent">HTML内容</param>
        /// <param name="bookUrl">书籍URL</param>
        /// <returns>书籍信息</returns>
        private BookInfo ParseBookInfo(BookSource bookSource, string htmlContent, string bookUrl)
        {
            var document = _browsingContext.OpenAsync(req => req.Content(htmlContent)).Result;

            var bookInfo = new BookInfo
            {
                Name = ExtractContent(document, bookSource.ruleBookInfo.name),
                Author = ExtractContent(document, bookSource.ruleBookInfo.author),
                CoverUrl = ResolveUrl(bookSource.bookSourceUrl, ExtractContent(document, bookSource.ruleBookInfo.coverUrl)),
                Intro = ExtractContent(document, bookSource.ruleBookInfo.intro),
                Kind = ExtractContent(document, bookSource.ruleBookInfo.kind),
                LastChapter = ExtractContent(document, bookSource.ruleBookInfo.lastChapter),
                BookUrl = bookUrl,
                TocUrl = string.IsNullOrEmpty(bookSource.ruleBookInfo.tocUrl) ? bookUrl : ResolveUrl(bookSource.bookSourceUrl, bookSource.ruleBookInfo.tocUrl)
            };

            return bookInfo;
        }

        /// <summary>
        /// 解析目录列表
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="htmlContent">HTML内容</param>
        /// <returns>目录列表</returns>
        private List<ChapterInfo> ParseChapterList(BookSource bookSource, string htmlContent)
        {
            var results = new List<ChapterInfo>();
            var document = _browsingContext.OpenAsync(req => req.Content(htmlContent)).Result;

            var chapterListSelector = bookSource.ruleToc.chapterList;
            var chapterItems = document.QuerySelectorAll(chapterListSelector);

            foreach (var item in chapterItems)
            {
                var chapter = new ChapterInfo
                {
                    ChapterName = ExtractContent(item, bookSource.ruleToc.chapterName),
                    ChapterUrl = ResolveUrl(bookSource.bookSourceUrl, ExtractContent(item, bookSource.ruleToc.chapterUrl))
                };

                if (!string.IsNullOrEmpty(chapter.ChapterName) && !string.IsNullOrEmpty(chapter.ChapterUrl))
                {
                    results.Add(chapter);
                }
            }

            return results;
        }

        /// <summary>
        /// 解析章节内容
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="htmlContent">HTML内容</param>
        /// <returns>章节内容</returns>
        private ChapterContent ParseChapterContent(BookSource bookSource, string htmlContent)
        {
            var document = _browsingContext.OpenAsync(req => req.Content(htmlContent)).Result;

            var content = ExtractContent(document, bookSource.ruleContent.content);
            var nextContentUrl = ExtractContent(document, bookSource.ruleContent.nextContentUrl);

            // 处理内容替换规则
            if (!string.IsNullOrEmpty(bookSource.ruleContent.replaceRegex))
            {
                content = ProcessReplaceRules(content, bookSource.ruleContent.replaceRegex);
            }

            return new ChapterContent
            {
                Content = content,
                NextContentUrl = string.IsNullOrEmpty(nextContentUrl) ? null : ResolveUrl(bookSource.bookSourceUrl, nextContentUrl)
            };
        }

        /// <summary>
        /// 提取内容
        /// </summary>
        /// <param name="context">DOM上下文</param>
        /// <param name="selector">选择器规则</param>
        /// <returns>提取的内容</returns>
        private string ExtractContent(INode context, string selector)
        {
            if (string.IsNullOrEmpty(selector))
                return string.Empty;

            try
            {
                // 处理不同类型的选择器
                if (context is IParentNode parent)
                {
                    // 简单处理CSS选择器
                    if (!selector.StartsWith("@"))
                    {
                        // 尝试使用CSS选择器
                        var element = parent.QuerySelector(selector);
                        if (element != null)
                        {
                            return element.TextContent.Trim();
                        }
                    }
                    // 处理JavaScript选择器
                    else if (selector.StartsWith("@js:"))
                    {
                        // 这里可以添加JavaScript选择器的处理逻辑
                        return string.Empty;
                    }
                    // 处理其他类型的选择器
                    else
                    {
                        // 尝试直接获取节点内容
                        return context.TextContent?.Trim() ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录选择器解析错误，但不中断程序
                Console.WriteLine($"选择器解析错误: {selector} - {ex.Message}");
            }

            return string.Empty;
        }

        /// <summary>
        /// 处理替换规则
        /// </summary>
        /// <param name="content">原始内容</param>
        /// <param name="replaceRegex">替换规则</param>
        /// <returns>处理后的内容</returns>
        private string ProcessReplaceRules(string content, string replaceRegex)
        {
            // 简单实现，实际需要解析更复杂的规则
            return content;
        }

        /// <summary>
        /// 解析URL
        /// </summary>
        /// <param name="baseUrl">基础URL</param>
        /// <param name="relativeUrl">相对URL</param>
        /// <returns>完整URL</returns>
        private string ResolveUrl(string baseUrl, string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl))
                return string.Empty;

            if (Uri.IsWellFormedUriString(relativeUrl, UriKind.Absolute))
                return relativeUrl;

            if (Uri.TryCreate(new Uri(baseUrl), relativeUrl, out var resolvedUri))
                return resolvedUri.ToString();

            return relativeUrl;
        }
    }

    /// <summary>
    /// 搜索结果
    /// </summary>
    public class BookSearchResult
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Kind { get; set; }
        public string LastChapter { get; set; }
        public string BookUrl { get; set; }
        public string WordCount { get; set; }
    }

    /// <summary>
    /// 书籍信息
    /// </summary>
    public class BookInfo
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string CoverUrl { get; set; }
        public string Intro { get; set; }
        public string Kind { get; set; }
        public string LastChapter { get; set; }
        public string BookUrl { get; set; }
        public string TocUrl { get; set; }
    }

    /// <summary>
    /// 章节信息
    /// </summary>
    public class ChapterInfo
    {
        public string ChapterName { get; set; }
        public string ChapterUrl { get; set; }
    }

    /// <summary>
    /// 章节内容
    /// </summary>
    public class ChapterContent
    {
        public string Content { get; set; }
        public string NextContentUrl { get; set; }
    }
}