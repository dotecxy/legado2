using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Legado.Core.Data.Entities;
using Legado.Core.Data.Entities.Rules;
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Helps.Books;

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 获取目录
    /// </summary>
    public static class BookChapterList
    {
        /// <summary>
        /// 解析目录列表
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="book">书籍</param>
        /// <param name="baseUrl">基础URL</param>
        /// <param name="redirectUrl">重定向URL</param>
        /// <param name="body">页面内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>章节列表</returns>
        public static async Task<List<BookChapter>> AnalyzeChapterList(
            BookSource bookSource,
            Book book,
            string baseUrl,
            string redirectUrl,
            string body,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new Exception($"获取网页内容失败: {baseUrl}");
            }

            var chapterList = new List<BookChapter>();
            Debug.Log(bookSource.BookSourceUrl, $"≡获取成功:{baseUrl}");
            Debug.Log(bookSource.BookSourceUrl, body, state: 30);

            var tocRule = bookSource.GetTocRule();
            var nextUrlList = new List<string> { redirectUrl };
            bool reverse = false;
            string listRule = tocRule.ChapterList ?? "";

            if (listRule.StartsWith("-"))
            {
                reverse = true;
                listRule = listRule.Substring(1);
            }
            if (listRule.StartsWith("+"))
            {
                listRule = listRule.Substring(1);
            }

            // 解析第一页
            var chapterData = await AnalyzeChapterListInternalAsync(
                book, baseUrl, redirectUrl, body,
                tocRule, listRule, bookSource, true, true, cancellationToken);
            
            chapterList.AddRange(chapterData.Chapters);

            // 处理分页
            switch (chapterData.NextUrls.Count)
            {
                case 0:
                    break;
                case 1:
                    // 单线程顺序解析
                    string nextUrl = chapterData.NextUrls[0];
                    while (!string.IsNullOrEmpty(nextUrl) && !nextUrlList.Contains(nextUrl))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        nextUrlList.Add(nextUrl);

                        var analyzeUrl = new AnalyzeUrl(
                            mUrl: nextUrl,
                            source: bookSource,
                            ruleData: book);
                        
                        var res = await analyzeUrl.GetStrResponseAsync();
                        if (!string.IsNullOrEmpty(res.Body))
                        {
                            chapterData = await AnalyzeChapterListInternalAsync(
                                book, nextUrl, nextUrl,
                                res.Body, tocRule, listRule, bookSource,
                                cancellationToken: cancellationToken);
                            
                            nextUrl = chapterData.NextUrls.FirstOrDefault() ?? "";
                            chapterList.AddRange(chapterData.Chapters);
                        }
                        else
                        {
                            break;
                        }
                    }
                    Debug.Log(bookSource.BookSourceUrl, $"◇目录总页数:{nextUrlList.Count}");
                    break;
                default:
                    // 并发解析
                    Debug.Log(bookSource.BookSourceUrl, $"◇并发解析目录,总页数:{chapterData.NextUrls.Count}");
                    
                    // TODO: 实现并发控制，根据AppConfig.threadCount控制并发数
                    var tasks = chapterData.NextUrls.Select(async urlStr =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var analyzeUrl = new AnalyzeUrl(
                            mUrl: urlStr,
                            source: bookSource,
                            ruleData: book);
                        
                        var res = await analyzeUrl.GetStrResponseAsync();
                        return await AnalyzeChapterListInternalAsync(
                            book, urlStr, res.Url,
                            res.Body ?? "", tocRule, listRule, bookSource, getNextUrl: false,
                            cancellationToken: cancellationToken);
                    });

                    var results = await Task.WhenAll(tasks);
                    foreach (var result in results)
                    {
                        chapterList.AddRange(result.Chapters);
                    }
                    break;
            }

            if (chapterList.Count == 0)
            {
                throw new Exception("目录列表为空");
            }

            if (reverse)
            {
                chapterList.Reverse();
            }

            cancellationToken.ThrowIfCancellationRequested();

            // 去重
            var linkedHashSet = new LinkedList<BookChapter>();
            var seen = new HashSet<string>();
            foreach (var chapter in chapterList)
            {
                var key = chapter.Url + chapter.Title;
                if (seen.Add(key))
                {
                    linkedHashSet.AddLast(chapter);
                }
            }
            var list = linkedHashSet.ToList();

            // TODO: 实现book.GetReverseToc()
            // if (!book.GetReverseToc())
            // {
            //     list.Reverse();
            // }

            Debug.Log(book.Origin, $"◇目录总数:{list.Count}");

            cancellationToken.ThrowIfCancellationRequested();

            // 设置章节索引
            for (int index = 0; index < list.Count; index++)
            {
                list[index].Index = index;
            }

            // 执行格式JS
            var formatJs = tocRule.FormatJs;
            if (!string.IsNullOrEmpty(formatJs))
            {
                // TODO: 实现JS格式化标题
                // using (Context.Enter())
                // {
                //     var bindings = new ScriptBindings();
                //     bindings["gInt"] = 0;
                //     for (int index = 0; index < list.Count; index++)
                //     {
                //         var bookChapter = list[index];
                //         bindings["index"] = index + 1;
                //         bindings["chapter"] = bookChapter;
                //         bindings["title"] = bookChapter.Title;
                //         var result = RhinoScriptEngine.Eval(formatJs, bindings);
                //         if (result != null)
                //         {
                //             bookChapter.Title = result.ToString();
                //         }
                //     }
                // }
            }

            // TODO: 实现标题替换规则
            // var replaceRules = ContentProcessor.Get(book).GetTitleReplaceRules();
            
            // 更新书籍信息
            if (list.Count > 0)
            {
                var durChapterIndex = Math.Min(book.DurChapterIndex, list.Count - 1);
                book.DurChapterTitle = list[durChapterIndex >= 0 ? durChapterIndex : 0].GetDisplayTitle();
            }

            if (book.TotalChapterNum < list.Count)
            {
                // book.LastCheckCount = list.Count - book.TotalChapterNum;
                book.LatestChapterTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            // book.LastCheckTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            book.TotalChapterNum = list.Count;

            if (list.Count > 0)
            {
                var latestIndex = book.SimulatedTotalChapterNum() - 1;
                if (latestIndex < 0) latestIndex = 0;
                if (latestIndex >= list.Count) latestIndex = list.Count - 1;
                book.LatestChapterTitle = list[latestIndex].GetDisplayTitle();
            }

            cancellationToken.ThrowIfCancellationRequested();

            // 获取字数
            GetWordCount(list, book);

            return list;
        }

        /// <summary>
        /// 内部解析目录列表
        /// </summary>
        private static async Task<ChapterListData> AnalyzeChapterListInternalAsync(
            Book book,
            string baseUrl,
            string redirectUrl,
            string body,
            TocRule tocRule,
            string listRule,
            BookSource bookSource,
            bool getNextUrl = true,
            bool log = false,
            CancellationToken cancellationToken = default)
        {
            var analyzeRule = new AnalyzeRule(book, bookSource);
            analyzeRule.SetContent(body).SetBaseUrl(baseUrl);
            analyzeRule.SetRedirectUrl(redirectUrl);
            analyzeRule.SetCancellationToken(cancellationToken);

            // 获取目录列表
            var chapterList = new List<BookChapter>();
            Debug.Log(bookSource.BookSourceUrl, "┌获取目录列表", log);
            var elements = analyzeRule.GetElements(listRule);
            Debug.Log(bookSource.BookSourceUrl, $"└列表大小:{elements.Count}", log);

            // 获取下一页链接
            var nextUrlList = new List<string>();
            var nextTocRule = tocRule.NextTocUrl;
            if (getNextUrl && !string.IsNullOrEmpty(nextTocRule))
            {
                Debug.Log(bookSource.BookSourceUrl, "┌获取目录下一页列表", log);
                var nextUrls = analyzeRule.GetStringList(nextTocRule, isUrl: true);
                if (nextUrls != null)
                {
                    foreach (var item in nextUrls)
                    {
                        if (item != redirectUrl)
                        {
                            nextUrlList.Add(item);
                        }
                    }
                }
                Debug.Log(bookSource.BookSourceUrl, $"└{string.Join("，\n", nextUrlList)}", log);
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (elements.Count > 0)
            {
                Debug.Log(bookSource.BookSourceUrl, "┌解析目录列表", log);
                var nameRule = analyzeRule.SplitSourceRule(tocRule.ChapterName);
                var urlRule = analyzeRule.SplitSourceRule(tocRule.ChapterUrl);
                var vipRule = analyzeRule.SplitSourceRule(tocRule.IsVip);
                var payRule = analyzeRule.SplitSourceRule(tocRule.IsPay);
                var upTimeRule = analyzeRule.SplitSourceRule(tocRule.UpdateTime);
                var isVolumeRule = analyzeRule.SplitSourceRule(tocRule.IsVolume);

                for (int index = 0; index < elements.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var item = elements[index];
                    analyzeRule.SetContent(item);

                    var bookChapter = new BookChapter
                    {
                        BookUrl = book.BookUrl,
                        BaseUrl = redirectUrl
                    };
                    analyzeRule.SetChapter(bookChapter);

                    bookChapter.Title = analyzeRule.GetString(nameRule);
                    bookChapter.Url = analyzeRule.GetString(urlRule);
                    bookChapter.Tag = analyzeRule.GetString(upTimeRule);

                    var isVolume = analyzeRule.GetString(isVolumeRule);
                    bookChapter.IsVolume = false;
                    if (IsTrue(isVolume))
                    {
                        bookChapter.IsVolume = true;
                    }

                    if (string.IsNullOrEmpty(bookChapter.Url))
                    {
                        if (bookChapter.IsVolume)
                        {
                            bookChapter.Url = bookChapter.Title + index;
                            Debug.Log(bookSource.BookSourceUrl, $"⇒一级目录{index}未获取到url,使用标题替代");
                        }
                        else
                        {
                            bookChapter.Url = baseUrl;
                            Debug.Log(bookSource.BookSourceUrl, $"⇒目录{index}未获取到url,使用baseUrl替代");
                        }
                    }

                    if (!string.IsNullOrEmpty(bookChapter.Title))
                    {
                        var isVip = analyzeRule.GetString(vipRule);
                        var isPay = analyzeRule.GetString(payRule);
                        if (IsTrue(isVip))
                        {
                            bookChapter.IsVip = true;
                        }
                        if (IsTrue(isPay))
                        {
                            bookChapter.IsPay = true;
                        }
                        chapterList.Add(bookChapter);
                    }
                }

                Debug.Log(bookSource.BookSourceUrl, "└目录列表解析完成", log);

                if (chapterList.Count == 0)
                {
                    Debug.Log(bookSource.BookSourceUrl, "◇章节列表为空", log);
                }
                else
                {
                    Debug.Log(bookSource.BookSourceUrl, "≡首章信息", log);
                    Debug.Log(bookSource.BookSourceUrl, $"◇章节名称:{chapterList[0].Title}", log);
                    Debug.Log(bookSource.BookSourceUrl, $"◇章节链接:{chapterList[0].Url}", log);
                    Debug.Log(bookSource.BookSourceUrl, $"◇章节信息:{chapterList[0].Tag}", log);
                    Debug.Log(bookSource.BookSourceUrl, $"◇是否VIP:{chapterList[0].IsVip}", log);
                    Debug.Log(bookSource.BookSourceUrl, $"◇是否购买:{chapterList[0].IsPay}", log);
                }
            }

            return new ChapterListData(chapterList, nextUrlList);
        }

        /// <summary>
        /// 获取章节字数
        /// </summary>
        private static void GetWordCount(List<BookChapter> list, Book book)
        {
            // TODO: 实现AppConfig.tocCountWords配置检查
            // if (!AppConfig.TocCountWords)
            // {
            //     return;
            // }

            // TODO: 实现从数据库获取已有章节的字数
            // var chapterList = appDb.BookChapterDao.GetChapterList(book.BookUrl);
            // if (chapterList.Count > 0)
            // {
            //     var map = chapterList.ToDictionary(c => c.GetFileName(), c => c.WordCount);
            //     foreach (var bookChapter in list)
            //     {
            //         if (map.TryGetValue(bookChapter.GetFileName(), out var wordCount))
            //         {
            //             bookChapter.WordCount = wordCount;
            //         }
            //     }
            // }
        }

        /// <summary>
        /// 判断字符串是否为true
        /// </summary>
        private static bool IsTrue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            
            var lower = value.ToLower().Trim();
            return lower == "true" || lower == "1" || lower == "yes";
        }
    }

    /// <summary>
    /// 章节列表数据
    /// </summary>
    internal class ChapterListData
    {
        public List<BookChapter> Chapters { get; }
        public List<string> NextUrls { get; }

        public ChapterListData(List<BookChapter> chapters, List<string> nextUrls)
        {
            Chapters = chapters;
            NextUrls = nextUrls;
        }
    }
}
