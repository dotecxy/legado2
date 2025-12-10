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
    /// 获取书籍列表（对应 Kotlin 的 BookList.kt）
    /// </summary>
    public static class BookList
    {
        /// <summary>
        /// 解析书籍列表（对应 Kotlin 的 analyzeBookList）
        /// </summary>
        public static List<SearchBook> AnalyzeBookList(
            BookSource bookSource,
            RuleData ruleData,
            AnalyzeUrl analyzeUrl,
            string baseUrl,
            string body,
            bool isSearch = true,
            bool isRedirect = false,
            Func<string, string, bool> filter = null,
            Func<int, bool> shouldBreak = null)
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new Exception($"Error getting web content: {analyzeUrl.RuleUrl}");
            }

            var bookList = new List<SearchBook>();
            // Debug.Log(bookSource.BookSourceUrl, "≡获取成功:" + analyzeUrl.RuleUrl);

            var analyzeRule = new AnalyzeRule(ruleData, bookSource);
            analyzeRule.SetContent(body);
            analyzeRule.SetBaseUrl(baseUrl);
            analyzeRule.SetRedirectUrl(baseUrl);

            // 检查是否为详情页
            // TODO: 实现 bookUrlPattern 匹配逻辑

            // 获取书籍列表规则
            IBookListRule bookListRule;
            if (isSearch)
            {
                bookListRule = bookSource.RuleSearch;
            }
            else if (bookSource.RuleExplore != null && !string.IsNullOrWhiteSpace(bookSource.RuleExplore.BookList))
            {
                bookListRule = bookSource.RuleExplore;
            }
            else
            {
                bookListRule = bookSource.RuleSearch;
            }

            if (bookListRule == null)
            {
                return bookList;
            }

            var ruleList = bookListRule.BookList ?? "";
            var reverse = false;

            if (ruleList.StartsWith("-"))
            {
                reverse = true;
                ruleList = ruleList.Substring(1);
            }
            if (ruleList.StartsWith("+"))
            {
                ruleList = ruleList.Substring(1);
            }

            // Debug.Log(bookSource.BookSourceUrl, "┌获取书籍列表");
            var collections = analyzeRule.GetElements(ruleList);

            if (collections.Count == 0)
            {
                // Debug.Log(bookSource.BookSourceUrl, "└列表为空,按详情页解析");
                var infoItem = GetInfoItem(
                    bookSource, analyzeRule, analyzeUrl, body, baseUrl,
                    ruleData.getVariable(), isRedirect, filter
                );

                if (infoItem != null)
                {
                    infoItem.InfoHtml = body;
                    bookList.Add(infoItem);
                }
            }
            else
            {
                // Debug.Log(bookSource.BookSourceUrl, $"└列表大小:{collections.Count}");

                // 拆分规则
                var ruleName = analyzeRule.SplitSourceRule(bookListRule.Name);
                var ruleBookUrl = analyzeRule.SplitSourceRule(bookListRule.BookUrl);
                var ruleAuthor = analyzeRule.SplitSourceRule(bookListRule.Author);
                var ruleCoverUrl = analyzeRule.SplitSourceRule(bookListRule.CoverUrl);
                var ruleIntro = analyzeRule.SplitSourceRule(bookListRule.Intro);
                var ruleKind = analyzeRule.SplitSourceRule(bookListRule.Kind);
                var ruleLastChapter = analyzeRule.SplitSourceRule(bookListRule.LastChapter);
                var ruleWordCount = analyzeRule.SplitSourceRule(bookListRule.WordCount);

                for (int index = 0; index < collections.Count; index++)
                {
                    var item = collections[index];
                    var searchBook = GetSearchItem(
                        bookSource, analyzeRule, item, baseUrl,
                        ruleData.getVariable(), index == 0, filter,
                        ruleName, ruleBookUrl, ruleAuthor, ruleCoverUrl,
                        ruleIntro, ruleKind, ruleLastChapter, ruleWordCount
                    );

                    if (searchBook != null)
                    {
                        if (baseUrl == searchBook.BookUrl)
                        {
                            searchBook.InfoHtml = body;
                        }
                        bookList.Add(searchBook);
                    }

                    if (shouldBreak?.Invoke(bookList.Count) == true)
                    {
                        break;
                    }
                }

                // 去重
                var uniqueBooks = bookList.Distinct().ToList();
                bookList = uniqueBooks;

                if (reverse)
                {
                    bookList.Reverse();
                }
            }

            // Debug.Log(bookSource.BookSourceUrl, $"◇书籍总数:{bookList.Count}");
            return bookList;
        }

        /// <summary>
        /// 获取详情页书籍信息（对应 Kotlin 的 getInfoItem）
        /// </summary>
        private static SearchBook GetInfoItem(
            BookSource bookSource,
            AnalyzeRule analyzeRule,
            AnalyzeUrl analyzeUrl,
            string body,
            string baseUrl,
            string variable,
            bool isRedirect,
            Func<string, string, bool> filter)
        {
            var book = new Book { Variable = variable };
            book.BookUrl = isRedirect ? baseUrl : analyzeUrl.RuleUrl;
            book.Origin = bookSource.BookSourceUrl;
            book.OriginName = bookSource.BookSourceName;
            book.OriginOrder = bookSource.CustomOrder;
            book.Type = bookSource.BookSourceType;

            // 解析书籍信息
            BookInfo.AnalyzeBookInfo(
                bookSource, book, baseUrl, baseUrl, body, canReName: false
            ).Wait();

            if (filter?.Invoke(book.Name, book.Author) == false)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(book.Name))
            {
                // 创建 SearchBook 对象
                var searchBook = new SearchBook
                {
                    BookUrl = book.BookUrl,
                    Origin = book.Origin,
                    OriginName = book.OriginName,
                    OriginOrder = book.OriginOrder,
                    Type = book.Type,
                    Name = book.Name,
                    Author = book.Author,
                    Kind = book.Kind,
                    CoverUrl = book.CoverUrl,
                    Intro = book.Intro,
                    WordCount = book.WordCount,
                    LatestChapterTitle = book.LatestChapterTitle,
                    TocUrl = book.TocUrl,
                    Variable = book.Variable
                };
                return searchBook;
            }

            return null;
        }

        /// <summary>
        /// 获取搜索结果条目（对应 Kotlin 的 getSearchItem）
        /// </summary>
        private static SearchBook GetSearchItem(
            BookSource bookSource,
            AnalyzeRule analyzeRule,
            object item,
            string baseUrl,
            string variable,
            bool log,
            Func<string, string, bool> filter,
            List<SourceRule> ruleName,
            List<SourceRule> ruleBookUrl,
            List<SourceRule> ruleAuthor,
            List<SourceRule> ruleCoverUrl,
            List<SourceRule> ruleIntro,
            List<SourceRule> ruleKind,
            List<SourceRule> ruleLastChapter,
            List<SourceRule> ruleWordCount)
        {
            var searchBook = new SearchBook { Variable = variable };
            searchBook.Type = bookSource.BookSourceType;
            searchBook.Origin = bookSource.BookSourceUrl;
            searchBook.OriginName = bookSource.BookSourceName;
            searchBook.OriginOrder = bookSource.CustomOrder;

            analyzeRule.SetContent(item);

            // Debug.Log(bookSource.BookSourceUrl, "┌获取书名", log);
            searchBook.Name = analyzeRule.GetString(ruleName)?.Trim() ?? "";
            // Debug.Log(bookSource.BookSourceUrl, $"└{searchBook.Name}", log);

            if (!string.IsNullOrWhiteSpace(searchBook.Name))
            {
                // Debug.Log(bookSource.BookSourceUrl, "┌获取作者", log);
                searchBook.Author = analyzeRule.GetString(ruleAuthor)?.Trim() ?? "";
                // Debug.Log(bookSource.BookSourceUrl, $"└{searchBook.Author}", log);

                if (filter?.Invoke(searchBook.Name, searchBook.Author) == false)
                {
                    return null;
                }

                // 获取分类
                try
                {
                    var kindList = analyzeRule.GetStringList(ruleKind);
                    searchBook.Kind = kindList != null ? string.Join(",", kindList) : null;
                }
                catch { }

                // 获取字数
                try
                {
                    searchBook.WordCount = analyzeRule.GetString(ruleWordCount);
                }
                catch { }

                // 获取最新章节
                try
                {
                    searchBook.LatestChapterTitle = analyzeRule.GetString(ruleLastChapter);
                }
                catch { }

                // 获取简介
                try
                {
                    searchBook.Intro = analyzeRule.GetString(ruleIntro);
                }
                catch { }

                // 获取封面URL
                try
                {
                    var coverUrl = analyzeRule.GetString(ruleCoverUrl);
                    if (!string.IsNullOrWhiteSpace(coverUrl))
                    {
                        searchBook.CoverUrl = coverUrl;
                    }
                }
                catch { }

                // 获取详情页URL
                searchBook.BookUrl = analyzeRule.GetString(ruleBookUrl, isUrl: true);
                if (string.IsNullOrWhiteSpace(searchBook.BookUrl))
                {
                    searchBook.BookUrl = baseUrl;
                }

                return searchBook;
            }

            return null;
        }
    }
}
