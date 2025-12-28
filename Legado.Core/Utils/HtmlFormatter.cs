using System;
using System.Text;
using System.Text.RegularExpressions;
using Legado.Core.Constants;

namespace Legado.Core.Utils
{
    /// <summary>
    /// HTML格式化工具类
    /// </summary>
    public static class HtmlFormatter
    {
        // &nbsp; 正则
        private static readonly Regex _nbspRegex = new Regex(@"(&nbsp;)+", RegexOptions.Compiled);
        
        // &ensp; 和 &emsp; 正则
        private static readonly Regex _espRegex = new Regex(@"(&ensp;|&emsp;)", RegexOptions.Compiled);
        
        // 不可打印字符正则
        private static readonly Regex _noPrintRegex = new Regex(@"(&thinsp;|&zwnj;|&zwj;|\u2009|\u200C|\u200D)", RegexOptions.Compiled);
        
        // 换行HTML标签正则
        private static readonly Regex _wrapHtmlRegex = new Regex(@"</?(?:div|p|br|hr|h\d|article|dd|dl)[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        // 注释正则
        private static readonly Regex _commentRegex = new Regex(@"<!--[^>]*-->", RegexOptions.Compiled);
        
        // 非img的HTML标签正则
        private static readonly Regex _notImgHtmlRegex = new Regex(@"</?(?!img)[a-zA-Z]+(?=[ >])[^<>]*>", RegexOptions.Compiled);
        
        // 其他HTML标签正则
        private static readonly Regex _otherHtmlRegex = new Regex(@"</?[a-zA-Z]+(?=[ >])[^<>]*>", RegexOptions.Compiled);
        
        // 格式化图片正则
        private static readonly Regex _formatImageRegex = new Regex(
            @"<img[^>]*\ssrc\s*=\s*['""]([^'""\{>]*\{(?:[^{}]|\{[^}>]+\})+\})['""][^>]*>|<img[^>]*\s(?:data-src|src)\s*=\s*['""]([^'"">]+)['""][^>]*>|<img[^>]*\sdata-[^=>]*=\s*['""]([^'"">]*)['""][^>]*>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        // 缩进正则1（换行+空白）
        private static readonly Regex _indent1Regex = new Regex(@"\s*\n+\s*", RegexOptions.Compiled);
        
        // 缩进正则2（开头换行空白）
        private static readonly Regex _indent2Regex = new Regex(@"^[\n\s]+", RegexOptions.Compiled);
        
        // 结尾正则
        private static readonly Regex _lastRegex = new Regex(@"[\n\s]+$", RegexOptions.Compiled);

        /// <summary>
        /// 格式化HTML文本
        /// </summary>
        /// <param name="html">HTML文本</param>
        /// <param name="otherRegex">其他需要移除的正则，默认为otherHtmlRegex</param>
        /// <returns>格式化后的纯文本</returns>
        public static string Format(string html, Regex otherRegex = null)
        {
            if (string.IsNullOrEmpty(html))
                return "";

            otherRegex = otherRegex ?? _otherHtmlRegex;

            return _lastRegex.Replace(
                _indent2Regex.Replace(
                    _indent1Regex.Replace(
                        otherRegex.Replace(
                            _commentRegex.Replace(
                                _wrapHtmlRegex.Replace(
                                    _noPrintRegex.Replace(
                                        _espRegex.Replace(
                                            _nbspRegex.Replace(html, " "),
                                            " "),
                                        ""),
                                    "\n"),
                                ""),
                            ""),
                        "\n\u3000\u3000"),
                    "\u3000\u3000"),
                "");
        }

        /// <summary>
        /// 格式化HTML文本，保留img标签
        /// </summary>
        /// <param name="html">HTML文本</param>
        /// <param name="redirectUrl">重定向URL，用于处理相对路径</param>
        /// <returns>格式化后的文本（保留img标签）</returns>
        public static string FormatKeepImg(string html, Uri redirectUrl = null)
        {
            if (string.IsNullOrEmpty(html))
                return "";

            var keepImgHtml = Format(html, _notImgHtmlRegex);

            var matcher = _formatImageRegex.Match(keepImgHtml);
            if (!matcher.Success)
                return keepImgHtml;

            int appendPos = 0;
            var sb = new StringBuilder();

            while (matcher.Success)
            {
                string param = "";
                
                // 获取匹配的图片URL
                string imgSrc = matcher.Groups[1].Success ? matcher.Groups[1].Value :
                               matcher.Groups[2].Success ? matcher.Groups[2].Value :
                               matcher.Groups[3].Value;

                // 处理带参数的URL
                if (matcher.Groups[1].Success)
                {
                    var urlMatcher = AppPattern.PARAM_PATTERN.Match(imgSrc);
                    if (urlMatcher.Success)
                    {
                        param = "," + imgSrc.Substring(urlMatcher.Index + urlMatcher.Length);
                        imgSrc = imgSrc.Substring(0, urlMatcher.Index);
                    }
                }

                // 拼接结果
                sb.Append(keepImgHtml.Substring(appendPos, matcher.Index - appendPos));
                sb.Append("<img src=\"");
                sb.Append(NetworkUtils.GetAbsoluteURL(redirectUrl, imgSrc));
                sb.Append(param);
                sb.Append("\">");

                appendPos = matcher.Index + matcher.Length;
                matcher = matcher.NextMatch();
            }

            if (appendPos < keepImgHtml.Length)
            {
                sb.Append(keepImgHtml.Substring(appendPos));
            }

            return sb.ToString();
        }
    }
}
