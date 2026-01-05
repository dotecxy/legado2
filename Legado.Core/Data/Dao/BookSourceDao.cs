using Legado.Core.Data.Entities;
using  Legado.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书源数据访问实现（对应 Kotlin 的 BookSourceDao.kt）
    /// </summary>
    public class BookSourceDao : BaseDao<BookSource>, IBookSourceDao
    {
        public BookSourceDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // ================= 查询方法 =================

        /// <summary>
        /// 获取所有书源（Part 部分）
        /// </summary>
        public async Task<List<BookSourcePart>> GetAllPartAsync()
        {
            // TODO: BookSourcePart 实体类需要创建
            // 暂时返回完整 BookSource
            var sources = await GetAllAsync();
            return new List<BookSourcePart>(); // 待实现
        }

        /// <summary>
        /// 获取所有书源
        /// </summary>
        public override async Task<List<BookSource>> GetAllAsync()
        {
            var sql = "SELECT * FROM book_sources ORDER BY customOrder";
            var result = await QueryAsync<BookSource>(sql);
            return result;
        }

        /// <summary>
        /// 搜索书源
        /// </summary>
        public async Task<List<BookSourcePart>> SearchAsync(string searchKey)
        {
            // TODO: BookSourcePart 实体类需要创建
            // 暂时返回空列表
            return new List<BookSourcePart>();
        }

        /// <summary>
        /// 搜索已启用的书源
        /// </summary>
        public async Task<List<BookSourcePart>> SearchEnabledAsync(string searchKey)
        {
            // TODO: 实现联表查询
            return new List<BookSourcePart>();
        }

        /// <summary>
        /// 按分组搜索
        /// </summary>
        public async Task<List<BookSourcePart>> GroupSearchAsync(string searchKey)
        {
            // TODO: BookSourcePart 实体类需要创建
            return new List<BookSourcePart>();
        }

        /// <summary>
        /// 获取已启用的书源
        /// </summary>
        public async Task<List<BookSourcePart>> GetEnabledAsync()
        {
            // TODO: BookSourcePart 实体类需要创建
            return new List<BookSourcePart>();
        }

        /// <summary>
        /// 获取已禁用的书源
        /// </summary>
        public async Task<List<BookSourcePart>> GetDisabledAsync()
        {
            // TODO: BookSourcePart 实体类需要创建
            return new List<BookSourcePart>();
        }

        /// <summary>
        /// 获取启用发现功能的书源
        /// </summary>
        public async Task<List<BookSourcePart>> GetExploreAsync()
        {
            // TODO: BookSourcePart 实体类需要创建
            return new List<BookSourcePart>();
        }

        /// <summary>
        /// 获取有登录 URL 的书源
        /// </summary>
        public async Task<List<BookSourcePart>> GetLoginAsync()
        {
            // TODO: BookSourcePart 实体类需要创建
            return new List<BookSourcePart>();
        }

        /// <summary>
        /// 获取未分组的书源
        /// </summary>
        public async Task<List<BookSourcePart>> GetNoGroupAsync()
        {
            // TODO: BookSourcePart 实体类需要创建
            return new List<BookSourcePart>();
        }

        /// <summary>
        /// 按分组获取书源
        /// </summary>
        public async Task<List<BookSource>> GetByGroupAsync(string group)
        {
            var sources = await GetAllAsync();
            return sources.Where(s =>
                s.BookSourceGroup?.Contains(group) ?? false
            ).ToList();
        }

        /// <summary>
        /// 按分组获取已启用的书源
        /// </summary>
        public async Task<List<BookSource>> GetEnabledByGroupAsync(string group)
        {
            var sources = await GetAllEnabledAsync();
            return sources.Where(s =>
            {
                if (string.IsNullOrEmpty(s.BookSourceGroup))
                    return false;

                return s.BookSourceGroup == group ||
                       s.BookSourceGroup.StartsWith(group + ",") ||
                       s.BookSourceGroup.EndsWith("," + group) ||
                       s.BookSourceGroup.Contains("," + group + ",");
            }).ToList();
        }

        /// <summary>
        /// 按类型获取已启用的书源
        /// </summary>
        public async Task<List<BookSource>> GetEnabledByTypeAsync(int type)
        {
            var sources = await GetAllEnabledAsync();
            return sources.Where(s =>
                s.BookUrlPattern != "NONE" &&
                s.BookSourceType == type
            ).ToList();
        }

        /// <summary>
        /// 根据 URL 获取书源
        /// </summary>
        public async Task<BookSource> GetBookSourceAsync(string key)
        {
            return await GetFirstOrDefaultAsync(s => s.BookSourceUrl == key);
        }

        /// <summary>
        /// 根据 URL 获取书源 Part
        /// </summary>
        public async Task<BookSourcePart> GetBookSourcePartAsync(string key)
        {
            // TODO: BookSourcePart 实体类需要创建
            return null;
        }

        /// <summary>
        /// 获取有书籍URL模式的书源
        /// </summary>
        public async Task<List<BookSource>> GetHasBookUrlPatternAsync()
        {
            var sources = await GetAllEnabledAsync();
            return sources.Where(s =>
                !string.IsNullOrWhiteSpace(s.BookUrlPattern) &&
                s.BookUrlPattern != "NONE"
            ).ToList();
        }

        // ================= 分组相关 =================

        /// <summary>
        /// 获取所有分组（未处理）
        /// </summary>
        public async Task<List<string>> GetAllGroupsUnProcessedAsync()
        {
            var sources = await GetAllAsync();
            return sources
                .Select(s => s.BookSourceGroup)
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 获取所有分组（已处理）
        /// </summary>
        public async Task<List<string>> GetAllGroupsAsync()
        {
            var groupsUnProcessed = await GetAllGroupsUnProcessedAsync();
            return DealGroups(groupsUnProcessed);
        }

        /// <summary>
        /// 获取已启用书源的分组
        /// </summary>
        public async Task<List<string>> GetEnabledGroupsAsync()
        {
            var sources = await GetEnabledAsync();
            var groups = sources
                .Select(s => s.BookSourceGroup)
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct()
                .ToList();
            return DealGroups(groups);
        }

        // ================= 统计方法 =================

        /// <summary>
        /// 获取书源总数
        /// </summary>
        public async Task<int> GetCountAsync()
        {
            var sql = "SELECT COUNT(*) FROM book_sources";
            return await ExecuteScalarAsync<int>(sql);
        }

        /// <summary>
        /// 检查书源是否存在
        /// </summary>
        public async Task<bool> HasAsync(string key)
        {
            var sql = "SELECT COUNT(*) FROM book_sources WHERE book_source_url = ?";
            var count = await ExecuteScalarAsync<int>(sql, key);
            return count > 0;
        }

        /// <summary>
        /// 获取最小排序值
        /// </summary>
        public async Task<int> GetMinOrderAsync()
        {
            var sources = await GetAllAsync();
            return sources.Any() ? sources.Min(s => s.CustomOrder) : 0;
        }

        /// <summary>
        /// 获取最大排序值
        /// </summary>
        public async Task<int> GetMaxOrderAsync()
        {
            var sources = await GetAllAsync();
            return sources.Any() ? sources.Max(s => s.CustomOrder) : 0;
        }

        /// <summary>
        /// 检查是否有重复排序
        /// </summary>
        public async Task<bool> HasDuplicateOrderAsync()
        {
            var sources = await GetAllAsync();
            var orderGroups = sources.GroupBy(s => s.CustomOrder);
            return orderGroups.Any(g => g.Count() > 1);
        }

        // ================= 增删改操作 =================

        /// <summary>
        /// 插入书源
        /// </summary>
        public async Task InsertAsync(params BookSource[] bookSources)
        {
            if (bookSources == null || bookSources.Length == 0)
                return;

            await InsertOrReplaceAllAsync(bookSources);
        }

        /// <summary>
        /// 更新书源
        /// </summary>
        public async Task UpdateAsync(params BookSource[] bookSources)
        {
            if (bookSources == null || bookSources.Length == 0)
                return;

            await base.UpdateAllAsync(bookSources);
        }

        /// <summary>
        /// 删除书源
        /// </summary>
        public async Task DeleteAsync(params BookSource[] bookSources)
        {
            if (bookSources == null || bookSources.Length == 0)
                return;

            await base.DeleteAllAsync(bookSources);
        }

        /// <summary>
        /// 批量删除书源（事务操作）
        /// </summary>
        public async Task DeleteAsync(List<BookSourcePart> bookSources)
        {
            await DeleteBatchAsync(bookSources);
        }

        /// <summary>
        /// 根据 URL 删除书源
        /// </summary>
        public async Task DeleteAsync(string key)
        {
            await ExecuteAsync(
                "DELETE FROM book_sources WHERE book_source_url = ?",
                key
            );
        }

        /// <summary>
        /// 批量删除书源（事务操作）
        /// </summary>
        public async Task DeleteBatchAsync(List<BookSourcePart> bookSources)
        {
            await RunInTransactionAsync(transaction =>
            {
                foreach (var bs in bookSources)
                {
                    transaction.Execute(
                        "DELETE FROM book_sources WHERE book_source_url = @a",
                       new { a = bs.BookSourceUrl }
                    );
                }
            });
        }

        /// <summary>
        /// 启用/禁用书源
        /// </summary>
        public async Task EnableAsync(string bookSourceUrl, bool enable)
        {
            await ExecuteAsync(
                "UPDATE book_sources SET enabled = ? WHERE book_source_url = @a",
                new { a = enable ? 1 : 0, bookSourceUrl }
            );
        }

        /// <summary>
        /// 批量启用/禁用（事务操作）
        /// </summary>
        public async Task EnableAsync(bool enable, List<BookSourcePart> bookSources)
        {
            await EnableBatchAsync(enable, bookSources);
        }

        /// <summary>
        /// 批量启用/禁用（事务操作）
        /// </summary>
        public async Task EnableBatchAsync(bool enable, List<BookSourcePart> bookSources)
        {
            await RunInTransactionAsync(transaction =>
            {
                foreach (var bs in bookSources)
                {
                    transaction.Execute(
                        "UPDATE book_sources SET enabled = @a WHERE book_source_url = @b",
                        new { a = enable ? 1 : 0, b = bs.BookSourceUrl }
                    );
                }
            });
        }

        /// <summary>
        /// 启用/禁用发现功能
        /// </summary>
        public async Task EnableExploreAsync(string bookSourceUrl, bool enable)
        {
            await ExecuteAsync(
                "UPDATE book_sources SET enabled_explore = ? WHERE book_source_url = @a",
                new { a = enable ? 1 : 0, bookSourceUrl }
            );
        }

        /// <summary>
        /// 批量启用/禁用发现功能
        /// </summary>
        public async Task EnableExploreAsync(bool enable, List<BookSourcePart> bookSources)
        {
            await RunInTransactionAsync(transaction =>
            {
                foreach (var bs in bookSources)
                {
                    transaction.Execute(
                        "UPDATE book_sources SET enabled_explore = ? WHERE book_source_url = @a",
                        new { a = enable ? 1 : 0, bs.BookSourceUrl }
                    );
                }
            });
        }

        /// <summary>
        /// 更新排序
        /// </summary>
        public async Task UpdateOrderAsync(string bookSourceUrl, int customOrder)
        {
            await ExecuteAsync(
                "UPDATE book_sources SET custom_order = @a WHERE book_source_url = @b",
                new { a = customOrder, b = bookSourceUrl }
            );
        }

        /// <summary>
        /// 批量更新排序（事务操作）
        /// </summary>
        public async Task UpdateOrderAsync(List<BookSourcePart> bookSources)
        {
            await UpdateOrderBatchAsync(bookSources);
        }

        /// <summary>
        /// 批量更新排序（事务操作）
        /// </summary>
        public async Task UpdateOrderBatchAsync(List<BookSourcePart> bookSources)
        {
            await RunInTransactionAsync(transaction =>
            {
                foreach (var bs in bookSources)
                {
                    transaction.Execute(
                        "UPDATE book_sources SET custom_order = @a WHERE book_source_url = @b",
                       new { a = bs.CustomOrder, b = bs.BookSourceUrl }
                    );
                }
            });
        }

        /// <summary>
        /// 更新分组
        /// </summary>
        public async Task UpdateGroupAsync(string bookSourceUrl, string bookSourceGroup)
        {
            await ExecuteAsync(
                "UPDATE book_sources SET book_source_group = @a WHERE book_source_url = @b",
                new { a = bookSourceGroup, b = bookSourceUrl }
            );
        }

        /// <summary>
        /// 批量更新分组（事务操作）
        /// </summary>
        public async Task UpdateGroupAsync(List<BookSourcePart> bookSources)
        {
            await UpdateGroupBatchAsync(bookSources);
        }

        /// <summary>
        /// 获取所有已启用的书源
        /// </summary>
        public async Task<List<BookSource>> GetAllEnabledAsync()
        {
            var sql = "SELECT * FROM book_sources WHERE enabled = 1 ORDER BY custom_order";
            var result = await QueryAsync<BookSource>(sql);
            return result;
        }

        /// <summary>
        /// 批量更新分组（事务操作）
        /// </summary>
        public async Task UpdateGroupBatchAsync(List<BookSourcePart> bookSources)
        {
            await RunInTransactionAsync(transaction =>
            {
                foreach (var bs in bookSources)
                {
                    if (!string.IsNullOrEmpty(bs.BookSourceGroup))
                    {
                        transaction.Execute(
                            "UPDATE book_sources SET book_source_group = @a WHERE book_source_url = @b",
                           new { a = bs.BookSourceGroup, b = bs.BookSourceUrl }
                        );
                    }
                }
            });
        }

        // ================= Observable 数据流 =================

        /// <summary>
        /// 观察所有书源
        /// </summary>
        public IObservable<List<BookSource>> ObserveAll()
        {
            return Observable.Create<List<BookSource>>(async observer =>
            {
                var sources = await GetAllAsync();
                observer.OnNext(sources);
                observer.OnCompleted();
            });
        }

        /// <summary>
        /// 观察搜索结果
        /// </summary>
        public IObservable<List<BookSourcePart>> ObserveSearch(string searchKey)
        {
            return Observable.Create<List<BookSourcePart>>(async observer =>
            {
                var sources = await SearchAsync(searchKey);
                observer.OnNext(sources);
                observer.OnCompleted();
            });
        }

        /// <summary>
        /// 观察已启用的书源
        /// </summary>
        public IObservable<List<BookSourcePart>> ObserveEnabled()
        {
            return Observable.Create<List<BookSourcePart>>(async observer =>
            {
                var sources = await GetEnabledAsync();
                observer.OnNext(sources);
                observer.OnCompleted();
            });
        }

        /// <summary>
        /// 观察分组
        /// </summary>
        public IObservable<List<string>> ObserveGroups()
        {
            return Observable.Create<List<string>>(async observer =>
            {
                var groups = await GetAllGroupsAsync();
                observer.OnNext(groups);
                observer.OnCompleted();
            });
        }

        // ================= 辅助方法 =================

        /// <summary>
        /// 处理分组（对应 Kotlin 的 dealGroups）
        /// </summary>
        private List<string> DealGroups(List<string> list)
        {
            var groups = new HashSet<string>();
            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                var parts = item.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        groups.Add(trimmed);
                }
            }

            // TODO: 实现中文排序 cnCompare
            return groups.OrderBy(g => g).ToList();
        }
    }
}
