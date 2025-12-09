using Legado.Core.Data.Entities.Rules;
using Newtonsoft.Json;
using SQLite; // 对应 sqlite-net-pcl

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书源实体 (对应 BookSource.kt)
    /// </summary>
    [Table("book_sources")]
    public class BookSource : BaseSource
    {
        // 搜索 URL 规则
        [Column("searchUrl")]
        [JsonProperty("searchUrl")]
        public string SearchUrl { get; set; }

        // 发现/分类 URL 规则
        [Column("exploreUrl")]
        [JsonProperty("exploreUrl")]
        public string ExploreUrl { get; set; }

        // 权重
        [Column("weight")]
        [JsonProperty("weight")]
        public int Weight { get; set; } = 0;

        // **************** 核心解析规则 ****************

        // 搜索结果解析规则
        [Column("ruleSearch")]
        [JsonProperty("ruleSearch")]
        public RuleSearch RuleSearch { get; set; }

        // 发现结果解析规则
        [Column("ruleExplore")]
        [JsonProperty("ruleExplore")]
        public ExploreRule RuleExplore { get; set; }

        // 书籍详情页规则
        [Column("ruleBookInfo")]
        [JsonProperty("ruleBookInfo")]
        public RuleBookInfo RuleBookInfo { get; set; }

        // 目录列表规则
        [Column("ruleToc")]
        [JsonProperty("ruleToc")]
        public TocRule RuleToc { get; set; }

        // 正文解析规则
        [Column("ruleContent")]
        [JsonProperty("ruleContent")]
        public RuleContent RuleContent { get; set; }

        // 评论解析规则
        [Column("ruleReview")]
        [JsonProperty("ruleReview")]
        public ReviewRule ruleReview { get; set; }
    } 

    /// <summary>
    /// 书籍详情页规则
    /// </summary>
    public class RuleBookInfo
    {
        [JsonProperty("init")]
        public string Init { get; set; } // 预处理脚本

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("intro")]
        public string Intro { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("lastChapter")]
        public string LastChapter { get; set; }

        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        [JsonProperty("tocUrl")]
        public string TocUrl { get; set; } // 目录页URL

        [JsonProperty("wordCount")]
        public string WordCount { get; set; }

        [JsonProperty("canReName")]
        public string CanReName { get; set; }

        [JsonProperty("downloadUrls")]
        public string DownloadUrls { get; set; }
    }


    /// <summary>
    /// 搜索与发现规则
    /// </summary>
    public class RuleSearch : IBookListRule
    {
        [JsonProperty("checkKeyWord")]
        public string CheckKeyWord { get; set; }

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

        [JsonProperty("lastChapter")]
        public string LastChapter { get; set; }

        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; }

        [JsonProperty("wordCount")]
        public string WordCount { get; set; }
    }

      

    /// <summary>
    /// 正文规则
    /// </summary>
    public class RuleContent
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("nextContentUrl")]
        public string NextContentUrl { get; set; }

        [JsonProperty("webJs")]
        public string WebJs { get; set; } // 网页加载完成后执行的JS（WebView用）

        [JsonProperty("sourceRegex")]
        public string SourceRegex { get; set; } // 资源正则

        [JsonProperty("replaceRegex")]
        public string ReplaceRegex { get; set; } // 净化正则

        [JsonProperty("imageStyle")]
        public string ImageStyle { get; set; } // 图片样式
    }
}