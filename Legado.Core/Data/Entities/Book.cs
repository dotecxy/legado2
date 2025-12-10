using Legado.Core.Models;
using Newtonsoft.Json;
using SQLite; // 对应 sqlite-net-pcl

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书籍实体 (对应 Book.kt)
    /// </summary>
    [Table("books")]
    public class Book : IBaseBook
    {
        // 书籍 URL，作为主键
        [PrimaryKey, Column("bookUrl")]
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; }

        // 目录 URL (有时与 BookUrl 不同)
        [Column("tocUrl")]
        [JsonProperty("tocUrl")]
        public string TocUrl { get; set; }

        // 书源 URL (外键关联 BookSource)
        [Column("origin")]
        [JsonProperty("origin")]
        public string Origin { get; set; }

        // 原书源名称 (备份用)
        [Column("originName")]
        [JsonProperty("originName")]
        public string OriginName { get; set; }

        [Column("name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Column("author")]
        [JsonProperty("author")]
        public string Author { get; set; }

        // 分类 (玄幻, 都市...)
        [Column("kind")]
        [JsonProperty("kind")]
        public string Kind { get; set; }

        // 自定义标签
        [Column("customTag")]
        [JsonProperty("customTag")]
        public string CustomTag { get; set; }

        [Column("coverUrl")]
        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        [Column("intro")]
        [JsonProperty("intro")]
        public string Intro { get; set; }

        // 字数
        [Column("wordCount")]
        [JsonProperty("wordCount")]
        public string WordCount { get; set; }

        // 最新章节标题
        [Column("latestChapterTitle")]
        [JsonProperty("latestChapterTitle")]
        public string LatestChapterTitle { get; set; }

        // 最新章节时间
        [Column("latestChapterTime")]
        [JsonProperty("latestChapterTime")]
        public long LatestChapterTime { get; set; }

        // 当前阅读进度 (章节索引)
        [Column("durChapterIndex")]
        [JsonProperty("durChapterIndex")]
        public int DurChapterIndex { get; set; } = 0;

        // 当前阅读章节标题
        [Column("durChapterTitle")]
        [JsonProperty("durChapterTitle")]
        public string DurChapterTitle { get; set; }

        // 当前阅读位置 (滚动位置/页码)
        [Column("durChapterPos")]
        [JsonProperty("durChapterPos")]
        public int DurChapterPos { get; set; } = 0;

        // 上次阅读时间
        [Column("durChapterTime")]
        [JsonProperty("durChapterTime")]
        public long DurChapterTime { get; set; } = 0;

        // 总章节数
        [Column("totalChapterNum")]
        [JsonProperty("totalChapterNum")]
        public int TotalChapterNum { get; set; } = 0;

        // 排序值
        [Column("order")]
        [JsonProperty("order")]
        public int Order { get; set; } = 0;

        // 变量集合 (用于存储 JS 执行过程中的临时变量，JSON格式)
        [Column("variable")]
        [JsonProperty("variable")]
        public string Variable { get; set; }

        // **************** 状态位 ****************

        // 是否可以更新
        [Column("canUpdate")]
        [JsonProperty("canUpdate")]
        public bool CanUpdate { get; set; } = true;

        // 0: text, 1: audio
        [Column("type")]
        [JsonProperty("type")]
        public int Type { get; set; } = 0;

        // 分组 (0: 未分组)
        [Column("group")]
        [JsonProperty("group")]
        public long Group { get; set; } = 0;

        // 书源排序
        [Column("originOrder")]
        [JsonProperty("originOrder")]
        public int OriginOrder { get; set; } = 0;

        // IBaseBook 接口实现
        [Ignore]
        [JsonIgnore]
        public string InfoHtml { get; set; }

        [Ignore]
        [JsonIgnore]
        public string TocHtml { get; set; }

        // IRuleData 接口实现
        public virtual bool putVariable(string key, string value)
        {
            // TODO: 实现变量存储
            return true;
        }

        public string getVariable()
        {
            return Variable;
        }

        public string getVariable(string key)
        {
            // TODO: 实现变量读取
            return "";
        }

        public void putBigVariable(string key, string value)
        {
            // TODO: 实现大变量存储
        }

        public string getBigVariable(string key)
        {
            // TODO: 实现大变量读取
            return null;
        }
    }
}