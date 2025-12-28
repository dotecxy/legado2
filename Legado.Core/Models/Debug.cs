using Legado.Core.Data.Entities;
using Legado.Core.Models.WebBooks;
using Legado.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Legado.Core.Models
{
    /// <summary>
    /// 调试工具类（对应 Kotlin 的 Debug.kt）
    /// </summary>
    public static class Debug
    {
        /// <summary>
        /// 调试回调接口
        /// </summary>
        public interface ICallback
        {
            void PrintLog(int state, string msg);
        }

        public static ICallback Callback { get; set; }
        private static string _debugSource;
        private static readonly ConcurrentDictionary<string, string> DebugMessageMap = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, long> DebugTimeMap = new ConcurrentDictionary<string, long>();
        public static bool IsChecking { get; set; } = false;

        private static long _startTime;
        private static readonly object _lock = new object();
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// 调试超时时间（毫秒）
        /// </summary>
        public const long Timeout = 60000; // 60秒

        /// <summary>
        /// 记录调试日志
        /// 对应 Kotlin 的 log()
        /// </summary>
        public static void Log(
            string sourceUrl,
            string msg = "",
            bool print = true,
            bool isHtml = false,
            bool showTime = true,
            int state = 1)
        {
            lock (_lock)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"sourceDebug: {msg}");
#endif

                // 调试信息始终要执行
                if (Callback != null)
                {
                    if ((_debugSource != sourceUrl || !print))
                        return;

                    var printMsg = msg;
                    if (isHtml)
                    {
                        printMsg = HtmlFormatter.Format(printMsg);
                    }

                    if (showTime)
                    {
                        var time = FormatDebugTime(DateTimeOffset.Now.ToUnixTimeMilliseconds() - _startTime);
                        printMsg = $"{time} {printMsg}";
                    }

                    Callback.PrintLog(state, printMsg);
                }

                if (IsChecking && sourceUrl != null && msg.Length < 30)
                {
                    var printMsg = msg;
                    if (isHtml)
                    {
                        printMsg = HtmlFormatter.Format(printMsg);
                    }

                    if (showTime && DebugTimeMap.TryGetValue(sourceUrl, out var debugTime))
                    {
                        var time = FormatDebugTime(DateTimeOffset.Now.ToUnixTimeMilliseconds() - debugTime);
                        printMsg = printMsg.Replace("┌", "").Replace("└", "").Replace("︽", "").Replace("︾", "").Replace("◇", "").Replace("⇒", "").Replace("≡", "");
                        DebugMessageMap[sourceUrl] = $"{time} {printMsg}";
                    }
                }
            }
        }

        /// <summary>
        /// 记录调试日志（简化版）
        /// 对应 Kotlin 的 log(msg: String?)
        /// </summary>
        public static void Log(string msg)
        {
            Log(_debugSource, msg ?? "", true);
        }

        /// <summary>
        /// 取消调试
        /// 对应 Kotlin 的 cancelDebug()
        /// </summary>
        public static void CancelDebug(bool destroy = false)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            if (destroy)
            {
                _debugSource = null;
                Callback = null;
            }
        }

        /// <summary>
        /// 开始校验
        /// 对应 Kotlin 的 startChecking()
        /// </summary>
        public static void StartChecking(BookSource source)
        {
            IsChecking = true;
            DebugTimeMap[source.BookSourceUrl] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            DebugMessageMap[source.BookSourceUrl] = $"{FormatDebugTime(0)} 开始校验";
        }

        /// <summary>
        /// 完成校验
        /// 对应 Kotlin 的 finishChecking()
        /// </summary>
        public static void FinishChecking()
        {
            IsChecking = false;
        }

        /// <summary>
        /// 获取响应时间
        /// 对应 Kotlin 的 getRespondTime()
        /// </summary>
        public static long GetRespondTime(string sourceUrl)
        {
            return DebugTimeMap.TryGetValue(sourceUrl, out var time) ? time : Timeout;
        }

        /// <summary>
        /// 更新最终消息
        /// 对应 Kotlin 的 updateFinalMessage()
        /// </summary>
        public static void UpdateFinalMessage(string sourceUrl, string state)
        {
            if (DebugTimeMap.TryGetValue(sourceUrl, out var debugTime) && 
                DebugMessageMap.ContainsKey(sourceUrl))
            {
                var spendingTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - debugTime;
                DebugTimeMap[sourceUrl] = state == "校验成功" ? spendingTime : Timeout + spendingTime;
                var printTime = FormatDebugTime(spendingTime);
                DebugMessageMap[sourceUrl] = $"{printTime} {state}";
            }
        }

        /// <summary>
        /// 开始调试 RSS 源
        /// 对应 Kotlin 的 startDebug(scope, rssSource)
        /// </summary>
        public static async Task StartDebug(RssSource rssSource)
        {
            CancelDebug();
            _debugSource = rssSource.SourceUrl;
            Log(_debugSource, "︾开始解析");

            // TODO: 实现 RSS 调试逻辑
            Log(_debugSource, "⇒RSS 调试功能待实现", state: -1);
        }

        /// <summary>
        /// 开始调试书源
        /// 对应 Kotlin 的 startDebug(scope, bookSource, key)
        /// </summary>
        public static async Task StartDebug(BookSource bookSource, string key)
        {
            CancelDebug();
            _debugSource = bookSource.BookSourceUrl;
            _startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                if (IsAbsUrl(key))
                {
                    var book = new Book
                    {
                        Origin = bookSource.BookSourceUrl,
                        BookUrl = key
                    };
                    Log(bookSource.BookSourceUrl, $"⇒开始访问详情页:{key}");
                    await InfoDebug(bookSource, book, cancellationToken);
                }
                else if (key.Contains("::"))
                {
                    var url = key.Substring(key.IndexOf("::") + 2);
                    Log(bookSource.BookSourceUrl, $"⇒开始访问发现页:{url}");
                    await ExploreDebug(bookSource, url, cancellationToken);
                }
                else if (key.StartsWith("++"))
                {
                    var url = key.Substring(2);
                    var book = new Book
                    {
                        Origin = bookSource.BookSourceUrl,
                        TocUrl = url
                    };
                    Log(bookSource.BookSourceUrl, $"⇒开始访目录页:{url}");
                    await TocDebug(bookSource, book, cancellationToken);
                }
                else if (key.StartsWith("--"))
                {
                    var url = key.Substring(2);
                    var book = new Book
                    {
                        Origin = bookSource.BookSourceUrl
                    };
                    Log(bookSource.BookSourceUrl, $"⇒开始访正文页:{url}");
                    var chapter = new BookChapter
                    {
                        Title = "调试",
                        Url = url
                    };
                    await ContentDebug(bookSource, book, chapter, null, cancellationToken);
                }
                else
                {
                    Log(bookSource.BookSourceUrl, $"⇒开始搜索关键字:{key}");
                    await SearchDebug(bookSource, key, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Log(_debugSource, "⇒调试已取消", state: -1);
            }
            catch (Exception ex)
            {
                Log(_debugSource, $"⇒调试异常: {ex.Message}\n{ex.StackTrace}", state: -1);
            }
        }

        /// <summary>
        /// 发现页调试
        /// 对应 Kotlin 的 exploreDebug()
        /// </summary>
        private static async Task ExploreDebug(BookSource bookSource, string url, CancellationToken cancellationToken)
        {
            Log(_debugSource, "︾开始解析发现页");
            try
            {
                var webBook = new WebBook();
                var exploreBooks = await webBook.ExploreBookAsync(bookSource, url, 1);

                if (exploreBooks != null && exploreBooks.Count > 0)
                {
                    Log(_debugSource, "︽发现页解析完成");
                    Log(_debugSource, "", showTime: false);
                    var book = ToBook(exploreBooks[0]);
                    await InfoDebug(bookSource, book, cancellationToken);
                }
                else
                {
                    Log(_debugSource, "︽未获取到书籍", state: -1);
                }
            }
            catch (Exception ex)
            {
                Log(_debugSource, $"{ex.Message}\n{ex.StackTrace}", state: -1);
            }
        }

        /// <summary>
        /// 搜索页调试
        /// 对应 Kotlin 的 searchDebug()
        /// </summary>
        private static async Task SearchDebug(BookSource bookSource, string key, CancellationToken cancellationToken)
        {
            Log(_debugSource, "︾开始解析搜索页");
            try
            {
                var webBook = new WebBook();
                var searchBooks = await webBook.SearchBookAsync(bookSource, key, 1);

                if (searchBooks != null && searchBooks.Count > 0)
                {
                    Log(_debugSource, "︽搜索页解析完成");
                    Log(_debugSource, "", showTime: false);
                    var book = ToBook(searchBooks[0]);
                    await InfoDebug(bookSource, book, cancellationToken);
                }
                else
                {
                    Log(_debugSource, "︽未获取到书籍", state: -1);
                }
            }
            catch (Exception ex)
            {
                Log(_debugSource, $"{ex.Message}\n{ex.StackTrace}", state: -1);
            }
        }

        /// <summary>
        /// 详情页调试
        /// 对应 Kotlin 的 infoDebug()
        /// </summary>
        private static async Task InfoDebug(BookSource bookSource, Book book, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(book.TocUrl))
            {
                Log(_debugSource, "≡已获取目录链接,跳过详情页");
                Log(_debugSource, "", showTime: false);
                await TocDebug(bookSource, book, cancellationToken);
                return;
            }

            Log(_debugSource, "︾开始解析详情页");
            try
            {
                var webBook = new WebBook();
                await webBook.GetBookInfoAsync(bookSource, book);

                Log(_debugSource, "︽详情页解析完成");
                Log(_debugSource, "", showTime: false);

                // TODO: 检查是否为网络文件
                // if (!book.isWebFile)
                await TocDebug(bookSource, book, cancellationToken);
            }
            catch (Exception ex)
            {
                Log(_debugSource, $"{ex.Message}\n{ex.StackTrace}", state: -1);
            }
        }

        /// <summary>
        /// 目录页调试
        /// 对应 Kotlin 的 tocDebug()
        /// </summary>
        private static async Task TocDebug(BookSource bookSource, Book book, CancellationToken cancellationToken)
        {
            Log(_debugSource, "︾开始解析目录页");
            try
            {
                var webBook = new WebBook();
                var chapters = await webBook.GetChapterListAsync(bookSource, book, false, cancellationToken);

                Log(_debugSource, "︽目录页解析完成");
                Log(_debugSource, "", showTime: false);

                // 过滤掉卷标且 URL 以标题开头的章节
                var toc = chapters.Where(c => !(c.IsVolume && c.Url.StartsWith(c.Title))).ToList();

                if (toc.Count == 0)
                {
                    Log(_debugSource, "≡没有正文章节");
                    return;
                }

                var nextChapterUrl = toc.Count > 1 ? toc[1].Url : toc[0].Url;
                await ContentDebug(bookSource, book, toc[0], nextChapterUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                Log(_debugSource, $"{ex.Message}\n{ex.StackTrace}", state: -1);
            }
        }

        /// <summary>
        /// 正文页调试
        /// 对应 Kotlin 的 contentDebug()
        /// </summary>
        private static async Task ContentDebug(
            BookSource bookSource,
            Book book,
            BookChapter bookChapter,
            string nextChapterUrl,
            CancellationToken cancellationToken)
        {
            Log(_debugSource, "︾开始解析正文页");
            try
            {
                var webBook = new WebBook();
                var content = await webBook.GetContentAsync(bookSource, book, bookChapter, nextChapterUrl);

                Log(_debugSource, "︽正文页解析完成", state: 1000);
            }
            catch (Exception ex)
            {
                Log(_debugSource, $"{ex.Message}\n{ex.StackTrace}", state: -1);
            }
        }

        /// <summary>
        /// SearchBook 转换为 Book
        /// 对应 Kotlin 的 SearchBook.toBook()
        /// </summary>
        private static Book ToBook(SearchBook searchBook)
        {
            return new Book
            {
                BookUrl = searchBook.BookUrl,
                Origin = searchBook.Origin,
                OriginName = searchBook.OriginName,
                OriginOrder = searchBook.OriginOrder,
                Type = searchBook.Type,
                Name = searchBook.Name,
                Author = searchBook.Author,
                Kind = searchBook.Kind,
                CoverUrl = searchBook.CoverUrl,
                Intro = searchBook.Intro,
                WordCount = searchBook.WordCount,
                LatestChapterTitle = searchBook.LatestChapterTitle,
                TocUrl = searchBook.TocUrl,
                Variable = searchBook.Variable
            };
        }

        /// <summary>
        /// 判断是否为绝对 URL
        /// 对应 Kotlin 的 String.isAbsUrl()
        /// </summary>
        private static bool IsAbsUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }
         

        /// <summary>
        /// 格式化调试时间
        /// 对应 Kotlin 的 SimpleDateFormat("[mm:ss.SSS]")
        /// </summary>
        private static string FormatDebugTime(long milliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
            return $"[{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}]";
        }
    }
}
