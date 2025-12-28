using Legado.Core.Data.Entities;
using Legado.Core.Data.Entities.Rules;
using Legado.Core.Helps.Books;
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Utils;
using System;
using System.Threading.Tasks;

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 获取详情 (对应 Kotlin: BookInfo.kt)
    /// </summary>
    public static class BookInfo
    {
        /// <summary>
        /// 解析书籍信息
        /// 对应 Kotlin 的 analyzeBookInfo(bookSource, book, baseUrl, redirectUrl, body, canReName)
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="book">书籍</param>
        /// <param name="baseUrl">基础URL</param>
        /// <param name="redirectUrl">重定向URL</param>
        /// <param name="body">HTML内容</param>
        /// <param name="canReName">是否可以重命名</param>
        public static async Task AnalyzeBookInfo(
            BookSource bookSource,
            Book book,
            string baseUrl,
            string redirectUrl,
            string body,
            bool canReName = true)
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new Exception($"Error getting web content: {baseUrl}");
            }

            Debug.Log(bookSource.BookSourceUrl, $"≡获取成功:{baseUrl}");
            Debug.Log(bookSource.BookSourceUrl, body, state: 20);

            var analyzeRule = new AnalyzeRule(book, bookSource);
            analyzeRule.SetContent(body);
            analyzeRule.SetBaseUrl(baseUrl);
            analyzeRule.SetRedirectUrl(redirectUrl);
            // TODO: analyzeRule.SetCoroutineContext(coroutineContext);

            await AnalyzeBookInfo(book, body, analyzeRule, bookSource, baseUrl, redirectUrl, canReName);
        }

        /// <summary>
        /// 解析书籍信息（详细版本）
        /// 对应 Kotlin 的 analyzeBookInfo(book, body, analyzeRule, bookSource, baseUrl, redirectUrl, canReName)
        /// </summary>
        public static async Task AnalyzeBookInfo(
            Book book,
            string body,
            AnalyzeRule analyzeRule,
            BookSource bookSource,
            string baseUrl,
            string redirectUrl,
            bool canReName)
        {
            var infoRule = bookSource.GetBookInfoRule();
            if (infoRule == null)
            {
                return;
            }

            // 执行 init 脚本
            if (!string.IsNullOrWhiteSpace(infoRule.Init))
            {
                // TODO: coroutineContext.ensureActive()
                Debug.Log(bookSource.BookSourceUrl, "≡执行详情页初始化规则");
                try
                {
                    analyzeRule.SetContent(analyzeRule.GetElement(infoRule.Init));
                }
                catch (Exception e)
                {
                    Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
                }
            }

            var mCanReName = canReName && !string.IsNullOrWhiteSpace(infoRule.CanReName);

            // 获取书名
            // TODO: coroutineContext.ensureActive()
            Debug.Log(bookSource.BookSourceUrl, "┌获取书名");
            try
            {
                var name = BookHelper.FormatBookName(analyzeRule.GetString(infoRule.Name));
                if (!string.IsNullOrEmpty(name) && (mCanReName || string.IsNullOrEmpty(book.Name)))
                {
                    book.Name = name;
                }
                Debug.Log(bookSource.BookSourceUrl, $"└{name}");
            }
            catch (Exception e)
            {
                Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
            }

            // 获取作者
            // TODO: coroutineContext.ensureActive()
            Debug.Log(bookSource.BookSourceUrl, "┌获取作者");
            try
            {
                var author = BookHelper.FormatBookAuthor(analyzeRule.GetString(infoRule.Author));
                if (!string.IsNullOrEmpty(author) && (mCanReName || string.IsNullOrEmpty(book.Author)))
                {
                    book.Author = author;
                }
                Debug.Log(bookSource.BookSourceUrl, $"└{author}");
            }
            catch (Exception e)
            {
                Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
            }

            // 获取分类
            // TODO: coroutineContext.ensureActive()
            Debug.Log(bookSource.BookSourceUrl, "┌获取分类");
            try
            {
                var kindList = analyzeRule.GetStringList(infoRule.Kind);
                if (kindList != null && kindList.Count > 0)
                {
                    var kind = string.Join(",", kindList);
                    if (!string.IsNullOrEmpty(kind))
                    {
                        book.Kind = kind;
                    }
                    Debug.Log(bookSource.BookSourceUrl, $"└{kind}");
                }
                else
                {
                    Debug.Log(bookSource.BookSourceUrl, "└");
                }
            }
            catch (Exception e)
            {
                // TODO: coroutineContext.ensureActive()
                Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
                System.Diagnostics.Debug.WriteLine($"获取分类出错: {e}");
            }

            // 获取字数
            // TODO: coroutineContext.ensureActive()
            Debug.Log(bookSource.BookSourceUrl, "┌获取字数");
            try
            {
                var wordCount = StringUtils.WordCountFormat(analyzeRule.GetString(infoRule.WordCount));
                if (!string.IsNullOrEmpty(wordCount))
                {
                    book.WordCount = wordCount;
                }
                Debug.Log(bookSource.BookSourceUrl, $"└{wordCount}");
            }
            catch (Exception e)
            {
                // TODO: coroutineContext.ensureActive()
                Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
                System.Diagnostics.Debug.WriteLine($"获取字数出错: {e}");
            }

            // 获取最新章节
            // TODO: coroutineContext.ensureActive()
            Debug.Log(bookSource.BookSourceUrl, "┌获取最新章节");
            try
            {
                var lastChapter = analyzeRule.GetString(infoRule.LastChapter);
                if (!string.IsNullOrEmpty(lastChapter))
                {
                    book.LatestChapterTitle = lastChapter;
                }
                Debug.Log(bookSource.BookSourceUrl, $"└{lastChapter}");
            }
            catch (Exception e)
            {
                // TODO: coroutineContext.ensureActive()
                Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
                System.Diagnostics.Debug.WriteLine($"获取最新章节出错: {e}");
            }

            // 获取简介
            // TODO: coroutineContext.ensureActive()
            Debug.Log(bookSource.BookSourceUrl, "┌获取简介");
            try
            {
                var intro = HtmlFormatter.Format(analyzeRule.GetString(infoRule.Intro));
                if (!string.IsNullOrEmpty(intro))
                {
                    book.Intro = intro;
                }
                Debug.Log(bookSource.BookSourceUrl, $"└{intro}");
            }
            catch (Exception e)
            {
                // TODO: coroutineContext.ensureActive()
                Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
                System.Diagnostics.Debug.WriteLine($"获取简介出错: {e}");
            }

            // 获取封面链接
            // TODO: coroutineContext.ensureActive()
            Debug.Log(bookSource.BookSourceUrl, "┌获取封面链接");
            try
            {
                var coverUrl = analyzeRule.GetString(infoRule.CoverUrl);
                if (!string.IsNullOrEmpty(coverUrl))
                {
                    book.CoverUrl = NetworkUtils.GetAbsoluteURL(redirectUrl, coverUrl);
                }
                Debug.Log(bookSource.BookSourceUrl, $"└{coverUrl}");
            }
            catch (Exception e)
            {
                // TODO: coroutineContext.ensureActive()
                Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
                System.Diagnostics.Debug.WriteLine($"获取封面出错: {e}");
            }

            // TODO: coroutineContext.ensureActive()
            if (!book.IsWebFile())
            {
                // 获取目录链接
                Debug.Log(bookSource.BookSourceUrl, "┌获取目录链接");
                book.TocUrl = analyzeRule.GetString(infoRule.TocUrl, isUrl: true);
                if (string.IsNullOrEmpty(book.TocUrl))
                {
                    book.TocUrl = baseUrl;
                }
                if (book.TocUrl == baseUrl)
                {
                    book.TocHtml = body;
                }
                Debug.Log(bookSource.BookSourceUrl, $"└{book.TocUrl}");
            }
            else
            {
                // 获取文件下载链接
                Debug.Log(bookSource.BookSourceUrl, "┌获取文件下载链接");
                try
                {
                    var downloadUrls = analyzeRule.GetStringList(infoRule.DownloadUrls, isUrl: true);
                    if (downloadUrls == null || downloadUrls.Count == 0)
                    {
                        Debug.Log(bookSource.BookSourceUrl, "└");
                        throw new Exception("下载链接为空");
                    }
                    else
                    {
                        book.DownloadUrls = downloadUrls;
                        Debug.Log(bookSource.BookSourceUrl, $"└{string.Join(",\n", downloadUrls)}");
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
                    throw;
                }
            }

            await Task.CompletedTask;
        }
    }
}
