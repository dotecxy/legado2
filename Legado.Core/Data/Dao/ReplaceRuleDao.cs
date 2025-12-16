using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 替换规则数据访问实现（对应 Kotlin 的 ReplaceRuleDao.kt）
    /// </summary>
    public class ReplaceRuleDao : DapperDao<ReplaceRule>, IReplaceRuleDao
    {
        public ReplaceRuleDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // ================= 查询方法 =================

        /// <summary>
        /// 获取所有替换规则
        /// </summary>
        public override async Task<List<ReplaceRule>> GetAllAsync()
        {
            var sql = "SELECT * FROM replace_rules ORDER BY `order`";
            var result = await QueryAsync<ReplaceRule>(sql);
            return result;
        }

        /// <summary>
        /// 搜索替换规则
        /// </summary>
        public async Task<List<ReplaceRule>> SearchAsync(string key)
        {
            var sql = "SELECT * FROM replace_rules WHERE `group` LIKE ? OR name LIKE ? ORDER BY `order`";
            var result = await QueryAsync<ReplaceRule>(sql, $"%{key}%", $"%{key}%");
            return result;
        }

        /// <summary>
        /// 按分组搜索
        /// </summary>
        public async Task<List<ReplaceRule>> GroupSearchAsync(string key)
        {
            var sql = "SELECT * FROM replace_rules WHERE `group` LIKE ? ORDER BY `order`";
            var result = await QueryAsync<ReplaceRule>(sql, $"%{key}%");
            return result;
        }

        /// <summary>
        /// 获取未分组的规则
        /// </summary>
        public async Task<List<ReplaceRule>> GetNoGroupAsync()
        {
            var sql = "SELECT * FROM replace_rules WHERE `group` IS NULL OR `group` = '' OR `group` LIKE '%未分组%' ORDER BY `order`";
            var result = await QueryAsync<ReplaceRule>(sql);
            return result;
        }

        /// <summary>
        /// 根据 ID 查询
        /// </summary>
        public async Task<ReplaceRule> FindByIdAsync(long id)
        {
            return await GetFirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// 根据 ID 获取（接口要求的方法）
        /// </summary>
        public async Task<ReplaceRule> GetAsync(long id)
        {
            return await FindByIdAsync(id);
        }

        /// <summary>
        /// 根据多个 ID 查询
        /// </summary>
        public async Task<List<ReplaceRule>> FindByIdsAsync(params long[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<ReplaceRule>();

            var placeholders = string.Join(",", ids.Select(_ => "?"));
            var sql = $"SELECT * FROM replace_rules WHERE id IN ({placeholders}) ORDER BY `order`";
            var result = await QueryAsync<ReplaceRule>(sql, ids.Cast<object>().ToArray());
            return result;
        }

        /// <summary>
        /// 查找启用的内容替换规则（按作用域）
        /// </summary>
        public async Task<List<ReplaceRule>> FindEnabledByContentScopeAsync(string name, string origin)
        {
            var sql = "SELECT * FROM replace_rules WHERE isEnabled = 1 AND scopeContent = 1 ORDER BY `order`";
            var rules = await QueryAsync<ReplaceRule>(sql);
            return rules.Where(r => IsInScope(r, name, origin)).ToList();
        }

        /// <summary>
        /// 查找启用的标题替换规则（按作用域）
        /// </summary>
        public async Task<List<ReplaceRule>> FindEnabledByTitleScopeAsync(string name, string origin)
        {
            var sql = "SELECT * FROM replace_rules WHERE isEnabled = 1 AND scopeTitle = 1 ORDER BY `order`";
            var rules = await QueryAsync<ReplaceRule>(sql);
            return rules.Where(r => IsInScope(r, name, origin)).ToList();
        }

        /// <summary>
        /// 按分组获取规则
        /// </summary>
        public async Task<List<ReplaceRule>> GetByGroupAsync(string group)
        {
            var sql = "SELECT * FROM replace_rules WHERE `group` LIKE ? ORDER BY `order`";
            var result = await QueryAsync<ReplaceRule>(sql, $"%{group}%");
            return result;
        }

        /// <summary>
        /// 获取所有已启用的规则
        /// </summary>
        public async Task<List<ReplaceRule>> GetAllEnabledAsync()
        {
            var sql = "SELECT * FROM replace_rules WHERE isEnabled = 1 ORDER BY `order`";
            var result = await QueryAsync<ReplaceRule>(sql);
            return result;
        }

        /// <summary>
        /// 获取已启用的规则（接口要求的方法）
        /// </summary>
        public async Task<List<ReplaceRule>> GetEnabledAsync()
        {
            return await GetAllEnabledAsync();
        }

        // ================= 分组相关 =================

        /// <summary>
        /// 获取所有分组（未处理）
        /// </summary>
        public async Task<List<string>> GetGroupsUnProcessedAsync()
        {
            // SQLite-net-pcl 不支持直接查询基本类型，需要从实体中获取
            var rules = await GetAllAsync();
            return rules
                .Select(r => r.Group)
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 获取所有分组（已处理）
        /// </summary>
        public async Task<List<string>> GetAllGroupsAsync()
        {
            var groupsUnProcessed = await GetGroupsUnProcessedAsync();
            return DealGroups(groupsUnProcessed);
        }

        // ================= 统计方法 =================

        /// <summary>
        /// 获取最小排序值
        /// </summary>
        public async Task<int> GetMinOrderAsync()
        {
            var sql = "SELECT MIN(`order`) FROM replace_rules";
            var result = await ExecuteScalarAsync<int?>(sql);
            return result ?? 0;
        }

        /// <summary>
        /// 获取最大排序值
        /// </summary>
        public async Task<int> GetMaxOrderAsync()
        {
            var sql = "SELECT MAX(`order`) FROM replace_rules";
            var result = await ExecuteScalarAsync<int?>(sql);
            return result ?? 0;
        }

        /// <summary>
        /// 获取禁用规则数量
        /// </summary>
        public async Task<int> GetSummaryAsync()
        {
            var sql = "SELECT COUNT(*) - SUM(CASE WHEN isEnabled = 1 THEN 1 ELSE 0 END) FROM replace_rules";
            var result = await ExecuteScalarAsync<int>(sql);
            return result;
        }

        // ================= 增删改操作 =================

        /// <summary>
        /// 插入规则
        /// </summary>
        public async Task<List<long>> InsertAsync(params ReplaceRule[] replaceRules)
        {
            var ids = new List<long>();
            if (replaceRules == null || replaceRules.Length == 0)
                return ids;

            foreach (var rule in replaceRules)
            {
                await base.InsertAsync(rule);
                ids.Add(rule.Id);
            }
            return ids;
        }

        /// <summary>
        /// 插入规则（无返回值版本，接口要求）
        /// </summary>
        async Task IReplaceRuleDao.InsertAsync(params ReplaceRule[] replaceRules)
        {
            await InsertAsync(replaceRules);
        }

        /// <summary>
        /// 更新规则
        /// </summary>
        public async Task UpdateAsync(params ReplaceRule[] replaceRules)
        {
            if (replaceRules == null || replaceRules.Length == 0)
                return;

            await base.UpdateAllAsync(replaceRules);
        }

        /// <summary>
        /// 删除规则
        /// </summary>
        public async Task DeleteAsync(params ReplaceRule[] replaceRules)
        {
            if (replaceRules == null || replaceRules.Length == 0)
                return;

            await base.DeleteAllAsync(replaceRules);
        }

        /// <summary>
        /// 全部启用/禁用
        /// </summary>
        public async Task EnableAllAsync(bool enable)
        {
            await ExecuteAsync(
                "UPDATE replace_rules SET isEnabled = ?",
                enable ? 1 : 0
            );
        }

        // ================= Observable 数据流 =================

        /// <summary>
        /// 观察所有规则
        /// </summary>
        public IObservable<List<ReplaceRule>> ObserveAll()
        {
            return Observable.Create<List<ReplaceRule>>(async observer =>
            {
                var rules = await GetAllAsync();
                observer.OnNext(rules);
                observer.OnCompleted();
            });
        }

        /// <summary>
        /// 观察搜索结果
        /// </summary>
        public IObservable<List<ReplaceRule>> ObserveSearch(string key)
        {
            return Observable.Create<List<ReplaceRule>>(async observer =>
            {
                var rules = await SearchAsync(key);
                observer.OnNext(rules);
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
        /// 检查是否在作用域内
        /// </summary>
        private bool IsInScope(ReplaceRule rule, string name, string origin)
        {
            // 检查包含作用域
            var inScope = string.IsNullOrEmpty(rule.Scope) ||
                         (rule.Scope.Contains(name) || rule.Scope.Contains(origin));

            if (!inScope)
                return false;

            // 检查排除作用域
            if (!string.IsNullOrEmpty(rule.ExcludeScope))
            {
                if (rule.ExcludeScope.Contains(name) || rule.ExcludeScope.Contains(origin))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 处理分组
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
