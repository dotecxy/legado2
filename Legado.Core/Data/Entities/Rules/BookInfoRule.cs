using Newtonsoft.Json;

namespace Legado.Core.Data.Entities.Rules
{
    /// <summary>
    /// 书籍详情页规则（对应 Kotlin 的 BookInfoRule.kt）
    /// </summary>
    public class BookInfoRule
    {
        /// <summary>
        /// 初始化脚本
        /// </summary>
        [JsonProperty("init")]
        public string Init { get; set; }

        /// <summary>
        /// 书名规则
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 作者规则
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; set; }

        /// <summary>
        /// 简介规则
        /// </summary>
        [JsonProperty("intro")]
        public string Intro { get; set; }

        /// <summary>
        /// 分类规则
        /// </summary>
        [JsonProperty("kind")]
        public string Kind { get; set; }

        /// <summary>
        /// 最新章节规则
        /// </summary>
        [JsonProperty("lastChapter")]
        public string LastChapter { get; set; }

        /// <summary>
        /// 更新时间规则
        /// </summary>
        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; }

        /// <summary>
        /// 封面链接规则
        /// </summary>
        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        /// <summary>
        /// 目录链接规则
        /// </summary>
        [JsonProperty("tocUrl")]
        public string TocUrl { get; set; }

        /// <summary>
        /// 字数规则
        /// </summary>
        [JsonProperty("wordCount")]
        public string WordCount { get; set; }

        /// <summary>
        /// 是否可更新规则
        /// </summary>
        [JsonProperty("canReName")]
        public string CanReName { get; set; }

        /// <summary>
        /// 下载链接规则
        /// </summary>
        [JsonProperty("downloadUrls")]
        public string DownloadUrls { get; set; }
    }
}
