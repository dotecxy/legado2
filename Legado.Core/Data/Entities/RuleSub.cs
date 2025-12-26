using Newtonsoft.Json;
using SQLite;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 规则订阅（对应 RuleSub.kt）
    /// </summary>
    [Table("rule_subs")]
    public class RuleSub
    {
        /// <summary>
        /// ID
        /// </summary>
        [PrimaryKey]
        [Column("id")]
        [JsonProperty("id")]
        public long Id { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 名称
        /// </summary>
        [Column("name")]
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// URL
        /// </summary>
        [Column("url")]
        [JsonProperty("url")]
        public string Url { get; set; } = "";

        /// <summary>
        /// 类型
        /// </summary>
        [Column("type")]
        [JsonProperty("type")]
        public int Type { get; set; } = 0;

        /// <summary>
        /// 自定义排序
        /// </summary>
        [Column("custom_order")]
        [JsonProperty("customOrder")]
        public int CustomOrder { get; set; } = 0;

        /// <summary>
        /// 自动更新
        /// </summary>
        [Column("auto_update")]
        [JsonProperty("autoUpdate")]
        public bool AutoUpdate { get; set; } = false;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("update")]
        [JsonProperty("update")]
        public long Update { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
