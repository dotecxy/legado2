using Legado.Core.Data.Entities;
using Legado.Core.Data.Entities.Rules;
using Legado.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Legado.Core.Helps.Source
{
    /// <summary>
    /// 书源扩展方法
    /// </summary>
    public static class BookSourceExtensions
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _mutexMap = new ConcurrentDictionary<string, SemaphoreSlim>();
        private static readonly ConcurrentDictionary<string, List<ExploreKind>> _exploreKindsMap = new ConcurrentDictionary<string, List<ExploreKind>>();
        
        // TODO: 实现缓存管理器
        // private static readonly ACache _cache = ACache.Get("explore");

        /// <summary>
        /// 获取发现分类缓存键（采用MD5作为key可以在分类修改后自动重新计算）
        /// </summary>
        private static string GetExploreKindsKey(this BookSource source)
        {
            var input = source.BookSourceUrl + source.ExploreUrl;
            return GetMd5(input);
        }

        /// <summary>
        /// 获取发现分类缓存键（BookSourcePart版本）
        /// </summary>
        private static string GetExploreKindsKey(this BookSourcePart source)
        {
            // TODO: 需要实现 GetBookSource() 方法
            // return source.GetBookSource()?.GetExploreKindsKey() ?? string.Empty;
            // BookSourcePart没有ExploreUrl属性，这里只使用BookSourceUrl
            return GetMd5(source.BookSourceUrl);
        }

        /// <summary>
        /// 获取发现分类列表（BookSourcePart版本）
        /// </summary>
        public static async Task<List<ExploreKind>> GetExploreKindsAsync(this BookSourcePart source)
        {
            // TODO: 需要实现 GetBookSource() 方法
            // var bookSource = source.GetBookSource();
            // if (bookSource != null)
            // {
            //     return await bookSource.GetExploreKindsAsync();
            // }
            // return new List<ExploreKind>();
            
            // 临时实现
            return await Task.FromResult(new List<ExploreKind>());
        }

        /// <summary>
        /// 获取发现分类列表
        /// 支持JS动态生成和静态配置（JSON数组或name::url格式）
        /// </summary>
        public static async Task<List<ExploreKind>> GetExploreKindsAsync(this BookSource source)
        {
            var exploreKindsKey = source.GetExploreKindsKey();
            
            // 从缓存中获取
            if (_exploreKindsMap.TryGetValue(exploreKindsKey, out var cachedKinds))
            {
                return cachedKinds;
            }

            var exploreUrl = source.ExploreUrl;
            if (string.IsNullOrWhiteSpace(exploreUrl))
            {
                return new List<ExploreKind>();
            }

            // 使用信号量确保同一书源的分类只计算一次
            var mutex = _mutexMap.GetOrAdd(source.BookSourceUrl, _ => new SemaphoreSlim(1, 1));
            await mutex.WaitAsync();

            try
            {
                // 双重检查
                if (_exploreKindsMap.TryGetValue(exploreKindsKey, out cachedKinds))
                {
                    return cachedKinds;
                }

                var kinds = new List<ExploreKind>();

                await Task.Run(() =>
                {
                    try
                    {
                        var ruleStr = exploreUrl;

                        // 如果是JS脚本，需要执行脚本获取结果
                        if (exploreUrl.StartsWith("<js>", StringComparison.OrdinalIgnoreCase) ||
                            exploreUrl.StartsWith("@js:", StringComparison.OrdinalIgnoreCase))
                        {
                            // TODO: 实现缓存读取和JS执行
                            // ruleStr = _cache.GetAsString(exploreKindsKey);
                            
                            // if (string.IsNullOrEmpty(ruleStr))
                            // {
                            //     var jsStr = exploreUrl.StartsWith("@")
                            //         ? exploreUrl.Substring(4)
                            //         : exploreUrl.Substring(4, exploreUrl.LastIndexOf("<"));
                            //     
                            //     // 执行JS脚本
                            //     ruleStr = JsEvaluator.EvalJS(jsStr).ToString().Trim();
                            //     _cache.Put(exploreKindsKey, ruleStr);
                            // }
                        }

                        // 解析规则字符串
                        if (!string.IsNullOrEmpty(ruleStr))
                        {
                            // 尝试解析为JSON数组
                            if (JsonHelper.IsJsonArray(ruleStr))
                            {
                                try
                                {
                                    var jsonKinds = JsonConvert.DeserializeObject<List<ExploreKind>>(ruleStr);
                                    if (jsonKinds != null)
                                    {
                                        kinds.AddRange(jsonKinds);
                                    }
                                }
                                catch
                                {
                                    // JSON解析失败，继续尝试其他格式
                                }
                            }

                            // 如果不是JSON或解析失败，尝试按行分割
                            if (kinds.Count == 0)
                            {
                                var parts = ruleStr.Split(new[] { "&&", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var part in parts)
                                {
                                    var kindCfg = part.Split(new[] { "::" }, StringSplitOptions.None);
                                    var name = kindCfg.Length > 0 ? kindCfg[0] : "";
                                    var url = kindCfg.Length > 1 ? kindCfg[1] : null;
                                    kinds.Add(new ExploreKind { Title = name, Url = url });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 出错时添加错误信息
                        kinds.Add(new ExploreKind
                        {
                            Title = $"ERROR:{ex.Message}",
                            Url = ex.StackTrace
                        });
                    }
                });

                _exploreKindsMap[exploreKindsKey] = kinds;
                return kinds;
            }
            finally
            {
                mutex.Release();
            }
        }

        /// <summary>
        /// 清除发现分类缓存（BookSourcePart版本）
        /// </summary>
        public static async Task ClearExploreKindsCacheAsync(this BookSourcePart source)
        {
            await Task.Run(() =>
            {
                var exploreKindsKey = source.GetExploreKindsKey();
                // TODO: 清除缓存
                // _cache.Remove(exploreKindsKey);
                _exploreKindsMap.TryRemove(exploreKindsKey, out _);
            });
        }

        /// <summary>
        /// 清除发现分类缓存（BookSource版本）
        /// </summary>
        public static async Task ClearExploreKindsCacheAsync(this BookSource source)
        {
            await Task.Run(() =>
            {
                var exploreKindsKey = source.GetExploreKindsKey();
                // TODO: 清除缓存
                // _cache.Remove(exploreKindsKey);
                _exploreKindsMap.TryRemove(exploreKindsKey, out _);
            });
        }

        /// <summary>
        /// 获取发现分类的JSON字符串
        /// </summary>
        public static string GetExploreKindsJson(this BookSource source)
        {
            // TODO: 从缓存读取
            // var exploreKindsKey = source.GetExploreKindsKey();
            // var cached = _cache.GetAsString(exploreKindsKey);
            // if (!string.IsNullOrEmpty(cached) && JsonHelper.IsJsonArray(cached))
            // {
            //     return cached;
            // }

            // 如果exploreUrl本身是JSON数组，直接返回
            if (!string.IsNullOrEmpty(source.ExploreUrl) && JsonHelper.IsJsonArray(source.ExploreUrl))
            {
                return source.ExploreUrl;
            }

            return "";
        }

        /// <summary>
        /// 获取书籍类型
        /// </summary>
        /// <param name="source">书源</param>
        /// <returns>书籍类型标志位（文本、音频、图片、文件等）</returns>
        public static int GetBookType(this BookSource source)
        {
            const int BookTypeText = 0;
            const int BookTypeAudio = 1;
            const int BookTypeImage = 2;
            const int BookTypeWebFile = 3;

            switch (source.BookSourceType)
            {
                case 3: // BookSourceType.File
                    return BookTypeText | BookTypeWebFile;
                case 2: // BookSourceType.Image
                    return BookTypeImage;
                case 1: // BookSourceType.Audio
                    return BookTypeAudio;
                default:
                    return BookTypeText;
            }
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

    /// <summary>
    /// JSON辅助类
    /// </summary>
    internal static class JsonHelper
    {
        /// <summary>
        /// 判断字符串是否为JSON数组
        /// </summary>
        public static bool IsJsonArray(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;

            var trimmed = str.Trim();
            return trimmed.StartsWith("[") && trimmed.EndsWith("]");
        }
    }
}
