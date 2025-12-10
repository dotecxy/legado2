using Newtonsoft.Json;
using SQLite;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// RSS阅读记录（对应 RssReadRecord.kt）
    /// </summary>
    [Table("rssReadRecords")]
    public class RssReadRecord
    {
        /// <summary>
        /// 记录字符串（主键）
        /// </summary>
        [PrimaryKey]
        [JsonProperty("record")]
        public string Record { get; set; } = "";

        /// <summary>
        /// 阅读时间
        /// </summary>
        [JsonProperty("read")]
        public long Read { get; set; } = 0L;
    }
}
