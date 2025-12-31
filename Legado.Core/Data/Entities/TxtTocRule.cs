using Newtonsoft.Json;
using Legado.FreeSql;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// TXT目录规则（对应 TxtTocRule.kt）
    /// </summary>
    [Table("txt_toc_rules")]
    public class TxtTocRule
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        [PrimaryKey]
        [Column("id")]
        [JsonProperty("id")]
        public long Id { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 规则名称
        /// </summary>
        [Column("name")]
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// 规则正则
        /// </summary>
        [Column("rule")]
        [JsonProperty("rule")]
        public string Rule { get; set; } = "";

        /// <summary>
        /// 示例
        /// </summary>
        [Column("example")]
        [JsonProperty("example")]
        public string Example { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        [Column("serial_number")]
        [JsonProperty("serialNumber")]
        public int SerialNumber { get; set; } = -1;

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("enable")]
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
