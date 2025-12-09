using Legado.Core.Data.Entities;
using Legado.Core.Data.Entities.Rules;
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
        /// <summary>
        /// 解析书籍列表
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="ruleData">规则数据</param>
        /// <param name="analyzeUrl">URL分析器</param>
        /// <param name="baseUrl">基础URL</param>
        /// <param name="body">HTML内容</param>
        /// <param name="isSearch">是否为搜索</param>
        /// <param name="isRedirect">是否为重定向</param>
        /// <param name="filter">过滤函数</param>
        /// <param name="shouldBreak">中断条件</param>
        /// <returns>书籍列表</returns>
        public static List<SearchBook> AnalyzeBookList(
            BookSource bookSource,
            RuleData ruleData,
            AnalyzeUrl analyzeUrl,
            string baseUrl,
            string body,
            bool isSearch,
            bool isRedirect = false,
            Func<string, string, bool> filter = null,
            Func<int, bool> shouldBreak = null)
        {
            var bookList = new List<SearchBook>();
            
            // 获取搜索或发现规则
            IBookListRule searchRule = isSearch ? (IBookListRule)bookSource.RuleSearch : (IBookListRule)bookSource.RuleExplore;
            if (searchRule == null)
            {
                return bookList;
            }

            string listRule = searchRule.BookList;
            string nameRule = searchRule.Name;
            string authorRule = searchRule.Author;
            string bookUrlRule = searchRule.BookUrl;
            string coverUrlRule = searchRule.CoverUrl;
            string kindRule = searchRule.Kind;
            string lastChapterRule = searchRule.LastChapter;
            string introRule = searchRule.Intro;
            
            // 创建解析规则
            var analyzeRule = new AnalyzeRule(ruleData, bookSource);
            analyzeRule.SetContent(body, baseUrl);
            analyzeRule.SetRedirectUrl(baseUrl);
            
            // 解析列表规则 - 获取列表元素
            var list = analyzeRule.GetStringList(listRule);
            if (list == null || !list.Any())
            {
                // 可能是单个结果，尝试直接解析
                list = new List<string> { body };
            }
            
            foreach (var item in list)
            {
                try
                {
                    var elementAnalyzeRule = new AnalyzeRule(ruleData, bookSource);
                    elementAnalyzeRule.SetContent(item, baseUrl);
                    elementAnalyzeRule.SetRedirectUrl(baseUrl);
                    
                    var book = new SearchBook();
                    book.Origin = bookSource.BookSourceUrl;
                    
                    // 解析书名
                    book.Name = elementAnalyzeRule.GetString(nameRule);
                    if (string.IsNullOrWhiteSpace(book.Name))
                    {
                        continue;
                    }
                    
                    // 解析作者
                    book.Author = elementAnalyzeRule.GetString(authorRule);
                    
                    // 解析书籍URL
                    book.BookUrl = elementAnalyzeRule.GetString(bookUrlRule, isUrl: true);
                    if (string.IsNullOrWhiteSpace(book.BookUrl))
                    {
                        continue;
                    }
                    
                    // 应用过滤
                    if (filter != null && !filter(book.Name, book.Author))
                    {
                        continue;
                    }
                    
                    // 解析封面URL
                    book.CoverUrl = elementAnalyzeRule.GetString(coverUrlRule, isUrl: true);
                    
                    // 解析分类
                    book.Kind = elementAnalyzeRule.GetString(kindRule);
                    
                    // 解析最新章节
                    book.LatestChapterTitle = elementAnalyzeRule.GetString(lastChapterRule);
                    
                    // 解析简介
                    book.Intro = elementAnalyzeRule.GetString(introRule);
                    
                    // 添加到列表
                    bookList.Add(book);
                    
                    // 检查中断条件
                    if (shouldBreak != null && shouldBreak(bookList.Count))
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    // 忽略单个条目的解析错误，继续下一个
                    continue;
                }
            }
            
            return bookList;
        }
    }
}