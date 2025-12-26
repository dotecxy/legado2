using Newtonsoft.Json;
using SQLite;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 阅读记录（对应 ReadRecord.kt）
    /// </summary>
    [Table("read_records")]
    public class ReadRecord
    {
        /// <summary>
        /// 设备ID（主键之一）
        /// </summary>
        [PrimaryKey]
        [Column("device_id")]
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; } = "";

        /// <summary>
        /// 书名（主键之一）
        /// </summary>
        [PrimaryKey]
        [Column("book_name")]
        [JsonProperty("bookName")]
        public string BookName { get; set; } = "";

        /// <summary>
        /// 阅读时长（毫秒）
        /// </summary>
        [Column("read_time")]
        [JsonProperty("readTime")]
        public long ReadTime { get; set; } = 0L;

        /// <summary>
        /// 最后阅读时间
        /// </summary>
        [Column("last_read")]
        [JsonProperty("lastRead")]
        public long LastRead { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
