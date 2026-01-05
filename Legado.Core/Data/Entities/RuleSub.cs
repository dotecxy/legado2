using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 规则订阅（对应 Kotlin 的 RuleSub.kt）
    /// </summary>
    [Table(Name = "rule_subs")]
    public class RuleSub
    {
        /// <summary>
        /// ID
        /// </summary>
        [Column(IsPrimary = true)]
        [JsonProperty("id")]
        public long Id { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// URL
        /// </summary>
        
        [JsonProperty("url")]
        public string Url { get; set; } = "";

        /// <summary>
        /// 类型
        /// </summary>
        
        [JsonProperty("type")]
        public int Type { get; set; } = 0;

        /// <summary>
        /// 自定义排序
        /// </summary>
        
        [JsonProperty("customOrder")]
        public int CustomOrder { get; set; } = 0;

        /// <summary>
        /// 自动更新
        /// </summary>
        
        [JsonProperty("autoUpdate")]
        public bool AutoUpdate { get; set; } = false;

        /// <summary>
        /// 更新时间
        /// </summary>
        
        [JsonProperty("update")]
        public long Update { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
