using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using System.Collections.Generic;
using System.Linq;

namespace Legado.Core.Parsers
{
    public class CssRuleParser
    {
        public List<string> Parse(object content, string rule, string baseUrl)
        {
            // rule 格式: @css:div.content a@href
            // 需要分割选择器和获取属性
            var parts = rule.Split('@');
            string selector = parts[0];
            string attr = parts.Length > 1 ? parts[1] : "text";

            var context = content as IElement; // 或者 Document
            if (context == null) return new List<string>();

            var elements = context.QuerySelectorAll(selector);
            var results = new List<string>();

            foreach (var el in elements)
            {
                if (attr == "text") results.Add(el.TextContent);
                else if (attr == "html") results.Add(el.InnerHtml);
                else results.Add(el.GetAttribute(attr) ?? "");
            }

            return results;
        }
    }
}