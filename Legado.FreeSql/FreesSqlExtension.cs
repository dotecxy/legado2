using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Legado.DB
{
    public static class FreesSqlExtension
    {
        public static async Task<int> ExecuteAsync(this IFreeSql freeSql, string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return 0;
            }
            return await freeSql.Ado.ExecuteNonQueryAsync(sql);
        }

        public static async Task<T> ExecuteScalarAsync<T>(this IFreeSql freeSql, string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return default(T);
            }
            return (T)await freeSql.Ado.ExecuteScalarAsync(sql);
        }


        public static int Execute(this IFreeSql freeSql, string sql)
        {
            return freeSql.Ado.ExecuteNonQuery(sql);
        }


        public static int Execute(this IFreeSql freeSql, string sql, object par)
        {
            return freeSql.Ado.ExecuteNonQuery(sql, par);
        }
    }



}
