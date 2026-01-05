using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 词典规则（对应 Kotlin 的 DictRule.kt）
    /// </summary>
    [Table(Name = "dict_rules")]
    public class DictRule
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
        /// 规则
        /// </summary>
        
        [JsonProperty("rule")]
        public string Rule { get; set; } = "";

        /// <summary>
        /// 是否启用
        /// </summary>
        
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 排序
        /// </summary>
        
        [JsonProperty("sortNumber")]
        public int SortNumber { get; set; } = 0;

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is DictRule other)
            {
                return Id == other.Id;
            }
            return false;
        }
    }
}
