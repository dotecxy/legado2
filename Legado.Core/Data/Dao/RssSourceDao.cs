using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// RSS 源数据访问实现（对应 Kotlin 的 RssSourceDao.kt）
    /// </summary>
    public class RssSourceDao : BaseDao<RssSource>, IRssSourceDao
    {
        public RssSourceDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // ================= 查询方法 =================

        /// <summary>
        /// 根据 URL 获取 RSS 源（对应 Kotlin 的 getByKey）
        /// </summary>
        public async Task<RssSource> GetAsync(string source_url)
        {
            return await GetFirstOrDefaultAsync(r => r.SourceUrl == source_url);
        }

        /// <summary>
        /// 根据多个 URL 获取 RSS 源（对应 Kotlin 的 getrss_sources）
        /// </summary>
        public async Task<List<RssSource>> GetByUrlsAsync(params string[] source_urls)
        {
            if (source_urls == null || source_urls.Length == 0)
                return new List<RssSource>();

            var placeholders = string.Join(",", source_urls.Select(_ => "?"));
            var sql = $"SELECT * FROM rss_sources WHERE source_url IN ({placeholders})";
            var result = await QueryAsync<RssSource>(sql, source_urls.Cast<object>().ToArray());
            return result;
        }

        /// <summary>
        /// 获取所有 RSS 源（对应 Kotlin 的 all）
        /// </summary>
        public override async Task<List<RssSource>> GetAllAsync()
        {
            var sql = "SELECT * FROM rss_sources ORDER BY custom_order";
            var result = await QueryAsync<RssSource>(sql);
            return result;
        }

        /// <summary>
        /// 搜索 RSS 源（对应 Kotlin 的 flowSearch）
        /// </summary>
        public async Task<List<RssSource>> SearchAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                return await GetAllAsync();

            var sources = await GetAllAsync();
            return sources.Where(r =>
                (r.SourceName?.Contains(key) ?? false) ||
                (r.SourceUrl?.Contains(key) ?? false) ||
                (r.SourceGroup?.Contains(key) ?? false) ||
                (r.SourceComment?.Contains(key) ?? false)
            ).ToList();
        }

        /// <summary>
        /// 按分组搜索（对应 Kotlin 的 flowGroupSearch）
        /// </summary>
        public async Task<List<RssSource>> GroupSearchAsync(string key)
        {
            var sources = await GetAllAsync();
            return sources.Where(r =>
            {
                if (string.IsNullOrEmpty(r.SourceGroup))
                    return false;

                // 完全匹配或者在逗号分隔的列表中
                return r.SourceGroup == key ||
                       r.SourceGroup.StartsWith(key + ",") ||
                       r.SourceGroup.EndsWith("," + key) ||
                       r.SourceGroup.Contains("," + key + ",");
            }).ToList();
        }

        /// <summary>
        /// 获取已启用的 RSS 源（对应 Kotlin 的 flowEnabled）
        /// </summary>
        public async Task<List<RssSource>> GetEnabledAsync()
        {
            var sql = "SELECT * FROM rss_sources WHERE enabled = 1 ORDER BY custom_order";
            var result = await QueryAsync<RssSource>(sql);
            return result;
        }

        /// <summary>
        /// 获取已禁用的 RSS 源
        /// </summary>
        public async Task<List<RssSource>> GetDisabledAsync()
        {
            var sql = "SELECT * FROM rss_sources WHERE enabled = 0 ORDER BY custom_order";
            var result = await QueryAsync<RssSource>(sql);
            return result;
        }

        /// <summary>
        /// 获取有登录 URL 的 RSS 源
        /// </summary>
        public async Task<List<RssSource>> GetLoginAsync()
        {
            var sources = await GetAllAsync();
            return sources.Where(r => !string.IsNullOrEmpty(r.LoginUrl)).ToList();
        }

        /// <summary>
        /// 获取未分组的 RSS 源
        /// </summary>
        public async Task<List<RssSource>> GetNoGroupAsync()
        {
            var sources = await GetAllAsync();
            return sources.Where(r =>
                string.IsNullOrEmpty(r.SourceGroup) ||
                r.SourceGroup.Contains("未分组")
            ).ToList();
        }

        /// <summary>
        /// 搜索已启用的 RSS 源
        /// </summary>
        public async Task<List<RssSource>> SearchEnabledAsync(string searchKey)
        {
            var sources = await GetEnabledAsync();
            return sources.Where(r =>
                (r.SourceName?.Contains(searchKey) ?? false) ||
                (r.SourceGroup?.Contains(searchKey) ?? false) ||
                (r.SourceUrl?.Contains(searchKey) ?? false) ||
                (r.SourceComment?.Contains(searchKey) ?? false)
            ).ToList();
        }

        /// <summary>
        /// 按分组获取已启用的源
        /// </summary>
        public async Task<List<RssSource>> GetEnabledByGroupAsync(string searchKey)
        {
            var sources = await GetEnabledAsync();
            return sources.Where(r =>
            {
                if (string.IsNullOrEmpty(r.SourceGroup))
                    return false;

                return r.SourceGroup == searchKey ||
                       r.SourceGroup.StartsWith(searchKey + ",") ||
                       r.SourceGroup.EndsWith("," + searchKey) ||
                       r.SourceGroup.Contains("," + searchKey + ",");
            }).ToList();
        }

        /// <summary>
        /// 根据分组获取源
        /// </summary>
        public async Task<List<RssSource>> GetByGroupAsync(string group)
        {
            var sources = await GetAllAsync();
            return sources.Where(r =>
                r.SourceGroup?.Contains(group) ?? false
            ).ToList();
        }

        /// <summary>
        /// 获取所有分组（未处理）
        /// </summary>
        public async Task<List<string>> GetAllGroupsUnProcessedAsync()
        {
            var sources = await GetAllAsync();
            return sources
                .Select(r => r.SourceGroup)
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

        // ================= 统计方法 =================

        /// <summary>
        /// 获取源总数（对应 Kotlin 的 size）
        /// </summary>
        public async Task<int> GetCountAsync()
        {
            var sql = "SELECT COUNT(*) FROM rss_sources";
            return await ExecuteScalarAsync<int>(sql);
        }

        /// <summary>
        /// 检查源是否存在（对应 Kotlin 的 has）
        /// </summary>
        public async Task<bool> HasAsync(string key)
        {
            var sql = "SELECT COUNT(*) FROM rss_sources WHERE source_url = ?";
            var count = await ExecuteScalarAsync<int>(sql, key);
            return count > 0;
        }

        /// <summary>
        /// 获取最小排序值
        /// </summary>
        public async Task<int> GetMinOrderAsync()
        {
            var sql = "SELECT MIN(custom_order) FROM rss_sources";
            var result = await ExecuteScalarAsync<int?>(sql);
            return result ?? 0;
        }

        /// <summary>
        /// 获取最大排序值
        /// </summary>
        public async Task<int> GetMaxOrderAsync()
        {
            var sql = "SELECT MAX(custom_order) FROM rss_sources";
            var result = await ExecuteScalarAsync<int?>(sql);
            return result ?? 0;
        }

        // ================= 增删改方法 =================

        /// <summary>
        /// 插入源（对应 Kotlin 的 insert）
        /// </summary>
        public async Task InsertAsync(params RssSource[] sources)
        {
            if (sources == null || sources.Length == 0)
                return;

            await InsertOrReplaceAllAsync(sources);
        }

        /// <summary>
        /// 更新源（对应 Kotlin 的 update）
        /// </summary>
        public async Task UpdateAsync(params RssSource[] sources)
        {
            if (sources == null || sources.Length == 0)
                return;

            await base.UpdateAllAsync(sources);
        }

        /// <summary>
        /// 删除源（对应 Kotlin 的 delete）
        /// </summary>
        public async Task DeleteAsync(params RssSource[] sources)
        {
            if (sources == null || sources.Length == 0)
                return;

            await base.DeleteAllAsync(sources);
        }

        /// <summary>
        /// 根据 URL 删除源
        /// </summary>
        public async Task DeleteAsync(string source_url)
        {
            await ExecuteAsync(
                "DELETE FROM rss_sources WHERE source_url = @",
                new { a = source_url }
            );
        }

        /// <summary>
        /// 删除默认源（分组包含 'legado'）
        /// </summary>
        public async Task DeleteDefaultAsync()
        {
            await ExecuteAsync(
                "DELETE FROM rss_sources WHERE source_group LIKE '%legado%'"
            );
        }

        /// <summary>
        /// 启用/禁用源（对应 Kotlin 的 enable）
        /// </summary>
        public async Task EnableAsync(string source_url, bool enable)
        {
            await ExecuteAsync(
                "UPDATE rss_sources SET enabled = @a WHERE source_url = @b",
                new { a = enable ? 1 : 0, b = source_url }
            );
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

                // 按逗号或换行符分割
                var parts = item.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        groups.Add(trimmed);
                }
            }

            // 排序（TODO: 实现中文排序 cnCompare）
            return groups.OrderBy(g => g).ToList();
        }
    }
}
