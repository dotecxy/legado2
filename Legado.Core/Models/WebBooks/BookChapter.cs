using System;

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 书籍章节
    /// </summary>
    public class BookChapter
    {
        /// <summary>
        /// 章节标题
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 章节URL
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// 是否为卷
        /// </summary>
        public bool IsVolume { get; set; }
        
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// 原始内容
        /// </summary>
        public string OriginalContent { get; set; }
        
        /// <summary>
        /// 内容是否有效
        /// </summary>
        public bool IsContentValid { get; set; }
        
        /// <summary>
        /// 获取绝对URL
        /// </summary>
        /// <param name="baseUrl">基础URL</param>
        /// <returns>绝对URL</returns>
        public string GetAbsoluteURL(string baseUrl = null)
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                return string.Empty;
            }
            
            // 如果已经是绝对URL，则直接返回
            if (Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            {
                return Url;
            }
            
            // 如果提供了基础URL，则尝试构建绝对URL
            if (!string.IsNullOrWhiteSpace(baseUrl) && Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
            {
                try
                {
                    var baseUri = new Uri(baseUrl);
                    var absoluteUri = new Uri(baseUri, Url);
                    return absoluteUri.ToString();
                }
                catch (UriFormatException)
                {
                    // 如果构建失败，则返回原始URL
                    return Url;
                }
            }
            
            // 否则返回原始URL
            return Url;
        }
    }
}