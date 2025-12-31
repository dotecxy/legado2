using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 阅读记录数据访问实现（对应 Kotlin 的 ReadRecordDao.kt）
    /// </summary>
    public class ReadRecordDao : BaseDao<ReadRecord>, IReadRecordDao
    {
        public ReadRecordDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<List<ReadRecord>> GetAllAsync()
        {
            var sql = "SELECT * FROM readRecord ORDER BY readTime DESC";
            var result = await QueryAsync<ReadRecord>(sql);
            return result;
        }

        public async Task<ReadRecord> GetAsync(string bookName)
        {
            return await GetFirstOrDefaultAsync(r => r.BookName == bookName);
        }

        public async Task InsertAsync(params ReadRecord[] records)
        {
            await InsertOrReplaceAllAsync(records);
        }

        public async Task UpdateAsync(params ReadRecord[] records)
        {
            await base.UpdateAllAsync(records);
        }

        public async Task DeleteAsync(params ReadRecord[] records)
        {
            await base.DeleteAllAsync(records);
        }

        public async Task ClearAsync()
        {
            await ExecuteAsync("DELETE FROM readRecord");
        }
    }
}
