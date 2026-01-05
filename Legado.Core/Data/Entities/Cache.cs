using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 缓存实体（对应 Kotlin 的 Cache.kt）
    /// </summary>
    [Table(Name = "caches")]
    public class Cache
    {
        /// <summary>
        /// 缓存键（主键）
        /// </summary>
        [Column(IsPrimary = true, StringLength = 255)]
        [JsonProperty("key")]
        public string Key { get; set; } = "";

        /// <summary>
        /// 缓存值
        /// </summary>
        [Column(StringLength = -1)] // -1 表示 TEXT 类型
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// 过期时间（Unix毫秒时间戳，0表示永不过期）
        /// </summary>
        
        [JsonProperty("deadline")]
        public long Deadline { get; set; } = 0L;

        /// <summary>
        /// 判断缓存是否已过期（对应 Kotlin 的 isExpired）
        /// </summary>
        public bool IsExpired()
        {
            if (Deadline == 0L) return false;
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() > Deadline;
        }
    }
}
