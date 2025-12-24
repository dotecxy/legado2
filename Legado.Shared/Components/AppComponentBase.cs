using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Legado.Shared.Components
{
    public class AppComponentBase : ComponentBase, IDisposable
    {
        [Inject]
        public NavigationManager Nav { get; set; } = null!;

        [Inject]
        public IDialogService DialogService { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public IServiceScopeFactory? ScopeFactory { get; set; }

        [Inject]
        public AppStates States { get; set; } = null!;

        [Inject]
        public IEventProvider EventProvider { get; set; } = null!;

        [Inject]
        public LegadoContext LegadoContext { get; set; }

        public virtual void Dispose()
        {
            // ScopeFactory不需要在这里Dispose
        }
    }
}