using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// RSS 阅读记录数据访问实现（对应 Kotlin 的 RssReadRecordDao.kt）
    /// </summary>
    public class RssReadRecordDao : ProxyDao<RssReadRecord>, IRssReadRecordDao
    {
        public RssReadRecordDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<RssReadRecord> GetAsync(string record)
        {
            return await GetFirstOrDefaultAsync(r => r.Record == record);
        }

        public override async Task<List<RssReadRecord>> GetAllAsync()
        {
            return await base.GetAllAsync();
        }

        public async Task InsertAsync(params RssReadRecord[] records)
        {
            await InsertOrReplaceAllAsync(records);
        }

        public async Task DeleteAsync(params RssReadRecord[] records)
        {
            await base.DeleteAllAsync(records);
        }

        public async Task DeleteAsync(string record)
        {
            var sql = "DELETE FROM rssReadRecord WHERE record = ?";
            await ExecuteAsync(sql, record);
        }
    }
}
