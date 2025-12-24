using Legado.Core;
using Legado.Core.Data.Entities;
using Legado.Core.Models.WebBooks;
using MudBlazor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Shared
{
    [SingletonDependency]
    public class LegadoContext
    {
        private readonly IEventProvider _ep;
        public List<BookSource> BookSources { get; private set; } = new List<BookSource>();
        public WebBook WebBook { get; private set; } = new WebBook();
        public int BookIndex { get; set; } = 6;
        public Book CurrentBook { get; set; }
        public BookChapter CurrentChapter { get; set; }
        public BookSource CurrentBookSource
        {
            get
            {
                return BookSources[BookIndex];
            }
        }

        public LegadoContext(IEventProvider eventProvider)
        {
            _ep = eventProvider;
        }

        public void Load()
        {
            try
            {

                var json = _ep.GetResourceManager().GetString("bs");
                var list = JsonConvert.DeserializeObject<List<BookSource>>(json);
                if (list?.Count > 0)
                {
                    BookSources.AddRange(list);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
