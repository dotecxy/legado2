using Legado.Core.Data.Entities;
using Legado.Core.Data.Entities.Rules;
using Legado.Core.Models.AnalyzeRules;
using System;
using System.Threading.Tasks;

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 书籍信息解析类 (对应 Kotlin: BookInfo.kt)
    /// </summary>
    public static class BookInfo
    {        
        /// <summary>
        /// 解析书籍信息（对应 Kotlin 的 analyzeBookInfo）
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
                throw new Exception("Error getting web content");
            }

            var infoRule = bookSource.RuleBookInfo;
            if (infoRule == null)
            {
                return;
            }

            // Debug.Log(bookSource.BookSourceUrl, "┌获取详情信息");
            // Debug.Log(bookSource.BookSourceUrl, "└$baseUrl");

            var analyzeRule = new AnalyzeRule(book, bookSource);
            analyzeRule.SetContent(body);
            analyzeRule.SetBaseUrl(baseUrl);
            analyzeRule.SetRedirectUrl(redirectUrl);

            // 执行 init 脚本
            if (!string.IsNullOrWhiteSpace(infoRule.Init))
            {
                try
                {
                    // TODO: 执行 JS 脚本
                     analyzeRule.SetContent(analyzeRule.GetString(infoRule.Init));
                }
                catch (Exception e)
                {
                    // Debug.Log error
                }
            }

            // 解析书名
            if (canReName)
            {
                var name = analyzeRule.GetString(infoRule.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    book.Name = name;
                }
            }

            // 解析作者
            var author = analyzeRule.GetString(infoRule.Author);
            if (!string.IsNullOrWhiteSpace(author))
            {
                book.Author = author;
            }

            // 解析封面URL
            var coverUrl = analyzeRule.GetString(infoRule.CoverUrl, isUrl: true);
            if (!string.IsNullOrWhiteSpace(coverUrl))
            {
                book.CoverUrl = coverUrl;
            }

            // 解析分类
            var kind = analyzeRule.GetString(infoRule.Kind);
            if (!string.IsNullOrWhiteSpace(kind))
            {
                book.Kind = kind;
            }

            // 解析简介
            var intro = analyzeRule.GetString(infoRule.Intro);
            if (!string.IsNullOrWhiteSpace(intro))
            {
                book.Intro = intro;
            }

            // 解析字数
            var wordCount = analyzeRule.GetString(infoRule.WordCount);
            if (!string.IsNullOrWhiteSpace(wordCount))
            {
                book.WordCount = wordCount;
            }

            // 解析最新章节
            var latestChapterTitle = analyzeRule.GetString(infoRule.LastChapter);
            if (!string.IsNullOrWhiteSpace(latestChapterTitle))
            {
                book.LatestChapterTitle = latestChapterTitle;
            }

            // 解析目录URL
            var tocUrl = analyzeRule.GetString(infoRule.TocUrl, isUrl: true);
            if (!string.IsNullOrWhiteSpace(tocUrl))
            {
                book.TocUrl = tocUrl;
            }
            else if (string.IsNullOrWhiteSpace(book.TocUrl))
            {
                book.TocUrl = baseUrl;
            }

            // 解析内容URL (CanReName)
            var canUpdate = analyzeRule.GetString(infoRule.CanReName);
            if (!string.IsNullOrWhiteSpace(canUpdate))
            {
                book.CanUpdate = canUpdate.ToLower() != "false";
            }

            // 执行详情页JS
            if (!string.IsNullOrWhiteSpace(infoRule.DownloadUrls))
            {
                try
                {
                    // TODO: 执行 JS 下载URL处理
                    // analyzeRule.GetStringList(infoRule.DownloadUrls);
                }
                catch (Exception e)
                {
                    // Debug.Log error
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 解析书籍信息（对应 Kotlin 的 analyzeBookInfo 重载方法）
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
            var infoRule = bookSource.RuleBookInfo;
            if (infoRule == null)
            {
                return;
            }

            // 执行 init 脚本
            if (!string.IsNullOrWhiteSpace(infoRule.Init))
            {
                try
                {
                    // Debug.Log(bookSource.BookSourceUrl, "≡执行详情页初始化规则");
                    // TODO: 执行 JS 脚本
                    // analyzeRule.SetContent(analyzeRule.GetElement(infoRule.Init));
                }
                catch (Exception e)
                {
                    // Debug.Log error
                }
            }

            var mCanReName = canReName && !string.IsNullOrWhiteSpace(infoRule.CanReName);

            // Debug.Log(bookSource.BookSourceUrl, "┌获取书名");
            var name = analyzeRule.GetString(infoRule.Name);
            if (!string.IsNullOrWhiteSpace(name) && (mCanReName || string.IsNullOrWhiteSpace(book.Name)))
            {
                book.Name = name.Trim();
            }
            // Debug.Log(bookSource.BookSourceUrl, $"└{name}");

            // Debug.Log(bookSource.BookSourceUrl, "┌获取作者");
            var author = analyzeRule.GetString(infoRule.Author);
            if (!string.IsNullOrWhiteSpace(author) && (mCanReName || string.IsNullOrWhiteSpace(book.Author)))
            {
                book.Author = author.Trim();
            }
            // Debug.Log(bookSource.BookSourceUrl, $"└{author}");

            // Debug.Log(bookSource.BookSourceUrl, "┌获取分类");
            try
            {
                var kindList = analyzeRule.GetStringList(infoRule.Kind);
                if (kindList != null && kindList.Count > 0)
                {
                    book.Kind = string.Join(",", kindList);
                }
                // Debug.Log(bookSource.BookSourceUrl, $"└{book.Kind}");
            }
            catch (Exception e)
            {
                // Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
            }

            // Debug.Log(bookSource.BookSourceUrl, "┌获取字数");
            try
            {
                var wordCount = analyzeRule.GetString(infoRule.WordCount);
                if (!string.IsNullOrWhiteSpace(wordCount))
                {
                    book.WordCount = wordCount;
                }
                // Debug.Log(bookSource.BookSourceUrl, $"└{book.WordCount}");
            }
            catch (Exception e)
            {
                // Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
            }

            // Debug.Log(bookSource.BookSourceUrl, "┌获取最新章节");
            try
            {
                var lastChapter = analyzeRule.GetString(infoRule.LastChapter);
                if (!string.IsNullOrWhiteSpace(lastChapter))
                {
                    book.LatestChapterTitle = lastChapter;
                }
                // Debug.Log(bookSource.BookSourceUrl, $"└{book.LatestChapterTitle}");
            }
            catch (Exception e)
            {
                // Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
            }

            // Debug.Log(bookSource.BookSourceUrl, "┌获取简介");
            try
            {
                var intro = analyzeRule.GetString(infoRule.Intro);
                if (!string.IsNullOrWhiteSpace(intro))
                {
                    book.Intro = intro;
                }
                // Debug.Log(bookSource.BookSourceUrl, $"└{book.Intro}");
            }
            catch (Exception e)
            {
                // Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
            }

            // Debug.Log(bookSource.BookSourceUrl, "┌获取封面链接");
            try
            {
                var coverUrl = analyzeRule.GetString(infoRule.CoverUrl);
                if (!string.IsNullOrWhiteSpace(coverUrl))
                {
                    // TODO: NetworkUtils.GetAbsoluteURL
                    book.CoverUrl = coverUrl;
                }
                // Debug.Log(bookSource.BookSourceUrl, $"└{book.CoverUrl}");
            }
            catch (Exception e)
            {
                // Debug.Log(bookSource.BookSourceUrl, $"└{e.Message}");
            }

            // Debug.Log(bookSource.BookSourceUrl, "┌获取目录链接");
            book.TocUrl = analyzeRule.GetString(infoRule.TocUrl, isUrl: true);
            if (string.IsNullOrWhiteSpace(book.TocUrl))
            {
                book.TocUrl = baseUrl;
            }
            if (book.TocUrl == baseUrl)
            {
                book.TocHtml = body;
            }
            // Debug.Log(bookSource.BookSourceUrl, $"└{book.TocUrl}");

            // 处理下载链接（如果有）
            if (!string.IsNullOrWhiteSpace(infoRule.DownloadUrls))
            {
                try
                {
                    var downloadUrls = analyzeRule.GetStringList(infoRule.DownloadUrls, isUrl: true);
                    if (downloadUrls != null && downloadUrls.Count > 0)
                    {
                        // TODO: 处理下载 URL
                    }
                }
                catch (Exception e)
                {
                    // Debug.Log error
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 解析书籍信息（便捷方法，对应 Kotlin 的 analyzeBookInfo）
        /// </summary>
        public static async Task analyzeBookInfo(
            Book book,
            string body,
            AnalyzeRule analyzeRule,
            BookSource bookSource,
            string baseUrl,
            string redirectUrl,
            bool canReName)
        {
            await AnalyzeBookInfo(book, body, analyzeRule, bookSource, baseUrl, redirectUrl, canReName);
        }
    }
}