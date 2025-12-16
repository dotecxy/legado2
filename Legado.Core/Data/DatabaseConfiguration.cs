using System.Collections.Generic;

namespace Legado.Core
{
    [QConfigurationSection(QProperties.DatabaseConfigName)]
    public class DatabaseConfiguration : Dictionary<string, string>
    {
    }
}
