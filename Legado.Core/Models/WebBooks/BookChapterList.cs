using Jint;
using Legado.Core.Data.Entities;
using Legado.Core.Data.Entities.Rules;
using Legado.Core.Models;
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Utils; // 假设 IsTrue() 等扩展方法在这里
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 获取目录
    /// 对应 Kotlin: BookChapterList.kt
    /// </summary>
    public static class BookChapterList
    {
        // 模拟 AppConfig
        private static int ThreadCount => 5; // AppConfig.threadCount
        private static bool TocCountWords => false; // AppConfig.tocCountWords

        /// <summary>
        /// 分析章节列表 (主入口)
        /// </summary>
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
                throw new Exception($"Error getting web content: {baseUrl}");
            }

            var chapterList = new List<BookChapter>();
            // Debug.Log(bookSource.BookSourceUrl, $"≡获取成功:{baseUrl}");

            var tocRule = bookSource.RuleToc; // 对应 bookSource.getTocRule()
            var nextUrlList = new HashSet<string> { redirectUrl }; // Set去重
            var nextUrlListOrder = new List<string> { redirectUrl }; // List保序

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

            // 1. 解析第一页
            var chapterData = AnalyzeChapterListInternal(
                book, baseUrl, redirectUrl, body,
                tocRule, listRule, bookSource, getNextUrl: true, log: true
            );

            chapterList.AddRange(chapterData.Item1);

            // 2. 处理下一页
            var foundNextUrls = chapterData.Item2;

            if (foundNextUrls.Count == 1)
            {
                // 单页模式（链式加载）
                string nextUrl = foundNextUrls[0];
                while (!string.IsNullOrEmpty(nextUrl) && !nextUrlList.Contains(nextUrl))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    nextUrlList.Add(nextUrl);

                    // 构造 AnalyzeUrl 并请求
                    var analyzeUrl = new AnalyzeUrl(
                        mUrl: nextUrl,
                        source: bookSource,
                        ruleData: book
                    );

                    try
                    {
                        var res = await analyzeUrl.GetStrResponseAwait(); // 控制并发访问
                        if (!string.IsNullOrEmpty(res.Body))
                        {
                            chapterData = AnalyzeChapterListInternal(
                                book, nextUrl, res.Url, // nextUrl -> redirectUrl
                                res.Body, tocRule, listRule, bookSource, getNextUrl: true, log: false
                            );

                            chapterList.AddRange(chapterData.Item1);
                            nextUrl = chapterData.Item2.FirstOrDefault() ?? "";
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Debug.Log error
                        break;
                    }
                }
            }
            else if (foundNextUrls.Count > 1)
            {
                // 并发模式
                // Debug.Log(bookSource.BookSourceUrl, $"◇并发解析目录,总页数:{foundNextUrls.Count}");

                // C# 并发控制：SemaphoreSlim
                using (var semaphore = new SemaphoreSlim(ThreadCount))
                {
                    var tasks = foundNextUrls.Select(async urlStr =>
                    {
                        await semaphore.WaitAsync(cancellationToken);
                        try
                        {
                            var analyzeUrl = new AnalyzeUrl(
                                mUrl: urlStr,
                                source: bookSource,
                                ruleData: book
                            );
                            var res = await analyzeUrl.GetStrResponseAwait();

                            return AnalyzeChapterListInternal(
                                book, urlStr, res.Url,
                                res.Body, tocRule, listRule, bookSource, getNextUrl: false, log: false
                            ).Item1;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    var results = await Task.WhenAll(tasks);
                    foreach (var list in results)
                    {
                        chapterList.AddRange(list);
                    }
                }
            }

            if (chapterList.Count == 0)
            {
                throw new Exception("Chapter list is empty");
            }

            // 3. 排序处理
            if (!reverse)
            {
                chapterList.Reverse();
            }

            cancellationToken.ThrowIfCancellationRequested();

            // 4. 去重 (LinkedHashSet logic: 保持插入顺序)
            // BookChapter 需要重写 Equals 和 GetHashCode (基于 Url 或 Title)
            var distinctList = chapterList.Distinct().ToList();

            // 检查书籍设置是否需要反转
            // if (!book.GetReverseToc()) distinctList.Reverse(); 
            // 注意：Kotlin代码逻辑是如果 book.getReverseToc() 为 false (默认)，则 Reverse。
            // 这意味着默认是正序（0->N），但前面 chapterList.Reverse() 了一次，所以这里再次 Reverse 恢复正序？
            // 原逻辑：
            // if (!reverse) chapterList.reverse() -> 此时通常是倒序（最新在最前）
            // if (!book.getReverseToc()) list.reverse() -> 如果不强制反转，再次反转 -> 变回正序
            // 结论：默认输出正序。

            // 假设 book.GetReverseToc() 默认为 false
            distinctList.Reverse();

            // Debug.Log(book.Origin, $"◇目录总数:{distinctList.Count}");

            // 5. 重置 Index
            for (int i = 0; i < distinctList.Count; i++)
            {
                distinctList[i].Index = i;
            }

            // 6. 执行 FormatJs
            string formatJs = tocRule.FormatJs;
            if (!string.IsNullOrWhiteSpace(formatJs))
            {
                var engine = new Engine(); // Jint
                engine.SetValue("gInt", 0);

                // 为了性能，JS Context 应该复用，但这里每个 Chapter 都要跑一次 Eval
                // 更好的做法是把 formatJs 包装成一个函数，然后循环调用
                // 这里简单移植：
                for (int i = 0; i < distinctList.Count; i++)
                {
                    var chapter = distinctList[i];
                    try
                    {
                        engine.SetValue("index", i + 1);
                        engine.SetValue("chapter", chapter);
                        engine.SetValue("title", chapter.Title);

                        var res = engine.Evaluate(formatJs);
                        if (res != null) chapter.Title = res.ToString();
                    }
                    catch (Exception ex)
                    {
                        // Log error
                    }
                }
            }

            // 7. 更新书籍信息 (LatestChapter 等)
            // ReplaceRules 逻辑略

            // book.TotalChapterNum = distinctList.Count;
            // ... 更新 LastCheckTime 等 ...

            cancellationToken.ThrowIfCancellationRequested();

            // 8. 填充字数 (可选)
            GetWordCount(distinctList, book);

            return distinctList;
        }

        /// <summary>
        /// 内部解析方法 (单页解析)
        /// </summary>
        private static Tuple<List<BookChapter>, List<string>> AnalyzeChapterListInternal(
            Book book,
            string baseUrl,
            string redirectUrl,
            string body,
            TocRule tocRule,
            string listRule,
            BookSource bookSource,
            bool getNextUrl = true,
            bool log = false)
        {
            var analyzeRule = new AnalyzeRule(null, bookSource);
            analyzeRule.SetContent(body);
            analyzeRule.SetBaseUrl ( baseUrl);
            analyzeRule.SetRedirectUrl(redirectUrl);

            // 获取目录列表
            var chapterList = new List<BookChapter>();
            // Debug.Log...

            // AnalyzeRule.GetElements 返回的是 List<Element> (AngleSharp) 或 List<string>
            // 这里假设 analyzeRule.GetElements 返回 List<object> (可以是 Element 或 String)
            var elements = analyzeRule.GetStringList(listRule);
            // Debug.Log...

            // 获取下一页链接
            var nextUrlList = new List<string>();
            string nextTocRule = tocRule.NextTocUrl;

            if (getNextUrl && !string.IsNullOrEmpty(nextTocRule))
            {
                // Debug.Log...
                var urls = analyzeRule.GetStringList(nextTocRule, isUrl: true);
                if (urls != null)
                {
                    foreach (var item in urls)
                    {
                        if (item != redirectUrl)
                        {
                            nextUrlList.Add(item);
                        }
                    }
                }
            }

            if (elements != null && elements.Count > 0)
            {
                // 预处理规则，避免在循环中重复 Split
                var nameRule = analyzeRule.SplitSourceRule(tocRule.ChapterName);
                var urlRule = analyzeRule.SplitSourceRule(tocRule.ChapterUrl);
                var vipRule = analyzeRule.SplitSourceRule(tocRule.IsVip);
                var payRule = analyzeRule.SplitSourceRule(tocRule.IsPay);
                var upTimeRule = analyzeRule.SplitSourceRule(tocRule.UpdateTime);
                var isVolumeRule = analyzeRule.SplitSourceRule(tocRule.IsVolume);

                for (int i = 0; i < elements.Count; i++)
                {
                    var item = elements[i];
                    analyzeRule.SetContent(item); // 切换当前上下文为列表项

                    var bookChapter = new BookChapter
                    {
                        BookUrl = book.BookUrl,
                        // BaseUrl = redirectUrl // 这里的 BaseUrl 指来源 URL
                    };

                    // analyzeRule.SetChapter(bookChapter); // 注入 chapter 变量供 rule 使用

                    bookChapter.Title = analyzeRule.GetString(nameRule);
                    bookChapter.Url = analyzeRule.GetString(urlRule);
                    bookChapter.Tag = analyzeRule.GetString(upTimeRule);

                    // IsVolume 处理
                    string isVolumeStr = analyzeRule.GetString(isVolumeRule);
                    bookChapter.IsVolume = IsTrue(isVolumeStr);

                    // Url 为空处理
                    if (string.IsNullOrEmpty(bookChapter.Url))
                    {
                        if (bookChapter.IsVolume)
                        {
                            bookChapter.Url = bookChapter.Title + i; // 卷名无链接，用标题+索引做ID
                        }
                        else
                        {
                            bookChapter.Url = baseUrl; // 降级
                        }
                    }

                    if (!string.IsNullOrEmpty(bookChapter.Title))
                    {
                        string isVipStr = analyzeRule.GetString(vipRule);
                        string isPayStr = analyzeRule.GetString(payRule);

                        if (IsTrue(isVipStr)) bookChapter.IsVip = true;
                        if (IsTrue(isPayStr)) bookChapter.IsPay = true;

                        chapterList.Add(bookChapter);
                    }
                }
            }

            return Tuple.Create(chapterList, nextUrlList);
        }

        private static void GetWordCount(List<BookChapter> list, Book book)
        {
            if (!TocCountWords) return;
            // 模拟数据库查询
            // var cachedChapters = DB.GetChapters(book.BookUrl);
            // logic...
        }

        // 辅助方法：模拟 String.isTrue()
        private static bool IsTrue(string val)
        {
            if (string.IsNullOrEmpty(val)) return false;
            val = val.ToLower().Trim();
            return val == "true" || val == "1" || val == "yes" || val == "on";
        }
    }
}