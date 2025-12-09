using Legado.Core.Data.Entities;
using Legado.Core.Data.Entities.Rules;
using Legado.Core.Models.AnalyzeRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 章节内容解析类 (对应 Kotlin: BookContent.kt)
    /// </summary>
    public static class BookContent
    {
        /// <summary>
        /// 解析章节内容
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="book">书籍</param>
        /// <param name="bookChapter">章节</param>
        /// <param name="baseUrl">基础URL</param>
        /// <param name="redirectUrl">重定向URL</param>
        /// <param name="body">HTML内容</param>
        /// <param name="nextChapterUrl">下一章节URL</param>
        /// <returns>解析后的章节内容</returns>
        public static async Task<string> AnalyzeContent(
            BookSource bookSource,
            Book book,
            BookChapter bookChapter,
            string baseUrl,
            string redirectUrl,
            string body,
            string nextChapterUrl = null)
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new Exception("Error getting web content");
            }

            var contentRule = bookSource.RuleContent;
            if (contentRule == null)
            {
                throw new Exception("Content rule is null");
            }

            // Debug.Log(bookSource.BookSourceUrl, "┌获取正文内容");
            // Debug.Log(bookSource.BookSourceUrl, "└$baseUrl");

            var analyzeRule = new AnalyzeRule(bookChapter, bookSource);
            analyzeRule.SetContent(body);
            analyzeRule.SetBaseUrl(baseUrl);
            analyzeRule.SetRedirectUrl(redirectUrl);
            // analyzeRule.SetNextChapterUrl(nextChapterUrl);

            var contentData = new StringBuilder();
            var nextUrlList = new List<string>();
            var contentList = new List<string>();

            // 解析内容规则
            var content = analyzeRule.GetString(contentRule.Content);
            if (!string.IsNullOrWhiteSpace(content))
            {
                contentList.Add(content);
            }

            // 解析下一页URL
            if (!string.IsNullOrWhiteSpace(contentRule.NextContentUrl))
            {
                var nextUrl = analyzeRule.GetString(contentRule.NextContentUrl, isUrl: true);
                if (!string.IsNullOrWhiteSpace(nextUrl) && nextUrl != baseUrl)
                {
                    nextUrlList.Add(nextUrl);
                }
            }

            // 处理分页内容
            while (nextUrlList.Count > 0 && nextUrlList.Count <= 20) // 限制最大分页数
            {
                var nextUrl = nextUrlList[0];
                nextUrlList.RemoveAt(0);

                // TODO: 加载下一页内容
                // var nextAnalyzeUrl = new AnalyzeUrl(nextUrl, bookSource, bookChapter);
                // var nextRes = await nextAnalyzeUrl.GetStrResponseAwait();
                // analyzeRule.SetContent(nextRes.Body, nextUrl);
                
                // var nextContent = analyzeRule.GetString(contentRule.Content);
                // if (!string.IsNullOrWhiteSpace(nextContent))
                // {
                //     contentList.Add(nextContent);
                // }

                break; // 暂时只处理第一页
            }

            // 拼接内容
            foreach (var str in contentList)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    if (contentData.Length > 0)
                    {
                        contentData.Append("\n");
                    }
                    contentData.Append(str);
                }
            }

            var contentStr = contentData.ToString();

            // 执行内容处理JS
            if (!string.IsNullOrWhiteSpace(contentRule.WebJs))
            {
                try
                {
                    // TODO: 执行 JS 脚本
                    // contentStr = analyzeRule.GetString(contentRule.WebJs) ?? contentStr;
                }
                catch (Exception e)
                {
                    // Debug.Log error
                }
            }

            // 内容净化和格式化
            contentStr = FormatContent(contentStr);

            await Task.CompletedTask;
            return contentStr;
        }

        /// <summary>
        /// 格式化内容
        /// </summary>
        private static string FormatContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return "";
            }

            // 移除空白字符
            content = content.Trim();

            // 规范化段落
            content = Regex.Replace(content, "\r\n", "\n");
            content = Regex.Replace(content, "\r", "\n");
            content = Regex.Replace(content, "\n{3,}", "\n\n"); // 最多保留两个换行

            return content;
        }
    }
}