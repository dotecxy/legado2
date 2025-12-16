using System;

namespace Legado.Core
{
    public class ApplicationInitializationContext
    {
        public IServiceProvider ServiceProvider { get; set; }

        public ApplicationInitializationContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}