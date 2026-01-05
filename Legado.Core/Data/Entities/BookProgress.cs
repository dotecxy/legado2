using FreeSql.DataAnnotations;
using Newtonsoft.Json;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书籍进度（对应 Kotlin 的 BookProgress.kt）
    /// </summary>
    [Table(Name = "book_progress")]
    public class BookProgress
    {
        /// <summary>
        /// 书籍名称（主键之一）
        /// </summary>
        [Column(IsPrimary = true, StringLength = 255)]
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// 作者（主键之一）
        /// </summary>
        [Column(IsPrimary = true, StringLength = 255)]
        [JsonProperty("author")]
        public string Author { get; set; } = "";

        /// <summary>
        /// 当前章节索引
        /// </summary>
        
        [JsonProperty("durChapterIndex")]
        public int DurChapterIndex { get; set; } = 0;

        /// <summary>
        /// 当前章节位置
        /// </summary>
        
        [JsonProperty("durChapterPos")]
        public int DurChapterPos { get; set; } = 0;

        /// <summary>
        /// 当前章节时间
        /// </summary>
        
        [JsonProperty("durChapterTime")]
        public long DurChapterTime { get; set; } = 0L;

        /// <summary>
        /// 当前章节标题
        /// </summary>
        
        [JsonProperty("durChapterTitle")]
        public string DurChapterTitle { get; set; }

        public override int GetHashCode()
        {
            return (Name + Author).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is BookProgress other)
            {
                return Name == other.Name && Author == other.Author;
            }
            return false;
        }
    }
}
