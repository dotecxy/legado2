using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Legado.Core.Models.AnalyzeRules
{
    /// <summary>
    /// 使用 JsonPath 分析规则 
    /// </summary>
    public class AnalyzeByJsonPath
    {
        private readonly JToken _jToken;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="json">JSON 字符串或对象</param>
        public AnalyzeByJsonPath(string json)
        {
            _jToken = Parse(json);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="token">JToken 对象</param>
        public AnalyzeByJsonPath(JToken token)
        {
            _jToken = token;
        }

        /// <summary>
        /// 解析 JSON
        /// </summary>
        /// <param name="json">JSON 字符串或对象</param>
        /// <returns>JToken 对象</returns>
        private static JToken Parse(object json)
        {
            if (json is JToken token)
                return token;

            if (json is string jsonStr)
            {
                try
                {
                    if (jsonStr.TrimStart().StartsWith("["))
                        return JArray.Parse(jsonStr);
                    else
                        return JObject.Parse(jsonStr);
                }
                catch
                {
                    // 如果解析失败，尝试作为纯字符串处理
                    return JValue.CreateString(jsonStr);
                }
            }

            // 其他类型直接转换为 JToken
            return JToken.FromObject(json);
        }

        /// <summary>
        /// 获取字符串结果
        /// 改进解析方法，解决阅读"&&"、"||"与 JsonPath 支持的"&&"、"||"之间的冲突
        /// 解决"{$.rule}"形式规则可能匹配错误的问题
        /// </summary>
        /// <param name="rule">JsonPath 规则</param>
        /// <returns>字符串结果，如果没有匹配则返回 null</returns>
        public string? GetString(string rule)
        {
            if (string.IsNullOrEmpty(rule))
                return null;

            // 创建规则分析器
            var ruleAnalyzer = new RuleAnalyzer(rule, true);

            // 分割规则
            var rules = ruleAnalyzer.SplitRule("&&", "||");

            if (rules.Count == 1)
            {
                ruleAnalyzer.ResetPos(); // 重置位置

                // 替换所有内嵌规则 {$.rule...}
                var result = ruleAnalyzer.InnerRule("{$.",
                   fr: innerRule => GetString(innerRule));

                if (string.IsNullOrEmpty(result))
                {
                    // 如果没有成功替换的内嵌规则，直接执行 JsonPath
                    try
                    {
                        var resultToken = _jToken.SelectToken(rule);
                        if (resultToken == null)
                            return null;

                        if (resultToken is JArray jArray)
                        {
                            return string.Join("\n", jArray.Select(token => token.ToString()));
                        }

                        return resultToken.ToString();
                    }
                    catch (Exception ex)
                    {
                        // 调试输出异常
                        DebugPrint(ex);
                        return null;
                    }
                }

                return result;
            }
            else
            {
                var textList = new List<string>();

                for (int i = 0; i < rules.Count; i++)
                {
                    var temp = GetString(rules[i]);
                    if (!string.IsNullOrEmpty(temp))
                    {
                        textList.Add(temp);

                        // 如果是 OR 规则，找到一个就退出
                        if (ruleAnalyzer.ElementsType == "||")
                            break;
                    }
                }

                return textList.Count > 0 ? string.Join("\n", textList) : null;
            }
        }

        /// <summary>
        /// 获取字符串列表结果
        /// </summary>
        /// <param name="rule">JsonPath 规则</param>
        /// <returns>字符串列表</returns>
        public List<string> GetStringList(string rule)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(rule))
                return result;

            // 创建规则分析器
            var ruleAnalyzer = new RuleAnalyzer(rule, true);

            // 分割规则
            var rules = ruleAnalyzer.SplitRule("&&", "||", "%%");

            if (rules.Count == 1)
            {
                ruleAnalyzer.ResetPos();

                // 替换所有内嵌规则
                var st = ruleAnalyzer.InnerRule("{$.",
                   fr: innerRule => GetString(innerRule));

                if (string.IsNullOrEmpty(st))
                {
                    // 如果没有成功替换的内嵌规则，直接执行 JsonPath
                    try
                    {
                        var resultTokens = _jToken.SelectTokens(rule);
                        if (resultTokens != null)
                        {
                            foreach (var token in resultTokens)
                            {
                                if (token != null)
                                    result.Add(token.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugPrint(ex);
                    }
                }
                else
                {
                    result.Add(st);
                }

                return result;
            }
            else
            {
                var results = new List<List<string>>();

                for (int i = 0; i < rules.Count; i++)
                {
                    var temp = GetStringList(rules[i]);
                    if (temp.Count > 0)
                    {
                        results.Add(temp);

                        // 如果是 OR 规则，找到一个就退出
                        if (ruleAnalyzer.ElementsType == "||")
                            break;
                    }
                }

                if (results.Count > 0)
                {
                    if (ruleAnalyzer.ElementsType == "%%")
                    {
                        // 交叉组合结果
                        var firstList = results[0];
                        for (int i = 0; i < firstList.Count; i++)
                        {
                            foreach (var temp in results)
                            {
                                if (i < temp.Count)
                                {
                                    result.Add(temp[i]);
                                }
                            }
                        }
                    }
                    else
                    {
                        // 合并所有结果
                        foreach (var temp in results)
                        {
                            result.AddRange(temp);
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// 获取对象结果
        /// </summary>
        /// <param name="rule">JsonPath 规则</param>
        /// <returns>对象结果</returns>
        public JToken? GetObject(string rule)
        {
            if (string.IsNullOrEmpty(rule))
                return null;

            try
            {
                return _jToken.SelectToken(rule);
            }
            catch (Exception ex)
            {
                DebugPrint(ex);
                return null;
            }
        }

        /// <summary>
        /// 获取列表结果
        /// </summary>
        /// <param name="rule">JsonPath 规则</param>
        /// <returns>对象列表</returns>
        public List<object> GetList(string rule)
        {
            var result = new List<object>();

            if (string.IsNullOrEmpty(rule))
                return result;

            // 创建规则分析器
            var ruleAnalyzer = new RuleAnalyzer(rule, true);

            // 分割规则
            var rules = ruleAnalyzer.SplitRule("&&", "||", "%%");

            if (rules.Count == 1)
            {
                try
                {
                    var tokens = _jToken.SelectTokens(rules[0]);
                    if (tokens != null)
                    {
                        foreach (var token in tokens)
                        {
                            if (token != null)
                                result.Add(token);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugPrint(ex);
                }
            }
            else
            {
                var results = new List<List<object>>();

                for (int i = 0; i < rules.Count; i++)
                {
                    var temp = GetList(rules[i]);
                    if (temp.Count > 0)
                    {
                        results.Add(temp);

                        // 如果是 OR 规则，找到一个就退出
                        if (ruleAnalyzer.ElementsType == "||")
                            break;
                    }
                }

                if (results.Count > 0)
                {
                    if (ruleAnalyzer.ElementsType == "%%")
                    {
                        // 交叉组合结果
                        var firstList = results[0];
                        for (int i = 0; i < firstList.Count; i++)
                        {
                            foreach (var temp in results)
                            {
                                if (i < temp.Count)
                                {
                                    result.Add(temp[i]);
                                }
                            }
                        }
                    }
                    else
                    {
                        // 合并所有结果
                        foreach (var temp in results)
                        {
                            result.AddRange(temp);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 调试输出
        /// </summary>
        /// <param name="ex">异常对象</param>
        private void DebugPrint(Exception ex)
        {
            // 在实际项目中，这里可以使用日志系统
            // 例如：Debug.WriteLine, ILogger, 或自定义的日志类
            System.Diagnostics.Debug.WriteLine($"JsonPath解析异常: {ex.Message}");
        }
    }

    /// <summary>
    /// JsonPath 工具类
    /// </summary>
    public static class JsonPathHelper
    {
        /// <summary>
        /// 解析 JSON
        /// </summary>
        public static JToken Parse(object json)
        {
            if (json is JToken token)
                return token;

            if (json is string jsonStr)
            {
                try
                {
                    if (jsonStr.TrimStart().StartsWith("["))
                        return JArray.Parse(jsonStr);
                    else
                        return JObject.Parse(jsonStr);
                }
                catch
                {
                    return JValue.CreateString(jsonStr);
                }
            }

            return JToken.FromObject(json);
        }
    }
}