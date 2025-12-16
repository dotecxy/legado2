using Legado.Core.Data.Entities;
using Legado.Core.Data.Entities.Rules;
using Legado.Core.Helps.Books;
using Legado.Core.Helps.Http;
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Models.WebBooks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks; 

namespace Legado
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            Console.WriteLine("引擎测试");
            Console.WriteLine("===================");

            try
            {
                // 加载文件
                Console.WriteLine("正在加载文件...");
                //var bookSourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bookSource.json");
                var bookSourcePath = @"D:\testb\bs.txt";
                if (!File.Exists(bookSourcePath))
                {
                    Console.WriteLine("错误：文件不存在！");
                    return;
                }

                var bookSourceContent = File.ReadAllText(bookSourcePath);
                var bookSources = JsonConvert.DeserializeObject<List<BookSource>>(bookSourceContent);
                bookSources = new List<BookSource>() { bookSources[bookSources.Count - 4] };

                Console.WriteLine($"成功加载 {bookSources.Count} 个");
                Console.WriteLine();

                // 测试搜索功能
                Console.WriteLine("测试搜索功能：");
                // 使用默认关键词"小说"进行测试
                var keyword = "诡秘之主";
                Console.WriteLine($"正在搜索");

                if (bookSources.Count > 0)
                {
                    var webBook = new WebBook();
                    var successCount = 0;
                    var tryCount = Math.Min(5, bookSources.Count);

                    for (int i = 0; i < tryCount && successCount == 0; i++)
                    {
                        var bookSource = bookSources[i];
                        Console.WriteLine($"\n正在尝试 [{i + 1}/{tryCount}]");

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
                                    Console.WriteLine($"   c：{result.Kind}");
                                    Console.WriteLine($"   c：{result.LatestChapterTitle}");
                                    Console.WriteLine($"   c：{result.BookUrl}");
                                    Console.WriteLine();


                                }


                                successCount++;
                            }
                            else
                            {
                                Console.WriteLine($"  未找到结果");
                            }

                            var first = searchResults.Where(s => s.Author == "爱潜水的乌贼" && s.Name == keyword).First();
                            var book = first.ToBook();
                            var infoList = await webBook.GetBookInfoAwait(bookSource, book);
                            var chapterList = await webBook.GetChapterListAwait(bookSource, book);
                            var first2 = chapterList.First();
                            var content =  await  webBook.GetContentAwait(bookSource, book, first2);
                            var content3 = ContentHelper.ReSegment(content, first2.Title);
                            AnalyzeUrl ar = new AnalyzeUrl(first2.BookUrl);
                            content= ar.htmlFormat(content); 
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
                        Console.WriteLine($"\n没有从任何获取到结果。");
                    }

                }

                var wb = new WebBook();
                var url = bookSources[0].ExploreUrl;
                var urls = JsonConvert.DeserializeObject<List<ExploreKind>>(bookSources[0].ExploreUrl);
                await wb.ExploreBookAwait(bookSources[0], urls[0].Url);



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
