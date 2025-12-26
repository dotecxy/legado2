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
using System.Text;
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

    /// <summary>
    /// 规则类
    /// </summary>
    public class SourceRule
    {
        internal string Rule { get; set; }
        internal RuleMode Mode { get; set; }
        internal string ReplaceRegex { get; set; } = "";
        internal string Replacement { get; set; } = "";
        internal bool ReplaceFirst { get; set; } = false;
        internal Dictionary<string, string> PutMap { get; } = new Dictionary<string, string>();
        internal List<string> RuleParam { get; private set; } = new List<string>();
        private List<int> RuleType { get; set; } = new List<int>();

        private const int GetRuleType = -2;
        private const int JsRuleType = -1;
        private const int DefaultRuleType = 0;

        // 正则表达式模式
        private static readonly Regex EvalPattern = new Regex(@"@get:\{[^}]+\}|(?<!\\)\{\{[^}]*\}\}", RegexOptions.Compiled);
        private static readonly Regex RegexPattern = new Regex(@"\$\d{1,2}", RegexOptions.Compiled);
        private static readonly Regex putPattern = new Regex(@"@put:\{([^:]+):([^}]+)\}", RegexOptions.Compiled);

        internal SourceRule(string ruleStr, RuleMode mode = RuleMode.Default)
        {
            Mode = mode;
            var rule = ruleStr;

            // 确定规则模式
            if (mode == RuleMode.Js || mode == RuleMode.Regex)
            {
                rule = ruleStr;
            }
            else if (ruleStr.StartsWith("@CSS:", StringComparison.OrdinalIgnoreCase))
            {
                Mode = RuleMode.Default;
                rule = ruleStr;
            }
            else if (ruleStr.StartsWith("@@"))
            {
                Mode = RuleMode.Default;
                rule = ruleStr.Substring(2);
            }
            else if (ruleStr.StartsWith("@XPath:", StringComparison.OrdinalIgnoreCase))
            {
                Mode = RuleMode.XPath;
                rule = ruleStr.Substring(7);
            }
            else if (ruleStr.StartsWith("@Json:", StringComparison.OrdinalIgnoreCase))
            {
                Mode = RuleMode.Json;
                rule = ruleStr.Substring(6);
            }
            else if (ruleStr.StartsWith("$.") || ruleStr.StartsWith("$["))
            {
                Mode = RuleMode.Json;
                rule = ruleStr;
            }
            else if (ruleStr.StartsWith("/")) // XPath特征很明显,无需配置单独的识别标头
            {
                Mode = RuleMode.XPath;
                rule = ruleStr;
            }

            // 分离put规则
            rule = SplitPutRule(rule, PutMap);

            // @get, {{, 拆分
            var start = 0;
            var evalMatches = EvalPattern.Matches(rule);

            if (evalMatches.Count > 0)
            {
                var firstMatch = evalMatches[0];
                var tmp = rule.Substring(start, firstMatch.Index);
                if (Mode != RuleMode.Js && Mode != RuleMode.Regex &&
                    (firstMatch.Index == 0 || !tmp.Contains("##")))
                {
                    Mode = RuleMode.Regex;
                }

                foreach (Match evalMatch in evalMatches)
                {
                    if (evalMatch.Index > start)
                    {
                        tmp = rule.Substring(start, evalMatch.Index - start);
                        SplitRegex(tmp);
                    }

                    tmp = evalMatch.Value;
                    if (tmp.StartsWith("@get:", StringComparison.OrdinalIgnoreCase))
                    {
                        RuleType.Add(GetRuleType);
                        RuleParam.Add(tmp.Substring(6, tmp.Length - 7)); // 去掉 @get:{ 和 }
                    }
                    else if (tmp.StartsWith("{{"))
                    {
                        RuleType.Add(JsRuleType);
                        RuleParam.Add(tmp.Substring(2, tmp.Length - 4)); // 去掉 {{ 和 }}
                    }
                    else
                    {
                        SplitRegex(tmp);
                    }

                    start = evalMatch.Index + evalMatch.Length;
                }
            }

            if (rule.Length > start)
            {
                var tmp = rule.Substring(start);
                SplitRegex(tmp);
            }

            Rule = rule;
        }

        /// <summary>
        /// 拆分 $\d{1,2}
        /// </summary>
        private void SplitRegex(string ruleStr)
        {
            var start = 0;
            var ruleStrArray = ruleStr.Split(new[] { "##" }, StringSplitOptions.None);
            var regexMatches = RegexPattern.Matches(ruleStrArray[0]);

            if (regexMatches.Count > 0)
            {
                if (Mode != RuleMode.Js && Mode != RuleMode.Regex)
                {
                    Mode = RuleMode.Regex;
                }

                foreach (Match regexMatch in regexMatches)
                {
                    if (regexMatch.Index > start)
                    {
                        var tmp = ruleStr.Substring(start, regexMatch.Index - start);
                        RuleType.Add(DefaultRuleType);
                        RuleParam.Add(tmp);
                    }

                    var tmp2 = regexMatch.Value;
                    RuleType.Add(int.Parse(tmp2.Substring(1)));
                    RuleParam.Add(tmp2);
                    start = regexMatch.Index + regexMatch.Length;
                }
            }

            if (ruleStr.Length > start)
            {
                var tmp = ruleStr.Substring(start);
                RuleType.Add(DefaultRuleType);
                RuleParam.Add(tmp);
            }
        }

        /// <summary>
        /// 替换 @get, {{, $\d
        /// </summary>
        internal void MakeUpRule(object result, AnalyzeRule analyzeRule)
        {
            var infoVal = new StringBuilder();

            if (RuleParam.Count > 0)
            {
                for (int index = RuleParam.Count - 1; index >= 0; index--)
                {
                    var regType = RuleType[index];

                    if (regType > DefaultRuleType)
                    {
                        // 处理 $\d 替换
                        if (result is List<string> resultList)
                        {
                            if (resultList.Count > regType && regType < resultList.Count)
                            {
                                var value = resultList[regType];
                                if (value != null)
                                {
                                    infoVal.Insert(0, value);
                                }
                            }
                            else
                            {
                                infoVal.Insert(0, RuleParam[index]);
                            }
                        }
                        else
                        {
                            infoVal.Insert(0, RuleParam[index]);
                        }
                    }
                    else if (regType == JsRuleType)
                    {
                        // 处理 {{ }} JavaScript 规则
                        if (IsRule(RuleParam[index]))
                        {
                            var ruleList = new List<SourceRule> { new SourceRule(RuleParam[index]) };
                            var value = analyzeRule.GetString(ruleList);
                            infoVal.Insert(0, value ?? "");
                        }
                        else
                        {
                            try
                            {
                                var jsEval = analyzeRule.EvalJs(RuleParam[index], result);
                                if (jsEval != null)
                                {
                                    if (jsEval is string strValue)
                                    {
                                        infoVal.Insert(0, strValue);
                                    }
                                    else if (jsEval is double doubleValue && doubleValue % 1.0 == 0.0)
                                    {
                                        infoVal.Insert(0, string.Format("{0:F0}", doubleValue));
                                    }
                                    else
                                    {
                                        infoVal.Insert(0, jsEval.ToString());
                                    }
                                }
                            }
                            catch
                            {
                                // JavaScript 执行失败，忽略
                            }
                        }
                    }
                    else if (regType == GetRuleType)
                    {
                        // 处理 @get 规则
                        infoVal.Insert(0, analyzeRule.Get(RuleParam[index]));
                    }
                    else
                    {
                        // 默认类型，直接插入
                        infoVal.Insert(0, RuleParam[index]);
                    }
                }

                Rule = infoVal.ToString();
            }

            // 分离正则表达式
            var ruleStrS = Rule.Split(new[] { "##" }, StringSplitOptions.None);
            Rule = ruleStrS[0].Trim();

            if (ruleStrS.Length > 1)
            {
                ReplaceRegex = ruleStrS[1];
            }
            if (ruleStrS.Length > 2)
            {
                Replacement = ruleStrS[2];
            }
            if (ruleStrS.Length > 3)
            {
                ReplaceFirst = true;
            }
        }

        /// <summary>
        /// 判断是否为规则
        /// </summary>
        private bool IsRule(string ruleStr)
        {
            return ruleStr.StartsWith("@") // js首个字符不可能是@，除非是装饰器，所以@开头规定为规则
                || ruleStr.StartsWith("$.")
                || ruleStr.StartsWith("$[")
                || ruleStr.StartsWith("//");
        }

        /// <summary>
        /// 获取参数数量
        /// </summary>
        internal int GetParamSize()
        {
            return RuleParam.Count;
        }


        /// <summary>
        /// 分离 put 规则
        /// </summary>
        private static string SplitPutRule(string ruleStr, Dictionary<string, string> putMap)
        {
            // 提取 @put:{key:value} 格式的规则
            return putPattern.Replace(ruleStr, match =>
            {
                var key = match.Groups[1].Value.Trim();
                var value = match.Groups[2].Value.Trim();
                putMap[key] = value;
                return "";
            });
        }
    }

    public class AnalyzeRule : JsExtensions
    {
        private IRuleData _ruleData;
        private readonly IBaseSource _source;
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

        public AnalyzeRule(IRuleData ruleData = null, IBaseSource source = null, bool preUpdateJs = false) : base(QApplication.QCreationOptions?.Configuration?.BasePath ?? "cache_path")
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
                Put("url", baseUrl);
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
                                RuleMode.Regex => AnalyzeByRegex.GetElement(result?.ToString(), rule.Split(new string[] { "&&" }, StringSplitOptions.RemoveEmptyEntries)),
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

        public object GetElement(string ruleStr)
        {
            if (string.IsNullOrEmpty(ruleStr)) return new List<object>();

            object tempResult = null;
            var content = this._content;

            var ruleList = SplitSourceRuleCacheString(ruleStr);

            if (content != null && ruleList.Count > 0)
            {
                foreach (var sourceRule in ruleList)
                {
                    tempResult = content;
                    PutRule(sourceRule.PutMap);
                    sourceRule.MakeUpRule(tempResult, this);
                    if (tempResult == null) continue;

                    var rule = sourceRule.Rule;
                    if (!string.IsNullOrEmpty(rule))
                    {
                        tempResult = sourceRule.Mode switch
                        {
                            RuleMode.Regex => AnalyzeByRegex.GetElement(tempResult?.ToString(), rule.Split(new string[] { "&&" }, StringSplitOptions.RemoveEmptyEntries)),
                            RuleMode.Js => EvalJs(rule, tempResult),
                            RuleMode.Json => new AnalyzeByJsonPath(JsonConvert.SerializeObject(tempResult)).GetList(rule),
                            RuleMode.XPath => new AnalyzeByXPath(tempResult).GetElements(rule).Cast<object>().ToList(),
                            _ => new AnalyzeByAngleSharp(tempResult).GetAllElements(rule)
                        };
                    }

                    if (tempResult != null && !string.IsNullOrEmpty(sourceRule.ReplaceRegex))
                    {
                        tempResult = ReplaceRegex(tempResult.ToString(), sourceRule);
                    }
                }
            }

            return tempResult;
        }

        public List<object> GetElements(string ruleStr)
        {
            if (string.IsNullOrEmpty(ruleStr)) return new List<object>();

            var result = new List<object>();
            var currentContent = _content;

            var ruleList = SplitSourceRuleCacheString(ruleStr);

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
                            RuleMode.Regex => AnalyzeByRegex.GetElement(tempResult?.ToString(), rule.Split(new string[] { "&&" }, StringSplitOptions.RemoveEmptyEntries)),
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
                                //RuleMode.Regex => AnalyzeByRegex.GetElement(result?.ToString(), rule.Split(new string[] { "&&" }, StringSplitOptions.RemoveEmptyEntries)),
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

            if (string.IsNullOrEmpty(resultStr))
            {
                var tempRule = string.Join("", ruleList.Select(r => r.Rule));
                if (tempRule.StartsWith("/") && !tempRule.StartsWith("//"))
                {
                    resultStr = tempRule;
                    isUrl = true;
                }
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
                _ruleData.PutVariable(key, value);
            }
            return value;
        }

        public string Get(string key)
        {
            if (_ruleData != null)
            {
                return _ruleData.GetBigVariable(key);
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
                bindingds.Add("cookie", QServiceProvider.GetService<CookieStore>());
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
                return "";
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
        public IBaseSource GetSource()
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

        public override IBaseSource getSource()
        {
            return this._source;
        }
    }


}