using Newtonsoft.Json;

namespace Legado.Core.Data.Entities.Rules
{
    /// <summary>
    /// Flex 子元素样式（用于发现分类）
    /// </summary>
    public class FlexChildStyle
    {
        /// <summary>
        /// 默认样式
        /// </summary>
        public static readonly FlexChildStyle DefaultStyle = new FlexChildStyle
        {
            FlexGrow = 1,
            FlexShrink = 1,
            FlexBasis = "auto"
        };

        /// <summary>
        /// Flex 增长因子
        /// </summary>
        [JsonProperty("flexGrow")]
        public int FlexGrow { get; set; } = 0;

        /// <summary>
        /// Flex 收缩因子
        /// </summary>
        [JsonProperty("flexShrink")]
        public int FlexShrink { get; set; } = 1;

        /// <summary>
        /// Flex 基础大小
        /// </summary>
        [JsonProperty("flexBasis")]
        public string FlexBasis { get; set; } = "auto";

        /// <summary>
        /// 对齐方式
        /// </summary>
        [JsonProperty("alignSelf")]
        public string AlignSelf { get; set; }

        /// <summary>
        /// 最小宽度
        /// </summary>
        [JsonProperty("minWidth")]
        public string MinWidth { get; set; }

        /// <summary>
        /// 最大宽度
        /// </summary>
        [JsonProperty("maxWidth")]
        public string MaxWidth { get; set; }
    }
}
