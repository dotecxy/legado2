using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.XPath;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Legado.Core.Data.Entities;
using Legado.Core.Helps;
using Legado.Core.Helps.Http;
using Legado.Core.Models;
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Utils;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Legado.Core.Models.AnalyzeRules
{
    public enum RuleMode
    {
        XPath,
        Json,
        Default,
        Js,
        Regex
    }

    public class SourceRule
    {
        public string Rule { get; set; }
        public RuleMode Mode { get; set; }
        public string ReplaceRegex { get; set; } = "";
        public string Replacement { get; set; } = "";
        public bool ReplaceFirst { get; set; }
        public Dictionary<string, string> PutMap { get; } = new Dictionary<string, string>();
        public List<string> RuleParam { get; } = new List<string>();
        public List<int> RuleType { get; } = new List<int>();
        private const int GetRuleType = -2;
        private const int JsRuleType = -1;
        private const int DefaultRuleType = 0;

        private static readonly Regex PutPattern = new Regex("@put:(\\{[^}]+?\\})", RegexOptions.IgnoreCase);
        private static readonly Regex EvalPattern = new Regex("@get:\\{[^}]+?\\}|\\{\\{[\\w\\W]*?\\}\\}", RegexOptions.IgnoreCase);
        private static readonly Regex RegexPattern = new Regex("\\$\\d{1,2}");

        public SourceRule(string ruleStr, RuleMode mode = RuleMode.Default)
        {
            Mode = mode;

            if (mode == RuleMode.Js || mode == RuleMode.Regex)
            {
                Rule = ruleStr;
            }
            else
            {
                Rule = ParseRule(ruleStr);
            }

            Rule = SplitPutRule(Rule, PutMap);

            ParseRuleComponents(Rule);
        }

        private string ParseRule(string ruleStr)
        {
            if (ruleStr.StartsWith("@CSS:", StringComparison.OrdinalIgnoreCase))
            {
                Mode = RuleMode.Default;
                return ruleStr.Substring("@CSS:".Length);
            }
            else if (ruleStr.StartsWith("@@"))
            {
                Mode = RuleMode.Default;
                return ruleStr.Substring(2);
            }
            else if (ruleStr.StartsWith("@XPath:", StringComparison.OrdinalIgnoreCase))
            {
                Mode = RuleMode.XPath;
                return ruleStr.Substring("@XPath:".Length);
            }
            else if (ruleStr.StartsWith("@Json:", StringComparison.OrdinalIgnoreCase))
            {
                Mode = RuleMode.Json;
                return ruleStr.Substring("@Json:".Length);
            }
            else if (ruleStr.StartsWith("$.") || ruleStr.StartsWith("$[") || ruleStr.StartsWith("{"))
            {
                Mode = RuleMode.Json;
                return ruleStr;
            }
            else if (ruleStr.StartsWith("/"))
            {
                Mode = RuleMode.XPath;
                return ruleStr;
            }
            else
            {
                return ruleStr;
            }
        }

        private string SplitPutRule(string rule, Dictionary<string, string> putMap)
        {
            var matches = PutPattern.Matches(rule);
            foreach (Match match in matches)
            {
                rule = rule.Replace(match.Value, "");
                var putJsonStr = match.Groups[1].Value;
                try
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(putJsonStr);
                    if (dict != null)
                    {
                        foreach (var kv in dict)
                        {
                            putMap[kv.Key] = kv.Value;
                        }
                    }
                }
                catch
                {
                    // 忽略JSON解析错误
                }
            }
            return rule;
        }

        private void ParseRuleComponents(string rule)
        {
            var matches = EvalPattern.Matches(rule);
            int start = 0;

            foreach (Match match in matches)
            {
                if (match.Index > start)
                {
                    var tmp = rule.Substring(start, match.Index - start).Trim();
                    if (!string.IsNullOrEmpty(tmp))
                    {
                        SplitRegexPart(tmp);
                    }
                }

                var tmpMatch = match.Value;
                if (tmpMatch.StartsWith("@get:", StringComparison.OrdinalIgnoreCase))
                {
                    RuleType.Add(GetRuleType);
                    RuleParam.Add(tmpMatch.Substring(6, tmpMatch.Length - 7));
                }
                else if (tmpMatch.StartsWith("{{"))
                {
                    RuleType.Add(JsRuleType);
                    RuleParam.Add(tmpMatch.Substring(2, tmpMatch.Length - 4));
                }
                else
                {
                    SplitRegexPart(tmpMatch);
                }

                start = match.Index + match.Length;
            }

            if (rule.Length > start)
            {
                var tmp = rule.Substring(start).Trim();
                if (!string.IsNullOrEmpty(tmp))
                {
                    SplitRegexPart(tmp);
                }
            }
        }

        private void SplitRegexPart(string ruleStr)
        {
            var parts = ruleStr.Split(new[] { "##" }, 2, StringSplitOptions.None);
            var mainPart = parts[0];

            var matches = RegexPattern.Matches(mainPart);
            int start = 0;

            foreach (Match match in matches)
            {
                if (match.Index > start)
                {
                    var tmp = mainPart.Substring(start, match.Index - start);
                    RuleType.Add(DefaultRuleType);
                    RuleParam.Add(tmp);
                }

                var tmpMatch = match.Value;
                RuleType.Add(int.Parse(tmpMatch.Substring(1)));
                RuleParam.Add(tmpMatch);

                start = match.Index + match.Length;
            }

            if (mainPart.Length > start)
            {
                var tmp = mainPart.Substring(start);
                RuleType.Add(DefaultRuleType);
                RuleParam.Add(tmp);
            }

            if (parts.Length > 1)
            {
                ReplaceRegex = parts[1];
            }
            if (parts.Length > 2)
            {
                Replacement = parts[2];
            }
            if (parts.Length > 3)
            {
                ReplaceFirst = true;
            }
        }

        public void MakeUpRule(object result, AnalyzeRule analyzer)
        {
            var infoVal = new System.Text.StringBuilder();

            for (int i = 0; i < RuleParam.Count; i++)
            {
                var regType = RuleType[i];

                if (regType > DefaultRuleType)
                {
                    var list = result as IList<string>;
                    if (list != null && list.Count > regType)
                    {
                        infoVal.Append(list[regType]);
                    }
                    else
                    {
                        infoVal.Append(RuleParam[i]);
                    }
                }
                else if (regType == JsRuleType)
                {
                    var param = RuleParam[i];
                    if (IsRule(param))
                    {
                        var rule = new SourceRule(param);
                        infoVal.Append(analyzer.GetString(new List<SourceRule> { rule }, result));
                    }
                    else
                    {
                        var jsEval = analyzer.EvalJs(param, result);
                        infoVal.Append(jsEval?.ToString() ?? "");
                    }
                }
                else if (regType == GetRuleType)
                {
                    infoVal.Append(analyzer.Get(RuleParam[i]));
                }
                else
                {
                    infoVal.Append(RuleParam[i]);
                }
            }

            Rule = infoVal.ToString();
        }

        private bool IsRule(string ruleStr)
        {
            return ruleStr.StartsWith("@") ||
                   ruleStr.StartsWith("$.") ||
                   ruleStr.StartsWith("$[") ||
                   ruleStr.StartsWith("//");
        }
    }

    public class AnalyzeRule
    {
        private IRuleData _ruleData;
        private readonly BaseSource _source;
        private readonly bool _preUpdateJs;

        private BookChapter _chapter;
        private string _nextChapterUrl;
        private object _content;
        private string _baseUrl;
        private Uri _redirectUrl;
        private bool _isJson;
        private bool _isRegex;

        private readonly ConcurrentDictionary<string, List<SourceRule>> _stringRuleCache =
            new ConcurrentDictionary<string, List<SourceRule>>();
        private readonly ConcurrentDictionary<string, Regex> _regexCache =
            new ConcurrentDictionary<string, Regex>();
        private readonly ConcurrentDictionary<string, JsEvaluator> _scriptCache =
            new ConcurrentDictionary<string, JsEvaluator>();

        private readonly HtmlParser _htmlParser = new HtmlParser();
        private static readonly MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
        private CancellationToken _cancellationToken = CancellationToken.None;

        public AnalyzeRule(IRuleData ruleData = null, BaseSource source = null, bool preUpdateJs = false)
        {
            _ruleData = ruleData;
            _source = source;
            _preUpdateJs = preUpdateJs;
        }

        public AnalyzeRule SetContent(object content, string baseUrl = null)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _isJson = content is not IElement && IsJson(content.ToString());
            SetBaseUrl(baseUrl);
            return this;
        }

        public AnalyzeRule SetBaseUrl(string baseUrl)
        {
            if (!string.IsNullOrEmpty(baseUrl))
            {
                _baseUrl = baseUrl;
            }
            return this;
        }

        public Uri SetRedirectUrl(string url)
        {
            if (IsDataUrl(url))
            {
                return _redirectUrl;
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                _redirectUrl = uri;
            }
            return _redirectUrl;
        }

        public List<string> GetStringList(string rule, object content = null, bool isUrl = false)
        {
            if (string.IsNullOrEmpty(rule)) return null;

            var ruleList = SplitSourceRuleCacheString(rule);
            return GetStringList(ruleList, content, isUrl);
        }

        public List<string> GetStringList(List<SourceRule> ruleList, object content = null, bool isUrl = false)
        {
            object result = null;
            var currentContent = content ?? _content;

            if (currentContent != null && ruleList.Any())
            {
                result = currentContent;

                if (result is JsValue jsValue && jsValue.IsObject())
                {
                    var sourceRule = ruleList.First();
                    PutRule(sourceRule.PutMap);
                    sourceRule.MakeUpRule(result, this);

                    if (sourceRule.RuleParam.Count > 1)
                    {
                        result = sourceRule.Rule;
                    }
                    else
                    {
                        var obj = jsValue.AsObject();
                        result = obj.HasProperty(sourceRule.Rule) ? obj.Get(sourceRule.Rule).ToString() : null;
                    }

                    if (result != null && !string.IsNullOrEmpty(sourceRule.ReplaceRegex))
                    {
                        if (result is IList<string> list)
                        {
                            result = list.Select(s => ReplaceRegex(s, sourceRule)).ToList();
                        }
                        else
                        {
                            result = ReplaceRegex(result.ToString(), sourceRule);
                        }
                    }
                }
                else
                {
                    foreach (var sourceRule in ruleList)
                    {
                        PutRule(sourceRule.PutMap);
                        sourceRule.MakeUpRule(result, this);

                        if (result == null) continue;

                        var rule = sourceRule.Rule;
                        if (!string.IsNullOrEmpty(rule))
                        {
                            result = sourceRule.Mode switch
                            {
                                RuleMode.Js => EvalJs(rule, result),
                                RuleMode.Json => new AnalyzeByJsonPath(JsonConvert.SerializeObject(result)).GetStringList(rule),
                                RuleMode.XPath => new AnalyzeByXPath(result).GetStringList(rule),
                                _ => new AnalyzeByAngleSharp(result).GetStringList(rule)
                            };
                        }

                        if (result != null && !string.IsNullOrEmpty(sourceRule.ReplaceRegex))
                        {
                            if (result is IList<string> list)
                            {
                                result = list.Select(s => ReplaceRegex(s, sourceRule)).ToList();
                            }
                            else
                            {
                                result = ReplaceRegex(result.ToString(), sourceRule);
                            }
                        }
                    }
                }
            }

            if (result == null) return null;

            if (result is string str)
            {
                result = str.Split('\n').ToList();
            }

            if (isUrl)
            {
                var urlList = new List<string>();
                if (result is IList<object> resultList)
                {
                    foreach (var url in resultList)
                    {
                        var absoluteUrl = NetworkUtils.GetAbsoluteUrl(_redirectUrl, url.ToString());
                        if (!string.IsNullOrEmpty(absoluteUrl) && !urlList.Contains(absoluteUrl))
                        {
                            urlList.Add(absoluteUrl);
                        }
                    }
                }
                return urlList;
            }

            return result as List<string>;
        }

        public string GetString(string rule, object content = null, bool isUrl = false)
        {
            if (string.IsNullOrEmpty(rule)) return string.Empty;

            var ruleList = SplitSourceRuleCacheString(rule);
            return GetString(ruleList, content, isUrl);
        }

        public List<object> GetElements(string rule)
        {
            if (string.IsNullOrEmpty(rule)) return new List<object>();

            var ruleList = SplitSourceRuleCacheString(rule);
            return GetElements(ruleList);
        }

        public List<object> GetElements(List<SourceRule> ruleList)
        {
            var result = new List<object>();
            var currentContent = _content;

            if (currentContent != null && ruleList.Any())
            {
                object tempResult = currentContent;

                foreach (var sourceRule in ruleList)
                {
                    PutRule(sourceRule.PutMap);
                    sourceRule.MakeUpRule(tempResult, this);

                    if (tempResult == null) continue;

                    var rule = sourceRule.Rule;
                    if (!string.IsNullOrEmpty(rule))
                    {
                        tempResult = sourceRule.Mode switch
                        {
                            RuleMode.Js => EvalJs(rule, tempResult),
                            RuleMode.Json => new AnalyzeByJsonPath(JsonConvert.SerializeObject(tempResult)).GetList(rule),
                            RuleMode.XPath => new AnalyzeByXPath(tempResult).GetElements(rule).Cast<object>().ToList(),
                            _ => new AnalyzeByAngleSharp(tempResult).GetAllElements(rule)
                        };
                    }

                    if (tempResult is List<object> list)
                    {
                        result = list;
                    }
                    else if (tempResult != null)
                    {
                        result.Add(tempResult);
                    }
                }
            }

            return result;
        }

        public string GetString(List<SourceRule> ruleList, object content = null, bool isUrl = false, bool unescape = true)
        {
            object result = null;
            var currentContent = content ?? _content;

            if (currentContent != null && ruleList.Any())
            {
                result = currentContent;

                if (result is JsValue jsValue && jsValue.IsObject())
                {
                    var sourceRule = ruleList.First();
                    PutRule(sourceRule.PutMap);
                    sourceRule.MakeUpRule(result, this);

                    if (sourceRule.RuleParam.Count > 1)
                    {
                        result = sourceRule.Rule;
                    }
                    else
                    {
                        var obj = jsValue.AsObject();
                        result = obj.HasProperty(sourceRule.Rule) ?
                                 obj.Get(sourceRule.Rule).ToString() : null;
                    }

                    if (result != null && !string.IsNullOrEmpty(sourceRule.ReplaceRegex))
                    {
                        result = ReplaceRegex(result.ToString(), sourceRule);
                    }
                }
                else
                {
                    foreach (var sourceRule in ruleList)
                    {
                        PutRule(sourceRule.PutMap);
                        sourceRule.MakeUpRule(result, this);

                        if (result == null) continue;

                        var rule = sourceRule.Rule;
                        if (!string.IsNullOrEmpty(rule) || string.IsNullOrEmpty(sourceRule.ReplaceRegex))
                        {
                            result = sourceRule.Mode switch
                            {
                                RuleMode.Js => EvalJs(rule, result),
                                RuleMode.Json => new AnalyzeByJsonPath(JsonConvert.SerializeObject(result)).GetString(rule),
                                RuleMode.XPath => new AnalyzeByXPath(result).GetString(rule),
                                _ => new AnalyzeByAngleSharp(result).GetString(rule)
                            };
                        }

                        if (result != null && !string.IsNullOrEmpty(sourceRule.ReplaceRegex))
                        {
                            result = ReplaceRegex(result.ToString(), sourceRule);
                        }
                    }
                }
            }

            var resultStr = result?.ToString() ?? "";

            if (unescape && resultStr.Contains('&'))
            {
                resultStr = WebUtility.HtmlDecode(resultStr);
            }

            if (isUrl)
            {
                return string.IsNullOrEmpty(resultStr) ?
                       (_baseUrl ?? "") :
                     NetworkUtils.GetAbsoluteUrl(_redirectUrl, resultStr);
            }

            return resultStr;
        }



        private string ReplaceRegex(string input, SourceRule rule)
        {
            if (string.IsNullOrEmpty(rule.ReplaceRegex)) return input;

            var regex = CompileRegexCache(rule.ReplaceRegex);
            if (regex == null) return input.Replace(rule.ReplaceRegex, rule.Replacement);

            if (rule.ReplaceFirst)
            {
                var match = regex.Match(input);
                if (match.Success)
                {
                    return regex.Replace(match.Value, rule.Replacement, 1);
                }
                return rule.Replacement;
            }
            else
            {
                return regex.Replace(input, rule.Replacement);
            }
        }

        private Regex CompileRegexCache(string pattern)
        {
            return _regexCache.GetOrAdd(pattern, p =>
            {
                try
                {
                    return new Regex(p, RegexOptions.Compiled);
                }
                catch
                {
                    return null;
                }
            });
        }

        private List<SourceRule> SplitSourceRuleCacheString(string rule)
        {
            if (string.IsNullOrEmpty(rule)) return new List<SourceRule>();

            return _stringRuleCache.GetOrAdd(rule, r => SplitSourceRule(r));
        }

        public List<SourceRule> SplitSourceRule(string ruleStr, bool allInOne = false)
        {
            if (string.IsNullOrEmpty(ruleStr)) return new List<SourceRule>();

            var ruleList = new List<SourceRule>();
            var mode = RuleMode.Default;
            var start = 0;

            if (allInOne && ruleStr.StartsWith(":"))
            {
                mode = RuleMode.Regex;
                _isRegex = true;
                start = 1;
            }
            else if (_isRegex)
            {
                mode = RuleMode.Regex;
            }

            // 解析JavaScript块
            var jsPattern = Legado.Core.Constants.AppPattern.JS_PATTERN;
            var jsMatches = jsPattern.Matches(ruleStr);

            foreach (Match match in jsMatches)
            {
                if (match.Index > start)
                {
                    var tmp = ruleStr.Substring(start, match.Index - start).Trim();
                    if (!string.IsNullOrEmpty(tmp))
                    {
                        ruleList.Add(new SourceRule(tmp, mode));
                    }
                }

                var jsCode = !string.IsNullOrEmpty(match.Groups[2].Value) ?
                             match.Groups[2].Value : match.Groups[1].Value;
                ruleList.Add(new SourceRule(jsCode, RuleMode.Js));
                start = match.Index + match.Length;
            }

            if (ruleStr.Length > start)
            {
                var tmp = ruleStr.Substring(start).Trim();
                if (!string.IsNullOrEmpty(tmp))
                {
                    ruleList.Add(new SourceRule(tmp, mode));
                }
            }

            return ruleList;
        }

        private void PutRule(Dictionary<string, string> map)
        {
            foreach (var kv in map)
            {
                Put(kv.Key, GetString(kv.Value));
            }
        }

        public string Put(string key, string value)
        {
            if (_ruleData != null)
            {
                _ruleData.putVariable(key, value);
            }
            return value;
        }

        public string Get(string key)
        {
            if (_ruleData != null)
            {
                return _ruleData.getBigVariable(key);
            }
            return "";
        } 

        public object EvalJs(string jsCode, object result = null)
        {
            var engine = _scriptCache.GetOrAdd(jsCode, code =>
            {
                var jsEvaluator = new JsEvaluator();
                var bindingds = new Dictionary<string, object>();
                // 添加全局对象
                bindingds.Add("java", this);
                bindingds.Add("cookie", CookieStore.Instance);
                bindingds.Add("cache", _memoryCache);
                bindingds.Add("source", _source);
                bindingds.Add("book", _ruleData as Book);
                bindingds.Add("result", result);
                bindingds.Add("baseUrl", _baseUrl);
                bindingds.Add("chapter", _chapter);
                bindingds.Add("title", _chapter?.Title);
                bindingds.Add("src", _content);
                bindingds.Add("nextChapterUrl", _nextChapterUrl);
                bindingds.Add("rssArticle", _ruleData as RssArticle);
                jsEvaluator.Bindings = bindingds;
                return jsEvaluator;
            });

            try
            {
                return engine.EvalJs(jsCode);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private bool IsJson(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            str = str.Trim();
            return (str.StartsWith("{") && str.EndsWith("}")) ||
                   (str.StartsWith("[") && str.EndsWith("]"));
        }

        private bool IsDataUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && url.StartsWith("data:");
        }

        public AnalyzeRule SetRuleData(IRuleData ruleData)
        {
            _ruleData = ruleData;
            return this;
        }

        public AnalyzeRule SetChapter(BookChapter chapter)
        {
            _chapter = chapter;
            return this;
        }

        public AnalyzeRule SetNextChapterUrl(string nextChapterUrl)
        {
            _nextChapterUrl = nextChapterUrl;
            return this;
        }

        public AnalyzeRule SetCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        /// <summary>
        /// 获取原始内容
        /// </summary>
        public object GetContent()
        {
            return _content;
        }

        /// <summary>
        /// 获取 BaseUrl
        /// </summary>
        public string GetBaseUrl()
        {
            return _baseUrl;
        }

        /// <summary>
        /// 获取 RedirectUrl
        /// </summary>
        public Uri GetRedirectUrl()
        {
            return _redirectUrl;
        }

        /// <summary>
        /// 获取 Source
        /// </summary>
        public BaseSource GetSource()
        {
            return _source;
        }

        /// <summary>
        /// 获取 RuleData
        /// </summary>
        public IRuleData GetRuleData()
        {
            return _ruleData;
        }
    }


}