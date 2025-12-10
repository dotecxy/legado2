using Newtonsoft.Json;

namespace Legado.Core.Data.Entities.Rules
{
    /// <summary>
    /// 行UI配置（对应 Kotlin 的 RowUi.kt）
    /// </summary>
    public class RowUi
    {
        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 类型（默认为 "text"）
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; } = "text";

        /// <summary>
        /// 操作
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        /// 样式
        /// </summary>
        [JsonProperty("style")]
        public FlexChildStyle Style { get; set; }

        /// <summary>
        /// 获取样式，如果为空则返回默认样式（对应 Kotlin 的 style()）
        /// </summary>
        public FlexChildStyle GetStyle()
        {
            return Style ?? FlexChildStyle.DefaultStyle;
        }

        /// <summary>
        /// UI 类型常量（对应 Kotlin 的 Type object）
        /// </summary>
        public static class TypeConstants
        {
            public const string Text = "text";
            public const string Password = "password";
            public const string Button = "button";
        }
    }
}
