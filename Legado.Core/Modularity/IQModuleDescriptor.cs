using System;
using System.Collections.Generic;
using System.Reflection;

namespace Legado.Core
{
    public interface IQModuleDescriptor
    {
        Type Type { get; }

        IQModule Instance { get; }

        IReadOnlyList<IQModuleDescriptor> Dependencies { get; }
    }
}