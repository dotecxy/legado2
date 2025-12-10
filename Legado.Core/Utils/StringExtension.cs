using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Utils
{
    internal static class StringExtension
    {
        public static string FirstCharToLower(this string input)
        {
            if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
                return input;

            Span<char> result = stackalloc char[input.Length];
            input.AsSpan().CopyTo(result);
            result[0] = char.ToLowerInvariant(result[0]);
            return new string(result.ToArray());
        }
    }
}
