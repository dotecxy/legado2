using Newtonsoft.Json;
using Legado.FreeSql;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 搜索关键词（对应 SearchKeyword.kt）
    /// </summary>
    [Table("search_keywords")]
    public class SearchKeyword
    {
        /// <summary>
        /// 搜索关键词
        /// </summary>
        [PrimaryKey]
        [Column("word")]
        [JsonProperty("word")]
        public string Word { get; set; } = "";

        /// <summary>
        /// 使用次数
        /// </summary>
        [Column("usage")]
        [JsonProperty("usage")]
        public int Usage { get; set; } = 1;

        /// <summary>
        /// 最后一次使用时间
        /// </summary>
        [Column("last_use_time")]
        [JsonProperty("lastUseTime")]
        public long LastUseTime { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
