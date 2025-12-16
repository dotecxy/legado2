using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 搜索关键词数据访问接口（对应 Kotlin 的 SearchKeywordDao.kt）
    /// </summary>
    public interface ISearchKeywordDao
    {
        Task<List<SearchKeyword>> GetAllAsync();
        Task<SearchKeyword> GetAsync(string word);
        Task InsertAsync(params SearchKeyword[] keywords);
        Task UpdateAsync(params SearchKeyword[] keywords);
        Task DeleteAsync(params SearchKeyword[] keywords);
        Task ClearAsync();
        // TODO: 实现更多查询方法
    }
}
