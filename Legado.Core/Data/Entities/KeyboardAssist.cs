using FreeSql.DataAnnotations;
using Newtonsoft.Json;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 键盘辅助（对应 Kotlin 的 KeyboardAssist.kt）
    /// </summary>
    [Table(Name = "keyboard_assists")]
    public class KeyboardAssist
    {
        /// <summary>
        /// 类型（主键之一）
        /// </summary>
        [Column(IsPrimary = true)]
        [JsonProperty("type")]
        public int Type { get; set; } = 0;

        /// <summary>
        /// 键（主键之一）
        /// </summary>
        [Column(IsPrimary = true, StringLength = 255)]
        [JsonProperty("key")]
        public string Key { get; set; } = "";

        /// <summary>
        /// 值
        /// </summary>
        
        [JsonProperty("value")]
        public string Value { get; set; } = "";

        /// <summary>
        /// 序号
        /// </summary>
        
        [JsonProperty("serialNo")]
        public int SerialNo { get; set; } = 0;
    }
}
