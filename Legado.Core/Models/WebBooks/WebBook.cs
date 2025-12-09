using Legado.Core.Data.Entities;
using Legado.Core.Models.AnalyzeRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 网络书籍操作类
    /// </summary>
    public class WebBook
    {
        /// <summary>
        /// 搜索书籍
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="key">搜索关键词</param>
        /// <param name="page">页码</param>
        /// <param name="filter">过滤函数</param>
        /// <param name="shouldBreak">中断条件</param>
        /// <returns>搜索结果列表</returns>
        public async Task<List<SearchBook>> SearchBookAwait(
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

            // 执行预更新JS
            // BookSource类没有GetPreUpdateJs方法，注释掉这段代码
            // var js = bookSource.GetPreUpdateJs();
            // if (!string.IsNullOrEmpty(js))
            // {
            //     // TODO: 实现EvalJS方法
            //     Console.WriteLine("执行预更新JS: " + js);
            // }

            // 执行请求
            var resBody = await analyzeUrl.GetStrResponseAwait();
            // 创建StrResponse对象
            var res = new StrResponse(bookSource.SearchUrl, resBody.Body);

            //检测书源是否已登录
            if (!string.IsNullOrWhiteSpace(bookSource.LoginCheckJs))
            {
                // TODO: 实现EvalJS方法
                Console.WriteLine("执行登录检查JS: " + bookSource.LoginCheckJs);
            }

            //CheckRedirect(bookSource, res);

            // 解析书籍列表
            //return BookList.AnalyzeBookList(
            //    bookSource: bookSource,
            //    ruleData: ruleData,
            //    analyzeUrl: analyzeUrl,
            //    baseUrl: res.Url,
            //    body: res.Body,
            //    isSearch: true,
            //    isRedirect: CheckRedirect(bookSource, res),
            //    filter: filter,
            //    shouldBreak: shouldBreak
            //);
            return null;
        }

        ///// <summary>
        ///// 发现书籍
        ///// </summary>
        ///// <param name="bookSource">书源</param>
        ///// <param name="url">发现地址</param>
        ///// <param name="page">页码</param>
        ///// <returns>发现结果列表</returns>
        //public static List<SearchBook> ExploreBookAwait(
        //    BookSource bookSource,
        //    string url,
        //    int? page = 1)
        //{
        //    var ruleData = new RuleData();
        //    var analyzeUrl = new AnalyzeUrl(
        //        url,
        //        page: page,
        //        baseUrl: bookSource.BookSourceUrl,
        //        ruleData: null
        //    );

        //    var resBody = analyzeUrl.Execute();
        //    var res = new StrResponse(url, resBody);

        //    //检测书源是否已登录
        //    if (!string.IsNullOrWhiteSpace(bookSource.LoginCheckJs))
        //    {
        //        // TODO: 实现EvalJS方法
        //        Console.WriteLine("执行登录检查JS: " + bookSource.LoginCheckJs);
        //    }

        //    CheckRedirect(bookSource, res);

        //    // 解析书籍列表
        //    return BookList.AnalyzeBookList(
        //        bookSource: bookSource,
        //        ruleData: ruleData,
        //        analyzeUrl: analyzeUrl,
        //        baseUrl: res.Url,
        //        body: res.Body,
        //        isSearch: false,
        //        isRedirect: CheckRedirect(bookSource, res),
        //        filter: null,
        //        shouldBreak: null
        //    );
        //}

        ///// <summary>
        ///// 获取书籍信息
        ///// </summary>
        ///// <param name="bookSource">书源</param>
        ///// <param name="book">书籍</param>
        ///// <param name="canReName">是否可以重命名</param>
        ///// <returns>更新后的书籍信息</returns>
        //public static Book GetBookInfoAwait(
        //    BookSource bookSource,
        //    Book book,
        //    bool canReName = true)
        //{
        //    // TODO: 实现类型管理逻辑

        //    if (!string.IsNullOrWhiteSpace(book.InfoHtml))
        //    {
        //        BookInfo.AnalyzeBookInfo(
        //            bookSource: bookSource,
        //            book: book,
        //            baseUrl: book.BookUrl,
        //            redirectUrl: book.BookUrl,
        //            body: book.InfoHtml,
        //            canReName: canReName
        //        );
        //    }
        //    else
        //    {
        //        var analyzeUrl = new AnalyzeUrl(
        //            book.BookUrl,
        //            baseUrl: bookSource.BookSourceUrl,
        //            ruleData: null
        //        );

        //        var resBody = analyzeUrl.Execute();
        //        var res = new StrResponse(book.BookUrl, resBody);

        //        //检测书源是否已登录
        //        if (!string.IsNullOrWhiteSpace(bookSource.LoginCheckJs))
        //        {
        //            // TODO: 实现EvalJS方法
        //            Console.WriteLine("执行登录检查JS: " + bookSource.LoginCheckJs);
        //        }

        //        CheckRedirect(bookSource, res);

        //        BookInfo.AnalyzeBookInfo(
        //            bookSource: bookSource,
        //            book: book,
        //            baseUrl: book.BookUrl,
        //            redirectUrl: res.Url,
        //            body: res.Body,
        //            canReName: canReName
        //        );
        //    }

        //    return book;
        //}

        ///// <summary>
        ///// 获取章节列表
        ///// </summary>
        ///// <param name="bookSource">书源</param>
        ///// <param name="book">书籍</param>
        ///// <param name="runPreUpdateJs">是否运行预更新JS</param>
        ///// <returns>章节列表</returns>
        //public static List<BookChapter> GetChapterListAwait(
        //    BookSource bookSource,
        //    Book book,
        //    bool runPreUpdateJs = false)
        //{
        //    // TODO: 实现类型管理逻辑

        //    if (runPreUpdateJs)
        //    {
        //        RunPreUpdateJs(bookSource, book);
        //    }

        //    if (book.BookUrl == book.TocUrl && !string.IsNullOrWhiteSpace(book.TocHtml))
        //    {
        //        return BookChapterList.AnalyzeChapterList(
        //            bookSource: bookSource,
        //            book: book,
        //            baseUrl: book.TocUrl,
        //            redirectUrl: book.TocUrl,
        //            body: book.TocHtml
        //        );
        //    }
        //    else
        //    {
        //        var analyzeUrl = new AnalyzeUrl(
        //            book.TocUrl,
        //            baseUrl: book.BookUrl,
        //            ruleData: null
        //        );

        //        var resBody = analyzeUrl.Execute();
        //        var res = new StrResponse(book.TocUrl, resBody);

        //        //检测书源是否已登录
        //        if (!string.IsNullOrWhiteSpace(bookSource.LoginCheckJs))
        //        {
        //            // TODO: 实现EvalJS方法
        //            Console.WriteLine("执行登录检查JS: " + bookSource.LoginCheckJs);
        //        }

        //        CheckRedirect(bookSource, res);

        //        return BookChapterList.AnalyzeChapterList(
        //            bookSource: bookSource,
        //            book: book,
        //            baseUrl: book.TocUrl,
        //            redirectUrl: res.Url,
        //            body: res.Body
        //        );
        //    }
        //}

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

        ///// <summary>
        ///// 获取章节内容
        ///// </summary>
        ///// <param name="bookSource">书源</param>
        ///// <param name="book">书籍</param>
        ///// <param name="bookChapter">章节</param>
        ///// <param name="nextChapterUrl">下一章节URL</param>
        ///// <param name="needSave">是否需要保存</param>
        ///// <returns>章节内容</returns>
        //public static async Task<string> GetContentAwait(
        //    BookSource bookSource,
        //    Book book,
        //    BookChapter bookChapter,
        //    string nextChapterUrl = null,
        //    bool needSave = true)
        //{
        //    var contentRule = bookSource.GetContentRule();
        //    if (string.IsNullOrWhiteSpace(contentRule.Content))
        //    {
        //        // TODO: 添加日志
        //        return bookChapter.Url;
        //    }

        //    if (bookChapter.IsVolume && bookChapter.Url.StartsWith(bookChapter.Title))
        //    {
        //        // TODO: 添加日志
        //        return string.Empty;
        //    }

        //    if (bookChapter.Url == book.BookUrl && !string.IsNullOrWhiteSpace(book.TocHtml))
        //    {
        //        return await BookContent.AnalyzeContent(
        //            bookSource: bookSource,
        //            book: book,
        //            chapter: bookChapter,
        //            baseUrl: bookChapter.GetAbsoluteURL(book.TocUrl),
        //            redirectUrl: bookChapter.GetAbsoluteURL(book.TocUrl),
        //            body: book.TocHtml
        //        );
        //    }
        //    else
        //    {
        //        var analyzeUrl = new AnalyzeUrl(
        //            bookChapter.GetAbsoluteURL(book.TocUrl),
        //            baseUrl: book.TocUrl,
        //            ruleData: null
        //        );

        //        var resBody = analyzeUrl.Execute();
        //        var res = new StrResponse(bookChapter.GetAbsoluteURL(book.TocUrl), resBody);

        //        //检测书源是否已登录
        //        if (!string.IsNullOrWhiteSpace(bookSource.LoginCheckJs))
        //        {
        //            // TODO: 实现EvalJS方法
        //            Console.WriteLine("执行登录检查JS: " + bookSource.LoginCheckJs);
        //        }

        //        CheckRedirect(bookSource, res);

        //        return await BookContent.AnalyzeContent(
        //            bookSource: bookSource,
        //            book: book,
        //            chapter: bookChapter,
        //            baseUrl: bookChapter.GetAbsoluteURL(book.TocUrl),
        //            redirectUrl: res.Url,
        //            body: res.Body
        //        );
        //    }
        //}

        ///// <summary>
        ///// 精准搜索
        ///// </summary>
        ///// <param name="bookSource">书源</param>
        ///// <param name="name">书名</param>
        ///// <param name="author">作者</param>
        ///// <returns>书籍信息</returns>
        //public static Book PreciseSearchAwait(
        //    BookSource bookSource,
        //    string name,
        //    string author)
        //{
        //    var searchResults = SearchBookAwait(
        //        bookSource,
        //        name,
        //        filter: (fName, fAuthor) => fName == name && fAuthor == author,
        //        shouldBreak: (count) => count > 0
        //    );

        //    var searchBook = searchResults.FirstOrDefault();
        //    if (searchBook != null)
        //    {
        //        return searchBook.ToBook();
        //    }

        //    throw new Exception($"未搜索到 {name}({author}) 书籍");
        //}

        ///// <summary>
        ///// 检测重定向
        ///// </summary>
        ///// <param name="bookSource">书源</param>
        ///// <param name="response">响应</param>
        //private static bool CheckRedirect(BookSource bookSource, StrResponse response)
        //{
        //    try
        //    {
        //        string finalUrl;
        //        if (response.Raw != null && response.Raw.RequestMessage != null)
        //        {
        //            finalUrl = response.Raw.RequestMessage.RequestUri.ToString();
        //        }
        //        else
        //        {
        //            finalUrl = response.Url;
        //        }
        //        Console.WriteLine($"当前地址: {finalUrl}");
        //        return finalUrl != bookSource.SearchUrl;
        //    }
        //    catch (Exception)
        //    {
        //        Console.WriteLine($"当前地址: {response.Url}");
        //        return response.Url != bookSource.SearchUrl;
        //    }
        //}
    }

    /// <summary>
    /// 搜索书籍实体类
    /// </summary>
    public class SearchBook
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string BookUrl { get; set; }
        public string CoverUrl { get; set; }
        public string Kind { get; set; }
        public string LastChapter { get; set; }
        public string Intro { get; set; }

        /// <summary>
        /// 转换为Book对象
        /// </summary>
        /// <returns>Book对象</returns>
        public Book ToBook()
        {
            return new Book
            {
                Name = this.Name,
                Author = this.Author,
                BookUrl = this.BookUrl,
                CoverUrl = this.CoverUrl,
                Kind = this.Kind,
                Intro = this.Intro
            };
        }
    }
}