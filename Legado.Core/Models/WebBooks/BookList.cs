using Legado.Core.Data.Entities;
using Legado.Core.Models.AnalyzeRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 
namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 书籍列表解析类
    /// </summary>
    public static class BookList
    {
        ///// <summary>
        ///// 解析书籍列表
        ///// </summary>
        ///// <param name="bookSource">书源</param>
        ///// <param name="ruleData">规则数据</param>
        ///// <param name="analyzeUrl">URL分析器</param>
        ///// <param name="baseUrl">基础URL</param>
        ///// <param name="body">HTML内容</param>
        ///// <param name="isSearch">是否为搜索</param>
        ///// <param name="isRedirect">是否为重定向</param>
        ///// <param name="filter">过滤函数</param>
        ///// <param name="shouldBreak">中断条件</param>
        ///// <returns>书籍列表</returns>
        //public static List<SearchBook> AnalyzeBookList(
        //    BookSource bookSource,
        //    RuleData ruleData,
        //    AnalyzeUrl analyzeUrl,
        //    string baseUrl,
        //    string body,
        //    bool isSearch,
        //    bool isRedirect = false,
        //    Func<string, string, bool> filter = null,
        //    Func<int, bool> shouldBreak = null)
        //{
        //    var bookList = new List<SearchBook>();
        //    // 获取搜索或发现规则
        //    object rule = isSearch ? (object)bookSource.RuleSearch : (object)bookSource.GetExploreRule();
        //    string listRule = string.Empty;
        //    string nameRule = string.Empty;
        //    string authorRule = string.Empty;
        //    string bookUrlRule = string.Empty;
        //    string coverUrlRule = string.Empty;
        //    string kindRule = string.Empty;
        //    string lastChapterRule = string.Empty;
        //    string introRule = string.Empty;
            
        //    // 根据规则类型获取相应属性
        //    if (isSearch)
        //    {
        //        var searchRule = (RuleSearch)rule;
        //        listRule = searchRule.BookList;
        //        nameRule = searchRule.Name;
        //        authorRule = searchRule.Author;
        //        bookUrlRule = searchRule.BookUrl;
        //        coverUrlRule = searchRule.CoverUrl;
        //        kindRule = searchRule.Kind;
        //        lastChapterRule = searchRule.LastChapter;
        //        introRule = searchRule.Intro;
        //    }
        //    else
        //    {
        //        var exploreRule = (LegadoBookSource.Rules.ExploreRule)rule;
        //        listRule = exploreRule.BookList;
        //        nameRule = exploreRule.Name;
        //        authorRule = exploreRule.Author;
        //        bookUrlRule = exploreRule.BookUrl;
        //        coverUrlRule = exploreRule.CoverUrl;
        //        kindRule = exploreRule.Kind;
        //        lastChapterRule = exploreRule.LastChapter;
        //        introRule = exploreRule.Intro;
        //    }
            
        //    var analyzeRule = new LegadoBookSource.AnalyzeRule.AnalyzeRule(null, body, true);
        //    analyzeRule.SetContent(body, baseUrl);
            
        //    // 解析列表规则
        //    var list = analyzeRule.GetElements(listRule);
        //    if (list == null || !list.Any())
        //    {
        //        // TODO: 添加日志
        //        return bookList;
        //    }
            
        //    foreach (var item in list)
        //    {
        //        var elementAnalyzeRule = new LegadoBookSource.AnalyzeRule.AnalyzeRule(null, item, true);
        //        elementAnalyzeRule.SetContent(item, baseUrl);
                
        //        var book = new SearchBook();
                
        //        // 解析书名
        //        book.Name = elementAnalyzeRule.GetString(nameRule);
        //        if (string.IsNullOrWhiteSpace(book.Name))
        //        {
        //            continue;
        //        }
                
        //        // 解析作者
        //        book.Author = elementAnalyzeRule.GetString(authorRule);
                
        //        // 解析书籍URL
        //        book.BookUrl = elementAnalyzeRule.GetString(bookUrlRule);
        //        if (string.IsNullOrWhiteSpace(book.BookUrl))
        //        {
        //            continue;
        //        }
                
        //        // 处理相对URL
        //        if (!string.IsNullOrWhiteSpace(book.BookUrl) && !book.BookUrl.StartsWith("http"))
        //        {
        //            book.BookUrl = new Uri(new Uri(baseUrl), book.BookUrl).ToString();
        //        }
                
        //        // 应用过滤
        //        if (filter != null && !filter(book.Name, book.Author))
        //        {
        //            continue;
        //        }
                
        //        // 解析封面URL
        //        book.CoverUrl = elementAnalyzeRule.GetString(coverUrlRule);
        //        if (!string.IsNullOrWhiteSpace(book.CoverUrl) && !book.CoverUrl.StartsWith("http"))
        //        {
        //            book.CoverUrl = new Uri(new Uri(baseUrl), book.CoverUrl).ToString();
        //        }
                
        //        // 解析分类
        //        book.Kind = elementAnalyzeRule.GetString(kindRule);
                
        //        // 解析最新章节
        //        book.LastChapter = elementAnalyzeRule.GetString(lastChapterRule);
                
        //        // 解析简介
        //        book.Intro = elementAnalyzeRule.GetString(introRule);
                
        //        // 添加到列表
        //        bookList.Add(book);
                
        //        // 检查中断条件
        //        if (shouldBreak != null && shouldBreak(bookList.Count))
        //        {
        //            break;
        //        }
        //    }
            
        //    return bookList;
        //}
    }
}