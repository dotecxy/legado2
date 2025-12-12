using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 搜索关键词数据访问实现（对应 Kotlin 的 SearchKeywordDao.kt）
    /// </summary>
    public class SearchKeywordDao : ISearchKeywordDao
    {
        private readonly SQLiteAsyncConnection _database;

        public SearchKeywordDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<List<SearchKeyword>> GetAllAsync()
        {
            return await _database.Table<SearchKeyword>()
                .OrderByDescending(k => k.Usage)
                .ToListAsync();
        }

        public async Task<SearchKeyword> GetAsync(string word)
        {
            return await _database.Table<SearchKeyword>()
                .Where(k => k.Word == word)
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(params SearchKeyword[] keywords)
        {
            foreach (var keyword in keywords)
                await _database.InsertOrReplaceAsync(keyword);
        }

        public async Task UpdateAsync(params SearchKeyword[] keywords)
        {
            foreach (var keyword in keywords)
                await _database.UpdateAsync(keyword);
        }

        public async Task DeleteAsync(params SearchKeyword[] keywords)
        {
            foreach (var keyword in keywords)
                await _database.DeleteAsync(keyword);
        }

        public async Task ClearAsync()
        {
            await _database.ExecuteAsync("DELETE FROM search_keywords");
        }
    }
}
