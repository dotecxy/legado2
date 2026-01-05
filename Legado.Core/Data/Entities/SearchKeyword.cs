using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 搜索关键词（对应 Kotlin 的 SearchKeyword.kt）
    /// </summary>
    [Table(Name = "search_keywords")]
    public class SearchKeyword
    {
        /// <summary>
        /// 搜索关键词
        /// </summary>
        [Column(IsPrimary = true, StringLength = 255)]
        [JsonProperty("word")]
        public string Word { get; set; } = "";

        /// <summary>
        /// 使用次数
        /// </summary>
        [JsonProperty("usage")]
        public int Usage { get; set; } = 1;

        /// <summary>
        /// 最后一次使用时间
        /// </summary>
        
        [JsonProperty("lastUseTime")]
        public long LastUseTime { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
