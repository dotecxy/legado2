using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Legado.Core.Parsers
{
    /// <summary>
    /// 移植 Legado 的 Default (Jsoup) 规则
    /// 语法: class.name.index@tag.name.index
    /// </summary>
    public class JsoupDefaultParser
    {
        public List<string> Parse(object content, string rule, string baseUrl)
        {
            var doc = content as IDocument;
            var elements = content as IHtmlCollection<IElement>;
            IElement singleElement = content as IElement;

            // 统一转为 List<IElement> 作为起始上下文
            var context = new List<IElement>();
            if (doc != null) context.Add(doc.Body);
            else if (elements != null) context.AddRange(elements);
            else if (singleElement != null) context.Add(singleElement);

            // 预处理正则替换 (##regex##replacement)
            string regexPattern = "";
            string regexReplacement = "";
            if (rule.Contains("##"))
            {
                var parts = rule.Split(new[] { "##" }, StringSplitOptions.None);
                rule = parts[0];
                if (parts.Length > 1) regexPattern = parts[1];
                if (parts.Length > 2) regexReplacement = parts[2];
            }

            // 分割规则 @
            var segments = rule.Split('@');
            var results = new List<string>();

            // 如果规则为空，返回当前文本
            if (segments.Length == 0) return GetTextList(context);

            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                bool isLast = i == segments.Length - 1;

                // 检查是否是获取内容的指令 (text, href, src, html 等)
                if (isLast && IsContentInstruction(segment))
                {
                    results.AddRange(ExtractContent(context, segment, baseUrl));
                }
                else
                {
                    // 解析选择器: class.name.index 或 tag.div[0:1]
                    context = ApplySelector(context, segment);
                }
            }

            // 如果最后一步不是内容指令，默认取 text
            if (results.Count == 0 && context.Count > 0)
            {
                // 此时如果没有明确指定@text，通常返回空或者继续处理，视具体逻辑而定
                // 简单的实现：
                results.AddRange(context.Select(e => e.TextContent));
            }

            // 执行正则净化
            if (!string.IsNullOrEmpty(regexPattern))
            {
                for (int k = 0; k < results.Count; k++)
                {
                    results[k] = Regex.Replace(results[k], regexPattern, regexReplacement);
                }
            }

            return results;
        }

        private bool IsContentInstruction(string segment)
        {
            var s = segment.ToLower();
            return s == "text" || s == "textnodes" || s == "owntext" ||
                   s == "href" || s == "src" || s == "html" || s == "all";
        }

        private List<string> ExtractContent(List<IElement> elements, string type, string baseUrl)
        {
            var list = new List<string>();
            foreach (var el in elements)
            {
                switch (type.ToLower())
                {
                    case "text": list.Add(el.TextContent.Trim()); break;
                    case "html": list.Add(el.InnerHtml); break;
                    case "href":
                        var href = el.GetAttribute("href");
                        // 这里需要处理相对路径转绝对路径 logic
                        list.Add(FormatUrl(href, baseUrl));
                        break;
                    case "src":
                        var src = el.GetAttribute("src");
                        list.Add(FormatUrl(src, baseUrl));
                        break;
                    // ... 其他 textNodes 等实现略 ...
                    default: list.Add(el.TextContent); break;
                }
            }
            return list;
        }

        private string FormatUrl(string url, string baseUrl)
        {
            if (string.IsNullOrEmpty(url)) return "";
            if (url.StartsWith("http")) return url;
            try { return new Uri(new Uri(baseUrl), url).ToString(); }
            catch { return url; }
        }

        // 核心：解析单个片段 e.g. "class.odd.0" or "tag.div[-1]"
        private List<IElement> ApplySelector(List<IElement> input, string segment)
        {
            var output = new List<IElement>();

            // 简单的解析逻辑：按 . 分割
            // 注意：Legado 支持更复杂的写法，如 text.包含文字
            var parts = segment.Split('.');
            string type = parts[0];
            string name = parts.Length > 1 ? parts[1] : "";
            string indexRule = parts.Length > 2 ? parts[2] : "";

            // 支持类似数组的写法 tag.div[0] -> 提取出 indexRule
            if (segment.Contains("["))
            {
                // 正则提取 [] 内的内容并覆盖 indexRule logic
                // 暂时略过正则实现，假设已解析
            }

            foreach (var parent in input)
            {
                IEnumerable<IElement> found = null;

                // 1. 根据类型查找
                switch (type)
                {
                    case "class":
                        found = parent.QuerySelectorAll($".{name}");
                        break;
                    case "id":
                        found = parent.QuerySelectorAll($"#{name}");
                        break;
                    case "tag":
                        found = parent.QuerySelectorAll(name);
                        break;
                    case "text":
                        // 特殊: 根据文本内容筛选
                        found = parent.QuerySelectorAll("*").Where(e => e.TextContent.Contains(name));
                        break;
                    case "children":
                        found = parent.Children;
                        break;
                    default:
                        // 尝试作为 tag 处理
                        found = parent.QuerySelectorAll(type);
                        break;
                }

                if (found != null)
                {
                    // 2. 根据 indexRule 筛选 (0, -1, 1:3, !0)
                    var filtered = FilterByIndex(found.ToList(), indexRule);
                    output.AddRange(filtered);
                }
            }

            return output;
        }

        // 处理 Legado 复杂的索引逻辑 (!, -, :)
        private List<IElement> FilterByIndex(List<IElement> list, string rule)
        {
            if (string.IsNullOrEmpty(rule)) return list;
            if (list.Count == 0) return list;

            var result = new List<IElement>();

            // 处理排除 !
            bool exclude = rule.StartsWith("!");
            if (exclude) rule = rule.Substring(1);

            // 处理单个索引或切片 (此处需要根据 Legado 规则完善解析逻辑)
            // 简单实现：处理单个数字
            if (int.TryParse(rule, out int idx))
            {
                int realIndex = idx >= 0 ? idx : list.Count + idx;
                if (realIndex >= 0 && realIndex < list.Count)
                {
                    if (!exclude) result.Add(list[realIndex]);
                    else
                    {
                        list.RemoveAt(realIndex);
                        result = list;
                    }
                }
            }
            else
            {
                // TODO: 实现切片逻辑 : 以及多索引 1,2,3
                // 遇到非数字通常意味着不是索引，而是其他规则，但在 Default 规则中 .第三段 通常就是索引
                return list; // 暂时返回全部
            }

            return result;
        }
    }
}