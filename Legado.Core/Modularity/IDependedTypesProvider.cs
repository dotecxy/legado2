using System;

namespace Legado.Core
{
    public interface IDependedTypesProvider
    {
        Type[] GetDependedTypes();
    }
}