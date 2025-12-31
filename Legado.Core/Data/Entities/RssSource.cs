using Newtonsoft.Json;
using Legado.FreeSql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// RSS书源（对应 RssSource.kt）
    /// </summary>
    [Table("rss_sources")]
    public class RssSource
    {
        /// <summary>
        /// 源地址（主键）
        /// </summary>
        [PrimaryKey]
        [Column("source_url")]
        [JsonProperty("sourceUrl")]
        public string SourceUrl { get; set; } = "";

        /// <summary>
        /// 名称
        /// </summary>
        [Column("source_name")]
        [JsonProperty("sourceName")]
        public string SourceName { get; set; } = "";

        /// <summary>
        /// 图标
        /// </summary>
        [Column("source_icon")]
        [JsonProperty("sourceIcon")]
        public string SourceIcon { get; set; } = "";

        /// <summary>
        /// 分组
        /// </summary>
        [Column("source_group")]
        [JsonProperty("sourceGroup")]
        public string SourceGroup { get; set; }

        /// <summary>
        /// 注释
        /// </summary>
        [Column("source_comment")]
        [JsonProperty("sourceComment")]
        public string SourceComment { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("enabled")]
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 自定义变量说明
        /// </summary>
        [Column("variable_comment")]
        [JsonProperty("variableComment")]
        public string VariableComment { get; set; }

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
        public bool EnabledCookieJar { get; set; } = true;

        /// <summary>
        /// 并发率
        /// </summary>
        [Column("concurrent_rate")]
        [JsonProperty("concurrentRate")]
        public string ConcurrentRate { get; set; }

        /// <summary>
        /// 请求头
        /// </summary>
        [Column("header")]
        [JsonProperty("header")]
        public string Header { get; set; }

        /// <summary>
        /// 登录地址
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
        /// 登录检测JS
        /// </summary>
        [Column("login_check_js")]
        [JsonProperty("loginCheckJs")]
        public string LoginCheckJs { get; set; }

        /// <summary>
        /// 封面解密JS
        /// </summary>
        [Column("cover_decode_js")]
        [JsonProperty("coverDecodeJs")]
        public string CoverDecodeJs { get; set; }

        /// <summary>
        /// 分类URL
        /// </summary>
        [Column("sort_url")]
        [JsonProperty("sortUrl")]
        public string SortUrl { get; set; }

        /// <summary>
        /// 是否单URL源
        /// </summary>
        [Column("single_url")]
        [JsonProperty("singleUrl")]
        public bool SingleUrl { get; set; } = false;

        /// <summary>
        /// 列表样式（0,1,2）
        /// </summary>
        [Column("article_style")]
        [JsonProperty("articleStyle")]
        public int ArticleStyle { get; set; } = 0;

        /// <summary>
        /// 列表规则
        /// </summary>
        [Column("rule_articles")]
        [JsonProperty("ruleArticles")]
        public string RuleArticles { get; set; }

        /// <summary>
        /// 下一页规则
        /// </summary>
        [Column("rule_next_page")]
        [JsonProperty("ruleNextPage")]
        public string RuleNextPage { get; set; }

        /// <summary>
        /// 标题规则
        /// </summary>
        [Column("rule_title")]
        [JsonProperty("ruleTitle")]
        public string RuleTitle { get; set; }

        /// <summary>
        /// 发布日期规则
        /// </summary>
        [Column("rule_pub_date")]
        [JsonProperty("rulePubDate")]
        public string RulePubDate { get; set; }

        /// <summary>
        /// 描述规则
        /// </summary>
        [Column("rule_description")]
        [JsonProperty("ruleDescription")]
        public string RuleDescription { get; set; }

        /// <summary>
        /// 图片规则
        /// </summary>
        [Column("rule_image")]
        [JsonProperty("ruleImage")]
        public string RuleImage { get; set; }

        /// <summary>
        /// 链接规则
        /// </summary>
        [Column("rule_link")]
        [JsonProperty("ruleLink")]
        public string RuleLink { get; set; }

        /// <summary>
        /// 正文规则
        /// </summary>
        [Column("rule_content")]
        [JsonProperty("ruleContent")]
        public string RuleContent { get; set; }

        /// <summary>
        /// 正文URL白名单
        /// </summary>
        [Column("content_whitelist")]
        [JsonProperty("contentWhitelist")]
        public string ContentWhitelist { get; set; }

        /// <summary>
        /// 正文URL黑名单
        /// </summary>
        [Column("content_blacklist")]
        [JsonProperty("contentBlacklist")]
        public string ContentBlacklist { get; set; }

        /// <summary>
        /// 跳转URL拦截（JS，返回true拦截）
        /// </summary>
        [Column("should_override_url_loading")]
        [JsonProperty("shouldOverrideUrlLoading")]
        public string ShouldOverrideUrlLoading { get; set; }

        /// <summary>
        /// WebView样式
        /// </summary>
        [Column("style")]
        [JsonProperty("style")]
        public string Style { get; set; }

        /// <summary>
        /// 启用JS
        /// </summary>
        [Column("enable_js")]
        [JsonProperty("enableJs")]
        public bool EnableJs { get; set; } = true;

        /// <summary>
        /// 使用BaseUrl加载
        /// </summary>
        [Column("load_with_base_url")]
        [JsonProperty("loadWithBaseUrl")]
        public bool LoadWithBaseUrl { get; set; } = true;

        /// <summary>
        /// 注入JS
        /// </summary>
        [Column("inject_js")]
        [JsonProperty("injectJs")]
        public string InjectJs { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Column("last_update_time")]
        [JsonProperty("lastUpdateTime")]
        public long LastUpdateTime { get; set; } = 0;

        /// <summary>
        /// 自定义排序
        /// </summary>
        [Column("custom_order")]
        [JsonProperty("customOrder")]
        public int CustomOrder { get; set; } = 0;

        /// <summary>
        /// 获取标签（对应 Kotlin 的 getTag）
        /// </summary>
        public string GetTag()
        {
            return SourceName;
        }

        /// <summary>
        /// 获取键（对应 Kotlin 的 getKey）
        /// </summary>
        public string GetKey()
        {
            return SourceUrl;
        }

        public override bool Equals(object obj)
        {
            if (obj is RssSource other)
            {
                return other.SourceUrl == SourceUrl;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return SourceUrl.GetHashCode();
        }

        /// <summary>
        /// 判断两个源是否相等（对应 Kotlin 的 equal）
        /// </summary>
        public bool Equal(RssSource source)
        {
            return Equal(SourceUrl, source.SourceUrl)
                && Equal(SourceName, source.SourceName)
                && Equal(SourceIcon, source.SourceIcon)
                && Enabled == source.Enabled
                && Equal(SourceGroup, source.SourceGroup)
                && EnabledCookieJar == source.EnabledCookieJar
                && Equal(SourceComment, source.SourceComment)
                && Equal(ConcurrentRate, source.ConcurrentRate)
                && Equal(Header, source.Header)
                && Equal(LoginUrl, source.LoginUrl)
                && Equal(LoginUi, source.LoginUi)
                && Equal(LoginCheckJs, source.LoginCheckJs)
                && Equal(CoverDecodeJs, source.CoverDecodeJs)
                && Equal(SortUrl, source.SortUrl)
                && SingleUrl == source.SingleUrl
                && ArticleStyle == source.ArticleStyle
                && Equal(RuleArticles, source.RuleArticles)
                && Equal(RuleNextPage, source.RuleNextPage)
                && Equal(RuleTitle, source.RuleTitle)
                && Equal(RulePubDate, source.RulePubDate)
                && Equal(RuleDescription, source.RuleDescription)
                && Equal(RuleLink, source.RuleLink)
                && Equal(RuleContent, source.RuleContent)
                && EnableJs == source.EnableJs
                && LoadWithBaseUrl == source.LoadWithBaseUrl
                && Equal(VariableComment, source.VariableComment)
                && Equal(Style, source.Style)
                && Equal(InjectJs, source.InjectJs);
        }

        private bool Equal(string a, string b)
        {
            return a == b || (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b));
        }

        /// <summary>
        /// 获取显示名称和分组（对应 Kotlin 的 getDisplayNameGroup）
        /// </summary>
        public string GetDisplayNameGroup()
        {
            if (string.IsNullOrWhiteSpace(SourceGroup))
            {
                return SourceName;
            }
            return $"{SourceName} ({SourceGroup})";
        }

        /// <summary>
        /// 添加分组（对应 Kotlin 的 addGroup）
        /// </summary>
        public RssSource AddGroup(string groups)
        {
            if (!string.IsNullOrWhiteSpace(SourceGroup))
            {
                var groupSet = new HashSet<string>(
                    SourceGroup.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(g => g.Trim())
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                );

                var newGroups = groups.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => g.Trim())
                    .Where(g => !string.IsNullOrWhiteSpace(g));

                foreach (var g in newGroups)
                {
                    groupSet.Add(g);
                }

                SourceGroup = string.Join(",", groupSet);
            }

            if (string.IsNullOrWhiteSpace(SourceGroup))
            {
                SourceGroup = groups;
            }

            return this;
        }

        /// <summary>
        /// 移除分组（对应 Kotlin 的 removeGroup）
        /// </summary>
        public RssSource RemoveGroup(string groups)
        {
            if (!string.IsNullOrWhiteSpace(SourceGroup))
            {
                var groupSet = new HashSet<string>(
                    SourceGroup.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(g => g.Trim())
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                );

                var removeGroups = groups.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => g.Trim())
                    .Where(g => !string.IsNullOrWhiteSpace(g));

                foreach (var g in removeGroups)
                {
                    groupSet.Remove(g);
                }

                SourceGroup = string.Join(",", groupSet);
            }

            return this;
        }

        /// <summary>
        /// 获取显示变量注释（对应 Kotlin 的 getDisplayVariableComment）
        /// </summary>
        public string GetDisplayVariableComment(string otherComment)
        {
            if (string.IsNullOrWhiteSpace(VariableComment))
            {
                return otherComment;
            }
            return $"{VariableComment}\n{otherComment}";
        }
    }
}
