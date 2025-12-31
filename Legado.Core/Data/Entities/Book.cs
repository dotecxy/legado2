using Legado.Core.Extensions;
using Legado.Core.Models;
using Newtonsoft.Json;
using Legado.FreeSql;
using System;
using System.Collections.Generic; // 对应 sqlite-net-pcl

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书籍实体 (对应 Book.kt)
    /// </summary>
    [Table("books")]
    public class Book : IBaseBook
    {
        /// <summary>
        /// 书籍URL，作为主键
        /// </summary>
        [PrimaryKey, Column("book_url")]
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; }

        /// <summary>
        /// 目录URL（有时与BookUrl不同）
        /// </summary>
        [Column("toc_url")]
        [JsonProperty("tocUrl")]
        public string TocUrl { get; set; }

        /// <summary>
        /// 书源URL（外键关联BookSource）
        /// </summary>
        [Column("origin")]
        [JsonProperty("origin")]
        public string Origin { get; set; }

        /// <summary>
        /// 原书源名称（备份用）
        /// </summary>
        [Column("origin_name")]
        [JsonProperty("originName")]
        public string OriginName { get; set; }

        /// <summary>
        /// 书名
        /// </summary>
        [Column("name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        [Column("author")]
        [JsonProperty("author")]
        public string Author { get; set; }

        /// <summary>
        /// 分类（玄幻、都市...）
        /// </summary>
        [Column("kind")]
        [JsonProperty("kind")]
        public string Kind { get; set; }

        /// <summary>
        /// 自定义标签
        /// </summary>
        [Column("custom_tag")]
        [JsonProperty("customTag")]
        public string CustomTag { get; set; }

        /// <summary>
        /// 封面URL
        /// </summary>
        [Column("cover_url")]
        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        [Column("intro")]
        [JsonProperty("intro")]
        public string Intro { get; set; }

        /// <summary>
        /// 字数
        /// </summary>
        [Column("word_count")]
        [JsonProperty("wordCount")]
        public string WordCount { get; set; }

        /// <summary>
        /// 最新章节标题
        /// </summary>
        [Column("latest_chapter_title")]
        [JsonProperty("latestChapterTitle")]
        public string LatestChapterTitle { get; set; }

        /// <summary>
        /// 最新章节时间
        /// </summary>
        [Column("latest_chapter_time")]
        [JsonProperty("latestChapterTime")]
        public long LatestChapterTime { get; set; }

        /// <summary>
        /// 当前阅读进度（章节索引）
        /// </summary>
        [Column("dur_chapter_index")]
        [JsonProperty("durChapterIndex")]
        public int DurChapterIndex { get; set; } = 0;

        /// <summary>
        /// 当前阅读章节标题
        /// </summary>
        [Column("dur_chapter_title")]
        [JsonProperty("durChapterTitle")]
        public string DurChapterTitle { get; set; }

        /// <summary>
        /// 当前阅读位置（滚动位置/页码）
        /// </summary>
        [Column("dur_chapter_pos")]
        [JsonProperty("durChapterPos")]
        public long DurChapterPos { get; set; } = 0;

        /// <summary>
        /// 上次阅读时间
        /// </summary>
        [Column("dur_chapter_time")]
        [JsonProperty("durChapterTime")]
        public long DurChapterTime { get; set; } = 0;

        /// <summary>
        /// 总章节数
        /// </summary>
        [Column("total_chapter_num")]
        [JsonProperty("totalChapterNum")]
        public int TotalChapterNum { get; set; } = 0;

        /// <summary>
        /// 排序值
        /// </summary>
        [Column("order")]
        [JsonProperty("order")]
        public int Order { get; set; } = 0;

        /// <summary>
        /// 变量集合（用于存储JS执行过程中的临时变量，JSON格式）
        /// </summary>
        [Column("variable")]
        [JsonProperty("variable")]
        public string Variable { get; set; }

        /// <summary>
        /// 是否可以更新
        /// </summary>
        [Column("can_update")]
        [JsonProperty("canUpdate")]
        public bool CanUpdate { get; set; } = true;

        /// <summary>
        /// 类型（0: text, 1: audio）
        /// </summary>
        [Column("type")]
        [JsonProperty("type")]
        public int Type { get; set; } = 0;

        /// <summary>
        /// 分组（0: 未分组）
        /// </summary>
        [Column("group")]
        [JsonProperty("group")]
        public long Group { get; set; } = 0;

        /// <summary>
        /// 书源排序
        /// </summary>
        [Column("origin_order")]
        [JsonProperty("originOrder")]
        public int OriginOrder { get; set; } = 0;

        /// <summary>
        /// 书籍信息HTML
        /// </summary>
        [Ignore]
        [JsonIgnore]
        public string InfoHtml { get; set; }

        /// <summary>
        /// 目录HTML
        /// </summary>
        [Ignore]
        [JsonIgnore]
        public string TocHtml { get; set; }

        /// <summary>
        /// 下载链接列表（用于WebFile类型书籍）
        /// </summary>
        [Ignore]
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

        public bool SetCurrentChapter(BookChapter chapter, BookChapter lastestChapter, int chapterIndex, long pos)
        {
            if (chapter == null) return false;

            this.DurChapterIndex = chapterIndex;
            this.DurChapterPos = pos;
            this.DurChapterTime = DateTime.Now.ToTimeStamp();
            this.LatestChapterTitle = lastestChapter?.Title ?? "";
            this.LatestChapterTime = lastestChapter?.Index ?? 0;
            this.TotalChapterNum = chapter.TotalChapterNum;
            this.DurChapterTitle = chapter.Title;

            return true;
        }
    }
}