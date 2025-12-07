using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Legado.Core.Parsers
{
    public class RuleAnalyzer
    {
        // 处理 || && %% 连接符
        // 这里简化处理，先只处理单个规则，连接符逻辑需要在上层处理
        public List<string> Analyze(object content, string rule, string baseUrl = "")
        {
            if (string.IsNullOrEmpty(rule)) return new List<string>();

            // 1. 处理 JS 规则 (@js: 或 <js>)
            // 需引入 Jint 等引擎，此处略过具体实现，假设 rule 已经剥离了 js

            // 2. 识别前缀并分发
            if (rule.StartsWith("@css:"))
            {
                return new CssRuleParser().Parse(content, rule.Substring(5), baseUrl);
            }
            else if (rule.StartsWith("@xpath:") || rule.StartsWith("//"))
            {
                // AngleSharp.XPath 扩展或 HtmlAgilityPack
                return new XPathRuleParser().Parse(content, rule, baseUrl);
            }
            else if (rule.StartsWith("@json:") || rule.StartsWith("$."))
            {
                return new JsonRuleParser().Parse(content, rule, baseUrl);
            }
            else
            {
                // 默认为 Jsoup Default 规则 (class.0@tag.a@text)
                return new JsoupDefaultParser().Parse(content, rule, baseUrl);
            }
        }
    }
}