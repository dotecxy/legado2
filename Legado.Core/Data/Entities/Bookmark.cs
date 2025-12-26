using Newtonsoft.Json;
using SQLite;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书签（对应 Bookmark.kt）
    /// </summary>
    [Table("bookmarks")]
    public class Bookmark
    {
        /// <summary>
        /// 时间戳作为ID
        /// </summary>
        [PrimaryKey]
        [Column("time")]
        [JsonProperty("time")]
        public long Time { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 书籍URL
        /// </summary>
        [Indexed]
        [Column("book_url")]
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; } = "";

        /// <summary>
        /// 书名
        /// </summary>
        [Column("book_name")]
        [JsonProperty("bookName")]
        public string BookName { get; set; } = "";

        /// <summary>
        /// 作者
        /// </summary>
        [Column("book_author")]
        [JsonProperty("bookAuthor")]
        public string BookAuthor { get; set; } = "";

        /// <summary>
        /// 章节索引
        /// </summary>
        [Column("chapter_index")]
        [JsonProperty("chapterIndex")]
        public int ChapterIndex { get; set; } = 0;

        /// <summary>
        /// 章节位置
        /// </summary>
        [Column("chapter_pos")]
        [JsonProperty("chapterPos")]
        public int ChapterPos { get; set; } = 0;

        /// <summary>
        /// 章节名称
        /// </summary>
        [Column("chapter_name")]
        [JsonProperty("chapterName")]
        public string ChapterName { get; set; } = "";

        /// <summary>
        /// 书签文本
        /// </summary>
        [Column("book_text")]
        [JsonProperty("bookText")]
        public string BookText { get; set; } = "";

        /// <summary>
        /// 书签内容
        /// </summary>
        [Column("content")]
        [JsonProperty("content")]
        public string Content { get; set; } = "";

        public override int GetHashCode()
        {
            return Time.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Bookmark other)
            {
                return Time == other.Time;
            }
            return false;
        }
    }
}
