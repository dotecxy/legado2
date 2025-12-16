using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Core.Helps
{
    /// <summary>
    /// 规则大数据帮助类
    /// 用于存储和管理书籍、RSS等规则相关的大数据
    /// 数据以文件形式存储，使用MD5作为文件名避免冲突
    /// </summary>
    public static class RuleBigDataHelper
    {
        private static readonly string RuleDataDir;
        private static readonly string BookDataDir;
        private static readonly string RssDataDir;

        static RuleBigDataHelper()
        {
            // TODO: 需要根据实际情况配置数据存储目录
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            RuleDataDir = Path.Combine(appDataPath, "Legado", "ruleData");
            BookDataDir = Path.Combine(RuleDataDir, "book");
            RssDataDir = Path.Combine(RuleDataDir, "rss");

            // 确保目录存在
            Directory.CreateDirectory(RuleDataDir);
            Directory.CreateDirectory(BookDataDir);
            Directory.CreateDirectory(RssDataDir);
        }

        /// <summary>
        /// 清理无效的数据文件
        /// </summary>
        public static async Task ClearInvalidAsync()
        {
            await Task.Run(() =>
            {
                // 清理书籍数据
                ClearInvalidBookData();

                // 清理RSS数据
                ClearInvalidRssData();
            });
        }

        private static void ClearInvalidBookData()
        {
            if (!Directory.Exists(BookDataDir))
                return;

            var directories = Directory.GetDirectories(BookDataDir);
            foreach (var dir in directories)
            {
                var bookUrlFile = Path.Combine(dir, "bookUrl.txt");
                if (!File.Exists(bookUrlFile))
                {
                    Directory.Delete(dir, true);
                }
                else
                {
                    // TODO: 检查书籍是否还存在于数据库中
                    // var bookUrl = File.ReadAllText(bookUrlFile);
                    // if (!await appDb.bookDao.HasAsync(bookUrl))
                    // {
                    //     Directory.Delete(dir, true);
                    // }
                }
            }

            // 清理单个文件
            var files = Directory.GetFiles(BookDataDir);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        private static void ClearInvalidRssData()
        {
            if (!Directory.Exists(RssDataDir))
                return;

            var directories = Directory.GetDirectories(RssDataDir);
            foreach (var dir in directories)
            {
                var originFile = Path.Combine(dir, "origin.txt");
                if (!File.Exists(originFile))
                {
                    Directory.Delete(dir, true);
                }
                else
                {
                    // TODO: 检查RSS源是否还存在于数据库中
                    // var origin = File.ReadAllText(originFile);
                    // if (!await appDb.rssSourceDao.HasAsync(origin))
                    // {
                    //     Directory.Delete(dir, true);
                    // }
                }
            }

            // 清理单个文件
            var files = Directory.GetFiles(RssDataDir);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        /// <summary>
        /// 存储书籍变量
        /// </summary>
        /// <param name="bookUrl">书籍URL</param>
        /// <param name="key">变量键</param>
        /// <param name="value">变量值，为null时删除</param>
        public static void PutBookVariable(string bookUrl, string key, string value)
        {
            var md5BookUrl = GetMd5(bookUrl);
            var md5Key = GetMd5(key);
            var dir = Path.Combine(BookDataDir, md5BookUrl);
            var valueFile = Path.Combine(dir, $"{md5Key}.txt");

            if (value == null)
            {
                if (File.Exists(valueFile))
                {
                    File.Delete(valueFile);
                }
            }
            else
            {
                Directory.CreateDirectory(dir);
                File.WriteAllText(valueFile, value, Encoding.UTF8);

                var bookUrlFile = Path.Combine(dir, "bookUrl.txt");
                if (!File.Exists(bookUrlFile))
                {
                    File.WriteAllText(bookUrlFile, bookUrl, Encoding.UTF8);
                }
            }
        }

        /// <summary>
        /// 获取书籍变量
        /// </summary>
        /// <param name="bookUrl">书籍URL</param>
        /// <param name="key">变量键</param>
        /// <returns>变量值，不存在时返回null</returns>
        public static string GetBookVariable(string bookUrl, string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            var md5BookUrl = GetMd5(bookUrl);
            var md5Key = GetMd5(key);
            var valueFile = Path.Combine(BookDataDir, md5BookUrl, $"{md5Key}.txt");

            if (File.Exists(valueFile))
            {
                return File.ReadAllText(valueFile, Encoding.UTF8);
            }
            return null;
        }

        /// <summary>
        /// 检查书籍变量是否存在
        /// </summary>
        public static bool HasBookVariable(string bookUrl, string key)
        {
            var md5BookUrl = GetMd5(bookUrl);
            var md5Key = GetMd5(key);
            var valueFile = Path.Combine(BookDataDir, md5BookUrl, $"{md5Key}.txt");
            return File.Exists(valueFile);
        }

        /// <summary>
        /// 存储章节变量
        /// </summary>
        /// <param name="bookUrl">书籍URL</param>
        /// <param name="chapterUrl">章节URL</param>
        /// <param name="key">变量键</param>
        /// <param name="value">变量值，为null时删除</param>
        public static void PutChapterVariable(string bookUrl, string chapterUrl, string key, string value)
        {
            var md5BookUrl = GetMd5(bookUrl);
            var md5ChapterUrl = GetMd5(chapterUrl);
            var md5Key = GetMd5(key);
            var dir = Path.Combine(BookDataDir, md5BookUrl, md5ChapterUrl);
            var valueFile = Path.Combine(dir, $"{md5Key}.txt");

            if (value == null)
            {
                if (File.Exists(valueFile))
                {
                    File.Delete(valueFile);
                }
            }
            else
            {
                Directory.CreateDirectory(dir);
                File.WriteAllText(valueFile, value, Encoding.UTF8);

                var bookUrlFile = Path.Combine(BookDataDir, md5BookUrl, "bookUrl.txt");
                if (!File.Exists(bookUrlFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(bookUrlFile));
                    File.WriteAllText(bookUrlFile, bookUrl, Encoding.UTF8);
                }
            }
        }

        /// <summary>
        /// 获取章节变量
        /// </summary>
        /// <param name="bookUrl">书籍URL</param>
        /// <param name="chapterUrl">章节URL</param>
        /// <param name="key">变量键</param>
        /// <returns>变量值，不存在时返回null</returns>
        public static string GetChapterVariable(string bookUrl, string chapterUrl, string key)
        {
            var md5BookUrl = GetMd5(bookUrl);
            var md5ChapterUrl = GetMd5(chapterUrl);
            var md5Key = GetMd5(key);
            var valueFile = Path.Combine(BookDataDir, md5BookUrl, md5ChapterUrl, $"{md5Key}.txt");

            if (File.Exists(valueFile))
            {
                return File.ReadAllText(valueFile, Encoding.UTF8);
            }
            return null;
        }

        /// <summary>
        /// 存储RSS变量
        /// </summary>
        /// <param name="origin">RSS源标识</param>
        /// <param name="link">文章链接</param>
        /// <param name="key">变量键</param>
        /// <param name="value">变量值，为null时删除</param>
        public static void PutRssVariable(string origin, string link, string key, string value)
        {
            var md5Origin = GetMd5(origin);
            var md5Link = GetMd5(link);
            var md5Key = GetMd5(key);
            var dir = Path.Combine(RssDataDir, md5Origin, md5Link);
            var valueFile = Path.Combine(dir, $"{md5Key}.txt");

            if (value == null)
            {
                if (File.Exists(valueFile))
                {
                    File.Delete(valueFile);
                }
            }
            else
            {
                Directory.CreateDirectory(dir);
                File.WriteAllText(valueFile, value, Encoding.UTF8);

                var originFile = Path.Combine(RssDataDir, md5Origin, "origin.txt");
                if (!File.Exists(originFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(originFile));
                    File.WriteAllText(originFile, origin, Encoding.UTF8);
                }

                var linkFile = Path.Combine(dir, "origin.txt");
                if (!File.Exists(linkFile))
                {
                    File.WriteAllText(linkFile, link, Encoding.UTF8);
                }
            }
        }

        /// <summary>
        /// 获取RSS变量
        /// </summary>
        /// <param name="origin">RSS源标识</param>
        /// <param name="link">文章链接</param>
        /// <param name="key">变量键</param>
        /// <returns>变量值，不存在时返回null</returns>
        public static string GetRssVariable(string origin, string link, string key)
        {
            var md5Origin = GetMd5(origin);
            var md5Link = GetMd5(link);
            var md5Key = GetMd5(key);
            var valueFile = Path.Combine(RssDataDir, md5Origin, md5Link, $"{md5Key}.txt");

            if (File.Exists(valueFile))
            {
                return File.ReadAllText(valueFile, Encoding.UTF8);
            }
            return null;
        }

        /// <summary>
        /// 计算字符串的MD5值（16进制小写字符串）
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
