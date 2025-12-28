using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    public static class ExceptionExtension
    {
        public static void ThrowIfNull(this object obj, [CallerMemberName] string callerName = "obj")
        {
            if (obj == null)
            {
                throw new ArgumentNullException(callerName ?? "obj");
            }
        }
    }
}
