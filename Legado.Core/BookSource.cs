using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Legado.Core
{
    /// <summary>
    /// 书源模型
    /// </summary>
    public class BookSource
    {
        /// <summary>
        /// 书源名称
        /// </summary>
        public string bookSourceName { get; set; }

        /// <summary>
        /// 书源类型：0-网络小说
        /// </summary>
        public int bookSourceType { get; set; }

        /// <summary>
        /// 书源网址
        /// </summary>
        public string bookSourceUrl { get; set; }

        /// <summary>
        /// 自定义排序
        /// </summary>
        public int customOrder { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool enabled { get; set; }

        /// <summary>
        /// 是否启用Cookie
        /// </summary>
        public bool enabledCookieJar { get; set; }

        /// <summary>
        /// 是否启用发现
        /// </summary>
        public bool enabledExplore { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public long lastUpdateTime { get; set; }

        /// <summary>
        /// 响应时间
        /// </summary>
        public long respondTime { get; set; }

        /// <summary>
        /// 搜索规则
        /// </summary>
        public RuleSearch ruleSearch { get; set; }

        /// <summary>
        /// 书籍信息规则
        /// </summary>
        public RuleBookInfo ruleBookInfo { get; set; }

        /// <summary>
        /// 目录规则
        /// </summary>
        public RuleToc ruleToc { get; set; }

        /// <summary>
        /// 章节内容规则
        /// </summary>
        public RuleContent ruleContent { get; set; }

        /// <summary>
        /// 搜索URL
        /// </summary>
        public string searchUrl { get; set; }

        /// <summary>
        /// 权重
        /// </summary>
        public int weight { get; set; }
    }

    /// <summary>
    /// 搜索规则
    /// </summary>
    public class RuleSearch
    {
        /// <summary>
        /// 书籍列表选择器
        /// </summary>
        public string bookList { get; set; }

        /// <summary>
        /// 书籍名称选择器
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 作者选择器
        /// </summary>
        public string author { get; set; }

        /// <summary>
        /// 分类选择器
        /// </summary>
        public string kind { get; set; }

        /// <summary>
        /// 最新章节选择器
        /// </summary>
        public string lastChapter { get; set; }

        /// <summary>
        /// 书籍链接选择器
        /// </summary>
        public string bookUrl { get; set; }

        /// <summary>
        /// 字数选择器
        /// </summary>
        public string wordCount { get; set; }
    }

    /// <summary>
    /// 书籍信息规则
    /// </summary>
    public class RuleBookInfo
    {
        /// <summary>
        /// 书籍名称选择器
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 作者选择器
        /// </summary>
        public string author { get; set; }

        /// <summary>
        /// 分类选择器
        /// </summary>
        public string kind { get; set; }

        /// <summary>
        /// 封面URL选择器
        /// </summary>
        public string coverUrl { get; set; }

        /// <summary>
        /// 简介选择器
        /// </summary>
        public string intro { get; set; }

        /// <summary>
        /// 最新章节选择器
        /// </summary>
        public string lastChapter { get; set; }

        /// <summary>
        /// 目录URL
        /// </summary>
        public string tocUrl { get; set; }
    }

    /// <summary>
    /// 目录规则
    /// </summary>
    public class RuleToc
    {
        /// <summary>
        /// 章节列表选择器
        /// </summary>
        public string chapterList { get; set; }

        /// <summary>
        /// 章节名称选择器
        /// </summary>
        public string chapterName { get; set; }

        /// <summary>
        /// 章节链接选择器
        /// </summary>
        public string chapterUrl { get; set; }

        /// <summary>
        /// 下一页目录URL选择器
        /// </summary>
        public string nextTocUrl { get; set; }
    }

    /// <summary>
    /// 章节内容规则
    /// </summary>
    public class RuleContent
    {
        /// <summary>
        /// 内容选择器
        /// </summary>
        public string content { get; set; }

        /// <summary>
        /// 下一章内容URL选择器
        /// </summary>
        public string nextContentUrl { get; set; }

        /// <summary>
        /// 替换正则表达式
        /// </summary>
        public string replaceRegex { get; set; }
    }
}
