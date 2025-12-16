using Legado.Core.Data.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Legado.Core.Helps
{
    /// <summary>
    /// 替换规则解析器
    /// 用于解析JSON格式的替换规则
    /// </summary>
    public static class ReplaceAnalyzer
    {
        /// <summary>
        /// 将JSON数组转换为替换规则列表
        /// </summary>
        /// <param name="json">JSON数组字符串</param>
        /// <returns>替换规则列表，解析失败时返回错误信息</returns>
        public static (bool Success, List<ReplaceRule> Rules, string Error) JsonToReplaceRules(string json)
        {
            try
            {
                var replaceRules = new List<ReplaceRule>();
                var array = JArray.Parse(json);

                foreach (var item in array)
                {
                    var result = JsonToReplaceRule(item.ToString());
                    if (result.Success && result.Rule != null)
                    {
                        if (result.Rule.IsValid())
                        {
                            replaceRules.Add(result.Rule);
                        }
                    }
                    else if (!result.Success)
                    {
                        return (false, null, result.Error);
                    }
                }

                return (true, replaceRules, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// 将JSON对象转换为替换规则
        /// 支持两种格式：标准格式和旧格式（regex、replaceSummary等字段）
        /// </summary>
        /// <param name="json">JSON对象字符串</param>
        /// <returns>替换规则，解析失败时返回错误信息</returns>
        public static (bool Success, ReplaceRule Rule, string Error) JsonToReplaceRule(string json)
        {
            try
            {
                var trimmedJson = json.Trim();
                
                // 尝试直接反序列化为ReplaceRule
                ReplaceRule replaceRule = null;
                try
                {
                    replaceRule = JsonConvert.DeserializeObject<ReplaceRule>(trimmedJson);
                }
                catch
                {
                    // 反序列化失败，继续使用JObject解析
                }

                // 如果直接反序列化成功且pattern不为空，直接返回
                if (replaceRule != null && !string.IsNullOrEmpty(replaceRule.Pattern))
                {
                    return (true, replaceRule, null);
                }

                // 使用JObject解析旧格式
                var jsonItem = JObject.Parse(trimmedJson);
                var rule = new ReplaceRule();

                // 解析ID（优先使用id，否则使用时间戳）
                rule.Id = jsonItem.Value<long?>("id") ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                // 解析pattern（优先使用regex字段）
                rule.Pattern = jsonItem.Value<string>("regex") ?? jsonItem.Value<string>("pattern") ?? "";
                if (string.IsNullOrEmpty(rule.Pattern))
                {
                    return (false, null, "格式不对：pattern不能为空");
                }

                // 解析name（优先使用replaceSummary字段）
                rule.Name = jsonItem.Value<string>("replaceSummary") ?? jsonItem.Value<string>("name") ?? "";

                // 解析replacement
                rule.Replacement = jsonItem.Value<string>("replacement") ?? "";

                // 解析isRegex
                rule.IsRegex = jsonItem.Value<bool?>("isRegex") ?? false;

                // 解析scope（从useTo字段）
                rule.Scope = jsonItem.Value<string>("useTo") ?? jsonItem.Value<string>("scope");

                // 解析isEnabled（从enable字段）
                rule.IsEnabled = jsonItem.Value<bool?>("enable") ?? jsonItem.Value<bool?>("isEnabled") ?? false;

                // 解析order（从serialNumber字段）
                rule.Order = jsonItem.Value<int?>("serialNumber") ?? jsonItem.Value<int?>("order") ?? 0;

                return (true, rule, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
