using Legado.Core.Data.Entities;
using Legado.Core.Data.Entities.Rules;
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 获取书籍列表（对应 Kotlin 的 BookList.kt）
    /// </summary>
    public static class BookList
    {
        /// <summary>
        /// 分析书籍列表
        /// 对应 Kotlin 的 analyzeBookList()
        /// </summary>
        public static async Task<List<SearchBook>> AnalyzeBookList(
            BookSource bookSource,
            RuleData ruleData,
            AnalyzeUrl analyzeUrl,
            string baseUrl,
            string body,
            bool isSearch = true,
            bool isRedirect = false,
            Func<string, string, bool> filter = null,
            Func<int, bool> shouldBreak = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new Exception($"Error getting web content: {analyzeUrl.RuleUrl}");
            }

            var bookList = new List<SearchBook>();
            // Debug.Log($"≡获取成功:{analyzeUrl.RuleUrl}");
            // Debug.Log(body, state: 10);

            var analyzeRule = new AnalyzeRule(ruleData, bookSource);
            analyzeRule.SetContent(body).SetBaseUrl(baseUrl);
            analyzeRule.SetRedirectUrl(baseUrl);
            // analyzeRule.SetCoroutineContext(coroutineContext);

            if (!isSearch)
            {
                CheckExploreJson(bookSource);
            }

            // 检查是否为详情页
            // TODO: 实现 BookUrlPattern 检查
            /*
            if (isSearch && !string.IsNullOrWhiteSpace(bookSource.BookUrlPattern))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var regex = new Regex(bookSource.BookUrlPattern);
                if (regex.IsMatch(baseUrl))
                {
                    // Debug.Log("≡链接为详情页");
                    var searchBook = await GetInfoItem(
                        bookSource,
                        analyzeRule,
                        analyzeUrl,
                        body,
                        baseUrl,
                        ruleData.getVariable(),
                        isRedirect,
                        filter,
                        cancellationToken
                    );
                    if (searchBook != null)
                    {
                        searchBook.InfoHtml = body;
                        bookList.Add(searchBook);
                    }
                    return bookList;
                }
            }
            */

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

            var ruleList = bookListRule?.BookList ?? "";
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

            // Debug.Log("┌获取书籍列表");
            var collections = analyzeRule.GetElements(ruleList);
            cancellationToken.ThrowIfCancellationRequested();

            // TODO: 实现 BookUrlPattern 检查
            if (collections.Count == 0) // && string.IsNullOrEmpty(bookSource.BookUrlPattern))
            {
                // Debug.Log("└列表为空,按详情页解析");
                var searchBook = await GetInfoItem(
                    bookSource,
                    analyzeRule,
                    analyzeUrl,
                    body,
                    baseUrl,
                    ruleData.GetVariable(),
                    isRedirect,
                    filter,
                    cancellationToken
                );
                if (searchBook != null)
                {
                    searchBook.InfoHtml = body;
                    bookList.Add(searchBook);
                }
            }
            else
            {
                var ruleName = analyzeRule.SplitSourceRule(bookListRule.Name);
                var ruleBookUrl = analyzeRule.SplitSourceRule(bookListRule.BookUrl);
                var ruleAuthor = analyzeRule.SplitSourceRule(bookListRule.Author);
                var ruleCoverUrl = analyzeRule.SplitSourceRule(bookListRule.CoverUrl);
                var ruleIntro = analyzeRule.SplitSourceRule(bookListRule.Intro);
                var ruleKind = analyzeRule.SplitSourceRule(bookListRule.Kind);
                var ruleLastChapter = analyzeRule.SplitSourceRule(bookListRule.LastChapter);
                var ruleWordCount = analyzeRule.SplitSourceRule(bookListRule.WordCount);

                // Debug.Log($"└列表大小:{collections.Count}");

                for (int index = 0; index < collections.Count; index++)
                {
                    var item = collections[index];
                    var searchBook = await GetSearchItem(
                        bookSource,
                        analyzeRule,
                        item,
                        baseUrl,
                        ruleData.GetVariable(),
                        index == 0,
                        filter,
                        ruleName,
                        ruleBookUrl,
                        ruleAuthor,
                        ruleKind,
                        ruleCoverUrl,
                        ruleWordCount,
                        ruleIntro,
                        ruleLastChapter,
                        cancellationToken
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
                var uniqueBooks = new LinkedHashSet<SearchBook>(bookList);
                bookList.Clear();
                bookList.AddRange(uniqueBooks);

                if (reverse)
                {
                    bookList.Reverse();
                }
            }

            // Debug.Log($"◇书籍总数:{bookList.Count}");
            return bookList;
        }

        /// <summary>
        /// 获取详情页信息项
        /// 对应 Kotlin 的 getInfoItem()
        /// </summary>
        private static async Task<SearchBook> GetInfoItem(
            BookSource bookSource,
            AnalyzeRule analyzeRule,
            AnalyzeUrl analyzeUrl,
            string body,
            string baseUrl,
            string variable,
            bool isRedirect,
            Func<string, string, bool> filter,
            CancellationToken cancellationToken = default)
        {
            var book = new Book { Variable = variable };
            book.BookUrl = isRedirect
                ? baseUrl
                : NetworkUtils.GetAbsoluteURL(analyzeUrl.Url, analyzeUrl.RuleUrl);
            book.Origin = bookSource.BookSourceUrl;
            book.OriginName = bookSource.BookSourceName;
            book.OriginOrder = bookSource.CustomOrder;
            book.Type = GetBookType(bookSource);

            analyzeRule.SetRuleData(book);
            await BookInfo.AnalyzeBookInfo(
                book,
                body,
                analyzeRule,
                bookSource,
                baseUrl,
                baseUrl,
                false
            );

            if (filter?.Invoke(book.Name, book.Author) == false)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(book.Name))
            {
                return ToSearchBook(book);
            }

            return null;
        }

        /// <summary>
        /// 获取搜索项
        /// 对应 Kotlin 的 getSearchItem()
        /// </summary>
        private static async Task<SearchBook> GetSearchItem(
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
            List<SourceRule> ruleKind,
            List<SourceRule> ruleCoverUrl,
            List<SourceRule> ruleWordCount,
            List<SourceRule> ruleIntro,
            List<SourceRule> ruleLastChapter,
            CancellationToken cancellationToken = default)
        {
            var searchBook = new SearchBook { Variable = variable };
            searchBook.Type = GetBookType(bookSource);
            searchBook.Origin = bookSource.BookSourceUrl;
            searchBook.OriginName = bookSource.BookSourceName;
            searchBook.OriginOrder = bookSource.CustomOrder;

            analyzeRule.SetRuleData(searchBook);
            analyzeRule.SetContent(item);
            cancellationToken.ThrowIfCancellationRequested();

            // Debug.Log("┌获取书名", log);
            searchBook.Name = FormatBookName(analyzeRule.GetString(ruleName));
            // Debug.Log($"└{searchBook.Name}", log);

            if (!string.IsNullOrEmpty(searchBook.Name))
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Debug.Log("┌获取作者", log);
                searchBook.Author = FormatBookAuthor(analyzeRule.GetString(ruleAuthor));
                // Debug.Log($"└{searchBook.Author}", log);

                if (filter?.Invoke(searchBook.Name, searchBook.Author) == false)
                {
                    return null;
                }

                cancellationToken.ThrowIfCancellationRequested();
                // Debug.Log("┌获取分类", log);
                try
                {
                    var kindList = analyzeRule.GetStringList(ruleKind);
                    if (kindList != null && kindList.Count > 0)
                    {
                        searchBook.Kind = string.Join(",", kindList);
                    }
                    // Debug.Log($"└{searchBook.Kind ?? ""}", log);
                }
                catch (Exception e)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // Debug.Log($"└{e.Message}", log);
                }

                cancellationToken.ThrowIfCancellationRequested();
                // Debug.Log("┌获取字数", log);
                try
                {
                    searchBook.WordCount = WordCountFormat(analyzeRule.GetString(ruleWordCount));
                    // Debug.Log($"└{searchBook.WordCount}", log);
                }
                catch (Exception e)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // Debug.Log($"└{e.Message}", log);
                }

                cancellationToken.ThrowIfCancellationRequested();
                // Debug.Log("┌获取最新章节", log);
                try
                {
                    searchBook.LatestChapterTitle = analyzeRule.GetString(ruleLastChapter);
                    // Debug.Log($"└{searchBook.LatestChapterTitle}", log);
                }
                catch (Exception e)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // Debug.Log($"└{e.Message}", log);
                }

                cancellationToken.ThrowIfCancellationRequested();
                // Debug.Log("┌获取简介", log);
                try
                {
                    var intro = analyzeRule.GetString(ruleIntro);
                    searchBook.Intro = FormatHtml(intro);
                    // Debug.Log($"└{searchBook.Intro}", log);
                }
                catch (Exception e)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // Debug.Log($"└{e.Message}", log);
                }

                cancellationToken.ThrowIfCancellationRequested();
                // Debug.Log("┌获取封面链接", log);
                try
                {
                    var coverUrl = analyzeRule.GetString(ruleCoverUrl);
                    if (!string.IsNullOrEmpty(coverUrl))
                    {
                        searchBook.CoverUrl = NetworkUtils.GetAbsoluteURL(baseUrl, coverUrl);
                    }
                    // Debug.Log($"└{searchBook.CoverUrl ?? ""}", log);
                }
                catch (Exception e)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // Debug.Log($"└{e.Message}", log);
                }

                cancellationToken.ThrowIfCancellationRequested();
                // Debug.Log("┌获取详情页链接", log);
                searchBook.BookUrl = analyzeRule.GetString(ruleBookUrl, isUrl: true);
                if (string.IsNullOrEmpty(searchBook.BookUrl))
                {
                    searchBook.BookUrl = baseUrl;
                }
                // Debug.Log($"└{searchBook.BookUrl}", log);

                return searchBook;
            }

            return null;
        }

        /// <summary>
        /// 检查发现规则 JSON 格式
        /// 对应 Kotlin 的 checkExploreJson()
        /// </summary>
        private static void CheckExploreJson(BookSource bookSource)
        {
            // TODO: 实现 Debug.callback 检查
            // if (Debug.callback == null) return;
            
            var json = ExploreKindsJson(bookSource);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            try
            {
                var kinds = JsonConvert.DeserializeObject<List<ExploreKind>>(json);
                if (kinds != null)
                {
                    return;
                }
            }
            catch
            {
                // Debug.Log("≡发现地址规则 JSON 格式不规范，请改为规范格式");
            }
        }

        /// <summary>
        /// 获取发现规则 JSON
        /// 对应 Kotlin 的 BookSource.exploreKindsJson()
        /// </summary>
        private static string ExploreKindsJson(BookSource bookSource)
        {
            return bookSource.ExploreUrl ?? "";
        }

        /// <summary>
        /// 获取书籍类型
        /// 对应 Kotlin 的 BookSource.getBookType()
        /// </summary>
        private static int GetBookType(BookSource bookSource)
        {
            return bookSource.BookSourceType;
        }

        /// <summary>
        /// Book 转换为 SearchBook
        /// 对应 Kotlin 的 Book.toSearchBook()
        /// </summary>
        private static SearchBook ToSearchBook(Book book)
        {
            return new SearchBook
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
        }

        /// <summary>
        /// 格式化书名
        /// 对应 Kotlin 的 BookHelp.formatBookName()
        /// </summary>
        private static string FormatBookName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";
            return name.Trim();
        }

        /// <summary>
        /// 格式化作者名
        /// 对应 Kotlin 的 BookHelp.formatBookAuthor()
        /// </summary>
        private static string FormatBookAuthor(string author)
        {
            if (string.IsNullOrWhiteSpace(author))
                return "";
            return author.Trim();
        }

        /// <summary>
        /// 格式化 HTML 内容
        /// 对应 Kotlin 的 HtmlFormatter.format()
        /// </summary>
        private static string FormatHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return "";
            // TODO: 实现完整的 HTML 格式化逻辑
            return html.Trim();
        }

        /// <summary>
        /// 格式化字数
        /// 对应 Kotlin 的 StringUtils.wordCountFormat()
        /// </summary>
        private static string WordCountFormat(string wordCount)
        {
            if (string.IsNullOrWhiteSpace(wordCount))
                return "";
            // TODO: 实现字数格式化逻辑（如：10000 -> 1万字）
            return wordCount.Trim();
        }
    }

    /// <summary>
    /// LinkedHashSet 实现（保持插入顺序的去重集合）
    /// </summary>
    internal class LinkedHashSet<T> : IEnumerable<T>
    {
        private readonly Dictionary<T, LinkedListNode<T>> _dict;
        private readonly LinkedList<T> _list;

        public LinkedHashSet()
        {
            _dict = new Dictionary<T, LinkedListNode<T>>();
            _list = new LinkedList<T>();
        }

        public LinkedHashSet(IEnumerable<T> collection) : this()
        {
            foreach (var item in collection)
            {
                Add(item);
            }
        }

        public bool Add(T item)
        {
            if (_dict.ContainsKey(item))
                return false;

            var node = _list.AddLast(item);
            _dict[item] = node;
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
