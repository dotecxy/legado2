using Legado.Core.Data.Entities;
using Legado.Core.Models.AnalyzeRules;
using System.Threading.Tasks; 

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 书籍信息解析类
    /// </summary>
    public static class BookInfo
    {
        /// <summary>
        /// 解析书籍信息
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="book">书籍</param>
        /// <param name="baseUrl">基础URL</param>
        /// <param name="redirectUrl">重定向URL</param>
        /// <param name="body">HTML内容</param>
        /// <param name="canReName">是否可以重命名</param>
        //public static async Task AnalyzeBookInfo(
        //    BookSource bookSource,
        //    Book book,
        //    string baseUrl,
        //    string redirectUrl,
        //    string body,
        //    bool canReName = true)
        //{
        //    var rule = bookSource.GetBookInfoRule();
            
        //    // 保存原始数据用于后续解析
        //    book.InfoHtml = body;
            
        //    // 解析书籍信息
        //    // Book类没有实现IRuleData接口，所以传入null
        //    var analyzeRule = new  AnalyzeRule(null, body, true);
        //    analyzeRule.SetContent(body, baseUrl);
            
        //    // 解析书名
        //    if (canReName)
        //    {
        //        var name = analyzeRule.GetString(rule.Name);
        //        if (!string.IsNullOrWhiteSpace(name))
        //        {
        //            book.Name = name;
        //        }
        //    }
            
        //    // 解析作者
        //    var author = analyzeRule.GetString(rule.Author);
        //    if (!string.IsNullOrWhiteSpace(author))
        //    {
        //        book.Author = author;
        //    }
            
        //    // 解析封面URL
        //    var coverUrl = analyzeRule.GetString(rule.CoverUrl);
        //    if (!string.IsNullOrWhiteSpace(coverUrl))
        //    {
        //        // 使用GetString方法并设置isUrl参数为true来处理URL
        //        book.CoverUrl = analyzeRule.GetString(coverUrl, false, true);
        //    }
            
        //    // 解析分类
        //    var kind = analyzeRule.GetString(rule.Kind);
        //    if (!string.IsNullOrWhiteSpace(kind))
        //    {
        //        book.Kind = kind;
        //    }
            
        //    // 解析简介
        //    var intro = analyzeRule.GetString(rule.Intro);
        //    if (!string.IsNullOrWhiteSpace(intro))
        //    {
        //        book.Intro = intro;
        //    }
            
        //    // 解析最新章节标题
        //    var latestChapterTitle = analyzeRule.GetString(rule.LatestChapter);
        //    if (!string.IsNullOrWhiteSpace(latestChapterTitle))
        //    {
        //        book.LatestChapterTitle = latestChapterTitle;
        //    }
            
        //    // 解析目录URL
        //    var tocUrl = analyzeRule.GetString(rule.TocUrl);
        //    if (!string.IsNullOrWhiteSpace(tocUrl))
        //    {
        //        // 使用GetString方法并设置isUrl参数为true来处理URL
        //        book.TocUrl = analyzeRule.GetString(tocUrl, false, true);
        //    }
        //    else
        //    {
        //        book.TocUrl = book.BookUrl;
        //    }
            
        //    // 解析书籍字数
        //    var wordCount = analyzeRule.GetString(rule.WordCount);
        //    if (!string.IsNullOrWhiteSpace(wordCount))
        //    {
        //        book.WordCount = wordCount;
        //    }
            
        //    // 解析内容URL
        //    var contentUrl = analyzeRule.GetString(rule.ContentUrl);
        //    if (!string.IsNullOrWhiteSpace(contentUrl))
        //    {
        //        // TODO: 保存内容URL
        //    }
            
        //    // 执行详情页JS
        //    if (!string.IsNullOrWhiteSpace(rule.DetailJs))
        //    {
        //        //analyzeRule.EvalJS(rule.DetailJs);
        //    }
        //}
    }
}