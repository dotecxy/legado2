using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Legado.Core.Utils
{
    /// <summary>
    /// 字符串工具类
    /// </summary>
    public static class StringUtils
    {
        private const int HOUR_OF_DAY = 24;
        private const int DAY_OF_YESTERDAY = 2;
        private const int TIME_UNIT = 60;

        private static readonly Lazy<Dictionary<char, int>> _chnMap = new Lazy<Dictionary<char, int>>(InitChnMap);

        /// <summary>
        /// 初始化中文数字映射表
        /// </summary>
        private static Dictionary<char, int> InitChnMap()
        {
            var map = new Dictionary<char, int>();
            
            // 零一二三四五六七八九十
            string cnStr = "零一二三四五六七八九十";
            for (int i = 0; i <= 10; i++)
            {
                map[cnStr[i]] = i;
            }
            
            // 〇壹贰叁肆伍陆柒捌玖拾
            cnStr = "〇壹贰叁肆伍陆柒捌玖拾";
            for (int i = 0; i <= 10; i++)
            {
                map[cnStr[i]] = i;
            }
            
            map['两'] = 2;
            map['百'] = 100;
            map['佰'] = 100;
            map['千'] = 1000;
            map['仟'] = 1000;
            map['万'] = 10000;
            map['亿'] = 100000000;
            
            return map;
        }

        /// <summary>
        /// 将日期转换成昨天、今天、明天等友好显示
        /// </summary>
        /// <param name="source">日期字符串</param>
        /// <param name="pattern">日期格式</param>
        /// <returns>友好的日期显示</returns>
        public static string DateConvert(string source, string pattern)
        {
            try
            {
                if (!DateTime.TryParseExact(source, pattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    return "";
                }

                var now = DateTime.Now;
                var difSec = Math.Abs((long)(now - date).TotalSeconds);
                var difMin = difSec / 60;
                var difHour = difMin / 60;
                var difDate = difHour / 24;
                var oldHour = date.Hour;

                // 如果没有时间
                if (oldHour == 0)
                {
                    // 比日期:昨天今天和明天
                    if (difDate == 0)
                        return "今天";
                    if (difDate < DAY_OF_YESTERDAY)
                        return "昨天";
                    return date.ToString("yyyy-MM-dd");
                }

                if (difSec < TIME_UNIT)
                    return difSec + "秒前";
                if (difMin < TIME_UNIT)
                    return difMin + "分钟前";
                if (difHour < HOUR_OF_DAY)
                    return difHour + "小时前";
                if (difDate < DAY_OF_YESTERDAY)
                    return "昨天";
                return date.ToString("yyyy-MM-dd");
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 首字母大写
        /// </summary>
        /// <param name="str">输入字符串</param>
        /// <returns>首字母大写后的字符串</returns>
        public static string ToFirstCapital(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// 将文本中的半角字符转换成全角字符
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>全角字符串</returns>
        public static string HalfToFull(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32) // 半角空格
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] >= 33 && c[i] <= 126) // 其他符号都转换为全角
                {
                    c[i] = (char)(c[i] + 65248);
                }
            }
            return new string(c);
        }

        /// <summary>
        /// 字符串全角转换为半角
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>半角字符串</returns>
        public static string FullToHalf(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288) // 全角空格
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] >= 65281 && c[i] <= 65374)
                {
                    c[i] = (char)(c[i] - 65248);
                }
            }
            return new string(c);
        }

        /// <summary>
        /// 中文大写数字转数字
        /// </summary>
        /// <param name="chNum">中文数字字符串</param>
        /// <returns>转换后的数字，失败返回-1</returns>
        public static int ChineseNumToInt(string chNum)
        {
            if (string.IsNullOrEmpty(chNum))
                return -1;

            var chnMap = _chnMap.Value;
            int result = 0;
            int tmp = 0;
            int billion = 0;
            var cn = chNum.ToCharArray();

            // "一零二五" 形式
            if (cn.Length > 1 && Regex.IsMatch(chNum, "^[〇零一二三四五六七八九壹贰叁肆伍陆柒捌玖]+$"))
            {
                for (int i = 0; i < cn.Length; i++)
                {
                    if (chnMap.TryGetValue(cn[i], out int val))
                    {
                        cn[i] = (char)(48 + val);
                    }
                }
                if (int.TryParse(new string(cn), out int parsed))
                    return parsed;
                return -1;
            }

            // "一千零二十五", "一千二" 形式
            try
            {
                for (int i = 0; i < cn.Length; i++)
                {
                    if (!chnMap.TryGetValue(cn[i], out int tmpNum))
                        return -1;

                    if (tmpNum == 100000000)
                    {
                        result += tmp;
                        result *= tmpNum;
                        billion = billion * 100000000 + result;
                        result = 0;
                        tmp = 0;
                    }
                    else if (tmpNum == 10000)
                    {
                        result += tmp;
                        result *= tmpNum;
                        tmp = 0;
                    }
                    else if (tmpNum >= 10)
                    {
                        if (tmp == 0)
                            tmp = 1;
                        result += tmpNum * tmp;
                        tmp = 0;
                    }
                    else
                    {
                        if (i >= 2 && i == cn.Length - 1 && chnMap.TryGetValue(cn[i - 1], out int prevVal) && prevVal > 10)
                        {
                            tmp = tmpNum * prevVal / 10;
                        }
                        else
                        {
                            tmp = tmp * 10 + tmpNum;
                        }
                    }
                }
                result += tmp + billion;
                return result;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 字符串转数字
        /// </summary>
        /// <param name="str">输入字符串</param>
        /// <returns>转换后的数字，失败返回-1</returns>
        public static int StringToInt(string str)
        {
            if (string.IsNullOrEmpty(str))
                return -1;

            var num = Regex.Replace(FullToHalf(str), @"\s+", "");
            if (int.TryParse(num, out int result))
                return result;
            return ChineseNumToInt(num);
        }

        /// <summary>
        /// 是否包含数字
        /// </summary>
        /// <param name="text">输入字符串</param>
        /// <returns>是否包含数字</returns>
        public static bool IsContainNumber(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            return Regex.IsMatch(text, "[0-9]+");
        }

        /// <summary>
        /// 是否数字
        /// </summary>
        /// <param name="str">输入字符串</param>
        /// <returns>是否为数字</returns>
        public static bool IsNumeric(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            return Regex.IsMatch(str, "^-?[0-9]+$");
        }

        /// <summary>
        /// 格式化字数显示
        /// </summary>
        /// <param name="words">字数</param>
        /// <returns>格式化后的字数显示</returns>
        public static string WordCountFormat(int words)
        {
            if (words <= 0)
                return "";
            if (words > 10000)
                return (words / 10000.0).ToString("#.#") + "万字";
            return words + "字";
        }

        /// <summary>
        /// 格式化字数显示
        /// </summary>
        /// <param name="wc">字数字符串</param>
        /// <returns>格式化后的字数显示</returns>
        public static string WordCountFormat(string wc)
        {
            if (string.IsNullOrEmpty(wc))
                return "";
            if (IsNumeric(wc) && int.TryParse(wc, out int words))
                return WordCountFormat(words);
            return wc;
        }

        /// <summary>
        /// 移除字符串首尾空字符的高效方法(利用ASCII值判断,包括全角空格)
        /// </summary>
        /// <param name="s">输入字符串</param>
        /// <returns>去除首尾空白后的字符串</returns>
        public static string TrimEx(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            int start = 0;
            int len = s.Length;
            int end = len - 1;

            while (start < end && (s[start] <= 0x20 || s[start] == '　'))
            {
                ++start;
            }
            while (start < end && (s[end] <= 0x20 || s[end] == '　'))
            {
                --end;
            }
            ++end;

            return (start > 0 || end < len) ? s.Substring(start, end - start) : s;
        }

        /// <summary>
        /// 重复字符串
        /// </summary>
        /// <param name="str">要重复的字符串</param>
        /// <param name="n">重复次数</param>
        /// <returns>重复后的字符串</returns>
        public static string Repeat(string str, int n)
        {
            if (string.IsNullOrEmpty(str) || n <= 0)
                return "";

            var sb = new StringBuilder(str.Length * n);
            for (int i = 0; i < n; i++)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 移除UTF头（将\uXXXX转换为对应字符）
        /// </summary>
        /// <param name="data">输入字符串</param>
        /// <returns>转换后的字符串</returns>
        public static string RemoveUTFCharacters(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            var pattern = new Regex(@"\\u([0-9A-Fa-f]{4})");
            return pattern.Replace(data, m =>
            {
                int code = int.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
                return ((char)code).ToString();
            });
        }

        /// <summary>
        /// 压缩字符串（GZip + Base64）
        /// </summary>
        /// <param name="str">输入字符串</param>
        /// <returns>压缩后的Base64字符串</returns>
        public static string Compress(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            try
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                using (var outputStream = new MemoryStream())
                {
                    using (var gzip = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        gzip.Write(bytes, 0, bytes.Length);
                    }
                    return Convert.ToBase64String(outputStream.ToArray());
                }
            }
            catch
            {
                return str;
            }
        }

        /// <summary>
        /// 解压字符串（Base64 + GZip）
        /// </summary>
        /// <param name="str">压缩后的Base64字符串</param>
        /// <returns>解压后的原始字符串</returns>
        public static string UnCompress(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            try
            {
                var compressed = Convert.FromBase64String(str);
                using (var inputStream = new MemoryStream(compressed))
                using (var gzip = new GZipStream(inputStream, CompressionMode.Decompress))
                using (var outputStream = new MemoryStream())
                {
                    gzip.CopyTo(outputStream);
                    return Encoding.UTF8.GetString(outputStream.ToArray());
                }
            }
            catch
            {
                return str;
            }
        }
    }
}
