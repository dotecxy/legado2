using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Legado.Core.Utils
{

    public static class NetworkUtils
    {
        // 假设的公共后缀数据库接口
        public interface IPublicSuffixDatabase
        {
            string GetEffectiveTldPlusOne(string host);
        }

        // 单例实例
        private static Lazy<IPublicSuffixDatabase> _publicSuffixDatabase =
            new Lazy<IPublicSuffixDatabase>(() => new DefaultPublicSuffixDatabase());

        public static IPublicSuffixDatabase PublicSuffixDatabase => _publicSuffixDatabase.Value;

        /// <summary>
        /// 获取域名，供cookie保存和读取，处理失败返回传入的url
        /// http://1.2.3.4 => 1.2.3.4
        /// https://www.example.com => example.com
        /// http://www.biquge.com.cn => biquge.com.cn
        /// http://www.content.example.com => example.com
        /// </summary>
        public static string GetSubDomain(string url)
        {
            var baseUrl = GetBaseUrl(url);
            if (string.IsNullOrEmpty(baseUrl))
            {
                return url;
            }

            try
            {
                var uri = new Uri(baseUrl);
                var host = uri.Host;

                // 判断是否为IP地址
                if (IsIPAddress(host))
                {
                    return host;
                }

                // 使用PublicSuffixDatabase处理域名
                var effectiveDomain = PublicSuffixDatabase.GetEffectiveTldPlusOne(host);
                return effectiveDomain ?? host;
            }
            catch
            {
                return baseUrl;
            }
        }

        /// <summary>
        /// 获取域名，失败返回null
        /// </summary>
        public static string GetSubDomainOrNull(string url)
        {
            var baseUrl = GetBaseUrl(url);
            if (string.IsNullOrEmpty(baseUrl))
            {
                return null;
            }

            try
            {
                var uri = new Uri(baseUrl);
                var host = uri.Host;

                // 判断是否为IP地址
                if (IsIPAddress(host))
                {
                    return host;
                }

                // 使用PublicSuffixDatabase处理域名
                return PublicSuffixDatabase.GetEffectiveTldPlusOne(host) ?? host;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取完整主机名
        /// </summary>
        public static string GetDomain(string url)
        {
            var baseUrl = GetBaseUrl(url);
            if (string.IsNullOrEmpty(baseUrl))
            {
                return url;
            }

            try
            {
                var uri = new Uri(baseUrl);
                return uri.Host;
            }
            catch
            {
                return baseUrl;
            }
        }

        /// <summary>
        /// 获取基础URL（协议+主机+端口）
        /// </summary>
        public static string GetBaseUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            try
            {
                var uri = new Uri(url);
                var scheme = uri.Scheme;
                var host = uri.Host;
                var port = uri.Port;

                // 如果是默认端口，不显示端口号
                if ((scheme.Equals("http", StringComparison.OrdinalIgnoreCase) && port == 80) ||
                    (scheme.Equals("https", StringComparison.OrdinalIgnoreCase) && port == 443))
                {
                    return $"{scheme}://{host}";
                }

                return $"{scheme}://{host}:{port}";
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 判断是否为IP地址
        /// </summary>
        public static bool IsIPAddress(string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                return false;
            }

            // 检查IPv4
            if (IPAddress.TryParse(host, out var address))
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return true;
                }
            }

            // 检查IPv6
            if (host.Contains(':'))
            {
                // 尝试解析IPv6地址
                var ipv6Pattern = new Regex(@"^([0-9a-fA-F]{0,4}:){2,7}[0-9a-fA-F]{0,4}$");
                if (ipv6Pattern.IsMatch(host))
                {
                    return true;
                }
            }

            return false;
        }
    }

    // 默认的公共后缀数据库实现
    public class DefaultPublicSuffixDatabase : NetworkUtils.IPublicSuffixDatabase
    {
        // 可以在这里缓存公共后缀列表
        internal static readonly string[] PublicSuffixes = new[]
        {
        "com", "org", "net", "edu", "gov", "mil",
        "cn", "com.cn", "org.cn", "net.cn", "gov.cn",
        "jp", "co.jp", "ac.jp",
        "uk", "co.uk", "org.uk",
        "au", "com.au", "org.au",
        "de", "fr", "it", "es", "ru",
        // 可以添加更多后缀
    };

        public string GetEffectiveTldPlusOne(string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                return null;
            }

            // 分割主机名
            var parts = host.Split('.');
            if (parts.Length < 2)
            {
                return host;
            }

            // 尝试找到有效的TLD+1
            for (int i = parts.Length - 1; i > 0; i--)
            {
                var candidate = string.Join(".", parts, i - 1, parts.Length - i + 1);
                var tld = string.Join(".", parts, i, parts.Length - i);

                // 检查是否是公共后缀
                if (IsPublicSuffix(tld))
                {
                    return candidate;
                }
            }

            // 如果没有找到公共后缀，返回最后两部分
            if (parts.Length >= 2)
            {
                return $"{parts[2]}.{parts[1]}";
            }

            return host;
        }

        private bool IsPublicSuffix(string tld)
        {
            // 这里应该使用完整的公共后缀列表
            // 可以加载来自 https://publicsuffix.org/ 的列表

            // 简单实现：检查是否在已知后缀列表中
            return PublicSuffixes.Contains(tld.ToLower());
        }
    }

    // 可选：更完整的公共后缀数据库实现
    public class CompletePublicSuffixDatabase : NetworkUtils.IPublicSuffixDatabase
    {
        private HashSet<string> _publicSuffixes = new HashSet<string>();
        private DateTime _lastUpdated = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromDays(1);
        private readonly object _lock = new object();

        public string GetEffectiveTldPlusOne(string host)
        {
            EnsureLoaded();

            if (string.IsNullOrEmpty(host))
            {
                return null;
            }

            var parts = host.Split('.');
            var domainParts = new List<string>();

            for (int i = parts.Length - 1; i >= 0; i--)
            {
                domainParts.Insert(0, parts[i]);
                var candidate = string.Join(".", domainParts);

                // 如果是公共后缀，继续添加前一部分
                if (_publicSuffixes.Contains(candidate))
                {
                    continue;
                }

                // 如果不是公共后缀，返回当前候选域名加上前一部分
                if (i > 0)
                {
                    domainParts.Insert(0, parts[i - 1]);
                    return string.Join(".", domainParts);
                }
            }

            // 如果整个域名都是公共后缀，返回整个域名
            return host;
        }

        private void EnsureLoaded()
        {
            lock (_lock)
            {
                if ((DateTime.Now - _lastUpdated) > _cacheDuration || _publicSuffixes.Count == 0)
                {
                    LoadPublicSuffixes();
                    _lastUpdated = DateTime.Now;
                }
            }
        }

        private async void LoadPublicSuffixes()
        {
            try
            {
                // 从公共后缀列表网站加载
                using var client = new WebClient();
                var data = await client.DownloadStringTaskAsync("https://publicsuffix.org/list/public_suffix_list.dat");

                lock (_lock)
                {
                    _publicSuffixes.Clear();
                    foreach (var line in data.Split('\n'))
                    {
                        var trimmedLine = line.Trim();
                        if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("//"))
                        {
                            _publicSuffixes.Add(trimmedLine);
                        }
                    }
                }
            }
            catch
            {
                // 加载失败，使用内置的默认列表
                lock (_lock)
                {
                    _publicSuffixes = new HashSet<string>(DefaultPublicSuffixDatabase.PublicSuffixes);
                }
            }
        }
    }
}
