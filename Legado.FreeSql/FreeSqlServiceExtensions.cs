using FreeSql;
using FreeSql.Aop;
using FreeSql.DataAnnotations;
using  Legado.DB;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 服务扩展类
/// </summary>
public static class FreeSqlServiceExtensions
{
    /// <summary>
    /// 增加 FreeSql 数据库操作服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="optionsAction"></param>
    /// <param name="configureAction"></param>
    /// <param name="configEntityPropertyImage"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddFreeSql(this IServiceCollection services, Action<FreeSqlBuilder, IServiceProvider> optionsAction, Action<IFreeSql, IServiceProvider>? configureAction = null, bool configEntityPropertyImage = true, FreeSqlServiceOptions? configureOptions = null)
    {
        configureOptions ??= new FreeSqlServiceOptions(configEntityPropertyImage);
        services.AddSingleton(sp =>
        {
            var builder = new FreeSqlBuilder();
            optionsAction?.Invoke(builder, sp);
            var instance = builder.Build();
            instance.UseJsonMap();
            if (configEntityPropertyImage || (configureOptions?.ConfigEntityPropertyImage ?? false))
            {
                instance.Aop.AuditValue += AuditValue;
                instance.Aop.ConfigEntityProperty += ConfigEntityProperty;
                instance.Aop.ConfigEntity += Aop_ConfigEntity;

            }
            configureAction?.Invoke(instance, sp);
            return instance;
        });

        services.AddSingleton(configureOptions);



        return services;
    }



    public static void ConfigSQLiteBuild(this FreeSqlBuilder build, string dbPath, Logging.ILogger logger = null)
    {
        if (build == null) return;

        build.UseConnectionString(DataType.Sqlite, $"Data Source={dbPath};")
            .UseAutoSyncStructure(true)
            .UseNameConvert(FreeSql.Internal.NameConvertType.PascalCaseToUnderscoreWithLower)
            .UseMonitorCommand((cmd) => LogSQL(cmd, logger))
            .UseNoneCommandParameter(true);
    }

    private static void LogSQL(DbCommand cmd, ILogger logger)
    {
        if (logger == null) return;
        StringBuilder sb = new StringBuilder();
        sb.Append("\n参数：").AppendLine();
        foreach (DbParameter p in cmd.Parameters)
        {
            sb.AppendLine($"{p.ParameterName}={p.Value}");
        }
        logger.LogDebug($"执行SQL: {cmd.CommandText}{sb.ToString()}");
    }

    public static void ConfigureSQLitePRAGMA(this IFreeSql freeSql, int busyTimeout = 5000, string synchronous = "NORMAL", int cacheSize = -2000)
    {
        // SQLite 特定配置
        freeSql.Ado.ExecuteNonQuery("PRAGMA journal_mode = WAL");
        freeSql.Ado.ExecuteNonQuery($"PRAGMA busy_timeout = {busyTimeout}");
        freeSql.Ado.ExecuteNonQuery($"PRAGMA synchronous = {synchronous}");
        freeSql.Ado.ExecuteNonQuery($"PRAGMA cache_size = {cacheSize}");
    }




    #region 自定义Guid支持
    /// <summary>
    /// 审计属性值 , 实现插入/更新时统一处理某些值，比如某属性的雪花算法值、创建时间值、甚至是业务值。
    /// </summary>
    /// <param name="e"></param>
    public static void AuditValue(object? sender, FreeSql.Aop.AuditValueEventArgs? e)
    {
        if (e!.Column.CsType == typeof(Guid) && e.Column.Attribute.MapType == typeof(string) && e.Value?.ToString() == Guid.Empty.ToString())
        {
            e.Value = FreeUtil.NewMongodbId();
        } 
    }
    #endregion 

    #region 自定义实体特性
    private static void Aop_ConfigEntity(object sender, ConfigEntityEventArgs e)
    {
        // 获取实体类上的自定义特性
        var attr = e.EntityType.GetCustomAttributes(typeof(global::Legado.DB.TableAttribute), false)
            .FirstOrDefault() as global::Legado.DB.TableAttribute;

        if (attr != null)
        {
            // 修改映射的表名
            e.ModifyResult.Name = attr.Name; 
        }

        var attr2 = e.EntityType.GetCustomAttributes(typeof(global::Legado.DB.IndexedAttribute), false);

        foreach (IndexedAttribute item in attr2)
        {
            e.ModifyIndexResult.Add(new IndexAttribute(item.Name, item.Field, item.Unique));
        } 
    }

    /// <summary>
    /// 自定义实体特性 : 自定义Enum支持和Image支持
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static void ConfigEntityProperty(object? sender, FreeSql.Aop.ConfigEntityPropertyEventArgs? e)
    {
        var attr = e.Property.GetCustomAttributes(typeof(global::Legado.DB.ColumnAttribute), false)
            .FirstOrDefault() as global::Legado.DB.ColumnAttribute;
         

        if (attr != null)
        {
            e.ModifyResult.Name = attr.Name; 
        }

        var attr2 = e.Property.GetCustomAttributes(typeof(global::Legado.DB.PrimaryKeyAttribute), false)
            .FirstOrDefault() as global::Legado.DB.PrimaryKeyAttribute;


        if (attr2 != null)
        {
            e.ModifyResult.IsPrimary = true;
        }


        var attr3 = e.Property.GetCustomAttributes(typeof(global::Legado.DB.IgnoreAttribute), false)
            .FirstOrDefault() as global::Legado.DB.IgnoreAttribute;


        if (attr3 != null)
        {
            e.ModifyResult.IsIgnore = true;
        }

        var attr4 = e.Property.GetCustomAttributes(typeof(global::Legado.DB.AutoIncrementAttribute), false)
            .FirstOrDefault() as global::Legado.DB.AutoIncrementAttribute;


        if (attr4 != null)
        {
            e.ModifyResult.IsIdentity = true;
        }
         

        //其他
        if (e!.Property.PropertyType.IsEnum)
        {
            e.ModifyResult.MapType = typeof(int);
        }
        else if (e.Property.PropertyType == typeof(byte[]))
        {
            var orm = sender as IFreeSql;
            switch (orm?.Ado.DataType)
            {
                case DataType.SqlServer:
                    e.ModifyResult.DbType = "image";
                    break;
                case DataType.MySql:
                    e.ModifyResult.DbType = "longblob";
                    break;
                case DataType.PostgreSQL:
                    e.ModifyResult.DbType = "bytea";
                    break;
            }
        }
    }
    #endregion 


}

