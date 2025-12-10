using Newtonsoft.Json;

namespace Legado.Core.Data.Entities.Rules
{
    /// <summary>
    /// 发现分类（对应 Kotlin 的 ExploreKind.kt）
    /// </summary>
    public class ExploreKind
    {
        /// <summary>
        /// 分类标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; } = "";

        /// <summary>
        /// 分类链接
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 分类样式
        /// </summary>
        [JsonProperty("style")]
        public FlexChildStyle Style { get; set; }

        /// <summary>
        /// 获取样式（对应 Kotlin 的 style() 方法）
        /// </summary>
        /// <returns>样式对象，如果为空则返回默认样式</returns>
        public FlexChildStyle GetStyle()
        {
            return Style ?? FlexChildStyle.DefaultStyle;
        }
    }
}
