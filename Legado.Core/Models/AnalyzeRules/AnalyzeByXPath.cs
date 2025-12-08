using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.XPath;

namespace Legado.Core.Models.AnalyzeRules
{ 
    public class AnalyzeByXPath
    {
        private readonly object _jxNode;

        public AnalyzeByXPath(object doc)
        {
            _jxNode = Parse(doc);
        }

        private object Parse(object doc)
        {
            if (doc is INode node)
            {
                return node;
            }
            else if (doc is IDocument document)
            {
                return document.DocumentElement;
            }
            else if (doc is IElement element)
            {
                return element;
            }
            else if (doc is IEnumerable<INode> nodes)
            {
                // 如果有多个节点，返回第一个
                return nodes.FirstOrDefault() ?? ParseString(doc.ToString());
            }
            else
            {
                return ParseString(doc.ToString());
            }
        }

        private INode ParseString(string html)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            var html1 = html;

            // 处理表格单元格特殊情况
            if (html1.Trim().EndsWith("</td>"))
            {
                html1 = "<tr>" + html1 + "</tr>";
            }
            if (html1.Trim().EndsWith("</tr>") || html1.Trim().EndsWith("</tbody>"))
            {
                html1 = "<table>" + html1 + "</table>";
            }

            try
            {
                if (html1.Trim().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
                {
                    // 使用XML解析器
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(html1);

                    // 将XmlDocument转换为AngleSharp文档
                    var config = Configuration.Default.WithXPath();
                    var context = BrowsingContext.New(config);
                    var parser = context.GetService<IHtmlParser>();

                    // 解析XML
                    var doc = parser.ParseDocument("<root>" + html1 + "</root>");
                    return doc.DocumentElement;
                }
            }
            catch
            {
                // 如果XML解析失败，回退到HTML解析
            }

            // 使用HTML解析器
            var htmlConfig = Configuration.Default.WithXPath();
            var htmlContext = BrowsingContext.New(htmlConfig);
            var htmlParser = htmlContext.GetService<IHtmlParser>();

            var document = htmlParser.ParseDocument(html1);
            return document.DocumentElement;
        }

        private IEnumerable<INode> GetResult(string xPath)
        {
            if (string.IsNullOrEmpty(xPath) || _jxNode == null)
                return Enumerable.Empty<INode>();

            if (_jxNode is INode node)
            {
                try
                {
                    return node.SelectNodes(xPath) ?? Enumerable.Empty<INode>();
                }
                catch (Exception ex)
                {
                    // XPath表达式可能无效
                    Console.WriteLine($"XPath error: {ex.Message}");
                    return Enumerable.Empty<INode>();
                }
            }

            return Enumerable.Empty<INode>();
        }

        public List<INode> GetElements(string xPath)
        {
            if (string.IsNullOrEmpty(xPath))
                return null;

            var jxNodes = new List<INode>();
            var ruleAnalyzer = new RuleAnalyzer(xPath);
            var rules = ruleAnalyzer.SplitRule("&&", "||", "%%");

            if (rules.Count == 1)
            {
                var result = GetResult(rules[0]);
                return result?.ToList();
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
                        if (temp.Count > 0 && ruleAnalyzer.ElementsType == "||")
                        {
                            break;
                        }
                    }
                }

                if (results.Count > 0)
                {
                    if ("%%" == ruleAnalyzer.ElementsType)
                    {
                        // 交叉组合结果
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
                        // 普通组合结果
                        foreach (var temp in results)
                        {
                            jxNodes.AddRange(temp);
                        }
                    }
                }
            }

            return jxNodes;
        }

        public List<string> GetStringList(string xPath)
        {
            var result = new List<string>();
            var ruleAnalyzer = new RuleAnalyzer(xPath);
            var rules = ruleAnalyzer.SplitRule("&&", "||", "%%");

            if (rules.Count == 1)
            {
                var nodes = GetResult(xPath);
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        result.Add(GetNodeString(node));
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
                    if (temp != null && temp.Count > 0)
                    {
                        results.Add(temp);
                        if (temp.Count > 0 && ruleAnalyzer.ElementsType == "||")
                        {
                            break;
                        }
                    }
                }

                if (results.Count > 0)
                {
                    if ("%%" == ruleAnalyzer.ElementsType)
                    {
                        // 交叉组合结果
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
                        // 普通组合结果
                        foreach (var temp in results)
                        {
                            result.AddRange(temp);
                        }
                    }
                }
            }

            return result;
        }

        public string GetString(string rule)
        {
            var ruleAnalyzer = new RuleAnalyzer(rule);
            var rules = ruleAnalyzer.SplitRule("&&", "||");

            if (rules.Count == 1)
            {
                var nodes = GetResult(rule);
                if (nodes != null && nodes.Any())
                {
                    return string.Join("\n", nodes.Select(GetNodeString));
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
                        if (ruleAnalyzer.ElementsType == "||")
                        {
                            break;
                        }
                    }
                }

                return textList.Count > 0 ? string.Join("\n", textList) : null;
            }
        }

        private string GetNodeString(INode node)
        {
            if (node == null)
                return string.Empty;

            // 根据节点类型返回不同的字符串表示
            switch (node.NodeType)
            {
                case NodeType.Element:
                    return (node as IElement)?.TextContent ?? string.Empty;

                case NodeType.Attribute:
                    return (node as IAttr)?.Value ?? string.Empty;

                case NodeType.Text:
                case NodeType.Comment:
                case NodeType.ProcessingInstruction:
                    return node.TextContent ?? string.Empty;

                default:
                    return node.ToString() ?? string.Empty;
            }
        }
    }

    // 为方便使用，创建扩展方法
    public static class XPathExtensions
    {
        /// <summary>
        /// 在节点上执行XPath查询
        /// </summary>
        public static IEnumerable<INode> SelectNodes(this INode node, string xpath)
        {
            try
            {
                return node.SelectNodes(xpath);
            }
            catch
            {
                return Enumerable.Empty<INode>();
            }
        }

        /// <summary>
        /// 在节点上执行XPath查询，返回第一个匹配的节点
        /// </summary>
        public static INode SelectSingleNode(this INode node, string xpath)
        {
            try
            {
                return node.SelectNodes(xpath).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 在元素上执行XPath查询
        /// </summary>
        public static IEnumerable<INode> SelectNodes(this IElement element, string xpath)
        {
            return (element as INode).SelectNodes(xpath);
        }

        /// <summary>
        /// 在元素上执行XPath查询，返回第一个匹配的节点
        /// </summary>
        public static INode SelectSingleNode(this IElement element, string xpath)
        {
            return (element as INode).SelectSingleNode(xpath);
        }

        /// <summary>
        /// 在文档上执行XPath查询
        /// </summary>
        public static IEnumerable<INode> SelectNodes(this IDocument document, string xpath)
        {
            return (document.DocumentElement as INode).SelectNodes(xpath);
        }

        /// <summary>
        /// 在文档上执行XPath查询，返回第一个匹配的节点
        /// </summary>
        public static INode SelectSingleNode(this IDocument document, string xpath)
        {
            return (document.DocumentElement as INode).SelectSingleNode(xpath);
        }
    }
}
