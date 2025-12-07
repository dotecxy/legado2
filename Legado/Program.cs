using System;
using System.IO;
using System.Threading.Tasks;
using Legado.Core;

namespace Legado
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Legado爬虫引擎测试");
            Console.WriteLine("===================");

            try
            {
                // 加载书源文件
                Console.WriteLine("正在加载书源文件...");
                var bookSourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bookSource.json");
                if (!File.Exists(bookSourcePath))
                {
                    Console.WriteLine("错误：书源文件不存在！");
                    return;
                }

                var bookSourceContent = File.ReadAllText(bookSourcePath);
                var crawlerEngine = new CrawlerEngine();
                var bookSources = crawlerEngine.LoadBookSources(bookSourceContent);

                Console.WriteLine($"成功加载 {bookSources.Count} 个书源");
                Console.WriteLine();

                // 测试搜索功能
                Console.WriteLine("测试搜索功能：");
                // 使用默认关键词"小说"进行测试
                var keyword = "小说";
                Console.WriteLine($"正在搜索关键词：{keyword}");
                
                if (bookSources.Count > 0)
                {
                    // 使用第一个书源进行搜索
                    var firstBookSource = bookSources[0];
                    Console.WriteLine($"正在使用书源 '{firstBookSource.bookSourceName}' 搜索 '{keyword}'...");
                    
                    var searchResults = await crawlerEngine.SearchBooksAsync(firstBookSource, keyword);
                    
                    if (searchResults.Count > 0)
                    {
                        Console.WriteLine($"找到 {searchResults.Count} 个结果：");
                        for (int i = 0; i < Math.Min(5, searchResults.Count); i++)
                        {
                            var result = searchResults[i];
                            Console.WriteLine($"{i + 1}. {result.Name} - {result.Author}");
                            Console.WriteLine($"   分类：{result.Kind}");
                            Console.WriteLine($"   最新章节：{result.LastChapter}");
                            Console.WriteLine($"   链接：{result.BookUrl}");
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine("没有找到匹配的结果");
                    }
                }
                
                Console.WriteLine("测试完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试出错：{ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.ReadKey();
        }
    }
}
