using Newtonsoft.Json;
using SQLite; // 对应 sqlite-net-pcl

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 净化规则 (对应 ReplaceRule.kt)
    /// </summary>
    [Table("replace_rules")]
    public class ReplaceRule
    {
        [PrimaryKey, AutoIncrement]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Column("name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Column("group")]
        [JsonProperty("group")]
        public string Group { get; set; }

        [Column("pattern")]
        [JsonProperty("pattern")]
        public string Pattern { get; set; }

        [Column("replacement")]
        [JsonProperty("replacement")]
        public string Replacement { get; set; }

        // 作用范围 (书名或源URL的正则，为空则对所有生效)
        [Column("scope")]
        [JsonProperty("scope")]
        public string Scope { get; set; }

        [Column("isEnabled")]
        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; } = true;

        [Column("isRegex")]
        [JsonProperty("isRegex")]
        public bool IsRegex { get; set; } = true;
    }
}