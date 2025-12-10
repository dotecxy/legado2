using Newtonsoft.Json;
using SQLite;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// TXT目录规则（对应 TxtTocRule.kt）
    /// </summary>
    [Table("txtTocRules")]
    public class TxtTocRule
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        [PrimaryKey]
        [JsonProperty("id")]
        public long Id { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 规则名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// 规则正则
        /// </summary>
        [JsonProperty("rule")]
        public string Rule { get; set; } = "";

        /// <summary>
        /// 示例
        /// </summary>
        [JsonProperty("example")]
        public string Example { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        [JsonProperty("serialNumber")]
        public int SerialNumber { get; set; } = -1;

        /// <summary>
        /// 是否启用
        /// </summary>
        [JsonProperty("enable")]
        public bool Enable { get; set; } = true;

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is TxtTocRule other)
            {
                return Id == other.Id;
            }
            return false;
        }
    }
}
