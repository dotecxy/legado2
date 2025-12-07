using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Legado.Core.Parsers
{
    public class JsonRuleParser
    {
        public List<string> Parse(object content, string rule, string baseUrl)
        {
            string jsonString = content as string;
            if (jsonString == null) return new List<string>();

            // 移除前缀
            if (rule.StartsWith("@json:")) rule = rule.Substring(6);

            // 处理 Legado 的替换语法 {$.variable} -> 略，需正则处理

            try
            {
                JToken root = JToken.Parse(jsonString);
                // 使用 Newtonsoft 的 SelectTokens (支持大部分 JSONPath)
                IEnumerable<JToken> tokens = root.SelectTokens(rule);

                return tokens.Select(t => t.ToString()).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}