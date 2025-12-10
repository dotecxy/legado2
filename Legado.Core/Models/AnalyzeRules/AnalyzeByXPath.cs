using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.XPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Legado.Core.Models.AnalyzeRules
{
    /// <summary>
    /// XPath 解析类
    /// </summary>
    public class AnalyzeByXPath
    {
        private object _jxNode;

        public AnalyzeByXPath(object doc)
        {
            _jxNode = Parse(doc);
        }

        /// <summary>
        /// 解析文档对象
        /// </summary>
        private object Parse(object doc)
        {
            if (doc == null)
                return null;

            // 如果已经是 IDocument，直接返回
            if (doc is IDocument document)
                return document;

            // 如果是 IElement，返回其 Owner
            if (doc is IElement element)
                return element.Owner;

            // 如果是 INodeList，返回第一个元素的 Owner
            if (doc is IHtmlCollection<IElement> elements && elements.Length > 0)
                return elements[0].Owner;

            // 否则当作字符串解析
            return StrToDocument(doc.ToString());
        }

        /// <summary>
        /// 字符串转文档
        /// </summary>
        private IDocument StrToDocument(string html)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            var html1 = html;

            // 自动补全表格标签
            if (html1.EndsWith("</td>"))
            {
                html1 = $"<tr>{html1}</tr>";
            }
            if (html1.EndsWith("</tr>") || html1.EndsWith("</tbody>"))
            {
                html1 = $"<table>{html1}</table>";
            }

            try
            {
                // 检查是否为 XML
                if (html1.Trim().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
                {
                    var parser = new HtmlParser(new HtmlParserOptions
                    {
                        IsStrictMode = false,
                        IsEmbedded = false
                    });
                    return parser.ParseDocument(html1);
                }
            }
            catch { }

            // 默认按 HTML 解析
            var htmlParser = new HtmlParser();
            return htmlParser.ParseDocument(html1);
        }

        /// <summary>
        /// 获取 XPath 查询结果
        /// </summary>
        private List<INode> GetResult(string xPath)
        {
            if (_jxNode == null || string.IsNullOrEmpty(xPath))
                return null;

            try
            {
                if (_jxNode is IDocument doc)
                {
                    var nodes = doc.Body?.SelectNodes(xPath);
                    return nodes?.Cast<INode>().ToList();
                }
                else if (_jxNode is IElement elem)
                {
                    var nodes = elem.SelectNodes(xPath);
                    return nodes?.Cast<INode>().ToList();
                }
            }
            catch
            {
                // XPath 查询失败
            }

            return null;
        }

        /// <summary>
        /// 获取元素列表
        /// </summary>
        internal List<INode> GetElements(string xPath)
        {
            if (string.IsNullOrEmpty(xPath))
                return null;

            var jxNodes = new List<INode>();
            var ruleAnalyzes = new RuleAnalyzer(xPath);
            var rules = ruleAnalyzes.SplitRule("&&", "||", "%%");

            if (rules.Count == 1)
            {
                return GetResult(rules[0]);
            }
            else
            {
                var results = new List<List<INode>>();
                foreach (var rl in rules)
                {
                    var temp = GetElements(rl);
                    if (temp != null && temp.Count > 0)
                    {
                        results.Add(temp);
                        if (temp.Count > 0 && ruleAnalyzes.ElementsType == "||")
                        {
                            break;
                        }
                    }
                }

                if (results.Count > 0)
                {
                    if ("%%" == ruleAnalyzes.ElementsType)
                    {
                        // 交替合并
                        for (int i = 0; i < results[0].Count; i++)
                        {
                            foreach (var temp in results)
                            {
                                if (i < temp.Count)
                                {
                                    jxNodes.Add(temp[i]);
                                }
                            }
                        }
                    }
                    else
                    {
                        // 顺序合并
                        foreach (var temp in results)
                        {
                            jxNodes.AddRange(temp);
                        }
                    }
                }
            }

            return jxNodes.Count > 0 ? jxNodes : null;
        }

        /// <summary>
        /// 获取字符串列表
        /// </summary>
        internal List<string> GetStringList(string xPath)
        {
            var result = new List<string>();
            var ruleAnalyzes = new RuleAnalyzer(xPath);
            var rules = ruleAnalyzes.SplitRule("&&", "||", "%%");

            if (rules.Count == 1)
            {
                var nodes = GetResult(xPath);
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        result.Add(node.TextContent ?? "");
                    }
                }
                return result;
            }
            else
            {
                var results = new List<List<string>>();
                foreach (var rl in rules)
                {
                    var temp = GetStringList(rl);
                    if (temp.Count > 0)
                    {
                        results.Add(temp);
                        if (temp.Count > 0 && ruleAnalyzes.ElementsType == "||")
                        {
                            break;
                        }
                    }
                }

                if (results.Count > 0)
                {
                    if ("%%" == ruleAnalyzes.ElementsType)
                    {
                        // 交替合并
                        for (int i = 0; i < results[0].Count; i++)
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
                        // 顺序合并
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
        /// 获取字符串
        /// </summary>
        public string GetString(string rule)
        {
            var ruleAnalyzes = new RuleAnalyzer(rule);
            var rules = ruleAnalyzes.SplitRule("&&", "||");

            if (rules.Count == 1)
            {
                var nodes = GetResult(rule);
                if (nodes != null && nodes.Count > 0)
                {
                    return string.Join("\n", nodes.Select(n => n.TextContent ?? ""));
                }
                return null;
            }
            else
            {
                var textList = new List<string>();
                foreach (var rl in rules)
                {
                    var temp = GetString(rl);
                    if (!string.IsNullOrEmpty(temp))
                    {
                        textList.Add(temp);
                        if (ruleAnalyzes.ElementsType == "||")
                        {
                            break;
                        }
                    }
                }
                return string.Join("\n", textList);
            }
        }

        /// <summary>
        /// 获取字符串（便捷方法，小写命名）
        /// </summary>
        public string getString(string rule)
        {
            return GetString(rule);
        }
    }
}
