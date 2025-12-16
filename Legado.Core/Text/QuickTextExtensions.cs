using Autofac;

namespace Legado.Core
{
    internal static class QuickTextExtensions
    {
        internal static ContainerBuilder AddText(this ContainerBuilder serviceBuilder)
        {
            serviceBuilder.Register(p => new ChineseToPinyin()).As<IChineseToPinyin>().SingleInstance().Initializable();
            return serviceBuilder;
        }
    }
}
