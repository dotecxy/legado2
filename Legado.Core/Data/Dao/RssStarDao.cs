using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// RSS 收藏数据访问实现（对应 Kotlin 的 RssStarDao.kt）
    /// </summary>
    public class RssStarDao : DapperDao<RssStar>, IRssStarDao
    {
        public RssStarDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<List<RssStar>> GetAllAsync()
        {
            var sql = "SELECT * FROM rssStars ORDER BY starTime DESC";
            var result = await QueryAsync<RssStar>(sql);
            return result;
        }

        public async Task<RssStar> GetAsync(string origin, string link)
        {
            return await GetFirstOrDefaultAsync(s => s.Origin == origin && s.Link == link);
        }

        public async Task InsertAsync(params RssStar[] stars)
        {
            await InsertOrReplaceAllAsync(stars);
        }

        public async Task UpdateAsync(params RssStar[] stars)
        {
            await base.UpdateAllAsync(stars);
        }

        public async Task DeleteAsync(params RssStar[] stars)
        {
            await base.DeleteAllAsync(stars);
        }
    }
}
