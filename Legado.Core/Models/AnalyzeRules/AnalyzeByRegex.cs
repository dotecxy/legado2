using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace Legado.Core.Models.AnalyzeRules
{
    public static class AnalyzeByRegex
    {
        /// <summary>
        /// 编译正则表达式并缓存以提高性能
        /// </summary>
        private static readonly Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();


        /// <summary>
        /// 获取正则表达式匹配的元素
        /// </summary>
        /// <param name="res">输入字符串</param>
        /// <param name="regs">正则表达式数组</param>
        /// <param name="index">当前处理的正则表达式索引</param>
        /// <returns>匹配结果列表，如果没有匹配则返回null</returns>
        public static List<string> GetElement(string res, string[] regs, int index = 0)
        {
            var vIndex = index;

            if (string.IsNullOrEmpty(res) || regs == null || vIndex >= regs.Length)
                return null;

            // 编译正则表达式
            var regex = new Regex(regs[vIndex], RegexOptions.Compiled);
            var resM = regex.Match(res);

            if (!resM.Success)
                return null;

            // 判断索引的规则是否是最后一个规则
            if (vIndex + 1 == regs.Length)
            {
                // 创建结果容器
                var info = new List<string>();

                // 添加整个匹配结果
                info.Add(resM.Value);

                // 添加所有捕获组
                for (int groupIndex = 1; groupIndex < resM.Groups.Count; groupIndex++)
                {
                    info.Add(resM.Groups[groupIndex].Value);
                }

                return info;
            }
            else
            {
                // 收集所有匹配的结果
                var result = new StringBuilder();
                var currentMatch = resM;

                do
                {
                    result.Append(currentMatch.Value);
                } while ((currentMatch = currentMatch.NextMatch()).Success);

                // 递归处理下一个正则表达式
                return GetElement(result.ToString(), regs, ++vIndex);
            }
        }

        /// <summary>
        /// 获取正则表达式匹配的多个元素
        /// </summary>
        /// <param name="res">输入字符串</param>
        /// <param name="regs">正则表达式数组</param>
        /// <param name="index">当前处理的正则表达式索引</param>
        /// <returns>匹配结果列表的列表，如果没有匹配则返回空列表</returns>
        public static List<List<string>> GetElements(string res, string[] regs, int index = 0)
        {
            var vIndex = index;

            if (string.IsNullOrEmpty(res) || regs == null || vIndex >= regs.Length)
                return new List<List<string>>();

            // 编译正则表达式
            var regex = new Regex(regs[vIndex], RegexOptions.Compiled);
            var resM = regex.Match(res);

            if (!resM.Success)
                return new List<List<string>>();

            // 判断索引的规则是否是最后一个规则
            if (vIndex + 1 == regs.Length)
            {
                // 创建结果容器
                var books = new List<List<string>>();

                // 提取所有匹配
                do
                {
                    // 创建单个匹配结果容器
                    var info = new List<string>();

                    // 添加整个匹配结果
                    info.Add(resM.Value);

                    // 添加所有捕获组
                    for (int groupIndex = 1; groupIndex < resM.Groups.Count; groupIndex++)
                    {
                        info.Add(resM.Groups[groupIndex].Value);
                    }

                    books.Add(info);

                } while ((resM = resM.NextMatch()).Success);

                return books;
            }
            else
            {
                // 收集所有匹配的结果
                var result = new StringBuilder();
                var currentMatch = resM;

                do
                {
                    result.Append(currentMatch.Value);
                } while ((currentMatch = currentMatch.NextMatch()).Success);

                // 递归处理下一个正则表达式
                return GetElements(result.ToString(), regs, ++vIndex);
            }
        }

        /// <summary>
        /// 获取正则表达式匹配的元素（字符串分割版本）
        /// </summary>
        /// <param name="res">输入字符串</param>
        /// <param name="regs">用"&&"分割的正则表达式字符串</param>
        /// <returns>匹配结果列表，如果没有匹配则返回null</returns>
        public static List<string> GetElement(string res, string regs)
        {
            if (string.IsNullOrEmpty(regs))
                return null;

            var regArray = regs.Split(new[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);
            return GetElement(res, regArray);
        }

        /// <summary>
        /// 获取正则表达式匹配的多个元素（字符串分割版本）
        /// </summary>
        /// <param name="res">输入字符串</param>
        /// <param name="regs">用"&&"分割的正则表达式字符串</param>
        /// <returns>匹配结果列表的列表，如果没有匹配则返回空列表</returns>
        public static List<List<string>> GetElements(string res, string regs)
        {
            if (string.IsNullOrEmpty(regs))
                return new List<List<string>>();

            var regArray = regs.Split(new[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);
            return GetElements(res, regArray);
        }

        /// <summary>
        /// 获取正则表达式匹配的单个结果
        /// </summary>
        /// <param name="res">输入字符串</param>
        /// <param name="pattern">正则表达式</param>
        /// <param name="groupIndex">捕获组索引（0表示整个匹配）</param>
        /// <returns>匹配的字符串，如果没有匹配则返回空字符串</returns>
        public static string GetSingleMatch(string res, string pattern, int groupIndex = 0)
        {
            if (string.IsNullOrEmpty(res) || string.IsNullOrEmpty(pattern))
                return string.Empty;

            var match = Regex.Match(res, pattern);
            if (!match.Success || groupIndex >= match.Groups.Count)
                return string.Empty;

            return match.Groups[groupIndex].Value;
        }

        /// <summary>
        /// 获取正则表达式匹配的所有结果
        /// </summary>
        /// <param name="res">输入字符串</param>
        /// <param name="pattern">正则表达式</param>
        /// <param name="groupIndex">捕获组索引（0表示整个匹配）</param>
        /// <returns>匹配的字符串列表</returns>
        public static List<string> GetAllMatches(string res, string pattern, int groupIndex = 0)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(res) || string.IsNullOrEmpty(pattern))
                return result;

            var matches = Regex.Matches(res, pattern);

            foreach (Match match in matches)
            {
                if (match.Success && groupIndex < match.Groups.Count)
                {
                    result.Add(match.Groups[groupIndex].Value);
                }
            }

            return result;
        }

        /// <summary>
        /// 使用正则表达式替换字符串
        /// </summary>
        /// <param name="res">输入字符串</param>
        /// <param name="pattern">正则表达式</param>
        /// <param name="replacement">替换字符串</param>
        /// <param name="replaceFirst">是否只替换第一个匹配</param>
        /// <returns>替换后的字符串</returns>
        public static string Replace(string res, string pattern, string replacement, bool replaceFirst = false)
        {
            if (string.IsNullOrEmpty(res) || string.IsNullOrEmpty(pattern))
                return res;

            if (replaceFirst)
            {
                return Regex.Replace(res, pattern, replacement, RegexOptions.None, TimeSpan.FromSeconds(1));
            }
            else
            {
                return Regex.Replace(res, pattern, replacement, RegexOptions.None, TimeSpan.FromSeconds(1));
            }
        }

        /// <summary>
        /// 验证字符串是否匹配正则表达式
        /// </summary>
        public static bool IsMatch(string res, string pattern)
        {
            if (string.IsNullOrEmpty(res) || string.IsNullOrEmpty(pattern))
                return false;

            return Regex.IsMatch(res, pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
        }



        /// <summary>
        /// 获取编译好的正则表达式（带缓存）
        /// </summary>
        public static Regex GetCompiledRegex(string pattern, RegexOptions options = RegexOptions.Compiled)
        {
            if (string.IsNullOrEmpty(pattern))
                return null;

            var cacheKey = $"{pattern}|{(int)options}";

            lock (_regexCache)
            {
                if (!_regexCache.TryGetValue(cacheKey, out var regex))
                {
                    regex = new Regex(pattern, options, TimeSpan.FromSeconds(1));
                    _regexCache[cacheKey] = regex;
                }

                return regex;
            }
        }

        /// <summary>
        /// 清除正则表达式缓存
        /// </summary>
        public static void ClearCache()
        {
            lock (_regexCache)
            {
                _regexCache.Clear();
            }
        }
    }

    /// <summary>
    /// 正则表达式相关的扩展方法
    /// </summary>
    public static class RegexExtensions
    {
        /// <summary>
        /// 在字符串上执行正则表达式匹配
        /// </summary>
        public static List<string> MatchGroups(this string input, string pattern)
        {
            return AnalyzeByRegex.GetElement(input, new[] { pattern });
        }

        /// <summary>
        /// 在字符串上执行正则表达式匹配并返回所有结果
        /// </summary>
        public static List<List<string>> MatchAllGroups(this string input, string pattern)
        {
            return AnalyzeByRegex.GetElements(input, new[] { pattern });
        }

        /// <summary>
        /// 检查字符串是否匹配正则表达式
        /// </summary>
        public static bool Matches(this string input, string pattern)
        {
            return AnalyzeByRegex.IsMatch(input, pattern);
        }

        /// <summary>
        /// 使用正则表达式替换字符串
        /// </summary>
        public static string RegexReplace(this string input, string pattern, string replacement, bool replaceFirst = false)
        {
            return AnalyzeByRegex.Replace(input, pattern, replacement, replaceFirst);
        }

        /// <summary>
        /// 获取正则表达式匹配的所有结果
        /// </summary>
        public static List<string> ExtractAll(this string input, string pattern, int groupIndex = 0)
        {
            return AnalyzeByRegex.GetAllMatches(input, pattern, groupIndex);
        }
    }


}