using Legado.Core.Constants;
using Legado.Core.Data.Entities.Rules;
using Legado.Core.Utils;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书源实体类（对应 Kotlin 的 BookSource.kt）
    /// </summary>
    [Table("book_sources")]
    public class BookSource : IBaseSource
    {
        /// <summary>
        /// 地址，包括 http/https
        /// </summary>
        [PrimaryKey]
        public string BookSourceUrl { get; set; } = "";

        /// <summary>
        /// 名称
        /// </summary>
        public string BookSourceName { get; set; } = "";

        /// <summary>
        /// 分组
        /// </summary>
        public string BookSourceGroup { get; set; }

        /// <summary>
        /// 类型，0 文本，1 音频, 2 图片, 3 文件（指的是类似知轩藏书只提供下载的网站）
        /// </summary>
        public int BookSourceType { get; set; } = 0;

        /// <summary>
        /// 详情页url正则
        /// </summary>
        public string BookUrlPattern { get; set; }

        /// <summary>
        /// 手动排序编号
        /// </summary>
        public int CustomOrder { get; set; } = 0;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 启用发现
        /// </summary>
        public bool EnabledExplore { get; set; } = true;

        /// <summary>
        /// JS库
        /// </summary>
        public string JsLib { get; set; }

        /// <summary>
        /// 启用okhttp CookieJar 自动保存每次请求的cookie
        /// </summary>
        public bool? EnabledCookieJar { get; set; } = true;

        /// <summary>
        /// 并发率
        /// </summary>
        public string ConcurrentRate { get; set; }

        /// <summary>
        /// 请求头
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// 解析后的请求头字典（对应 Kotlin 的 getHeaderMap()）
        /// </summary>
        [Ignore]
        public Dictionary<string, string> HeaderMap
        {
            get
            {
                var map = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(Header))
                {
                    try
                    {
                        var headerObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(Header);
                        if (headerObj != null)
                        {
                            foreach (var kvp in headerObj)
                            {
                                map[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                    catch
                    {
                        // 如果不是 JSON 格式，忽略
                    }
                }
                return map;
            }
        }

        /// <summary>
        /// 登录地址
        /// </summary>
        public string LoginUrl { get; set; }

        /// <summary>
        /// 登录UI
        /// </summary>
        public string LoginUi { get; set; }

        /// <summary>
        /// 登录检测js
        /// </summary>
        public string LoginCheckJs { get; set; }

        /// <summary>
        /// 封面解密js
        /// </summary>
        public string CoverDecodeJs { get; set; }

        /// <summary>
        /// 注释
        /// </summary>
        public string BookSourceComment { get; set; }

        /// <summary>
        /// 自定义变量说明
        /// </summary>
        public string VariableComment { get; set; }

        /// <summary>
        /// 最后更新时间，用于排序
        /// </summary>
        public long LastUpdateTime { get; set; } = 0;

        /// <summary>
        /// 响应时间，用于排序
        /// </summary>
        public long RespondTime { get; set; } = 180000L;

        /// <summary>
        /// 智能排序的权重
        /// </summary>
        public int Weight { get; set; } = 0;

        /// <summary>
        /// 发现url
        /// </summary>
        public string ExploreUrl { get; set; }

        /// <summary>
        /// 发现筛选规则
        /// </summary>
        public string ExploreScreen { get; set; }

        /// <summary>
        /// 发现规则
        /// </summary>
        [Ignore]
        public ExploreRule RuleExplore { get; set; }

        // 用于序列化到数据库
        public string RuleExploreJson
        {
            get => RuleExplore != null ? JsonConvert.SerializeObject(RuleExplore) : null;
            set => RuleExplore = !string.IsNullOrEmpty(value) ? JsonConvert.DeserializeObject<ExploreRule>(value) : null;
        }

        /// <summary>
        /// 搜索url
        /// </summary>
        public string SearchUrl { get; set; }

        /// <summary>
        /// 搜索规则
        /// </summary>
        [Ignore]
        public SearchRule RuleSearch { get; set; }

        // 用于序列化到数据库
        public string RuleSearchJson
        {
            get => RuleSearch != null ? JsonConvert.SerializeObject(RuleSearch) : null;
            set => RuleSearch = !string.IsNullOrEmpty(value) ? JsonConvert.DeserializeObject<SearchRule>(value) : null;
        }

        /// <summary>
        /// 书籍信息页规则
        /// </summary>
        [Ignore]
        public BookInfoRule RuleBookInfo { get; set; }

        // 用于序列化到数据库
        public string RuleBookInfoJson
        {
            get => RuleBookInfo != null ? JsonConvert.SerializeObject(RuleBookInfo) : null;
            set => RuleBookInfo = !string.IsNullOrEmpty(value) ? JsonConvert.DeserializeObject<BookInfoRule>(value) : null;
        }

        /// <summary>
        /// 目录页规则
        /// </summary>
        [Ignore]
        public TocRule RuleToc { get; set; }

        // 用于序列化到数据库
        public string RuleTocJson
        {
            get => RuleToc != null ? JsonConvert.SerializeObject(RuleToc) : null;
            set => RuleToc = !string.IsNullOrEmpty(value) ? JsonConvert.DeserializeObject<TocRule>(value) : null;
        }

        /// <summary>
        /// 正文页规则
        /// </summary>
        [Ignore]
        public ContentRule RuleContent { get; set; }

        // 用于序列化到数据库
        public string RuleContentJson
        {
            get => RuleContent != null ? JsonConvert.SerializeObject(RuleContent) : null;
            set => RuleContent = !string.IsNullOrEmpty(value) ? JsonConvert.DeserializeObject<ContentRule>(value) : null;
        }

        /// <summary>
        /// 段评规则
        /// </summary>
        [Ignore]
        public ReviewRule RuleReview { get; set; }

        // 用于序列化到数据库
        public string RuleReviewJson
        {
            get => RuleReview != null ? JsonConvert.SerializeObject(RuleReview) : null;
            set => RuleReview = !string.IsNullOrEmpty(value) ? JsonConvert.DeserializeObject<ReviewRule>(value) : null;
        }

        // IRuleData 实现
        [Ignore]
        public Dictionary<string, string> VariableMap { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 获取标签（对应 Kotlin 的 getTag）
        /// </summary>
        public string GetTag()
        {
            return BookSourceName;
        }

        /// <summary>
        /// 获取唯一键（对应 Kotlin 的 getKey）
        /// </summary>
        public string GetKey()
        {
            return BookSourceUrl;
        }

        public override int GetHashCode()
        {
            return BookSourceUrl?.GetHashCode() ?? 0;
        }

        public override bool Equals(object obj)
        {
            return obj is BookSource other && other.BookSourceUrl == BookSourceUrl;
        }

        /// <summary>
        /// 获取搜索规则（对应 Kotlin 的 getSearchRule）
        /// </summary>
        public SearchRule GetSearchRule()
        {
            if (RuleSearch != null) return RuleSearch;
            var rule = new SearchRule();
            RuleSearch = rule;
            return rule;
        }

        /// <summary>
        /// 获取发现规则（对应 Kotlin 的 getExploreRule）
        /// </summary>
        public ExploreRule GetExploreRule()
        {
            if (RuleExplore != null) return RuleExplore;
            var rule = new ExploreRule();
            RuleExplore = rule;
            return rule;
        }

        /// <summary>
        /// 获取书籍信息规则（对应 Kotlin 的 getBookInfoRule）
        /// </summary>
        public BookInfoRule GetBookInfoRule()
        {
            if (RuleBookInfo != null) return RuleBookInfo;
            var rule = new BookInfoRule();
            RuleBookInfo = rule;
            return rule;
        }

        /// <summary>
        /// 获取目录规则（对应 Kotlin 的 getTocRule）
        /// </summary>
        public TocRule GetTocRule()
        {
            if (RuleToc != null) return RuleToc;
            var rule = new TocRule();
            RuleToc = rule;
            return rule;
        }

        /// <summary>
        /// 获取正文规则（对应 Kotlin 的 getContentRule）
        /// </summary>
        public ContentRule GetContentRule()
        {
            if (RuleContent != null) return RuleContent;
            var rule = new ContentRule();
            RuleContent = rule;
            return rule;
        }

        /// <summary>
        /// 获取显示名称和分组（对应 Kotlin 的 getDisPlayNameGroup）
        /// </summary>
        public string GetDisplayNameGroup()
        {
            if (string.IsNullOrWhiteSpace(BookSourceGroup))
            {
                return BookSourceName;
            }
            return $"{BookSourceName} ({BookSourceGroup})";
        }

        /// <summary>
        /// 添加分组（对应 Kotlin 的 addGroup）
        /// </summary>
        public BookSource AddGroup(string groups)
        {
            if (!string.IsNullOrWhiteSpace(BookSourceGroup))
            {
                var groupSet = new HashSet<string>(
                    BookSourceGroup.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
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
                BookSourceGroup = string.Join(",", groupSet);
            }
            if (string.IsNullOrWhiteSpace(BookSourceGroup))
            {
                BookSourceGroup = groups;
            }
            return this;
        }

        /// <summary>
        /// 移除分组（对应 Kotlin 的 removeGroup）
        /// </summary>
        public BookSource RemoveGroup(string groups)
        {
            if (!string.IsNullOrWhiteSpace(BookSourceGroup))
            {
                var groupSet = new HashSet<string>(
                    BookSourceGroup.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
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
                BookSourceGroup = string.Join(",", groupSet);
            }
            return this;
        }

        /// <summary>
        /// 是否有某个分组（对应 Kotlin 的 hasGroup）
        /// </summary>
        public bool HasGroup(string group)
        {
            if (!string.IsNullOrWhiteSpace(BookSourceGroup))
            {
                var groupSet = new HashSet<string>(
                    BookSourceGroup.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(g => g.Trim())
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                );
                return groupSet.Contains(group);
            }
            return false;
        }

        /// <summary>
        /// 移除无效分组（对应 Kotlin 的 removeInvalidGroups）
        /// </summary>
        public void RemoveInvalidGroups()
        {
            RemoveGroup(GetInvalidGroupNames());
        }

        /// <summary>
        /// 移除错误注释（对应 Kotlin 的 removeErrorComment）
        /// </summary>
        public void RemoveErrorComment()
        {
            if (!string.IsNullOrWhiteSpace(BookSourceComment))
            {
                var comments = BookSourceComment.Split(new[] { "\n\n" }, StringSplitOptions.None)
                    .Where(it => !it.StartsWith("// Error: "));
                BookSourceComment = string.Join("\n", comments);
            }
        }

        /// <summary>
        /// 添加错误注释（对应 Kotlin 的 addErrorComment）
        /// </summary>
        public void AddErrorComment(Exception e)
        {
            var errorMsg = $"// Error: {e.Message}";
            BookSourceComment = string.IsNullOrWhiteSpace(BookSourceComment)
                ? errorMsg
                : $"{errorMsg}\n\n{BookSourceComment}";
        }

        /// <summary>
        /// 获取检查关键字（对应 Kotlin 的 getCheckKeyword）
        /// </summary>
        public string GetCheckKeyword(string defaultValue)
        {
            var checkKeyWord = RuleSearch?.CheckKeyWord;
            if (!string.IsNullOrWhiteSpace(checkKeyWord))
            {
                return checkKeyWord;
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取无效分组名称（对应 Kotlin 的 getInvalidGroupNames）
        /// </summary>
        public string GetInvalidGroupNames()
        {
            if (!string.IsNullOrWhiteSpace(BookSourceGroup))
            {
                var invalidGroups = BookSourceGroup.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => g.Trim())
                    .Where(g => !string.IsNullOrWhiteSpace(g) && (g.Contains("失效") || g == "校验超时"));
                return string.Join(",", invalidGroups);
            }
            return "";
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

        /// <summary>
        /// 判断是否相等（对应 Kotlin 的 equal）
        /// </summary>
        public bool Equal(BookSource source)
        {
            return EqualString(BookSourceName, source.BookSourceName)
                && EqualString(BookSourceUrl, source.BookSourceUrl)
                && EqualString(BookSourceGroup, source.BookSourceGroup)
                && BookSourceType == source.BookSourceType
                && EqualString(BookUrlPattern, source.BookUrlPattern)
                && EqualString(BookSourceComment, source.BookSourceComment)
                && CustomOrder == source.CustomOrder
                && Enabled == source.Enabled
                && EnabledExplore == source.EnabledExplore
                && EnabledCookieJar == source.EnabledCookieJar
                && EqualString(VariableComment, source.VariableComment)
                && EqualString(ConcurrentRate, source.ConcurrentRate)
                && EqualString(JsLib, source.JsLib)
                && EqualString(Header, source.Header)
                && EqualString(LoginUrl, source.LoginUrl)
                && EqualString(LoginUi, source.LoginUi)
                && EqualString(LoginCheckJs, source.LoginCheckJs)
                && EqualString(CoverDecodeJs, source.CoverDecodeJs)
                && EqualString(ExploreUrl, source.ExploreUrl)
                && EqualString(SearchUrl, source.SearchUrl)
                && GetSearchRule().Equals(source.GetSearchRule())
                && GetExploreRule().Equals(source.GetExploreRule())
                && GetBookInfoRule().Equals(source.GetBookInfoRule())
                && GetTocRule().Equals(source.GetTocRule())
                && GetContentRule().Equals(source.GetContentRule());
        }

        /// <summary>
        /// 字符串相等判断（对应 Kotlin 的 equal 私有方法）
        /// </summary>
        private bool EqualString(string a, string b)
        {
            return a == b || (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b));
        }

        // IRuleData 接口实现
        public bool PutVariable(string key, string value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (value == null)
                {
                    VariableMap.Remove(key);
                }
                else
                {
                    VariableMap[key] = value;
                }
                return true;
            }
            return false;
        }

        public string GetVariable()
        {
            if (VariableMap == null || VariableMap.Count == 0)
            {
                return null;
            }
            return JsonConvert.SerializeObject(VariableMap);
        }

        public string GetVariable(string key)
        {
            if (VariableMap.TryGetValue(key, out var value))
            {
                return value;
            }
            return "";
        }

        public void PutBigVariable(string key, string value)
        {
            // TODO: 实现大数据存储
        }

        public string GetBigVariable(string key)
        {
            // TODO: 实现大数据读取
            return null;
        }
    }
}
