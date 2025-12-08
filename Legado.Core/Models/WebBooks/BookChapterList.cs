using Legado.Core.Data.Entities;
using Legado.Core.Models.AnalyzeRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 章节列表解析类
    /// </summary>
    public static class BookChapterList
    {
        /// <summary>
        /// 解析章节列表
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="book">书籍</param>
        /// <param name="baseUrl">基础URL</param>
        /// <param name="redirectUrl">重定向URL</param>
        /// <param name="body">HTML内容</param>
        /// <returns>章节列表</returns>
        public static List<BookChapter> AnalyzeChapterList(
            BookSource bookSource,
            Book book,
            string baseUrl,
            string redirectUrl,
            string body)
        {
            var chapterList = new List<BookChapter>();
            var rule = bookSource.GetTocRule();
            
            // 保存原始数据用于后续解析
            if (book.TocUrl == redirectUrl)
            {
                book.TocHtml = body;
            }
            
            // 解析章节列表
            var analyzeRule = new AnalyzeRule(null, body, true);
            analyzeRule.SetContent(body, baseUrl);
            
            // 解析章节列表
            var list = analyzeRule.GetElements(rule.ChapterList);
            if (list == null || !list.Any())
            {
                // TODO: 添加日志
                return chapterList;
            }
            
            string chapterUrl = null;
            string volumeUrl = null;
            string volumeTitle = null;
            
            foreach (var item in list)
            {
                var elementAnalyzeRule = new LegadoBookSource.AnalyzeRule.AnalyzeRule(null, item, true);
                elementAnalyzeRule.SetContent(item, baseUrl);
                
                // 解析标题
                var title = elementAnalyzeRule.GetString(rule.ChapterName);
                if (string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }
                
                // 解析URL
                var url = elementAnalyzeRule.GetString(rule.ChapterUrl);
                if (string.IsNullOrWhiteSpace(url))
                {
                    // 如果URL为空，使用上一个URL
                    url = string.IsNullOrWhiteSpace(chapterUrl) ? book.TocUrl : chapterUrl;
                }
                else
                {
                    // 处理相对URL
                    if (!url.StartsWith("http"))
                    {
                        url = new Uri(new Uri(baseUrl), url).ToString();
                    }
                    chapterUrl = url;
                }
                
                // 解析是否为卷
                var isVolume = false;
                if (!string.IsNullOrWhiteSpace(rule.IsVolume))
                {
                    var boolStr = elementAnalyzeRule.GetString(rule.IsVolume);
                    isVolume = bool.TryParse(boolStr, out var result) ? result : false;
                }
                
                if (isVolume)
                {
                    // 处理卷
                    volumeUrl = url;
                    volumeTitle = title;
                    
                    var volumeChapter = new BookChapter
                    {
                        Title = title,
                        Url = $"{title}@",
                        IsVolume = true
                    };
                    
                    chapterList.Add(volumeChapter);
                }
                else
                {
                    // 处理章节
                    var chapter = new BookChapter
                    {
                        Title = title,
                        Url = url,
                        IsVolume = false
                    };
                    
                    chapterList.Add(chapter);
                }
            }
            
            // 更新书籍总章节数
            book.TotalChapterNum = chapterList.Count(chapter => !chapter.IsVolume);
            
            // 执行目录页JS
            if (!string.IsNullOrWhiteSpace(rule.FormatJs))
            {
                analyzeRule.EvalJS(rule.FormatJs);
            }
            
            return chapterList;
        }
    }
}