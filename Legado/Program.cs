using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Legado.Core.Data.Entities;
using Legado.Core.Models.WebBooks;
using Newtonsoft.Json;

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
                var bookSources = JsonConvert.DeserializeObject<List<BookSource>>(bookSourceContent);

                Console.WriteLine($"成功加载 {bookSources.Count} 个书源");
                Console.WriteLine();

                // 测试搜索功能
                Console.WriteLine("测试搜索功能：");
                // 使用默认关键词"小说"进行测试
                var keyword = "小说";
                Console.WriteLine($"正在搜索关键词：{keyword}");
                
                if (bookSources.Count > 0)
                {
                    var webBook = new WebBook();
                    var successCount = 0;
                    var tryCount = Math.Min(5, bookSources.Count);
                    
                    for (int i = 0; i < tryCount && successCount == 0; i++)
                    {
                        var bookSource = bookSources[i];
                        Console.WriteLine($"\n正在尝试书源 [{i + 1}/{tryCount}]: '{bookSource.BookSourceName}'");
                        
                        try
                        {
                            var searchResults = await webBook.SearchBookAwait(bookSource, keyword);
                            
                            if (searchResults != null && searchResults.Count > 0)
                            {
                                Console.WriteLine($"\n找到 {searchResults.Count} 个结果：");
                                for (int j = 0; j < Math.Min(5, searchResults.Count); j++)
                                {
                                    var result = searchResults[j];
                                    Console.WriteLine($"{j + 1}. {result.Name} - {result.Author}");
                                    Console.WriteLine($"   分类：{result.Kind}");
                                    Console.WriteLine($"   最新章节：{result.LatestChapterTitle}");
                                    Console.WriteLine($"   链接：{result.BookUrl}");
                                    Console.WriteLine();
                                }
                                successCount++;
                            }
                            else
                            {
                                Console.WriteLine($"  未找到结果");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  错误: {ex.Message}");
                            if (i == tryCount - 1 && successCount == 0)
                            {
                                Console.WriteLine($"\n详细错误信息:");
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                    
                    if (successCount == 0)
                    {
                        Console.WriteLine($"\n没有从任何书源获取到结果。");
                    }
                }
                
                Console.WriteLine("测试完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试出错：{ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
             
        }
    }
}
