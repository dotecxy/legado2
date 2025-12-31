using Newtonsoft.Json;
using Legado.FreeSql;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// RSS阅读记录（对应 RssReadRecord.kt）
    /// </summary>
    [Table("rss_read_records")]
    public class RssReadRecord
    {
        /// <summary>
        /// 记录字符串（主键）
        /// </summary>
        [PrimaryKey]
        [Column("record")]
        [JsonProperty("record")]
        public string Record { get; set; } = "";

        /// <summary>
        /// 阅读时间
        /// </summary>
        [Column("read")]
        [JsonProperty("read")]
        public long Read { get; set; } = 0L;
    }
}
