using System;
using Newtonsoft.Json;

namespace Legado.Core.Models
{
    /// <summary>
    /// 章节信息
    /// 对应 Kotlin 的 BookChapter.kt
    /// </summary>
    public class BookChapter
    {
        [JsonProperty("url")]
        public string Url { get; set; } // 章节链接

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; } // 关联的书籍ID

        [JsonProperty("index")]
        public int Index { get; set; } // 章节序号

        [JsonProperty("resourceUrl")]
        public string ResourceUrl { get; set; } // 实际资源链接(音频/图片)

        [JsonProperty("tag")]
        public string Tag { get; set; } // 特殊标记

        [JsonProperty("start")]
        public long? Start { get; set; } // 音频起始位置

        [JsonProperty("end")]
        public long? End { get; set; } // 音频结束位置

        [JsonProperty("isVolume")]
        public bool IsVolume { get; set; } // 是否是卷名

        [JsonProperty("isVip")]
        public bool IsVip { get; set; }

        [JsonProperty("isPay")]
        public bool IsPay { get; set; }

        // 扩展字段：缓存内容
        [JsonIgnore]
        public string CachedContent { get; set; }
    }
}