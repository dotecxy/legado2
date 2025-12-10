using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Legado.Core.Models.WebBooks
{
    /// <summary>
    /// 搜索模型类（对应 Kotlin 的 SearchModel.kt）
    /// 管理多书源并发搜索、分页、去重等功能
    /// </summary>
    public class SearchModel
    {
        private readonly ICallBack _callBack;
        private readonly SemaphoreSlim _searchSemaphore;
        private readonly int _threadCount;
        
        private long _searchId = 0L;
        private int _searchPage = 1;
        private string _searchKey = "";
        private List<BookSourcePart> _bookSources = new List<BookSourcePart>();
        private List<SearchBook> _searchBooks = new List<SearchBook>();
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isWorking = true;
        private readonly object _workingLock = new object();

        /// <summary>
        /// 线程数（对应 Kotlin 的 threadCount）
        /// </summary>
        public int ThreadCount => _threadCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="callBack">回调接口</param>
        /// <param name="threadCount">并发线程数，默认为5</param>
        public SearchModel(ICallBack callBack, int threadCount = 5)
        {
            _callBack = callBack ?? throw new ArgumentNullException(nameof(callBack));
            _threadCount = Math.Min(threadCount, 16); // 最大线程数限制
            _searchSemaphore = new SemaphoreSlim(_threadCount);
        }

        /// <summary>
        /// 开始搜索（对应 Kotlin 的 search）
        /// </summary>
        /// <param name="searchId">搜索ID</param>
        /// <param name="key">搜索关键词</param>
        public void Search(long searchId, string key)
        {
            if (searchId != _searchId)
            {
                if (string.IsNullOrEmpty(key))
                {
                    return;
                }

                _searchKey = key;
                
                if (_searchId != 0L)
                {
                    Close();
                }

                _searchBooks.Clear();
                _bookSources = _callBack.GetBookSources();
                
                if (_bookSources == null || _bookSources.Count == 0)
                {
                    _callBack.OnSearchCancel(new Exception("启用书源为空"));
                    return;
                }

                _searchId = searchId;
                _searchPage = 1;
            }
            else
            {
                _searchPage++;
            }

            StartSearch();
        }

        /// <summary>
        /// 开始搜索内部实现（对应 Kotlin 的 startSearch）
        /// </summary>
        private void StartSearch()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            Task.Run(async () =>
            {
                try
                {
                    _callBack.OnSearchStart();
                    
                    var precision = _callBack.GetPrecisionSearch();
                    var hasMore = false;

                    // 并发搜索所有书源
                    var tasks = _bookSources.Select(async bookSourcePart =>
                    {
                        await _searchSemaphore.WaitAsync(token);
                        try
                        {
                            // 等待工作状态
                            while (!_isWorking && !token.IsCancellationRequested)
                            {
                                await Task.Delay(100, token);
                            }

                            token.ThrowIfCancellationRequested();

                            // 通过回调获取完整书源
                            var bookSource = _callBack.GetBookSource(bookSourcePart.BookSourceUrl);
                            if (bookSource == null)
                            {
                                return null;
                            }

                            // 调用 WebBook 搜索
                            var webBook = new WebBook();
                            var timeout = new CancellationTokenSource(30000);
                            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, timeout.Token).Token;

                            var items = await webBook.SearchBookAwait(
                                bookSource,
                                _searchKey,
                                _searchPage,
                                filter: precision ? (name, author) =>
                                    name.Contains(_searchKey) || author.Contains(_searchKey)
                                    : null
                            );

                            return items;
                        }
                        finally
                        {
                            _searchSemaphore.Release();
                        }
                    });

                    var results = await Task.WhenAll(tasks);

                    // 合并结果
                    foreach (var items in results)
                    {
                        token.ThrowIfCancellationRequested();
                        
                        if (items != null && items.Count > 0)
                        {
                            hasMore = true;
                            await MergeItems(items, precision, token);
                            _callBack.OnSearchSuccess(_searchBooks);
                        }
                    }

                    _callBack.OnSearchFinish(_searchBooks.Count == 0, hasMore);
                }
                catch (OperationCanceledException)
                {
                    // 搜索被取消
                }
                catch (Exception ex)
                {
                    _callBack.OnSearchCancel(ex);
                }
            }, token);
        }

        /// <summary>
        /// 合并搜索结果（对应 Kotlin 的 mergeItems）
        /// </summary>
        private async Task MergeItems(List<SearchBook> newBooks, bool precision, CancellationToken token)
        {
            await Task.Run(() =>
            {
                if (newBooks == null || newBooks.Count == 0)
                    return;

                var copyData = new List<SearchBook>(_searchBooks);
                var equalData = new List<SearchBook>();
                var containsData = new List<SearchBook>();
                var otherData = new List<SearchBook>();

                // 分类现有数据
                foreach (var book in copyData)
                {
                    token.ThrowIfCancellationRequested();
                    
                    if (book.Name == _searchKey || book.Author == _searchKey)
                    {
                        equalData.Add(book);
                    }
                    else if (book.Name.Contains(_searchKey) || book.Author.Contains(_searchKey))
                    {
                        containsData.Add(book);
                    }
                    else
                    {
                        otherData.Add(book);
                    }
                }

                // 合并新数据
                foreach (var nBook in newBooks)
                {
                    token.ThrowIfCancellationRequested();
                    
                    if (nBook.Name == _searchKey || nBook.Author == _searchKey)
                    {
                        MergeToList(equalData, nBook);
                    }
                    else if (nBook.Name.Contains(_searchKey) || nBook.Author.Contains(_searchKey))
                    {
                        MergeToList(containsData, nBook);
                    }
                    else if (!precision)
                    {
                        MergeToList(otherData, nBook);
                    }
                }

                token.ThrowIfCancellationRequested();

                // 按来源数量排序并合并
                var result = new List<SearchBook>();
                result.AddRange(equalData.OrderByDescending(b => GetOriginCount(b)));
                result.AddRange(containsData.OrderByDescending(b => GetOriginCount(b)));
                
                if (!precision)
                {
                    result.AddRange(otherData.OrderByDescending(b => GetOriginCount(b)));
                }

                _searchBooks = result;
            }, token);
        }

        /// <summary>
        /// 合并书籍到列表
        /// </summary>
        private void MergeToList(List<SearchBook> list, SearchBook newBook)
        {
            var existing = list.FirstOrDefault(b => 
                b.Name == newBook.Name && b.Author == newBook.Author);
            
            if (existing != null)
            {
                // 添加书源来源（假设有 AddOrigin 方法或 Origins 属性）
                // existing.AddOrigin(newBook.Origin);
            }
            else
            {
                list.Add(newBook);
            }
        }

        /// <summary>
        /// 获取书籍来源数量
        /// </summary>
        private int GetOriginCount(SearchBook book)
        {
            // 假设 SearchBook 有 Origins 属性或类似字段
            // return book.Origins?.Count ?? 1;
            return 1; // 默认值
        }

        /// <summary>
        /// 暂停搜索（对应 Kotlin 的 pause）
        /// </summary>
        public void Pause()
        {
            lock (_workingLock)
            {
                _isWorking = false;
            }
        }

        /// <summary>
        /// 恢复搜索（对应 Kotlin 的 resume）
        /// </summary>
        public void Resume()
        {
            lock (_workingLock)
            {
                _isWorking = true;
            }
        }

        /// <summary>
        /// 取消搜索（对应 Kotlin 的 cancelSearch）
        /// </summary>
        public void CancelSearch()
        {
            Close();
            _callBack.OnSearchCancel();
        }

        /// <summary>
        /// 关闭搜索（对应 Kotlin 的 close）
        /// </summary>
        public void Close()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _searchId = 0L;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Close();
            _searchSemaphore?.Dispose();
        }

        /// <summary>
        /// 搜索回调接口（对应 Kotlin 的 CallBack）
        /// </summary>
        public interface ICallBack
        {
            /// <summary>
            /// 获取书源列表（对应 Kotlin 的 getSearchScope().getBookSourceParts()）
            /// </summary>
            List<BookSourcePart> GetBookSources();

            /// <summary>
            /// 获取完整书源（对应 Kotlin 的 getBookSource()）
            /// </summary>
            BookSource GetBookSource(string bookSourceUrl);

            /// <summary>
            /// 获取精准搜索设置
            /// </summary>
            bool GetPrecisionSearch();

            /// <summary>
            /// 搜索开始回调
            /// </summary>
            void OnSearchStart();

            /// <summary>
            /// 搜索成功回调
            /// </summary>
            /// <param name="searchBooks">搜索结果列表</param>
            void OnSearchSuccess(List<SearchBook> searchBooks);

            /// <summary>
            /// 搜索完成回调
            /// </summary>
            /// <param name="isEmpty">是否为空</param>
            /// <param name="hasMore">是否有更多结果</param>
            void OnSearchFinish(bool isEmpty, bool hasMore);

            /// <summary>
            /// 搜索取消回调
            /// </summary>
            /// <param name="exception">异常信息</param>
            void OnSearchCancel(Exception exception = null);
        }
    }
}