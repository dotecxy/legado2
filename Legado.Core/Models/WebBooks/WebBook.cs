using Legado.Core.Data.Entities;
using Legado.Core.Helps.Books;
using Legado.Core.Helps.Source;
using Legado.Core.Models.AnalyzeRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 网络书籍操作类 (对应 Kotlin: WebBook.kt)
    /// </summary>
    public class WebBook
    {
        /// <summary>
        /// 搜索书籍（对应 Kotlin 的 searchBookAwait）
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="key">搜索关键词</param>
        /// <param name="page">页码</param>
        /// <param name="filter">过滤函数</param>
        /// <param name="shouldBreak">中断条件</param>
        /// <returns>搜索结果列表</returns>
        public async Task<List<SearchBook>> SearchBookAsync(
            BookSource bookSource,
            string key,
            int? page = 1,
            Func<string, string, bool> filter = null,
            Func<int, bool> shouldBreak = null)
        {
            if (string.IsNullOrWhiteSpace(bookSource.SearchUrl))
            {
                throw new Exception("搜索url不能为空");
            }

            var ruleData = new RuleData();
            var analyzeUrl = new AnalyzeUrl(
               mUrl: bookSource.SearchUrl,
               key: key,
                page: page,
                baseUrl: bookSource.BookSourceUrl,
                source: bookSource
            );

            // 执行请求
            var resBody = await analyzeUrl.GetStrResponseAsync();

            // 检测书源是否已登录
            if (!string.IsNullOrWhiteSpace(bookSource.LoginCheckJs))
            {
                // TODO: 实现EvalJS方法
                Console.WriteLine("执行登录检查JS: " + bookSource.LoginCheckJs);
            }

            // 解析书籍列表
            return await BookList.AnalyzeBookList(
                bookSource: bookSource,
                ruleData: ruleData,
                analyzeUrl: analyzeUrl,
                baseUrl: resBody.Url,
                body: resBody.Body,
                isSearch: true,
                isRedirect: false,
                filter: filter,
                shouldBreak: shouldBreak
            );
        }

        /// <summary>
        /// 发现书籍（对应 Kotlin 的 exploreBookAwait）
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="url">发现地址</param>
        /// <param name="page">页码</param>
        /// <returns>发现结果列表</returns>
        public async Task<List<SearchBook>> ExploreBookAsync(
            BookSource bookSource,
            string url,
            int? page = 1)
        {
            var ruleData = new RuleData();
            var analyzeUrl = new AnalyzeUrl(
                mUrl: url,
                page: page,
                baseUrl: bookSource.BookSourceUrl,
                source: bookSource
            );

            var resBody = await analyzeUrl.GetStrResponseAsync();

            //检测书源是否已登录
            if (!string.IsNullOrWhiteSpace(bookSource.LoginCheckJs))
            {
                // TODO: 实现EvalJS方法
                Console.WriteLine("执行登录检查JS: " + bookSource.LoginCheckJs);
            }

            // 解析书籍列表
            return await BookList.AnalyzeBookList(
                bookSource: bookSource,
                ruleData: ruleData,
                analyzeUrl: analyzeUrl,
                baseUrl: resBody.Url,
                body: resBody.Body,
                isSearch: false,
                isRedirect: false,
                filter: null,
                shouldBreak: null
            );
        }

        /// <summary>
        /// 获取书籍信息（对应 Kotlin 的 getBookInfoAwait）
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="book">书籍</param>
        /// <param name="canReName">是否可以重命名</param>
        /// <returns>更新后的书籍信息</returns>
        public async Task<Book> GetBookInfoAsync(
            BookSource bookSource,
            Book book,
            bool canReName = true)
        {
            book.RemoveAllBookType();
            book.AddType(bookSource.GetBookType());
            if (!book.InfoHtml.IsNullOrEmpty())
            {
                await BookInfo.AnalyzeBookInfo(
                   bookSource: bookSource,
                   book: book,
                   baseUrl: book.BookUrl,
                   redirectUrl: book.BookUrl,
                   body: book.InfoHtml,
                    canReName: canReName
                 );
            }
            else
            {
                var analyzeUrl = new AnalyzeUrl(
                    mUrl: book.BookUrl,
                    baseUrl: bookSource.BookSourceUrl,
                    source: bookSource,
                    ruleData: book
                );
                var res = await analyzeUrl.GetStrResponseAsync();
                //检测书源是否已登录
                if (!string.IsNullOrWhiteSpace(bookSource.LoginCheckJs))
                {
                    res = analyzeUrl.EvalJS(jsStr: bookSource.LoginCheckJs, result: res) as StrResponse;
                }
                //TODO检查重定向


                await BookInfo.AnalyzeBookInfo(
                    bookSource: bookSource,
                book: book,
                baseUrl: book.BookUrl,
                redirectUrl: res.Url,
                body: res.Body,
                canReName: canReName
                );
            }


            return book;
        }

        /// <summary>
        /// 获取章节列表（对应 Kotlin 的 getChapterListAwait）
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="book">书籍</param>
        /// <param name="runPreUpdateJs">是否运行预更新JS</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>章节列表</returns>
        public async Task<List<BookChapter>> GetChapterListAsync(
            BookSource bookSource,
            Book book,
            bool runPreUpdateJs = false,
            CancellationToken cancellationToken = default)
        {
            // 运行预更新JS
            if (runPreUpdateJs && !string.IsNullOrWhiteSpace(bookSource.RuleToc?.PreUpdateJs))
            {
                try
                {
                    // TODO: 执行 JS 脚本
                    // var analyzeRule = new AnalyzeRule(book, bookSource);
                    // analyzeRule.GetString(bookSource.RuleToc.PreUpdateJs);
                }
                catch (Exception e)
                {
                    // Debug.Log error
                }
            }

            var tocUrl = string.IsNullOrWhiteSpace(book.TocUrl) ? book.BookUrl : book.TocUrl;
            var analyzeUrl = new AnalyzeUrl(
                mUrl: tocUrl,
                baseUrl: book.BookUrl,
                source: bookSource,
                ruleData: book
            );

            var resBody = await analyzeUrl.GetStrResponseAsync();

            //检测书源是否已登录
            if (!string.IsNullOrWhiteSpace(bookSource.LoginCheckJs))
            {
                // TODO: 实现EvalJS方法
                Console.WriteLine("执行登录检查JS: " + bookSource.LoginCheckJs);
            }

            return await BookChapterList.AnalyzeChapterList(
                bookSource: bookSource,
                book: book,
                baseUrl: tocUrl,
                redirectUrl: resBody.Url,
                body: resBody.Body,
                cancellationToken: cancellationToken
            );
        }

        ///// <summary>
        ///// 运行预更新JS
        ///// </summary>
        ///// <param name="bookSource">书源</param>
        ///// <param name="book">书籍</param>
        //private static void RunPreUpdateJs(BookSource bookSource, Book book)
        //{
        //    var preUpdateJs = bookSource.GetTocRule()?.PreUpdateJs;
        //    if (!string.IsNullOrWhiteSpace(preUpdateJs))
        //    {
        //        // TODO: 实现AnalyzeRule和EvalJS
        //        Console.WriteLine("执行预更新JS: " + preUpdateJs);
        //    }
        //}

        /// <summary>
        /// 获取章节内容（对应 Kotlin 的 getContentAwait）
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="book">书籍</param>
        /// <param name="bookChapter">章节</param>
        /// <param name="nextChapterUrl">下一章节URL</param>
        /// <returns>章节内容</returns>
        public async Task<string> GetContentAsync(
            BookSource bookSource,
            Book book,
            BookChapter bookChapter,
            string nextChapterUrl = null)
        {
            var contentRule = bookSource.RuleContent;
            if (contentRule == null || string.IsNullOrWhiteSpace(contentRule.Content))
            {
                throw new Exception("Content rule is null");
            }

            // 卷名特殊处理
            if (bookChapter.IsVolume && bookChapter.Url.StartsWith(bookChapter.Title))
            {
                return string.Empty;
            }

            var chapterUrl = bookChapter.GetAbsoluteURL();
            var analyzeUrl = new AnalyzeUrl(
                mUrl: chapterUrl,
                baseUrl: book.TocUrl,
                source: bookSource,
                ruleData: bookChapter
            );

            var resBody = await analyzeUrl.GetStrResponseAsync();

            //检测书源是否已登录
            if (!string.IsNullOrWhiteSpace(bookSource.LoginCheckJs))
            {
                // TODO: 实现EvalJS方法
                Console.WriteLine("执行登录检查JS: " + bookSource.LoginCheckJs);
            }

            return await BookContent.AnalyzeContent(
                bookSource: bookSource,
                book: book,
                bookChapter: bookChapter,
                baseUrl: chapterUrl,
                redirectUrl: resBody.Url,
                body: resBody.Body,
                nextChapterUrl: nextChapterUrl
            );
        }

        /// <summary>
        /// 精准搜索（对应 Kotlin 的 preciseSearchAwait）
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="name">书名</param>
        /// <param name="author">作者</param>
        /// <returns>书籍信息</returns>
        public async Task<Book> PreciseSearchAsync(
            BookSource bookSource,
            string name,
            string author)
        {
            var searchResults = await SearchBookAsync(
                bookSource,
                name,
                filter: (fName, fAuthor) => fName == name && fAuthor == author,
                shouldBreak: (count) => count > 0
            );

            var searchBook = searchResults.FirstOrDefault();
            if (searchBook != null)
            {
                return searchBook.ToBook();
            }

            throw new Exception($"未搜索到 {name}({author}) 书籍");
        }

        public async Task<List<SearchBook>> PreciseSearchAsync(
            BookSource bookSource,
            string name, int page = 1)
        {
            var searchResults = await SearchBookAsync(
                bookSource,
                name,
                filter: (fName, fAuthor) => fName.Contains(name) || fAuthor.Contains(name),
                shouldBreak: (count) => count > 0,
                page: page

            );

            return searchResults ?? new List<SearchBook>();
        }

        /// <summary>
        /// 搜索书籍（便捷方法，对应 Kotlin 的 searchBookAwait）
        /// </summary>
        public async Task<List<SearchBook>> searchBookAsync(
            BookSource bookSource,
            string key,
            int? page = 1,
            Func<string, string, bool> filter = null,
            Func<int, bool> shouldBreak = null)
        {
            return await SearchBookAsync(bookSource, key, page, filter, shouldBreak);
        }

        /// <summary>
        /// 发现书籍（便捷方法，对应 Kotlin 的 exploreBookAwait）
        /// </summary>
        public async Task<List<SearchBook>> exploreBookAsync(
            BookSource bookSource,
            string url,
            int? page = 1)
        {
            return await ExploreBookAsync(bookSource, url, page);
        }

        /// <summary>
        /// 获取书籍信息（便捷方法，对应 Kotlin 的 getBookInfoAwait）
        /// </summary>
        public async Task<Book> getBookInfoAsync(
            BookSource bookSource,
            Book book,
            bool canReName = true)
        {
            return await GetBookInfoAsync(bookSource, book, canReName);
        }

        /// <summary>
        /// 获取章节列表（便捷方法，对应 Kotlin 的 getChapterListAwait）
        /// </summary>
        public async Task<List<BookChapter>> getChapterListAsync(
            BookSource bookSource,
            Book book,
            bool runPreUpdateJs = false,
            CancellationToken cancellationToken = default)
        {
            return await GetChapterListAsync(bookSource, book, runPreUpdateJs, cancellationToken);
        }

        /// <summary>
        /// 获取章节内容（便捷方法，对应 Kotlin 的 getContentAwait）
        /// </summary>
        public async Task<string> getContentAsync(
            BookSource bookSource,
            Book book,
            BookChapter bookChapter,
            string nextChapterUrl = null)
        {
            return await GetContentAsync(bookSource, book, bookChapter, nextChapterUrl);
        }

        /// <summary>
        /// 精准搜索（便捷方法，对应 Kotlin 的 preciseSearchAwait）
        /// </summary>
        public async Task<Book> preciseSearchAsync(
            BookSource bookSource,
            string name,
            string author)
        {
            return await PreciseSearchAsync(bookSource, name, author);
        }
    }
}