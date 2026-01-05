using FreeSql.DataAnnotations;
using Newtonsoft.Json;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// RSS阅读记录（对应 Kotlin 的 RssReadRecord.kt）
    /// </summary>
    [Table(Name = "rss_read_records")]
    public class RssReadRecord
    {
        /// <summary>
        /// 记录字符串（主键）
        /// </summary>
        [Column(IsPrimary = true, StringLength = 255)]
        [JsonProperty("record")]
        public string Record { get; set; } = "";

        /// <summary>
        /// 阅读时间
        /// </summary>
        [JsonProperty("read")]
        public long Read { get; set; } = 0L;
    }
}
