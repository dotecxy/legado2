using Newtonsoft.Json;
using SQLite;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 键盘辅助（对应 KeyboardAssist.kt）
    /// </summary>
    [Table("keyboardAssists")]
    public class KeyboardAssist
    {
        /// <summary>
        /// 类型（主键之一）
        /// </summary>
        [PrimaryKey]
        [JsonProperty("type")]
        public int Type { get; set; } = 0;

        /// <summary>
        /// 键（主键之一）
        /// </summary>
        [PrimaryKey]
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
