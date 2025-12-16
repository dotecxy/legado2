using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Legado.Core.Helps.Books
{
    /// <summary>
    /// 内容帮助类（对应 ContentHelp.kt）
    /// 提供段落重排、引号处理等文本格式化功能
    /// </summary>
    public static class ContentHelper
    {
        // 句子结尾的标点
        private const string MarkSentencesEnd = "？。！?!~";
        private const string MarkSentencesEndP = ".？。！?!~";
        
        // 句中标点
        private const string MarkSentencesMid = ".，、,—…";
        private const string MarkSentencesSay = "问说喊唱叫骂道着答";
        
        // XXX说：""的冒号
        private const string MarkQuotationBefore = "，：,:";
        
        // 引号
        private const string MarkQuotation = "\u201c\u201d";
        
        // 限制字典的长度
        private const int WordMaxLength = 16;
        
        // 段落对话正则
        private static readonly Regex ParagraphDialogRegex = new Regex(@"^[\u201c\u201d][^\u201c\u201d]+[\u201c\u201d]$");

        /// <summary>
        /// 段落重排算法入口
        /// 把整篇内容输入，连接错误的分段，再把每个段落重新切分
        /// </summary>
        /// <param name="content">正文</param>
        /// <param name="chapterName">标题</param>
        /// <returns>重新分段后的内容</returns>
        public static string ReSegment(string content, string chapterName)
        {
            if (string.IsNullOrWhiteSpace(content))
                return content;

            var dict = MakeDict(content);
            
            // 处理引号和分段
            var p = content
                .Replace("&quot;", "\"")
                .Split(new[] { "\n" }, StringSplitOptions.None);

            var buffer = new StringBuilder((int)(content.Length * 1.15));
            buffer.Append(" ");

            if (chapterName.Trim() != p[0].Trim())
            {
                // 去除段落内空格
                buffer.Append(Regex.Replace(p[0], @"[\u3000\s]+", ""));
            }

            // 重新黏合错误分段的段落
            for (int i = 1; i < p.Length; i++)
            {
                if (buffer.Length > 0 && Match(MarkSentencesEnd, buffer[buffer.Length - 1]))
                {
                    buffer.Append("\n");
                }
                buffer.Append(Regex.Replace(p[i], @"[\u3000\s]", ""));
            }

            // TODO: 实现完整的段落重排逻辑
            return buffer.ToString();
        }

        /// <summary>
        /// 创建词条字典
        /// </summary>
        private static List<string> MakeDict(string str)
        {
            // 引号中间不包含任何标点
            var pattern = new Regex(@"(?<=[\u0022\u0027\u201c\u201d])([^\p{P}]{1," + WordMaxLength + @"})(?=[\u0022\u0027\u201c\u201d])");
            var matches = pattern.Matches(str);
            
            var cache = new HashSet<string>();
            var dict = new List<string>();
            
            foreach (Match match in matches)
            {
                var word = match.Value;
                if (cache.Contains(word))
                {
                    if (!dict.Contains(word))
                        dict.Add(word);
                }
                else
                {
                    cache.Add(word);
                }
            }
            
            return dict;
        }

        /// <summary>
        /// 匹配字符是否在规则中
        /// </summary>
        private static bool Match(string rule, char chr)
        {
            return rule.IndexOf(chr) != -1;
        }

        /// <summary>
        /// 计算字符串最后出现与字典中字符匹配的位置
        /// </summary>
        private static int SeekLast(string str, string key, int from, int to)
        {
            if (str.Length - from < 1) return -1;
            
            int i = str.Length - 1;
            if (from < i && i > 0) i = from;
            
            int t = 0;
            if (to > 0) t = to;
            
            while (i > t)
            {
                if (key.IndexOf(str[i]) != -1)
                    return i;
                i--;
            }
            
            return -1;
        }

        /// <summary>
        /// 计算字符串与字典中字符的最短距离
        /// </summary>
        private static int SeekIndex(string str, string key, int from, int to, bool inOrder)
        {
            if (str.Length - from < 1) return -1;
            
            int i = 0;
            if (from > i) i = from;
            
            int t = str.Length;
            if (to > 0) t = Math.Min(t, to);
            
            while (i < t)
            {
                char c = inOrder ? str[i] : str[str.Length - i - 1];
                if (key.IndexOf(c) != -1)
                    return i;
                i++;
            }
            
            return -1;
        }
    }
}
