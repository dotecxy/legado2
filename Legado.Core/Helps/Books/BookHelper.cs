using Legado.Core.Constants;
using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Legado.Core.Helps.Books
{
    /// <summary>
    /// 书籍帮助类（对应 BookHelp.kt）
    /// 提供章节匹配、书名格式化等功能
    /// </summary>
    public static class BookHelper
    {
        /// <summary>
        /// 格式化书名
        /// </summary>
        public static string FormatBookName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // 移除书名中的特殊字符
            return Regex.Replace(name, AppPattern.NameRegex, "").Trim();
        }

        /// <summary>
        /// 格式化作者
        /// </summary>
        public static string FormatBookAuthor(string author)
        {
            if (string.IsNullOrWhiteSpace(author))
                return string.Empty;

            // 移除作者名中的特殊字符
            return Regex.Replace(author, AppPattern.AuthorRegex, "").Trim();
        }

        /// <summary>
        /// 根据目录名获取当前章节
        /// </summary>
        public static int GetDurChapter(
            int oldDurChapterIndex,
            string oldDurChapterName,
            List<BookChapter> newChapterList,
            int oldChapterListSize = 0)
        {
            if (oldDurChapterIndex <= 0) return 0;
            if (newChapterList == null || newChapterList.Count == 0)
                return oldDurChapterIndex;

            var oldChapterNum = GetChapterNum(oldDurChapterName);
            var oldName = GetPureChapterName(oldDurChapterName);
            var newChapterSize = newChapterList.Count;

            int durIndex = oldChapterListSize == 0
                ? oldDurChapterIndex
                : oldDurChapterIndex * oldChapterListSize / newChapterSize;

            int min = Math.Max(0, Math.Min(oldDurChapterIndex, durIndex) - 10);
            int max = Math.Min(newChapterSize - 1, Math.Max(oldDurChapterIndex, durIndex) + 10);

            double nameSim = 0.0;
            int newIndex = 0;
            int newNum = 0;

            // 使用名称相似度匹配
            if (!string.IsNullOrEmpty(oldName))
            {
                for (int i = min; i <= max; i++)
                {
                    var newName = GetPureChapterName(newChapterList[i].Title);
                    var temp = CalculateSimilarity(oldName, newName);
                    if (temp > nameSim)
                    {
                        nameSim = temp;
                        newIndex = i;
                    }
                }
            }

            // 使用章节序号匹配
            if (nameSim < 0.96 && oldChapterNum > 0)
            {
                for (int i = min; i <= max; i++)
                {
                    var temp = GetChapterNum(newChapterList[i].Title);
                    if (temp == oldChapterNum)
                    {
                        newNum = temp;
                        newIndex = i;
                        break;
                    }
                    else if (Math.Abs(temp - oldChapterNum) < Math.Abs(newNum - oldChapterNum))
                    {
                        newNum = temp;
                        newIndex = i;
                    }
                }
            }

            return (nameSim > 0.96 || Math.Abs(newNum - oldChapterNum) < 1)
                ? newIndex
                : Math.Min(Math.Max(0, newChapterList.Count - 1), oldDurChapterIndex);
        }

        /// <summary>
        /// 从书籍对象获取当前章节
        /// </summary>
        public static int GetDurChapter(Book oldBook, List<BookChapter> newChapterList)
        {
            if (oldBook == null) return 0;

            return GetDurChapter(
                oldBook.DurChapterIndex,
                oldBook.DurChapterTitle,
                newChapterList,
                oldBook.TotalChapterNum);
        }

        /// <summary>
        /// 获取章节序号
        /// </summary>
        private static int GetChapterNum(string chapterName)
        {
            if (string.IsNullOrWhiteSpace(chapterName))
                return -1;

            // 章节序号匹配模式
            var pattern1 = new Regex(@".*?第([\d零〇一二两三四五六七八九十百千万壹贰叁肆伍陆柒捌玖拾佰仟]+)[章节篇回集话]");
            var pattern2 = new Regex(@"^(?:[\d零〇一二两三四五六七八九十百千万壹贰叁肆伍陆柒捌玖拾佰仟]+[,:、])*([\d零〇一二两三四五六七八九十百千万壹贰叁肆伍陆柒捌玖拾佰仟]+)(?:[,:、]|\.[^\d])");

            var match = pattern1.Match(chapterName);
            if (!match.Success)
                match = pattern2.Match(chapterName);

            if (match.Success && match.Groups.Count > 1)
            {
                return ConvertChineseNumberToInt(match.Groups[1].Value);
            }

            return -1;
        }

        /// <summary>
        /// 获取纯净的章节名（去除序号等）
        /// </summary>
        private static string GetPureChapterName(string chapterName)
        {
            if (string.IsNullOrWhiteSpace(chapterName))
                return string.Empty;

            var result = chapterName;

            // 去除空格
            result = Regex.Replace(result, @"\s", "");

            // 去除章节序号
            result = Regex.Replace(result, 
                @"^.*?第(?:[\d零〇一二两三四五六七八九十百千万壹贰叁肆伍陆柒捌玖拾佰仟]+)[章节篇回集话](?!$)|^(?:[\d零〇一二两三四五六七八九十百千万壹贰叁肆伍陆柒捌玖拾佰仟]+[,:、])*(?:[\d零〇一二两三四五六七八九十百千万壹贰叁肆伍陆柒捌玖拾佰仟]+)(?:[,:、](?!$)|\\.(?=[^\\d]))", 
                "");

            // 去除括号及其内容
            result = Regex.Replace(result, 
                @"(?!^)(?:[〖【《〔\[{(][^〖【《〔\[{()〕》】〗\]}]+)?[)〕》】〗\]}]$|^[〖【《〔\[{(](?:[^〖【《〔\[{()〕》】〗\]}]+[〕》】〗\]})])?(?!$)", 
                "");

            // 去除非字母数字中日韩文字
            result = Regex.Replace(result, @"[^\w\u4E00-\u9FEF〇\u3400-\u4DBF\u20000-\u2A6DF\u2A700-\u2EBEF]", "");

            return result;
        }

        /// <summary>
        /// 计算相似度（简化版）
        /// </summary>
        private static double CalculateSimilarity(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0.0;

            // 简化的 Jaccard 相似度计算
            var set1 = new HashSet<char>(str1);
            var set2 = new HashSet<char>(str2);

            var intersection = set1.Intersect(set2).Count();
            var union = set1.Union(set2).Count();

            return union == 0 ? 0.0 : (double)intersection / union;
        }

        /// <summary>
        /// 转换中文数字为阿拉伯数字（简化版）
        /// </summary>
        private static int ConvertChineseNumberToInt(string chinese)
        {
            if (string.IsNullOrWhiteSpace(chinese))
                return -1;

            // 尝试直接解析数字
            if (int.TryParse(chinese, out int result))
                return result;

            // TODO: 实现完整的中文数字转换
            // 这里提供简化版本，只处理常见的简单数字
            var map = new Dictionary<char, int>
            {
                {'零', 0}, {'〇', 0},
                {'一', 1}, {'壹', 1},
                {'二', 2}, {'两', 2}, {'贰', 2},
                {'三', 3}, {'叁', 3},
                {'四', 4}, {'肆', 4},
                {'五', 5}, {'伍', 5},
                {'六', 6}, {'陆', 6},
                {'七', 7}, {'柒', 7},
                {'八', 8}, {'捌', 8},
                {'九', 9}, {'玖', 9},
                {'十', 10}, {'拾', 10}
            };

            if (chinese.Length == 1 && map.ContainsKey(chinese[0]))
                return map[chinese[0]];

            return -1;
        }
    }
}
