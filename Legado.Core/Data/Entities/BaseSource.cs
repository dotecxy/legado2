using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Legado.Core.Helps.Http.Api;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SQLite; // 对应 sqlite-net-pcl
using System;
using System.Collections.Generic;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 源的基类 (对应 BaseSource.kt)
    /// </summary>
    public abstract class BaseSource
    {
        // 对应 JSON 中的 sourceUrl，作为主键
        [PrimaryKey, Column("bookSourceUrl")]
        [JsonProperty("bookSourceUrl")]
        public string BookSourceUrl { get; set; }

        [Column("bookSourceName")]
        [JsonProperty("bookSourceName")]
        public string BookSourceName { get; set; }

        [Column("bookSourceGroup")]
        [JsonProperty("bookSourceGroup")]
        public string BookSourceGroup { get; set; }

        // 0: 文本, 1: 音频, 2: 图片, 3: 文件(PDF/EPUB)
        [Column("bookSourceType")]
        [JsonProperty("bookSourceType")]
        public int BookSourceType { get; set; } = 0;

        // 登录URL
        [Column("loginUrl")]
        [JsonProperty("loginUrl")]
        public string LoginUrl { get; set; }

        // 登录UI规则
        [Column("loginUi")]
        [JsonProperty("loginUi")]
        public string LoginUi { get; set; }

        // 登录检测规则
        [Column("loginCheckJs")]
        [JsonProperty("loginCheckJs")]
        public string LoginCheckJs { get; set; }

        // 封面解密规则
        [Column("coverDecodeJs")]
        [JsonProperty("coverDecodeJs")]
        public string CoverDecodeJs { get; set; }

        // 注释
        [Column("bookSourceComment")]
        [JsonProperty("bookSourceComment")]
        public string BookSourceComment { get; set; }

        // 最后更新时间
        [Column("lastUpdateTime")]
        [JsonProperty("lastUpdateTime")]
        public long LastUpdateTime { get; set; }

        // 自定义排序权重
        [Column("customOrder")]
        [JsonProperty("customOrder")]
        public int CustomOrder { get; set; }

        // 是否启用
        [Column("enabled")]
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        // 是否启用探索
        [Column("enabledExplore")]
        [JsonProperty("enabledExplore")]
        public bool EnabledExplore { get; set; } = true;

        // 是否启用 CookieJar (自动管理 Cookie)
        [Column("enabledCookieJar")]
        [JsonProperty("enabledCookieJar")]
        public bool EnabledCookieJar { get; set; } = true; // 默认为 true

        // 并发率
        [Column("concurrentRate")]
        [JsonProperty("concurrentRate")]
        public string ConcurrentRate { get; set; }

        // JS库
        [Column("jsLib")]
        [JsonProperty("jsLib")]
        public string JsLib { get; set; }

        // 变量说明
        [Column("variableComment")]
        [JsonProperty("variableComment")]
        public string VariableComment { get; set; }

        // 请求头 (存储为 JSON 字符串)
        [Column("header")]
        [JsonProperty("header")]
        public string Header { get; set; }


        [JsonProperty("cookieStore")]
        public ICookieManager CookieStore { get; set; }

        // **************** 辅助方法 ****************

        /// <summary>
        /// 获取标签（对应 Kotlin 的 getTag）
        /// </summary>
        public virtual string GetTag()
        {
            return BookSourceName;
        }

        /// <summary>
        /// 获取 Header 的字典对象 (不存库)
        /// </summary>
        [Ignore, JsonIgnore]
        public Dictionary<string, string> HeaderMap
        {
            get
            {
                if (string.IsNullOrEmpty(Header)) return new Dictionary<string, string>();
                try
                {
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(Header)
                        ?? new Dictionary<string, string>();
                }
                catch
                {
                    return new Dictionary<string, string>();
                }
            }
        }

        /// <summary>
        /// 获取请求头（对应 Kotlin 的 getHeaderMap）
        /// </summary>
        public virtual Dictionary<string, string> GetHeaderMap(bool hasLoginHeader = false)
        {
            var headers = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(Header))
            {
                try
                {
                    var json = Header;
                    if (Header.StartsWith("@js:", StringComparison.OrdinalIgnoreCase))
                    {
                        json = EvalJS(Header.Substring(4))?.ToString() ?? "";
                    }
                    else if (Header.StartsWith("<js>", StringComparison.OrdinalIgnoreCase))
                    {
                        var endIndex = Header.LastIndexOf("<");
                        json = EvalJS(Header.Substring(4, endIndex - 4))?.ToString() ?? "";
                    }

                    var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (map != null)
                    {
                        foreach (var kv in map)
                        {
                            headers[kv.Key] = kv.Value;
                        }
                    }
                }
                catch
                {
                    // 忽略解析错误
                }
            }

            if (!headers.ContainsKey("User-Agent"))
            {
                headers["User-Agent"] = "Mozilla/5.0";
            }

            if (hasLoginHeader)
            {
                var loginHeaders = GetLoginHeaderMap();
                if (loginHeaders != null)
                {
                    foreach (var kv in loginHeaders)
                    {
                        headers[kv.Key] = kv.Value;
                    }
                }
            }

            return headers;
        }

        /// <summary>
        /// 获取登录头部信息（对应 Kotlin 的 getLoginHeaderMap）
        /// </summary>
        public virtual Dictionary<string, string> GetLoginHeaderMap()
        {
            // TODO: 实现缓存读取
            return null;
        }

        /// <summary>
        /// 获取变量（对应 Kotlin 的 getVariable）
        /// </summary>
        public virtual string GetVariable()
        {
            // TODO: 实现缓存读取
            return "";
        }

        /// <summary>
        /// 设置变量（对应 Kotlin 的 setVariable）
        /// </summary>
        public virtual void SetVariable(string variable)
        {
            // TODO: 实现缓存存储
        }

        public virtual string GetKey() => BookSourceUrl;


        public BaseSource GetSource()
        {
            return this;
        }


        public object EvalJS(string jsStr, Action<Engine> bindgingsConfig = null)
        {
            using var engine = new Engine(options =>
            {
                options.AllowClr(); // 允许访问 C# 类和方法
                options.SetTypeResolver(new TypeResolver
                {
                    MemberNameComparer = StringComparer.Ordinal
                });
            });
            engine.SetValue("java", this);
            engine.SetValue("source", this);

            engine.SetValue("baseUrl", GetKey());
            engine.SetValue("cookie", CookieStore);
            //engine.SetValue("cache", CacheManager);
            bindgingsConfig.Invoke(engine);

            return ConvertJsValue(engine.Evaluate(jsStr));
        }

        /// <summary>
        /// 辅助方法：将 Jint 的 JsValue 转换为 C# 原生对象
        /// </summary>
        private object ConvertJsValue(JsValue value)
        {
            if (value == null || value.IsNull() || value.IsUndefined()) return null;
            if (value.IsString()) return value.ToString();
            if (value.IsBoolean()) return value.AsBoolean();
            if (value.IsNumber()) return value.AsNumber();
            if (value.IsArray())
            {
                var arr = value.AsArray();
                var list = new List<object>();
                foreach (var item in arr) list.Add(ConvertJsValue(item));
                return list;
            }
            // 对于复杂对象，返回原始 JsValue 或继续转换
            return value.ToString();
        }

    }
}