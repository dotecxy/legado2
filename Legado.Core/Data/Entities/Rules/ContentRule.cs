using Newtonsoft.Json;

namespace Legado.Core.Data.Entities.Rules
{
    /// <summary>
    /// 正文处理规则（对应 Kotlin 的 ContentRule.kt）
    /// </summary>
    public class ContentRule
    {
        /// <summary>
        /// 正文内容规则
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// 标题规则（有些网站只能在正文中获取标题）
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 下一页链接规则
        /// </summary>
        [JsonProperty("nextContentUrl")]
        public string NextContentUrl { get; set; }

        /// <summary>
        /// Web JS 脚本
        /// </summary>
        [JsonProperty("webJs")]
        public string WebJs { get; set; }

        /// <summary>
        /// 来源正则表达式
        /// </summary>
        [JsonProperty("sourceRegex")]
        public string SourceRegex { get; set; }

        /// <summary>
        /// 替换规则
        /// </summary>
        [JsonProperty("replaceRegex")]
        public string ReplaceRegex { get; set; }

        /// <summary>
        /// 图片样式（默认大小居中，FULL为最大宽度）
        /// </summary>
        [JsonProperty("imageStyle")]
        public string ImageStyle { get; set; }

        /// <summary>
        /// 图片bytes二次解密js，返回解密后的bytes
        /// </summary>
        [JsonProperty("imageDecode")]
        public string ImageDecode { get; set; }

        /// <summary>
        /// 购买操作，js或者包含{{js}}的url
        /// </summary>
        [JsonProperty("payAction")]
        public string PayAction { get; set; }
    }
}
