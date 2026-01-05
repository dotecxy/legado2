using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书签（对应 Kotlin 的 Bookmark.kt）
    /// </summary>
    [Table(Name = "bookmarks")]
    public class Bookmark
    {
        /// <summary>
        /// 时间戳作为ID
        /// </summary>
        [Column(IsPrimary = true)]
        [JsonProperty("time")]
        public long Time { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 书籍URL
        /// </summary>
        [Column(ServerTime = DateTimeKind.Unspecified)]
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; } = "";

        /// <summary>
        /// 书名
        /// </summary>
        
        [JsonProperty("bookName")]
        public string BookName { get; set; } = "";

        /// <summary>
        /// 作者
        /// </summary>
        
        [JsonProperty("bookAuthor")]
        public string BookAuthor { get; set; } = "";

        /// <summary>
        /// 章节索引
        /// </summary>
        
        [JsonProperty("chapterIndex")]
        public int ChapterIndex { get; set; } = 0;

        /// <summary>
        /// 章节位置
        /// </summary>
        
        [JsonProperty("chapterPos")]
        public int ChapterPos { get; set; } = 0;

        /// <summary>
        /// 章节名称
        /// </summary>
        
        [JsonProperty("chapterName")]
        public string ChapterName { get; set; } = "";

        /// <summary>
        /// 书签文本
        /// </summary>
        
        [JsonProperty("bookText")]
        public string BookText { get; set; } = "";

        /// <summary>
        /// 书签内容
        /// </summary>
        
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
