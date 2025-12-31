using Newtonsoft.Json;
using Legado.FreeSql; // 对应 sqlite-net-pcl
using System;
using System.Text.RegularExpressions;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 净化规则 (对应 ReplaceRule.kt)
    /// </summary>
    [Table("replace_rules")]
    public class ReplaceRule
    {
        [PrimaryKey, AutoIncrement]
        [JsonProperty("id")]
        public long Id { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        [Column("name")]
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [Column("group")]
        [JsonProperty("group")]
        public string Group { get; set; }

        [Column("pattern")]
        [JsonProperty("pattern")]
        public string Pattern { get; set; } = "";

        [Column("replacement")]
        [JsonProperty("replacement")]
        public string Replacement { get; set; } = "";

        // 作用范围 (书名或源URL的正则，为空则对所有生效)
        [Column("scope")]
        [JsonProperty("scope")]
        public string Scope { get; set; }

        /// <summary>
        /// 作用于标题
        /// </summary>
        [Column("scopeTitle")]
        [JsonProperty("scopeTitle")]
        public bool ScopeTitle { get; set; } = false;

        /// <summary>
        /// 作用于正文
        /// </summary>
        [Column("scopeContent")]
        [JsonProperty("scopeContent")]
        public bool ScopeContent { get; set; } = true;

        /// <summary>
        /// 排除范围
        /// </summary>
        [Column("excludeScope")]
        [JsonProperty("excludeScope")]
        public string ExcludeScope { get; set; }

        [Column("isEnabled")]
        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; } = true;

        [Column("isRegex")]
        [JsonProperty("isRegex")]
        public bool IsRegex { get; set; } = true;

        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        [Column("timeoutMillisecond")]
        [JsonProperty("timeoutMillisecond")]
        public long TimeoutMillisecond { get; set; } = 3000L;

        /// <summary>
        /// 排序（对应 Kotlin 的 order）
        /// </summary>
        [Column("sortOrder")]
        [JsonProperty("order")]
        public int Order { get; set; } = int.MinValue;

        public override bool Equals(object other)
        {
            if (other is ReplaceRule rule)
            {
                return rule.Id == Id;
            }
            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// 获取显示名称和分组（对应 Kotlin 的 getDisplayNameGroup）
        /// </summary>
        public string GetDisplayNameGroup()
        {
            if (string.IsNullOrWhiteSpace(Group))
            {
                return Name;
            }
            return $"{Name} ({Group})";
        }

        /// <summary>
        /// 验证规则是否有效（对应 Kotlin 的 isValid）
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Pattern))
            {
                return false;
            }

            // 判断正则表达式是否正确
            if (IsRegex)
            {
                try
                {
                    Regex.Match("", Pattern);
                }
                catch (ArgumentException)
                {
                    return false;
                }

                // Pattern.compile测试通过，但是部分情况下会替换超时，报错，一般发生在修改表达式时漏删了
                if (Pattern.EndsWith("|") && !Pattern.EndsWith("\\|"))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取有效的超时时间（对应 Kotlin 的 getValidTimeoutMillisecond）
        /// </summary>
        public long GetValidTimeoutMillisecond()
        {
            if (TimeoutMillisecond <= 0)
            {
                return 3000L;
            }
            return TimeoutMillisecond;
        }
    }
}