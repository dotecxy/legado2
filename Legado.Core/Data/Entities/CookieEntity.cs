using Newtonsoft.Json;
using SQLite;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// Cookie存储（对应 Cookie.kt）
    /// </summary>
    [Table("cookies")]
    public class CookieEntity
    {
        /// <summary>
        /// 源URL（主键）
        /// </summary>
        [PrimaryKey]
        [Column("url")]
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Cookie内容
        /// </summary>
        [Column("cookie")]
        [JsonProperty("cookie")]
        public string Cookie { get; set; }
    }
}