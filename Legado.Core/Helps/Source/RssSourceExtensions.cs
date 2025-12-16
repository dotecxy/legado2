using Legado.Core.Data.Entities;
using Legado.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Core.Helps.Source
{
    /// <summary>
    /// RSS源扩展方法
    /// </summary>
    public static class RssSourceExtensions
    {
        // TODO: 实现缓存管理器
        // private static readonly ACache _cache = ACache.Get("rssSortUrl");

        /// <summary>
        /// 获取排序URL的缓存键
        /// </summary>
        private static string GetSortUrlsKey(this RssSource source)
        {
            var input = source.SourceUrl + source.SortUrl;
            return GetMd5(input);
        }

        /// <summary>
        /// 获取排序URLs列表
        /// 支持JS动态生成和静态配置
        /// </summary>
        /// <param name="source">RSS源</param>
        /// <returns>名称和URL的键值对列表</returns>
        public static async Task<List<(string Name, string Url)>> GetSortUrlsAsync(this RssSource source)
        {
            var result = new List<(string Name, string Url)>();

            await Task.Run(() =>
            {
                try
                {
                    var str = source.SortUrl;

                    // 如果是JS脚本，需要执行脚本获取结果
                    if (!string.IsNullOrEmpty(source.SortUrl) &&
                        (source.SortUrl.StartsWith("<js>", StringComparison.OrdinalIgnoreCase) ||
                         source.SortUrl.StartsWith("@js:", StringComparison.OrdinalIgnoreCase)))
                    {
                        // TODO: 实现缓存读取
                        // var sortUrlsKey = source.GetSortUrlsKey();
                        // str = _cache.GetAsString(sortUrlsKey);

                        // if (string.IsNullOrEmpty(str))
                        // {
                        //     var jsStr = source.SortUrl.StartsWith("@")
                        //         ? source.SortUrl.Substring(4)
                        //         : source.SortUrl.Substring(4, source.SortUrl.LastIndexOf("<"));
                        //     
                        //     // TODO: 执行JS脚本
                        //     // str = JsEvaluator.EvalJS(jsStr).ToString();
                        //     // _cache.Put(sortUrlsKey, str);
                        // }
                    }

                    // 解析排序URL字符串
                    if (!string.IsNullOrEmpty(str))
                    {
                        var parts = str.Split(new[] { "&&", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            var colonIndex = part.IndexOf("::");
                            var name = colonIndex >= 0 ? part.Substring(0, colonIndex) : part;
                            var url = colonIndex >= 0 && colonIndex + 2 < part.Length 
                                ? part.Substring(colonIndex + 2) 
                                : "";

                            if (!string.IsNullOrEmpty(url))
                            {
                                if (url.StartsWith("{{"))
                                {
                                    result.Add((name, url));
                                }
                                else
                                {
                                    var absoluteUrl = NetworkUtils.GetAbsoluteUrl(source.SourceUrl, url);
                                    result.Add((name, absoluteUrl));
                                }
                            }
                        }
                    }

                    // 如果没有解析到任何URL，使用源URL
                    if (result.Count == 0)
                    {
                        result.Add(("", source.SourceUrl));
                    }
                }
                catch
                {
                    // 出错时返回源URL
                    if (result.Count == 0)
                    {
                        result.Add(("", source.SourceUrl));
                    }
                }
            });

            return result;
        }

        /// <summary>
        /// 清除排序URL的缓存
        /// </summary>
        public static async Task RemoveSortCacheAsync(this RssSource source)
        {
            await Task.Run(() =>
            {
                // TODO: 实现缓存清除
                // var sortUrlsKey = source.GetSortUrlsKey();
                // _cache.Remove(sortUrlsKey);
            });
        }

        /// <summary>
        /// 计算MD5值
        /// </summary>
        private static string GetMd5(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
