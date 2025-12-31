using Newtonsoft.Json;
using Legado.FreeSql;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 在线朗读引擎（对应 HttpTTS.kt）
    /// </summary>
    [Table("http_tts")]
    public class HttpTTS
    {
        /// <summary>
        /// ID
        /// </summary>
        [PrimaryKey]
        [Column("id")]
        [JsonProperty("id")]
        public long Id { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 名称
        /// </summary>
        [Column("name")]
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// URL
        /// </summary>
        [Column("url")]
        [JsonProperty("url")]
        public string Url { get; set; } = "";

        /// <summary>
        /// 内容类型
        /// </summary>
        [Column("content_type")]
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        /// <summary>
        /// 并发率
        /// </summary>
        [Column("concurrent_rate")]
        [JsonProperty("concurrentRate")]
        public string ConcurrentRate { get; set; } = "0";

        /// <summary>
        /// 登录URL
        /// </summary>
        [Column("login_url")]
        [JsonProperty("loginUrl")]
        public string LoginUrl { get; set; }

        /// <summary>
        /// 登录UI
        /// </summary>
        [Column("login_ui")]
        [JsonProperty("loginUi")]
        public string LoginUi { get; set; }

        /// <summary>
        /// 请求头
        /// </summary>
        [Column("header")]
        [JsonProperty("header")]
        public string Header { get; set; }

        /// <summary>
        /// JS库
        /// </summary>
        [Column("js_lib")]
        [JsonProperty("jsLib")]
        public string JsLib { get; set; }

        /// <summary>
        /// 启用CookieJar
        /// </summary>
        [Column("enabled_cookie_jar")]
        [JsonProperty("enabledCookieJar")]
        public bool EnabledCookieJar { get; set; } = false;

        /// <summary>
        /// 登录检测JS
        /// </summary>
        [Column("login_check_js")]
        [JsonProperty("loginCheckJs")]
        public string LoginCheckJs { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Column("last_update_time")]
        [JsonProperty("lastUpdateTime")]
        public long LastUpdateTime { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 获取标签（对应 Kotlin 的 getTag）
        /// </summary>
        public string GetTag()
        {
            return Name;
        }

        /// <summary>
        /// 获取键（对应 Kotlin 的 getKey）
        /// </summary>
        public string GetKey()
        {
            return $"httpTts:{Id}";
        }
    }
}
