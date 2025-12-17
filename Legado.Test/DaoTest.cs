using Legado.Core;
using Legado.Core.Data.Dao;
using Legado.Core.Data.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Test
{
    internal class DaoTest
    {
        IServiceProvider sp;

        [OneTimeSetUp]
        public async Task Setup()
        {
            await Task.Factory.StartNew(() =>
            {
                _ = new TestApplication();
                sp = QApplication.QServiceProvider;
            });
        }

        [Test]
        public async Task TestSave()
        {
            var log = sp.GetService<Serilog.ILogger>();
            log.Information("testSave");


            BookDao bookDao = sp.GetService<BookDao>();
            //await bookDao.CreateTableAsnyc();

            bookDao.Insert(new Book()
            {
                Name = "aaaa",
            });
        }
    }
}
