using System;

namespace Legado.Core.Utils
{
    public class ExecutingIndicator
    {
        public bool IsExecuting { get; private set; }
        public void Execute(Action action)
        {
            try
            {
                IsExecuting = true;
                action?.Invoke();
            }
            finally
            {
                IsExecuting = false;
            }
        }

    }
}
