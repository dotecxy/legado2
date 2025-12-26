using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 搜索书籍结果（对应 SearchBook.kt）
    /// </summary>
    [Table("search_books")]
    public class SearchBook : IBaseBook, IComparable<SearchBook>
    {
        /// <summary>
        /// 书籍URL（主键）
        /// </summary>
        [PrimaryKey]
        [Column("book_url")]
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; } = "";

        /// <summary>
        /// 书源
        /// </summary>
        [Column("origin")]
        [JsonProperty("origin")]
        public string Origin { get; set; } = "";

        /// <summary>
        /// 书源名称
        /// </summary>
        [Column("origin_name")]
        [JsonProperty("originName")]
        public string OriginName { get; set; } = "";

        /// <summary>
        /// 书籍类型（BookType）
        /// </summary>
        [Column("type")]
        [JsonProperty("type")]
        public int Type { get; set; } = 0;

        /// <summary>
        /// 书名
        /// </summary>
        [Column("name")]
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// 作者
        /// </summary>
        [Column("author")]
        [JsonProperty("author")]
        public string Author { get; set; } = "";

        /// <summary>
        /// 分类
        /// </summary>
        [Column("kind")]
        [JsonProperty("kind")]
        public string Kind { get; set; }

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
        /// 目录页URL
        /// </summary>
        [Column("toc_url")]
        [JsonProperty("tocUrl")]
        public string TocUrl { get; set; } = "";

        /// <summary>
        /// 时间戳
        /// </summary>
        [Column("time")]
        [JsonProperty("time")]
        public long Time { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 变量
        /// </summary>
        [Column("variable")]
        [JsonProperty("variable")]
        public string Variable { get; set; }

        /// <summary>
        /// 书源排序
        /// </summary>
        [Column("origin_order")]
        [JsonProperty("originOrder")]
        public int OriginOrder { get; set; } = 0;

        /// <summary>
        /// 章节字数文本
        /// </summary>
        [Column("chapter_word_count_text")]
        [JsonProperty("chapterWordCountText")]
        public string ChapterWordCountText { get; set; }

        /// <summary>
        /// 章节字数
        /// </summary>
        [Column("chapter_word_count")]
        [JsonProperty("chapterWordCount")]
        public int ChapterWordCount { get; set; } = -1;

        /// <summary>
        /// 响应时间
        /// </summary>
        [Column("respond_time")]
        [JsonProperty("respondTime")]
        public int RespondTime { get; set; } = -1;

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
        /// 变量映射表
        /// </summary>
        [Ignore]
        [JsonIgnore]
        public Dictionary<string, string> VariableMap
        {
            get
            {
                if (_variableMap == null)
                {
                    if (string.IsNullOrEmpty(Variable))
                    {
                        _variableMap = new Dictionary<string, string>();
                    }
                    else
                    {
                        try
                        {
                            _variableMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(Variable)
                                ?? new Dictionary<string, string>();
                        }
                        catch
                        {
                            _variableMap = new Dictionary<string, string>();
                        }
                    }
                }
                return _variableMap;
            }
        }

        private Dictionary<string, string> _variableMap;

        /// <summary>
        /// 来源集合
        /// </summary>
        [Ignore]
        [JsonIgnore]
        public HashSet<string> Origins { get; } = new HashSet<string>();

        public override bool Equals(object obj)
        {
            return obj is SearchBook other && other.BookUrl == BookUrl;
        }

        public override int GetHashCode()
        {
            return BookUrl.GetHashCode();
        }

        /// <summary>
        /// 比较排序（对应 Kotlin 的 compareTo）
        /// </summary>
        public int CompareTo(SearchBook other)
        {
            return other.OriginOrder - this.OriginOrder;
        }

        /// <summary>
        /// 添加书源（对应 Kotlin 的 addOrigin）
        /// </summary>
        public void AddOrigin(string origin)
        {
            Origins.Add(origin);
        }

        /// <summary>
        /// 获取显示的最新章节标题（对应 Kotlin 的 getDisplayLastChapterTitle）
        /// </summary>
        public string GetDisplayLastChapterTitle()
        {
            if (!string.IsNullOrEmpty(LatestChapterTitle))
            {
                return LatestChapterTitle;
            }
            return "无最新章节";
        }

        /// <summary>
        /// 释放HTML数据（对应 Kotlin 的 releaseHtmlData）
        /// </summary>
        public void ReleaseHtmlData()
        {
            InfoHtml = null;
            TocHtml = null;
        }

        /// <summary>
        /// 主键字符串（对应 Kotlin 的 primaryStr）
        /// </summary>
        public string PrimaryStr()
        {
            return Origin + BookUrl;
        }

        /// <summary>
        /// 转换为Book对象（对应 Kotlin 的 toBook）
        /// </summary>
        public Book ToBook()
        {
            var book = new Book
            {
                Name = Name,
                Author = Author,
                Kind = Kind,
                BookUrl = BookUrl,
                Origin = Origin,
                OriginName = OriginName,
                Type = Type,
                WordCount = WordCount,
                LatestChapterTitle = LatestChapterTitle,
                CoverUrl = CoverUrl,
                Intro = Intro,
                TocUrl = TocUrl,
                OriginOrder = OriginOrder,
                Variable = Variable, 
            };
            book.InfoHtml = this.InfoHtml;
            book.TocHtml = this.TocHtml;
            return book;
        }

        // IRuleData 接口实现
        public bool PutVariable(string key, string value)
        {
            VariableMap[key] = value;
            Variable = JsonConvert.SerializeObject(VariableMap);
            return true;
        }

        public string GetVariable()
        {
            return Variable;
        }

        public string GetVariable(string key)
        {
            return VariableMap.TryGetValue(key, out var value) ? value : "";
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
    }
}