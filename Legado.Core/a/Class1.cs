using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Legado.Core.Models
{
    /// <summary>
    /// 书源定义实体
    /// 对应 Kotlin 的 BookSource.kt
    /// </summary>
    public class BookSource
    {
        [JsonProperty("bookSourceUrl")]
        public string BookSourceUrl { get; set; }

        [JsonProperty("bookSourceName")]
        public string BookSourceName { get; set; }

        [JsonProperty("bookSourceGroup")]
        public string BookSourceGroup { get; set; }

        [JsonProperty("bookSourceType")]
        public int BookSourceType { get; set; } = 0; // 0:文本, 1:音频, 2:图片...

        [JsonProperty("bookSourceComment")]
        public string BookSourceComment { get; set; }

        [JsonProperty("loginUrl")]
        public string LoginUrl { get; set; }

        [JsonProperty("loginUi")]
        public string LoginUi { get; set; } // 登录UI配置

        [JsonProperty("loginCheckJs")]
        public string LoginCheckJs { get; set; }

        [JsonProperty("concurrentRate")]
        public string ConcurrentRate { get; set; } // 并发率

        [JsonProperty("header")]
        public string Header { get; set; } // 请求头 JSON 字符串

        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonProperty("enabledCookieJar")]
        public bool EnabledCookieJar { get; set; } = true; // 是否自动管理Cookie

        [JsonProperty("weight")]
        public int Weight { get; set; } = 0; // 排序权重

        // === 下面是核心规则对象 ===

        [JsonProperty("ruleSearch")]
        public SearchRule RuleSearch { get; set; }

        [JsonProperty("ruleExplore")]
        public ExploreRule RuleExplore { get; set; } // 发现/分类规则

        [JsonProperty("ruleBookInfo")]
        public BookInfoRule RuleBookInfo { get; set; }

        [JsonProperty("ruleToc")]
        public TocRule RuleToc { get; set; }

        [JsonProperty("ruleContent")]
        public ContentRule RuleContent { get; set; }

        // 构造函数初始化对象，防止空引用
        public BookSource()
        {
            RuleSearch = new SearchRule();
            RuleExplore = new ExploreRule();
            RuleBookInfo = new BookInfoRule();
            RuleToc = new TocRule();
            RuleContent = new ContentRule();
        }
    }
}