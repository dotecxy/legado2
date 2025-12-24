using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Core.Helps
{
    public interface IWinDialog
    {
        Task<bool> ShowBrowserDialogAsync(IntentData data);
        Task<bool> ShowDialogAsync(IntentData data);
    }
}
