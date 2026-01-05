using FreeSql.DataAnnotations;
using Legado.Core.Extensions;
using Legado.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书籍实体 (对应 Kotlin 的 Book.kt)
    /// </summary>
    [Table(Name = "books")]
    [Index("name", "author", IsUnique = true)] // 对应 Kotlin 的 indices = [Index(value = ["name", "author"], unique = true)]
    public class Book : IBaseBook
    {
        /// <summary>
        /// 书籍URL，作为主键
        /// </summary>
        [Column(IsPrimary = true, StringLength = 255)]
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; } = "";

        /// <summary>
        /// 目录URL（有时与BookUrl不同）
        /// </summary>
        [Column(StringLength = 255)]
        [JsonProperty("tocUrl")]
        public string TocUrl { get; set; } = "";

        /// <summary>
        /// 书源URL（外键关联BookSource）
        /// </summary>
        [Column(StringLength = 255)] // 对应 BookType.localTag
        [JsonProperty("origin")]
        public string Origin { get; set; } = "loc://local";

        /// <summary>
        /// 原书源名称（备份用）
        /// </summary>
        [Column(StringLength = 255)]
        [JsonProperty("originName")]
        public string OriginName { get; set; } = "";

        /// <summary>
        /// 书名
        /// </summary>
        [Column(StringLength = 255)]
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// 作者
        /// </summary>
        [Column(StringLength = 255)]
        [JsonProperty("author")]
        public string Author { get; set; } = "";

        /// <summary>
        /// 分类（玄幻、都市...）
        /// </summary>
        [JsonProperty("kind")]
        public string Kind { get; set; }

        /// <summary>
        /// 自定义标签
        /// </summary>
        [JsonProperty("customTag")]
        public string CustomTag { get; set; }

        /// <summary>
        /// 封面URL
        /// </summary>
        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        /// <summary>
        /// 自定义封面URL
        /// </summary>
        [JsonProperty("customCoverUrl")]
        public string CustomCoverUrl { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        [JsonProperty("intro")]
        public string Intro { get; set; }

        /// <summary>
        /// 自定义简介
        /// </summary>
        [JsonProperty("customIntro")]
        public string CustomIntro { get; set; }

        /// <summary>
        /// 字符集（仅适用于本地书籍）
        /// </summary>
        [JsonProperty("charset")]
        public string Charset { get; set; }

        /// <summary>
        /// 类型（0: text, 1: audio）
        /// </summary>
        [Column()]
        [JsonProperty("type")]
        public int Type { get; set; } = 0;

        /// <summary>
        /// 分组（0: 未分组）
        /// </summary>
        [Column()]
        [JsonProperty("group")]
        public long Group { get; set; } = 0;

        /// <summary>
        /// 最新章节标题
        /// </summary>
        [JsonProperty("latestChapterTitle")]
        public string LatestChapterTitle { get; set; }

        /// <summary>
        /// 最新章节时间
        /// </summary>
        [Column()]
        [JsonProperty("latestChapterTime")]
        public long LatestChapterTime { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 最近一次更新书籍信息的时间
        /// </summary>
        [Column()]
        [JsonProperty("lastCheckTime")]
        public long LastCheckTime { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 最近一次发现新章节的数量
        /// </summary>
        [Column()]
        [JsonProperty("lastCheckCount")]
        public int LastCheckCount { get; set; } = 0;

        /// <summary>
        /// 总章节数
        /// </summary>
        [Column()]
        [JsonProperty("totalChapterNum")]
        public int TotalChapterNum { get; set; } = 0;

        /// <summary>
        /// 当前阅读章节标题
        /// </summary>
        [JsonProperty("durChapterTitle")]
        public string DurChapterTitle { get; set; }

        /// <summary>
        /// 当前阅读进度（章节索引）
        /// </summary>
        [Column()]
        [JsonProperty("durChapterIndex")]
        public int DurChapterIndex { get; set; } = 0;

        /// <summary>
        /// 当前阅读位置（首行字符的索引位置）
        /// </summary>
        [Column()]
        [JsonProperty("durChapterPos")]
        public int DurChapterPos { get; set; } = 0;

        /// <summary>
        /// 上次阅读时间
        /// </summary>
        [Column()]
        [JsonProperty("durChapterTime")]
        public long DurChapterTime { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 字数
        /// </summary>
        [JsonProperty("wordCount")]
        public string WordCount { get; set; }

        /// <summary>
        /// 是否可以更新
        /// </summary>
        [Column()]
        [JsonProperty("canUpdate")]
        public bool CanUpdate { get; set; } = true;

        /// <summary>
        /// 手动排序
        /// </summary>
        [Column()]
        [JsonProperty("order")]
        public int Order { get; set; } = 0;

        /// <summary>
        /// 书源排序
        /// </summary>
        [Column()]
        [JsonProperty("originOrder")]
        public int OriginOrder { get; set; } = 0;

        /// <summary>
        /// 变量集合（用于存储JS执行过程中的临时变量，JSON格式）
        /// </summary>
        [JsonProperty("variable")]
        public string Variable { get; set; }

        /// <summary>
        /// 阅读设置
        /// </summary>
        [JsonProperty("readConfig")]
        public string ReadConfig { get; set; }

        /// <summary>
        /// 同步时间
        /// </summary>
        [Column()]
        [JsonProperty("syncTime")]
        public long SyncTime { get; set; } = 0;

        /// <summary>
        /// 书籍信息HTML
        /// </summary>
        [Column(IsIgnore = true)]
        [JsonIgnore]
        public string InfoHtml { get; set; }

        /// <summary>
        /// 目录HTML
        /// </summary>
        [Column(IsIgnore = true)]
        [JsonIgnore]
        public string TocHtml { get; set; }

        /// <summary>
        /// 下载链接列表（用于WebFile类型书籍）
        /// </summary>
        [Column(IsIgnore = true)]
        [JsonIgnore]
        public List<string> DownloadUrls { get; set; }

        /// <summary>
        /// 存储变量
        /// </summary>
        public virtual bool PutVariable(string key, string value)
        {
            // TODO: 实现变量存储
            return true;
        }

        public string GetVariable()
        {
            return Variable;
        }

        public string GetVariable(string key)
        {
            // TODO: 实现变量读取
            return "";
        }

        public void PutBigVariable(string key, string value)
        {
            // TODO: 实现大变量存储
        }

        public string GetBigVariable(string key)
        {
            // TODO: 实现大变量读取
            return null;
        }

        public bool SetCurrentChapter(BookChapter chapter, BookChapter lastestChapter,int chapterCount, int chapterIndex, long pos)
        {
            if (chapter == null) return false;

            this.DurChapterIndex = chapterIndex;
            this.DurChapterPos = (int)pos; // 修正类型不匹配
            this.DurChapterTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            this.LatestChapterTitle = lastestChapter?.Title ?? "";
            this.LatestChapterTime = lastestChapter?.Index ?? 0;
            this.TotalChapterNum = chapterCount;
            this.DurChapterTitle = chapter.Title;

            return true;
        }
    }
}