using Autofac.Builder;

namespace Legado.Core
{
    public static class AutofacRegistrationBuilderExtensions
    {
        public static void Initializable<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registrationBuilder)
        {
            registrationBuilder.OnActivated(e =>
            {
                if (e.Instance is IInitializable initObj)
                    initObj.Initialize();
            });
        }
    }
}
