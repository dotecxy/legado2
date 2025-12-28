using Legado.Core.Constants;
using Legado.Core.Data.Entities;
using Legado.Core.Data.Entities.Rules;
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 获取正文 (对应 Kotlin: BookContent.kt)
    /// </summary>
    public static class BookContent
    {
        /// <summary>
        /// 解析正文内容
        /// 对应 Kotlin 的 analyzeContent
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="book">书籍</param>
        /// <param name="bookChapter">章节</param>
        /// <param name="baseUrl">基础URL</param>
        /// <param name="redirectUrl">重定向URL</param>
        /// <param name="body">HTML内容</param>
        /// <param name="nextChapterUrl">下一章URL</param>
        /// <param name="needSave">是否需要保存</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>正文内容</returns>
        public static async Task<string> AnalyzeContent(
            BookSource bookSource,
            Book book,
            BookChapter bookChapter,
            string baseUrl,
            string redirectUrl,
            string body,
            string nextChapterUrl = null,
            bool needSave = true,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new Exception($"Error getting web content: {baseUrl}");
            }

            Debug.Log(bookSource.BookSourceUrl, $"≡获取成功:{baseUrl}");
            Debug.Log(bookSource.BookSourceUrl, body, state: 40);

            // 获取下一章URL
            var mNextChapterUrl = nextChapterUrl;
            if (string.IsNullOrEmpty(mNextChapterUrl))
            {
                // TODO: 从数据库获取下一章URL
                // mNextChapterUrl = appDb.bookChapterDao.getChapter(book.bookUrl, bookChapter.index + 1)?.url
                //     ?? appDb.bookChapterDao.getChapter(book.bookUrl, 0)?.url;
            }

            var contentList = new List<string>();
            var nextUrlList = new List<string> { redirectUrl };
            var contentRule = bookSource.GetContentRule();

            var analyzeRule = new AnalyzeRule(book, bookSource);
            analyzeRule.SetContent(body, baseUrl);
            analyzeRule.SetRedirectUrl(redirectUrl);
            // TODO: analyzeRule.SetCoroutineContext(coroutineContext);
            analyzeRule.SetChapter(bookChapter);
            analyzeRule.SetNextChapterUrl(mNextChapterUrl);

            // TODO: coroutineContext.ensureActive()
            cancellationToken.ThrowIfCancellationRequested();

            // 获取标题
            var titleRule = contentRule.Title;
            if (!string.IsNullOrWhiteSpace(titleRule))
            {
                try
                {
                    var title = analyzeRule.GetString(titleRule);
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        bookChapter.Title = title;
                        // TODO: bookChapter.TitleMD5 = null;
                        // TODO: appDb.bookChapterDao.update(bookChapter);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(bookSource.BookSourceUrl, $"获取标题出错, {e.Message}");
                }
            }

            // 解析正文
            var contentData = await AnalyzeContentInternal(
                book, baseUrl, redirectUrl, body, contentRule, bookChapter, bookSource, mNextChapterUrl,
                cancellationToken: cancellationToken);
            contentList.Add(contentData.Content);

            // 单页次页模式
            if (contentData.NextUrlList.Count == 1)
            {
                var nextUrl = contentData.NextUrlList[0];
                while (!string.IsNullOrEmpty(nextUrl) && !nextUrlList.Contains(nextUrl))
                {
                    // 检查是否为下一章URL
                    if (!string.IsNullOrEmpty(mNextChapterUrl) &&
                        NetworkUtils.GetAbsoluteURL(redirectUrl, nextUrl) ==
                        NetworkUtils.GetAbsoluteURL(redirectUrl, mNextChapterUrl))
                    {
                        break;
                    }

                    nextUrlList.Add(nextUrl);
                    cancellationToken.ThrowIfCancellationRequested();

                    var analyzeUrl = new AnalyzeUrl(
                        mUrl: nextUrl,
                        source: bookSource,
                        ruleData: book
                    );

                    var res = await analyzeUrl.GetStrResponseAsync();
                    if (!string.IsNullOrEmpty(res.Body))
                    {
                        contentData = await AnalyzeContentInternal(
                            book, nextUrl, res.Url, res.Body, contentRule,
                            bookChapter, bookSource, mNextChapterUrl,
                            printLog: false,
                            cancellationToken: cancellationToken);
                        nextUrl = contentData.NextUrlList.Count > 0 ? contentData.NextUrlList[0] : "";
                        contentList.Add(contentData.Content);
                        Debug.Log(bookSource.BookSourceUrl, $"第{contentList.Count}页完成");
                    }
                    else
                    {
                        break;
                    }
                }
                Debug.Log(bookSource.BookSourceUrl, $"◇本章总页数:{nextUrlList.Count}");
            }
            // 并发解析模式
            else if (contentData.NextUrlList.Count > 1)
            {
                Debug.Log(bookSource.BookSourceUrl, $"◇并发解析正文,总页数:{contentData.NextUrlList.Count}");

                // TODO: 使用AppConfig.threadCount控制并发数
                var tasks = contentData.NextUrlList.Select(async urlStr =>
                {
                    var analyzeUrl = new AnalyzeUrl(
                        mUrl: urlStr,
                        source: bookSource,
                        ruleData: book
                    );

                    var res = await analyzeUrl.GetStrResponseAsync();
                    var result = await AnalyzeContentInternal(
                        book, urlStr, res.Url, res.Body, contentRule,
                        bookChapter, bookSource, mNextChapterUrl,
                        getNextPageUrl: false,
                        printLog: false,
                        cancellationToken: cancellationToken);
                    return result.Content;
                }).ToList();

                var results = await Task.WhenAll(tasks);
                foreach (var content in results)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    contentList.Add(content);
                }
            }

            var contentStr = string.Join("\n", contentList);

            // 全文替换
            var replaceRegex = contentRule.ReplaceRegex;
            if (!string.IsNullOrEmpty(replaceRegex))
            {
                contentStr = string.Join("\n", AppPattern.LFRegex.Split(contentStr).Select(s => s.Trim()));
                contentStr = analyzeRule.GetString(replaceRegex, contentStr);
                contentStr = string.Join("\n", AppPattern.LFRegex.Split(contentStr).Select(s => $"\u3000\u3000{s}"));
            }

            Debug.Log(bookSource.BookSourceUrl, "┌获取章节名称");
            Debug.Log(bookSource.BookSourceUrl, $"└{bookChapter.Title}");
            Debug.Log(bookSource.BookSourceUrl, "┌获取正文内容");
            Debug.Log(bookSource.BookSourceUrl, $"└\n{contentStr}");

            if (!bookChapter.IsVolume && string.IsNullOrWhiteSpace(contentStr))
            {
                throw new Exception("内容为空");
            }

            if (needSave)
            {
                // TODO: BookHelp.SaveContent(bookSource, book, bookChapter, contentStr);
            }

            return contentStr;
        }

        /// <summary>
        /// 内部解析正文方法
        /// 对应 Kotlin 的私有 analyzeContent
        /// </summary>
        private static async Task<ContentData> AnalyzeContentInternal(
            Book book,
            string baseUrl,
            string redirectUrl,
            string body,
            ContentRule contentRule,
            BookChapter chapter,
            BookSource bookSource,
            string nextChapterUrl,
            bool getNextPageUrl = true,
            bool printLog = true,
            CancellationToken cancellationToken = default)
        {
            var analyzeRule = new AnalyzeRule(book, bookSource);
            analyzeRule.SetContent(body, baseUrl);
            // TODO: analyzeRule.SetCoroutineContext(coroutineContext);
            var rUrl = analyzeRule.SetRedirectUrl(redirectUrl);
            analyzeRule.SetNextChapterUrl(nextChapterUrl);
            var nextUrlList = new List<string>();
            analyzeRule.SetChapter(chapter);

            // 获取正文 (unescape=false因为我们后面会单独处理HTML解码)
            var content = analyzeRule.GetString(contentRule.Content);
            content = HtmlFormatter.FormatKeepImg(content, rUrl);

            // HTML实体解码
            if (content.IndexOf('&') > -1)
            {
                content = HttpUtility.HtmlDecode(content);
            }

            // 获取下一页链接
            if (getNextPageUrl)
            {
                var nextUrlRule = contentRule.NextContentUrl;
                if (!string.IsNullOrEmpty(nextUrlRule))
                {
                    Debug.Log(bookSource.BookSourceUrl, "┌获取正文下一页链接", printLog);
                    try
                    {
                        var urls = analyzeRule.GetStringList(nextUrlRule, isUrl: true);
                        if (urls != null)
                        {
                            nextUrlList.AddRange(urls);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(bookSource.BookSourceUrl, $"获取下一页链接出错: {e.Message}");
                    }
                    Debug.Log(bookSource.BookSourceUrl, $"└{string.Join("，", nextUrlList)}", printLog);
                }
            }

            return new ContentData(content, nextUrlList);
        }

        /// <summary>
        /// 内容数据类（对应 Kotlin 的 Pair&lt;String, List&lt;String&gt;&gt;）
        /// </summary>
        private class ContentData
        {
            public string Content { get; }
            public List<string> NextUrlList { get; }

            public ContentData(string content, List<string> nextUrlList)
            {
                Content = content;
                NextUrlList = nextUrlList;
            }
        }
    }
}
