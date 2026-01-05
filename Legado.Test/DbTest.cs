using Autofac.Core;
using FreeSql.DataAnnotations;
using FreeSql.Internal.Model;
using Legado.Core;
using Legado.Core.DependencyInjection;
using  Legado.DB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace Legado.Test
{
    internal class DbTest
    {
        private IServiceProvider _serviceProvider;

        [OneTimeSetUp]
        public async Task InitAsync()
        {
            await Task.Delay(100);
            var builder = Host.CreateDefaultBuilder();

            builder.UseServiceProviderFactory(new QAutofacFactory((containerBuilder, options) =>
            {
                options.Configuration.BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data2");
            }));
#if DEBUG
            builder.UseEnvironment(Environments.Development);
#endif

            builder.ConfigureServices(s =>
            {

                //#if WINDOWS
                //        string dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "hybrid.db");
                //#elif ANDROID || IOS || MACCATALYST
                //        string dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "hybrid.db");
                //#else
                //                string dbpath = "hybrid.db";
                //#endif
                s.AddFreeSql((option, p) =>
                {
                    var confuration = p.GetRequiredService<IQConfiguration>();
                    var applitionOption = p.GetRequiredService<QConfigurationBuilderOptions>();
                    var dbpath = Path.Combine(applitionOption.BasePath, "hybrid.db");
                    var logger = p.GetRequiredService<ILogger<IFreeSql>>();

                    option.ConfigSQLiteBuild(dbpath, logger);

                }
                , configureAction: (fsql, p) =>
                {
                    fsql.ConfigureSQLitePRAGMA();
                }
                , configEntityPropertyImage: true);
            });

            var host = builder.Build();
            _serviceProvider = host.Services;
            _ = host.RunAsync();
        }

        [Test]
        public async Task TestFreesql()
        {
            var fsql = _serviceProvider.GetRequiredService<IFreeSql>();
            var env = _serviceProvider.GetRequiredService<IHostEnvironment>();
            bool isDev = env.IsDevelopment();

            var str=@"\[Table\[[""']([^""']+)[""']\]\]";
            await fsql.InsertOrUpdate<Student>().SetSource(new Student() { Id = "9527" }).ExecuteAffrowsAsync();

            DynamicFilterInfo filter = new DynamicFilterInfo();
            filter.Field = "Id";
            filter.Value = "9";
            filter.Operator = DynamicFilterOperator.Contains;

            var list = await fsql.Select<Student>().Where(a => a.Id.Contains("9")).ToListAsync();
        }
    }

    [DB.Table( "student")]
    public class Student
    {
        [PrimaryKey]
        public string Id { get; set; }
    }
}
