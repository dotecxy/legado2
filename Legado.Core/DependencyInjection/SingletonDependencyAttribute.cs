using System;

namespace Legado.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class SingletonDependencyAttribute : Attribute
    {
        public bool PropertiesAutowired { get; set; }
    }
}
