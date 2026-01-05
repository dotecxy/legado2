using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 阅读记录（对应 Kotlin 的 ReadRecord.kt）
    /// </summary>
    [Table(Name = "read_records")]
    public class ReadRecord
    {
        /// <summary>
        /// 设备ID（主键之一）
        /// </summary>
        [Column(IsPrimary = true, StringLength = 255)]
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; } = "";

        /// <summary>
        /// 书名（主键之一）
        /// </summary>
        [Column(IsPrimary = true, StringLength = 255)]
        [JsonProperty("bookName")]
        public string BookName { get; set; } = "";

        /// <summary>
        /// 阅读时长（毫秒）
        /// </summary>
        
        [JsonProperty("readTime")]
        public long ReadTime { get; set; } = 0L;

        /// <summary>
        /// 最后阅读时间
        /// </summary>
        
        [JsonProperty("lastRead")]
        public long LastRead { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
