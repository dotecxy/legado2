using Newtonsoft.Json;
using SQLite; // 对应 sqlite-net-pcl

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 章节实体 (对应 BookChapter.kt)
    /// </summary>
    [Table("book_chapters")]
    public class BookChapter
    {
        // 数据库自增 ID (Legado 源码在 Room 中使用复合主键，SQLite-net 建议用自增ID)
        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int Id { get; set; }

        // 所属书籍 URL
        [Indexed]
        [Column("bookUrl")]
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; }

        // 章节序号
        [Column("index")]
        [JsonProperty("index")]
        public int Index { get; set; }

        // 章节链接 (通常作为书源内的唯一标识)
        [Column("url")]
        [JsonProperty("url")]
        public string Url { get; set; }

        [Column("title")]
        [JsonProperty("title")]
        public string Title { get; set; }

        // 章节标签 (如: 更新、VIP)
        [Column("tag")]
        [JsonProperty("tag")]
        public string Tag { get; set; }

        // 变量 (JSON)
        [Column("variable")]
        [JsonProperty("variable")]
        public string Variable { get; set; }

        // 是否已缓存
        [Ignore] // 通常不序列化到 JSON
        [JsonIgnore]
        public bool IsCached { get; set; }
    }
}