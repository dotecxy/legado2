using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Legado.Core.Helps.Books
{
    /// <summary>
    /// 内容处理器（对应 ContentProcessor.kt）
    /// 处理书籍内容的替换规则和格式化
    /// </summary>
    public class ContentProcessor
    {
        private readonly Book _book;
        private List<ReplaceRule> _titleReplaceRules;
        private List<ReplaceRule> _contentReplaceRules;

        private ContentProcessor(Book book)
        {
            _book = book ?? throw new ArgumentNullException(nameof(book));
        }

        /// <summary>
        /// 获取内容处理器实例
        /// </summary>
        public static ContentProcessor Get(Book book)
        {
            return new ContentProcessor(book);
        }

        /// <summary>
        /// 获取标题替换规则
        /// </summary>
        public List<ReplaceRule> GetTitleReplaceRules()
        {
            if (_titleReplaceRules == null)
            {
                _titleReplaceRules = LoadReplaceRules(true);
            }
            return _titleReplaceRules;
        }

        /// <summary>
        /// 获取内容替换规则
        /// </summary>
        public List<ReplaceRule> GetContentReplaceRules()
        {
            if (_contentReplaceRules == null)
            {
                _contentReplaceRules = LoadReplaceRules(false);
            }
            return _contentReplaceRules;
        }

        /// <summary>
        /// 加载替换规则
        /// </summary>
        private List<ReplaceRule> LoadReplaceRules(bool forTitle)
        {
            // TODO: 从数据库加载替换规则
            // 这里返回空列表作为占位
            return new List<ReplaceRule>();
        }

        /// <summary>
        /// 处理章节内容
        /// </summary>
        public BookContent ProcessContent(string content, string chapterName)
        {
            if (string.IsNullOrEmpty(content))
            {
                return new BookContent(false, new List<string>());
            }

            var contentRules = GetContentReplaceRules();
            var effectiveRules = new List<ReplaceRule>();

            // 应用替换规则
            var processedContent = content;
            foreach (var rule in contentRules)
            {
                if (rule.IsEnabled && rule.ScopeContent)
                {
                    try
                    {
                        if (rule.IsRegex)
                        {
                            var regex = new Regex(rule.Pattern, RegexOptions.None, 
                                TimeSpan.FromMilliseconds(rule.TimeoutMillisecond));
                            var replaced = regex.Replace(processedContent, rule.Replacement ?? string.Empty);
                            
                            if (replaced != processedContent)
                            {
                                processedContent = replaced;
                                effectiveRules.Add(rule);
                            }
                        }
                        else
                        {
                            var replaced = processedContent.Replace(rule.Pattern, rule.Replacement ?? string.Empty);
                            if (replaced != processedContent)
                            {
                                processedContent = replaced;
                                effectiveRules.Add(rule);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // 忽略替换错误
                    }
                }
            }

            // 分割成段落
            var textList = processedContent
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            // 检查是否移除了相同标题
            bool sameTitleRemoved = false;
            if (textList.Count > 0 && !string.IsNullOrWhiteSpace(chapterName))
            {
                var firstLine = textList[0];
                if (firstLine.Contains(chapterName) || chapterName.Contains(firstLine))
                {
                    textList.RemoveAt(0);
                    sameTitleRemoved = true;
                }
            }

            return new BookContent(sameTitleRemoved, textList, effectiveRules);
        }

        /// <summary>
        /// 处理章节标题
        /// </summary>
        public string ProcessTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return title;

            var titleRules = GetTitleReplaceRules();
            var processedTitle = title;

            foreach (var rule in titleRules)
            {
                if (rule.IsEnabled && rule.ScopeTitle)
                {
                    try
                    {
                        if (rule.IsRegex)
                        {
                            var regex = new Regex(rule.Pattern, RegexOptions.None,
                                TimeSpan.FromMilliseconds(rule.TimeoutMillisecond));
                            processedTitle = regex.Replace(processedTitle, rule.Replacement ?? string.Empty);
                        }
                        else
                        {
                            processedTitle = processedTitle.Replace(rule.Pattern, rule.Replacement ?? string.Empty);
                        }
                    }
                    catch (Exception)
                    {
                        // 忽略替换错误
                    }
                }
            }

            return processedTitle.Trim();
        }
    }
}
