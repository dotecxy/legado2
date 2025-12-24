using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Legado.Core.Utils
{
    /// <summary>
    /// 网络工具类
    /// </summary>
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
        /// 判断是否联网
        /// </summary>
        public static bool IsAvailable()
        {
            try
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
            catch
            {
                return false;
            }
        }

        // 不需要编码的查询字符集（对应 notNeedEncodingQuery）
        private static readonly Lazy<BitArray> NotNeedEncodingQuery = new Lazy<BitArray>(() =>
        {
            var bitSet = new BitArray(256);
            // a-z
            for (int i = 'a'; i <= 'z'; i++)
            {
                bitSet.Set(i, true);
            }
            // A-Z
            for (int i = 'A'; i <= 'Z'; i++)
            {
                bitSet.Set(i, true);
            }
            // 0-9
            for (int i = '0'; i <= '9'; i++)
            {
                bitSet.Set(i, true);
            }
            // 特殊字符
            foreach (char c in "!$&()*+,-./:;=?@[\\]^_`{|}~")
            {
                bitSet.Set(c, true);
            }
            return bitSet;
        });

        // 不需要编码的表单字符集（对应 notNeedEncodingForm）
        private static readonly Lazy<BitArray> NotNeedEncodingForm = new Lazy<BitArray>(() =>
        {
            var bitSet = new BitArray(256);
            // a-z
            for (int i = 'a'; i <= 'z'; i++)
            {
                bitSet.Set(i, true);
            }
            // A-Z
            for (int i = 'A'; i <= 'Z'; i++)
            {
                bitSet.Set(i, true);
            }
            // 0-9
            for (int i = '0'; i <= '9'; i++)
            {
                bitSet.Set(i, true);
            }
            // 特殊字符
            foreach (char c in "*-._")
            {
                bitSet.Set(c, true);
            }
            return bitSet;
        });

        /// <summary>
        /// 支持JAVA的URLEncoder.encode出来的string做判断。即: 将' '转成'+'
        /// 0-9a-zA-Z保留
        /// ! * ' ( ) ; : @ & = + $ , / ? # [ ] 保留
        /// 其他字符转成%XX的格式，X是16进制的大写字符，范围是[0-9A-F]
        /// </summary>
        public static bool EncodedQuery(string str)
        {
            var needEncode = false;
            var i = 0;

            while (i < str.Length)
            {
                var c = str[i];
                if (c < 256 && NotNeedEncodingQuery.Value.Get(c))
                {
                    i++;
                    continue;
                }

                if (c == '%' && i + 2 < str.Length)
                {
                    // 判断是否符合urlEncode规范
                    var c1 = str[++i];
                    var c2 = str[++i];
                    if (IsDigit16Char(c1) && IsDigit16Char(c2))
                    {
                        i++;
                        continue;
                    }
                }

                // 其他字符，肯定需要urlEncode
                needEncode = true;
                break;
            }

            return !needEncode;
        }

        /// <summary>
        /// 判断表单数据是否已编码
        /// </summary>
        public static bool EncodedForm(string str)
        {
            var needEncode = false;
            var i = 0;

            while (i < str.Length)
            {
                var c = str[i];
                if (c < 256 && NotNeedEncodingForm.Value.Get(c))
                {
                    i++;
                    continue;
                }

                if (c == '%' && i + 2 < str.Length)
                {
                    // 判断是否符合urlEncode规范
                    var c1 = str[++i];
                    var c2 = str[++i];
                    if (IsDigit16Char(c1) && IsDigit16Char(c2))
                    {
                        i++;
                        continue;
                    }
                }

                // 其他字符，肯定需要urlEncode
                needEncode = true;
                break;
            }

            return !needEncode;
        }

        /// <summary>
        /// 判断c是否是16进制的字符
        /// </summary>
        private static bool IsDigit16Char(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
        }

        /// <summary>
        /// 获取绝对地址
        /// </summary>
        public static string GetAbsoluteURL(string baseURL, string relativePath)
        {
            if (string.IsNullOrEmpty(baseURL))
                return relativePath?.Trim() ?? "";

            Uri absoluteUrl = null;
            try
            {
                // 处理逗号分隔的情况
                var baseUrlClean = baseURL.Contains(",") ? baseURL.Substring(0, baseURL.IndexOf(',')) : baseURL;
                absoluteUrl = new Uri(baseUrlClean);
            }
            catch
            {
                // 解析失败
            }

            return GetAbsoluteURL(absoluteUrl, relativePath);
        }

        /// <summary>
        /// 获取绝对地址
        /// </summary>
        public static string GetAbsoluteURL(Uri baseURL, string relativePath)
        {
            var relativePathTrim = relativePath?.Trim() ?? "";
            if (baseURL == null)
                return relativePathTrim;

            if (IsAbsUrl(relativePathTrim))
                return relativePathTrim;

            if (IsDataUrl(relativePathTrim))
                return relativePathTrim;

            if (relativePathTrim.StartsWith("javascript", StringComparison.OrdinalIgnoreCase))
                return "";

            var relativeUrl = relativePathTrim;
            try
            {
                var parseUrl = new Uri(baseURL, relativePath);
                relativeUrl = parseUrl.ToString();
                return relativeUrl;
            }
            catch (Exception e)
            {
                // 网址拼接出错
                System.Diagnostics.Debug.WriteLine($"网址拼接出错: {e.Message}");
            }

            return relativeUrl;
        }

        /// <summary>
        /// 获取绝对地址（便捷方法，对应其他文件的调用）
        /// </summary>
        public static string GetAbsoluteUrl(string baseUrl, string relativeUrl)
        {
            return GetAbsoluteURL(baseUrl, relativeUrl);
        }

        /// <summary>
        /// 获取绝对地址（便捷方法，对应其他文件的调用）
        /// </summary>
        public static string GetAbsoluteUrl(Uri baseUri, string relativeUrl)
        {
            return GetAbsoluteURL(baseUri, relativeUrl);
        }

        /// <summary>
        /// 获取基础URL
        /// </summary>
        public static string GetBaseUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var index = url.IndexOf("/", 9);
                return index == -1 ? url : url.Substring(0, index);
            }

            return null;
        }

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
                return url;

            try
            {
                var mURL = new Uri(baseUrl);
                var host = mURL.Host;

                // 判断是否为IP地址
                if (IsIPAddress(host))
                    return host;

                // PublicSuffixDatabase处理域名
                return PublicSuffixDatabase.GetEffectiveTldPlusOne(host) ?? host;
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
                return null;

            try
            {
                var mURL = new Uri(baseUrl);
                var host = mURL.Host;

                // 判断是否为IP地址
                if (IsIPAddress(host))
                    return host;

                // PublicSuffixDatabase处理域名
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
                return url;

            try
            {
                var host= new Uri(baseUrl).Host;
                var parts = host.Split('.');
                if (parts.Length >= 2)
                {
                    return parts[parts.Length - 2] + "." + parts[parts.Length - 1];
                }
                return host;
            }
            catch
            {
                return baseUrl;
            }
        }

        /// <summary>
        /// 获取本地IP地址列表
        /// </summary>
        public static List<IPAddress> GetLocalIPAddress()
        {
            var addressList = new List<IPAddress>();

            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var networkInterface in interfaces)
                {
                    if (networkInterface.OperationalStatus != OperationalStatus.Up)
                        continue;

                    var ipProperties = networkInterface.GetIPProperties();
                    foreach (var unicastAddress in ipProperties.UnicastAddresses)
                    {
                        var address = unicastAddress.Address;
                        if (!IPAddress.IsLoopback(address) && address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            addressList.Add(address);
                        }
                    }
                }
            }
            catch
            {
                // 获取失败
            }

            return addressList;
        }

        /// <summary>
        /// 检查是否是有效的IPv4地址
        /// </summary>
        public static bool IsIPv4Address(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            if (input[0] < '1' || input[0] > '9')
                return false;

            if (input.Count(c => c == '.') != 3)
                return false;

            return IPAddress.TryParse(input, out var address) &&
                   address.AddressFamily == AddressFamily.InterNetwork;
        }

        /// <summary>
        /// 检查是否是有效的IPv6地址
        /// </summary>
        public static bool IsIPv6Address(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            if (!input.Contains(":"))
                return false;

            return IPAddress.TryParse(input, out var address) &&
                   address.AddressFamily == AddressFamily.InterNetworkV6;
        }

        /// <summary>
        /// 检查是否是有效的IP地址
        /// </summary>
        public static bool IsIPAddress(string input)
        {
            return IsIPv4Address(input) || IsIPv6Address(input);
        }

        /// <summary>
        /// 判断是否为绝对URL
        /// </summary>
        private static bool IsAbsUrl(string url)
        {
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断是否为Data URL
        /// </summary>
        private static bool IsDataUrl(string url)
        {
            return url.StartsWith("data:", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// 默认的公共后缀数据库实现
    /// </summary>
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
                return $"{parts[parts.Length - 2]}.{parts[parts.Length - 1]}";
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
}
