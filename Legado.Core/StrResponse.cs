using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core
{
    public class StrResponse
    {
        public string Url { get; set; }
        public string Body { get; set; }
        public bool IsError { get; set; }

        public StrResponse(string url, string body)
        {
            Url = url;
            Body = body;
        }
    }
}
