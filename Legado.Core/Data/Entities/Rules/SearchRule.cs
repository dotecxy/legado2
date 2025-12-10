using Newtonsoft.Json;

namespace Legado.Core.Data.Entities.Rules
{
    /// <summary>
    /// 搜索结果处理规则（对应 Kotlin 的 SearchRule.kt）
    /// </summary>
    public class SearchRule : IBookListRule
    {
        /// <summary>
        /// 校验关键字
        /// </summary>
        [JsonProperty("checkKeyWord")]
        public string CheckKeyWord { get; set; }

        /// <summary>
        /// 书籍列表选择器
        /// </summary>
        [JsonProperty("bookList")]
        public string BookList { get; set; }

        /// <summary>
        /// 书名选择器
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 作者选择器
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; set; }

        /// <summary>
        /// 简介选择器
        /// </summary>
        [JsonProperty("intro")]
        public string Intro { get; set; }

        /// <summary>
        /// 分类选择器
        /// </summary>
        [JsonProperty("kind")]
        public string Kind { get; set; }

        /// <summary>
        /// 最新章节选择器
        /// </summary>
        [JsonProperty("lastChapter")]
        public string LastChapter { get; set; }

        /// <summary>
        /// 更新时间选择器
        /// </summary>
        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; }

        /// <summary>
        /// 书籍链接选择器
        /// </summary>
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; }

        /// <summary>
        /// 封面链接选择器
        /// </summary>
        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        /// <summary>
        /// 字数选择器
        /// </summary>
        [JsonProperty("wordCount")]
        public string WordCount { get; set; }
    }
}
