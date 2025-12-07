using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Legado.Core.Models
{
    /// <summary>
    /// 书籍实体
    /// 对应 Kotlin 的 Book.kt
    /// </summary>
    public class Book
    {
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; } // 唯一ID，通常是详情页链接

        [JsonProperty("tocUrl")]
        public string TocUrl { get; set; } // 目录页链接 (有时与BookUrl不同)

        [JsonProperty("origin")]
        public string Origin { get; set; } // 来源的 BookSourceUrl

        [JsonProperty("originName")]
        public string OriginName { get; set; } // 来源名称

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("customTag")]
        public string CustomTag { get; set; }

        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        [JsonProperty("intro")]
        public string Intro { get; set; }

        [JsonProperty("charset")]
        public string Charset { get; set; } // 编码

        [JsonProperty("type")]
        public int Type { get; set; } = 0; // 0:text, 1:audio

        [JsonProperty("group")]
        public int Group { get; set; } = 0; // 分组ID

        [JsonProperty("latestChapterTitle")]
        public string LatestChapterTitle { get; set; }

        [JsonProperty("latestChapterTime")]
        public long LatestChapterTime { get; set; } // 时间戳

        [JsonProperty("lastCheckTime")]
        public long LastCheckTime { get; set; }

        [JsonProperty("lastCheckCount")]
        public int LastCheckCount { get; set; }

        [JsonProperty("totalChapterNum")]
        public int TotalChapterNum { get; set; }

        [JsonProperty("durChapterTitle")]
        public string DurChapterTitle { get; set; }

        [JsonProperty("durChapterIndex")]
        public int DurChapterIndex { get; set; } // 当前阅读章节索引

        [JsonProperty("durChapterPos")]
        public int DurChapterPos { get; set; } // 当前阅读位置

        [JsonProperty("durChapterTime")]
        public long DurChapterTime { get; set; }

        [JsonProperty("variable")]
        public string Variable { get; set; } // 自定义变量 (JSON String)

        // 扩展方法：获取变量字典
        public Dictionary<string, string> GetVariableMap()
        {
            if (string.IsNullOrEmpty(Variable)) return new Dictionary<string, string>();
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(Variable);
            }
            catch { return new Dictionary<string, string>(); }
        }
    }
}