using Newtonsoft.Json;

namespace Legado.Core.Models
{
    /// <summary>
    /// 搜索规则
    /// </summary>
    public class SearchRule
    {
        [JsonProperty("checkKeyWord")]
        public string CheckKeyWord { get; set; } // 校验关键字

        [JsonProperty("bookList")]
        public string BookList { get; set; } // 书籍列表规则

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("intro")]
        public string Intro { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; } // 分类信息

        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; } // 详情页URL

        [JsonProperty("lastChapter")]
        public string LastChapter { get; set; } // 最新章节

        [JsonProperty("wordCount")]
        public string WordCount { get; set; }
    }

    /// <summary>
    /// 发现/分类规则 (通常包含一个 sourceUrl 列表规则)
    /// </summary>
    public class ExploreRule
    {
        [JsonProperty("bookList")]
        public string BookList { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("intro")]
        public string Intro { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; }

        [JsonProperty("lastChapter")]
        public string LastChapter { get; set; }
    }

    /// <summary>
    /// 书籍详情页规则
    /// </summary>
    public class BookInfoRule
    {
        [JsonProperty("init")]
        public string Init { get; set; } // 详情页预处理(通常是JS或正则)

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("intro")]
        public string Intro { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        [JsonProperty("lastChapter")]
        public string LastChapter { get; set; }

        [JsonProperty("tocUrl")]
        public string TocUrl { get; set; } // 目录页URL

        [JsonProperty("wordCount")]
        public string WordCount { get; set; }

        [JsonProperty("canReName")]
        public string CanReName { get; set; } // 允许重命名规则
    }

    /// <summary>
    /// 目录页规则
    /// </summary>
    public class TocRule
    {
        [JsonProperty("chapterList")]
        public string ChapterList { get; set; }

        [JsonProperty("chapterName")]
        public string ChapterName { get; set; }

        [JsonProperty("chapterUrl")]
        public string ChapterUrl { get; set; }

        [JsonProperty("isVip")]
        public string IsVip { get; set; } // VIP标识

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; } // 更新时间

        [JsonProperty("nextTocUrl")]
        public string NextTocUrl { get; set; } // 下一页目录
    }

    /// <summary>
    /// 正文页规则
    /// </summary>
    public class ContentRule
    {
        [JsonProperty("content")]
        public string Content { get; set; } // 正文内容

        [JsonProperty("nextContentUrl")]
        public string NextContentUrl { get; set; } // 下一页正文

        [JsonProperty("webJs")]
        public string WebJs { get; set; } // 加载WebView时执行的JS

        [JsonProperty("sourceRegex")]
        public string SourceRegex { get; set; } // 资源正则

        [JsonProperty("replaceRegex")]
        public string ReplaceRegex { get; set; } // 替换/净化正则 ##...##...

        [JsonProperty("imageStyle")]
        public string ImageStyle { get; set; } // 图片样式 full/TEXT 等
    }
}