using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Legado.Core.Data.Entities.Rules
{
    /// <summary>
    /// 发现结果规则（对应 Kotlin 的 ExploreRule.kt）
    /// </summary>
    public class ExploreRule : IBookListRule
    {
        /// <summary>
        /// 书籍列表选择器
        /// </summary>
        [JsonPropertyName("bookList")]
        public string BookList { get; set; } = "";

        /// <summary>
        /// 书名选择器
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// 作者选择器
        /// </summary>
        [JsonPropertyName("author")]
        public string Author { get; set; } = "";

        /// <summary>
        /// 简介选择器
        /// </summary>
        [JsonPropertyName("intro")]
        public string Intro { get; set; } = "";

        /// <summary>
        /// 分类选择器
        /// </summary>
        [JsonPropertyName("kind")]
        public string Kind { get; set; } = "";

        /// <summary>
        /// 最新章节选择器
        /// </summary>
        [JsonPropertyName("lastChapter")]
        public string LastChapter { get; set; } = "";

        /// <summary>
        /// 更新时间选择器
        /// </summary>
        [JsonPropertyName("updateTime")]
        public string UpdateTime { get; set; } = "";

        /// <summary>
        /// 书籍链接选择器
        /// </summary>
        [JsonPropertyName("bookUrl")]
        public string BookUrl { get; set; } = "";

        /// <summary>
        /// 封面链接选择器
        /// </summary>
        [JsonPropertyName("coverUrl")]
        public string CoverUrl { get; set; } = "";

        /// <summary>
        /// 字数选择器
        /// </summary>
        [JsonPropertyName("wordCount")]
        public string WordCount { get; set; } = "";
    }

}
