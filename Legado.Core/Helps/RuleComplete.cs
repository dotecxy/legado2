using System.Text.RegularExpressions;

namespace Legado.Core.Helps
{
    /// <summary>
    /// 规则自动补全工具
    /// 对简单规则进行补全，简化部分书源规则的编写
    /// 对JSOUP/XPath/CSS规则生效
    /// </summary>
    public static class RuleComplete
    {
        // 需要补全的规则模式
        private static readonly Regex NeedComplete = new Regex(
            @"(?<!(@|/|^|[|%&]{2})(attr|text|ownText|textNodes|href|content|html|alt|all|value|src)(\(\))?)(?<seq>\&{2}|%%|\|{2}|$)",
            RegexOptions.Compiled
        );

        // 不能补全的规则模式（存在js/json/{{xx}}的复杂情况）
        private static readonly Regex NotComplete = new Regex(
            @"^:|^##|\{\{|@js:|<js>|@Json:|\$\.",
            RegexOptions.Compiled
        );

        // 修正从图片获取信息的规则
        private static readonly Regex FixImgInfo = new Regex(
            @"(?<=(^|tag\.|[\+/@>~| &]))img(?<at>(\[@?.+\]|\.[-\w]+)?)[@/]+text(\(\))?(?<seq>\&{2}|%%|\|{2}|$)",
            RegexOptions.Compiled
        );

        // 判断是否为XPath规则
        private static readonly Regex IsXpath = new Regex(
            @"^//|^@Xpath:",
            RegexOptions.Compiled
        );

        // 尾部分割符（##或,{）
        private static readonly Regex TailSplit = new Regex(
            @"##|,\{",
            RegexOptions.Compiled
        );

        /// <summary>
        /// 对简单规则进行自动补全
        /// </summary>
        /// <param name="rules">需要补全的规则</param>
        /// <param name="preRule">预处理规则或列表规则</param>
        /// <param name="type">补全结果的类型，可选的值有: 1=文字(默认) 2=链接 3=图片</param>
        /// <returns>补全后的规则，如果不需要补全则返回原规则</returns>
        public static string AutoComplete(string rules, string preRule = null, int type = 1)
        {
            // 规则为空或包含不能补全的模式
            if (string.IsNullOrEmpty(rules) || NotComplete.IsMatch(rules))
                return rules;

            // 预处理规则包含不能补全的模式
            if (!string.IsNullOrEmpty(preRule) && NotComplete.IsMatch(preRule))
                return rules;

            // 分离尾部规则（##分割的正则或,{分割的参数）
            string tailStr = "";
            string cleanedRule = rules;

            var splitMatch = TailSplit.Match(rules);
            if (splitMatch.Success)
            {
                var splitIndex = splitMatch.Index;
                cleanedRule = rules.Substring(0, splitIndex);
                tailStr = rules.Substring(splitIndex);
            }

            // 根据规则类型确定补全模板
            string textRule;
            string linkRule;
            string imgRule;
            string imgText;

            if (IsXpath.IsMatch(cleanedRule))
            {
                // XPath规则
                textRule = "//text()${seq}";
                linkRule = "//@href${seq}";
                imgRule = "//@src${seq}";
                imgText = "img${at}/@alt${seq}";
            }
            else
            {
                // JSOUP/CSS规则
                textRule = "@text${seq}";
                linkRule = "@href${seq}";
                imgRule = "@src${seq}";
                imgText = "img${at}@alt${seq}";
            }

            // 执行补全
            string result;
            switch (type)
            {
                case 1: // 文字
                    result = NeedComplete.Replace(cleanedRule, m => 
                        textRule.Replace("${seq}", m.Groups["seq"].Value));
                    result = FixImgInfo.Replace(result, m =>
                        imgText.Replace("${at}", m.Groups["at"].Value)
                               .Replace("${seq}", m.Groups["seq"].Value));
                    break;

                case 2: // 链接
                    result = NeedComplete.Replace(cleanedRule, m => 
                        linkRule.Replace("${seq}", m.Groups["seq"].Value));
                    break;

                case 3: // 图片
                    result = NeedComplete.Replace(cleanedRule, m => 
                        imgRule.Replace("${seq}", m.Groups["seq"].Value));
                    break;

                default:
                    return rules;
            }

            return result + tailStr;
        }
    }
}
