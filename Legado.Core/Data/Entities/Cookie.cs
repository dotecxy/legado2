using Newtonsoft.Json;
using FreeSql.DataAnnotations;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// Cookie存储（对应 Kotlin 的 Cookie.kt）
    /// </summary>
    [Table(Name = "cookies")]
    public class Cookie
    {
        /// <summary>
        /// 源URL（主键）
        /// </summary>
        [Column(IsPrimary = true)]
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Cookie内容
        /// </summary>
        [JsonProperty("cookie")]
        [Column(Name = "cookie")]
        public string Value { get; set; }
    }
}