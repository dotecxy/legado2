using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegadoBookSource.AnalyzeRule;
using LegadoBookSource.Entities;

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 搜索模型类
    /// </summary>
    public static class SearchModel
    {
        /// <summary>
        /// 搜索书籍
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="keyword">关键词</param>
        /// <param name="page">页码</param>
        /// <returns>搜索结果</returns>
        public static async Task<SearchResult>
            Search(
            BookSource bookSource,
            string keyword,
            int page)
        {
            var searchRule = bookSource.GetSearchRule();
            if (searchRule == null)
            {
                throw new Exception("搜索规则不存在");
            }
            
            var baseUrl = bookSource.BookSourceUrl;
            var ruleData = new RuleData { BaseUrl = baseUrl };
            
            // 构建搜索URL
            var analyzeUrl = new AnalyzeUrl(
                searchRule.Url,
                keyword,
                page,
                baseUrl,
                ruleData
            );
            
            // 执行搜索
            var body = analyzeUrl.Execute();
            var redirectUrl = analyzeUrl.Url;
            
            // 创建StrResponse对象
            var res = new StrResponse(searchRule.Url, body);
            
            // 解析搜索结果
            // AnalyzeRule是命名空间，需要使用完整的类型名称
            var analyzeRule = new LegadoBookSource.AnalyzeRule.AnalyzeRule(null, body, true);
            analyzeRule.SetContent(body, baseUrl);
            
            // 获取书籍列表
            var bookElements = analyzeRule.GetElements(searchRule.BookList);
            
            var bookList = new List<SearchBook>();
            foreach (var element in bookElements)
            {
                // 为每个元素创建一个新的AnalyzeRule实例
                var elementAnalyzeRule = new LegadoBookSource.AnalyzeRule.AnalyzeRule(null, element, true);
                elementAnalyzeRule.SetContent(body, baseUrl);
                
                var searchBook = new SearchBook
                {
                    Name = elementAnalyzeRule.GetString(searchRule.Name),
                    Author = elementAnalyzeRule.GetString(searchRule.Author),
                    BookUrl = elementAnalyzeRule.GetString(searchRule.BookUrl, false, true), // isUrl=true
                    CoverUrl = elementAnalyzeRule.GetString(searchRule.CoverUrl, false, true), // isUrl=true
                    LastChapter = elementAnalyzeRule.GetString(searchRule.LastChapter),
                    Intro = elementAnalyzeRule.GetString(searchRule.Intro)
                    // SearchBook类没有UpdateTime和WordCount属性，移除这些赋值
                };
                bookList.Add(searchBook);
            }
            
            // 检查是否有下一页
            var hasNext = false;
            if (page == 1 && !string.IsNullOrWhiteSpace(searchRule.NextPage))
            {
                // 使用GetString方法检查是否有下一页
                hasNext = !string.IsNullOrWhiteSpace(analyzeRule.GetString(searchRule.NextPage));
            }
            
            return new SearchResult
            {
                BookList = bookList,
                HasNext = hasNext
            };
        }
        
        /// <summary>
        /// 探索书籍
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="page">页码</param>
        /// <returns>探索结果</returns>
        public static async Task<SearchResult>
            Explore(
            BookSource bookSource,
            int page)
        {
            var exploreRule = bookSource.GetExploreRule();
            if (exploreRule == null)
            {
                throw new Exception("探索规则不存在");
            }
            
            var baseUrl = bookSource.BookSourceUrl;
            var ruleData = new RuleData { BaseUrl = baseUrl };
            
            // 构建探索URL
            var analyzeUrl = new AnalyzeUrl(
                exploreRule.Url,
                null, // key参数
                page,
                baseUrl,
                ruleData
            );
            
            // 执行探索请求
            var body = analyzeUrl.Execute();
            var redirectUrl = analyzeUrl.Url;
            
            // 创建StrResponse对象
            var res = new StrResponse(exploreRule.Url, body);
            
            // 解析探索结果
            // AnalyzeRule是命名空间，需要使用完整的类型名称
            var analyzeRule = new LegadoBookSource.AnalyzeRule.AnalyzeRule(null, body, true);
            analyzeRule.SetContent(body, baseUrl);
            
            // 获取书籍列表
            var bookElements = analyzeRule.GetElements(exploreRule.BookList);
            
            var bookList = new List<SearchBook>();
            foreach (var element in bookElements)
            {
                // 为每个元素创建一个新的AnalyzeRule实例
                var elementAnalyzeRule = new LegadoBookSource.AnalyzeRule.AnalyzeRule(null, element, true);
                elementAnalyzeRule.SetContent(body, baseUrl);
                
                var searchBook = new SearchBook
                {
                    Name = elementAnalyzeRule.GetString(exploreRule.Name),
                    Author = elementAnalyzeRule.GetString(exploreRule.Author),
                    BookUrl = elementAnalyzeRule.GetString(exploreRule.BookUrl, false, true), // isUrl=true
                    CoverUrl = elementAnalyzeRule.GetString(exploreRule.CoverUrl, false, true), // isUrl=true
                    LastChapter = elementAnalyzeRule.GetString(exploreRule.LastChapter),
                    Intro = elementAnalyzeRule.GetString(exploreRule.Intro)
                    // SearchBook类没有UpdateTime和WordCount属性，移除这些赋值
                };
                bookList.Add(searchBook);
            }
            
            // 检查是否有下一页
            var hasNext = false;
            if (page == 1 && !string.IsNullOrWhiteSpace(exploreRule.NextPage))
            {
                // 使用GetString方法检查是否有下一页
                hasNext = !string.IsNullOrWhiteSpace(analyzeRule.GetString(exploreRule.NextPage));
            }
            
            return new SearchResult
            {
                BookList = bookList,
                HasNext = hasNext
            };
        }
    }
    
    /// <summary>
    /// 搜索结果类
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// 书籍列表
        /// </summary>
        public List<SearchBook> BookList { get; set; }
        
        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNext { get; set; }
    }
}