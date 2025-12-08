using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Legado.Core.Models.AnalyzeRules
{
    public sealed class RuleExcepion : Exception
    {
        public RuleExcepion()
        {
        }

        public RuleExcepion(string message) : base(message)
        {
        }

        public RuleExcepion(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RuleExcepion(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
