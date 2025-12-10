using Newtonsoft.Json;
using SQLite;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 阅读记录（对应 ReadRecord.kt）
    /// </summary>
    [Table("readRecord")]
    public class ReadRecord
    {
        /// <summary>
        /// 设备ID（主键之一）
        /// </summary>
        [PrimaryKey]
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; } = "";

        /// <summary>
        /// 书名（主键之一）
        /// </summary>
        [PrimaryKey]
        [JsonProperty("bookName")]
        public string BookName { get; set; } = "";

        /// <summary>
        /// 阅读时长（毫秒）
        /// </summary>
        [Column("readTime")]
        [JsonProperty("readTime")]
        public long ReadTime { get; set; } = 0L;

        /// <summary>
        /// 最后阅读时间
        /// </summary>
        [Column("lastRead")]
        [JsonProperty("lastRead")]
        public long LastRead { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
