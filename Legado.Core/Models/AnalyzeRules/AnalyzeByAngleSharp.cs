using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.XPath;

namespace Legado.Core.Models.AnalyzeRules
{
    /// <summary>
    /// 使用 AngleSharp 分析规则 
    /// 对应 Kotlin: AnalyzeByJSoup.kt
    /// </summary>
    public class AnalyzeByAngleSharp
    {
        private readonly IElement _element;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="doc">HTML 字符串或 DOM 元素</param>
        public AnalyzeByAngleSharp(object doc)
        {
            _element = Parse(doc);
        }

        /// <summary>
        /// 获取当前元素
        /// </summary>
        public IElement GetElement()
        {
            return _element;
        }

        /// <summary>
        /// 解析 HTML
        /// </summary>
        private static IElement Parse(object doc)
        {
            if (doc is IElement element)
                return element;

            var html = doc?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(html))
                return null;

            // 检查是否是 XML
            if (html.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            {
                var parser = new HtmlParser(new HtmlParserOptions
                {
                    IsNotConsumingCharacterReferences = true
                });
                var document = parser.ParseDocument(html);
                return document.DocumentElement ?? document.Body;
            }
            else
            {
                // 解析 HTML
                var context = BrowsingContext.New(Configuration.Default);
                var parser = context.GetService<IHtmlParser>();
                var document = parser.ParseDocument(html);

                return document.Body ?? document.DocumentElement;
            }
        }

        /// <summary>
        /// 获取元素列表
        /// </summary>
        public List<IElement> GetElements(string rule)
        {
            return GetElements(_element, rule);
        }

        /// <summary>
        /// 获取所有元素（对应 getAllElements）
        /// </summary>
        public List<object> GetAllElements(string rule)
        {
            var elements = GetElements(rule);
            return elements.Cast<object>().ToList();
        }

        /// <summary>
        /// 合并内容列表，得到内容
        /// </summary>
        public string GetString(string ruleStr)
        {
            if (string.IsNullOrEmpty(ruleStr))
                return null;

            var list = GetStringList(ruleStr);
            if (list.Count == 0)
                return null;

            if (list.Count == 1)
                return list[0];

            return string.Join("\n", list);
        }

        /// <summary>
        /// 获取第一个字符串
        /// </summary>
        public string GetString0(string ruleStr)
        {
            var list = GetStringList(ruleStr);
            return list.Count > 0 ? list[0] : string.Empty;
        }

        /// <summary>
        /// 获取所有字符串列表（对应 getStringList）
        /// </summary>
        public List<string> GetStringList(string ruleStr)
        {
            return GetStringList(ruleStr, false);
        }

        /// <summary>
        /// 获取所有字符串列表（带是否保留Html参数）
        /// </summary>
        public List<string> GetStringList(string ruleStr, bool isUrl)
        {
            var textS = new List<string>();

            if (string.IsNullOrEmpty(ruleStr))
                return textS;

            // 拆分规则
            var sourceRule = new SourceRule(ruleStr);

            if (string.IsNullOrEmpty(sourceRule.Rule))
            {
                // 如果没有元素规则，直接返回元素数据
                textS.Add(_element.TextContent ?? string.Empty);
            }
            else
            {
                var ruleAnalyzer = new RuleAnalyzer(sourceRule.Rule);
                var ruleStrS = ruleAnalyzer.SplitRule("&&", "||", "%%");

                var results = new List<List<string>>();
                foreach (var ruleStrX in ruleStrS)
                {
                    List<string> temp = null;

                    if (sourceRule.Mode == RuleMode.Default)
                    {
                        var lastIndex = ruleStrX.LastIndexOf('@');
                        if (lastIndex >= 0)
                        {
                            var cssSelector = ruleStrX.Substring(0, lastIndex);
                            var attrName = ruleStrX.Substring(lastIndex + 1);

                            var parts = cssSelector.Split('@');
                            List<IElement> elements2 = new List<IElement>();
                            foreach (var part in parts)
                            {
                                elements2 = new ElementsSingle().GetElementsSingle(_element, part).ToList();
                            } 
                            temp = GetResultLast(elements2, attrName);
                        }
                    }
                    else
                    {
                        temp = GetResultList(ruleStrX);
                    }

                    if (temp != null && temp.Count > 0)
                    {
                        results.Add(temp);
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
                                    textS.Add(temp[i]);
                                }
                            }
                        }
                    }
                    else
                    {
                        // 合并所有结果
                        foreach (var temp in results)
                        {
                            textS.AddRange(temp);
                        }
                    }
                }
            }

            return textS;
        }

        /// <summary>
        /// 获取 HTML 列表（对应 Kotlin 的 getHtmlList）
        /// </summary>
        public List<string> GetHtmlList(string ruleStr)
        {
            var htmlList = new List<string>();
            
            if (string.IsNullOrEmpty(ruleStr))
                return htmlList;

            var elements = GetElements(ruleStr);
            foreach (var element in elements)
            {
                if (element != null)
                {
                    htmlList.Add(element.OuterHtml);
                }
            }

            return htmlList;
        }

        /// <summary>
        /// 获取单个 HTML（对应 Kotlin 的 getHtml）
        /// </summary>
        public string GetHtml(string ruleStr)
        {
            var htmlList = GetHtmlList(ruleStr);
            if (htmlList.Count == 0)
                return null;
            
            if (htmlList.Count == 1)
                return htmlList[0];
            
            return string.Join("\n", htmlList);
        }

        /// <summary>
        /// 获取第一个 HTML（对应 Kotlin 的 getHtml0）
        /// </summary>
        public string GetHtml0(string ruleStr)
        {
            var htmlList = GetHtmlList(ruleStr);
            return htmlList.Count > 0 ? htmlList[0] : string.Empty;
        }

        /// <summary>
        /// 获取属性列表（对应 Kotlin 的 getAttributeList）
        /// </summary>
        public List<string> GetAttributeList(string ruleStr, string attrName)
        {
            var attrList = new List<string>();
            
            if (string.IsNullOrEmpty(ruleStr) || string.IsNullOrEmpty(attrName))
                return attrList;

            var elements = GetElements(ruleStr);
            foreach (var element in elements)
            {
                if (element != null)
                {
                    var attrValue = element.GetAttribute(attrName);
                    if (!string.IsNullOrEmpty(attrValue))
                    {
                        attrList.Add(attrValue);
                    }
                }
            }

            return attrList;
        }

        /// <summary>
        /// 获取单个属性（对应 Kotlin 的 getAttribute）
        /// </summary>
        public string GetAttribute(string ruleStr, string attrName)
        {
            var attrList = GetAttributeList(ruleStr, attrName);
            if (attrList.Count == 0)
                return null;
            
            if (attrList.Count == 1)
                return attrList[0];
            
            return string.Join("\n", attrList);
        }

        /// <summary>
        /// 获取第一个属性（对应 Kotlin 的 getAttribute0）
        /// </summary>
        public string GetAttribute0(string ruleStr, string attrName)
        {
            var attrList = GetAttributeList(ruleStr, attrName);
            return attrList.Count > 0 ? attrList[0] : string.Empty;
        }

        /// <summary>
        /// 获取文本列表（对应 Kotlin 的 getTextList）
        /// </summary>
        public List<string> GetTextList(string ruleStr)
        {
            var textList = new List<string>();
            
            if (string.IsNullOrEmpty(ruleStr))
                return textList;

            var elements = GetElements(ruleStr);
            foreach (var element in elements)
            {
                if (element != null)
                {
                    var text = element.TextContent?.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        textList.Add(text);
                    }
                }
            }

            return textList;
        }

        /// <summary>
        /// 获取单个文本（对应 Kotlin 的 getText）
        /// </summary>
        public string GetText(string ruleStr)
        {
            var textList = GetTextList(ruleStr);
            if (textList.Count == 0)
                return null;
            
            if (textList.Count == 1)
                return textList[0];
            
            return string.Join("\n", textList);
        }

        /// <summary>
        /// 获取第一个文本（对应 Kotlin 的 getText0）
        /// </summary>
        public string GetText0(string ruleStr)
        {
            var textList = GetTextList(ruleStr);
            return textList.Count > 0 ? textList[0] : string.Empty;
        }

        /// <summary>
        /// 获取 OwnText 列表（对应 Kotlin 的 getOwnTextList）
        /// </summary>
        public List<string> GetOwnTextList(string ruleStr)
        {
            var textList = new List<string>();
            
            if (string.IsNullOrEmpty(ruleStr))
                return textList;

            var elements = GetElements(ruleStr);
            foreach (var element in elements)
            {
                if (element != null)
                {
                    var childrenText = element.ChildNodes
                        .OfType<IText>()
                        .Select(n => n.Text?.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();

                    if (childrenText.Count > 0)
                    {
                        textList.Add(string.Join(" ", childrenText));
                    }
                }
            }

            return textList;
        }

        /// <summary>
        /// 获取单个 OwnText（对应 Kotlin 的 getOwnText）
        /// </summary>
        public string GetOwnText(string ruleStr)
        {
            var textList = GetOwnTextList(ruleStr);
            if (textList.Count == 0)
                return null;
            
            if (textList.Count == 1)
                return textList[0];
            
            return string.Join("\n", textList);
        }

        /// <summary>
        /// 获取第一个 OwnText（对应 Kotlin 的 getOwnText0）
        /// </summary>
        public string GetOwnText0(string ruleStr)
        {
            var textList = GetOwnTextList(ruleStr);
            return textList.Count > 0 ? textList[0] : string.Empty;
        }

        /// <summary>
        /// 获取 TextNodes 列表（对应 Kotlin 的 getTextNodesList）
        /// </summary>
        public List<string> GetTextNodesList(string ruleStr)
        {
            var textList = new List<string>();
            
            if (string.IsNullOrEmpty(ruleStr))
                return textList;

            var elements = GetElements(ruleStr);
            foreach (var element in elements)
            {
                if (element != null)
                {
                    var textNodes = element.ChildNodes.OfType<IText>().ToList();
                    var nodeTexts = new List<string>();
                    
                    foreach (var node in textNodes)
                    {
                        var text = node.Text?.Trim();
                        if (!string.IsNullOrEmpty(text))
                        {
                            nodeTexts.Add(text);
                        }
                    }
                    
                    if (nodeTexts.Count > 0)
                    {
                        textList.Add(string.Join("\n", nodeTexts));
                    }
                }
            }

            return textList;
        }

        /// <summary>
        /// 获取单个 TextNodes（对应 Kotlin 的 getTextNodes）
        /// </summary>
        public string GetTextNodes(string ruleStr)
        {
            var textList = GetTextNodesList(ruleStr);
            if (textList.Count == 0)
                return null;
            
            if (textList.Count == 1)
                return textList[0];
            
            return string.Join("\n", textList);
        }

        /// <summary>
        /// 获取第一个 TextNodes（对应 Kotlin 的 getTextNodes0）
        /// </summary>
        public string GetTextNodes0(string ruleStr)
        {
            var textList = GetTextNodesList(ruleStr);
            return textList.Count > 0 ? textList[0] : string.Empty;
        }

        /// <summary>
        /// 获取 All Inner Html 列表（对应 Kotlin 的 getAllInnerHtmlList）
        /// </summary>
        public List<string> GetAllInnerHtmlList(string ruleStr)
        {
            var htmlList = new List<string>();
            
            if (string.IsNullOrEmpty(ruleStr))
                return htmlList;

            var elements = GetElements(ruleStr);
            foreach (var element in elements)
            {
                if (element != null)
                {
                    htmlList.Add(element.InnerHtml);
                }
            }

            return htmlList;
        }

        /// <summary>
        /// 获取单个 All Inner Html（对应 Kotlin 的 getAllInnerHtml）
        /// </summary>
        public string GetAllInnerHtml(string ruleStr)
        {
            var htmlList = GetAllInnerHtmlList(ruleStr);
            if (htmlList.Count == 0)
                return null;
            
            if (htmlList.Count == 1)
                return htmlList[0];
            
            return string.Join("\n", htmlList);
        }

        /// <summary>
        /// 获取第一个 All Inner Html（对应 Kotlin 的 getAllInnerHtml0）
        /// </summary>
        public string GetAllInnerHtml0(string ruleStr)
        {
            var htmlList = GetAllInnerHtmlList(ruleStr);
            return htmlList.Count > 0 ? htmlList[0] : string.Empty;
        }

        /// <summary>
        /// 获取元素
        /// </summary>
        private List<IElement> GetElements(IElement temp, string rule)
        {
            if (temp == null || string.IsNullOrEmpty(rule))
                return new List<IElement>();

            var elements = new List<IElement>();
            var sourceRule = new SourceRule(rule);
            var ruleAnalyzer = new RuleAnalyzer(sourceRule.Rule);
            var ruleStrS = ruleAnalyzer.SplitRule("&&", "||", "%%");

            var elementsList = new List<IElement>();

            if (sourceRule.Mode== RuleMode.Default)
            {
                foreach (var ruleStr in ruleStrS)
                {
                    var tempS = new ElementsSingle().GetElementsSingle(temp, ruleStr);
                    elementsList.AddRange(tempS);

                    if (tempS.Count > 0 && ruleAnalyzer.ElementsType == "||")
                        break;
                }
            }
            else
            {
                foreach (var ruleStr in ruleStrS)
                {
                    var rsRule = new RuleAnalyzer(ruleStr);
                    rsRule.Trim();  // 修剪当前规则之前的"@"或者空白符

                    var rs = rsRule.SplitRule("@");
                    List<IElement> el;

                    if (rs.Count > 1)
                    {
                        var elList = new List<IElement>();
                        elList.Add(temp);

                        foreach (var rl in rs)
                        {
                            var es = new List<IElement>();
                            foreach (var et in elList.OfType<IElement>())
                            {
                                es.AddRange(GetElements(et, rl));
                            }
                            elList.Clear();
                            elList.AddRange(es);
                        }
                        el = elList;
                    }
                    else
                    {
                        el = new ElementsSingle().GetElementsSingle(temp, ruleStr);
                    }

                    elementsList.AddRange(el);

                    if (el.Count > 0 && ruleAnalyzer.ElementsType == "||")
                        break;
                }
            }

            if (elementsList.Count > 0)
            {
                if (ruleAnalyzer.ElementsType == "%%")
                {
                    // 交叉组合
                    var firstList = elementsList[0]; 
                    for (int i = 0; i < firstList.ChildElementCount; i++)
                    {
                        foreach (var es in elementsList)
                        {
                            if (i < es.ChildElementCount)
                            {
                                elements.Add(es);
                            }
                        }
                    }
                }
                else
                {
                    // 合并所有
                    foreach (var es in elementsList)
                    {
                        elements.Add(es);
                    }
                }
            }

            return elements;
        }

        /// <summary>
        /// 获取内容列表
        /// </summary>
        private List<string> GetResultList(string ruleStr)
        {
            if (string.IsNullOrEmpty(ruleStr))
                return null;

            var elements = new List<IElement>();
            elements.Add(_element);

            var rule = new RuleAnalyzer(ruleStr);
            rule.Trim();

            var rules = rule.SplitRule("@");
            var last = rules.Count - 1;

            for (int i = 0; i < last; i++)
            {
                var es = new List<IElement>();
                foreach (var elt in elements.OfType<IElement>())
                {
                    es.AddRange(new ElementsSingle().GetElementsSingle(elt, rules[i]));
                }
                elements.Clear();
                elements = es;
            }

            return elements.Count > 0 ? GetResultLast(elements, rules[last]) : null;
        }

        /// <summary>
        /// 根据最后一个规则获取内容
        /// </summary>
        private List<string> GetResultLast(List<IElement> elements, string lastRule)
        {
            var textS = new List<string>();

            switch (lastRule.ToLower())
            {
                case "text":
                    foreach (var element in elements.OfType<IElement>())
                    {
                        var text = element.TextContent?.Trim();
                        if (!string.IsNullOrEmpty(text))
                        {
                            textS.Add(text);
                        }
                    }
                    break;

                case "textnodes":
                    foreach (var element in elements.OfType<IElement>())
                    {
                        var tn = new List<string>();
                        var textNodes = element.ChildNodes.OfType<IText>().ToList();
                        foreach (var node in textNodes)
                        {
                            var text = node.Text?.Trim();
                            if (!string.IsNullOrEmpty(text))
                            {
                                tn.Add(text);
                            }
                        }
                        if (tn.Count > 0)
                        {
                            textS.Add(string.Join("\n", tn));
                        }
                    }
                    break;

                case "owntext":
                    foreach (var element in elements.OfType<IElement>())
                    {
                        // AngleSharp 没有直接的 OwnText 属性，我们可以近似实现
                        var childrenText = element.ChildNodes
                            .OfType<IText>()
                            .Select(n => n.Text?.Trim())
                            .Where(t => !string.IsNullOrEmpty(t))
                            .ToList();

                        if (childrenText.Count > 0)
                        {
                            textS.Add(string.Join(" ", childrenText));
                        }
                    }
                    break;

                case "html":
                    foreach (var element in elements.OfType<IElement>())
                    {
                        // 移除 script 和 style 标签
                        foreach (var script in element.QuerySelectorAll("script").ToList())
                        {
                            script.Remove();
                        }
                        foreach (var style in element.QuerySelectorAll("style").ToList())
                        {
                            style.Remove();
                        }

                        var html = element.OuterHtml;
                        if (!string.IsNullOrEmpty(html))
                        {
                            textS.Add(html);
                        }
                    }
                    break;

                case "all":
                    foreach (var element in elements.OfType<IElement>())
                    {
                        textS.Add(element.OuterHtml);
                    }
                    break;

                default:
                    // 默认为属性
                    foreach (var element in elements.OfType<IElement>())
                    {
                        var attrValue = element.GetAttribute(lastRule);
                        if (!string.IsNullOrEmpty(attrValue) && !textS.Contains(attrValue))
                        {
                            textS.Add(attrValue);
                        }
                    }
                    break;
            }

            return textS;
        }
    } 

    /// <summary>
    /// 元素选择器
    /// 支持多种索引和筛选方式
    /// </summary>
    internal class ElementsSingle
    {
        private char _split = '.';
        private string _beforeRule = "";
        private readonly List<int> _indexDefault = new List<int>();
        private readonly List<object> _indexes = new List<object>();

        /// <summary>
        /// 获取单个规则对应的元素
        /// </summary>
        public List<IElement> GetElementsSingle(IElement temp, string rule)
        {
            FindIndexSet(rule);

            // 获取所有元素
            List<IElement> elements;

            if (string.IsNullOrEmpty(_beforeRule))
            {
                // 允许索引直接作为根元素
                elements = new List<IElement>();
            }
            else
            {
                var rules = _beforeRule.Split('.');
                switch (rules[0].ToLower())
                {
                    case "children":
                        elements = new List<IElement>(temp.Children);
                        break;

                    case "class":
                        elements = temp.QuerySelectorAll($".{rules[1]}").ToList();
                        break;

                    case "tag":
                        elements = temp.QuerySelectorAll(rules[1]).ToList();
                        break;

                    case "id":
                        elements = temp.QuerySelectorAll($"#{rules[1]}").ToList();
                        break;

                    case "text":
                        // AngleSharp 没有直接的文本包含选择器，我们可以通过 LINQ 实现
                        var matchingElements = temp.Children
                            .Where(child =>
                                child.TextContent?.Contains(rules[1]) == true)
                            .ToList();
                        elements = new List<IElement>(matchingElements);
                        break;

                    default:
                        // 尝试作为 CSS 选择器
                        elements = temp.QuerySelectorAll(_beforeRule).ToList();
                        break;
                }
            }

            var len = elements.Count;
            var lastIndexes = Math.Max(_indexDefault.Count, _indexes.Count) - 1;
            var indexSet = new HashSet<int>();

            // 获取无重且不越界的索引集合
            if (_indexes.Count == 0)
            {
                for (int ix = _indexDefault.Count - 1; ix >= 0; ix--)
                {
                    var it = _indexDefault[ix];
                    AddIndexToSet(indexSet, it, len);
                }
            }
            else
            {
                for (int ix = _indexes.Count - 1; ix >= 0; ix--)
                {
                    var item = _indexes[ix];

                    if (item is Triple<int?, int?, int> range)
                    {
                        // 处理区间
                        var (startX, endX, stepX) = range;
                        var start = startX ?? 0;
                        var end = endX ?? (len - 1);

                        if (start < 0) start += len;
                        if (end < 0) end += len;

                        if ((start < 0 && end < 0) || (start >= len && end >= len))
                            continue;

                        if (start >= len) start = len - 1;
                        else if (start < 0) start = 0;

                        if (end >= len) end = len - 1;
                        else if (end < 0) end = 0;

                        if (start == end || Math.Abs(stepX) >= len)
                        {
                            indexSet.Add(start);
                            continue;
                        }

                        var step = stepX > 0 ? stepX : (Math.Abs(stepX) < len ? stepX + len : 1);

                        if (end > start)
                        {
                            for (int i = start; i <= end; i += Math.Abs(step))
                            {
                                indexSet.Add(i);
                            }
                        }
                        else
                        {
                            for (int i = start; i >= end; i -= Math.Abs(step))
                            {
                                indexSet.Add(i);
                            }
                        }
                    }
                    else if (item is int singleIndex)
                    {
                        AddIndexToSet(indexSet, singleIndex, len);
                    }
                }
            }

            // 根据索引集合筛选元素
            if (_split == '!')
            {
                // 排除模式
                var result = new List<IElement>();
                for (int i = 0; i < len; i++)
                {
                    if (!indexSet.Contains(i))
                    {
                        result.Add(elements[i]);
                    }
                }
                return result;
            }
            else if (_split == '.')
            {
                // 选择模式
                var result = new List<IElement>();
                foreach (var idx in indexSet.OrderBy(i => i))
                {
                    if (idx >= 0 && idx < len)
                    {
                        result.Add(elements[idx]);
                    }
                }
                return result;
            }

            return elements;
        }

        /// <summary>
        /// 添加索引到集合
        /// </summary>
        private void AddIndexToSet(HashSet<int> set, int index, int length)
        {
            if (index >= 0 && index < length)
            {
                set.Add(index);
            }
            else if (index < 0 && length >= Math.Abs(index))
            {
                set.Add(index + length);
            }
        }

        /// <summary>
        /// 查找索引集合
        /// </summary>
        private void FindIndexSet(string rule)
        {
            var rus = rule.Trim();
            var len = rus.Length;
            int? curInt = null;
            var curMinus = false;
            var curList = new List<int?>();
            var l = "";

            var head = rus.EndsWith("]");

            if (head)
            {
                len--; // 跳过尾部 ']'

                while (len-- >= 0)
                {
                    var rl = rus[len];
                    if (rl == ' ') continue;

                    if (char.IsDigit(rl))
                    {
                        l = rl + l;
                    }
                    else if (rl == '-')
                    {
                        curMinus = true;
                    }
                    else
                    {
                        curInt = ParseInt(l, curMinus);

                        switch (rl)
                        {
                            case ':':
                                curList.Add(curInt);
                                break;

                            default:
                                if (curList.Count == 0)
                                {
                                    if (curInt == null) break;
                                    _indexes.Add(curInt.Value);
                                }
                                else
                                {
                                    var triple = new Triple<int?, int?, int>(
                                        curInt,
                                        curList.LastOrDefault(),
                                        curList.Count == 2 ? curList.FirstOrDefault() ?? 1 : 1
                                    );
                                    _indexes.Add(triple);
                                    curList.Clear();
                                }

                                if (rl == '!')
                                {
                                    _split = '!';
                                    while (len > 0 && rus[--len] == ' ') { }
                                }

                                if (rl == '[')
                                {
                                    _beforeRule = rus.Substring(0, len);
                                    return;
                                }

                                if (rl != ',') break;
                                break;
                        }

                        l = "";
                        curMinus = false;
                    }
                }
            }
            else
            {
                while (len-- >= 0)
                {
                    var rl = rus[len];
                    if (rl == ' ') continue;

                    if (char.IsDigit(rl))
                    {
                        l = rl + l;
                    }
                    else if (rl == '-')
                    {
                        curMinus = true;
                    }
                    else
                    {
                        if (rl == '!' || rl == '.' || rl == ':')
                        {
                            var value = ParseInt(l, curMinus);
                            if (value.HasValue)
                            {
                                _indexDefault.Add(value.Value);
                            }

                            if (rl != ':')
                            {
                                _split = rl;
                                _beforeRule = rus.Substring(0, len);
                                return;
                            }
                        }
                        else
                        {
                            break;
                        }

                        l = "";
                        curMinus = false;
                    }
                }
            }

            _split = ' ';
            _beforeRule = rus;
        }

        private int? ParseInt(string str, bool isMinus)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            if (int.TryParse(str, out var result))
            {
                return isMinus ? -result : result;
            }

            return null;
        }
    }

    /// <summary>
    /// 三元组，用于表示区间
    /// </summary>
    internal class Triple<T1, T2, T3>
    {
        public T1 First { get; }
        public T2 Second { get; }
        public T3 Third { get; }

        public Triple(T1 first, T2 second, T3 third)
        {
            First = first;
            Second = second;
            Third = third;
        }

        public void Deconstruct(out T1 first, out T2 second, out T3 third)
        {
            first = First;
            second = Second;
            third = Third;
        }
    }

}