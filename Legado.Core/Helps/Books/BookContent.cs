using Legado.Core.Data.Entities;
using System.Collections.Generic;

namespace Legado.Core.Helps.Books
{
    /// <summary>
    /// 书籍内容类（对应 BookContent.kt）
    /// 包含处理后的文本列表和生效的替换规则
    /// </summary>
    public class BookContent
    {
        /// <summary>
        /// 是否移除了相同标题
        /// </summary>
        public bool SameTitleRemoved { get; set; }

        /// <summary>
        /// 文本列表
        /// </summary>
        public List<string> TextList { get; set; } = new List<string>();

        /// <summary>
        /// 起效的替换规则
        /// </summary>
        public List<ReplaceRule> EffectiveReplaceRules { get; set; }

        public BookContent()
        {
        }

        public BookContent(bool sameTitleRemoved, List<string> textList, List<ReplaceRule> effectiveReplaceRules = null)
        {
            SameTitleRemoved = sameTitleRemoved;
            TextList = textList ?? new List<string>();
            EffectiveReplaceRules = effectiveReplaceRules;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString()
        {
            return string.Join("\n", TextList);
        }
    }
}
