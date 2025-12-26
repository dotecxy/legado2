using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 搜索关键词数据访问实现（对应 Kotlin 的 SearchKeywordDao.kt）
    /// </summary>
    public class SearchKeywordDao : ProxyDao<SearchKeyword>, ISearchKeywordDao
    {
        public SearchKeywordDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<List<SearchKeyword>> GetAllAsync()
        {
            var sql = "SELECT * FROM search_keywords ORDER BY usage DESC";
            var result = await QueryAsync<SearchKeyword>(sql);
            return result;
        }

        public async Task<SearchKeyword> GetAsync(string word)
        {
            return await GetFirstOrDefaultAsync(k => k.Word == word);
        }

        public async Task InsertAsync(params SearchKeyword[] keywords)
        {
            await InsertOrReplaceAllAsync(keywords);
        }

        public async Task UpdateAsync(params SearchKeyword[] keywords)
        {
            await base.UpdateAllAsync(keywords);
        }

        public async Task DeleteAsync(params SearchKeyword[] keywords)
        {
            await base.DeleteAllAsync(keywords);
        }

        public async Task ClearAsync()
        {
            var sql = "DELETE FROM search_keywords";
            await ExecuteAsync(sql);
        }
    }
}
